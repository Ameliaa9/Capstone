using System.Collections;
using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public enum TaskType
    {
        CollectCoins,        // existing: collect X coins
        NoHazardHit,         // dont get hit by any hazards
        HitTargets,          // hit 5 targets
        NoEnlargedMap,       // no opening enlarged map
        VisitLandmark        // visit a landmark
    }

    [Header("UI")]
    public GameObject taskPanel;
    public TextMeshProUGUI counterText;          // task processing
    public TextMeshProUGUI taskTitleText;        // task title
    public GameObject bonusStarPopup;

    [Header("Settings")]
    public float bonusPopupDuration = 3f;
    public DeliverySystem deliverySystem;

    [Header("Task Params")]
    public int targetCoins = 10;     // CollectCoins
    public int targetHits = 5;       // HitTargets

    // runtime state
    public TaskType currentTask;
    bool taskActive;
    bool bonusEarned;

    // CollectCoins
    int currentCoins;

    // NoHazardHit
    bool gotHitByHazard;

    // HitTargets
    int currentHits;

    // NoEnlargedMap
    bool openedLargeMap;

    // VisitLandmark
    bool visitedLandmark;

    public void OnDeliveryStarted()
    {
        taskActive = true;
        bonusEarned = false;

        // randomy pick one task out of five
        currentTask = (TaskType)Random.Range(0, System.Enum.GetValues(typeof(TaskType)).Length);

        
        currentCoins = 0;
        currentHits = 0;
        gotHitByHazard = false;
        openedLargeMap = false;
        visitedLandmark = false;

        if (taskPanel != null)
            taskPanel.SetActive(true);

        if (bonusStarPopup != null)
            bonusStarPopup.SetActive(false);

        UpdateUI();
    }

    //

    public void OnCoinCollected()
    {
        if (!taskActive) return;
        if (currentTask != TaskType.CollectCoins) return;
        if (bonusEarned) return;

        if (currentCoins >= targetCoins) return;

        currentCoins++;
        if (currentCoins >= targetCoins)
            bonusEarned = true;

        UpdateUI();
    }

    public void OnHazardHit()
    {
        if (!taskActive) return;
        if (currentTask != TaskType.NoHazardHit) return;

        gotHitByHazard = true;
        
        UpdateUI();
    }

    public void OnTargetHit()
    {
        if (!taskActive) return;
        if (currentTask != TaskType.HitTargets) return;
        if (bonusEarned) return;

        currentHits++;
        if (currentHits >= targetHits)
            bonusEarned = true;

        UpdateUI();
    }

    public void OnMapOpened()
    {
        if (!taskActive) return;
        if (currentTask != TaskType.NoEnlargedMap) return;

        openedLargeMap = true;
        UpdateUI();
    }

    public void OnLandmarkVisited()
    {
        if (!taskActive) return;
        if (currentTask != TaskType.VisitLandmark) return;
        if (bonusEarned) return;

        visitedLandmark = true;
        bonusEarned = true;

        UpdateUI();
    }

    
    // Delivery End
    

    public void OnDeliveryFinished()
    {
        if (!taskActive) return;

        
        if (currentTask == TaskType.NoHazardHit)
        {
            // not hazard hit
            bonusEarned = !gotHitByHazard;
        }
        else if (currentTask == TaskType.NoEnlargedMap)
        {
            bonusEarned = !openedLargeMap;
        }

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
        
        if (taskTitleText != null)
            taskTitleText.text = GetTaskTitle(currentTask);

        if (counterText == null) return;

        switch (currentTask)
        {
            case TaskType.CollectCoins:
                counterText.text = $"{currentCoins}/{targetCoins}";
                break;

            case TaskType.NoHazardHit:
                counterText.text = gotHitByHazard ? "Failed (Hit!)" : "0 Hits";
                break;

            case TaskType.HitTargets:
                counterText.text = $"{currentHits}/{targetHits}";
                break;

            case TaskType.NoEnlargedMap:
                counterText.text = openedLargeMap ? "Failed (Map Opened)" : "Map Not Opened";
                break;

            case TaskType.VisitLandmark:
                counterText.text = visitedLandmark ? "Visited!" : "Not Visited Yet";
                break;
        }
    }

    string GetTaskTitle(TaskType type)
    {
        switch (type)
        {
            case TaskType.CollectCoins: return $"Collect {targetCoins} Coins";
            case TaskType.NoHazardHit: return "No Hazard Hit";
            case TaskType.HitTargets: return $"Hit {targetHits} Targets";
            case TaskType.NoEnlargedMap: return "Don't Open Big Map";
            case TaskType.VisitLandmark: return "Visit a Landmark";
        }
        return "Task";
    }

    IEnumerator HideBonusPopup()
    {
        yield return new WaitForSeconds(bonusPopupDuration);

        if (bonusStarPopup != null)
            bonusStarPopup.SetActive(false);
    }
}