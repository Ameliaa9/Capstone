using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class OpenUpgrades : MonoBehaviour
{
    private GameObject upgradeUI;
    private GameObject canvas1;
    public DeliverySystem deliverySystemScript;
    public int starWalletAmount;
    public TextMeshProUGUI starWalletText;
    public UnityEngine.UI.Button upgrade1;

    void Start()
    {
        upgradeUI = GameObject.Find("UpgradeCanvas");
        upgradeUI.SetActive(false);
        canvas1 = GameObject.Find("Canvas (1)");
        starWalletAmount = deliverySystemScript.totalStarsCollected;
        starWalletText.text = ("= " + starWalletAmount.ToString());
        


    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Bike")
        {
            upgradeUI.SetActive(true);
            canvas1.SetActive(false);

            
            starWalletAmount = deliverySystemScript.totalStarsCollected;
            starWalletText.text = ("= " + starWalletAmount.ToString());
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Bike")
        {
            upgradeUI.SetActive(false);
            canvas1.SetActive(true);
        }
    }

    public void PurchaseUpgrade()
    {
        deliverySystemScript.totalStarsCollected -= 10000;
        starWalletAmount = deliverySystemScript.totalStarsCollected;
        starWalletText.text = ("= " + starWalletAmount.ToString());
    }

}
