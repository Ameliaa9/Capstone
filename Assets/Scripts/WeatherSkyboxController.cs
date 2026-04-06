using UnityEngine;

public class WeatherSkyboxController : MonoBehaviour
{
    [Header("References")]
    public WeatherManager weatherManager;

    [Header("Transition")]
    public float transitionSpeed = 1.0f;

    [Header("Sunny Skybox Settings")]
    public Color sunnyTint = Color.white;
    public float sunnyExposure = 1.3f;
    public float sunnyAtmosphereThickness = 1.0f;
    public Color sunnySkyTint = new Color(0.5f, 0.6f, 0.8f);
    public Color sunnyGroundColor = new Color(0.37f, 0.35f, 0.34f);

    [Header("Cloudy Skybox Settings")]
    public Color cloudyTint = new Color(0.82f, 0.84f, 0.88f);
    public float cloudyExposure = 0.9f;
    public float cloudyAtmosphereThickness = 1.4f;
    public Color cloudySkyTint = new Color(0.42f, 0.46f, 0.52f);
    public Color cloudyGroundColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("Rainy Skybox Settings")]
    public Color rainyTint = new Color(0.72f, 0.76f, 0.82f);
    public float rainyExposure = 0.75f;
    public float rainyAtmosphereThickness = 1.6f;
    public Color rainySkyTint = new Color(0.32f, 0.36f, 0.42f);
    public Color rainyGroundColor = new Color(0.25f, 0.25f, 0.25f);

    private Material runtimeSkyboxMaterial;

    private static readonly int TintId = Shader.PropertyToID("_Tint");
    private static readonly int ExposureId = Shader.PropertyToID("_Exposure");
    private static readonly int AtmosphereThicknessId = Shader.PropertyToID("_AtmosphereThickness");
    private static readonly int SkyTintId = Shader.PropertyToID("_SkyTint");
    private static readonly int GroundColorId = Shader.PropertyToID("_GroundColor");

    private void Start()
    {
        if (RenderSettings.skybox != null)
        {
            runtimeSkyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = runtimeSkyboxMaterial;
        }
    }

    private void Update()
    {
        if (weatherManager == null || runtimeSkyboxMaterial == null)
            return;

        GetTargetSkyboxValues(
            weatherManager.currentWeather,
            out Color targetTint,
            out float targetExposure,
            out float targetAtmosphereThickness,
            out Color targetSkyTint,
            out Color targetGroundColor
        );

        UpdateSkyboxColor(TintId, targetTint);
        UpdateSkyboxFloat(ExposureId, targetExposure);
        UpdateSkyboxFloat(AtmosphereThicknessId, targetAtmosphereThickness);
        UpdateSkyboxColor(SkyTintId, targetSkyTint);
        UpdateSkyboxColor(GroundColorId, targetGroundColor);

        DynamicGI.UpdateEnvironment();
    }

    private void GetTargetSkyboxValues(
        WeatherManager.WeatherType weather,
        out Color tint,
        out float exposure,
        out float atmosphereThickness,
        out Color skyTint,
        out Color groundColor)
    {
        switch (weather)
        {
            case WeatherManager.WeatherType.Sunny:
                tint = sunnyTint;
                exposure = sunnyExposure;
                atmosphereThickness = sunnyAtmosphereThickness;
                skyTint = sunnySkyTint;
                groundColor = sunnyGroundColor;
                break;

            case WeatherManager.WeatherType.Cloudy:
                tint = cloudyTint;
                exposure = cloudyExposure;
                atmosphereThickness = cloudyAtmosphereThickness;
                skyTint = cloudySkyTint;
                groundColor = cloudyGroundColor;
                break;

            case WeatherManager.WeatherType.Rainy:
                tint = rainyTint;
                exposure = rainyExposure;
                atmosphereThickness = rainyAtmosphereThickness;
                skyTint = rainySkyTint;
                groundColor = rainyGroundColor;
                break;

            default:
                tint = sunnyTint;
                exposure = sunnyExposure;
                atmosphereThickness = sunnyAtmosphereThickness;
                skyTint = sunnySkyTint;
                groundColor = sunnyGroundColor;
                break;
        }
    }

    private void UpdateSkyboxColor(int propertyId, Color targetColor)
    {
        if (!runtimeSkyboxMaterial.HasProperty(propertyId))
            return;

        Color current = runtimeSkyboxMaterial.GetColor(propertyId);
        Color next = Color.Lerp(current, targetColor, Time.deltaTime * transitionSpeed);
        runtimeSkyboxMaterial.SetColor(propertyId, next);
    }

    private void UpdateSkyboxFloat(int propertyId, float targetValue)
    {
        if (!runtimeSkyboxMaterial.HasProperty(propertyId))
            return;

        float current = runtimeSkyboxMaterial.GetFloat(propertyId);
        float next = Mathf.Lerp(current, targetValue, Time.deltaTime * transitionSpeed);
        runtimeSkyboxMaterial.SetFloat(propertyId, next);
    }
}