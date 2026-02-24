using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        public Rigidbody projectileRigidbody;
        public MeshCollider projectileMeshCollider;
        
        public DeliverySystem deliverySystem;
        public float lifetimeInWorld = 5f;


        public void Throw(Vector3 velocity)
        {
            projectileMeshCollider.enabled = true;
            projectileRigidbody.useGravity = true;
            projectileRigidbody.linearVelocity = velocity;

            Destroy(gameObject, lifetimeInWorld);
        }

        void OnCollisionEnter(Collision collision)
        {
            // hit target start counting
            if (collision.gameObject.CompareTag("Target"))
            {
                var hitOnce = collision.gameObject.GetComponent<TargetHitOnce>();
                if (hitOnce == null || hitOnce.TryMarkHit())
                    FindObjectOfType<TaskManager>()?.OnTargetHit();

                return;
            }

            // hit correct house
            if (collision.gameObject == deliverySystem.DeliveryLocations[deliverySystem.currentDelivery])
            {
                deliverySystem.ProjectileHitHouse(deliverySystem.currentDelivery);
            }
            else
            {
                Debug.Log($"Projectile collided with wrong gameObject: {collision.gameObject.name}");
            }
        }
    }
}