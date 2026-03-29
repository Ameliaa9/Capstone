using UnityEngine;
using UnityEngine.UI;

public class DeliverButtonSwitch : MonoBehaviour
{
    //References
    public DeliverySystem deliverySystem;
    public DeliveryBoard deliveryBoard;

    //Buttons / UI
    public GameObject closeXButton;
    public GameObject deliverButton;
    public Image[] selectedCustomerIcons;
    public GameObject[] selectedCustomerIconObjects;

    //Delivery Settings
    public int depotIndex = 0;

    private int selectedCustomerIndex = -1;

    void OnEnable()
    {
        ClearSelection();
    }

    public void SelectCustomer(int buttonyoujustclicked)
    {
        selectedCustomerIndex = deliveryBoard.buttons[buttonyoujustclicked];

        if (closeXButton != null) closeXButton.SetActive(false);
        if (deliverButton != null) deliverButton.SetActive(true);

        if (selectedCustomerIcons[0] != null)
        {
            selectedCustomerIcons[0].sprite = deliverySystem.Deliveries[selectedCustomerIndex].customerIcon;
            selectedCustomerIcons[0].enabled = true;
        }

        if (selectedCustomerIconObjects != null && selectedCustomerIconObjects.Length > 0 && selectedCustomerIconObjects[0] != null)
        {
            selectedCustomerIconObjects[0].SetActive(true);
        }
    }

    public void ClearSelection()
    {
        selectedCustomerIndex = -1;

        if (closeXButton != null) closeXButton.SetActive(true);
        if (deliverButton != null) deliverButton.SetActive(false);

        if (selectedCustomerIcons != null)
        {
            for (int i = 0; i < selectedCustomerIcons.Length; i++)
            {
                if (selectedCustomerIcons[i] != null)
                {
                    selectedCustomerIcons[i].enabled = false;
                }
            }
        }

        if (selectedCustomerIconObjects != null)
        {
            for (int i = 0; i < selectedCustomerIconObjects.Length; i++)
            {
                if (selectedCustomerIconObjects[i] != null)
                {
                    selectedCustomerIconObjects[i].SetActive(false);
                }
            }
        }
    }

    public void OnPressDeliver()
    {
        if (deliverySystem == null) return;
        if (selectedCustomerIndex < 0) return;

        if (deliverySystem.currentPackages == 1)
        {
            deliverySystem.secondaryDelivery = deliverySystem.currentDelivery;

            deliverySystem.currentDelivery = selectedCustomerIndex;
            deliverySystem.StartDelivery(selectedCustomerIndex);
        }
        else
        {
            deliverySystem.currentDelivery = selectedCustomerIndex;
            deliverySystem.StartDelivery(selectedCustomerIndex);
        }
    }

    public void CloseUI()
    {
        ClearSelection();
        gameObject.SetActive(false);
    }
}