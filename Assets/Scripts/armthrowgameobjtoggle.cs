using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameObjectToggle : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The GameObject to toggle (leave empty to toggle this object)")]
    [SerializeField] private GameObject targetObject;

    [Header("Watch Settings")]
    [Tooltip("The GameObject that holds the Mesh Renderer to watch")]
    [SerializeField] private GameObject watchObject;
    [Tooltip("Reverse the logic (Target checks when Renderer is enabled, unchecks when disabled)")]
    [SerializeField] private bool invertLogic = false;

    // We use the base 'Renderer' class so it automatically supports 
    // both MeshRenderer and SkinnedMeshRenderer (used by characters)
    private Renderer cachedRenderer;
    private bool lastRendererState;

    void Start()
    {
        // If no target specified, use this object
        if (targetObject == null)
        {
            targetObject = gameObject;
        }

        // Scan the watched object (and its children) for a renderer
        if (watchObject != null)
        {
            cachedRenderer = watchObject.GetComponentInChildren<Renderer>();

            if (cachedRenderer != null)
            {
                // Sync the initial state so it doesn't trigger a false toggle on frame 1
                lastRendererState = cachedRenderer.enabled;
            }
            else
            {
                Debug.LogWarning($"GameObjectToggle: No MeshRenderer or SkinnedMeshRenderer found on '{watchObject.name}' or its children!", watchObject);
            }
        }
    }

    void Update()
    {
        // Do nothing if we didn't find a renderer or a target
        if (cachedRenderer == null || targetObject == null) return;

        // Check if the renderer's enabled checkbox has changed
        bool currentRendererState = cachedRenderer.enabled;

        if (currentRendererState != lastRendererState)
        {
            // Update our tracked state
            lastRendererState = currentRendererState;

            // Apply inversion if needed
            bool desiredTargetState = invertLogic ? currentRendererState : !currentRendererState;

            // Apply the state to the target GameObject
            targetObject.SetActive(desiredTargetState);
        }
    }

    // Status properties for the custom inspector
    public GameObject CurrentTarget => targetObject;
    public GameObject CurrentWatch => watchObject;
    public bool IsRendererEnabled => cachedRenderer != null && cachedRenderer.enabled;
    public bool IsTargetActive => targetObject != null && targetObject.activeInHierarchy;
    public bool RendererFound => cachedRenderer != null;
}

// Custom Inspector
#if UNITY_EDITOR
[CustomEditor(typeof(GameObjectToggle))]
public class GameObjectToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameObjectToggle script = (GameObjectToggle)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Status Info", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"Watch Object: {(script.CurrentWatch != null ? script.CurrentWatch.name : "None")}");

            if (script.RendererFound)
            {
                EditorGUILayout.LabelField($"Renderer Enabled: {script.IsRendererEnabled}");
            }
            else
            {
                EditorGUILayout.HelpBox("No Renderer found on Watch Object!", MessageType.Error);
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField($"Target Object: {(script.CurrentTarget != null ? script.CurrentTarget.name : "None")}");
            EditorGUILayout.LabelField($"Target Active: {script.IsTargetActive}");
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see status", MessageType.Info);
        }

        // Force inspector to repaint so status updates live
        Repaint();
    }
}
#endif