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

        [Header("Spawn Offset")]
        public float spawnHeightOffset = 0.0f;        // set as o first 
        private bool isAiming = false;

        private Vector3 cachedStartPos;
        private Vector3 cachedVelocity;

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                isAiming = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                if (isAiming)
                {
                    Throw();
                }

                isAiming = false;
                curveVisualizer.HideProjectileCurve();
            }

            if (isAiming)
            {
                UpdateAim();
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
