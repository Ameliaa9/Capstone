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
            if (collision.gameObject == deliverySystem.DeliveryLocations[deliverySystem.currentDelivery])
            {
                deliverySystem.ProjectileHitHouse(deliverySystem.currentDelivery);
            }
            else
            {
                Debug.Log($"Projectile collided with wrong gameObject" + collision.gameObject);
            }
            
        }
    }
}