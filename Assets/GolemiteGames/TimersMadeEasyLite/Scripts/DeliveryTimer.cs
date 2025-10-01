using UnityEngine;
using TMPro;
using System.Collections;

public class DeliveryTimer : MonoBehaviour
{
    public float startTime = 30f;     
    private float currentTime;
    private bool isRunning = false;
    public int maxTime = 10; 

    public TextMeshProUGUI countdownText;

 
    public GameObject associatedImage;

   
    private static GameObject currentlyShownImage = null;

    private void Start()
    {
        ResetTimer();
    }

    public void SetDuration(int seconds)
    {
        maxTime = seconds;
    }


    public void SetDurationAndStart(int seconds)
    {
        SetDuration(seconds);
        StartTimerWithImage();
    }

   
    public void StartTimerWithImage()
    {
        currentTime = maxTime;
        isRunning = true;

       
        if (currentlyShownImage != null && currentlyShownImage != associatedImage)
        {
            currentlyShownImage.SetActive(false);
        }

        
        if (associatedImage != null)
        {
            associatedImage.SetActive(true);
            currentlyShownImage = associatedImage;

           
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

