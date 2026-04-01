using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameObjectToggle : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The GameObject to toggle (leave empty to toggle this object)")]
    [SerializeField] private GameObject targetObject;

    [Header("Timing Settings")]
    [Tooltip("Time in seconds between toggles")]
    [SerializeField] private float toggleInterval = 2f;
    [Tooltip("Randomize interval by +/- this amount")]
    [SerializeField] private float randomVariance = 0f;
    [Tooltip("Start with target disabled")]
    [SerializeField] private bool startDisabled = false;

    [Header("Options")]
    [Tooltip("Begin toggling automatically on Start")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Stop after this many toggles (0 = infinite)")]
    [SerializeField] private int maxToggleCount = 0;

    private float nextToggleTime;
    private int toggleCount;
    private bool isWaiting;

    // Used to safely cancel the background timer when the object is destroyed
    private CancellationTokenSource cancellationTokenSource;

    void Start()
    {
        // If no target specified, use this object
        if (targetObject == null)
        {
            targetObject = gameObject;
        }

        if (startDisabled)
        {
            targetObject.SetActive(false);
        }

        if (autoStart)
        {
            StartToggling();
        }
    }

    public async void StartToggling()
    {
        if (isWaiting) return; // Prevent multiple loops running at once

        isWaiting = true;
        toggleCount = 0;

        // Reset cancellation token
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            // Start the asynchronous loop
            await ToggleLoop(cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // This is expected when the loop is stopped or the object is destroyed
        }
    }

    private async Task ToggleLoop(CancellationToken token)
    {
        while (isWaiting && !token.IsCancellationRequested)
        {
            // Calculate the interval
            float interval = toggleInterval + Random.Range(-randomVariance, randomVariance);
            interval = Mathf.Max(0.01f, interval);

            // Use realtime since Time.time stops updating for disabled objects
            nextToggleTime = Time.realtimeSinceStartup + interval;

            // Wait asynchronously (this does not stop when the GameObject is disabled!)
            await Task.Delay(Mathf.RoundToInt(interval * 1000), token);

            // Double check cancellation before executing toggle
            if (token.IsCancellationRequested || !isWaiting || targetObject == null) break;

            // Toggle target state
            targetObject.SetActive(!targetObject.activeSelf);
            toggleCount++;

            // Check max count
            if (maxToggleCount > 0 && toggleCount >= maxToggleCount)
            {
                isWaiting = false;
                break;
            }
        }
    }

    public void StopToggling()
    {
        isWaiting = false;
        cancellationTokenSource?.Cancel();
    }

    public void ResetToggle()
    {
        StopToggling();
        toggleCount = 0;
    }

    // Clean up the background task if this script/object gets deleted
    private void OnDestroy()
    {
        StopToggling();
        cancellationTokenSource?.Dispose();
    }

    // Status
    public bool IsRunning => isWaiting;
    public int CurrentToggleCount => toggleCount;
    public float TimeUntilNextToggle => isWaiting ? Mathf.Max(0, nextToggleTime - Time.realtimeSinceStartup) : 0f;
    public GameObject CurrentTarget => targetObject;
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
            EditorGUILayout.LabelField($"Running: {script.IsRunning}");
            EditorGUILayout.LabelField($"Toggle Count: {script.CurrentToggleCount}");
            EditorGUILayout.LabelField($"Target: {(script.CurrentTarget != null ? script.CurrentTarget.name : "None")}");
            if (script.IsRunning)
            {
                EditorGUILayout.LabelField($"Time Until Next: {script.TimeUntilNextToggle:F2}s");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see status", MessageType.Info);
        }

        // Force inspector to repaint so the countdown timer updates smoothly
        Repaint();
    }
}
#endif