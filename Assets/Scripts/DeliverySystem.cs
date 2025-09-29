using UnityEngine;
using TMPro;
using System.Collections;

public class DeliverySystem : MonoBehaviour
{
    [Header("Delivery Setup")]
    public Transform[] deliveryHouses;
    public Transform depot;
    public float[] deliveryTimes;

    [Header("TMP Messages")]
    public TMP_Text collectedText;
    public TMP_Text returnText;
    public TMP_Text successText;
    public TMP_Text failText;

    [Header("Delivery Timer (ONE text only)")]
    public TMP_Text timerText;

    [Header("Images for Results")]
    public GameObject successImage;
    public GameObject failImage;
    public GameObject thankYouPopup;

    private int currentDelivery = 0;
    private bool hasPackage = false;
    private bool timerRunning = false;
    private float timer;

    void Start()
    {
        if (collectedText == null) Debug.LogWarning("collectedText not assigned!");
        if (returnText == null) Debug.LogWarning("returnText not assigned!");
        if (successText == null) Debug.LogWarning("successText not assigned!");
        if (failText == null) Debug.LogWarning("failText not assigned!");
        if (timerText == null) Debug.LogWarning("timerText not assigned!");

        if (successImage != null) successImage.SetActive(false);
        if (failImage != null) failImage.SetActive(false);
        if (thankYouPopup != null) thankYouPopup.SetActive(false);

        if (timerText != null) timerText.text = "";
    }

    void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = $"Time left: {Mathf.Ceil(timer)}s";

            if (timer <= 0f)
            {
                timerRunning = false;
                hasPackage = false;
                Debug.Log("Time ran out for house " + currentDelivery);

                if (timerText != null)
                    timerText.text = "";

                if (failText != null)
                    StartCoroutine(ShowTMPMessage(failText, "Time ran out! Return to depot.", 3f));

                if (failImage != null)
                    StartCoroutine(ShowImageForSeconds(failImage, 3f));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Depot") && !hasPackage && currentDelivery < deliveryHouses.Length)
        {
            Debug.Log("Triggered depot! Picking up package.");
            hasPackage = true;

            if (collectedText != null)
                StartCoroutine(ShowTMPMessage(collectedText, "Package collected!", 3f));

            StartTimer();
        }

        
        if (other.CompareTag("TargetBuilding") && hasPackage)
        {
            Debug.Log("Entered target building trigger.");
            DeliverySpot spot = other.GetComponent<DeliverySpot>();
            if (spot != null)
            {
                Debug.Log($"House index of this building: {spot.houseIndex}, Current delivery: {currentDelivery}");
                if (spot.houseIndex == currentDelivery)
                {
                    HandleDeliverySuccess();
                }
                else
                {
                    Debug.Log("Wrong house! Cannot deliver here yet.");
                }
            }
            else
            {
                Debug.LogWarning("No DeliverySpot component on target building!");
            }
        }
    }

    void StartTimer()
    {
        if (currentDelivery < deliveryTimes.Length)
        {
            timer = deliveryTimes[currentDelivery];
            timerRunning = true;
            Debug.Log($"Started timer for house {currentDelivery}: {timer} seconds");
        }
    }

    void StopTimer()
    {
        timerRunning = false;
        if (timerText != null) timerText.text = "";
        Debug.Log($"Stopped timer for house {currentDelivery}");
    }

    private void HandleDeliverySuccess()
    {
        StopTimer();
        hasPackage = false;
        Debug.Log("Delivery successful at house " + currentDelivery);

        if (successText != null)
            StartCoroutine(ShowTMPMessage(successText, "Delivery successful!", 3f));

        if (successImage != null)
            StartCoroutine(ShowImageForSeconds(successImage, 3f));

        if (thankYouPopup != null)
            StartCoroutine(ShowImageForSeconds(thankYouPopup, 5f));

        currentDelivery++;

        if (currentDelivery < deliveryHouses.Length)
        {
            if (returnText != null)
                StartCoroutine(ShowTMPMessage(returnText, "Return to depot to pick up the next package.", 3f));
        }
    }

    private IEnumerator ShowTMPMessage(TMP_Text textField, string message, float duration)
    {
        if (textField == null) yield break;

        textField.gameObject.SetActive(true);
        textField.text = message;
        yield return new WaitForSeconds(duration);
        textField.text = "";
    }

    private IEnumerator ShowImageForSeconds(GameObject imageObj, float duration)
    {
        imageObj.SetActive(true);
        yield return new WaitForSeconds(duration);
        imageObj.SetActive(false);
    }

    
    public void DeliverToHouse(int houseIndex)
    {
        if (!hasPackage || !timerRunning) return;

        if (houseIndex == currentDelivery)
        {
            HandleDeliverySuccess();
        }
        else
        {
            Debug.Log("Wrong house hit! Current delivery is " + currentDelivery);
        }
    }
}
