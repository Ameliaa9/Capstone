using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageFlashUI : MonoBehaviour
{
    [SerializeField] private Image damageImage;
    [SerializeField] private float maxAlpha = 0.7f;
    [SerializeField] private float fadeInSpeed = 15f;
    [SerializeField] private float fadeOutSpeed = 5f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (damageImage == null)
            damageImage = GetComponent<Image>();

        SetAlpha(0f);
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float alpha = damageImage.color.a;

        while (alpha < maxAlpha)
        {
            alpha += Time.deltaTime * fadeInSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        alpha = maxAlpha;
        SetAlpha(alpha);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeOutSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
        flashRoutine = null;
    }

    private void SetAlpha(float a)
    {
        if (damageImage == null) return;

        Color c = damageImage.color;
        c.a = Mathf.Clamp01(a);
        damageImage.color = c;
    }
}