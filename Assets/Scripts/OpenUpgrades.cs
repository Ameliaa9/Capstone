using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenUpgrades : MonoBehaviour
{
    // Upgrade UI Canvas
    private GameObject upgradeUI;
    // Other Canvases it needs to hide when you open the upgrade shop
    private GameObject canvas1;

    // Scripts the upgrades shop needs to reference to make purchases and give upgrades
    public DeliverySystem deliverySystemScript;
    public BikeMovement bikeScript;

    // The shops upgrade buttons
    public UnityEngine.UI.Button upgrade1;
    public UnityEngine.UI.Button upgrade2;

    //custom cursors
    // public JoystickCursor player1Cursor;
    // public JoystickCursor player2Cursor;

    // Cost in stars for each upgrade type
    public int speedUpgradeCost = 10;      // Stars required to buy a speed upgrade
    public int inventoryUpgradeCost = 25;  // Stars required to buy a package inventory upgrade

    // Upgrade values
    public int speedUpgradeAmount = 5;     // How much to increase bike leg power 
    public int inventoryUpgradeAmount = 1; // How many extra packages the player can carry 

    public GameObject notEnoughStarsPopup;

    // Track whether each upgrade has already been purchased (so it can only be bought once)
    public bool speedUpgradePurchased = false;
    public bool inventoryUpgradePurchased = false;

    public GameObject speedUpgradeOwnedImage;
    public GameObject inventoryUpgradeOwnedImage;

    void Start()
    {
        // Find all the canvases you want to use
        upgradeUI = GameObject.Find("UpgradeCanvas");
        canvas1 = GameObject.Find("Canvas (1)");

        // Turn the uprade UI off until you collide with the shop
        upgradeUI.SetActive(false);

        // Make sure the "not enough stars" popup starts hidden
        if (notEnoughStarsPopup != null)
            notEnoughStarsPopup.SetActive(false);

        // Owned Image Setup
        // If purchased: show OWNED image
        // If not purchased: show the button
        if (speedUpgradeOwnedImage != null)
            speedUpgradeOwnedImage.SetActive(speedUpgradePurchased);
        if (upgrade1 != null)
            upgrade1.gameObject.SetActive(!speedUpgradePurchased);

        if (inventoryUpgradeOwnedImage != null)
            inventoryUpgradeOwnedImage.SetActive(inventoryUpgradePurchased);
        if (upgrade2 != null)
            upgrade2.gameObject.SetActive(!inventoryUpgradePurchased);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Bike") // nothing but the bike can collide to activate the shop
        {
            upgradeUI.SetActive(true);
            canvas1.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Bike")
        {
            upgradeUI.SetActive(false);
            canvas1.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    //close upgrades with x button
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        canvas1.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void UpgradeSpeed(int upgradeAmount) // Upgrade increment can vary
    {
        bikeScript.speed += upgradeAmount;
    }

    public void UpgradePackageInventory(int upgradeAmount)
    {
        deliverySystemScript.maxPackages += upgradeAmount;
    }

    public void BuySpeedUpgrade()
    {
        Debug.Log("BuySpeedUpgrade CALLED");
        Debug.Log("Current stars: " + deliverySystemScript.totalStarsCollected);

        // Stop here if players already bought this upgrade once
        if (speedUpgradePurchased)
            return;

        if (deliverySystemScript.totalStarsCollected >= speedUpgradeCost)
        {
            // subtract stars directly from total stars
            deliverySystemScript.totalStarsCollected -= speedUpgradeCost;
            Debug.Log("Stars after purchase: " + deliverySystemScript.totalStarsCollected);

            // apply speed upgrade
            UpgradeSpeed(speedUpgradeAmount);

            // mark as purchased so it can't be bought again
            speedUpgradePurchased = true;

            // Show OWNED image and hide button
            if (upgrade1 != null)
                upgrade1.gameObject.SetActive(false);
            if (speedUpgradeOwnedImage != null)
                speedUpgradeOwnedImage.SetActive(true);
        }
        else
        {
            ShowNotEnoughStarsPopup();
        }
    }

    // Attempt to buy the package inventory upgrade:
    // If the player has enough stars, subtract the cost and apply the upgrade
    // If not, show the "not enough stars" popup
    public void BuyInventoryUpgrade()
    {
        Debug.Log("BuyInventoryUpgrade CALLED");
        Debug.Log("Current stars: " + deliverySystemScript.totalStarsCollected);

        // Stop here if players already bought this upgrade once
        if (inventoryUpgradePurchased)
            return;

        if (deliverySystemScript.totalStarsCollected >= inventoryUpgradeCost)
        {
            // subtract stars
            deliverySystemScript.totalStarsCollected -= inventoryUpgradeCost;
            Debug.Log("Stars after purchase: " + deliverySystemScript.totalStarsCollected);

            // apply inventory upgrade
            UpgradePackageInventory(inventoryUpgradeAmount);

            // mark as purchased so it can't be bought again
            inventoryUpgradePurchased = true;

            // Show OWNED image and hide button
            if (upgrade2 != null)
                upgrade2.gameObject.SetActive(false);
            if (inventoryUpgradeOwnedImage != null)
                inventoryUpgradeOwnedImage.SetActive(true);
        }
        else
        {
            ShowNotEnoughStarsPopup();
        }
    }

    // turn on not enough stars pop for visual feedback
    public void ShowNotEnoughStarsPopup()
    {
        if (notEnoughStarsPopup != null)
            notEnoughStarsPopup.SetActive(true);
    }

    // Turn off the popup 
    public void CloseNotEnoughStarsPopup()
    {
        if (notEnoughStarsPopup != null)
            notEnoughStarsPopup.SetActive(false);
    }
}