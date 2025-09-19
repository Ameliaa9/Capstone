using UnityEngine;

public class BikeCameraOrbit : MonoBehaviour
{
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 2, -6);
    public float rotationSpeed = 70f;
    public float verticalSpeed = 50f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private float yaw = 0f;
    private float pitch = 20f;

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

        if (Input.GetKey(KeyCode.A)) inputX = -1f;
        if (Input.GetKey(KeyCode.D)) inputX = 1f;
        if (Input.GetKey(KeyCode.W)) inputY = 1f;
        if (Input.GetKey(KeyCode.S)) inputY = -1f;

        yaw += inputX * rotationSpeed * Time.deltaTime;
        pitch += inputY * verticalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 rotatedOffset = rotation * offset;

        transform.position = followTarget.position + rotatedOffset;
        transform.LookAt(followTarget.position + Vector3.up * 1.5f);
    }
}

