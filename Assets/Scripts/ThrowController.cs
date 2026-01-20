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
        public float projectileRadius = 0.25f;
        public float hitOffsetUp = 0.05f;

        [Header("Gamepad Settings (Player 2)")]
        public int gamepadPlayerIndex = 2;   // This is for Player2 to use
        public int r2ButtonIndex = 7;        // PS5's R2 

        [Header("Charge Settings")]
        public float minThrowSpeed = 6f;        // short distance
        public float maxThrowSpeed = 18f;       // max distance 
        public float chargeTimeToMax = 1.2f;    // how long to reach max distance
        public AnimationCurve chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Arc Settings")]
        [Tooltip("Fixed launch angle in degrees. Higher = more arc, shorter range.")]
        public float fixedAngleDeg = 45f;

        [Header("Spawn Offset")]
        public float spawnHeightOffset = 0.0f;

        private bool isAiming = false;

        // Charge state
        private float holdTime = 0f;

        private Vector3 cachedStartPos;
        private Vector3 cachedVelocity;

        void Update()
        {
            //  Mouse input     
            bool mouseAimDown = Input.GetMouseButtonDown(1);
            bool mouseAimUp = Input.GetMouseButtonUp(1);
            bool mouseAimHeld = Input.GetMouseButton(1);

            // PS5 R2 
            string r2KeyName = "Joystick" + gamepadPlayerIndex + "Button" + r2ButtonIndex;
            KeyCode r2Key = (KeyCode)System.Enum.Parse(typeof(KeyCode), r2KeyName);

            bool r2Down = Input.GetKeyDown(r2Key);
            bool r2Up = Input.GetKeyUp(r2Key);
            bool r2Held = Input.GetKey(r2Key);

            // Combined input
            bool aimDown = mouseAimDown || r2Down;
            bool aimUp = mouseAimUp || r2Up;
            bool aimHeld = mouseAimHeld || r2Held;

            // Start aiming + start charging
            if (aimDown)
            {
                isAiming = true;
                holdTime = 0f; // reset charge at the moment start aiming
            }

            // While holding: charge + update prediction curve every frame
            if (isAiming && aimHeld)
            {
                holdTime += Time.deltaTime;
                UpdateAim(); // prediction line will grow as speed increases
            }

            // Release: throw using the last cached velocity
            if (aimUp)
            {
                if (isAiming)
                {
                    Throw();
                }

                isAiming = false;
                holdTime = 0f;
                curveVisualizer.HideProjectileCurve();
            }
        }

        void UpdateAim()
        {
            if (throwOrigin == null || curveVisualizer == null) return;

            Camera cam = throwCamera != null ? throwCamera : Camera.main;
            if (cam == null) return;

            // Direction follows camera yaw only 
            Vector3 camForward = cam.transform.forward;
            Vector3 horizontalDir = new Vector3(camForward.x, 0f, camForward.z);

            if (horizontalDir.sqrMagnitude < 0.001f)
                horizontalDir = transform.forward;

            horizontalDir.Normalize();

            // Charge the current throw speed 
            float t = Mathf.Clamp01(holdTime / Mathf.Max(0.0001f, chargeTimeToMax));
            float shaped = (chargeCurve != null) ? chargeCurve.Evaluate(t) : t;
            float currentSpeed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, shaped);

            // Fixed arc angle
            float angleRad = fixedAngleDeg * Mathf.Deg2Rad;

            Vector3 launchDir =
                horizontalDir * Mathf.Cos(angleRad) +
                Vector3.up * Mathf.Sin(angleRad);

            launchDir.Normalize();

            Vector3 launchVelocity = launchDir * currentSpeed;

            // Startpoint
            Vector3 startPos = throwOrigin.position;

            // Debug: start marker
            Debug.DrawRay(startPos, Vector3.up * 2f, Color.red, 0f, false);

            // Draw arc prediction
            RaycastHit hit;
            curveVisualizer.VisualizeProjectileCurve(
                projectileStartPosition: startPos,
                projectileStartPositionForwardOffset: 0f,
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
            if (projectilePrefab == null) return;

            Vector3 spawnPos = cachedStartPos + Vector3.up * spawnHeightOffset;

            // Debug: spawn marker
            Debug.DrawRay(spawnPos, Vector3.up * 2f, Color.green, 2f, false);

            Projectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            proj.deliverySystem = deliverySystem;
            proj.Throw(cachedVelocity);

            curveVisualizer.HideProjectileCurve();
        }
    }
}
