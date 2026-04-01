using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FlexibleBikeLegs_MultiDeform : MonoBehaviour
{
    [System.Serializable]
    public class DeformGroup
    {
        [Header("Primary Mesh (Master)")]
        public Transform primaryMeshTarget;

        [Header("Secondary Meshes (Slaves)")]
        public List<Transform> secondaryMeshTargets = new List<Transform>();

        [Header("Joints")]
        public Transform anchor;    // Hip
        public Transform tip;       // Foot
        public Transform footJoint; // Pedal Pivot

        public bool recalcNormals = true;

        [HideInInspector] public List<DeformData> meshData = new List<DeformData>();
        [HideInInspector] public bool ready;
    }

    [System.Serializable]
    public class DeformData
    {
        public Transform target;
        public Mesh deformMesh;
        public float[] vertT;
        public Vector3[] localVertPerp;
        public Vector3 localRestDir;
        public float restLen;
    }

    [System.Serializable]
    public class LegMember
    {
        public string sideName;
        public Transform pedalTarget;

        public List<Transform> footMeshes = new List<Transform>();
        public List<DeformGroup> deformGroups = new List<DeformGroup>();

        [HideInInspector] public List<Vector3> footRelOffsets = new List<Vector3>();
        [HideInInspector] public List<Quaternion> footRelRots = new List<Quaternion>();
    }

    public LegMember left;
    public LegMember right;

    [Header("Debug")]
    public bool enableDebugLogs = false;
    public string debugPrefix = "";

    void Start()
    {
        SetupLeg(left);
        SetupLeg(right);
    }

    void SetupLeg(LegMember leg)
    {
        if (leg == null || leg.pedalTarget == null)
        {
            LogWarning($"Leg setup skipped - leg or pedalTarget is null");
            return;
        }

        leg.footRelOffsets.Clear();
        leg.footRelRots.Clear();

        foreach (Transform f in leg.footMeshes)
        {
            if (f == null)
            {
                LogWarning($"Skipping null foot mesh in {leg.sideName}");
                continue;
            }
            leg.footRelOffsets.Add(leg.pedalTarget.InverseTransformPoint(f.position));
            leg.footRelRots.Add(Quaternion.Inverse(transform.rotation) * f.rotation);
        }

        Log($"Setup {leg.sideName} leg: {leg.footMeshes.Count} foot meshes, {leg.deformGroups.Count} deform groups");

        foreach (var group in leg.deformGroups)
        {
            if (group != null) SetupDeformGroup(group);
        }
    }

    void SetupDeformGroup(DeformGroup group)
    {
        if (group.primaryMeshTarget == null)
        {
            LogWarning("SetupDeformGroup skipped - primaryMeshTarget is null");
            return;
        }

        if (group.anchor == null || group.tip == null || group.footJoint == null)
        {
            LogError($"Missing joints in deform group! Anchor: {group.anchor != null}, Tip: {group.tip != null}, FootJoint: {group.footJoint != null}");
            group.ready = false;
            return;
        }

        Vector3 scale = transform.lossyScale;
        if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f) || Mathf.Approximately(scale.z, 0f))
        {
            LogError($"Transform has zero scale! Scale: {scale}. This will cause NaN values.");
            group.ready = false;
            return;
        }

        group.meshData.Clear();
        InitializeMeshData(group, group.primaryMeshTarget);

        foreach (Transform t in group.secondaryMeshTargets)
        {
            if (t != null) InitializeMeshData(group, t);
        }

        group.ready = group.meshData.Count > 0;
        Log($"Deform group ready: {group.ready}, mesh data count: {group.meshData.Count}");
    }

    void InitializeMeshData(DeformGroup group, Transform target)
    {
        if (target == null) return;

        MeshFilter mf = target.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            LogWarning($"Target {target.name} is missing a MeshFilter or Mesh!");
            return;
        }

        DeformData data = new DeformData();
        data.target = target;
        data.deformMesh = Instantiate(mf.sharedMesh);
        data.deformMesh.MarkDynamic();
        mf.mesh = data.deformMesh;

        Vector3 worldBoneVec = group.tip.position - group.anchor.position;
        data.restLen = worldBoneVec.magnitude;

        if (data.restLen < 0.001f)
        {
            LogError($"Bone length is zero or near-zero ({data.restLen}) for {target.name}!");
            return;
        }

        Vector3 normalizedBone = worldBoneVec / data.restLen;

        if (IsNaN(normalizedBone))
        {
            LogError($"Normalized bone vector is NaN for {target.name}!");
            return;
        }

        data.localRestDir = transform.InverseTransformDirection(normalizedBone);

        if (IsNaN(data.localRestDir))
        {
            LogError($"localRestDir is NaN after InverseTransformDirection! Check for zero scale. Scale: {transform.lossyScale}");
            data.localRestDir = Vector3.up;
        }

        Vector3[] verts = data.deformMesh.vertices;
        data.vertT = new float[verts.Length];
        data.localVertPerp = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldV = target.TransformPoint(verts[i]);

            if (IsNaN(worldV))
            {
                data.vertT[i] = 0f;
                data.localVertPerp[i] = Vector3.zero;
                continue;
            }

            Vector3 fromAnchor = worldV - group.anchor.position;
            float t = Vector3.Dot(fromAnchor, normalizedBone) / data.restLen;
            data.vertT[i] = t;

            Vector3 worldPerp = fromAnchor - (t * data.restLen * normalizedBone);
            data.localVertPerp[i] = transform.InverseTransformDirection(worldPerp);

            if (IsNaN(data.localVertPerp[i]))
            {
                data.localVertPerp[i] = Vector3.zero;
            }
        }

        group.meshData.Add(data);
    }

    void LateUpdate()
    {
        UpdateLeg(left);
        UpdateLeg(right);
    }

    void UpdateLeg(LegMember leg)
    {
        if (leg == null || leg.pedalTarget == null) return;

        foreach (var group in leg.deformGroups)
        {
            if (group == null || !group.ready || group.anchor == null || group.tip == null || group.footJoint == null) continue;

            group.tip.position = group.footJoint.position;

            foreach (var data in group.meshData)
            {
                UpdateMeshDeformation(group, data);
            }
        }

        for (int i = 0; i < leg.footMeshes.Count; i++)
        {
            if (i >= leg.footRelOffsets.Count || leg.footMeshes[i] == null) continue;

            leg.footMeshes[i].position = leg.pedalTarget.TransformPoint(leg.footRelOffsets[i]);
            leg.footMeshes[i].rotation = transform.rotation * leg.footRelRots[i];
        }
    }

    void UpdateMeshDeformation(DeformGroup group, DeformData data)
    {
        Vector3 currBoneVec = group.tip.position - group.anchor.position;
        float currLen = currBoneVec.magnitude;

        if (currLen < 0.001f) return;

        Vector3 currDir = currBoneVec / currLen;
        Vector3 worldRestDir = transform.TransformDirection(data.localRestDir);

        if (IsNaN(worldRestDir) || IsNaN(currDir) || worldRestDir.sqrMagnitude < 0.0001f || currDir.sqrMagnitude < 0.0001f) return;

        float dot = Vector3.Dot(worldRestDir.normalized, currDir.normalized);

        Quaternion boneRot;
        if (dot > 0.9999f)
        {
            boneRot = Quaternion.identity;
        }
        else if (dot < -0.9999f)
        {
            Vector3 perp = Vector3.Cross(worldRestDir, Vector3.up);
            if (perp.sqrMagnitude < 0.001f) perp = Vector3.Cross(worldRestDir, Vector3.right);
            boneRot = Quaternion.AngleAxis(180f, perp.normalized);
        }
        else
        {
            boneRot = Quaternion.FromToRotation(worldRestDir.normalized, currDir.normalized);
        }

        if (IsNaN(boneRot)) return;

        UpdateMeshWithRotation(group, data, currDir, currLen, boneRot);
    }

    void UpdateMeshWithRotation(DeformGroup group, DeformData data, Vector3 currDir, float currLen, Quaternion boneRot)
    {
        // --- THE FIX: FORCE THE MESHFILTER TO USE OUR MESH ---
        // If another script swapped the mesh out behind our backs, we steal it back immediately.
        MeshFilter mf = data.target.GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != data.deformMesh)
        {
            mf.mesh = data.deformMesh;
        }
        // ------------------------------------------------------

        Vector3[] verts = data.deformMesh.vertices;
        bool hasNaN = false;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldPos = group.anchor.position + (data.vertT[i] * currLen * currDir);
            Vector3 worldPerp = transform.TransformDirection(data.localVertPerp[i]);
            Vector3 rotatedPerp = boneRot * worldPerp;
            worldPos += rotatedPerp;

            verts[i] = data.target.InverseTransformPoint(worldPos);

            if (IsNaN(verts[i]))
            {
                hasNaN = true;
                verts[i] = Vector3.zero;
            }
        }

        data.deformMesh.vertices = verts;
        data.deformMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 20f);
        if (group.recalcNormals) data.deformMesh.RecalculateNormals();

        if (hasNaN)
        {
            LogError($"NaN detected in final vertices for {data.target.name}!");
        }
    }

    #region Utility Methods

    private bool IsNaN(Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
               float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
    }

    private bool IsNaN(Quaternion q)
    {
        return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w) ||
               float.IsInfinity(q.x) || float.IsInfinity(q.y) || float.IsInfinity(q.z) || float.IsInfinity(q.w);
    }

    private void Log(string message)
    {
        if (enableDebugLogs) Debug.Log($"[{debugPrefix}{gameObject.name}] {message}");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[{debugPrefix}{gameObject.name}] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[{debugPrefix}{gameObject.name}] {message}");
    }

    #endregion

    #region Export/Import Methods

    [System.Serializable]
    public class ExportData
    {
        public string scriptName;
        public ExportLegData leftLeg;
        public ExportLegData rightLeg;
    }

    [System.Serializable]
    public class ExportLegData
    {
        public string sideName;
        public string pedalTargetPath;
        public List<string> footMeshPaths = new List<string>();
        public List<ExportDeformGroupData> deformGroups = new List<ExportDeformGroupData>();
    }

    [System.Serializable]
    public class ExportDeformGroupData
    {
        public string primaryMeshTargetPath;
        public List<string> secondaryMeshTargetPaths = new List<string>();
        public string anchorPath;
        public string tipPath;
        public string footJointPath;
        public bool recalcNormals;
    }

    private string GetTransformPath(Transform t)
    {
        if (t == null) return null;
        return GetFullPath(t);
    }

    private string GetFullPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetFullPath(t.parent) + "/" + t.name;
    }

    private Transform FindTransformByPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return transform.root.Find(path);
    }

    public void ExportSettings()
    {
#if UNITY_EDITOR
        ExportData exportData = new ExportData
        {
            scriptName = gameObject.name,
            leftLeg = ExportLegDataFromLeg(left),
            rightLeg = ExportLegDataFromLeg(right)
        };

        string json = JsonUtility.ToJson(exportData, true);
        string defaultFileName = $"{gameObject.name}_BikeLegs_Settings.json";

        string filePath = EditorUtility.SaveFilePanel("Export Bike Legs Settings", "", defaultFileName, "json");

        if (!string.IsNullOrEmpty(filePath))
        {
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"Settings exported to: {filePath}");
            EditorUtility.DisplayDialog("Export Successful", $"Settings saved to:\n{filePath}", "OK");
        }
