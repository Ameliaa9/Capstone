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

    private float previousYRotation;
    private Vector3 previousPosition;
    private float currentSteerAngle;

    void Start()
    {
        if (bikeTransform == null)
            bikeTransform = transform;

        previousYRotation = bikeTransform.eulerAngles.y;
        previousPosition = bikeTransform.position;
    }

    void Update()
    {
        if (bikeTransform == null || handlebarTransform == null) return;

        if (Time.deltaTime <= 0f)
            return;

        float currentYRotation = bikeTransform.eulerAngles.y;
        float deltaAngle = Mathf.DeltaAngle(previousYRotation, currentYRotation);
        float rotationSpeed = deltaAngle / Time.deltaTime;
        previousYRotation = currentYRotation;

        Vector3 displacement = bikeTransform.position - previousPosition;
        float forwardSpeed = Vector3.Dot(displacement, bikeTransform.forward) / Time.deltaTime;
        previousPosition = bikeTransform.position;

        if (isInvertedBikeModel)
        {
            forwardSpeed *= -1f;
        }

        float targetAngle = rotationSpeed * steerSensitivity;

        if (forwardSpeed < -0.1f)
        {
            targetAngle *= -1f;
        }

        targetAngle = Mathf.Clamp(targetAngle, -maxSteerAngle, maxSteerAngle);

        if (float.IsNaN(targetAngle) || float.IsInfinity(targetAngle))
            targetAngle = 0f;

        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.deltaTime * smoothing);

        if (float.IsNaN(currentSteerAngle) || float.IsInfinity(currentSteerAngle))
            currentSteerAngle = 0f;

        handlebarTransform.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
    }
}