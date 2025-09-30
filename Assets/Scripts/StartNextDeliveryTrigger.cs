using UnityEngine;

public class StartNextDeliveryTrigger : MonoBehaviour
{
    public DeliveryTimer deliveryTimer; // renamed version of your Timer.cs
    public GameObject thankYouImage;    // image to show when timer starts
    public float deliveryDuration = 5f; // custom time for this delivery

    private static GameObject currentlyShownImage = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartDeliveryTimer();
        }
    }

    private void StartDeliveryTimer()
    {
        if (deliveryTimer != null)
        {
            deliveryTimer.SetDurationAndStart((int)deliveryDuration);


        }

        // Hide previous image if another was active
        if (currentlyShownImage != null && currentlyShownImage != thankYouImage)
        {
            currentlyShownImage.SetActive(false);
        }

        // Show new image
        if (thankYouImage != null)
        {
            thankYouImage.SetActive(true);
            currentlyShownImage = thankYouImage;

            // Automatically hide it after the timer duration
            StartCoroutine(HideAfterSeconds(thankYouImage, deliveryDuration));
        }
    }

    private System.Collections.IEnumerator HideAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null)
        {
            obj.SetActive(false);
            if (currentlyShownImage == obj)
                currentlyShownImage = null;
        }
    }
}
