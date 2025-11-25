using KikiNgao.SimpleBikeControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class oldOpenUpgrades : MonoBehaviour
{
    // Upgrade UI Canvas
    private GameObject upgradeUI;
    // Other Canvases it needs to hide when you open the upgrade shop
    private GameObject canvas1;

    // Scripts the upgrades shop needs to reference to make purchases and give upgrades
    public DeliverySystem deliverySystemScript;
    public SimpleBike bikeScript;

    // Your personal star wallet ( separate from total amount of stars collected )
    public int starWalletAmount;
    public TextMeshProUGUI starWalletText;
    public int starsSpent;

    // The shops upgrade buttons
    public UnityEngine.UI.Button upgrade1;
    public UnityEngine.UI.Button upgrade2;

    //custom cursors
    public JoystickCursor player1Cursor;
    public JoystickCursor player2Cursor;

    void Start()
    {
        // Find all the canvases you want to use
        upgradeUI = GameObject.Find("UpgradeCanvas");
        canvas1 = GameObject.Find("Canvas (1)");

        // Turn the uprade UI off until you collide with the shop
        upgradeUI.SetActive(false);

        // Initialize the star wallet with 0 stars
        starWalletAmount = deliverySystemScript.totalStarsCollected;
        starWalletText.text = ("= " + starWalletAmount.ToString());
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Bike") // nothing but the bike can collide to activate the shop
        {
            upgradeUI.SetActive(true);
            canvas1.SetActive(false);

            // Re-initialize so recently collected stars are added to wallet before entry
            starWalletAmount = deliverySystemScript.totalStarsCollected - starsSpent;
            starWalletText.text = ("= " + starWalletAmount.ToString());

            GameManager.UnlockCursor(); // THANK YOU FOR UNLOCKING THE CURSOR =)

            if (player1Cursor) player1Cursor.gameObject.SetActive(true);
            if (player2Cursor) player2Cursor.gameObject.SetActive(true);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Bike")
        {
            upgradeUI.SetActive(false);
            canvas1.SetActive(true);

            //hide custom cursors
            if (player1Cursor) player1Cursor.gameObject.SetActive(false);
            if (player2Cursor) player2Cursor.gameObject.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            GameManager.LockCursor();
        }
    }

    //close upgrades with x button
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        canvas1.SetActive(true);

        //hide custom cursors
        if (player1Cursor) player1Cursor.gameObject.SetActive(false);
        if (player2Cursor) player2Cursor.gameObject.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.LockCursor();
    }

    public void PurchaseUpgrade(int cost) // Cost per upgrade can vary
    {
        starWalletAmount -= cost;
        starsSpent += cost;
        starWalletText.text = ("= " + starWalletAmount.ToString());
    }

    public void UpgradeSpeed(int upgradeAmount) // Upgrade increment can vary
    {
        bikeScript.legPower += upgradeAmount;
    }

    public void UpgradePackageInventory(int upgradeAmount)
    {
        //something.packageInventory += upgradeAmount;
    }
}