#else
        Debug.LogWarning("Export is only available in the Unity Editor.");
#endif
    }

    private ExportLegData ExportLegDataFromLeg(LegMember leg)
    {
        if (leg == null) return null;

        ExportLegData data = new ExportLegData
        {
            sideName = leg.sideName,
            pedalTargetPath = GetTransformPath(leg.pedalTarget)
        };

        foreach (Transform t in leg.footMeshes)
        {
            data.footMeshPaths.Add(GetTransformPath(t));
        }

        foreach (DeformGroup group in leg.deformGroups)
        {
            if (group == null) continue;

            ExportDeformGroupData groupData = new ExportDeformGroupData
            {
                primaryMeshTargetPath = GetTransformPath(group.primaryMeshTarget),
                anchorPath = GetTransformPath(group.anchor),
                tipPath = GetTransformPath(group.tip),
                footJointPath = GetTransformPath(group.footJoint),
                recalcNormals = group.recalcNormals
            };

            foreach (Transform t in group.secondaryMeshTargets)
            {
                groupData.secondaryMeshTargetPaths.Add(GetTransformPath(t));
            }

            data.deformGroups.Add(groupData);
        }

        return data;
    }

    public void ImportSettings()
    {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Import Bike Legs Settings", "", "json");

        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                ExportData importData = JsonUtility.FromJson<ExportData>(json);

                if (importData == null)
                {
                    EditorUtility.DisplayDialog("Import Failed", "Invalid JSON file.", "OK");
                    return;
                }

                Undo.RecordObject(this, "Import Bike Legs Settings");

                ImportLegDataToLeg(importData.leftLeg, ref left);
                ImportLegDataToLeg(importData.rightLeg, ref right);

                EditorUtility.SetDirty(this);
                Debug.Log($"Settings imported from: {filePath}");
                EditorUtility.DisplayDialog("Import Successful", "Settings loaded successfully!", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Import error: {e.Message}");
                EditorUtility.DisplayDialog("Import Failed", $"Error:\n{e.Message}", "OK");
            }
        }
