using UnityEngine;
using System.Collections.Generic;

public class MultiLegRigger : MonoBehaviour
{
    [System.Serializable]
    public class LegEntry
    {
        public string characterName;
        public Transform leftLegMesh;
        public Transform rightLegMesh;

        [HideInInspector] public bool leftDone, rightDone;
        [HideInInspector] public List<Transform> leftBones = new List<Transform>();
        [HideInInspector] public List<Transform> rightBones = new List<Transform>();
    }

    [Header("Characters to Rig")]
    public List<LegEntry> characters = new List<LegEntry>();

    [Header("Bone Settings")]
    public float thighLength = 0.4f;
    public float shinLength = 0.4f;
    public float influenceRadius = 0.3f;

    [Header("Placement Offsets")]
    public Vector3 leftHipOffset = new Vector3(-0.15f, 0.3f, 0f);
    public Vector3 rightHipOffset = new Vector3(0.15f, 0.3f, 0f);

    [Header("Debug")]
    public bool showBonesInScene = true;
    public float boneGizmoSize = 0.05f;

    void OnDrawGizmosSelected()
    {
        if (!showBonesInScene) return;

        foreach (var character in characters)
        {
            DrawLegGizmos(character.leftLegMesh, character.leftDone, Color.red);
            DrawLegGizmos(character.rightLegMesh, character.rightDone, Color.blue);
        }
    }

    void DrawLegGizmos(Transform legMesh, bool isDone, Color color)
    {
        if (!isDone || legMesh == null) return;

        SkinnedMeshRenderer smr = legMesh.GetComponent<SkinnedMeshRenderer>();
        if (smr == null || smr.bones == null) return;

        Gizmos.color = color;

        foreach (var bone in smr.bones)
        {
            if (bone == null) continue;

            Gizmos.DrawSphere(bone.position, boneGizmoSize);

            if (bone.parent != null && bone.parent != legMesh)
            {
                Gizmos.DrawLine(bone.parent.position, bone.position);
            }
        }
    }

    public void RigAll()
    {
        int riggedCount = 0;

        foreach (var character in characters)
        {
            if (character.leftLegMesh != null && !character.leftDone)
            {
                Transform[] bones = RigLeg(character.leftLegMesh, true, character.characterName);
                character.leftDone = true;
                character.leftBones.AddRange(bones);
                riggedCount++;
            }

            if (character.rightLegMesh != null && !character.rightDone)
            {
                Transform[] bones = RigLeg(character.rightLegMesh, false, character.characterName);
                character.rightDone = true;
                character.rightBones.AddRange(bones);
                riggedCount++;
            }
        }

        Debug.Log($"Rigged {riggedCount} legs total!");

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    Transform[] RigLeg(Transform legMesh, bool isLeft, string charName)
    {
        Debug.Log($"Starting rig for {charName} {(isLeft ? "Left" : "Right")} leg on {legMesh.name}");

        MeshFilter mf = legMesh.GetComponent<MeshFilter>();
        if (mf == null)
        {
            Debug.LogError($"No mesh filter on {legMesh.name}");
            return null;
        }

        MeshRenderer mr = legMesh.GetComponent<MeshRenderer>();
        Material savedMaterial = mr != null ? mr.sharedMaterial : null;

        Bounds bounds = mf.sharedMesh.bounds;
        Vector3 meshCenter = mf.transform.TransformPoint(bounds.center);
        Vector3 meshTop = mf.transform.TransformPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));

        Vector3 hipPos = meshTop + (isLeft ? Vector3.left : Vector3.right) * 0.05f;
        Vector3 kneePos = hipPos + Vector3.down * thighLength;
        Vector3 anklePos = kneePos + Vector3.down * shinLength;

        Debug.Log($"Hip: {hipPos}, Knee: {kneePos}, Ankle: {anklePos}");

        string prefix = $"{charName}_{(isLeft ? "L" : "R")}";

        Transform hip = CreateBone($"{prefix}_Hip", hipPos, legMesh.parent);
        Transform knee = CreateBone($"{prefix}_Knee", kneePos, hip);
        Transform ankle = CreateBone($"{prefix}_Ankle", anklePos, knee);

        Transform[] bones = new Transform[] { hip, knee, ankle };

        GenerateSkinnedMesh(mf, mr, bones, savedMaterial);

