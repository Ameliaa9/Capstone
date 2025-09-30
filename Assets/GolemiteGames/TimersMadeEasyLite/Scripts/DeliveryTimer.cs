using UnityEngine;
using TMPro;
using System.Collections;

public class DeliveryTimer : MonoBehaviour
{
    public float startTime = 30f;       // Time in seconds
    private float currentTime;
    private bool isRunning = false;
    public int maxTime = 10; // default value if not set

    public TextMeshProUGUI countdownText;

    // NEW: Optional image to show while this timer is active
    public GameObject associatedImage;

    // NEW: Shared static reference to track the current visible image
    private static GameObject currentlyShownImage = null;

    private void Start()
    {
        ResetTimer();
    }

    public void SetDuration(int seconds)
    {
        maxTime = seconds;
    }

    // NEW: Set the time and auto-start the timer with image
    public void SetDurationAndStart(int seconds)
    {
        SetDuration(seconds);
        StartTimerWithImage();
    }

    // NEW: Starts the timer and shows the assigned image
    public void StartTimerWithImage()
    {
        currentTime = maxTime;
        isRunning = true;

        // Turn off any previously shown image
        if (currentlyShownImage != null && currentlyShownImage != associatedImage)
        {
            currentlyShownImage.SetActive(false);
        }

        // Show this image
        if (associatedImage != null)
        {
            associatedImage.SetActive(true);
            currentlyShownImage = associatedImage;

            // Hide it after the timer duration
            StartCoroutine(HideAfterSeconds(associatedImage, maxTime));
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        ResetTimer();
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        UpdateUI();
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            Debug.Log("Timer ended for " + gameObject.name);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (countdownText != null)
        {
            int mins = Mathf.FloorToInt(currentTime / 60f);
            int secs = Mathf.FloorToInt(currentTime % 60f);
            countdownText.text = $"{mins:00}:{secs:00}";
        }
    }

    // NEW: Helper to hide the image after time ends
    private IEnumerator HideAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null)
        {
            obj.SetActive(false);
            if (currentlyShownImage == obj)
                currentlyShownImage = null;
        }
    }
}

