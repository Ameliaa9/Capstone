using UnityEngine;

public class DeliveryBoard : MonoBehaviour
{
    // starts delivery, displays collected text, makes haspackage true
    public DeliverySystem deliverySystem;
    public GameObject deliveryUI;
    private GameObject canvas1;

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

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            deliveryUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseDeliveryBoard()
    {
        deliveryUI.SetActive(false);
        canvas1.SetActive(true);

        
    }


    public void DeliveryInitiation(int deliveryIndex)
    {
        if (deliverySystem.hasPackage)
        {
            Debug.Log("Delivery can't be activated twice.");
        }
        else
        {
            deliverySystem.currentDelivery = deliveryIndex;
            deliverySystem.StartDelivery(deliveryIndex);
            
        }

    }
}