        Debug.Log($"Finished rigging {prefix} - check Hierarchy for {prefix}_Hip");
        return bones;
    }

    Transform CreateBone(string name, Vector3 pos, Transform parent)
    {
        GameObject bone = new GameObject(name);
        bone.transform.position = pos;
        bone.transform.SetParent(parent);

#if UNITY_EDITOR
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = name + "_Visual";
        marker.transform.SetParent(bone.transform);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = Vector3.one * 0.02f;
        DestroyImmediate(marker.GetComponent<Collider>());

        var mr = marker.GetComponent<MeshRenderer>();
        if (mr != null) mr.material.color = Color.yellow;
#endif

        Debug.Log($"Created bone: {name} at {pos}");
        return bone.transform;
    }

    void GenerateSkinnedMesh(MeshFilter mf, MeshRenderer mr, Transform[] bones, Material savedMaterial)
    {
        Mesh original = mf.sharedMesh;
        Mesh skinned = Instantiate(original);
        skinned.name = original.name + "_Skinned";

        Vector3[] verts = original.vertices;
        BoneWeight[] weights = new BoneWeight[verts.Length];

        Transform legTransform = mf.transform;

        for (int v = 0; v < verts.Length; v++)
        {
            Vector3 worldPos = legTransform.TransformPoint(verts[v]);

            float[] dists = new float[bones.Length];
            for (int b = 0; b < bones.Length; b++)
            {
                dists[b] = Vector3.Distance(worldPos, bones[b].position);
            }

            int closest = 0, second = 1;
            if (dists[1] < dists[0]) { closest = 1; second = 0; }

            for (int i = 2; i < dists.Length; i++)
            {
                if (dists[i] < dists[closest]) { second = closest; closest = i; }
                else if (dists[i] < dists[second]) { second = i; }
            }

            float w1 = 1 - Mathf.Clamp01(dists[closest] / influenceRadius);
            float w2 = 1 - Mathf.Clamp01(dists[second] / influenceRadius);
            float sum = w1 + w2;

            if (sum > 0.001f)
            {
                weights[v] = new BoneWeight
                {
                    boneIndex0 = closest,
                    weight0 = w1 / sum,
                    boneIndex1 = second,
                    weight1 = w2 / sum
                };
            }
            else
            {
                weights[v] = new BoneWeight { boneIndex0 = closest, weight0 = 1 };
            }
        }

        skinned.boneWeights = weights;

        Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            bindPoses[i] = bones[i].worldToLocalMatrix * legTransform.localToWorldMatrix;
        }
        skinned.bindposes = bindPoses;

        DestroyImmediate(mf);
        if (mr != null) DestroyImmediate(mr);

        SkinnedMeshRenderer smr = legTransform.gameObject.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = skinned;
        smr.bones = bones;
        smr.rootBone = bones[0];

        if (savedMaterial != null)
        {
            smr.sharedMaterial = savedMaterial;
        }

        Debug.Log($"SkinnedMeshRenderer created on {legTransform.name} with {bones.Length} bones");
    }

    // NEW: Clear all bones and restore original mesh
    public void ClearAllBones()
    {
        int clearedCount = 0;

        foreach (var character in characters)
        {
            // Clear left leg bones
            if (character.leftBones.Count > 0)
            {
                foreach (var bone in character.leftBones)
                {
                    if (bone != null)
                    {
                        // Also destroy visual markers (children)
                        foreach (Transform child in bone.GetComponentsInChildren<Transform>(true))
                        {
                            if (child != bone && child != null)
                            {
                                DestroyImmediate(child.gameObject);
                            }
                        }
                        DestroyImmediate(bone.gameObject);
                    }
                }
                character.leftBones.Clear();
                character.leftDone = false;

                // Restore original mesh
                RestoreOriginalMesh(character.leftLegMesh);
                clearedCount++;
            }

            // Clear right leg bones
            if (character.rightBones.Count > 0)
            {
                foreach (var bone in character.rightBones)
                {
                    if (bone != null)
                    {
                        foreach (Transform child in bone.GetComponentsInChildren<Transform>(true))
                        {
                            if (child != bone && child != null)
                            {
                                DestroyImmediate(child.gameObject);
                            }
                        }
                        DestroyImmediate(bone.gameObject);
                    }
                }
                character.rightBones.Clear();
                character.rightDone = false;

                RestoreOriginalMesh(character.rightLegMesh);
                clearedCount++;
            }
        }

        Debug.Log($"Cleared bones from {clearedCount} legs!");

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    void RestoreOriginalMesh(Transform legMesh)
    {
        if (legMesh == null) return;

        // Remove SkinnedMeshRenderer
        SkinnedMeshRenderer smr = legMesh.GetComponent<SkinnedMeshRenderer>();
        if (smr != null)
        {
            Mesh skinnedMesh = smr.sharedMesh;

            // Add back original components
            MeshFilter mf = legMesh.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = legMesh.gameObject.AddComponent<MeshRenderer>();

            // Try to find original mesh (remove "_Skinned" suffix)
            string originalName = skinnedMesh.name.Replace("_Skinned", "");
            Mesh originalMesh = FindOriginalMesh(originalName);

            if (originalMesh != null)
            {
                mf.sharedMesh = originalMesh;
                mr.sharedMaterial = smr.sharedMaterial;
                Debug.Log($"Restored original mesh: {originalName} on {legMesh.name}");
            }
            else
            {
                // Fallback: use the mesh without bone weights
                Mesh cleanMesh = new Mesh();
                cleanMesh.vertices = skinnedMesh.vertices;
                cleanMesh.triangles = skinnedMesh.triangles;
                cleanMesh.normals = skinnedMesh.normals;
                cleanMesh.uv = skinnedMesh.uv;

                mf.sharedMesh = cleanMesh;
                mr.sharedMaterial = smr.sharedMaterial;
                Debug.Log($"Created clean mesh for {legMesh.name}");
            }

            DestroyImmediate(smr);
        }
    }

    Mesh FindOriginalMesh(string meshName)
    {
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Mesh " + meshName);
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null && mesh.name == meshName)
            {
                return mesh;
            }
        }
#endif
        return null;
    }

    public void ClearAll()
    {
        ClearAllBones();
    }

    public void AutoFindLegs()
    {
        characters.Clear();

        MeshFilter[] allMeshes = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        Dictionary<string, LegEntry> charMap = new Dictionary<string, LegEntry>();

        foreach (var mf in allMeshes)
        {
            string name = mf.name.ToLower();
            string rootName = mf.transform.root.name;

            if (!name.Contains("leg") && !name.Contains("limb")) continue;

            if (!charMap.ContainsKey(rootName))
            {
                charMap[rootName] = new LegEntry { characterName = rootName };
            }

            if (name.Contains("left") || name.Contains("l_") || name.Contains("_l"))
                charMap[rootName].leftLegMesh = mf.transform;
            else if (name.Contains("right") || name.Contains("r_") || name.Contains("_r"))
                charMap[rootName].rightLegMesh = mf.transform;
        }

        characters.AddRange(charMap.Values);
        Debug.Log($"Found {characters.Count} characters with legs");
    }
}