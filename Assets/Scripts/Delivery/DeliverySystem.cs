using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class DeliverySystem : MonoBehaviour
{
    [Header("Delivery Setup")]
    public GameObject[] DeliveryLocations;
    public int currentDelivery = 0;
    public bool hasPackage = false;

    public float[] deliveryTimes;
    private float currentTimer;
    private bool timerRunning = false;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text collectedText;
    public TMP_Text returnText;
    public TMP_Text successText;
    public TMP_Text failText;

    [Header("Customer Voice")]
    [Tooltip("Must match AudioManager customerId exactly. Index must match currentDelivery.")]
    public string[] customerIds;

    [Header("Images")]
    public GameObject successImage;
    public GameObject failImage;
    public GameObject successBackground;
    public GameObject failBackground;

    [Header("Minimap Arrows")]
    public GameObject[] deliveryArrows;
    private GameObject currentArrow;

    [Header("Phones")]
    public GameObject[] phones;
    private GameObject currentPhone;

    [Header("Star System")]
    public int totalStarsCollected = 0;
    public GameObject starIcon;
    public GameObject starIcon1;
    public GameObject starIcon2;
    public GameObject starIcon3;
    public GameObject starIcon4;

    private bool speedBoostUnlockShown = false;

    public TaskManager coinTaskManager;

    void Start()
    {
        if (successImage) successImage.SetActive(false);
        if (failImage) failImage.SetActive(false);

        if (phones != null)
            foreach (var p in phones)
                if (p != null) p.SetActive(false);

        if (deliveryArrows != null)
            foreach (var a in deliveryArrows)
                if (a != null) a.SetActive(false);

        if (timerText) timerText.text = "";

        if (collectedText) collectedText.gameObject.SetActive(false);
        if (returnText) returnText.gameObject.SetActive(false);
        if (successText) successText.gameObject.SetActive(false);
        if (failText) failText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTimer -= Time.deltaTime;
            if (timerText) timerText.text = $"Time left: {Mathf.Ceil(currentTimer)}s";

            if (currentTimer <= 0f)
            {
                timerRunning = false;
                hasPackage = false;
                if (timerText) timerText.text = "";
                if (currentPhone) currentPhone.SetActive(false);
                if (currentArrow) currentArrow.SetActive(false);

                Debug.Log($"? Timer ran out for delivery {currentDelivery}");

                if (failText) StartCoroutine(ShowTMPMessage(failText, "Time ran out! Return to depot.", 5f));
                if (failImage) StartCoroutine(ShowImageForSeconds(failBackground, 5f));
                if (failImage) StartCoroutine(ShowImageForSeconds(failImage, 3f));
            }
        }
    }

    public void GiveStars(int starsGained)
    {
        totalStarsCollected = totalStarsCollected + starsGained;
    }

    public void StartDelivery(int depotIndex)
    {
        currentTimer = deliveryTimes[currentDelivery];
        timerRunning = true;
        hasPackage = true;

        if (timerText) timerText.text = $"Time left: {Mathf.Ceil(currentTimer)}s";
        Debug.Log($"?? Started delivery {currentDelivery} with {currentTimer} seconds");

        if (phones != null && depotIndex < phones.Length)
        {
            if (currentPhone) currentPhone.SetActive(false);
            currentPhone = phones[depotIndex];
            if (currentPhone) currentPhone.SetActive(true);
        }

        int idx = currentDelivery;
        if (deliveryArrows != null && idx < deliveryArrows.Length)
        {
            if (currentArrow) currentArrow.SetActive(false);
            currentArrow = deliveryArrows[idx];
            if (currentArrow) currentArrow.SetActive(true);
        }

        if (coinTaskManager != null)
            coinTaskManager.OnDeliveryStarted();

        TutorialManager.Instance?.DeliveryStarted();
    }

    void CompleteDelivery()
    {
        timerRunning = false;
        hasPackage = false;

        TutorialManager.Instance?.CorrectDeliveryHit();


        if (timerText) timerText.text = "";
        if (currentPhone) currentPhone.SetActive(false);
        if (currentArrow) currentArrow.SetActive(false);

        Debug.Log($"? Delivery {currentDelivery} successful!");

        if (LeaderboardManager.Instance != null)
        {
            float timeTaken = deliveryTimes[currentDelivery] - currentTimer;
            LeaderboardManager.Instance.AddScore(timeTaken);
            Debug.Log("Leaderboard score saved: " + currentTimer);
        }
        else
        {
            Debug.LogWarning("LeaderboardManager not found!");
        }

        if (successText) StartCoroutine(ShowSuccessThenUnlockMessage());
        if (successImage) StartCoroutine(ShowImageForSeconds(successImage, 3f));

        //


        int starsEarned = 0;

        Debug.Log(currentTimer + " TIME LEFT TOTAL");

        if (currentTimer <= deliveryTimes[currentDelivery] / 5f)
        {
            starsEarned = 1;
            GiveStars(1);
            Debug.Log("Gets 1 star.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5f * 1.5f)
        {
            starsEarned = 2;
            GiveStars(2);
            Debug.Log("Gets 2 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5f * 2f)
        {
            starsEarned = 3;
            GiveStars(3);
            Debug.Log("Gets 3 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5f * 3f)
        {
            starsEarned = 4;
            GiveStars(4);
            Debug.Log("Gets 4 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon3, 3f));
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5f * 4f)
        {
            starsEarned = 5;
            GiveStars(5);
            Debug.Log("Gets 5 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon3, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon4, 3f));
        }
        else 
        {
            starsEarned = 5;
            GiveStars(6);
            Debug.Log("Gets 6 stars. Omgosh! (Voice plays as 5-star)");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon3, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon4, 3f));
        }

        Debug.Log(totalStarsCollected);

       
        string customerId = GetCustomerIdFromDeliveryIndex(currentDelivery);
        if (AudioManager.I != null && !string.IsNullOrWhiteSpace(customerId))
        {
            AudioManager.I.PlayCustomerVoice(customerId, starsEarned);
        }
        else
        {
            Debug.LogWarning($"[DeliverySystem] Voice not played. customerId='{customerId}', starsEarned={starsEarned}");
        }
       

        if (coinTaskManager != null)
            coinTaskManager.OnDeliveryFinished();
    }

    public void AddBonusStar()
    {
        GiveStars(1);
    }

    public void ProjectileHitHouse(int houseIndex)
    {
        if (hasPackage && houseIndex == currentDelivery)
        {
            Debug.Log($"?? Projectile delivered to correct house {houseIndex}");
            CompleteDelivery();
        }
    }

    private IEnumerator ShowSuccessThenUnlockMessage()
    {
        if (successBackground) successBackground.SetActive(true);

        yield return StartCoroutine(ShowTMPMessage(
            successText,
            "Delivery successful! Return to depot for another package",
            5f));

        if (!speedBoostUnlockShown && totalStarsCollected >= 10 && returnText != null)
        {
            speedBoostUnlockShown = true;

            yield return StartCoroutine(ShowTMPMessage(
                returnText,
                "You might wanna visit the upgrade shop...",
                5f));
        }

        if (successBackground) successBackground.SetActive(false);
    }

    private IEnumerator ShowTMPMessage(TMP_Text textField, string message, float duration)
    {
        if (!textField) yield break;
        textField.gameObject.SetActive(true);
        textField.text = message;
        yield return new WaitForSeconds(duration);
        textField.text = "";
    }

    private IEnumerator ShowImageForSeconds(GameObject imageObj, float duration)
    {
        if (!imageObj) yield break;
        imageObj.SetActive(true);
        yield return new WaitForSeconds(duration);
        imageObj.SetActive(false);
    }

    private string GetCustomerIdFromDeliveryIndex(int index)
    {
        if (customerIds == null || index < 0 || index >= customerIds.Length)
            return "";

        return customerIds[index];
    }
}

