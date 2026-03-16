using System.Collections.Generic;
using UnityEngine;

public class ScooterDeformSystem : MonoBehaviour
{
    [Header("Whitelist Container")]
    [Tooltip("The parent object holding all scooter parts and NPC skins.")]
    public GameObject scooterRoot;
    public bool includeRiggedParts = true;

    [Header("Left Handlebar / Grip")]
    public GameObject leftGripSource;
    public float leftInfluenceRadius = 0.45f;

    [Header("Right Handlebar / Grip")]
    public GameObject rightGripSource;
    public float rightInfluenceRadius = 0.45f;

    [Header("Global Intensity")]
    [Range(0, 1)] public float deformGlobalWeight = 1.0f;

    private class DeformableElement
    {
        public Mesh meshInstance;
        public Renderer renderer;
        public Vector3[] baseCoords;
        public Vector3[] modifiedCoords;
        public float[] weightL;
        public float[] weightR;
        public Matrix4x4 initialLeftMatrix;
        public Matrix4x4 initialRightMatrix;
    }

    private List<DeformableElement> activeElements = new List<DeformableElement>();

    void Start()
    {
        // Short delay to ensure all 'unchecked' objects are registered by the engine
        Invoke(nameof(InitializeScooterElements), 0.15f);
    }

    public void InitializeScooterElements()
    {
        activeElements.Clear();
        if (scooterRoot == null) return;

        // Find every renderer (Mesh or Skinned) even if inactive
        Renderer[] allRenderers = scooterRoot.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer ren in allRenderers)
        {
            Mesh targetMesh = null;

            if (ren is MeshRenderer)
            {
                MeshFilter mf = ren.GetComponent<MeshFilter>();
                if (mf && mf.sharedMesh)
                {
                    targetMesh = Instantiate(mf.sharedMesh);
                    mf.mesh = targetMesh;
                }
            }
            else if (includeRiggedParts && ren is SkinnedMeshRenderer smr)
            {
                if (smr.sharedMesh)
                {
                    targetMesh = Instantiate(smr.sharedMesh);
                    smr.sharedMesh = targetMesh;
                }
            }

            if (targetMesh != null) RegisterElement(targetMesh, ren);
        }

        Debug.Log($"Scooter System: {activeElements.Count} parts bound and ready.");
    }

    void RegisterElement(Mesh m, Renderer r)
    {
        DeformableElement element = new DeformableElement
        {
            meshInstance = m,
            renderer = r,
            baseCoords = m.vertices,
            modifiedCoords = new Vector3[m.vertices.Length],
            weightL = new float[m.vertices.Length],
            weightR = new float[m.vertices.Length]
        };

        Transform t = r.transform;

        // Capture the "Rest Pose" of the scooter parts relative to the grips
        if (leftGripSource) element.initialLeftMatrix = leftGripSource.transform.worldToLocalMatrix * t.localToWorldMatrix;
        if (rightGripSource) element.initialRightMatrix = rightGripSource.transform.worldToLocalMatrix * t.localToWorldMatrix;

        for (int i = 0; i < element.baseCoords.Length; i++)
        {
            Vector3 worldPos = t.TransformPoint(element.baseCoords[i]);

            if (leftGripSource)
            {
                float d = Vector3.Distance(worldPos, leftGripSource.transform.position);
                element.weightL[i] = d <= leftInfluenceRadius ? Mathf.SmoothStep(1f, 0f, d / leftInfluenceRadius) : 0f;
            }

            if (rightGripSource)
            {
                float d = Vector3.Distance(worldPos, rightGripSource.transform.position);
                element.weightR[i] = d <= rightInfluenceRadius ? Mathf.SmoothStep(1f, 0f, d / rightInfluenceRadius) : 0f;
            }
        }

        activeElements.Add(element);
    }

    void LateUpdate()
    {
        if (activeElements.Count == 0) return;

        foreach (DeformableElement el in activeElements)
        {
            Transform t = el.renderer.transform;

            Matrix4x4 matL = leftGripSource ? t.worldToLocalMatrix * leftGripSource.transform.localToWorldMatrix * el.initialLeftMatrix : Matrix4x4.identity;
            Matrix4x4 matR = rightGripSource ? t.worldToLocalMatrix * rightGripSource.transform.localToWorldMatrix * el.initialRightMatrix : Matrix4x4.identity;

            for (int i = 0; i < el.baseCoords.Length; i++)
            {
                Vector3 origin = el.baseCoords[i];
                Vector3 result = origin;

                if (leftGripSource && el.weightL[i] > 0)
                    result = Vector3.Lerp(result, matL.MultiplyPoint3x4(origin), el.weightL[i] * deformGlobalWeight);

                if (rightGripSource && el.weightR[i] > 0)
                    result = Vector3.Lerp(result, matR.MultiplyPoint3x4(origin), el.weightR[i] * deformGlobalWeight);

                el.modifiedCoords[i] = result;
            }

            el.meshInstance.vertices = el.modifiedCoords;

            // Only calculate normals for active scooter parts to save battery/CPU
            if (el.renderer.gameObject.activeInHierarchy) el.meshInstance.RecalculateNormals();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (leftGripSource)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(leftGripSource.transform.position, leftInfluenceRadius);
        }
        if (rightGripSource)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(rightGripSource.transform.position, rightInfluenceRadius);
        }
    }
}