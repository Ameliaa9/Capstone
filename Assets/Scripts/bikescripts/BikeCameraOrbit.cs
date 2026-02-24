using UnityEngine;

public class BikeCameraOrbit : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 2, -6);

    [Header("Rotation")]
    public float rotationSpeed = 70f;
    public float verticalSpeed = 50f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Camera Collision")]
    public float collisionRadius = 0.25f;
    public float collisionBuffer = 0.15f;
    public float collisionSmooth = 12f;
    public LayerMask collisionLayers = ~0;

    [Header("Aim Target")]
    public Transform aimTarget;
    public float aimDistance = 10f;

    private float yaw;
    private float pitch = 20f;
    private float currentDistance;

    void Start()
    {
        if (!followTarget)
        {
            Debug.LogError("Follow target not assigned!");
            enabled = false;
            return;
        }

        yaw = transform.eulerAngles.y;
        currentDistance = offset.magnitude;
    }

    void Update()
    {
        ReadInput();
        UpdateCamera();
    }

    void ReadInput()
    {
        float inputX = Input.GetAxis("Joystick2Horizontal");
        float inputY = Input.GetAxis("Joystick2Vertical");

        if (Mathf.Abs(inputX) < 0.2f) inputX = 0f;
        if (Mathf.Abs(inputY) < 0.2f) inputY = 0f;

        if (inputX == 0f && inputY == 0f)
        {
            if (Input.GetKey(KeyCode.G)) inputX = -1f;
            if (Input.GetKey(KeyCode.J)) inputX = 1f;
            if (Input.GetKey(KeyCode.H)) inputY = 1f;
            if (Input.GetKey(KeyCode.Y)) inputY = -1f;
        }

        yaw += inputX * rotationSpeed * Time.deltaTime;
        pitch += inputY * verticalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void UpdateCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPos = followTarget.position + Vector3.up * 1.5f;
        Vector3 desiredOffset = rotation * offset;
        float desiredDistance = offset.magnitude;

        Vector3 desiredCamPos = targetPos + desiredOffset.normalized * desiredDistance;

        // camera collision
        RaycastHit hit;
        if (Physics.SphereCast(
            targetPos,
            collisionRadius,
            desiredOffset.normalized,
            out hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            desiredDistance = Mathf.Max(hit.distance - collisionBuffer, 0.5f);
        }

        currentDistance = Mathf.Lerp(
            currentDistance,
            desiredDistance,
            Time.deltaTime * collisionSmooth
        );

        transform.position = targetPos + desiredOffset.normalized * currentDistance;
        transform.LookAt(targetPos);

        if (aimTarget)
        {
            aimTarget.position = targetPos + rotation * Vector3.forward * aimDistance;
        }
    }
}
