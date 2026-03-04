using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FemaleLimbPendulumSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // How the pivot point for each limb is determined.
    // -------------------------------------------------------------------------
    public enum PivotMode
    {
        // Point on the mesh bounds closest to the character root — this is always the
        // attachment end (shoulder/hip) regardless of arm angle or character pose.
        RendererBoundsClosestToRoot,

        RendererBoundsTop,    // Highest world-Y — only correct when arm hangs straight down
        RendererBoundsBottom,
        RendererBoundsCenter,
        ManualOffset          // limb.position + pivotOffset in ROOT local space
    }

    [System.Serializable]
    public class LimbPattern
    {
        [Tooltip("Keywords to match transform names against. Use names specific to the top-level " +
                 "limb mesh only — do NOT mix parent and child names in the same pattern.")]
        public string[] nameKeywords = new string[] { "arm" };

        [Tooltip("Rotation axis in CHARACTER ROOT local space.")]
        public Vector3 rotationAxis = Vector3.right;

        [Tooltip("Baseline invert flag. Flip if the limb swings backwards.")]
        public bool invertSwing = false;

        [Tooltip("When true, left-side limbs (name contains l_, _l, or left) auto-invert.")]
        public bool autoInvertLeftSide = true;

        [Tooltip("RendererBoundsTop: finds the top of the arm mesh = shoulder. " +
                 "Recommended for boneless characters — no manual tuning needed.")]
        public PivotMode pivotMode = PivotMode.RendererBoundsTop;

        [Tooltip("Only used when PivotMode = ManualOffset. Offset in CHARACTER ROOT local space.")]
        public Vector3 pivotOffset = Vector3.zero;

        [Tooltip("Skip transforms with no MeshRenderer/SkinnedMeshRenderer. " +
                 "Keep ON for boneless characters to block empty parent containers from matching.")]
        public bool requireRenderer = true;
    }

    [System.Serializable]
    public class PendulumTarget
    {
        public string label;
        public Transform transform;
        public Transform rootTransform;
        public Vector3 rotationAxis = Vector3.right;
        public bool invertSwing = false;
        public string matchedPattern;

        // Tweak this in the Inspector (at runtime or edit-time) to nudge the pivot
        // from its auto-detected position. In LIMB LOCAL space — so (0,1,0) moves
        // the pivot one unit in the arm mesh's local up direction regardless of
        // how the character is oriented.
        [Tooltip("Fine-tune the pivot position in LIMB LOCAL space. Adjust this live " +
                 "while the scene plays — the magenta/cyan gizmo sphere moves with it.")]
        public Vector3 pivotAdjust = Vector3.zero;

        [HideInInspector] public Vector3 cachedLimbLocalPivot;
        [HideInInspector] public Vector3 initialLocalPos;
        [HideInInspector] public Quaternion initialLocalRot;
    }

    [System.Serializable]
    private class PendulumData
    {
        public float swingAngle;
        public float speed;
        public List<TargetSaveData> targets = new List<TargetSaveData>();
    }

    [System.Serializable]
    private class TargetSaveData
    {
        public string label;
        public string path;
        public string rootPath;
        public Vector3 limbLocalPivot;
        public Vector3 pivotAdjust;
        public Vector3 axis;
        public bool inverted;
    }

    // =========================================================================
    [Header("Detection")]
    public List<Transform> pathObjects = new List<Transform>();
    public bool autoRefreshOnUpdate = true;
    public float refreshInterval = 1.0f;

    [Header("Blacklist")]
    public string[] blacklistedWords = new string[] { "male", "man", "boy" };

    [Header("Limb Patterns")]
    public List<LimbPattern> limbPatterns = new List<LimbPattern>()
    {
        new LimbPattern {
            nameKeywords = new string[] { "arm" },
            rotationAxis = Vector3.right,
            invertSwing  = false,
            autoInvertLeftSide = true,
            pivotMode    = PivotMode.RendererBoundsClosestToRoot,
            requireRenderer = true
        },
        new LimbPattern {
            nameKeywords = new string[] { "leg" },
            rotationAxis = Vector3.right,
            invertSwing  = true,
            autoInvertLeftSide = true,
            pivotMode    = PivotMode.RendererBoundsClosestToRoot,
            requireRenderer = true
        }
    };

    [Header("Global Settings")]
    public float swingAngle = 30f;
    public float speed = 5.0f;

    [Header("Debug — Populated at Runtime")]
    public List<PendulumTarget> targets = new List<PendulumTarget>();

    private float lastRefreshTime;
    private HashSet<Transform> processedLimbs = new HashSet<Transform>();

    // Returns the pivot point in LIMB LOCAL space, captured at rest pose.
    // Stored as limb-local so TransformPoint reconstructs the correct world position
    // each frame regardless of where the character has moved or rotated.
    private Vector3 ComputeLimbLocalPivot(Transform limb, Transform root, LimbPattern pattern)
    {
        Vector3 worldPivot;

        if (pattern.pivotMode == PivotMode.ManualOffset)
        {
            worldPivot = root != null
                ? limb.position + root.TransformDirection(pattern.pivotOffset)
                : limb.position + pattern.pivotOffset;
        }
        else
        {
            Renderer rend = limb.GetComponent<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning($"[PendulumSystem] '{limb.name}' has no Renderer; pivot = transform origin.", limb);
                return Vector3.zero; // limb-local origin
            }

            Bounds b = rend.bounds; // world-space AABB

            switch (pattern.pivotMode)
            {
                case PivotMode.RendererBoundsClosestToRoot:
                    // The end of the limb closest to the body = shoulder / hip.
                    // ClosestPoint returns the surface point on the AABB nearest to root,
                    // which is correct for any arm angle (T-pose, arms up, hanging, etc.)
                    Vector3 rootPos = root != null ? root.position : limb.position;
                    worldPivot = b.ClosestPoint(rootPos);
                    break;

                case PivotMode.RendererBoundsTop:
                    worldPivot = new Vector3(b.center.x, b.max.y, b.center.z);
                    break;

                case PivotMode.RendererBoundsBottom:
                    worldPivot = new Vector3(b.center.x, b.min.y, b.center.z);
                    break;

                case PivotMode.RendererBoundsCenter:
                default:
                    worldPivot = b.center;
                    break;
            }
        }

        // Convert world pivot → limb local space.
        // After the rest-pose reset in LateUpdate, limb.TransformPoint(this)
        // reconstructs worldPivot at whatever position the character is now at.
        return limb.InverseTransformPoint(worldPivot);
    }

    // Called every frame AFTER the rest-pose reset.
    // cachedLimbLocalPivot = auto-detected shoulder/hip point in limb-local space.
    // pivotAdjust          = user nudge, also in limb-local space.
    private Vector3 GetWorldPivot(PendulumTarget pt)
    {
        return pt.transform.TransformPoint(pt.cachedLimbLocalPivot + pt.pivotAdjust);
    }

    private Vector3 GetWorldAxis(PendulumTarget pt)
    {
        return pt.rootTransform != null
            ? pt.rootTransform.TransformDirection(pt.rotationAxis).normalized
            : pt.rotationAxis.normalized;
    }

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================
    void Start() => RefreshAllTargets();

    void Update()
    {
        if (autoRefreshOnUpdate && Time.time - lastRefreshTime > refreshInterval)
        {
            RefreshAllTargets();
            lastRefreshTime = Time.time;
        }
    }

    void LateUpdate()
    {
        // Step 1: reset every limb to its captured rest pose.
        foreach (var pt in targets)
        {
            if (pt.transform == null) continue;
            pt.transform.localPosition = pt.initialLocalPos;
            pt.transform.localRotation = pt.initialLocalRot;
        }

        // Step 2: rotate each limb around its cached world pivot.
        float angleBase = Mathf.Sin(Time.time * speed) * swingAngle;

        foreach (var pt in targets)
        {
            if (pt.transform == null) continue;
            float angle = pt.invertSwing ? -angleBase : angleBase;
            pt.transform.RotateAround(GetWorldPivot(pt), GetWorldAxis(pt), angle);
        }

        targets.RemoveAll(t => t.transform == null);
    }

    // =========================================================================
    // REFRESH
    // =========================================================================
    [ContextMenu("Refresh All Targets")]
    public void RefreshAllTargets()
    {
        var candidates = new List<PendulumTarget>();
        var skippedRoots = new HashSet<Transform>();

        foreach (Transform pathParent in pathObjects)
        {
            if (pathParent == null) continue;

            foreach (Transform t in pathParent.GetComponentsInChildren<Transform>(true))
            {
                if (t == pathParent || processedLimbs.Contains(t)) continue;

                Transform root = FindCharacterRoot(t, pathParent);
                if (skippedRoots.Contains(root)) continue;
                if (IsRootBlacklisted(root)) { skippedRoots.Add(root); continue; }

                PendulumTarget pt = CreateTarget(t, root);
                if (pt != null) candidates.Add(pt);
            }
        }

        // Only keep the highest ancestor in any hierarchy chain.
        foreach (var candidate in candidates)
        {
            bool hasAncestor = candidates.Any(
                o => o != candidate && candidate.transform.IsChildOf(o.transform));

            processedLimbs.Add(candidate.transform);

            if (hasAncestor) continue; // child — skip, ancestor handles it
            if (targets.Contains(candidate)) continue;

            candidate.initialLocalPos = candidate.transform.localPosition;
            candidate.initialLocalRot = candidate.transform.localRotation;
            targets.Add(candidate);
        }
    }

    private PendulumTarget CreateTarget(Transform limb, Transform root)
    {
        string n = limb.name.ToLower();
        bool isLeft = n.Contains("l_") || n.Contains("_l") || n.Contains("left");

        foreach (var pattern in limbPatterns)
        {
            if (!pattern.nameKeywords.Any(k => n.Contains(k.ToLower()))) continue;
            if (pattern.requireRenderer && limb.GetComponent<Renderer>() == null) continue;

            bool invert = pattern.invertSwing;
            if (pattern.autoInvertLeftSide && isLeft) invert = !invert;

            return new PendulumTarget
            {
                label = $"{root.name}_{limb.name}",
                transform = limb,
                rootTransform = root,
                rotationAxis = pattern.rotationAxis,
                invertSwing = invert,
                matchedPattern = string.Join(", ", pattern.nameKeywords),
                cachedLimbLocalPivot = ComputeLimbLocalPivot(limb, root, pattern)
            };
        }
        return null;
    }

    private bool IsRootBlacklisted(Transform root)
    {
        if (root == null) return false;
        string n = root.name.ToLower();
        return blacklistedWords.Any(w => !string.IsNullOrEmpty(w) && n.Contains(w.ToLower()));
    }

    private Transform FindCharacterRoot(Transform limb, Transform container)
    {
        Transform cur = limb;
        while (cur.parent != null && cur.parent != container)
            cur = cur.parent;
        return (cur.parent == container) ? cur : limb;
    }

    public void RegisterFemaleNPC(Transform root)
    {
        if (root == null || IsRootBlacklisted(root)) return;
        RefreshAllTargets();
    }

    public void OnNPCCloned(Transform npc) => RegisterFemaleNPC(npc);

    // =========================================================================
    // GIZMOS
    // =========================================================================
    void OnDrawGizmos()
    {
        if (targets == null) return;
        foreach (var pt in targets)
        {
            if (pt.transform == null) continue;

            Vector3 pivot = GetWorldPivot(pt);
            Vector3 axis = GetWorldAxis(pt);

            Gizmos.color = pt.invertSwing ? Color.cyan : Color.magenta;
            Gizmos.DrawWireSphere(pivot, 0.05f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pt.transform.position, pivot);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(pivot, axis * 0.2f);

#if UNITY_EDITOR
            if (Selection.activeGameObject == gameObject)
            {
                Handles.Label(pivot + Vector3.up * 0.1f,
                    $"{pt.label}\n({pt.matchedPattern})\nadjust: {pt.pivotAdjust:F2}");

                // Draggable handle — drag the sphere in the Scene view to adjust pivot.
                EditorGUI.BeginChangeCheck();
                Vector3 newPivot = Handles.PositionHandle(pivot, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Adjust Pivot");
                    // Convert the dragged world position back to limb-local adjustment.
                    Vector3 newLimbLocal = pt.transform.InverseTransformPoint(newPivot);
                    pt.pivotAdjust = newLimbLocal - pt.cachedLimbLocalPivot;
                    EditorUtility.SetDirty(this);
                }
            }
#endif
        }
    }

    // =========================================================================
    // DEBUG
    // =========================================================================
    [ContextMenu("DEBUG — Log All Matched Transforms")]
    public void DebugLogMatches()
    {
        Debug.Log("=== FemaleLimbPendulumSystem: keyword scan ===");
        foreach (Transform pathParent in pathObjects)
        {
            if (pathParent == null) continue;
            foreach (Transform t in pathParent.GetComponentsInChildren<Transform>(true))
            {
                if (t == pathParent) continue;
                bool hasRend = t.GetComponent<Renderer>() != null;
                foreach (var pattern in limbPatterns)
                {
                    string n = t.name.ToLower();
                    if (!pattern.nameKeywords.Any(k => n.Contains(k.ToLower()))) continue;
                    bool skip = pattern.requireRenderer && !hasRend;
                    string state = skip ? "SKIPPED (no renderer)" : "TARGETED";
                    Debug.Log($"  [{state}] '{t.name}'  renderer={hasRend}  " +
                              $"pattern=[{string.Join(", ", pattern.nameKeywords)}]  " +
                              $"path={GetPath(t)}", t);
                }
            }
        }
        Debug.Log("=== Scan complete ===");
    }

    [ContextMenu("DEBUG — Log Pivot World Positions")]
    public void DebugLogPivots()
    {
        Debug.Log("=== Active target pivots (world space) ===");
        foreach (var pt in targets)
        {
            if (pt.transform == null) continue;
            Vector3 wp = GetWorldPivot(pt);
            Debug.Log($"  '{pt.label}'  limbPos={pt.transform.position}  " +
                      $"worldPivot={wp}  limbLocalPivot={pt.cachedLimbLocalPivot}  " +
                      $"axis={GetWorldAxis(pt)}", pt.transform);
        }
        Debug.Log("=== End ===");
    }

    // =========================================================================
    // IMPORT / EXPORT
    // =========================================================================
    public void ExportSettings(string filePath)
    {
        var data = new PendulumData { swingAngle = swingAngle, speed = speed };
        foreach (var t in targets)
            data.targets.Add(new TargetSaveData
            {
                label = t.label,
                path = GetPath(t.transform),
                rootPath = GetPath(t.rootTransform),
                limbLocalPivot = t.cachedLimbLocalPivot,
                pivotAdjust = t.pivotAdjust,
                axis = t.rotationAxis,
                inverted = t.invertSwing
            });
        File.WriteAllText(filePath, JsonUtility.ToJson(data, true));
    }

    public void ImportSettings(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var data = JsonUtility.FromJson<PendulumData>(File.ReadAllText(filePath));
        swingAngle = data.swingAngle;
        speed = data.speed;
        targets.Clear();
        processedLimbs.Clear();
        foreach (var d in data.targets)
        {
            Transform t = GameObject.Find(d.path)?.transform;
            Transform r = string.IsNullOrEmpty(d.rootPath)
                        ? null : GameObject.Find(d.rootPath)?.transform;
            if (t == null) continue;
            targets.Add(new PendulumTarget
            {
                label = d.label,
                transform = t,
                rootTransform = r,
                cachedLimbLocalPivot = d.limbLocalPivot,
                pivotAdjust = d.pivotAdjust,
                rotationAxis = d.axis,
                invertSwing = d.inverted
            });
            processedLimbs.Add(t);
        }
    }

    private string GetPath(Transform t)
    {
        if (t == null) return "";
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    [ContextMenu("Clear All Targets")]
    public void ClearTargets()
    {
        targets.Clear();
        processedLimbs.Clear();
    }
}

