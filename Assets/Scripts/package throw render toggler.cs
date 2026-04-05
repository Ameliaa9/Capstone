using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RendererVisibilityToggle : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The GameObject whose Mesh/Skinned Renderer will become invisible/visible")]
    [SerializeField] private GameObject targetObject;

    [Header("Watch Settings")]
    [Tooltip("The GameObject that holds the Mesh Renderer to watch")]
    [SerializeField] private GameObject watchObject;
    [Tooltip("Reverse the logic (Target shows when Watch Renderer is enabled, hides when disabled)")]
    [SerializeField] private bool invertLogic = false;

    private Renderer watchRenderer;
    private Renderer targetRenderer;
    private bool lastWatchState;

    void Start()
    {
        if (watchObject != null)
        {
            watchRenderer = watchObject.GetComponentInChildren<Renderer>();

            if (watchRenderer != null)
            {
                lastWatchState = watchRenderer.enabled;
            }
            else
            {
                Debug.LogWarning($"RendererVisibilityToggle: No Renderer found on '{watchObject.name}'!", watchObject);
            }
        }

        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponentInChildren<Renderer>();

            if (targetRenderer == null)
            {
                Debug.LogWarning($"RendererVisibilityToggle: No Renderer found on Target '{targetObject.name}'!", targetObject);
            }
        }

        // FIX: Instantly apply the correct visibility state on Start so they are synced
        if (watchRenderer != null && targetRenderer != null)
        {
            bool desiredTargetState = invertLogic ? lastWatchState : !lastWatchState;
            targetRenderer.enabled = desiredTargetState;
        }
    }

    void Update()
    {
        if (watchRenderer == null || targetRenderer == null) return;

        bool currentWatchState = watchRenderer.enabled;

        if (currentWatchState != lastWatchState)
        {
            lastWatchState = currentWatchState;
            bool desiredTargetState = invertLogic ? currentWatchState : !currentWatchState;
            targetRenderer.enabled = desiredTargetState;
        }
    }

    public GameObject CurrentTarget => targetObject;
    public GameObject CurrentWatch => watchObject;
    public bool IsWatchRendererEnabled => watchRenderer != null && watchRenderer.enabled;
    public bool IsTargetRendererEnabled => targetRenderer != null && targetRenderer.enabled;
    public bool RenderersFound => watchRenderer != null && targetRenderer != null;
}

#if UNITY_EDITOR
[CustomEditor(typeof(RendererVisibilityToggle))]
public class RendererVisibilityToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RendererVisibilityToggle script = (RendererVisibilityToggle)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Status Info", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"Watch Object: {(script.CurrentWatch != null ? script.CurrentWatch.name : "None")}");

            if (script.RenderersFound)
            {
                EditorGUILayout.LabelField($"Watch Renderer Enabled: {script.IsWatchRendererEnabled}");
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Target Object: {(script.CurrentTarget != null ? script.CurrentTarget.name : "None")}");
                EditorGUILayout.LabelField($"Target Renderer Enabled: {script.IsTargetRendererEnabled}");
            }
            else
            {
                EditorGUILayout.HelpBox("Missing Renderer on Watch Object, Target Object, or both!", MessageType.Error);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see status", MessageType.Info);
        }

        Repaint();
    }
}
#endif