using UnityEngine;

public class CameraOrbitWithWASD : MonoBehaviour
{
    public Transform followTarget; 
    public float rotationSpeed = 70f;

    private float yaw = 0f;

    void Start()
    {
        if (followTarget == null)
        {
            Debug.LogWarning("Follow target not set.");
        }
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        float camInput = 0f;

        if (Input.GetKey(KeyCode.A)) camInput = -1f;
        if (Input.GetKey(KeyCode.D)) camInput = 1f;

        yaw += camInput * rotationSpeed * Time.deltaTime;

        if (followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = Quaternion.Euler(0, yaw, 0);
        }
    }
}
