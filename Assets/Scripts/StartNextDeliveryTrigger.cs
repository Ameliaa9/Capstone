using UnityEngine;

public class StartNextDeliveryTrigger : MonoBehaviour
{
    public DeliveryTimer deliveryTimer; 
    public GameObject thankYouImage;    
    public float deliveryDuration = 5f; 

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

        
        if (currentlyShownImage != null && currentlyShownImage != thankYouImage)
        {
            currentlyShownImage.SetActive(false);
        }

        
        if (thankYouImage != null)
        {
            thankYouImage.SetActive(true);
            currentlyShownImage = thankYouImage;

            
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
