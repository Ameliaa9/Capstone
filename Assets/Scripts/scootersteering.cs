using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BikeHandleControllerFixed : MonoBehaviour
{
    [Header("Pause Settings")]
    [Tooltip("If true, handlebar freezes when game is paused.")]
    public bool freezeOnPause = true;

    [Header("References")]
    [Tooltip("The main transform of the bike body. Auto-finds Root if empty.")]
    public Transform bikeBodyTransform;

    [Tooltip("The handlebar mesh/pivot that will rotate visually. Auto-finds Self if empty.")]
    public Transform handlebarPivotTransform;

    [Header("Followers")]
    [Tooltip("Other meshes/objects that should rotate exactly with the handlebar.")]
    public Transform[] additionalFollowers;

    [Header("Axis Configuration")]
    [Tooltip("The local axis around which the handlebar turns.")]
    public Vector3 localRotationAxis = Vector3.up;

    [Header("Orientation Fix")]
    [Tooltip("Enable if your bike model faces backwards.")]
    public bool isInvertedModel = false;

    [Header("Steering Settings")]
    public float turnSensitivity = 0.2f;
    public float maxTurnAngle = 45f;
    public float lerpSpeed = 8f;

    [Header("Debug Visualization")]
    public float gizmoSize = 0.5f;

    private float previousYRotation;
    private Vector3 previousPosition;
    private float currentTurnAngle;

    private Quaternion initialHandleRotation;
    private Quaternion[] initialFollowerRotations;

    private bool isFirstFrame = true;

    void Start()
    {
        if (bikeBodyTransform == null) bikeBodyTransform = transform.root;
        if (handlebarPivotTransform == null) handlebarPivotTransform = this.transform;

        previousYRotation = bikeBodyTransform.eulerAngles.y;
        previousPosition = bikeBodyTransform.position;

        initialHandleRotation = handlebarPivotTransform.localRotation;

        if (additionalFollowers != null && additionalFollowers.Length > 0)
        {
            initialFollowerRotations = new Quaternion[additionalFollowers.Length];
            for (int i = 0; i < additionalFollowers.Length; i++)
            {
                if (additionalFollowers[i] != null)
                {
                    initialFollowerRotations[i] = additionalFollowers[i].localRotation;
                }
            }
        }
    }

    void Update()
    {
        if (bikeBodyTransform == null || handlebarPivotTransform == null) return;

        // FIX: Skip during pause if configured
        if (freezeOnPause && Time.timeScale == 0f) return;

        // Skip first frame
        if (isFirstFrame)
        {
            isFirstFrame = false;
            previousYRotation = bikeBodyTransform.eulerAngles.y;
            previousPosition = bikeBodyTransform.position;
            return;
        }

        // FIX: Use unscaledDeltaTime to prevent division by zero
        float delta = Time.unscaledDeltaTime;
        if (delta < 0.0001f) delta = 0.0001f;

        // Calculate rotation speed using safe delta
        float currentYRotation = bikeBodyTransform.eulerAngles.y;
        float deltaAngle = Mathf.DeltaAngle(previousYRotation, currentYRotation);
        float rotationSpeed = deltaAngle / delta;
        previousYRotation = currentYRotation;

        // Calculate forward speed using safe delta
        Vector3 displacement = bikeBodyTransform.position - previousPosition;
        float forwardSpeed = Vector3.Dot(displacement, bikeBodyTransform.forward) / delta;
        previousPosition = bikeBodyTransform.position;

        // Orientation fix
        if (isInvertedModel) forwardSpeed *= -1f;

        // Calculate target angle
        float targetAngle = rotationSpeed * turnSensitivity;

        // Reverse logic
        if (forwardSpeed < -0.1f) targetAngle *= -1f;

        targetAngle = Mathf.Clamp(targetAngle, -maxTurnAngle, maxTurnAngle);

        // Apply smooth rotation using unscaled time
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetAngle, delta * lerpSpeed);

        // Apply rotation
        Quaternion steerRotation = Quaternion.AngleAxis(currentTurnAngle, localRotationAxis.normalized);
        handlebarPivotTransform.localRotation = initialHandleRotation * steerRotation;

        if (additionalFollowers != null && initialFollowerRotations != null)
        {
            for (int i = 0; i < additionalFollowers.Length; i++)
            {
                if (additionalFollowers[i] != null)
                {
                    additionalFollowers[i].localRotation = initialFollowerRotations[i] * steerRotation;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (handlebarPivotTransform == null) return;

        Vector3 axis = localRotationAxis.normalized;

        Gizmos.color = Color.green;
        Vector3 worldAxisDirection = handlebarPivotTransform.TransformDirection(axis);
        Gizmos.DrawRay(handlebarPivotTransform.position, worldAxisDirection * gizmoSize);

        Vector3 tip = handlebarPivotTransform.position + (worldAxisDirection * gizmoSize);
        Gizmos.DrawSphere(tip, 0.02f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(handlebarPivotTransform.position, handlebarPivotTransform.forward * gizmoSize);

#if UNITY_EDITOR
        Handles.color = new Color(0f, 1f, 0f, 0.2f);
        Handles.DrawWireDisc(handlebarPivotTransform.position, worldAxisDirection, gizmoSize * 0.5f);
#endif
    }
}