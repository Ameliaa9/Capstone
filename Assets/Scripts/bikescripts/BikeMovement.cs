using UnityEngine;

public class BikeMovement : MonoBehaviour
{
    public float speed = 6f;
    public float turnSpeed = 180f;

    [Header("Jump")]
    public float jumpSpeed = 8f;
    public float fallMultiplier = 3f;
    public float groundCheckDistance = 0.6f;

    [Header("Leaning (Visual Only)")]
    public Transform bikeVisual;
    public float leanAngle = 20f;
    public float leanSpeed = 8f;

    [Header("Stability")]
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.8f, 0f);
    public float angularDragAmount = 6f;
    public float maxTiltAngle = 25f;
    public float uprightCorrectionSpeed = 6f;

    public float maxHealth = 100f;

    private Rigidbody rb;
    private bool jumpTriggered;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.centerOfMass = centerOfMassOffset;
        rb.angularDamping = angularDragAmount;
    }

    void Update()
    {
        // keyboard jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpTriggered = true;
        }

        // player 2 controller
        if (Input.GetKeyDown(KeyCode.Joystick2Button0))
        {
            jumpTriggered = true;
        }
    }

    void FixedUpdate()
    {
        // Check if bike is touching the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

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

        Vector3 velocity = transform.forward * move * speed;
        velocity.y = rb.linearVelocity.y;

        // Jump only if grounded
        if (jumpTriggered && isGrounded)
        {
            velocity.y = jumpSpeed;
        }

        jumpTriggered = false;

        rb.linearVelocity = velocity;

        // turn
        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f)
        );

        // heavier fall
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
        }

        // auto-upright if tipped too far
        float currentX = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
        float currentZ = Mathf.DeltaAngle(0f, transform.eulerAngles.z);

        if (Mathf.Abs(currentX) > maxTiltAngle || Mathf.Abs(currentZ) > maxTiltAngle)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, uprightCorrectionSpeed * Time.fixedDeltaTime));
        }

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

    public void ApplySpeedBoost(float boostAmount, float boostDuration)
    {
        StopAllCoroutines();
        StartCoroutine(SpeedBoostRoutine(boostAmount, boostDuration));
    }

    private System.Collections.IEnumerator SpeedBoostRoutine(float boostAmount, float boostDuration)
    {
        float originalSpeed = speed;
        speed += boostAmount;

        yield return new WaitForSeconds(boostDuration);

        speed = originalSpeed;
    }
}