using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BikeHandleControllerFixed : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main transform of the bike body. Auto-finds Root if empty.")]
    public Transform bikeBodyTransform;

    [Tooltip("The handlebar mesh/pivot that will rotate visually. Auto-finds Self if empty.")]
    public Transform handlebarPivotTransform;

    [Header("Followers")]
    [Tooltip("Other meshes/objects that should rotate exactly with the handlebar (e.g. brake levers, separate wheel mesh).")]
    public Transform[] additionalFollowers;

    [Header("Axis Configuration")]
    [Tooltip("The local axis around which the handlebar turns. Default is Up (0,1,0). Change X, Y, Z to rotate the pivot axis.")]
    public Vector3 localRotationAxis = Vector3.up;

    [Header("Orientation Fix")]
    [Tooltip("Enable this if your bike model is facing backwards (Blue arrow points behind the bike).")]
    public bool isInvertedModel = false;

    [Header("Steering Settings")]
    [Tooltip("How much the handlebar turns relative to the bike's rotation speed.")]
    public float turnSensitivity = 0.2f;

    [Tooltip("Maximum angle (in degrees) the handlebar can turn left or right.")]
    public float maxTurnAngle = 45f;

    [Tooltip("How smoothly the handlebar reacts to rotation.")]
    public float lerpSpeed = 8f;

    [Header("Debug Visualization")]
    [Tooltip("Size of the gizmo arrows drawn in the Scene view.")]
    public float gizmoSize = 0.5f;

    // Internal state variables
    private float previousYRotation;
    private Vector3 previousPosition;
    private float currentTurnAngle;

    // Store initial rotations to preserve model orientation
    private Quaternion initialHandleRotation;
    private Quaternion[] initialFollowerRotations; // Array to store follower start rotations

    private bool isFirstFrame = true;

    void Start()
    {
        // --- AUTO-REFERENCE LOGIC ---

        // 1. Find Bike Body: If not assigned, assume the root of the hierarchy is the bike.
        if (bikeBodyTransform == null)
        {
            bikeBodyTransform = transform.root;
        }

        // 2. Find Handlebar: If not assigned, assume this script is on the handlebar.
        if (handlebarPivotTransform == null)
        {
            handlebarPivotTransform = this.transform;
        }

        // Initialize previous values
        previousYRotation = bikeBodyTransform.eulerAngles.y;
        previousPosition = bikeBodyTransform.position;

        // --- PRESERVE ORIGINAL ORIENTATION ---

        // Store main handlebar rotation
        initialHandleRotation = handlebarPivotTransform.localRotation;

        // Store follower rotations
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

        // Skip first frame to avoid initial rotation spikes
        if (isFirstFrame)
        {
            isFirstFrame = false;
            previousYRotation = bikeBodyTransform.eulerAngles.y;
            previousPosition = bikeBodyTransform.position;
            return;
        }

        // --- 1. Calculate Bike Rotation Speed (Yaw) ---
        float currentYRotation = bikeBodyTransform.eulerAngles.y;
        float deltaAngle = Mathf.DeltaAngle(previousYRotation, currentYRotation);
        float rotationSpeed = deltaAngle / Time.deltaTime;
        previousYRotation = currentYRotation;

        // --- 2. Calculate Forward Speed ---
        Vector3 displacement = bikeBodyTransform.position - previousPosition;
        float forwardSpeed = Vector3.Dot(displacement, bikeBodyTransform.forward) / Time.deltaTime;
        previousPosition = bikeBodyTransform.position;

        // --- ORIENTATION FIX ---
        if (isInvertedModel)
        {
            forwardSpeed *= -1f;
        }

        // --- 3. Calculate Target Steering Angle ---
        float targetAngle = rotationSpeed * turnSensitivity;

        // --- REVERSE LOGIC ---
        if (forwardSpeed < -0.1f)
        {
            targetAngle *= -1f;
        }

        targetAngle = Mathf.Clamp(targetAngle, -maxTurnAngle, maxTurnAngle);

        // --- 4. Apply Smooth Rotation ---
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetAngle, Time.deltaTime * lerpSpeed);

        // --- APPLY ROTATION WITH CUSTOM AXIS ---
        // We create a rotation around the user-defined axis
        Quaternion steerRotation = Quaternion.AngleAxis(currentTurnAngle, localRotationAxis.normalized);

        // Apply to main handlebar
        handlebarPivotTransform.localRotation = initialHandleRotation * steerRotation;

        // Apply to followers
        if (additionalFollowers != null && initialFollowerRotations != null)
        {
            for (int i = 0; i < additionalFollowers.Length; i++)
            {
                // Only apply if the reference is valid and we have a stored initial rotation
                if (additionalFollowers[i] != null)
                {
                    additionalFollowers[i].localRotation = initialFollowerRotations[i] * steerRotation;
                }
            }
        }
    }

    // --- GIZMOS LOGIC ---
    void OnDrawGizmos()
    {
        if (handlebarPivotTransform == null) return;

        // Ensure axis is normalized for drawing
        Vector3 axis = localRotationAxis.normalized;

        // 1. Draw the Custom Rotation Axis - GREEN
        // This converts the local axis vector into a world direction for drawing
        Gizmos.color = Color.green;
        Vector3 worldAxisDirection = handlebarPivotTransform.TransformDirection(axis);
        Gizmos.DrawRay(handlebarPivotTransform.position, worldAxisDirection * gizmoSize);

        // Draw a small sphere at the tip
        Vector3 tip = handlebarPivotTransform.position + (worldAxisDirection * gizmoSize);
        Gizmos.DrawSphere(tip, 0.02f);

        // 2. Draw the Forward Direction - BLUE
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(handlebarPivotTransform.position, handlebarPivotTransform.forward * gizmoSize);

        // 3. Draw the Rotation Plane Disc (Editor Only)
#if UNITY_EDITOR
        Handles.color = new Color(0f, 1f, 0f, 0.2f); // Semi-transparent green
        // The disc is drawn perpendicular to the rotation axis
        Handles.DrawWireDisc(handlebarPivotTransform.position, worldAxisDirection, gizmoSize * 0.5f);
#endif
    }
}