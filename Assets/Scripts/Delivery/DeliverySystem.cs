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
    public GameObject[] deliveryWorldIcons;

    public int currentDelivery = 999;// Index for the delivery
    public int previousDelivery = 0;// Index for previous delivery
    public int secondaryDelivery = 0;// Index for secondary delivery
    public int maxPackages = 1;
    public int currentPackages = 0; // New variable to count player package amount

    private float currentTimer; // The current max time and the variable time which is changed by the value inside of Delivery scriptable object
    private bool timerRunning = false; // Determines if the timer is running

    [Header("Delivery Building Layers")]
    public string normalBuildingLayer = "Default";
    public string activeDeliveryLayer = "deliveryoutline";

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text collectedText;
    public TMP_Text returnText;
    public TMP_Text successText;
    public TMP_Text failText;

    public TextMeshProUGUI[] previousTexts;
    public Image[] previousImages;
    public int previousCount;

    public GameObject[] inventoryPackages;
    public Image[] packageImages;

    [Header("Radio Stuff")]
    public RadioPlayerMP3 rpmp3;
    private bool pitch0Active = true;
    private bool pitch1Active;
    private bool pitch2Active;

    [Header("Timer Color Gradient")]
    public Color timerStartColor = Color.black;
    public Color timerEndColor = Color.red;

    [Header("Customer Voice")]
    [Tooltip("Must match AudioManager customerId exactly. Index must match currentDelivery.")]
    public string[] customerIds;

    [Header("Popup Objects")]
    public GameObject successImage;
    public GameObject failObject;
    public GameObject successBackground;
    public GameObject failBackground;

    public GameObject newPopupObj;

    [Header("Popup Animation")]
    public TimerPopupHandler popup;
    public TimerPopupHandler newPopup;
    public TimerPopupHandler failPopup;

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
    private int starsEarned;

    [Header("5 Star Fireworks")]
    public GameObject fireworkPrefab;
    public Camera celebrationCamera;
    public Vector3 leftFireworkLocalPos = new Vector3(-2.2f, 1.6f, 6f);
    public Vector3 rightFireworkLocalPos = new Vector3(2.2f, 1.6f, 6f);
    public float fireworkDestroyDelay = 4f;

    private bool speedBoostUnlockShown = false;

    public TaskManager coinTaskManager;

    void Start()
    {
        if (successImage) successImage.SetActive(false);
        if (failObject) failObject.SetActive(false);
        if (newPopupObj) newPopupObj.SetActive(false);

        if (deliveryArrows != null)
            foreach (var a in deliveryArrows)
                if (a != null) a.SetActive(false);

        if (deliveryWorldIcons != null)
            foreach (var icon in deliveryWorldIcons)
                if (icon != null) icon.SetActive(false);

        if (timerText) timerText.text = "";

        if (collectedText) collectedText.gameObject.SetActive(false);
        if (returnText) returnText.gameObject.SetActive(false);
        if (successText) successText.gameObject.SetActive(false);
        if (failText) failText.gameObject.SetActive(false);

        inventoryPackages[0].SetActive(false);
        inventoryPackages[1].SetActive(false);

        previousCount = 0;

        if (previousTexts != null)
        {
            for (int i = 0; i < previousTexts.Length; i++)
            {
                if (previousTexts[i] != null)
                    previousTexts[i].text = "";
            }
        }

        if (previousImages != null)
        {
            for (int i = 0; i < previousImages.Length; i++)
            {
                if (previousImages[i] != null)
                    previousImages[i].enabled = false;
            }
        }

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
                if (failObject) StartCoroutine(ShowImageForSeconds(failBackground, 5f));

                failPopup.popupTextName.text = Deliveries[currentDelivery].name;
                failPopup.popupText.text = Deliveries[currentDelivery].dialogue[3];
                failPopup.popupImage.sprite = Deliveries[currentDelivery].customerIcon;
                if (failObject) StartCoroutine(ShowImageForSeconds(failObject, 5f));

                rpmp3.songAudioSource.pitch = 1;
                inventoryPackages[0].SetActive(false);
                inventoryPackages[1].SetActive(false);
                currentPackages = 0;
                UpdateActiveDeliveryLayers();
                UpdateActiveDeliveryWorldIcons();
            }

            MusicSpeedUp();
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

            packageImages[0].sprite = Deliveries[currentDelivery].customerIcon;
            inventoryPackages[0].SetActive(true);

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

            packageImages[0].sprite = Deliveries[currentDelivery].customerIcon;
            inventoryPackages[0].SetActive(true);
            packageImages[1].sprite = Deliveries[secondaryDelivery].customerIcon;
            inventoryPackages[1].SetActive(true);

            currentTimer = currentTimer + Deliveries[secondaryDelivery].deliveryTime;
            timerRunning = true;

            phoneImage.sprite = combinedPhoneImage;
            phoneText.text = combinedPhoneText;

            AudioManager.I?.PlayPackagePickup();

            UpdateTimerDisplay();
            Debug.Log($"?? Started delivery {currentDelivery} with {currentTimer} seconds");

            pitch0Active = true;
            pitch1Active = false;
            pitch2Active = false;
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

        UpdateActiveDeliveryLayers();
        UpdateActiveDeliveryWorldIcons();
    }

    void CompleteDelivery(int deliveryCompleted)
    {
        previousDelivery = deliveryCompleted;
        currentPackages -= 1;

        SetDeliveryBuildingLayer(deliveryCompleted, normalBuildingLayer);
        if (currentPackages == 1 && deliveryCompleted == currentDelivery)
        {
            inventoryPackages[0].SetActive(false);
            TutorialManager.Instance?.CorrectDeliveryHit();
            Debug.Log($"? Delivery {currentDelivery} successful!");

            if (popup != null)
            {
                popup.popupTextName.text = Deliveries[currentDelivery].name;
                popup.popupText.text = Deliveries[currentDelivery].dialogue[4];
                popup.popupImage.sprite = Deliveries[currentDelivery].customerIcon;
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

            TryTriggerFiveStarFireworks();
            CompilePastDeliveryInfo();

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
            UpdateActiveDeliveryLayers();
            UpdateActiveDeliveryWorldIcons();
        }
        else if (currentPackages == 1 && deliveryCompleted == secondaryDelivery)
        {
            inventoryPackages[1].SetActive(false);
            TutorialManager.Instance?.CorrectDeliveryHit();
            Debug.Log($"? Delivery {secondaryDelivery} successful!");

            if (popup != null)
            {
                popup.popupTextName.text = Deliveries[secondaryDelivery].name;
                popup.popupText.text = Deliveries[secondaryDelivery].dialogue[4];
                popup.popupImage.sprite = Deliveries[secondaryDelivery].customerIcon;
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

            TryTriggerFiveStarFireworks();
            CompilePastDeliveryInfo();

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
            UpdateActiveDeliveryLayers();
            UpdateActiveDeliveryWorldIcons();
        }
        else
        {
            rpmp3.songAudioSource.pitch = 1;
            inventoryPackages[0].SetActive(false);
            inventoryPackages[1].SetActive(false);
            timerRunning = false;

            TutorialManager.Instance?.CorrectDeliveryHit();

            if (timerText) timerText.text = "";
            if (currentPhone) currentPhone.SetActive(false);
            if (currentArrow) currentArrow.SetActive(false);

            Debug.Log($"? Delivery {currentDelivery} successful!");

            if (popup != null)
            {
                popup.popupTextName.text = Deliveries[currentDelivery].name;
                popup.popupText.text = Deliveries[currentDelivery].dialogue[4];
                popup.popupImage.sprite = Deliveries[currentDelivery].customerIcon;
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

            TryTriggerFiveStarFireworks();
            CompilePastDeliveryInfo();

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

            UpdateActiveDeliveryLayers();
            UpdateActiveDeliveryWorldIcons();
        }
    }

    public void AddBonusStar()
    {
        GiveStars(1);
        starsEarned = starsEarned + 1;
    }

    public void ProjectileHitHouse(int houseIndex)
    {
        if (currentPackages >= 1)
        {
            Debug.Log($"?? Projectile delivered to correct house {houseIndex}");
            CompleteDelivery(houseIndex);
        }
    }

    public void CompilePastDeliveryInfo()
    {
        float timeElapsedCalculation = MathF.Round(Deliveries[previousDelivery].deliveryTime - currentTimer, 2);
        string customerName = Deliveries[previousDelivery].name;
        string customerLocation = Deliveries[previousDelivery].location;
        string customerTimeElapsed = timeElapsedCalculation.ToString();
        string customerTimeTotal = Deliveries[previousDelivery].deliveryTime.ToString();
        string customerDifficulty = Deliveries[previousDelivery].difficulty;

        string finalString = customerName + " - " + customerLocation + " - Time: " + customerTimeElapsed + " / " + customerTimeTotal + " - " + customerDifficulty + " - Rating: " + starsEarned;

        if (previousCount >= 6)
        {
            previousCount = 0;
            previousTexts[previousCount].text = finalString;
            previousImages[previousCount].sprite = Deliveries[previousDelivery].customerIcon;
            previousImages[previousCount].enabled = true;
        }
        else
        {
            previousTexts[previousCount].text = finalString;
            previousImages[previousCount].sprite = Deliveries[previousDelivery].customerIcon;
            previousImages[previousCount].enabled = true;
            previousCount += 1;
        }
    }

    public void MusicSpeedUp()
    {
        if (currentPackages == 2)
        {
            float mTotalTime = Deliveries[currentDelivery].deliveryTime + Deliveries[secondaryDelivery].deliveryTime;

            if (currentTimer <= mTotalTime / 1.5f && pitch0Active)
            {
                rpmp3.songAudioSource.pitch = 1.1f;
                pitch1Active = true;
                pitch0Active = false;

                CustomerDialogue(0, true);
            }
            else if (currentTimer <= mTotalTime / 2f && pitch1Active)
            {
                rpmp3.songAudioSource.pitch = 1.2f;
                pitch2Active = true;
                pitch1Active = false;

                CustomerDialogue(1, true);
            }
            else if (currentTimer <= mTotalTime / 3f && pitch2Active)
            {
                rpmp3.songAudioSource.pitch = 1.3f;

                CustomerDialogue(2, true);
                pitch2Active = false;
            }
            else if (currentTimer > mTotalTime / 1.5f)
            {
                rpmp3.songAudioSource.pitch = 1f;
            }
            else
            {
                return;
            }
        }
        else if (currentPackages == 1)
        {
            float mTotalTime = Deliveries[currentDelivery].deliveryTime;

            if (currentTimer <= mTotalTime / 1.5f && pitch0Active)
            {
                rpmp3.songAudioSource.pitch = 1.1f;
                pitch1Active = true;
                pitch0Active = false;

                CustomerDialogue(0, false);
            }
            else if (currentTimer <= mTotalTime / 2f && pitch1Active)
            {
                rpmp3.songAudioSource.pitch = 1.2f;
                pitch2Active = true;
                pitch1Active = false;

                CustomerDialogue(1, false);
            }
            else if (currentTimer <= mTotalTime / 3f && pitch2Active)
            {
                rpmp3.songAudioSource.pitch = 1.3f;
                CustomerDialogue(2, false);
                pitch2Active = false;
            }
            else if (currentTimer > mTotalTime / 1.5f)
            {
                rpmp3.songAudioSource.pitch = 1f;
                pitch0Active = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            rpmp3.songAudioSource.pitch = 1;
        }
    }

    public void CustomerDialogue(int dialogueIndex, bool isRandom)
    {
        if (isRandom)
        {
            int coinFlip = UnityEngine.Random.Range(0, 2);
            if (coinFlip == 0)
            {
                newPopup.popupTextName.text = Deliveries[secondaryDelivery].name;
                newPopup.popupText.text = Deliveries[secondaryDelivery].dialogue[dialogueIndex];
                newPopup.popupImage.sprite = Deliveries[secondaryDelivery].customerIcon;
            }
            else
            {
                newPopup.popupTextName.text = Deliveries[currentDelivery].name;
                newPopup.popupText.text = Deliveries[currentDelivery].dialogue[dialogueIndex];
                newPopup.popupImage.sprite = Deliveries[currentDelivery].customerIcon;
            }
        }
        else
        {
            newPopup.popupTextName.text = Deliveries[currentDelivery].name;
            newPopup.popupText.text = Deliveries[currentDelivery].dialogue[dialogueIndex];
            newPopup.popupImage.sprite = Deliveries[currentDelivery].customerIcon;
        }

        newPopup.ShowPopup();
        if (newPopupObj) StartCoroutine(ShowImageForSeconds(newPopupObj, 5f));
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

    private void TriggerFiveStarFireworks()
    {
        if (fireworkPrefab == null || celebrationCamera == null) return;

        GameObject leftFx = Instantiate(fireworkPrefab, celebrationCamera.transform);
        leftFx.transform.localPosition = leftFireworkLocalPos;
        leftFx.transform.localRotation = Quaternion.identity;

        GameObject rightFx = Instantiate(fireworkPrefab, celebrationCamera.transform);
        rightFx.transform.localPosition = rightFireworkLocalPos;
        rightFx.transform.localRotation = Quaternion.identity;

        foreach (ParticleSystem ps in leftFx.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        foreach (ParticleSystem ps in rightFx.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        Destroy(leftFx, fireworkDestroyDelay);
        Destroy(rightFx, fireworkDestroyDelay);
    }

    private void TryTriggerFiveStarFireworks()
    {
        if (starsEarned >= 5)
        {
            TriggerFiveStarFireworks();
        }
    }

    private string GetCustomerIdFromDeliveryIndex(int index)
    {
        if (customerIds == null || index < 0 || index >= customerIds.Length)
            return "";

        return customerIds[index];
    }

    void SetDeliveryBuildingLayer(int deliveryIndex, string layerName)
    {
        if (deliveryIndex < 0 || deliveryIndex >= DeliveryLocations.Length) return;
        if (DeliveryLocations[deliveryIndex] == null) return;

        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning("Layer not found: " + layerName);
            return;
        }

        SetLayerRecursively(DeliveryLocations[deliveryIndex], layer);
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void UpdateActiveDeliveryLayers()
    {
        for (int i = 0; i < DeliveryLocations.Length; i++)
        {
            SetDeliveryBuildingLayer(i, normalBuildingLayer);
        }

        if (currentPackages >= 1 && currentDelivery >= 0 && currentDelivery < DeliveryLocations.Length)
        {
            SetDeliveryBuildingLayer(currentDelivery, activeDeliveryLayer);
        }

        if (currentPackages >= 2 && secondaryDelivery >= 0 && secondaryDelivery < DeliveryLocations.Length)
        {
            SetDeliveryBuildingLayer(secondaryDelivery, activeDeliveryLayer);
        }
    }

    void UpdateActiveDeliveryWorldIcons()
    {
        if (deliveryWorldIcons == null) return;

        for (int i = 0; i < deliveryWorldIcons.Length; i++)
        {
            if (deliveryWorldIcons[i] != null)
                deliveryWorldIcons[i].SetActive(false);
        }

        if (currentPackages >= 1 && currentDelivery >= 0 && currentDelivery < deliveryWorldIcons.Length)
        {
            if (deliveryWorldIcons[currentDelivery] != null)
                deliveryWorldIcons[currentDelivery].SetActive(true);
        }

        if (currentPackages >= 2 && secondaryDelivery >= 0 && secondaryDelivery < deliveryWorldIcons.Length)
        {
            if (deliveryWorldIcons[secondaryDelivery] != null)
                deliveryWorldIcons[secondaryDelivery].SetActive(true);
        }
    }
}