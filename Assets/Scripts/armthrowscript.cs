using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiArmSwing : MonoBehaviour
{
    [System.Serializable]
    public class ArmOverride
    {
        public bool useCustomOffset = false;
        public Vector3 customOffset = Vector3.zero;
    }

    [Header("Arm Meshes")]
    [Tooltip("Drag any mesh transforms here - can be from anywhere in the scene")]
    public List<Transform> armMeshes = new List<Transform>();

    [Header("Hold Toggle Control")]
    [Tooltip("Drag a GameObject here. Toggling its active state will control the arms.")]
    public GameObject holdToggle;
    [Tooltip("If unchecked: Arms hold when Toggle is ON. If checked: Arms hold when Toggle is OFF.")]
    public bool invertToggleLogic = false;

    [Header("Universal Settings")]
    [SerializeField] private float swingAngle = 45f;
    [SerializeField] private float swingDuration = 0.4f;
    [SerializeField] private AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float returnSmoothing = 8f;
    [Tooltip("1 = forward, -1 = backward")]
    [SerializeField] private float swingDirection = 1f;
    [Tooltip("Local axis to rotate around")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;

    [Header("Pivot Point Offset")]
    [Tooltip("Global offset from the mesh transform position to create a custom rotation pivot")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;
    [Tooltip("Show the offset pivot point in gizmos instead of the transform position")]
    [SerializeField] private bool showOffsetPivot = true;

    [Header("Per-Arm Pivot Overrides")]
    [Tooltip("Allow overriding the pivot offset for specific arms if their 3D model origins are different")]
    public List<ArmOverride> armOverrides = new List<ArmOverride>();

    [Header("Gizmo Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float gizmoSize = 0.05f;
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private Color activeSwingColor = Color.green;
    [SerializeField] private bool showRotationAxis = true;
    [SerializeField] private float axisLength = 0.2f;

    private List<Quaternion> defaultRotations = new List<Quaternion>();
    private List<Vector3> defaultPositions = new List<Vector3>();
    private List<Vector3> defaultScales = new List<Vector3>();
    private List<Transform> originalParents = new List<Transform>();
    private List<bool> isSwinging = new List<bool>();

    private List<Coroutine> swingCoroutines = new List<Coroutine>();
    private List<GameObject> activePivots = new List<GameObject>();

    private bool isCurrentlyHeld = false;

    void Start()
    {
        CacheDefaults();

        // Evaluate combined starting state
        bool shouldHoldOnStart = EvaluateHoldState();

        if (shouldHoldOnStart)
        {
            isCurrentlyHeld = true;
            HoldAll();
        }
    }

    void Update()
    {
        // Check combined state of toggle and inverted logic
        bool targetState = EvaluateHoldState();

        if (targetState && !isCurrentlyHeld)
        {
            HoldAll();
            isCurrentlyHeld = true;
        }
        else if (!targetState && isCurrentlyHeld)
        {
            ReleaseAll();
            isCurrentlyHeld = false;
        }
    }

    // Determines if arms SHOULD be held based on the toggle and invert checkmark
    private bool EvaluateHoldState()
    {
        if (holdToggle == null) return false;

        if (invertToggleLogic)
        {
            return !holdToggle.activeInHierarchy;
        }
        else
        {
            return holdToggle.activeInHierarchy;
        }
    }

    public void CacheDefaults()
    {
        defaultRotations.Clear();
        defaultPositions.Clear();
        defaultScales.Clear();
        originalParents.Clear();
        isSwinging.Clear();
        swingCoroutines.Clear();
        activePivots.Clear();

        foreach (var mesh in armMeshes)
        {
            if (mesh != null)
            {
                defaultRotations.Add(mesh.localRotation);
                defaultPositions.Add(mesh.localPosition);
                defaultScales.Add(mesh.localScale);
                originalParents.Add(mesh.parent);
                isSwinging.Add(false);
                swingCoroutines.Add(null);
                activePivots.Add(null);
            }
        }

        while (armOverrides.Count < armMeshes.Count) armOverrides.Add(new ArmOverride());
        while (armOverrides.Count > armMeshes.Count) armOverrides.RemoveAt(armOverrides.Count - 1);
    }

    public Vector3 GetPivotPosition(Transform arm, int index = -1)
    {
        if (arm == null) return Vector3.zero;

        Vector3 usedOffset = pivotOffset;

        if (index >= 0 && index < armOverrides.Count && armOverrides[index].useCustomOffset)
        {
            usedOffset = armOverrides[index].customOffset;
        }

        return arm.position + arm.TransformDirection(usedOffset);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        for (int i = 0; i < armMeshes.Count; i++)
        {
            var mesh = armMeshes[i];
            if (mesh == null) continue;

            bool swinging = (i < isSwinging.Count) ? isSwinging[i] : false;
            Gizmos.color = swinging ? activeSwingColor : gizmoColor;

            Vector3 pivotPos = showOffsetPivot ? GetPivotPosition(mesh, i) : mesh.position;
            Gizmos.DrawSphere(pivotPos, gizmoSize);

            if (showOffsetPivot)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(mesh.position, pivotPos);
                Gizmos.color = swinging ? activeSwingColor : gizmoColor;
            }

            if (showRotationAxis)
            {
                Vector3 worldAxis = mesh.TransformDirection(rotationAxis.normalized);
                Gizmos.DrawLine(pivotPos, pivotPos + worldAxis * axisLength);

                Vector3 arrowPos = pivotPos + worldAxis * axisLength;
                Vector3 perp = Vector3.Cross(worldAxis, Vector3.up).normalized;
                if (perp == Vector3.zero) perp = Vector3.Cross(worldAxis, Vector3.forward).normalized;

                Gizmos.DrawLine(arrowPos, arrowPos - worldAxis * 0.05f + perp * 0.02f);
                Gizmos.DrawLine(arrowPos, arrowPos - worldAxis * 0.05f - perp * 0.02f);
            }

#if UNITY_EDITOR
            string label = $"Arm {i}";
            if (i < armOverrides.Count && armOverrides[i].useCustomOffset) label += " (Custom)";
            UnityEditor.Handles.Label(pivotPos + Vector3.up * gizmoSize * 2, label);
#endif
        }
    }

    private void HoldAll()
    {
        for (int i = 0; i < armMeshes.Count; i++)
        {
            if (armMeshes[i] == null) continue;
            if (!armMeshes[i].gameObject.activeInHierarchy) continue;

            if (isSwinging[i] && swingCoroutines[i] != null)
            {
                StopCoroutine(swingCoroutines[i]);
                ForceCleanup(i);
            }

            swingCoroutines[i] = StartCoroutine(HoldRoutine(i));
        }
    }

    private void ReleaseAll()
    {
        for (int i = 0; i < armMeshes.Count; i++)
        {
            if (armMeshes[i] == null) continue;

            if (isSwinging[i] && swingCoroutines[i] != null)
            {
                StopCoroutine(swingCoroutines[i]);
            }

            swingCoroutines[i] = StartCoroutine(ReleaseRoutine(i));
        }
    }

    IEnumerator HoldRoutine(int index)
    {
        isSwinging[index] = true;
        Transform target = armMeshes[index];
        Transform originalParent = originalParents[index] != null ? originalParents[index] : transform;

        GameObject pivotObj = new GameObject("HoldPivot");
        pivotObj.hideFlags = HideFlags.HideAndDontSave;

        // Parent the pivot to the original parent FIRST so it moves with the object
        pivotObj.transform.SetParent(originalParent, worldPositionStays: true);

        // Snap exactly to the desired pivot point
        pivotObj.transform.position = GetPivotPosition(target, index);
        pivotObj.transform.rotation = target.rotation;

        activePivots[index] = pivotObj;
        target.SetParent(pivotObj.transform, true);

        float timer = 0f;
        float halfDuration = swingDuration * 0.5f;

        // Swing forward to the peak
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = swingCurve.Evaluate(timer / halfDuration);
            float current = Mathf.Lerp(0, swingAngle * swingDirection, t);
            pivotObj.transform.localRotation = Quaternion.AngleAxis(current, rotationAxis);
            yield return null;
        }

        // Snap exactly to target angle and hold indefinitely
        pivotObj.transform.localRotation = Quaternion.AngleAxis(swingAngle * swingDirection, rotationAxis);
    }

    IEnumerator ReleaseRoutine(int index)
    {
        Transform target = armMeshes[index];

        if (index < activePivots.Count && activePivots[index] != null)
        {
            GameObject pivotObj = activePivots[index];
            Quaternion startRot = pivotObj.transform.localRotation;
            Quaternion endRot = Quaternion.identity;

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * returnSmoothing;
                pivotObj.transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            // Pivot is back to 0. Safely detach and clean up.
            if (index < originalParents.Count && originalParents[index] != null)
            {
                target.SetParent(originalParents[index], true);
            }

            target.localPosition = defaultPositions[index];
            target.localRotation = defaultRotations[index];
            target.localScale = defaultScales[index];

            Destroy(pivotObj);
            activePivots[index] = null;
        }
        else
        {
            Quaternion start = target.localRotation;
            Quaternion defaultRot = defaultRotations[index];
            Vector3 defaultPos = defaultPositions[index];
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * returnSmoothing;
                target.localRotation = Quaternion.Slerp(start, defaultRot, t);
                yield return null;
            }
            target.localRotation = defaultRot;
            target.localPosition = defaultPos;
        }

        isSwinging[index] = false;
        swingCoroutines[index] = null;
    }

    private void ForceCleanup(int index)
    {
        Transform target = armMeshes[index];
        if (target == null) return;

        if (index < originalParents.Count && originalParents[index] != null)
        {
            target.SetParent(originalParents[index], true);
        }
        if (index < defaultPositions.Count) target.localPosition = defaultPositions[index];
        if (index < defaultRotations.Count) target.localRotation = defaultRotations[index];
        if (index < defaultScales.Count) target.localScale = defaultScales[index];

        if (index < activePivots.Count && activePivots[index] != null)
        {
            Destroy(activePivots[index]);
            activePivots[index] = null;
        }

        isSwinging[index] = false;
    }

    public void ReturnAllToIdle()
    {
        ReleaseAll();
        isCurrentlyHeld = false;
    }

    void OnDestroy()
    {
        foreach (var pivot in activePivots)
        {
            if (pivot != null) Destroy(pivot);
        }
    }
}