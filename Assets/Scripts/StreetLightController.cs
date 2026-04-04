using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("References")]
    public TimeOfDayManager timeOfDayManager;
    public WeatherManager weatherManager;
    public Light streetLight;

    [Header("Activation Rules")]
    public bool turnOnAtNight = true;
    public bool turnOnWhenRainy = true;

    [Header("Night Time Range")]
    public float nightStartHour = 18f;
    public float nightEndHour = 6f;

    [Header("Light Settings")]
    public float offIntensity = 0f;
    public float onIntensity = 112.2f;
    public float transitionSpeed = 2f;

    private void Reset()
    {
        if (streetLight == null)
            streetLight = GetComponentInChildren<Light>();
    }

    private void Update()
    {
        if (timeOfDayManager == null || weatherManager == null || streetLight == null)
            return;

        bool isNight = IsNightTime(timeOfDayManager.currentTime);
        bool isRainy = weatherManager.currentWeather == WeatherManager.WeatherType.Rainy;

        bool shouldBeOn = false;

        if (turnOnAtNight && isNight)
            shouldBeOn = true;

        if (turnOnWhenRainy && isRainy)
            shouldBeOn = true;

        float targetIntensity = shouldBeOn ? onIntensity : offIntensity;

        streetLight.intensity = Mathf.Lerp(
            streetLight.intensity,
            targetIntensity,
            Time.deltaTime * transitionSpeed
        );

        streetLight.enabled = streetLight.intensity > 0.01f;
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