using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;

public class DeliverySystem : MonoBehaviour
{
    [Header("Delivery Setup")]
    public Delivery[] Deliveries; // The delivery scriptable object containing all of the delivery information
    public GameObject[] DeliveryLocations; // The physical delivery locations ( for the Projectile script )

    public int currentDelivery = 0;// Index for the delivery
    public int previousDelivery = 0;// Index for previous delivery
    public int secondaryDelivery = 0;// Index for secondary delivery
    public int maxPackages = 1;
    public int currentPackages = 0; // New variable to count player package amount

    private float currentTimer; // The current max time and the variable time which is changed by the value inside of Delivery scriptable object
    private bool timerRunning = false; // Determines if the timer is running

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text collectedText;
    public TMP_Text returnText;
    public TMP_Text successText;
    public TMP_Text failText;

    [Header("Timer Color Gradient")]
    public Color timerStartColor = Color.black;
    public Color timerEndColor = Color.red;

    [Header("Customer Voice")]
    [Tooltip("Must match AudioManager customerId exactly. Index must match currentDelivery.")]
    public string[] customerIds;

    [Header("Images")]
    public GameObject successImage;
    public GameObject failImage;
    public GameObject successBackground;
    public GameObject failBackground;

    [Header("Popup Animation")]
    public TimerPopupHandler popup;

    [Header("Minimap Arrows")]
    public GameObject[] deliveryArrows;
    private GameObject currentArrow;

    [Header("Phones")]
    public GameObject currentPhone;
    public Image phoneImage;
    public TextMeshProUGUI phoneText;

    public Sprite combinedPhoneImage;
    public string combinedPhoneText;

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
            UpdateTimerDisplay();

            if (currentTimer <= 0f)
            {
                timerRunning = false;
                if (timerText) timerText.text = "";
                if (currentPhone) currentPhone.SetActive(false);
                if (currentArrow) currentArrow.SetActive(false);

                Debug.Log($"? Timer ran out for delivery {currentDelivery}");

                AudioManager.I?.PlayDeliveryMissed(0.8f);

                if (failText) StartCoroutine(ShowTMPMessage(failText, "Time ran out! Return to depot.", 5f));
                if (failImage) StartCoroutine(ShowImageForSeconds(failBackground, 5f));
                if (failImage) StartCoroutine(ShowImageForSeconds(failImage, 5f));
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (!timerText) return;

        timerText.text = $"Time left: {Mathf.Ceil(currentTimer)}s";

        float totalTime = Deliveries[currentDelivery].deliveryTime;
        float t = 1f - (currentTimer / totalTime);
        t = Mathf.Clamp01(t);

        Color baseColor = Color.Lerp(timerStartColor, timerEndColor, t);

        if (currentTimer <= 30f)
        {
            float flash = Mathf.PingPong(Time.time * 5f, 1f);
            timerText.color = Color.Lerp(baseColor, Color.white, flash);
        }
        else
        {
            timerText.color = baseColor;
        }
    }

    public void GiveStars(int starsGained)
    {
        totalStarsCollected = totalStarsCollected + starsGained;
    }

