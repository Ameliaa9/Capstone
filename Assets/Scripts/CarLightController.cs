using UnityEngine;

public class CarLightController : MonoBehaviour
{
    [Header("References")]
    public TimeOfDayManager timeOfDayManager;
    public WeatherManager weatherManager;
    public Light[] headlights;

    [Header("Activation Rules")]
    public bool turnOnAtNight = true;
    public bool turnOnWhenRainy = true;

    [Header("Night Time Range")]
    public float nightStartHour = 18f;
    public float nightEndHour = 6f;

    [Header("Base Light Settings")]
    public float offIntensity = 0f;
    public float normalNightIntensity = 45f;
    public float rainyDayIntensity = 18f;
    public float rainyNightBonusIntensity = 12f;
    public float transitionSpeed = 3f;

    [Header("Rain Response")]
    [Range(0f, 1f)]
    public float rainyDayMinThreshold = 0.1f;

    public bool scaleRainyDayByRainIntensity = true;
    public bool scaleRainyNightBonusByRainIntensity = true;

    [Header("Enable / Disable")]
    public bool disableLightComponentWhenOff = true;
    public float lightEnableThreshold = 0.01f;

    private void Reset()
    {
        if (headlights == null || headlights.Length == 0)
        {
            headlights = GetComponentsInChildren<Light>(true);
        }
    }

    private void Update()
    {
        if (timeOfDayManager == null || weatherManager == null || headlights == null || headlights.Length == 0)
            return;

        bool isNight = IsNightTime(timeOfDayManager.currentTime);
        bool isRainy = weatherManager.currentWeather == WeatherManager.WeatherType.Rainy;

        float rainIntensity = 0f;
        if (isRainy)
        {
            rainIntensity = Mathf.Clamp01(weatherManager.currentRainIntensity);
        }

        float targetIntensity = offIntensity;

        if (turnOnAtNight && isNight)
        {
            targetIntensity = normalNightIntensity;

            if (turnOnWhenRainy && isRainy)
            {
                float rainyNightBonus = rainyNightBonusIntensity;

                if (scaleRainyNightBonusByRainIntensity)
                    rainyNightBonus *= rainIntensity;

                targetIntensity += rainyNightBonus;
            }
        }
        else if (turnOnWhenRainy && isRainy)
        {
            if (rainIntensity >= rainyDayMinThreshold)
            {
                if (scaleRainyDayByRainIntensity)
                {
                    targetIntensity = Mathf.Lerp(offIntensity, rainyDayIntensity, rainIntensity);
                }
                else
                {
                    targetIntensity = rainyDayIntensity;
                }
            }
        }

        UpdateHeadlights(targetIntensity);
    }

    private void UpdateHeadlights(float targetIntensity)
    {
        for (int i = 0; i < headlights.Length; i++)
        {
            Light lightComponent = headlights[i];
            if (lightComponent == null) continue;

            lightComponent.intensity = Mathf.Lerp(
                lightComponent.intensity,
                targetIntensity,
                Time.deltaTime * transitionSpeed
            );

            if (disableLightComponentWhenOff)
            {
                lightComponent.enabled = lightComponent.intensity > lightEnableThreshold;
            }
            else
            {
                lightComponent.enabled = true;
            }
        }
    }

    private bool IsNightTime(float currentHour)
    {
        if (nightStartHour > nightEndHour)
        {
            return currentHour >= nightStartHour || currentHour < nightEndHour;
        }
        else
        {
            return currentHour >= nightStartHour && currentHour < nightEndHour;
        }
    }
}