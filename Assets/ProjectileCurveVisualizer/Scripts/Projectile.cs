using UnityEngine;
using System.Collections; // Needed for IEnumerator

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        public Rigidbody projectileRigidbody;
        public MeshCollider projectileMeshCollider;

        [Header("Delivery Settings")]
        public StartNextDeliveryTrigger deliveryTrigger;

        [Header("Building Tags + Notifications")]
        public string[] buildingTags;              // assign TargetBuilding, TargetBuilding2, TargetBuilding3 in Inspector
        public GameObject[] buildingNotificationUIs; // assign UI images for each building in Inspector

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

            // Loop through all tags to see which one we hit
            for (int i = 0; i < buildingTags.Length; i++)
            {
                if (!string.IsNullOrEmpty(buildingTags[i]) && collision.gameObject.CompareTag(buildingTags[i]))
                {
                    Debug.Log("Correct building hit: " + buildingTags[i]);

                    // Show the notification UI for this building
                    if (i < buildingNotificationUIs.Length && buildingNotificationUIs[i] != null)
                    {
                        buildingNotificationUIs[i].SetActive(true);
                        StartCoroutine(HideAfterSeconds(buildingNotificationUIs[i], 5f)); // ? Hide after 5 seconds
                    }

                    // Tell the delivery system that we succeeded
                    if (deliveryTrigger != null)
                    {
                        deliveryTrigger.MarkDeliveryComplete();
                    }

                    break; // exit loop once matched
                }
            }
        }

        // ? Coroutine to hide the notification UI
        private IEnumerator HideAfterSeconds(GameObject obj, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}