    public void StartDelivery(int depotIndex)
    {
        if (currentPackages < 1)
        {
            currentPackages += 1;
            currentTimer = Deliveries[currentDelivery].deliveryTime;
            timerRunning = true;

            phoneImage.sprite = Deliveries[currentDelivery].customerPhoneIcon;
            phoneText.text = Deliveries[currentDelivery].name;
            currentPhone.SetActive(true);

            AudioManager.I?.PlayPackagePickup();

            UpdateTimerDisplay();
            Debug.Log($"?? Started delivery {currentDelivery} with {currentTimer} seconds");
        }
        else if (currentPackages == 1 && maxPackages == 2)
        {
            currentPackages += 1;
            currentTimer = currentTimer + Deliveries[currentDelivery].deliveryTime;
            timerRunning = true;

            phoneImage.sprite = combinedPhoneImage;
            phoneText.text = combinedPhoneText;

            AudioManager.I?.PlayPackagePickup();

            UpdateTimerDisplay();
            Debug.Log($"?? Started delivery {currentDelivery} with {currentTimer} seconds");
        }
        else
        {
            Debug.Log(" Max package limit reached.");
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

    void CompleteDelivery(int deliveryCompleted)
    {
        
        currentPackages -= 1;
        if (currentPackages == 1 && deliveryCompleted == currentDelivery)
        {
            TutorialManager.Instance?.CorrectDeliveryHit();
            Debug.Log($"? Delivery {currentDelivery} successful!");

            if (popup != null)
            {
                popup.ShowPopup();
            }

            if (LeaderboardManager.Instance != null)
            {
                float timeTaken = Deliveries[currentDelivery].deliveryTime - currentTimer;
                LeaderboardManager.Instance.AddScore(timeTaken);
                Debug.Log("Leaderboard score saved: " + currentTimer);
            }
            else
            {
                Debug.LogWarning("LeaderboardManager not found!");
            }

            if (successText) StartCoroutine(ShowSuccessThenUnlockMessage());
            if (successImage) StartCoroutine(ShowImageForSeconds(successImage, 5f));

            int starsEarned = 0;

            Debug.Log(currentTimer + " TIME LEFT TOTAL");

            if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f)
            {
                starsEarned = 1;
                GiveStars(1);
                Debug.Log("Gets 1 star.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 1.5f)
            {
                starsEarned = 2;
                GiveStars(2);
                Debug.Log("Gets 2 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 2f)
            {
                starsEarned = 3;
                GiveStars(3);
                Debug.Log("Gets 3 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 3f)
            {
                starsEarned = 4;
                GiveStars(4);
                Debug.Log("Gets 4 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 4f)
            {
                starsEarned = 5;
                GiveStars(5);
                Debug.Log("Gets 5 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon4, 5f));
            }
            else
            {
                starsEarned = 5;
                GiveStars(5);
                Debug.Log("Gets 5 stars. Omgosh! (Voice plays as 5-star)");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon4, 5f));
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

            currentDelivery = secondaryDelivery;
            phoneImage.sprite = Deliveries[currentDelivery].customerPhoneIcon;
            phoneText.text = Deliveries[currentDelivery].name;
        }
        else if (currentPackages == 1 && deliveryCompleted == secondaryDelivery)
        {
            TutorialManager.Instance?.CorrectDeliveryHit();
            Debug.Log($"? Delivery {secondaryDelivery} successful!");

            if (popup != null)
            {
                popup.ShowPopup();
            }

            if (LeaderboardManager.Instance != null)
            {
                float timeTaken = Deliveries[secondaryDelivery].deliveryTime - currentTimer;
                LeaderboardManager.Instance.AddScore(timeTaken);
                Debug.Log("Leaderboard score saved: " + currentTimer);
            }
            else
            {
                Debug.LogWarning("LeaderboardManager not found!");
            }

            if (successText) StartCoroutine(ShowSuccessThenUnlockMessage());
            if (successImage) StartCoroutine(ShowImageForSeconds(successImage, 5f));

            int starsEarned = 0;

            Debug.Log(currentTimer + " TIME LEFT TOTAL");

            if (currentTimer <= Deliveries[secondaryDelivery].deliveryTime / 5f)
            {
                starsEarned = 1;
                GiveStars(1);
                Debug.Log("Gets 1 star.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
            }
            else if (currentTimer <= Deliveries[secondaryDelivery].deliveryTime / 5f * 1.5f)
            {
                starsEarned = 2;
                GiveStars(2);
                Debug.Log("Gets 2 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
            }
            else if (currentTimer <= Deliveries[secondaryDelivery].deliveryTime / 5f * 2f)
            {
                starsEarned = 3;
                GiveStars(3);
                Debug.Log("Gets 3 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
            }
            else if (currentTimer <= Deliveries[secondaryDelivery].deliveryTime / 5f * 3f)
            {
                starsEarned = 4;
                GiveStars(4);
                Debug.Log("Gets 4 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
            }
            else if (currentTimer <= Deliveries[secondaryDelivery].deliveryTime / 5f * 4f)
            {
                starsEarned = 5;
                GiveStars(5);
                Debug.Log("Gets 5 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon4, 5f));
            }
            else
            {
                starsEarned = 5;
                GiveStars(5);
                Debug.Log("Gets 5 stars. Omgosh! (Voice plays as 5-star)");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon4, 5f));
            }

            Debug.Log(totalStarsCollected);

            string customerId = GetCustomerIdFromDeliveryIndex(secondaryDelivery);
            if (AudioManager.I != null && !string.IsNullOrWhiteSpace(customerId))
            {
                AudioManager.I.PlayCustomerVoice(customerId, starsEarned);
            }
            else
            {
                Debug.LogWarning($"[DeliverySystem] Voice not played. customerId='{customerId}', starsEarned={starsEarned}");
            }

            secondaryDelivery = -1;
            phoneImage.sprite = Deliveries[currentDelivery].customerPhoneIcon;
            phoneText.text = Deliveries[currentDelivery].name;
        }
        else
        {
            //previousDelivery = currentDelivery;
            timerRunning = false;

            TutorialManager.Instance?.CorrectDeliveryHit();

            if (timerText) timerText.text = "";
            if (currentPhone) currentPhone.SetActive(false);
            if (currentArrow) currentArrow.SetActive(false);

            Debug.Log($"? Delivery {currentDelivery} successful!");

            if (popup != null)
            {
                popup.ShowPopup();
            }

            if (LeaderboardManager.Instance != null)
            {
                float timeTaken = Deliveries[currentDelivery].deliveryTime - currentTimer;
                LeaderboardManager.Instance.AddScore(timeTaken);
                Debug.Log("Leaderboard score saved: " + currentTimer);
            }
            else
            {
                Debug.LogWarning("LeaderboardManager not found!");
            }

            if (successText) StartCoroutine(ShowSuccessThenUnlockMessage());
            if (successImage) StartCoroutine(ShowImageForSeconds(successImage, 5f));

            int starsEarned = 0;

            Debug.Log(currentTimer + " TIME LEFT TOTAL");

            if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f)
            {
                starsEarned = 1;
                GiveStars(1);
                Debug.Log("Gets 1 star.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 1.5f)
            {
                starsEarned = 2;
                GiveStars(2);
                Debug.Log("Gets 2 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 2f)
            {
                starsEarned = 3;
                GiveStars(3);
                Debug.Log("Gets 3 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 3f)
            {
                starsEarned = 4;
                GiveStars(4);
                Debug.Log("Gets 4 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
            }
            else if (currentTimer <= Deliveries[currentDelivery].deliveryTime / 5f * 4f)
            {
                starsEarned = 5;
                GiveStars(5);
                Debug.Log("Gets 5 stars.");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon4, 5f));
            }
            else
            {
                starsEarned = 5;
                GiveStars(5);
                Debug.Log("Gets 5 stars. Omgosh! (Voice plays as 5-star)");
                StartCoroutine(ShowImageForSeconds(starIcon, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon1, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon2, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon3, 5f));
                StartCoroutine(ShowImageForSeconds(starIcon4, 5f));
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
        
    }

    public void AddBonusStar()
    {
        GiveStars(1);
    }

    public void ProjectileHitHouse(int houseIndex)
    {
        if ( currentPackages >= 1)
        {
            Debug.Log($"?? Projectile delivered to correct house {houseIndex}");
            CompleteDelivery(houseIndex);
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

        AudioManager.I?.PlayMenuPopupOpen();

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