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

    private Renderer cachedRenderer;
    private bool lastRendererState;

    void Start()
    {
        if (targetObject == null) targetObject = gameObject;

        if (watchObject != null)
        {
            cachedRenderer = watchObject.GetComponentInChildren<Renderer>();

            if (cachedRenderer != null)
            {
                lastRendererState = cachedRenderer.enabled;

                // FIX: Instantly apply the correct state on Start so they are synced
                bool desiredTargetState = invertLogic ? lastRendererState : !lastRendererState;
                targetObject.SetActive(desiredTargetState);
            }
            else
            {
                Debug.LogWarning($"GameObjectToggle: No MeshRenderer or SkinnedMeshRenderer found on '{watchObject.name}' or its children!", watchObject);
            }
        }
    }

    void Update()
    {
        if (cachedRenderer == null || targetObject == null) return;

        bool currentRendererState = cachedRenderer.enabled;

        if (currentRendererState != lastRendererState)
        {
            lastRendererState = currentRendererState;
            bool desiredTargetState = invertLogic ? currentRendererState : !currentRendererState;
            targetObject.SetActive(desiredTargetState);
        }
    }

    public GameObject CurrentTarget => targetObject;
    public GameObject CurrentWatch => watchObject;
    public bool IsRendererEnabled => cachedRenderer != null && cachedRenderer.enabled;
    public bool IsTargetActive => targetObject != null && targetObject.activeInHierarchy;
    public bool RendererFound => cachedRenderer != null;
}

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

        Repaint();
    }
}
#endif