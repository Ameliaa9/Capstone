using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.Threading.Tasks;
using System.Threading;

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

    [Header("Images")]
    public GameObject successImage;
    public GameObject failImage;

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

    void Start()
    {
     
        if (successImage) successImage.SetActive(false);
        if (failImage) failImage.SetActive(false);

        if (phones != null)
            foreach (var p in phones)
                if (p != null) p.SetActive(false);

        if (timerText) timerText.text = "";
        if (collectedText) collectedText.text = "";
        if (returnText) returnText.text = "";
        if (successText) successText.text = "";
        if (failText) failText.text = "";
        
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

                Debug.Log($"? Timer ran out for delivery {currentDelivery}");

                if (failText) StartCoroutine(ShowTMPMessage(failText, "Time ran out! Return to depot.", 7f));
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
    }

    void CompleteDelivery()
    {
        timerRunning = false;
        hasPackage = false;

        if (timerText) timerText.text = "";
        if (currentPhone) currentPhone.SetActive(false);

        Debug.Log($"? Delivery {currentDelivery} successful!");

        if (successText) StartCoroutine(ShowTMPMessage(successText, "Delivery successful!", 3f));
        if (successImage) StartCoroutine(ShowImageForSeconds(successImage, 3f));

        Debug.Log(currentTimer + " TIME LEFT TOTAL");
        if (currentTimer <= deliveryTimes[currentDelivery] / 5)
        {
            GiveStars(1);
            Debug.Log("Gets 1 star.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            Debug.Log(totalStarsCollected);
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5 * 1.5)
        {
            GiveStars(2);
            Debug.Log("Gets 2 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            Debug.Log(totalStarsCollected);
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5 * 2)
        {
            GiveStars(3);
            Debug.Log("Gets 3 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            Debug.Log(totalStarsCollected);
        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5 * 3)
        {
            GiveStars(4);
            Debug.Log("Gets 4 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon3, 3f));
            Debug.Log(totalStarsCollected);

        }
        else if (currentTimer <= deliveryTimes[currentDelivery] / 5 * 4)
        {
            GiveStars(5);
            Debug.Log("Gets 5 stars.");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon3, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon4, 3f));
            Debug.Log(totalStarsCollected);

        }
        else if (currentTimer > deliveryTimes[currentDelivery] / 5 * 4)
        {
            GiveStars(6);
            Debug.Log("Gets 6 stars.  Omgosh!");
            StartCoroutine(ShowImageForSeconds(starIcon, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon1, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon2, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon3, 3f));
            StartCoroutine(ShowImageForSeconds(starIcon4, 3f));
            Debug.Log(totalStarsCollected);
        }
        else
        {
            Debug.Log("Gets 0 stars.");
            Debug.Log(totalStarsCollected);
        }

        //currentDelivery++;
    }

    
    public void ProjectileHitHouse(int houseIndex)
    {
        if (hasPackage && houseIndex == currentDelivery)
        {
            Debug.Log($"?? Projectile delivered to correct house {houseIndex}");
            CompleteDelivery();
        }
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
}
