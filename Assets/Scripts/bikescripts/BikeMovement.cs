using UnityEngine;

public class BikeMovement : MonoBehaviour
{
    public float speed = 6f;
    public float turnSpeed = 180f;

    [Header("Leaning (Visual Only)")]
    public Transform bikeVisual;
    public float leanAngle = 20f;
    public float leanSpeed = 8f;

    public float maxHealth = 100f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        // keyboard
        float keyboardMove = 0f;
        float keyboardTurn = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) keyboardMove = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) keyboardMove = -1f;

        if (Input.GetKey(KeyCode.RightArrow)) keyboardTurn = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) keyboardTurn = -1f;

        // controller
        float controllerMove = Input.GetAxis("Joystick1Vertical");
        float controllerTurn = Input.GetAxis("Joystick1Horizontal");

        float move = Mathf.Abs(keyboardMove) > 0 ? keyboardMove : controllerMove;
        float turn = Mathf.Abs(keyboardTurn) > 0 ? keyboardTurn : controllerTurn;

        // move
        rb.MovePosition(
            rb.position + transform.forward * move * speed * Time.fixedDeltaTime
        );

        // turn
        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f)
        );

        // lean
        if (bikeVisual != null)
        {
            float targetLean = -turn * leanAngle;
            Quaternion leanRot = Quaternion.Euler(0f, 0f, targetLean);

            bikeVisual.localRotation = Quaternion.Lerp(
                bikeVisual.localRotation,
                leanRot,
                leanSpeed * Time.fixedDeltaTime
            );
        }
    }

    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    // check the speed 
    public float GetBikeSpeedKm()
    {
        float keyboardMove = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) keyboardMove = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) keyboardMove = -1f;

        float controllerMove = Input.GetAxis("Joystick1Vertical");

        float moveInput = Mathf.Abs(keyboardMove) > 0 ? keyboardMove : controllerMove;

        float currentSpeedMps = Mathf.Abs(moveInput * speed);

        return currentSpeedMps * 3.6f;
    }
}