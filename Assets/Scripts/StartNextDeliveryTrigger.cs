using UnityEngine;
using TMPro;

public class StartNextDeliveryTrigger : MonoBehaviour
{
    public Timer deliveryTimer;
    public ProjectileCurveVisualizerSystem.Projectile deliveryProjectile;
    public GameObject[] notificationUIs;
    public string[] targetTags;
    public float[] deliveryDurations; // Set custom timer durations in Inspector
    public Transform playerTransform; // Drag your Player here in the Inspector

    private int currentIndex = 0;
    private bool waitingForDelivery = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == playerTransform && !waitingForDelivery)
        {
            StartDeliveryRound();
        }
    }

    private void StartDeliveryRound()
    {
        // Prevent starting more than available rounds
        if (currentIndex >= targetTags.Length || currentIndex >= notificationUIs.Length || currentIndex >= deliveryDurations.Length)
        {
            Debug.Log("No more deliveries.");
            return;
        }

        // Turn off all UI notifications
        foreach (GameObject ui in notificationUIs)
        {
            if (ui != null) ui.SetActive(false);
        }

       

        // Set timer duration and reset
        if (deliveryTimer != null)
        {
            float seconds = deliveryDurations[currentIndex];
            deliveryTimer.seconds = (int)seconds;
            deliveryTimer.ResetTimer();
        }

        waitingForDelivery = true;
    }

    public void MarkDeliveryComplete()
    {
        Debug.Log("Delivery completed!");

        // Show success notification
        if (currentIndex < notificationUIs.Length && notificationUIs[currentIndex] != null)
        {
            notificationUIs[currentIndex].SetActive(true);
        }

        waitingForDelivery = false;
        currentIndex++;
    }

    public void MarkDeliveryFailed()
    {
        Debug.Log("Delivery failed.");

        waitingForDelivery = false;
        currentIndex++;
    }
}
