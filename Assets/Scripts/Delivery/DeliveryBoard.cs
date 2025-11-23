using UnityEngine;

public class DeliveryBoard : MonoBehaviour
{
    // starts delivery, displays collected text, makes haspackage true
    public DeliverySystem deliverySystem;
    public GameObject deliveryUI;

    private void Start()
    {
        deliveryUI = GameObject.Find("DeliveryCanvas");
        deliveryUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            deliveryUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            deliveryUI.SetActive(false);
        }
    }

    public void DeliveryInitiation()
    {
        int deliveryIndex;
        deliveryIndex = deliverySystem.currentDelivery;
        deliverySystem.StartDelivery(deliveryIndex);
        Debug.Log("THIS MESSAGE IS FROM THE FBI");
    }
}
