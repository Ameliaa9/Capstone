using System.Collections;
using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public GameObject taskPanel;
    public TextMeshProUGUI counterText;
    public GameObject bonusStarPopup;
    public int targetCoins = 10;
    public float bonusPopupDuration = 3f;
    public DeliverySystem deliverySystem;

    int currentCoins;
    bool taskActive;
    bool bonusEarned;

    public void OnDeliveryStarted()
    {
        taskActive = true;
        bonusEarned = false;
        currentCoins = 0;

        if (taskPanel != null)
            taskPanel.SetActive(true);

        if (bonusStarPopup != null)
            bonusStarPopup.SetActive(false);

        UpdateUI();
    }

    public void OnCoinCollected()
    {
        if (!taskActive)
            return;

        if (currentCoins >= targetCoins)
            return;

        currentCoins++;

        if (currentCoins >= targetCoins)
            bonusEarned = true;

        UpdateUI();
    }

    public void OnDeliveryFinished()
    {
        taskActive = false;

        if (taskPanel != null)
            taskPanel.SetActive(false);

        if (bonusEarned)
        {
            if (deliverySystem != null)
                deliverySystem.AddBonusStar();

            if (bonusStarPopup != null)
            {
                bonusStarPopup.SetActive(true);
                StartCoroutine(HideBonusPopup());
            }
        }
    }

    void UpdateUI()
    {
        if (counterText != null)
            counterText.text = currentCoins.ToString() + "/" + targetCoins.ToString();
    }

    IEnumerator HideBonusPopup()
    {
        yield return new WaitForSeconds(bonusPopupDuration);

        if (bonusStarPopup != null)
            bonusStarPopup.SetActive(false);
    }
}
