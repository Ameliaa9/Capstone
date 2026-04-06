using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Rainy
    }

    public enum TimePeriod
    {
        Day,
        Evening,
        Night
    }

    [Header("References")]
    public TimeOfDayManager timeOfDayManager;
    public ParticleSystem rainMainParticleSystem;
    public ParticleSystem rainNearParticleSystem;
    public ParticleSystem rainSplashBaseParticleSystem;
    public ParticleSystem rainSplashAccentParticleSystem;

    [Header("Current Weather")]
    public WeatherType currentWeather = WeatherType.Sunny;

    [Header("Automatic Weather")]
    public bool useAutomaticWeather = false;
    public Vector2 weatherDurationRange = new Vector2(45f, 90f);

    [Tooltip("If enabled, the system randomizes the starting weather at play start. If disabled, it uses Current Weather.")]
    public bool randomizeInitialWeather = false;

    [Header("Time Weighted Weather")]
    public bool useTimeWeightedWeather = true;

    private float weatherTimer;
    private float currentWeatherDuration;

    [Header("Transition")]
    public float transitionSpeed = 1.0f;
    public float rainTransitionSpeed = 1.2f;
    public float fogTransitionSpeed = 1.0f;

    [Header("Sunny Settings")]
    public float sunnySunMultiplier = 1.0f;
    public float sunnyAmbientMultiplier = 1.0f;
    public float sunnyRainMainRate = 0f;
    public float sunnyRainNearRate = 0f;
    public float sunnyRainSplashBaseRate = 0f;
    public float sunnyRainSplashAccentRate = 0f;
    public bool sunnyUseFog = false;
    public float sunnyFogMultiplier = 0f;

    [Header("Cloudy Settings")]
    public float cloudySunMultiplier = 0.75f;
    public float cloudyAmbientMultiplier = 0.9f;
    public float cloudyRainMainRate = 0f;
    public float cloudyRainNearRate = 0f;
    public float cloudyRainSplashBaseRate = 0f;
    public float cloudyRainSplashAccentRate = 0f;
    public bool cloudyUseFog = false;
    public float cloudyFogMultiplier = 0.4f;

    [Header("Rainy Settings")]
    public float rainySunMultiplier = 0.6f;
    public float rainyAmbientMultiplier = 0.8f;
    public float rainyRainMainRate = 600f;
    public float rainyRainNearRate = 180f;
    public float rainyRainSplashBaseRate = 260f;
    public float rainyRainSplashAccentRate = 50f;
    public bool rainyUseFog = true;
    public float rainyFogMultiplier = 1.2f;

    [Header("Base Automatic Weather Weights")]
    [Range(0f, 1f)] public float sunnyToSunnyChance = 0.35f;

    [Range(0f, 1f)] public float cloudyToSunnyChance = 0.30f;
    [Range(0f, 1f)] public float cloudyToCloudyChance = 0.35f;

    [Range(0f, 1f)] public float rainyToCloudyChance = 0.65f;

    [Header("Time Period Multipliers - Day")]
    public float daySunnyWeight = 1.4f;
    public float dayCloudyWeight = 1.0f;
    public float dayRainyWeight = 0.6f;

    [Header("Time Period Multipliers - Evening")]
    public float eveningSunnyWeight = 0.9f;
    public float eveningCloudyWeight = 1.25f;
    public float eveningRainyWeight = 1.0f;

    [Header("Time Period Multipliers - Night")]
    public float nightSunnyWeight = 0.5f;
    public float nightCloudyWeight = 1.2f;
    public float nightRainyWeight = 1.35f;

    private float targetSunMultiplier;
    private float targetAmbientMultiplier;
    private float targetRainMainRate;
    private float targetRainNearRate;
    private float targetRainSplashBaseRate;
    private float targetRainSplashAccentRate;

    private bool targetUseFog;
    private float targetFogMultiplier;
    private float currentFogMultiplier = 0f;

    private WeatherType previousWeather;
    private bool lastAutomaticWeatherState;

    private void Start()
    {
        if (randomizeInitialWeather)
        {
            currentWeather = (WeatherType)Random.Range(0, System.Enum.GetValues(typeof(WeatherType)).Length);
        }

        previousWeather = currentWeather;
        lastAutomaticWeatherState = useAutomaticWeather;

        UpdateWeatherTargets();

        if (timeOfDayManager != null)
        {
            timeOfDayManager.sunIntensityMultiplier = targetSunMultiplier;
            timeOfDayManager.ambientIntensityMultiplier = targetAmbientMultiplier;
        }

        InitializeRainSystem(rainMainParticleSystem, targetRainMainRate);
        InitializeRainSystem(rainNearParticleSystem, targetRainNearRate);
        InitializeRainSystem(rainSplashBaseParticleSystem, targetRainSplashBaseRate);
        InitializeRainSystem(rainSplashAccentParticleSystem, targetRainSplashAccentRate);

        InitializeFog();
        ResetWeatherTimer();
    }

    private void Update()
    {
        if (timeOfDayManager == null) return;

        HandleModeSwitch();
        HandleManualWeatherChange();
        HandleAutomaticWeather();

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
        UpdateRainEmission(rainSplashBaseParticleSystem, targetRainSplashBaseRate);
        UpdateRainEmission(rainSplashAccentParticleSystem, targetRainSplashAccentRate);

        UpdateFog();
    }

    private void HandleModeSwitch()
    {
        if (lastAutomaticWeatherState != useAutomaticWeather)
        {
            lastAutomaticWeatherState = useAutomaticWeather;
            ResetWeatherTimer();
        }
    }

    private void HandleManualWeatherChange()
    {
        if (!useAutomaticWeather && previousWeather != currentWeather)
        {
            previousWeather = currentWeather;
            ResetWeatherTimer();
        }
    }

    private void HandleAutomaticWeather()
    {
        if (!useAutomaticWeather) return;

        weatherTimer += Time.deltaTime;

        if (weatherTimer >= currentWeatherDuration)
        {
            currentWeather = GetNextWeather(currentWeather);
            previousWeather = currentWeather;
            ResetWeatherTimer();
        }
    }

    private void ResetWeatherTimer()
    {
        weatherTimer = 0f;
        currentWeatherDuration = Random.Range(weatherDurationRange.x, weatherDurationRange.y);
    }

    private WeatherType GetNextWeather(WeatherType current)
    {
        if (!useTimeWeightedWeather)
        {
            return GetBaseNextWeather(current);
        }

        TimePeriod period = GetCurrentTimePeriod();

        switch (current)
        {
            case WeatherType.Sunny:
                {
                    float sunnyWeight;
                    float cloudyWeight;

                    GetWeightedPair(period, out sunnyWeight, out cloudyWeight);

                    return GetWeightedChoice(
                        WeatherType.Sunny, sunnyWeight,
                        WeatherType.Cloudy, cloudyWeight
                    );
                }

            case WeatherType.Cloudy:
                {
                    float sunnyWeight;
                    float cloudyWeight;
                    float rainyWeight;

                    GetWeightedTriple(period, out sunnyWeight, out cloudyWeight, out rainyWeight);

                    return GetWeightedChoice(
                        WeatherType.Sunny, sunnyWeight,
                        WeatherType.Cloudy, cloudyWeight,
                        WeatherType.Rainy, rainyWeight
                    );
                }

            case WeatherType.Rainy:
                {
                    float cloudyWeight;
                    float rainyWeight;

                    GetWeightedRainPair(period, out cloudyWeight, out rainyWeight);

                    return GetWeightedChoice(
                        WeatherType.Cloudy, cloudyWeight,
                        WeatherType.Rainy, rainyWeight
                    );
                }
        }

        return WeatherType.Cloudy;
    }

    private WeatherType GetBaseNextWeather(WeatherType current)
    {
        float roll = Random.value;

        switch (current)
        {
            case WeatherType.Sunny:
                if (roll < sunnyToSunnyChance)
                    return WeatherType.Sunny;
                else
                    return WeatherType.Cloudy;

            case WeatherType.Cloudy:
                if (roll < cloudyToSunnyChance)
                    return WeatherType.Sunny;
                else if (roll < cloudyToSunnyChance + cloudyToCloudyChance)
                    return WeatherType.Cloudy;
                else
                    return WeatherType.Rainy;

            case WeatherType.Rainy:
                if (roll < rainyToCloudyChance)
                    return WeatherType.Cloudy;
                else
                    return WeatherType.Rainy;
        }

        return WeatherType.Cloudy;
    }

    private TimePeriod GetCurrentTimePeriod()
    {
        float hour = timeOfDayManager.currentTime;

        if (hour >= 6f && hour < 16f)
            return TimePeriod.Day;

        if (hour >= 16f && hour < 20f)
            return TimePeriod.Evening;

        return TimePeriod.Night;
    }

    private void GetWeightedPair(TimePeriod period, out float sunnyWeight, out float cloudyWeight)
    {
        float baseSunny = sunnyToSunnyChance;
        float baseCloudy = 1f - sunnyToSunnyChance;

        switch (period)
        {
            case TimePeriod.Day:
                sunnyWeight = baseSunny * daySunnyWeight;
                cloudyWeight = baseCloudy * dayCloudyWeight;
                break;

            case TimePeriod.Evening:
                sunnyWeight = baseSunny * eveningSunnyWeight;
                cloudyWeight = baseCloudy * eveningCloudyWeight;
                break;

            default:
                sunnyWeight = baseSunny * nightSunnyWeight;
                cloudyWeight = baseCloudy * nightCloudyWeight;
                break;
        }
    }

    private void GetWeightedTriple(TimePeriod period, out float sunnyWeight, out float cloudyWeight, out float rainyWeight)
    {
        float baseSunny = cloudyToSunnyChance;
        float baseCloudy = cloudyToCloudyChance;
        float baseRainy = Mathf.Max(0f, 1f - cloudyToSunnyChance - cloudyToCloudyChance);

        switch (period)
        {
            case TimePeriod.Day:
                sunnyWeight = baseSunny * daySunnyWeight;
                cloudyWeight = baseCloudy * dayCloudyWeight;
                rainyWeight = baseRainy * dayRainyWeight;
                break;

            case TimePeriod.Evening:
                sunnyWeight = baseSunny * eveningSunnyWeight;
                cloudyWeight = baseCloudy * eveningCloudyWeight;
                rainyWeight = baseRainy * eveningRainyWeight;
                break;

            default:
                sunnyWeight = baseSunny * nightSunnyWeight;
                cloudyWeight = baseCloudy * nightCloudyWeight;
                rainyWeight = baseRainy * nightRainyWeight;
                break;
        }
    }

    private void GetWeightedRainPair(TimePeriod period, out float cloudyWeight, out float rainyWeight)
    {
        float baseCloudy = rainyToCloudyChance;
        float baseRainy = 1f - rainyToCloudyChance;

        switch (period)
        {
            case TimePeriod.Day:
                cloudyWeight = baseCloudy * dayCloudyWeight;
                rainyWeight = baseRainy * dayRainyWeight;
                break;

            case TimePeriod.Evening:
                cloudyWeight = baseCloudy * eveningCloudyWeight;
                rainyWeight = baseRainy * eveningRainyWeight;
                break;

            default:
                cloudyWeight = baseCloudy * nightCloudyWeight;
                rainyWeight = baseRainy * nightRainyWeight;
                break;
        }
    }

    private WeatherType GetWeightedChoice(
        WeatherType a, float weightA,
        WeatherType b, float weightB)
    {
        float total = weightA + weightB;
        if (total <= 0f) return a;

        float roll = Random.Range(0f, total);

        if (roll < weightA)
            return a;

        return b;
    }

    private WeatherType GetWeightedChoice(
        WeatherType a, float weightA,
        WeatherType b, float weightB,
        WeatherType c, float weightC)
    {
        float total = weightA + weightB + weightC;
        if (total <= 0f) return b;

        float roll = Random.Range(0f, total);

        if (roll < weightA)
            return a;

        roll -= weightA;

        if (roll < weightB)
            return b;

        return c;
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
                targetRainSplashBaseRate = sunnyRainSplashBaseRate;
                targetRainSplashAccentRate = sunnyRainSplashAccentRate;
                targetUseFog = sunnyUseFog;
                targetFogMultiplier = sunnyFogMultiplier;
                break;

            case WeatherType.Cloudy:
                targetSunMultiplier = cloudySunMultiplier;
                targetAmbientMultiplier = cloudyAmbientMultiplier;
                targetRainMainRate = cloudyRainMainRate;
                targetRainNearRate = cloudyRainNearRate;
                targetRainSplashBaseRate = cloudyRainSplashBaseRate;
                targetRainSplashAccentRate = cloudyRainSplashAccentRate;
                targetUseFog = cloudyUseFog;
                targetFogMultiplier = cloudyFogMultiplier;
                break;

            case WeatherType.Rainy:
                targetSunMultiplier = rainySunMultiplier;
                targetAmbientMultiplier = rainyAmbientMultiplier;
                targetRainMainRate = rainyRainMainRate;
                targetRainNearRate = rainyRainNearRate;
                targetRainSplashBaseRate = rainyRainSplashBaseRate;
                targetRainSplashAccentRate = rainyRainSplashAccentRate;
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