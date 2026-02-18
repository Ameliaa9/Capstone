using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BikeWheelRotation : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }
    public enum ForwardDirection { X, Y, Z, NegativeX, NegativeY, NegativeZ }

    [Header("Movement Reference")]
    [Tooltip("The GameObject to track for movement (e.g., bike body, player controller)")]
    public Transform movementReference;

    [Tooltip("Which direction is 'forward' for movement tracking")]
    public ForwardDirection movementForwardDirection = ForwardDirection.Z;

    [Header("Wheel References")]
    [Tooltip("Add as many wheels as needed (front, rear, training wheels, etc.)")]
    public List<WheelData> wheels = new List<WheelData>();

    [Header("Default Rotation Settings")]
    [Tooltip("Default axis for new wheels (X = side to side, Y = up/down, Z = front to back)")]
    public RotationAxis defaultWheelRotationAxis = RotationAxis.X;

    [Tooltip("Rotate which direction is considered 'forward' (in degrees, rotates around Y axis)")]
    [Range(-180f, 180f)]
    public float forwardDirectionOffset = 0f;

    [Tooltip("How fast the wheels rotate (degrees per unit of movement)")]
    public float rotationSpeed = 360f;

    [Tooltip("Wheel radius in units (used for realistic rotation)")]
    public float wheelRadius = 0.3f;

    [Header("Debug Visualization")]
    [Tooltip("Show an arrow indicating the direction that makes wheels spin forward")]
    public bool showDirectionArrow = true;

    [Tooltip("Length of the direction arrow")]
    public float arrowLength = 2f;

    [Tooltip("Color of the direction arrow")]
    public Color arrowColor = Color.green;

    private Vector3 lastPosition;
    private float currentRotation = 0f;

    [System.Serializable]
    public class WheelData
    {
        [Tooltip("The wheel transform to rotate")]
        public Transform wheelTransform;

        [Tooltip("Which axis this wheel rotates around")]
        public RotationAxis rotationAxis = RotationAxis.X;

        [Tooltip("Optional: Custom radius for this wheel (0 = use default)")]
        public float customRadius = 0f;

        [Tooltip("Optional: Invert rotation direction for this wheel")]
        public bool invertRotation = false;

        [Tooltip("Optional: Rotation speed multiplier for this wheel")]
        public float speedMultiplier = 1f;

        [HideInInspector] public Quaternion initialRotation;
    }

    void Start()
    {
        if (movementReference == null)
        {
            movementReference = transform;
            Debug.Log("BikeWheelRotation: No movement reference assigned, using this GameObject's transform.");
        }

        lastPosition = movementReference.position;

        foreach (var wheel in wheels)
        {
            if (wheel.wheelTransform != null)
            {
                wheel.initialRotation = wheel.wheelTransform.localRotation;
            }
        }

        if (wheels.Count == 0)
        {
            Debug.LogWarning("BikeWheelRotation: No wheels assigned! Add wheels to the list in the Inspector.");
        }
    }

    void Update()
    {
        RotateWheelsBasedOnMovement();
    }

    void RotateWheelsBasedOnMovement()
    {
        Vector3 currentPosition = movementReference.position;
        Vector3 movement = currentPosition - lastPosition;
        Vector3 actualForward = GetMovementForward();
        float distanceMoved = Vector3.Dot(movement, actualForward);

        float circumference = 2f * Mathf.PI * wheelRadius;
        float rotationAmount = (distanceMoved / circumference) * 360f;

        currentRotation += rotationAmount;

        foreach (var wheel in wheels)
        {
            if (wheel.wheelTransform == null) continue;

            float radius = wheel.customRadius > 0 ? wheel.customRadius : wheelRadius;
            float wheelCircumference = 2f * Mathf.PI * radius;

            float wheelRotation = (distanceMoved / wheelCircumference) * 360f * wheel.speedMultiplier;
            if (wheel.invertRotation) wheelRotation = -wheelRotation;

            float totalRotation = currentRotation * wheel.speedMultiplier;
            if (wheel.invertRotation) totalRotation = -totalRotation;

            // Use per-wheel rotation axis
            Quaternion spinRotation = GetSpinRotation(totalRotation, wheel.rotationAxis);
            wheel.wheelTransform.localRotation = wheel.initialRotation * spinRotation;
        }

        lastPosition = currentPosition;
    }

    Quaternion GetSpinRotation(float rotation, RotationAxis axis)
    {
        switch (axis)
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

        if (Mathf.Abs(forwardDirectionOffset) > 0.01f)
        {
            Quaternion offsetRotation = Quaternion.AngleAxis(forwardDirectionOffset, movementReference.up);
            baseForward = offsetRotation * baseForward;
        }

        return baseForward;
    }

    public void AddWheel(Transform wheelTransform, RotationAxis axis = RotationAxis.X, float customRadius = 0f, bool invert = false, float speedMult = 1f)
    {
        WheelData newWheel = new WheelData
        {
            wheelTransform = wheelTransform,
            rotationAxis = axis,
            customRadius = customRadius,
            invertRotation = invert,
            speedMultiplier = speedMult,
            initialRotation = wheelTransform != null ? wheelTransform.localRotation : Quaternion.identity
        };
        wheels.Add(newWheel);
    }

    public void RemoveWheel(int index)
    {
        if (index >= 0 && index < wheels.Count)
        {
            wheels.RemoveAt(index);
        }
    }

    public void ClearWheels()
    {
        wheels.Clear();
    }

    public void SetWheelRotationFromVelocity(float velocity)
    {
        float circumference = 2f * Mathf.PI * wheelRadius;
        float rotationAmount = (velocity * Time.deltaTime / circumference) * 360f;

        currentRotation += rotationAmount;

        foreach (var wheel in wheels)
        {
            if (wheel.wheelTransform != null)
            {
                float totalRotation = currentRotation * wheel.speedMultiplier;
                if (wheel.invertRotation) totalRotation = -totalRotation;
                wheel.wheelTransform.localRotation = wheel.initialRotation * GetSpinRotation(totalRotation, wheel.rotationAxis);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDirectionArrow) return;

        Transform reference = movementReference != null ? movementReference : transform;
        Vector3 actualForward = movementReference != null ? GetMovementForward() : transform.forward;

        Gizmos.color = arrowColor;
        Vector3 start = reference.position;
        Vector3 end = start + actualForward * arrowLength;

        Gizmos.DrawLine(start, end);

        Vector3 right = Vector3.Cross(actualForward, Vector3.up).normalized;
        if (right.magnitude < 0.1f) right = Vector3.Cross(actualForward, Vector3.right).normalized;
        Vector3 up = Vector3.Cross(right, actualForward).normalized;

        right *= arrowLength * 0.2f;
        up *= arrowLength * 0.2f;

        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) + right);
        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) - right);
        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) + up);
        Gizmos.DrawLine(end, end - actualForward * (arrowLength * 0.3f) - up);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(end + up * 0.5f, "Forward",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = arrowColor },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            });
#endif
    }
}