#else
        Debug.LogWarning("Import is only available in the Unity Editor.");
#endif
    }

    private void ImportLegDataToLeg(ExportLegData data, ref LegMember leg)
    {
        if (data == null)
        {
            leg = null;
            return;
        }

        if (leg == null) leg = new LegMember();

        leg.sideName = data.sideName;
        leg.pedalTarget = FindTransformByPath(data.pedalTargetPath);

        leg.footMeshes.Clear();
        foreach (string path in data.footMeshPaths)
        {
            Transform t = FindTransformByPath(path);
            leg.footMeshes.Add(t);
        }

        leg.deformGroups.Clear();
        foreach (ExportDeformGroupData groupData in data.deformGroups)
        {
            DeformGroup group = new DeformGroup
            {
                primaryMeshTarget = FindTransformByPath(groupData.primaryMeshTargetPath),
                anchor = FindTransformByPath(groupData.anchorPath),
                tip = FindTransformByPath(groupData.tipPath),
                footJoint = FindTransformByPath(groupData.footJointPath),
                recalcNormals = groupData.recalcNormals
            };

            foreach (string path in groupData.secondaryMeshTargetPaths)
            {
                group.secondaryMeshTargets.Add(FindTransformByPath(path));
            }

            leg.deformGroups.Add(group);
        }
    }

    public void ResetLegs()
    {
        if (left != null)
        {
            left.footRelOffsets.Clear();
            left.footRelRots.Clear();
            foreach (var group in left.deformGroups)
            {
                if (group != null)
                {
                    group.meshData.Clear();
                    group.ready = false;
                }
            }
        }

        if (right != null)
        {
            right.footRelOffsets.Clear();
            right.footRelRots.Clear();
            foreach (var group in right.deformGroups)
            {
                if (group != null)
                {
                    group.meshData.Clear();
                    group.ready = false;
                }
            }
        }

        Log("Legs reset.");
    }

    #endregion
}