using System.Collections.Generic;
using UnityEngine;

public class RadiusMeshDeformer : MonoBehaviour
{
    [Header("Mesh Whitelist Setup")]
    public Transform meshesParent;
    public bool includeSkinnedMeshes = true;

    [Header("Controls")]
    public Transform leftControl;
    public float leftRadius = 0.5f;
    public Transform rightControl;
    public float rightRadius = 0.5f;

    [Header("Settings")]
    [Range(0, 1)] public float deformationStrength = 1f;

    private class MeshData
    {
        public Mesh mesh;
        public MeshFilter filter;
        public SkinnedMeshRenderer skinner;
        public Vector3[] originalVertices;
        public Vector3[] currentVertices;
        public float[] leftWeights;
        public float[] rightWeights;
        public Matrix4x4 meshToLeftStart;
        public Matrix4x4 meshToRightStart;
    }

    private List<MeshData> whitelistedMeshes = new List<MeshData>();

    void Start()
    {
        // Give the scene a tiny moment to settle, then bind everything
        Invoke(nameof(SetupAndBindMeshes), 0.1f);
    }

    public void SetupAndBindMeshes()
    {
        whitelistedMeshes.Clear();
        if (meshesParent == null) return;

        // THE KEY CHANGE: Passing 'true' into GetComponentsInChildren 
        // forces Unity to find the "unchecked" (inactive) NPCs.
        MeshFilter[] filters = meshesParent.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            Mesh instancedMesh = Instantiate(mf.sharedMesh);
            mf.mesh = instancedMesh;
            RegisterMeshData(instancedMesh, mf.transform, mf, null);
        }

        if (includeSkinnedMeshes)
        {
            SkinnedMeshRenderer[] skinners = meshesParent.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in skinners)
            {
                if (smr.sharedMesh == null) continue;
                Mesh instancedMesh = Instantiate(smr.sharedMesh);
                smr.sharedMesh = instancedMesh;
                RegisterMeshData(instancedMesh, smr.transform, null, smr);
            }
        }
       // Debug.Log($"Successfully bound {whitelistedMeshes.Count} meshes (including hidden ones).");
    }

    void RegisterMeshData(Mesh m, Transform t, MeshFilter mf, SkinnedMeshRenderer smr)
    {
        MeshData data = new MeshData
        {
            mesh = m,
            filter = mf,
            skinner = smr,
            originalVertices = m.vertices,
            currentVertices = new Vector3[m.vertices.Length],
            leftWeights = new float[m.vertices.Length],
            rightWeights = new float[m.vertices.Length]
        };

        if (leftControl) data.meshToLeftStart = leftControl.worldToLocalMatrix * t.localToWorldMatrix;
        if (rightControl) data.meshToRightStart = rightControl.worldToLocalMatrix * t.localToWorldMatrix;

        for (int i = 0; i < data.originalVertices.Length; i++)
        {
            Vector3 worldV = t.TransformPoint(data.originalVertices[i]);
            if (leftControl)
            {
                float d = Vector3.Distance(worldV, leftControl.position);
                data.leftWeights[i] = d <= leftRadius ? Mathf.SmoothStep(1f, 0f, d / leftRadius) : 0f;
            }
            if (rightControl)
            {
                float d = Vector3.Distance(worldV, rightControl.position);
                data.rightWeights[i] = d <= rightRadius ? Mathf.SmoothStep(1f, 0f, d / rightRadius) : 0f;
            }
        }
        whitelistedMeshes.Add(data);
    }

    void LateUpdate()
    {
        foreach (MeshData data in whitelistedMeshes)
        {
            // Even if the NPC is unchecked, we update the mesh data. 
            // If you want to save CPU, you could add: if (!meshTransform.gameObject.activeInHierarchy) continue;
            Transform t = data.filter != null ? data.filter.transform : data.skinner.transform;

            Matrix4x4 leftMat = leftControl ? t.worldToLocalMatrix * leftControl.localToWorldMatrix * data.meshToLeftStart : Matrix4x4.identity;
            Matrix4x4 rightMat = rightControl ? t.worldToLocalMatrix * rightControl.localToWorldMatrix * data.meshToRightStart : Matrix4x4.identity;

            for (int i = 0; i < data.originalVertices.Length; i++)
            {
                Vector3 v = data.originalVertices[i];
                Vector3 target = v;

                if (leftControl && data.leftWeights[i] > 0)
                    target = Vector3.Lerp(target, leftMat.MultiplyPoint3x4(v), data.leftWeights[i] * deformationStrength);

                if (rightControl && data.rightWeights[i] > 0)
                    target = Vector3.Lerp(target, rightMat.MultiplyPoint3x4(v), data.rightWeights[i] * deformationStrength);

                data.currentVertices[i] = target;
            }
            data.mesh.vertices = data.currentVertices;
            // Only recalculate normals for objects currently visible to save performance
            if (t.gameObject.activeInHierarchy) data.mesh.RecalculateNormals();
        }
    }
}