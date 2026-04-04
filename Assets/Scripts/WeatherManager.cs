using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Rainy
    }

    [Header("References")]
    public TimeOfDayManager timeOfDayManager;
    public ParticleSystem rainParticleSystem;

    [Header("Current Weather")]
    public WeatherType currentWeather = WeatherType.Sunny;

    [Header("Transition")]
    public float transitionSpeed = 1.5f;

    [Header("Sunny Settings")]
    public float sunnySunMultiplier = 1.0f;
    public float sunnyAmbientMultiplier = 1.0f;
    public float sunnyFogMultiplier = 1.0f;

    [Header("Cloudy Settings")]
    public float cloudySunMultiplier = 0.75f;
    public float cloudyAmbientMultiplier = 0.9f;
    public float cloudyFogMultiplier = 1.5f;

    [Header("Rainy Settings")]
    public float rainySunMultiplier = 0.6f;
    public float rainyAmbientMultiplier = 0.8f;
    public float rainyFogMultiplier = 2.0f;

    private float targetSunMultiplier;
    private float targetAmbientMultiplier;
    private float targetFogMultiplier;

    private void Start()
    {
        UpdateWeatherTargets();

        if (timeOfDayManager != null)
        {
            timeOfDayManager.sunIntensityMultiplier = targetSunMultiplier;
            timeOfDayManager.ambientIntensityMultiplier = targetAmbientMultiplier;
            timeOfDayManager.fogDensityMultiplier = targetFogMultiplier;
        }

        UpdateRainStateImmediate();
    }

    private void Update()
    {
        if (timeOfDayManager == null) return;

        UpdateWeatherTargets();

        timeOfDayManager.sunIntensityMultiplier = Mathf.Lerp(
            timeOfDayManager.sunIntensityMultiplier,
            targetSunMultiplier,
            Time.deltaTime * transitionSpeed
        );

        timeOfDayManager.ambientIntensityMultiplier = Mathf.Lerp(
            timeOfDayManager.ambientIntensityMultiplier,
            targetAmbientMultiplier,
            Time.deltaTime * transitionSpeed
        );

        timeOfDayManager.fogDensityMultiplier = Mathf.Lerp(
            timeOfDayManager.fogDensityMultiplier,
            targetFogMultiplier,
            Time.deltaTime * transitionSpeed
        );

        UpdateRainState();
    }

    private void UpdateWeatherTargets()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny:
                targetSunMultiplier = sunnySunMultiplier;
                targetAmbientMultiplier = sunnyAmbientMultiplier;
                targetFogMultiplier = sunnyFogMultiplier;
                break;

            case WeatherType.Cloudy:
                targetSunMultiplier = cloudySunMultiplier;
                targetAmbientMultiplier = cloudyAmbientMultiplier;
                targetFogMultiplier = cloudyFogMultiplier;
                break;

            case WeatherType.Rainy:
                targetSunMultiplier = rainySunMultiplier;
                targetAmbientMultiplier = rainyAmbientMultiplier;
                targetFogMultiplier = rainyFogMultiplier;
                break;
        }
    }

    private void UpdateRainState()
    {
        if (rainParticleSystem == null) return;

        if (currentWeather == WeatherType.Rainy)
        {
            if (!rainParticleSystem.isPlaying)
                rainParticleSystem.Play();
        }
        else
        {
            if (rainParticleSystem.isPlaying)
                rainParticleSystem.Stop();
        }
    }

    private void UpdateRainStateImmediate()
    {
        if (rainParticleSystem == null) return;

        if (currentWeather == WeatherType.Rainy)
        {
            rainParticleSystem.Play();
        }
        else
        {
            rainParticleSystem.Stop();
        }
    }
}