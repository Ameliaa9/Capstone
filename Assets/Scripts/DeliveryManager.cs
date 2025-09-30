using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public DeliveryTimer delivery1Timer;
    public DeliveryTimer delivery2Timer;
    public DeliveryTimer delivery3Timer;

    public GameObject startDelivery2Trigger;
    public GameObject startDelivery3Trigger;

    private int currentDelivery = 1;

    void Start()
    {
        delivery1Timer.StartTimer();
    }

    public void CompleteDelivery(int deliveryNumber)
    {
        if (deliveryNumber == 1)
        {
            delivery1Timer.StopTimer();
            startDelivery2Trigger.SetActive(true);
        }
        else if (deliveryNumber == 2)
        {
            delivery2Timer.StopTimer();
            startDelivery3Trigger.SetActive(true);
        }
        else if (deliveryNumber == 3)
        {
            delivery3Timer.StopTimer();
            Debug.Log("All deliveries complete!");
        }
    }

    public void StartNextDelivery(int nextDelivery)
    {
        if (nextDelivery == 2)
        {
            delivery2Timer.StartTimer();
            currentDelivery = 2;
        }
        else if (nextDelivery == 3)
        {
            delivery3Timer.StartTimer();
            currentDelivery = 3;
        }
    }
}
