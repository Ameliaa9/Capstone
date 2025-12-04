using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class ThrowController : MonoBehaviour
    {
        [Header("References")]
        public Transform throwOrigin;                 // ProjectileSpawnPoint
        public Projectile projectilePrefab;           // Projectile prefab
        public DeliverySystem deliverySystem;         // DeliverySystem on BikeTest
        public ProjectileCurveVisualizer curveVisualizer; // Trajectory visualization

        public Camera throwCamera;
        [Header("Throw Settings")]
        public float throwSpeed = 10f;                // set as medium speed first
        public float projectileRadius = 0.25f;
        public float hitOffsetUp = 0.05f;

        [Header("Gamepad Settings (Player 2)")]
        public int gamepadPlayerIndex = 2;   // This is for Player2 to use
        public int r2ButtonIndex = 7;        // PS5's R2 looks like Button7 if something error we can edit it


        [Header("Spawn Offset")]
        public float spawnHeightOffset = 0.0f;        // set as o first 
        private bool isAiming = false;

        private Vector3 cachedStartPos;
        private Vector3 cachedVelocity;

        void Update()
        {
            // Mouse input
            bool mouseAimDown = Input.GetMouseButtonDown(1);  
            bool mouseAimUp = Input.GetMouseButtonUp(1);    
            bool mouseAimHeld = Input.GetMouseButton(1);      

            // PS5 R2 INPUT
           
            string r2KeyName = "Joystick" + gamepadPlayerIndex + "Button" + r2ButtonIndex;
            KeyCode r2Key = (KeyCode)System.Enum.Parse(typeof(KeyCode), r2KeyName);

            bool r2Down = Input.GetKeyDown(r2Key);   
            bool r2Up = Input.GetKeyUp(r2Key);     
            bool r2Held = Input.GetKey(r2Key);       

            // Set up together
            bool aimDown = mouseAimDown || r2Down;
            bool aimUp = mouseAimUp || r2Up;
            bool aimHeld = mouseAimHeld || r2Held;

            
            if (aimDown)
            {
                isAiming = true;
            }

            if (aimUp)
            {
                if (isAiming)
                {
                    Throw();                        // Throw when loose hand
                }

                isAiming = false;
                curveVisualizer.HideProjectileCurve();
            }

            if (isAiming && aimHeld)
            {
                UpdateAim();                        // keep hoding to draw prediction curve line and aiming
            }
        }


        void UpdateAim()
        {
            Camera cam = throwCamera != null ? throwCamera : Camera.main;
            if (cam == null) return;

            // Throw Direction
            Vector3 camForward = cam.transform.forward;
            Vector3 horizontalDir = new Vector3(camForward.x, 0f, camForward.z);

            if (horizontalDir.sqrMagnitude < 0.001f)
                horizontalDir = transform.forward;

            horizontalDir.Normalize();

            float angleDeg = 45f;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 launchDir =
                horizontalDir * Mathf.Cos(angleRad) +
                Vector3.up * Mathf.Sin(angleRad);

            launchDir.Normalize();
            Vector3 launchVelocity = launchDir * throwSpeed;

            // start point = SpawnPoint 
            Vector3 startPos = throwOrigin.position;

            // Draw a distinct red vertical line in both Scene and Game to mark the "start point as determined by the code".
            Debug.DrawRay(startPos, Vector3.up * 2f, Color.red, 0f, false);

            // Draw an arc with Visualizer (no further push forward)
            RaycastHit hit;
            curveVisualizer.VisualizeProjectileCurve(
                projectileStartPosition: startPos,
                projectileStartPositionForwardOffset: 0f,     // set as 0 first
                launchVelocity: launchVelocity,
                projectileRadius: projectileRadius,
                distanceOffsetAboveHitPosition: hitOffsetUp,
                debugMode: false,
                updatedProjectileStartPosition: out _,
                hit: out hit
            );

            cachedStartPos = startPos;
            cachedVelocity = launchVelocity;
        }

        void Throw()
        {
            Vector3 spawnPos = cachedStartPos + Vector3.up * spawnHeightOffset;

            // Draw a green line to show where the package start from
            Debug.DrawRay(spawnPos, Vector3.up * 2f, Color.green, 2f, false);

            Projectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            proj.deliverySystem = deliverySystem;
            proj.Throw(cachedVelocity);

            curveVisualizer.HideProjectileCurve();
        }
    }
}
