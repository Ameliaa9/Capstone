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
            if (collision.gameObject.CompareTag("TargetBuilding"))
            {
                var hitOnce = collision.gameObject.GetComponent<TargetHitOnce>();
                if (hitOnce == null || hitOnce.TryMarkHit())
                    FindObjectOfType<TaskManager>()?.OnTargetHit();

                return;
            }

            Transform hitTransform = collision.transform;

            // hit correct house
            if (IsSameOrChildOf(hitTransform, deliverySystem.DeliveryLocations[deliverySystem.currentDelivery]))
            {
                deliverySystem.ProjectileHitHouse(deliverySystem.currentDelivery);
                Destroy(gameObject);
            }
            else if (deliverySystem.secondaryDelivery >= 0 &&
                     IsSameOrChildOf(hitTransform, deliverySystem.DeliveryLocations[deliverySystem.secondaryDelivery]))
            {
                deliverySystem.ProjectileHitHouse(deliverySystem.secondaryDelivery);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"Projectile collided with wrong gameObject: {collision.gameObject.name}");
            }
        }

        bool IsSameOrChildOf(Transform hitObject, GameObject targetObject)
        {
            if (hitObject == null || targetObject == null) return false;
            return hitObject == targetObject.transform || hitObject.IsChildOf(targetObject.transform);
        }
    }
}