// =============================================================================
// CUSTOM EDITOR
// =============================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(FemaleLimbPendulumSystem))]
public class FemaleLimbEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var s = (FemaleLimbPendulumSystem)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "If Limb Patterns shows stale keywords like \"arm, leg\" combined in one entry, " +
            "click RESET PATTERNS TO DEFAULTS — Unity caches old serialized data even after " +
            "the script changes.", MessageType.Warning);

        if (GUILayout.Button("RESET PATTERNS TO DEFAULTS", GUILayout.Height(28)))
        {
            Undo.RecordObject(s, "Reset Limb Patterns");
            s.limbPatterns = new List<FemaleLimbPendulumSystem.LimbPattern>
            {
                new FemaleLimbPendulumSystem.LimbPattern {
                    nameKeywords = new string[] { "arm" },
                    rotationAxis = Vector3.right,
                    invertSwing  = false,
                    autoInvertLeftSide = true,
                    pivotMode    = FemaleLimbPendulumSystem.PivotMode.RendererBoundsClosestToRoot,
                    requireRenderer = true
                },
                new FemaleLimbPendulumSystem.LimbPattern {
                    nameKeywords = new string[] { "leg" },
                    rotationAxis = Vector3.right,
                    invertSwing  = true,
                    autoInvertLeftSide = true,
                    pivotMode    = FemaleLimbPendulumSystem.PivotMode.RendererBoundsClosestToRoot,
                    requireRenderer = true
                }
            };
            EditorUtility.SetDirty(s);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("REFRESH TARGETS", GUILayout.Height(30)))
        {
            Undo.RecordObject(s, "Refresh Targets");
            s.ClearTargets();
            s.RefreshAllTargets();
            EditorUtility.SetDirty(s);
        }
        if (GUILayout.Button("CLEAR ALL TARGETS", GUILayout.Height(25)))
        {
            Undo.RecordObject(s, "Clear Targets");
            s.ClearTargets();
            EditorUtility.SetDirty(s);
        }

        if (GUILayout.Button("RESET PIVOT ADJUSTMENTS", GUILayout.Height(25)))
        {
            Undo.RecordObject(s, "Reset Pivot Adjustments");
            foreach (var t in s.targets) t.pivotAdjust = Vector3.zero;
            EditorUtility.SetDirty(s);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Active Targets: {s.targets.Count}", EditorStyles.helpBox);

        EditorGUILayout.Space();
        if (GUILayout.Button("DEBUG — Log Matched Transforms", GUILayout.Height(25)))
            s.DebugLogMatches();

        if (GUILayout.Button("DEBUG — Log Pivot World Positions", GUILayout.Height(25)))
            s.DebugLogPivots();

        EditorGUILayout.Space();
        if (GUILayout.Button("EXPORT JSON", GUILayout.Height(30)))
        {
            string path = EditorUtility.SaveFilePanel("Save", "", "FemaleLimbSettings", "json");
            if (!string.IsNullOrEmpty(path)) s.ExportSettings(path);
        }
        if (GUILayout.Button("IMPORT JSON", GUILayout.Height(30)))
        {
            string path = EditorUtility.OpenFilePanel("Load", "", "json");
            if (!string.IsNullOrEmpty(path)) s.ImportSettings(path);
        }
    }
}
#endif