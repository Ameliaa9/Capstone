using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        public Rigidbody projectileRigidbody;
        public MeshCollider projectileMeshCollider;

        private string targetTag;
        private GameObject notificationUI;
        private bool hasHit = false;

        public StartNextDeliveryTrigger deliveryTrigger;

        public void Throw(Vector3 velocity)
        {
            projectileMeshCollider.enabled = true;
            projectileRigidbody.useGravity = true;
            projectileRigidbody.linearVelocity = velocity;
            hasHit = false;
        }

        public void SetTargetTag(string tag)
        {
            targetTag = tag;
        }

        public void SetNotificationUI(GameObject ui)
        {
            notificationUI = ui;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;
            hasHit = true;

            if (collision.gameObject.CompareTag(targetTag))
            {
                Debug.Log("Correct building hit!");

                if (deliveryTrigger != null)
                {
                    deliveryTrigger.MarkDeliveryComplete();
                }
            }
        }
    }
}
