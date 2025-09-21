using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        public Rigidbody projectileRigidbody;
        public MeshCollider projectileMeshCollider;

        public GameObject notificationUI; 

        private bool hasHit = false;

        public void Throw(Vector3 velocity)
        {
            projectileMeshCollider.enabled = true;
            projectileRigidbody.useGravity = true;
            projectileRigidbody.linearVelocity = velocity;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;
            hasHit = true;

            if (collision.gameObject.CompareTag("TargetBuilding"))
            {
                if (notificationUI != null)
                {
                    notificationUI.SetActive(true);
                }
            }
        }
    }
}
