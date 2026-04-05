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
    public ParticleSystem rainMainParticleSystem;
    public ParticleSystem rainNearParticleSystem;

    [Header("Current Weather")]
    public WeatherType currentWeather = WeatherType.Sunny;

    [Header("Transition")]
    public float transitionSpeed = 1.0f;
    public float rainTransitionSpeed = 1.2f;
    public float fogTransitionSpeed = 1.0f;

    [Header("Sunny Settings")]
    public float sunnySunMultiplier = 1.0f;
    public float sunnyAmbientMultiplier = 1.0f;
    public float sunnyRainMainRate = 0f;
    public float sunnyRainNearRate = 0f;
    public bool sunnyUseFog = false;
    public float sunnyFogMultiplier = 0f;

    [Header("Cloudy Settings")]
    public float cloudySunMultiplier = 0.75f;
    public float cloudyAmbientMultiplier = 0.9f;
    public float cloudyRainMainRate = 0f;
    public float cloudyRainNearRate = 0f;
    public bool cloudyUseFog = false;
    public float cloudyFogMultiplier = 0.4f;

    [Header("Rainy Settings")]
    public float rainySunMultiplier = 0.6f;
    public float rainyAmbientMultiplier = 0.8f;
    public float rainyRainMainRate = 600f;
    public float rainyRainNearRate = 180f;
    public bool rainyUseFog = true;
    public float rainyFogMultiplier = 1.2f;

    private float targetSunMultiplier;
    private float targetAmbientMultiplier;
    private float targetRainMainRate;
    private float targetRainNearRate;

    private bool targetUseFog;
    private float targetFogMultiplier;
    private float currentFogMultiplier = 0f;

    private void Start()
    {
        UpdateWeatherTargets();

        if (timeOfDayManager != null)
        {
            timeOfDayManager.sunIntensityMultiplier = targetSunMultiplier;
            timeOfDayManager.ambientIntensityMultiplier = targetAmbientMultiplier;
        }

        InitializeRainSystem(rainMainParticleSystem, targetRainMainRate);
        InitializeRainSystem(rainNearParticleSystem, targetRainNearRate);
        InitializeFog();
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

        UpdateRainEmission(rainMainParticleSystem, targetRainMainRate);
        UpdateRainEmission(rainNearParticleSystem, targetRainNearRate);

        UpdateFog();
    }

    private void UpdateWeatherTargets()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny:
                targetSunMultiplier = sunnySunMultiplier;
                targetAmbientMultiplier = sunnyAmbientMultiplier;
                targetRainMainRate = sunnyRainMainRate;
                targetRainNearRate = sunnyRainNearRate;
                targetUseFog = sunnyUseFog;
                targetFogMultiplier = sunnyFogMultiplier;
                break;

            case WeatherType.Cloudy:
                targetSunMultiplier = cloudySunMultiplier;
                targetAmbientMultiplier = cloudyAmbientMultiplier;
                targetRainMainRate = cloudyRainMainRate;
                targetRainNearRate = cloudyRainNearRate;
                targetUseFog = cloudyUseFog;
                targetFogMultiplier = cloudyFogMultiplier;
                break;

            case WeatherType.Rainy:
                targetSunMultiplier = rainySunMultiplier;
                targetAmbientMultiplier = rainyAmbientMultiplier;
                targetRainMainRate = rainyRainMainRate;
                targetRainNearRate = rainyRainNearRate;
                targetUseFog = rainyUseFog;
                targetFogMultiplier = rainyFogMultiplier;
                break;
        }
    }

    private void InitializeRainSystem(ParticleSystem ps, float startRate)
    {
        if (ps == null) return;

        var emission = ps.emission;
        emission.rateOverTime = startRate;

        if (!ps.isPlaying)
            ps.Play();
    }

    private void UpdateRainEmission(ParticleSystem ps, float targetRate)
    {
        if (ps == null) return;

        var emission = ps.emission;
        float currentRate = emission.rateOverTime.constant;

        float newRate = Mathf.Lerp(
            currentRate,
            targetRate,
            Time.deltaTime * rainTransitionSpeed
        );

        emission.rateOverTime = newRate;

        if (!ps.isPlaying)
            ps.Play();
    }

    private void InitializeFog()
    {
        currentFogMultiplier = targetUseFog ? targetFogMultiplier : 0f;

        if (targetUseFog && currentFogMultiplier > 0.001f)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = timeOfDayManager.GetCurrentFogColor();
            RenderSettings.fogDensity = timeOfDayManager.GetCurrentFogDensity() * currentFogMultiplier;
        }
        else
        {
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0f;
        }
    }

    private void UpdateFog()
    {
        float targetFogValue = targetUseFog ? targetFogMultiplier : 0f;

        currentFogMultiplier = Mathf.Lerp(
            currentFogMultiplier,
            targetFogValue,
            Time.deltaTime * fogTransitionSpeed
        );

        if (currentFogMultiplier > 0.001f)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = timeOfDayManager.GetCurrentFogColor();
            RenderSettings.fogDensity = timeOfDayManager.GetCurrentFogDensity() * currentFogMultiplier;
        }
        else
        {
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0f;
        }
    }
}