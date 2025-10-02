using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        public Rigidbody projectileRigidbody;
        public MeshCollider projectileMeshCollider;
        public StartNextDeliveryTrigger deliveryTrigger;


        private bool hasHit = false;

        public void Throw(Vector3 velocity)
        {
            projectileMeshCollider.enabled = true;
            projectileRigidbody.useGravity = true;
            projectileRigidbody.linearVelocity = velocity;
            hasHit = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("TargetBuilding"))
            {
                DeliverySpot spot = collision.gameObject.GetComponent<DeliverySpot>();
                if (spot != null)
                {
                    // Find the DeliverySystem in the scene
                    DeliverySystem system = FindObjectOfType<DeliverySystem>();
                    if (system != null)
                        system.ProjectileHitHouse(spot.houseIndex);
                }
            }
        }
    }
}