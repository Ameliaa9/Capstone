using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("References")]
    public TimeOfDayManager timeOfDayManager;
    public WeatherManager weatherManager;
    public Light streetLight;
    public GameObject mapMarker;

    [Header("Activation Rules")]
    public bool turnOnAtNight = true;
    public bool turnOnWhenRainy = true;

    [Header("Night Time Range")]
    public float nightStartHour = 18f;
    public float nightEndHour = 6f;

    [Header("Base Light Settings")]
    public float offIntensity = 0f;
    public float normalNightIntensity = 112.2f;
    public float rainyDayIntensity = 45f;
    public float rainyNightBonusIntensity = 20f;
    public float transitionSpeed = 2f;

    [Header("Rain Response")]
   
    [Range(0f, 1f)]
    public float rainyDayMinThreshold = 0.1f;
    public bool scaleRainyDayByRainIntensity = true;
    public bool scaleRainyNightBonusByRainIntensity = true;

    [Header("Map Marker Settings")]
    public bool useMapMarker = true;
    public float markerOnThreshold = 0.01f;

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

        streetLight.intensity = Mathf.Lerp(
            streetLight.intensity,
            targetIntensity,
            Time.deltaTime * transitionSpeed
        );

        streetLight.enabled = streetLight.intensity > markerOnThreshold;

        if (useMapMarker && mapMarker != null)
        {
            bool markerShouldBeActive = streetLight.intensity > markerOnThreshold;

            if (mapMarker.activeSelf != markerShouldBeActive)
                mapMarker.SetActive(markerShouldBeActive);
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