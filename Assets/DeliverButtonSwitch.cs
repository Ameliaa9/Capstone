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
    }

    public void ClearSelection()
    {
        selectedCustomerIndex = -1;

        if (closeXButton != null) closeXButton.SetActive(true);
        if (deliverButton != null) deliverButton.SetActive(false);
    }

    public void OnPressDeliver()
    {
        if (deliverySystem == null) return;
        if (selectedCustomerIndex < 0) return;

        deliverySystem.currentDelivery = selectedCustomerIndex;
        deliverySystem.StartDelivery(selectedCustomerIndex);
    }

    public void CloseUI()
    {
        ClearSelection();
        gameObject.SetActive(false);
    }
}