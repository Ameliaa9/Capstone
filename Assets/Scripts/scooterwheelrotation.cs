using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Rotates scooter wheels based on movement of a reference object.
/// 
/// SETUP CHECKLIST:
/// 1. Assign Movement Reference (the object that moves)
/// 2. Assign Front Wheel and Rear Wheel transforms
/// 3. Set Wheel Rotation Axis (usually X)
/// 4. Adjust Wheel Radius to match your wheel size
/// 5. Use Forward Direction Offset if arrow points wrong way
/// 
/// TROUBLESHOOTING "IsFinite" ERRORS:
/// - Check that all GameObjects have POSITIVE scale (not 0 or negative)
/// - Ensure Wheel Radius is not 0 or extremely small
/// - Make sure wheels and movement reference are not at position (Infinity, Infinity, Infinity)
/// - Verify the movement reference object exists and is active
/// </summary>
public class ScooterWheelRotation : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }
    public enum ForwardDirection { X, Y, Z, NegativeX, NegativeY, NegativeZ }

    [Header("Movement Reference")]
    [Tooltip("The GameObject to track for movement (e.g., scooter body, player controller)")]
    public Transform movementReference;

    [Tooltip("Which direction is 'forward' for movement tracking")]
    public ForwardDirection movementForwardDirection = ForwardDirection.Z;

    [Header("Wheel References")]
    [Tooltip("Assign the front wheel GameObject")]
    public Transform frontWheel;

    [Tooltip("Assign the rear wheel GameObject")]
    public Transform rearWheel;

    [Header("Rotation Settings")]
    [Tooltip("Which axis the wheels rotate around (X = side to side, Y = up/down, Z = front to back)")]
    public RotationAxis wheelRotationAxis = RotationAxis.X;

    [Tooltip("Reverse the wheel rotation direction (check if wheels spin backwards)")]
    public bool invertRotation = false;

    [Tooltip("Rotate which direction is considered 'forward' (in degrees, rotates around Y axis)")]
    [Range(-180f, 180f)]
    public float forwardDirectionOffset = 0f;

    [Tooltip("How fast the wheels rotate (degrees per unit of movement)")]
    public float rotationSpeed = 360f;

    [Tooltip("Wheel radius in units (used for realistic rotation)")]
    public float wheelRadius = 0.3f;

    [Header("Debug Visualization")]
    [Tooltip("Show an arrow indicating the direction that makes wheels spin forward")]
    public bool showDirectionArrow = false;  // CHANGED TO FALSE - Enable this only after confirming no errors

    [Tooltip("Length of the direction arrow")]
    public float arrowLength = 2f;

    [Tooltip("Color of the direction arrow")]
    public Color arrowColor = Color.green;

    private Vector3 lastPosition;
    private float currentRotation = 0f;
    private Quaternion frontWheelInitialRotation;
    private Quaternion rearWheelInitialRotation;

    void Start()
    {
        Debug.Log("=== ScooterWheelRotation START ===");

        // If no movement reference is assigned, use this GameObject's transform
        if (movementReference == null)
        {
            movementReference = transform;
            Debug.Log("ScooterWheelRotation: No movement reference assigned, using this GameObject's transform.");
        }

        Debug.Log($"Movement Reference: {movementReference.name}");
        Debug.Log($"Movement Reference Position: {movementReference.position}");
        Debug.Log($"Movement Reference Scale: {movementReference.localScale}");
        Debug.Log($"Wheel Radius: {wheelRadius}");

        // Validate wheel radius
        if (wheelRadius <= 0.001f)
        {
            wheelRadius = 0.3f;
            Debug.LogWarning("ScooterWheelRotation: Wheel radius was too small, set to default 0.3");
        }

        // Validate transform
        if (!IsValidTransform(movementReference))
        {
            Debug.LogError($"ScooterWheelRotation: Movement reference '{movementReference.name}' has invalid transform!");
            Debug.LogError($"  - Position: {movementReference.position}");
            Debug.LogError($"  - Scale: {movementReference.localScale}");
            Debug.LogError($"  - Rotation: {movementReference.rotation}");
            enabled = false;
            return;
        }

        // Store the starting position
        lastPosition = movementReference.position;

        // Validate position
        if (!IsValidVector(lastPosition))
        {
            Debug.LogError($"ScooterWheelRotation: Movement reference has invalid position: {lastPosition}");
            enabled = false;
            return;
        }

        // Store initial wheel rotations to preserve their starting orientation
        if (frontWheel != null)
        {
            Debug.Log($"Front Wheel: {frontWheel.name}");
            Debug.Log($"Front Wheel Position: {frontWheel.position}");
            Debug.Log($"Front Wheel Local Scale: {frontWheel.localScale}");
            Debug.Log($"Front Wheel Rotation: {frontWheel.localRotation}");

            if (IsValidTransform(frontWheel))
            {
                frontWheelInitialRotation = frontWheel.localRotation;

                // Validate stored quaternion
                if (float.IsNaN(frontWheelInitialRotation.x) || float.IsNaN(frontWheelInitialRotation.y) ||
                    float.IsNaN(frontWheelInitialRotation.z) || float.IsNaN(frontWheelInitialRotation.w))
                {
                    Debug.LogError("Front wheel initial rotation is invalid! Using identity.");
                    frontWheelInitialRotation = Quaternion.identity;
                }

                Debug.Log($"Front wheel initial rotation stored: {frontWheelInitialRotation}");
            }
            else
            {
                Debug.LogError($"ScooterWheelRotation: Front wheel '{frontWheel.name}' has invalid transform!");
                Debug.LogError($"  - Position: {frontWheel.position}");
                Debug.LogError($"  - Scale: {frontWheel.localScale}");
                frontWheel = null;
            }
        }
        else
        {
            Debug.LogWarning("Front wheel not assigned!");
        }

        if (rearWheel != null)
        {
            Debug.Log($"Rear Wheel: {rearWheel.name}");
            Debug.Log($"Rear Wheel Position: {rearWheel.position}");
            Debug.Log($"Rear Wheel Local Scale: {rearWheel.localScale}");
            Debug.Log($"Rear Wheel Rotation: {rearWheel.localRotation}");

            if (IsValidTransform(rearWheel))
            {
                rearWheelInitialRotation = rearWheel.localRotation;

                // Validate stored quaternion
                if (float.IsNaN(rearWheelInitialRotation.x) || float.IsNaN(rearWheelInitialRotation.y) ||
                    float.IsNaN(rearWheelInitialRotation.z) || float.IsNaN(rearWheelInitialRotation.w))
                {
                    Debug.LogError("Rear wheel initial rotation is invalid! Using identity.");
                    rearWheelInitialRotation = Quaternion.identity;
                }

                Debug.Log($"Rear wheel initial rotation stored: {rearWheelInitialRotation}");
            }
            else
            {
                Debug.LogError($"ScooterWheelRotation: Rear wheel '{rearWheel.name}' has invalid transform!");
                Debug.LogError($"  - Position: {rearWheel.position}");
                Debug.LogError($"  - Scale: {rearWheel.localScale}");
                rearWheel = null;
            }
        }
        else
        {
            Debug.LogWarning("Rear wheel not assigned!");
        }

        // Validate wheel references
        if (frontWheel == null && rearWheel == null)
        {
            Debug.LogError("ScooterWheelRotation: No valid wheels assigned!");
            enabled = false;
        }

        Debug.Log("=== ScooterWheelRotation START COMPLETE ===");
    }

    bool IsValidVector(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
               !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }

    bool IsValidTransform(Transform t)
    {
        if (t == null) return false;

        Vector3 scale = t.localScale;
        bool scaleValid = scale.x > 0.001f && scale.y > 0.001f && scale.z > 0.001f;
        bool posValid = IsValidVector(t.position);
        bool rotValid = !float.IsNaN(t.rotation.x) && !float.IsNaN(t.rotation.y) &&
                        !float.IsNaN(t.rotation.z) && !float.IsNaN(t.rotation.w);

        // Also check parent chain
        if (t.parent != null)
        {
            Vector3 parentScale = t.parent.lossyScale;
            if (parentScale.x < 0.001f || parentScale.y < 0.001f || parentScale.z < 0.001f)
            {
                Debug.LogError($"Parent of '{t.name}' has invalid scale: {parentScale}");
                return false;
            }
        }

        return scaleValid && posValid && rotValid;
    }

    void Update()
    {
        RotateWheelsBasedOnMovement();
    }

    void RotateWheelsBasedOnMovement()
    {
        // Safety check: validate movement reference
        if (movementReference == null || !IsValidTransform(movementReference))
        {
            return;
        }

        // Calculate the distance moved this frame
        Vector3 currentPosition = movementReference.position;

        // Validate current position
        if (!IsValidVector(currentPosition))
        {
            Debug.LogError("ScooterWheelRotation: Invalid position detected, stopping rotation");
            enabled = false;
            return;
        }

        Vector3 movement = currentPosition - lastPosition;

        // Safety check: if movement is invalid or too large (teleport), skip this frame
        if (!IsValidVector(movement) || movement.sqrMagnitude > 1000f)
        {
            lastPosition = currentPosition;
            return;
        }

        // Project movement onto the correct forward direction to get forward/backward distance
        Vector3 actualForward = GetMovementForward();

        // Validate forward direction
        if (!IsValidVector(actualForward) || actualForward.sqrMagnitude < 0.01f)
        {
            lastPosition = currentPosition;
            return;
        }

        float distanceMoved = Vector3.Dot(movement, actualForward);

        // Validate distance
        if (float.IsNaN(distanceMoved) || float.IsInfinity(distanceMoved))
        {
            lastPosition = currentPosition;
            return;
        }

        // Calculate rotation based on wheel circumference
        float circumference = 2f * Mathf.PI * Mathf.Max(0.01f, wheelRadius);
        float rotationAmount = (distanceMoved / circumference) * 360f;

        // Validate rotation amount
        if (float.IsNaN(rotationAmount) || float.IsInfinity(rotationAmount))
        {
            lastPosition = currentPosition;
            return;
        }

        // Apply invert if enabled
        if (invertRotation)
        {
            rotationAmount = -rotationAmount;
        }

        // Clamp rotation amount to prevent extreme values
        rotationAmount = Mathf.Clamp(rotationAmount, -3600f, 3600f);

        // Accumulate rotation
        currentRotation += rotationAmount;

        // Prevent rotation from becoming too large (wrap it)
        if (Mathf.Abs(currentRotation) > 360000f)
        {
            currentRotation = currentRotation % 360f;
        }

        // Create rotation quaternion based on selected axis
        Quaternion spinRotation = GetSpinRotation(currentRotation);

        // Validate quaternion
        if (float.IsNaN(spinRotation.x) || float.IsNaN(spinRotation.y) ||
            float.IsNaN(spinRotation.z) || float.IsNaN(spinRotation.w))
        {
            Debug.LogError($"ScooterWheelRotation: Invalid spin rotation created! currentRotation={currentRotation}");
            currentRotation = 0f;
            return;
        }

        // Apply rotation to both wheels, preserving their initial orientation
        if (frontWheel != null && IsValidTransform(frontWheel))
        {
            Quaternion newRotation = frontWheelInitialRotation * spinRotation;

            // Validate the result
            if (float.IsNaN(newRotation.x) || float.IsNaN(newRotation.y) ||
                float.IsNaN(newRotation.z) || float.IsNaN(newRotation.w))
            {
                Debug.LogError($"ScooterWheelRotation: Front wheel rotation became invalid!");
                Debug.LogError($"  Initial: {frontWheelInitialRotation}, Spin: {spinRotation}");
                currentRotation = 0f;
                return;
            }

            frontWheel.localRotation = newRotation;
        }

        if (rearWheel != null && IsValidTransform(rearWheel))
        {
            Quaternion newRotation = rearWheelInitialRotation * spinRotation;

            // Validate the result
            if (float.IsNaN(newRotation.x) || float.IsNaN(newRotation.y) ||
                float.IsNaN(newRotation.z) || float.IsNaN(newRotation.w))
            {
                Debug.LogError($"ScooterWheelRotation: Rear wheel rotation became invalid!");
                Debug.LogError($"  Initial: {rearWheelInitialRotation}, Spin: {spinRotation}");
                currentRotation = 0f;
                return;
            }

            rearWheel.localRotation = newRotation;
        }

        // Update last position for next frame
        lastPosition = currentPosition;
    }

    // Helper method to get spin rotation quaternion based on selected axis
    Quaternion GetSpinRotation(float rotation)
    {
        switch (wheelRotationAxis)
        {
            case RotationAxis.X:
                return Quaternion.Euler(rotation, 0f, 0f);
            case RotationAxis.Y:
                return Quaternion.Euler(0f, rotation, 0f);
            case RotationAxis.Z:
                return Quaternion.Euler(0f, 0f, rotation);
            default:
                return Quaternion.Euler(rotation, 0f, 0f);
        }
    }

    // Helper method to get the correct forward direction based on selection
    Vector3 GetMovementForward()
    {
        if (movementReference == null)
        {
            return transform.forward;
        }

        Vector3 baseForward;
        switch (movementForwardDirection)
        {
            case ForwardDirection.X:
                baseForward = movementReference.right;
                break;
            case ForwardDirection.Y:
                baseForward = movementReference.up;
                break;
            case ForwardDirection.Z:
                baseForward = movementReference.forward;
                break;
            case ForwardDirection.NegativeX:
                baseForward = -movementReference.right;
                break;
            case ForwardDirection.NegativeY:
                baseForward = -movementReference.up;
                break;
            case ForwardDirection.NegativeZ:
                baseForward = -movementReference.forward;
                break;
            default:
                baseForward = movementReference.forward;
                break;
        }

        // Validate base forward
        if (!IsValidVector(baseForward) || baseForward.sqrMagnitude < 0.01f)
        {
            return Vector3.forward;
        }

        // Apply the forward direction offset (rotates around Y axis)
        if (Mathf.Abs(forwardDirectionOffset) > 0.01f)
        {
            Vector3 upVector = movementReference.up;
            if (!IsValidVector(upVector) || upVector.sqrMagnitude < 0.01f)
            {
                upVector = Vector3.up;
            }

            Quaternion offsetRotation = Quaternion.AngleAxis(forwardDirectionOffset, upVector);
            baseForward = offsetRotation * baseForward;
        }

        return baseForward.normalized;
    }

    // Optional: Call this if you want to manually set wheel rotation (e.g., for velocity-based movement)
    public void SetWheelRotationFromVelocity(float velocity)
    {
        // Validate input
        if (float.IsNaN(velocity) || float.IsInfinity(velocity))
        {
            return;
        }

        float circumference = 2f * Mathf.PI * Mathf.Max(0.01f, wheelRadius);
        float rotationAmount = (velocity * Time.deltaTime / circumference) * 360f;

        // Validate rotation amount
        if (float.IsNaN(rotationAmount) || float.IsInfinity(rotationAmount))
        {
            return;
        }

        // Apply invert if enabled
        if (invertRotation)
        {
            rotationAmount = -rotationAmount;
        }

        // Clamp rotation amount
        rotationAmount = Mathf.Clamp(rotationAmount, -3600f, 3600f);

        currentRotation += rotationAmount;

        // Wrap rotation to prevent overflow
        if (Mathf.Abs(currentRotation) > 360000f)
        {
            currentRotation = currentRotation % 360f;
        }

        Quaternion spinRotation = GetSpinRotation(currentRotation);

        if (frontWheel != null && IsValidTransform(frontWheel))
        {
            frontWheel.localRotation = frontWheelInitialRotation * spinRotation;
        }

        if (rearWheel != null && IsValidTransform(rearWheel))
        {
            rearWheel.localRotation = rearWheelInitialRotation * spinRotation;
        }
    }

    // Draw direction arrow in Scene view
    void OnDrawGizmos()
    {
        if (!showDirectionArrow) return;

        // Use movement reference if assigned, otherwise use this transform
        Transform reference = movementReference != null ? movementReference : transform;

        // Validate reference
        if (reference == null) return;

        // Get the actual forward direction based on settings
        Vector3 actualForward = movementReference != null ? GetMovementForward() : transform.forward;

        // Validate forward direction
        if (!IsValidVector(actualForward) || actualForward.sqrMagnitude < 0.01f) return;

        // Draw arrow showing forward direction
        Gizmos.color = arrowColor;
        Vector3 start = reference.position;
        Vector3 end = start + actualForward * arrowLength;

        // Draw main arrow line
        Gizmos.DrawLine(start, end);

        // Draw arrowhead (using perpendicular vectors)
        Vector3 right = Vector3.Cross(actualForward, Vector3.up).normalized;
        if (right.magnitude < 0.1f) right = Vector3.Cross(actualForward, Vector3.right).normalized;
        Vector3 up = Vector3.Cross(right, actualForward).normalized;

        right *= arrowLength * 0.2f;
        up *= arrowLength * 0.2f;

        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) + right);
        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) - right);
        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) + up);
        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) - up);

        // Draw label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(end + up * 0.5f, "Forward →",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = arrowColor },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            });
#endif
    }

    void OnValidate()
    {
        // Clamp values to prevent invalid settings
        wheelRadius = Mathf.Max(0.01f, wheelRadius);
        rotationSpeed = Mathf.Clamp(rotationSpeed, 1f, 10000f);
        arrowLength = Mathf.Max(0.1f, arrowLength);
    }
}