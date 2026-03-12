using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public string playerTag = "Player";
    public float healAmount = 10f;

    public BikeHealth bikeHealth;

    public AudioClip disappearSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (bikeHealth != null)
            {
                // Check if adding heal would go over max
                float newHealth = bikeHealth.Health + healAmount;

                // If it's above max health, force it to max
                if (newHealth > bikeHealth.maxHealth)
                {
                    float amountNeeded = bikeHealth.maxHealth - bikeHealth.Health;
                    bikeHealth.SetHealth(amountNeeded);
                }
                else
                {
                    bikeHealth.SetHealth(healAmount);
                }
            }

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.HealthPickupCollected();

                if (disappearSound != null)
                {
                    AudioSource.PlayClipAtPoint(disappearSound, transform.position);
                }

            }

            gameObject.SetActive(false);
        }
    }
}
