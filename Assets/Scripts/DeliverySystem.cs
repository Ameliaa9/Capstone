using UnityEngine;
using TMPro;
using System.Collections;

public class DeliverySystem : MonoBehaviour
{
    [Header("Delivery Setup")]
    public DeliverySpot[] deliverySpots;
    public float[] deliveryTimes;
    public string[] depotTags;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text collectedText;
    public TMP_Text returnText;
    public TMP_Text successText;
    public TMP_Text failText;

    [Header("Images")]
    public GameObject successImage;
    public GameObject failImage;

    [Header("Phones (optional per delivery)")]
    public GameObject[] phones;
    private GameObject currentPhone;

    private int currentDelivery = 0;
    private bool hasPackage = false;
    private bool timerRunning = false;
    private float currentTimer;

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

                if (failText) StartCoroutine(ShowTMPMessage(failText, "Time ran out! Return to depot.", 3f));
                if (failImage) StartCoroutine(ShowImageForSeconds(failImage, 3f));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
        for (int i = 0; i < depotTags.Length; i++)
        {
            if (other.CompareTag(depotTags[i]) && !hasPackage && currentDelivery < deliverySpots.Length)
            {
                Debug.Log($"Depot {i} triggered. Starting delivery {currentDelivery}");
                hasPackage = true;

                if (collectedText) StartCoroutine(ShowTMPMessage(collectedText, "Package collected!", 3f));

                StartDelivery(i);
                return;
            }
        }

        
        if (other.CompareTag("TargetBuilding") && hasPackage && currentDelivery < deliverySpots.Length)
        {
            DeliverySpot spot = other.GetComponent<DeliverySpot>();
            if (spot != null && spot.houseIndex == currentDelivery)
            {
                CompleteDelivery();
            }
        }
    }

    void StartDelivery(int depotIndex)
    {
        currentTimer = deliveryTimes[currentDelivery];
        timerRunning = true;

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

        currentDelivery++;

        if (currentDelivery < deliverySpots.Length)
        {
            if (returnText) StartCoroutine(ShowTMPMessage(returnText, "Return to depot for the next package.", 3f));
        }
        else
        {
            Debug.Log("?? All deliveries complete!");
        }
    }

    
    public void ProjectileHitHouse(int houseIndex)
    {
        if (hasPackage && houseIndex == currentDelivery)
        {
            Debug.Log($"?? Projectile delivered to correct house {houseIndex}");
            CompleteDelivery();
        }
        else
        {
            Debug.Log($"? Projectile hit wrong house {houseIndex}, expecting {currentDelivery}");
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
