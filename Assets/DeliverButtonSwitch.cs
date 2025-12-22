using UnityEngine;

public class DeliverButtonSwitch : MonoBehaviour
{
    //References
    public DeliverySystem deliverySystem;

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

    public void SelectCustomer(int customerIndex)
    {
        selectedCustomerIndex = customerIndex;

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
        deliverySystem.StartDelivery(depotIndex);
    }

    public void CloseUI()
    {
        ClearSelection();
        gameObject.SetActive(false);
    }
}