using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class greenSmoke : MonoBehaviour
{
    [Header("UI")]
    public Image smallSmokeImage;
    public Image bigSmokeImage;

    [Header("Health")]
    [SerializeField] private BikeHealth bikeHealth;
    [SerializeField] private int triggerDamagePerTick = 1;
    [SerializeField] private int collisionDamagePerTick = 2;
    [SerializeField] private float tickInterval = 1f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.3f;

    bool inTriggerArea;
    bool inCollision;

    Coroutine damageRoutine;
    Coroutine smallFadeRoutine;
    Coroutine bigFadeRoutine;

   
    private bool tutorialTriggered;

    void Start()
    {
        if (smallSmokeImage != null)
            smallSmokeImage.gameObject.SetActive(false);

        if (bigSmokeImage != null)
            bigSmokeImage.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bike"))
            return;

        inTriggerArea = true;
        UpdateDamageRoutine();
        TryTriggerTutorial();

        if (smallSmokeImage != null)
            StartFade(ref smallFadeRoutine, smallSmokeImage, 1f);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Bike"))
            return;

        inTriggerArea = false;
        UpdateDamageRoutine();

        if (smallSmokeImage != null)
            StartFade(ref smallFadeRoutine, smallSmokeImage, 0f, true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Bike"))
            return;

        inCollision = true;
        UpdateDamageRoutine();
        TryTriggerTutorial();

        if (bigSmokeImage != null)
            StartFade(ref bigFadeRoutine, bigSmokeImage, 1f);
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.CompareTag("Bike"))
            return;

        inCollision = false;
        UpdateDamageRoutine();

        if (bigSmokeImage != null)
            StartFade(ref bigFadeRoutine, bigSmokeImage, 0f, true);
    }

    void TryTriggerTutorial()
    {
        if (tutorialTriggered)
            return;

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ManholeTriggered();
            tutorialTriggered = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Bike"))
            return;

        TutorialManager.Instance?.ManholeTriggered();
    }


    void UpdateDamageRoutine()
    {
        if (inTriggerArea || inCollision)
        {
            if (damageRoutine == null)
                damageRoutine = StartCoroutine(DamageLoop());
        }
        else
        {
            if (damageRoutine != null)
            {
                StopCoroutine(damageRoutine);
                damageRoutine = null;
            }
        }
    }

    IEnumerator DamageLoop()
    {
        while (inTriggerArea || inCollision)
        {
            if (bikeHealth != null)
            {
                int dmg = inCollision ? collisionDamagePerTick : triggerDamagePerTick;
                if (dmg != 0)
                    bikeHealth.SetHealth(-dmg);
            }

            yield return new WaitForSeconds(tickInterval);
        }

        damageRoutine = null;
    }

    void StartFade(ref Coroutine routine, Image img, float targetAlpha, bool disableOnEnd = false)
    {
        if (img == null)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FadeImage(img, targetAlpha, disableOnEnd));
    }

    IEnumerator FadeImage(Image img, float targetAlpha, bool disableOnEnd)
    {
        img.gameObject.SetActive(true);

        Color c = img.color;
        float startAlpha = c.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = fadeDuration > 0f ? time / fadeDuration : 1f;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            img.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        img.color = c;

        if (disableOnEnd && Mathf.Approximately(targetAlpha, 0f))
            img.gameObject.SetActive(false);
    }
}
