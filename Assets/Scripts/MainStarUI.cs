using UnityEngine;
using TMPro;

public class MainStarUI : MonoBehaviour
{
    [Header("References")]
    public DeliverySystem deliverySystem;   
    public OpenUpgrades upgradeShop;       
    public TMP_Text starsTextMain;         

    void Update()
    {
        if (deliverySystem == null || starsTextMain == null)
            return;

        // Start with all stars ever earned
        int currentStars = deliverySystem.totalStarsCollected;

        // Subtract stars that have been spent in the upgrade shop 
        if (upgradeShop != null)
        {
            currentStars -= upgradeShop.starsSpent;
        }

        if (currentStars < 0)
            currentStars = 0;

        starsTextMain.text = currentStars.ToString();
    }
}
