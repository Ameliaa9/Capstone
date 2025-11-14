using UnityEngine;

public class BikeCameraOrbit : MonoBehaviour
{
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 2, -6);
    public float rotationSpeed = 70f;
    public float verticalSpeed = 50f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Aim Target")]
    public Transform aimTarget;
    public float aimDistance = 10f;

    private float yaw = 0f;
    private float pitch = 20f;


    public void RotateFromController(float x, float y)
    {
        yaw += x * rotationSpeed * Time.deltaTime;
        pitch += y * verticalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 rotatedOffset = rotation * offset;
        transform.position = followTarget.position + rotatedOffset;
        transform.LookAt(followTarget.position + Vector3.up * 1.5f);

        if (aimTarget != null)
            aimTarget.position = followTarget.position + rotation * (Vector3.forward * aimDistance);
    }

    void Start()
    {
        if (followTarget == null)
        {
            Debug.LogError("Follow target not assigned!");
            return;
        }

        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        float inputX = 0f;
        float inputY = 0f;

        inputX = Input.GetAxis("Joystick2Horizontal");
        inputY = -Input.GetAxis("Joystick2Vertical");

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

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 rotatedOffset = rotation * offset;

        transform.position = followTarget.position + rotatedOffset;
        transform.LookAt(followTarget.position + Vector3.up * 1.5f);

        if (aimTarget != null)
            aimTarget.position = followTarget.position + rotation * (Vector3.forward * aimDistance);
    }

}
