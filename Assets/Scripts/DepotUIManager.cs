using System.Collections;
using UnityEngine;

public class DepotUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject depotUI;
    public ScreenFader screenFader;
    public DepotEntranceTrigger depotTrigger;

    private bool isTransitioning = false;

    public void EnterDepot()
    {
        if (isTransitioning) return;
        StartCoroutine(EnterDepotRoutine());
    }

    private IEnumerator EnterDepotRoutine()
    {
        isTransitioning = true;

        
        yield return StartCoroutine(screenFader.FadeOut());

        
        depotUI.SetActive(true);

        
        yield return StartCoroutine(screenFader.FadeIn());

        isTransitioning = false;
    }

    public void ExitDepot()
    {
        if (isTransitioning) return;
        StartCoroutine(ExitDepotRoutine());
    }

    private IEnumerator ExitDepotRoutine()
    {
        isTransitioning = true;

        
        yield return StartCoroutine(screenFader.FadeOut());

        
        depotUI.SetActive(false);

       
        yield return StartCoroutine(screenFader.FadeIn());

        if (depotTrigger != null)
        {
            depotTrigger.ResetTrigger();
        }

        isTransitioning = false;
    }
}