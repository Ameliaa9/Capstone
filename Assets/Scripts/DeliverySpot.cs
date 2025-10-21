using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System;

public class DeliverySpot : MonoBehaviour
{
    [Header("Notification UI")]
    public GameObject successPopup;

    [Header("Timer for this Delivery")]
    public DeliveryTimer deliveryTimer;

    [Header("Index")]
    public int houseIndex;

    private bool delivered = false;

    public void OnPackageHit()
    {
        if (delivered) return;
        delivered = true;

        Debug.Log("Delivery successful at: " + gameObject.name);

        if (successPopup != null)
        {
            successPopup.SetActive(true);
            StartCoroutine(HideAfterSeconds(successPopup, 5f));
        }

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
