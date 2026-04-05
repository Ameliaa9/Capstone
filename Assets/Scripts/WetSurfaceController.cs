using UnityEngine;

public class WetSurfaceController : MonoBehaviour
{
    [Header("References")]
    public WeatherManager weatherManager;

    [Header("Road Materials")]
    public Material mainRoadMaterial;   // Material.001 13
    public Material sideRoadMaterial;   // Material.002 13

    [Header("Dry Colors")]
    public Color mainRoadDryColor = Color.white;
    public Color sideRoadDryColor = Color.white;

    [Header("Wet Colors")]
    public Color mainRoadWetColor = new Color(0.22f, 0.22f, 0.22f);
    public Color sideRoadWetColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("Smoothness")]
    public float mainRoadDrySmoothness = 0.1f;
    public float mainRoadWetSmoothness = 0.68f;

    public float sideRoadDrySmoothness = 0.1f;
    public float sideRoadWetSmoothness = 0.5f;

    [Header("Transition")]
    public float wetTransitionDuration = 10f;

    private float wetAmount = 0f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

    private void Update()
    {
        if (weatherManager == null) return;
        if (mainRoadMaterial == null || sideRoadMaterial == null) return;

        float targetWetAmount =
            weatherManager.currentWeather == WeatherManager.WeatherType.Rainy ? 1f : 0f;

        wetAmount = Mathf.MoveTowards(
            wetAmount,
            targetWetAmount,
            Time.deltaTime / wetTransitionDuration
        );

        UpdateMaterial(
            mainRoadMaterial,
            mainRoadDryColor,
            mainRoadWetColor,
            mainRoadDrySmoothness,
            mainRoadWetSmoothness
        );

        UpdateMaterial(
            sideRoadMaterial,
            sideRoadDryColor,
            sideRoadWetColor,
            sideRoadDrySmoothness,
            sideRoadWetSmoothness
        );
    }

    private void UpdateMaterial(
        Material mat,
        Color dryColor,
        Color wetColor,
        float drySmoothness,
        float wetSmoothness)
    {
        Color currentColor = Color.Lerp(dryColor, wetColor, wetAmount);
        float currentSmoothness = Mathf.Lerp(drySmoothness, wetSmoothness, wetAmount);

        if (mat.HasProperty(BaseColorId))
            mat.SetColor(BaseColorId, currentColor);

        if (mat.HasProperty(SmoothnessId))
            mat.SetFloat(SmoothnessId, currentSmoothness);
    }
}