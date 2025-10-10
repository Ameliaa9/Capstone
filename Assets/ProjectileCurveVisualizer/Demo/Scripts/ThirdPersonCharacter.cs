using UnityEngine;
using ProjectileCurveVisualizerSystem;

public class ThirdPersonCharacter : MonoBehaviour
{
    public Transform springArmTransform; 
    public Transform cameraTransform;   

    public ProjectileCurveVisualizer projectileCurveVisualizer;
    public GameObject projectileGameObject;

    public float cameraSpeed = 70f;
    public float verticalSpeed = 50f;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public float launchSpeed = 20f;

    private float yaw = 0f;
    private float pitch = 20f;

    private bool inProjectileMode = false;
    private Vector3 launchVelocity;
    private Vector3 updatedProjectileStartPosition;
    private RaycastHit hit;

    private float aimDistance = 10f;

    void Start()
    {
        yaw = springArmTransform.rotation.eulerAngles.y;
        pitch = springArmTransform.rotation.eulerAngles.x;
    }

    void Update()
    {
        HandleCameraAim();
        HandleProjectileMode();
    }

    void HandleCameraAim()
    {
        if (!Settings.mouseControl) return;


        float inputX = Input.GetAxis("Mouse X");
        float inputY = -Input.GetAxis("Mouse Y");

        yaw += inputX * cameraSpeed * Time.deltaTime;
        pitch += inputY * verticalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        springArmTransform.rotation = rotation;

      
        cameraTransform.localPosition = inProjectileMode
            ? Vector3.Lerp(cameraTransform.localPosition, new Vector3(1.5f, 0, -3f), 0.1f)
            : Vector3.Lerp(cameraTransform.localPosition, new Vector3(1.5f, 0, -4f), 0.1f);
    }

    void HandleProjectileMode()
    {
        
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick2Button2))
        {
            inProjectileMode = !inProjectileMode;
            if (!inProjectileMode)
                projectileCurveVisualizer.HideProjectileCurve();
        }

        if (!inProjectileMode) return;

        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float stickY = Input.GetAxis("Joystick2Axis5"); 
        launchSpeed = Mathf.Clamp(launchSpeed + (scroll + stickY * 0.5f) * 6f, 1f, 100f);

        Vector3 aimTargetPosition = cameraTransform.position;
        if (Input.mousePresent)
        {

            float adjust = 0f;

            aimTargetPosition += cameraTransform.forward * aimDistance;
            float mouseY = Input.GetAxis("Mouse Y");
            aimDistance = Mathf.Clamp(aimDistance - mouseY * 0.5f, 2f, 30f);

            if (Input.GetKey(KeyCode.Joystick2Button4))
                adjust -= 0.3f;
            if (Input.GetKey(KeyCode.Joystick2Button6)) 
                adjust += 0.3f;

            aimDistance = Mathf.Clamp(aimDistance + adjust, 2f, 30f);




        }
        else
        {
          
            float aimX = Input.GetAxis("Joystick2Axis3");
            float aimY = Input.GetAxis("Joystick2Axis4");
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0;
            Vector3 camRight = cameraTransform.right;
            camRight.y = 0;
            Vector3 aimDir = (camForward * aimY + camRight * aimX).normalized;
            aimTargetPosition += aimDir * aimDistance;
        }

        Vector3 direction = (aimTargetPosition - springArmTransform.position).normalized;
        launchVelocity = direction * launchSpeed;

        projectileCurveVisualizer.VisualizeProjectileCurve(
            springArmTransform.position,
            1.0f,
            launchVelocity,
            0.25f,
            0.1f,
            true,
            out updatedProjectileStartPosition,
            out hit
        );

        
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Joystick2Button7))
        {
            inProjectileMode = false;
            projectileCurveVisualizer.HideProjectileCurve();

            if (projectileGameObject != null)
            {
                Projectile projectile = Instantiate(projectileGameObject).GetComponent<Projectile>();
                projectile.transform.position = updatedProjectileStartPosition;
                projectile.Throw(launchVelocity);
            }
        }
    }
}
