using UnityEngine;
using System.Collections;

public class CharacterSelectionManager : MonoBehaviour
{
    public CharacterSelector player1;
    public CharacterSelector player2;

    public CanvasGroup selectionCanvasGroup;
    public float fadeDuration = 0.75f;

    private bool hasFaded = false;

    void Update()
    {
        if (hasFaded) return;

        if (player1.IsLockedIn() && player2.IsLockedIn())
        {
            hasFaded = true;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        float time = 0f;
        float startAlpha = selectionCanvasGroup.alpha;

        selectionCanvasGroup.interactable = false;
        selectionCanvasGroup.blocksRaycasts = false;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            selectionCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }

        selectionCanvasGroup.alpha = 0f;
        selectionCanvasGroup.gameObject.SetActive(false);
    }
}
