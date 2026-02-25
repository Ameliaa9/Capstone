using UnityEngine;
using TMPro;

public class MainStarUI : MonoBehaviour
{
    public DeliverySystem deliverySystem;
    public TMP_Text starsTextMain;

    void Update()
    {
        if (deliverySystem == null || starsTextMain == null)
            return;

        starsTextMain.text =
            deliverySystem.totalStarsCollected.ToString();
    }
}