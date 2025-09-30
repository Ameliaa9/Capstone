using UnityEngine;
using System.Collections;

public class DeliverySpot : MonoBehaviour
{
    [Header("Notification UI")]
    public GameObject successPopup; // Assign success popup UI

    [Header("Timer for this Delivery")]
    public DeliveryTimer deliveryTimer;     // Assign the timer GameObject's script

    private bool delivered = false;

    public int houseIndex;
    public void OnPackageHit()
    {
        if (delivered) return;
        delivered = true;

        Debug.Log("Delivery successful at: " + gameObject.name);

        // Show popup
        if (successPopup != null)
        {
            successPopup.SetActive(true);
            StartCoroutine(HideAfterSeconds(successPopup, 5f));
        }

        // Stop this building's timer
        if (deliveryTimer != null)
        {
            deliveryTimer.StopTimer();
        }
    }

    private IEnumerator HideAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null)
            obj.SetActive(false);
    }
}
