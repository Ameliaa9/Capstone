using UnityEngine;

public class BikeHandleController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main transform of the bike body.")]
    public Transform bikeTransform;

    [Tooltip("The handlebar mesh/pivot that will rotate visually.")]
    public Transform handlebarTransform;

    [Header("Orientation Fix")]
    [Tooltip("Enable this if your bike model is facing backwards (Blue arrow points behind the bike). This fixes the 'wrong direction' issue.")]
    public bool isInvertedBikeModel = false;

    [Header("Steering Settings")]
    [Tooltip("How much the handlebar turns relative to the bike's rotation speed.")]
    public float steerSensitivity = 0.2f;

    [Tooltip("Maximum angle (in degrees) the handlebar can turn left or right.")]
    public float maxSteerAngle = 45f;

    [Tooltip("How smoothly the handlebar reacts to rotation.")]
    public float smoothing = 8f;

    // Internal state variables
    private float previousYRotation;
    private Vector3 previousPosition;
    private float currentSteerAngle;

    void Start()
    {
        if (bikeTransform == null)
            bikeTransform = this.transform;

        previousYRotation = bikeTransform.eulerAngles.y;
        previousPosition = bikeTransform.position;
    }

    void Update()
    {
        if (bikeTransform == null || handlebarTransform == null) return;

        // --- 1. Calculate Bike Rotation Speed (Yaw) ---
        float currentYRotation = bikeTransform.eulerAngles.y;
        float deltaAngle = Mathf.DeltaAngle(previousYRotation, currentYRotation);
        float rotationSpeed = deltaAngle / Time.deltaTime;
        previousYRotation = currentYRotation;

        // --- 2. Calculate Forward Speed ---
        Vector3 displacement = bikeTransform.position - previousPosition;
        float forwardSpeed = Vector3.Dot(displacement, bikeTransform.forward) / Time.deltaTime;
        previousPosition = bikeTransform.position;

        // --- ORIENTATION FIX ---
        // If the model is inverted, we flip the speed detection so the script knows we are moving forward
        if (isInvertedBikeModel)
        {
            forwardSpeed *= -1f;
        }

        // --- 3. Calculate Target Steering Angle ---
        float targetAngle = rotationSpeed * steerSensitivity;

        // --- REVERSE LOGIC ---
        // If moving backward, invert steering direction
        if (forwardSpeed < -0.1f)
        {
            targetAngle *= -1f;
        }

        // Clamp the angle
        targetAngle = Mathf.Clamp(targetAngle, -maxSteerAngle, maxSteerAngle);

        // --- 4. Apply Smooth Rotation ---
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.deltaTime * smoothing);
        handlebarTransform.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
    }
}