using System.Collections.Generic;
using UnityEngine;

public class RadiusMeshDeformer : MonoBehaviour
{
    [Header("Mesh Whitelist Setup")]
    [Tooltip("The parent GameObject containing all the meshes you want to deform. No keywords used!")]
    public Transform meshesParent;

    [Header("Left Control")]
    [Tooltip("The object that controls the left side deformation (e.g., Left Handlebar grip or IK Target)")]
    public Transform leftControl;
    public float leftRadius = 0.5f;

    [Header("Right Control")]
    [Tooltip("The object that controls the right side deformation (e.g., Right Handlebar grip or IK Target)")]
    public Transform rightControl;
    public float rightRadius = 0.5f;

    [Header("Deformation Settings")]
    [Range(0, 1)] public float deformationStrength = 1f;

    // Internal data structure to cache everything for performance
    private class MeshData
    {
        public MeshFilter filter;
        public Mesh mesh;
        public Vector3[] originalVertices;
        public Vector3[] currentVertices;

        // Cached weights so we don't calculate distance every frame
        public float[] leftWeights;
        public float[] rightWeights;

        // Matrices to store the original relationship between the mesh and the controls
        public Matrix4x4 meshToLeftControlStart;
        public Matrix4x4 meshToRightControlStart;
    }

    private List<MeshData> whitelistedMeshes = new List<MeshData>();

    void Start()
    {
        InitializeWhitelist();
    }

    void InitializeWhitelist()
    {
        if (meshesParent == null)
        {
            Debug.LogWarning("Mesh Deformer: No meshesParent assigned!");
            return;
        }

        // Grab all MeshFilters strictly from the assigned parent and its children
        MeshFilter[] filters = meshesParent.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mf in filters)
        {
            // Instantiate the mesh so we don't permanently modify the project asset
            Mesh instancedMesh = Instantiate(mf.sharedMesh);
            mf.mesh = instancedMesh;

            MeshData data = new MeshData
            {
                filter = mf,
                mesh = instancedMesh,
                originalVertices = instancedMesh.vertices,
                currentVertices = new Vector3[instancedMesh.vertices.Length],
                leftWeights = new float[instancedMesh.vertices.Length],
                rightWeights = new float[instancedMesh.vertices.Length]
            };

            Transform meshTransform = mf.transform;

            // Store the initial offset matrices (Bind Poses)
            if (leftControl) data.meshToLeftControlStart = leftControl.worldToLocalMatrix * meshTransform.localToWorldMatrix;
            if (rightControl) data.meshToRightControlStart = rightControl.worldToLocalMatrix * meshTransform.localToWorldMatrix;

            // Pre-calculate radius weights for every vertex
            for (int i = 0; i < data.originalVertices.Length; i++)
            {
                Vector3 worldVertPos = meshTransform.TransformPoint(data.originalVertices[i]);

                if (leftControl)
                {
                    float distL = Vector3.Distance(worldVertPos, leftControl.position);
                    // Using SmoothStep for a natural, curved falloff instead of a harsh linear line
                    data.leftWeights[i] = distL <= leftRadius ? Mathf.SmoothStep(1f, 0f, distL / leftRadius) : 0f;
                }

                if (rightControl)
                {
                    float distR = Vector3.Distance(worldVertPos, rightControl.position);
                    data.rightWeights[i] = distR <= rightRadius ? Mathf.SmoothStep(1f, 0f, distR / rightRadius) : 0f;
                }
            }

            whitelistedMeshes.Add(data);
        }
    }

    void LateUpdate()
    {
        if (whitelistedMeshes.Count == 0) return;

        foreach (MeshData data in whitelistedMeshes)
        {
            Transform meshTransform = data.filter.transform;

            // Calculate current matrices to map control movements back onto the mesh local space
            Matrix4x4 leftDeformMatrix = Matrix4x4.identity;
            Matrix4x4 rightDeformMatrix = Matrix4x4.identity;

            if (leftControl) leftDeformMatrix = meshTransform.worldToLocalMatrix * leftControl.localToWorldMatrix * data.meshToLeftControlStart;
            if (rightControl) rightDeformMatrix = meshTransform.worldToLocalMatrix * rightControl.localToWorldMatrix * data.meshToRightControlStart;

            for (int i = 0; i < data.originalVertices.Length; i++)
            {
                Vector3 origVert = data.originalVertices[i];
                Vector3 finalVert = origVert;

                // Apply Left Control Deformation
                if (leftControl && data.leftWeights[i] > 0)
                {
                    Vector3 leftTarget = leftDeformMatrix.MultiplyPoint3x4(origVert);
                    float weight = data.leftWeights[i] * deformationStrength;
                    finalVert = Vector3.Lerp(finalVert, leftTarget, weight);
                }

                // Apply Right Control Deformation
                if (rightControl && data.rightWeights[i] > 0)
                {
                    Vector3 rightTarget = rightDeformMatrix.MultiplyPoint3x4(origVert);
                    float weight = data.rightWeights[i] * deformationStrength;
                    finalVert = Vector3.Lerp(finalVert, rightTarget, weight);
                }

                data.currentVertices[i] = finalVert;
            }

            // Apply deformed vertices to the mesh
            data.mesh.vertices = data.currentVertices;
            data.mesh.RecalculateNormals();
        }
    }

    // Draws spheres in the editor so you can physically see the radiuses 
    void OnDrawGizmosSelected()
    {
        if (leftControl)
        {
            Gizmos.color = new Color(1, 0, 0, 0.4f);
            Gizmos.DrawWireSphere(leftControl.position, leftRadius);
        }
        if (rightControl)
        {
            Gizmos.color = new Color(0, 0, 1, 0.4f);
            Gizmos.DrawWireSphere(rightControl.position, rightRadius);
        }
    }
}