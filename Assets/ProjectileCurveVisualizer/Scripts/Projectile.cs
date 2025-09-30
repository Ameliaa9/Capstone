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

        private void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;
            hasHit = true;

            DeliverySpot target = collision.collider.GetComponent<DeliverySpot>();
            if (target != null)
            {
                target.OnPackageHit();
            }
        }
    }
}
