using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MultiArmSwing : MonoBehaviour
{
    [Header("Arm Meshes")]
    [Tooltip("Drag any mesh transforms here - can be from anywhere in the scene")]
    public List<Transform> armMeshes = new List<Transform>();

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
    [Tooltip("Offset from the mesh transform position to create a custom rotation pivot")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;
    [Tooltip("Show the offset pivot point in gizmos instead of the transform position")]
    [SerializeField] private bool showOffsetPivot = true;

    [Header("Alternation")]
    [SerializeField] private bool alternateArms = true;
    [SerializeField] private bool sequentialMode = false;

    [Header("Editor Testing")]
    [SerializeField] private int testArmIndex = 0;

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

    // Robust tracking to prevent broken states when interrupts happen
    private List<Coroutine> swingCoroutines = new List<Coroutine>();
    private List<GameObject> activePivots = new List<GameObject>();

    private int lastArmIndex = -1;

    void Start()
    {
        CacheDefaults();
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
    }

    public Vector3 GetPivotPosition(Transform arm)
    {
        if (arm == null) return Vector3.zero;
        return arm.position + arm.TransformDirection(pivotOffset);
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

            Vector3 pivotPos = showOffsetPivot ? GetPivotPosition(mesh) : mesh.position;
            Gizmos.DrawSphere(pivotPos, gizmoSize);

            if (showOffsetPivot && pivotOffset != Vector3.zero)
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
            Handles.Label(pivotPos + Vector3.up * gizmoSize * 2, $"Arm {i}");
#endif
        }
    }

    public void TriggerArm(int index, bool allowRetrigger = false)
    {
        if (index < 0 || index >= armMeshes.Count) return;

        Transform arm = armMeshes[index];
        if (arm == null) return;

        // FIX: Use activeInHierarchy so it correctly detects if the arm itself OR its parents are unchecked
        if (!arm.gameObject.activeInHierarchy) return;

        if (!allowRetrigger && isSwinging[index]) return;

        // Stop existing swing safely using the Coroutine reference (prevents breaking other arms)
        if (isSwinging[index])
        {
            if (index < swingCoroutines.Count && swingCoroutines[index] != null)
            {
                StopCoroutine(swingCoroutines[index]);
            }
        }

        swingCoroutines[index] = StartCoroutine(SwingRoutine(index));
    }

    public void TriggerNext()
    {
        if (armMeshes.Count == 0) return;

        int nextIndex;
        if (sequentialMode)
        {
            nextIndex = (lastArmIndex + 1) % armMeshes.Count;
        }
        else if (alternateArms && armMeshes.Count == 2)
        {
            nextIndex = lastArmIndex == 0 ? 1 : 0;
        }
        else
        {
            do { nextIndex = Random.Range(0, armMeshes.Count); }
            while (nextIndex == lastArmIndex && armMeshes.Count > 1);
        }

        lastArmIndex = nextIndex;
        TriggerArm(nextIndex, false);
    }

    public void TriggerAll(bool forceRetrigger = true)
    {
        for (int i = 0; i < armMeshes.Count; i++)
        {
            if (armMeshes[i] == null) continue;
            if (!armMeshes[i].gameObject.activeInHierarchy) continue; // FIX: Hierarchy check

            TriggerArm(i, forceRetrigger);
        }
    }

    public void TriggerArms(params int[] indices)
    {
        foreach (int i in indices) TriggerArm(i, false);
    }

    public void TriggerRandom()
    {
        if (armMeshes.Count > 0) TriggerArm(Random.Range(0, armMeshes.Count), false);
    }

    public void TriggerWave(float delay = 0.1f)
    {
        StartCoroutine(WaveRoutine(delay));
    }

    IEnumerator WaveRoutine(float delay)
    {
        for (int i = 0; i < armMeshes.Count; i++)
        {
            if (armMeshes[i] == null || !armMeshes[i].gameObject.activeInHierarchy) continue; // FIX: Hierarchy check

            TriggerArm(i, false);
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator SwingRoutine(int index)
    {
        isSwinging[index] = true;
        Transform target = armMeshes[index];
        Quaternion defaultRot = defaultRotations[index];
        Vector3 defaultPos = defaultPositions[index];
        Vector3 defaultScale = defaultScales[index];
        Transform originalParent = originalParents[index];

        GameObject pivotObj = new GameObject("SwingPivot");
        pivotObj.hideFlags = HideFlags.HideAndDontSave;
        pivotObj.transform.position = GetPivotPosition(target);
        pivotObj.transform.rotation = target.rotation;

        activePivots[index] = pivotObj;
        target.SetParent(pivotObj.transform, true);

        float timer = 0f;

        // Forward swing
        while (timer < swingDuration * 0.5f)
        {
            timer += Time.deltaTime;
            float t = swingCurve.Evaluate(timer / (swingDuration * 0.5f));
            float current = Mathf.Lerp(0, swingAngle * swingDirection, t);
            pivotObj.transform.localRotation = Quaternion.AngleAxis(current, rotationAxis);
            yield return null;
        }

        // Backward swing
        timer = 0f;
        while (timer < swingDuration * 0.5f)
        {
            timer += Time.deltaTime;
            float t = swingCurve.Evaluate(timer / (swingDuration * 0.5f));
            float current = Mathf.Lerp(swingAngle * swingDirection, 0, t);
            pivotObj.transform.localRotation = Quaternion.AngleAxis(current, rotationAxis);
            yield return null;
        }

        // Restore original parent while preserving world transform
        if (originalParent != null) target.SetParent(originalParent, true);

        // Restore exact local values
        target.localPosition = defaultPos;
        target.localRotation = defaultRot;
        target.localScale = defaultScale;

        // Cleanup pivot safely
        if (activePivots[index] != null)
        {
            Destroy(activePivots[index]);
            activePivots[index] = null;
        }

        swingCoroutines[index] = null;
        isSwinging[index] = false;
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
        for (int i = 0; i < armMeshes.Count; i++)
        {
            if (armMeshes[i] != null)
            {
                // If it's currently swinging, safely interrupt and snap it back to its proper parent first
                if (isSwinging[i])
                {
                    if (i < swingCoroutines.Count && swingCoroutines[i] != null)
                    {
                        StopCoroutine(swingCoroutines[i]);
                        swingCoroutines[i] = null;
                    }
                    ForceCleanup(i);
                }
                StartCoroutine(SmoothReturn(i));
            }
        }
    }

    IEnumerator SmoothReturn(int index)
    {
        Transform target = armMeshes[index];
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

    public void StopArm(int index)
    {
        if (index >= 0 && index < armMeshes.Count)
        {
            if (isSwinging[index])
            {
                if (index < swingCoroutines.Count && swingCoroutines[index] != null)
                {
                    StopCoroutine(swingCoroutines[index]);
                    swingCoroutines[index] = null;
                }
                ForceCleanup(index);
            }
        }
    }

    public void StopAll()
    {
        for (int i = 0; i < armMeshes.Count; i++)
        {
            if (i < swingCoroutines.Count && swingCoroutines[i] != null)
            {
                StopCoroutine(swingCoroutines[i]);
                swingCoroutines[i] = null;
            }
            if (isSwinging[i])
            {
                ForceCleanup(i);
            }
        }
    }

    public bool IsAnySwinging()
    {
        foreach (var swinging in isSwinging) if (swinging) return true;
        return false;
    }

    public bool IsSwinging(int index)
    {
        return index >= 0 && index < isSwinging.Count && isSwinging[index];
    }

    public int Count => armMeshes.Count;

    public void AddArm(Transform mesh)
    {
        armMeshes.Add(mesh);
        defaultRotations.Add(mesh.localRotation);
        defaultPositions.Add(mesh.localPosition);
        defaultScales.Add(mesh.localScale);
        originalParents.Add(mesh.parent);
        isSwinging.Add(false);
        swingCoroutines.Add(null);
        activePivots.Add(null);
    }

    public void RemoveArm(int index)
    {
        if (index >= 0 && index < armMeshes.Count)
        {
            if (isSwinging[index]) ForceCleanup(index);

            armMeshes.RemoveAt(index);
            if (index < defaultRotations.Count) defaultRotations.RemoveAt(index);
            if (index < defaultPositions.Count) defaultPositions.RemoveAt(index);
            if (index < defaultScales.Count) defaultScales.RemoveAt(index);
            if (index < originalParents.Count) originalParents.RemoveAt(index);
            if (index < isSwinging.Count) isSwinging.RemoveAt(index);
            if (index < swingCoroutines.Count) swingCoroutines.RemoveAt(index);
            if (index < activePivots.Count) activePivots.RemoveAt(index);
        }
    }

    public void ClearArms()
    {
        StopAll();
        armMeshes.Clear();
        defaultRotations.Clear();
        defaultPositions.Clear();
        defaultScales.Clear();
        originalParents.Clear();
        isSwinging.Clear();
        swingCoroutines.Clear();
        activePivots.Clear();
    }

    [ContextMenu("Test Trigger Next")]
    void EditorTriggerNext() { if (Application.isPlaying) TriggerNext(); }

    [ContextMenu("Test Trigger All")]
    void EditorTriggerAll() { if (Application.isPlaying) TriggerAll(true); }

    [ContextMenu("Test Trigger Wave")]
    void EditorTriggerWave() { if (Application.isPlaying) TriggerWave(0.15f); }

    [ContextMenu("Test Return To Idle")]
    void EditorReturnToIdle() { if (Application.isPlaying) ReturnAllToIdle(); }

    [ContextMenu("Re-Cache Defaults")]
    void EditorCacheDefaults() => CacheDefaults();

    [ContextMenu("Find Arms In Children")]
    void FindInChildren()
    {
        armMeshes.Clear();
        FindRecursive(transform, "arm");
        CacheDefaults();
    }

    void FindRecursive(Transform parent, string searchTerm)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(searchTerm))
            {
                armMeshes.Add(child);
            }
            FindRecursive(child, searchTerm);
        }
    }

    public static MultiArmSwing FindAndTrigger(GameObject host, int armIndex = -1)
    {
        var swing = host.GetComponent<MultiArmSwing>();
        if (swing == null) swing = host.GetComponentInChildren<MultiArmSwing>();
        if (swing == null) swing = host.GetComponentInParent<MultiArmSwing>();

        if (swing != null)
        {
            if (armIndex < 0) swing.TriggerNext();
            else swing.TriggerArm(armIndex, false);
        }

        return swing;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MultiArmSwing))]
public class MultiArmSwingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MultiArmSwing script = (MultiArmSwing)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Test Buttons", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Trigger Next", GUILayout.Height(30)))
        {
            script.TriggerNext();
        }
        if (GUILayout.Button("Trigger All", GUILayout.Height(30)))
        {
            script.TriggerAll(true);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Trigger Wave", GUILayout.Height(30)))
        {
            script.TriggerWave(0.15f);
        }
        if (GUILayout.Button("Return To Idle", GUILayout.Height(30)))
        {
            script.ReturnAllToIdle();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Specific Arm:");
        int testIndex = EditorGUILayout.IntField(serializedObject.FindProperty("testArmIndex").intValue);
        if (GUILayout.Button("Trigger", GUILayout.Width(80)))
        {
            script.TriggerArm(testIndex, false);
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use test buttons", MessageType.Info);
        }
    }
}
#endif