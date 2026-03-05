using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MalePedestrianSystem : MonoBehaviour
{
    // ========================================================================
    // MALE SETTINGS
    // ========================================================================
    [System.Serializable]
    public class MaleGenderSettings
    {
        [Header("General Swing")]
        public float swingSpeed = 10f;
        public float legSwingAngle = 35f;
        public float armSwingAngle = 25f;

        [Header("Pivot Positions")]
        public float hipHeight = 0.9f;
        public float hipWidth = 0.15f;
        public float shoulderHeight = 1.5f;
        public float shoulderWidth = 0.2f;

        [Header("Mesh Deformation Settings (Male Only)")]
        public Vector3 leftLegBox = new Vector3(0.2f, 0.95f, 0.25f);
        public Vector3 rightLegBox = new Vector3(0.2f, 0.95f, 0.25f);
        public Vector3 leftFootBox = new Vector3(0.22f, 0.2f, 0.45f);
        public Vector3 rightFootBox = new Vector3(0.22f, 0.2f, 0.45f);
        public float footYOffset = -0.85f;
        public Vector3 leftArmBox = new Vector3(0.15f, 0.6f, 0.15f);
        public Vector3 rightArmBox = new Vector3(0.15f, 0.6f, 0.15f);
        public Vector3 leftHandBox = new Vector3(0.12f, 0.2f, 0.12f);
        public Vector3 rightHandBox = new Vector3(0.12f, 0.2f, 0.12f);
        public float handYOffset = -0.65f;
        public float armWeightThreshold = 0.3f;
        public float handWeightThreshold = 0.3f;
        public float limbExclusionThreshold = 0.5f;
        public float influenceFalloff = 0.15f;
        public float falloffHeight = 0.5f;
        public float separationStrength = 0.9f;
    }

    public class MeshData
    {
        public Transform meshTransform;
        public Mesh outputMesh;
        public Renderer outputRenderer;
        public bool isSkinned;
        public SkinnedMeshRenderer sourceSmr;
        public Mesh bakeMesh;
        public Vector3[] originalVerts;
        public Vector3[] localToLeftLeg;
        public Vector3[] localToRightLeg;
        public Vector3[] localToLeftArm;
        public Vector3[] localToRightArm;
        public float[] leftLegWeightRaw;
        public float[] rightLegWeightRaw;
        public float[] leftArmLimbWeight;
        public float[] rightArmLimbWeight;
        public float[] leftHandWeight;
        public float[] rightHandWeight;
    }

    public class MaleNPCData
    {
        public Transform root;
        public Vector3 lastPos;
        public float swingPhase;
        public MaleGenderSettings profile;
        public Transform leftLegPivot, rightLegPivot;
        public Transform leftArmPivot, rightArmPivot;
        public List<MeshData> activeMeshes = new List<MeshData>();
    }

    [System.Serializable]
    public class MaleSettingsContainer
    {
        public MaleGenderSettings maleProfile;
    }

    // ========================================================================
    // INSPECTOR FIELDS
    // ========================================================================
    [Header("Targeting")]
    [Tooltip("Add all NPC parent objects here")]
    public List<Transform> pathObjects = new List<Transform>();

    [Header("Gender Profiles")]
    public MaleGenderSettings maleProfile;

    [Header("Debug Output")]
    [Tooltip("Toggle verbose logging")]
    public bool verboseLogging = true;
    [Tooltip("Show gizmos in Scene view")]
    public bool showMaleGizmos = true;

    [ColorUsage(true, true)]
    public Color maleGizmoColor = Color.green;

    // ========================================================================
    // ACTIVE NPC LISTS
    // ========================================================================
    public List<MaleNPCData> activeMaleNPCs = new List<MaleNPCData>();

    // ========================================================================
    // LOGGING HELPER
    // ========================================================================
    void LogDebug(string message)
    {
        if (verboseLogging) Debug.Log($"[MaleSystem] {message}");
    }

    void LogWarning(string message)
    {
        Debug.LogWarning($"[MaleSystem] ⚠️ {message}");
    }

    void LogError(string message)
    {
        Debug.LogError($"[MaleSystem] ❌ {message}");
    }

    // ========================================================================
    // IMPORT/EXPORT
    // ========================================================================
    public void ExportSettings(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        var container = new MaleSettingsContainer
        {
            maleProfile = JsonUtility.FromJson<MaleGenderSettings>(JsonUtility.ToJson(maleProfile))
        };

        File.WriteAllText(path, JsonUtility.ToJson(container, true));
#if UNITY_EDITOR
        AssetDatabase.Refresh();
        LogDebug($"Settings exported to: {path}");
#endif
    }

    public void ImportSettings(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        string json = File.ReadAllText(path);
        var container = JsonUtility.FromJson<MaleSettingsContainer>(json);

        if (container == null)
        {
            LogError($"Failed to import settings from {path}");
            return;
        }

        if (container.maleProfile != null)
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(container.maleProfile), maleProfile);

        LogDebug($"Settings imported from: {path}");
    }

    // ========================================================================
    // INITIALIZATION
    // ========================================================================
    void Start()
    {
        LogDebug($"=== MALE SYSTEM START ===");
        LogDebug($"pathObjects count: {pathObjects?.Count ?? 0}");

        if (pathObjects == null || pathObjects.Count == 0)
        {
            LogError("NO pathObjects assigned! Drag NPC parents into 'pathObjects' list.");
            return;
        }

        var processedMale = new HashSet<Transform>();

        foreach (Transform pathParent in pathObjects)
        {
            if (pathParent == null) continue;
            LogDebug($"Processing path: {pathParent.name}");

            foreach (Renderer r in pathParent.GetComponentsInChildren<Renderer>(true))
            {
                Transform root = FindNPCRoot(r.transform, pathParent);
                if (root == null) continue;

                string name = root.name.ToLower();

                if (name.Contains("male"))
                {
                    if (processedMale.Add(root))
                        SetupMaleNPC(root);
                }
            }
        }

        LogDebug($"=== INITIALIZED: {activeMaleNPCs.Count} male NPCs ===");
    }

    Transform FindNPCRoot(Transform current, Transform limit)
    {
        while (current != null && current != limit)
        {
            string n = current.name.ToLower();
            if (n.Contains("male")) return current; // Only looking for males now
            current = current.parent;
        }
        return null;
    }

    // ========================================================================
    // MALE SETUP
    // ========================================================================
    void SetupMaleNPC(Transform npc)
    {
        MaleGenderSettings p = maleProfile;

        var data = new MaleNPCData
        {
            root = npc,
            lastPos = npc.position,
            profile = p
        };

        data.leftLegPivot = GetOrCreatePivot("L_LegPivot", npc, new Vector3(-p.hipWidth, p.hipHeight, 0));
        data.rightLegPivot = GetOrCreatePivot("R_LegPivot", npc, new Vector3(p.hipWidth, p.hipHeight, 0));
        data.leftArmPivot = GetOrCreatePivot("L_ArmPivot", npc, new Vector3(-p.shoulderWidth, p.shoulderHeight, 0));
        data.rightArmPivot = GetOrCreatePivot("R_ArmPivot", npc, new Vector3(p.shoulderWidth, p.shoulderHeight, 0));

        foreach (Renderer rend in npc.GetComponentsInChildren<Renderer>())
        {
            SkinnedMeshRenderer smr = rend as SkinnedMeshRenderer;
            MeshFilter mf = rend.GetComponent<MeshFilter>();

            Mesh sourceMesh = smr != null ? smr.sharedMesh : mf != null ? mf.sharedMesh : null;
            if (sourceMesh == null || !sourceMesh.isReadable) continue;

            MeshData mData = new MeshData();

            if (smr != null)
            {
                mData.isSkinned = true;
                mData.sourceSmr = smr;
                mData.bakeMesh = new Mesh();
                mData.bakeMesh.MarkDynamic();
                smr.BakeMesh(mData.bakeMesh);

                GameObject companion = new GameObject(smr.name + "_Deform");
                companion.transform.SetParent(smr.transform);
                companion.transform.localPosition = Vector3.zero;
                companion.transform.localRotation = Quaternion.identity;
                companion.transform.localScale = Vector3.one;

                MeshFilter companionMF = companion.AddComponent<MeshFilter>();
                MeshRenderer companionMR = companion.AddComponent<MeshRenderer>();
                companionMR.sharedMaterials = smr.sharedMaterials;

                mData.outputMesh = new Mesh();
                mData.outputMesh.MarkDynamic();
                companionMF.mesh = mData.outputMesh;
                mData.meshTransform = companion.transform;
                mData.outputRenderer = companionMR;
                smr.enabled = false;
            }
            else
            {
                mData.isSkinned = false;
                mData.outputMesh = Instantiate(sourceMesh);
                mData.outputMesh.MarkDynamic();
                mData.originalVerts = mData.outputMesh.vertices;
                mf.mesh = mData.outputMesh;
                mData.meshTransform = rend.transform;
                mData.outputRenderer = rend;

                mData.localToLeftLeg = new Vector3[mData.originalVerts.Length];
                mData.localToRightLeg = new Vector3[mData.originalVerts.Length];
                mData.localToLeftArm = new Vector3[mData.originalVerts.Length];
                mData.localToRightArm = new Vector3[mData.originalVerts.Length];
            }

            Vector3[] setupVerts = mData.isSkinned ? mData.bakeMesh.vertices : mData.originalVerts;
            Transform weightTransform = mData.isSkinned ? smr.transform : rend.transform;
            int vCount = setupVerts.Length;

            mData.leftLegWeightRaw = new float[vCount];
            mData.rightLegWeightRaw = new float[vCount];
            mData.leftArmLimbWeight = new float[vCount];
            mData.rightArmLimbWeight = new float[vCount];
            mData.leftHandWeight = new float[vCount];
            mData.rightHandWeight = new float[vCount];

            for (int v = 0; v < vCount; v++)
            {
                Vector3 worldV = weightTransform.TransformPoint(setupVerts[v]);
                Vector3 npcLocal = npc.InverseTransformPoint(worldV);
                float birthSide = Mathf.Sign(npcLocal.x);

                float wLL = GetCombinedWeight(worldV, data.leftLegPivot, p.leftLegBox, p.leftFootBox, p.footYOffset, p.influenceFalloff, p.falloffHeight);
                float wRL = GetCombinedWeight(worldV, data.rightLegPivot, p.rightLegBox, p.rightFootBox, p.footYOffset, p.influenceFalloff, p.falloffHeight);

                Vector3 localL = data.leftArmPivot.InverseTransformPoint(worldV);
                Vector3 localR = data.rightArmPivot.InverseTransformPoint(worldV);
                float armLimbL = CalculateBoxWeight(localL, Vector3.zero, p.leftArmBox, p.influenceFalloff, p.falloffHeight, true);
                float armLimbR = CalculateBoxWeight(localR, Vector3.zero, p.rightArmBox, p.influenceFalloff, p.falloffHeight, true);
                float handWL = CalculateBoxWeight(localL, new Vector3(0, p.handYOffset, 0), p.leftHandBox, p.influenceFalloff, p.falloffHeight, false);
                float handWR = CalculateBoxWeight(localR, new Vector3(0, p.handYOffset, 0), p.rightHandBox, p.influenceFalloff, p.falloffHeight, false);

                if (birthSide > 0) { wLL *= (1f - p.separationStrength); armLimbL *= (1f - p.separationStrength); handWL *= (1f - p.separationStrength); }
                else { wRL *= (1f - p.separationStrength); armLimbR *= (1f - p.separationStrength); handWR *= (1f - p.separationStrength); }

                mData.leftLegWeightRaw[v] = Mathf.Clamp01(wLL);
                mData.rightLegWeightRaw[v] = Mathf.Clamp01(wRL);
                mData.leftArmLimbWeight[v] = Mathf.Clamp01(armLimbL);
                mData.rightArmLimbWeight[v] = Mathf.Clamp01(armLimbR);
                mData.leftHandWeight[v] = Mathf.Clamp01(handWL);
                mData.rightHandWeight[v] = Mathf.Clamp01(handWR);

                if (!mData.isSkinned)
                {
                    mData.localToLeftLeg[v] = data.leftLegPivot.InverseTransformPoint(worldV);
                    mData.localToRightLeg[v] = data.rightLegPivot.InverseTransformPoint(worldV);
                    mData.localToLeftArm[v] = data.leftArmPivot.InverseTransformPoint(worldV);
                    mData.localToRightArm[v] = data.rightArmPivot.InverseTransformPoint(worldV);
                }
            }

            if (mData.isSkinned)
            {
                mData.outputMesh.vertices = mData.bakeMesh.vertices;
                mData.outputMesh.triangles = mData.bakeMesh.triangles;
                mData.outputMesh.normals = mData.bakeMesh.normals;
                mData.outputMesh.uv = mData.bakeMesh.uv;
                mData.outputMesh.tangents = mData.bakeMesh.tangents;
            }

            data.activeMeshes.Add(mData);
        }
        activeMaleNPCs.Add(data);
        LogDebug($"✓ Male NPC '{npc.name}' set up with {data.activeMeshes.Count} meshes");
    }

    // ========================================================================
    // HELPERS
    // ========================================================================
    Transform GetOrCreatePivot(string pivotName, Transform parent, Vector3 defaultLocalPos)
    {
        Transform found = parent.Find(pivotName);
        return found != null ? found : CreatePivot(pivotName, parent, defaultLocalPos);
    }

    Transform CreatePivot(string n, Transform parent, Vector3 localPos)
    {
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        return go.transform;
    }

    float GetCombinedWeight(Vector3 worldPos, Transform pivot, Vector3 limbBox, Vector3 extremityBox, float extremityY, float falloff, float fHeight)
    {
        Vector3 local = pivot.InverseTransformPoint(worldPos);
        return Mathf.Max(
            CalculateBoxWeight(local, Vector3.zero, limbBox, falloff, fHeight, true),
            CalculateBoxWeight(local, new Vector3(0, extremityY, 0), extremityBox, falloff, fHeight, false)
        );
    }

    float CalculateBoxWeight(Vector3 localPos, Vector3 boxCenter, Vector3 dims, float falloff, float fHeight, bool isTopDown)
    {
        Vector3 rel = localPos - boxCenter;
        float dX = Mathf.Max(0, Mathf.Abs(rel.x) - dims.x * 0.5f);
        float dZ = Mathf.Max(0, Mathf.Abs(rel.z) - dims.z * 0.5f);
        float dY = isTopDown
            ? (rel.y > 0 ? rel.y : rel.y < -dims.y ? Mathf.Abs(rel.y + dims.y) : 0)
            : Mathf.Max(0, Mathf.Abs(rel.y) - dims.y * 0.5f);

        float dist = new Vector3(dX, dY, dZ).magnitude;
        if (dist <= 0) return 1f;
        if (Mathf.Abs(rel.y) > fHeight) return 0f;
        return dist >= falloff ? 0f : 1f - dist / falloff;
    }

    // ========================================================================
    // UPDATE LOOP
    // ========================================================================
    void Update()
    {
        // --- MALE ANIMATION ---
        foreach (var npc in activeMaleNPCs)
        {
            float speed = Vector3.Distance(npc.root.position, npc.lastPos) / Time.deltaTime;
            npc.lastPos = npc.root.position;

            if (speed > 0.01f)
            {
                npc.swingPhase += Time.deltaTime * npc.profile.swingSpeed * (speed * 0.7f);
            }

            npc.leftLegPivot.localRotation = Quaternion.Euler(Mathf.Sin(npc.swingPhase) * npc.profile.legSwingAngle, 0, 0);
            npc.rightLegPivot.localRotation = Quaternion.Euler(Mathf.Sin(npc.swingPhase + Mathf.PI) * npc.profile.legSwingAngle, 0, 0);
            npc.leftArmPivot.localRotation = Quaternion.Euler(Mathf.Sin(npc.swingPhase + Mathf.PI) * npc.profile.armSwingAngle, 0, 0);
            npc.rightArmPivot.localRotation = Quaternion.Euler(Mathf.Sin(npc.swingPhase) * npc.profile.armSwingAngle, 0, 0);
        }
    }

    // ========================================================================
    // LATE UPDATE (MALE DEFORMATION)
    // ========================================================================
    void LateUpdate()
    {
        foreach (var npc in activeMaleNPCs)
        {
            MaleGenderSettings p = npc.profile;

            foreach (var mesh in npc.activeMeshes)
            {
                if (!mesh.outputRenderer.isVisible) continue;

                if (mesh.isSkinned)
                    mesh.sourceSmr.BakeMesh(mesh.bakeMesh);

                Vector3[] baseVerts = mesh.isSkinned ? mesh.bakeMesh.vertices : mesh.originalVerts;
                Transform baseTransform = mesh.isSkinned ? mesh.sourceSmr.transform : mesh.meshTransform;
                Vector3[] deformed = new Vector3[baseVerts.Length];

                for (int v = 0; v < deformed.Length; v++)
                {
                    Vector3 baseWorld = baseTransform.TransformPoint(baseVerts[v]);
                    Vector3 targetPos = baseWorld;

                    float legL = mesh.leftLegWeightRaw[v];
                    float legR = mesh.rightLegWeightRaw[v];
                    float finalLegL, finalLegR;
                    if (legL > legR && legL > 0.001f) { finalLegL = legL; finalLegR = 0f; }
                    else if (legR > 0.001f) { finalLegR = legR; finalLegL = 0f; }
                    else { finalLegL = 0f; finalLegR = 0f; }

                    float wLA = Mathf.Max(
                        mesh.leftArmLimbWeight[v] >= p.armWeightThreshold ? mesh.leftArmLimbWeight[v] : 0f,
                        mesh.leftHandWeight[v] >= p.handWeightThreshold ? mesh.leftHandWeight[v] : 0f
                    );
                    float wRA = Mathf.Max(
                        mesh.rightArmLimbWeight[v] >= p.armWeightThreshold ? mesh.rightArmLimbWeight[v] : 0f,
                        mesh.rightHandWeight[v] >= p.handWeightThreshold ? mesh.rightHandWeight[v] : 0f
                    );

                    float domLeg = Mathf.Max(finalLegL, finalLegR);
                    float domArm = Mathf.Max(wLA, wRA);
                    if (domLeg >= p.limbExclusionThreshold && domLeg >= domArm) { wLA = 0f; wRA = 0f; }
                    else if (domArm >= p.limbExclusionThreshold && domArm > domLeg) { finalLegL = 0f; finalLegR = 0f; }

                    Vector3 pLL, pRL, pLA, pRA;
                    if (mesh.isSkinned)
                    {
                        pLL = npc.leftLegPivot.InverseTransformPoint(baseWorld);
                        pRL = npc.rightLegPivot.InverseTransformPoint(baseWorld);
                        pLA = npc.leftArmPivot.InverseTransformPoint(baseWorld);
                        pRA = npc.rightArmPivot.InverseTransformPoint(baseWorld);
                    }
                    else
                    {
                        pLL = mesh.localToLeftLeg[v];
                        pRL = mesh.localToRightLeg[v];
                        pLA = mesh.localToLeftArm[v];
                        pRA = mesh.localToRightArm[v];
                    }

                    if (finalLegL > 0) targetPos = Vector3.Lerp(baseWorld, npc.leftLegPivot.TransformPoint(pLL), finalLegL);
                    else if (finalLegR > 0) targetPos = Vector3.Lerp(baseWorld, npc.rightLegPivot.TransformPoint(pRL), finalLegR);

                    if (wLA > wRA && wLA > 0) targetPos = Vector3.Lerp(targetPos, npc.leftArmPivot.TransformPoint(pLA), wLA);
                    else if (wRA > 0) targetPos = Vector3.Lerp(targetPos, npc.rightArmPivot.TransformPoint(pRA), wRA);

                    deformed[v] = mesh.meshTransform.InverseTransformPoint(targetPos);
                }

                mesh.outputMesh.vertices = deformed;
                mesh.outputMesh.RecalculateNormals();
            }
        }
    }

    // ========================================================================
    // GIZMOS
    // ========================================================================
    void OnDrawGizmosSelected()
    {
        if (maleProfile != null && showMaleGizmos)
        {
            Gizmos.color = maleGizmoColor;
            // Draw a reference gizmo at 0,0,0 or you could assign a target
            Vector3 center = Vector3.zero;
            // Original logic used masterPedestrian, you might want to re-add a reference field for this if needed
            // For now drawing at world zero to keep script functional
            Gizmos.DrawWireCube(
                center + Vector3.up * (maleProfile.hipHeight - maleProfile.falloffHeight),
                new Vector3(maleProfile.hipWidth * 4, 0.02f, 0.5f));
        }
    }

    void OnDestroy()
    {
        activeMaleNPCs.Clear();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MalePedestrianSystem))]
public class MalePedestrianSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("⚙️ SETTINGS MANAGEMENT", EditorStyles.boldLabel);
        MalePedestrianSystem script = (MalePedestrianSystem)target;

        if (GUILayout.Button("Export Settings", GUILayout.Height(30)))
        {
            string path = EditorUtility.SaveFilePanel("Export Male Settings", "Assets", "MalePedestrianSettings", "json");
            if (!string.IsNullOrEmpty(path)) script.ExportSettings(path);
        }

        if (GUILayout.Button("Import Settings", GUILayout.Height(30)))
        {
            string path = EditorUtility.OpenFilePanel("Import Male Settings", "Assets", "json");
            if (!string.IsNullOrEmpty(path)) { script.ImportSettings(path); Repaint(); }
        }
    }
}
#endif