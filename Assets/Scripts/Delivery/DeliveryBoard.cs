using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class DeliveryBoard : MonoBehaviour
{
    // starts delivery, displays collected text, makes haspackage true
    public DeliverySystem deliverySystem;
    public GameObject deliveryUI;

    public Image[] deliveryIcons;
    public TextMeshProUGUI[] deliveryTexts;

    public int randomRoll;
    public int myInt;

    public int[] buttons;

    private void Start()
    {
        deliveryUI = GameObject.Find("DepotUI");
        deliveryUI.SetActive(false);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bike") && deliverySystem.currentPackages < deliverySystem.maxPackages)
        {
            buttons = new int[3] {999,999,999};
            deliveryUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            for (myInt = 0; myInt < 3; myInt++)
            {
                RollDelivery();
            }
        }
        else
        {
            return;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Bike"))
        {
            deliveryUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseDeliveryBoard()
    {
        deliveryUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RollDelivery()
    {
        randomRoll = UnityEngine.Random.Range(0, deliverySystem.Deliveries.Length);
        Debug.Log(randomRoll);
        if (buttons.Contains(randomRoll))
        {
            RollDelivery();

        }
        else
        {
            deliveryIcons[myInt].sprite = deliverySystem.Deliveries[randomRoll].customerIcon;
            buttons[myInt] = randomRoll;
            CompileDeliveryString();
        }
    }

    public void CompileDeliveryString()
    {
        string customerName = deliverySystem.Deliveries[randomRoll].name;
        string customerLocation = deliverySystem.Deliveries[randomRoll].location;
        string customerTime = deliverySystem.Deliveries[randomRoll].deliveryTime.ToString();
        string customerDifficulty = deliverySystem.Deliveries[randomRoll].difficulty;

        string finalString = customerName + " - " + customerLocation + " - Time: " + customerTime + " - " + customerDifficulty;
        deliveryTexts[myInt].text = finalString;
    }
}