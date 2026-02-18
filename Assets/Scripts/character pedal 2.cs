using UnityEngine;
using System.Collections.Generic;

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

    void Start()
    {
        SetupLeg(left);
        SetupLeg(right);
    }

    void SetupLeg(LegMember leg)
    {
        if (leg == null || leg.pedalTarget == null) return;

        leg.footRelOffsets.Clear();
        leg.footRelRots.Clear();

        foreach (Transform f in leg.footMeshes)
        {
            if (f == null) continue; // Skip empty slots in inspector
            leg.footRelOffsets.Add(leg.pedalTarget.InverseTransformPoint(f.position));
            leg.footRelRots.Add(Quaternion.Inverse(transform.rotation) * f.rotation);
        }

        foreach (var group in leg.deformGroups)
        {
            if (group != null) SetupDeformGroup(group);
        }
    }

    void SetupDeformGroup(DeformGroup group)
    {
        if (group.primaryMeshTarget == null) return;
        group.meshData.Clear();

        // Setup Primary
        InitializeMeshData(group, group.primaryMeshTarget);

        // Setup Secondaries
        foreach (Transform t in group.secondaryMeshTargets)
        {
            if (t != null) InitializeMeshData(group, t);
        }

        group.ready = group.meshData.Count > 0;
    }

    void InitializeMeshData(DeformGroup group, Transform target)
    {
        if (target == null) return;

        MeshFilter mf = target.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning($"Target {target.name} is missing a MeshFilter or Mesh!");
            return;
        }

        DeformData data = new DeformData();
        data.target = target;
        data.deformMesh = Instantiate(mf.sharedMesh);
        data.deformMesh.MarkDynamic();
        mf.mesh = data.deformMesh;

        Vector3 worldBoneVec = group.tip.position - group.anchor.position;
        data.restLen = worldBoneVec.magnitude;
        if (data.restLen < 0.001f) return;

        data.localRestDir = transform.InverseTransformDirection(worldBoneVec.normalized);

        Vector3[] verts = data.deformMesh.vertices;
        data.vertT = new float[verts.Length];
        data.localVertPerp = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldV = target.TransformPoint(verts[i]);
            Vector3 fromAnchor = worldV - group.anchor.position;

            float t = Vector3.Dot(fromAnchor, worldBoneVec.normalized) / data.restLen;
            data.vertT[i] = t;

            Vector3 worldPerp = fromAnchor - (t * data.restLen * worldBoneVec.normalized);
            data.localVertPerp[i] = transform.InverseTransformDirection(worldPerp);
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

        // Update Deformation
        foreach (var group in leg.deformGroups)
        {
            if (group == null || !group.ready || group.anchor == null || group.tip == null || group.footJoint == null) continue;

            group.tip.position = group.footJoint.position;

            foreach (var data in group.meshData)
            {
                UpdateMeshDeformation(group, data);
            }
        }

        // Update Feet (Safe loop)
        for (int i = 0; i < leg.footMeshes.Count; i++)
        {
            // The IndexOutOfRange happened here because footRelOffsets.Count didn't match footMeshes.Count
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
        Quaternion boneRot = Quaternion.FromToRotation(worldRestDir, currDir);

        Vector3[] verts = data.deformMesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldPos = group.anchor.position + (data.vertT[i] * currLen * currDir);
            Vector3 worldPerp = transform.TransformDirection(data.localVertPerp[i]);
            worldPos += (boneRot * worldPerp);
            verts[i] = data.target.InverseTransformPoint(worldPos);
        }

        data.deformMesh.vertices = verts;
        data.deformMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 20f);
        if (group.recalcNormals) data.deformMesh.RecalculateNormals();
    }
}