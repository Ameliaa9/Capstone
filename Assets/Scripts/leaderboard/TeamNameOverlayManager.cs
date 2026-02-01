using UnityEngine;
using System.Collections;

public class TeamNameOverlayManager : MonoBehaviour
{
    public static TeamNameOverlayManager Instance;

    public CanvasGroup characterSelect;
    public CanvasGroup teamName;
    public float fadeTime = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    public void ShowKeyboard()
    {
        teamName.gameObject.SetActive(true);

        teamName.alpha = 0;
        teamName.interactable = true;
        teamName.blocksRaycasts = true;

        StartCoroutine(FadeIn(teamName));
    }

    public void ConfirmName()
    {
        StartCoroutine(FadeOutBoth());
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }
        cg.alpha = 1;
    }

    IEnumerator FadeOutBoth()
    {
        float t = 0;
        float charStart = characterSelect.alpha;

        characterSelect.interactable = false;
        characterSelect.blocksRaycasts = false;
        teamName.interactable = false;
        teamName.blocksRaycasts = false;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            teamName.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            characterSelect.alpha = Mathf.Lerp(charStart, 0, t / fadeTime);
            yield return null;
        }

        teamName.gameObject.SetActive(false);
        characterSelect.gameObject.SetActive(false);
    }
}
