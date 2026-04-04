using UnityEngine;

public class TimeOfDayManager : MonoBehaviour
{
    [Header("References")]
    public Light directionalLight;

    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float currentTime = 12f;
    public float dayDurationInMinutes = 12f;

    [Header("Sun Rotation")]
    public Vector3 sunriseRotation = new Vector3(15f, -30f, 0f);
    public Vector3 noonRotation = new Vector3(60f, -30f, 0f);
    public Vector3 sunsetRotation = new Vector3(170f, -30f, 0f);
    public Vector3 nightRotation = new Vector3(250f, -30f, 0f);

    [Header("Sun Light")]
    public Gradient lightColorOverDay;
    public AnimationCurve lightIntensityOverDay = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Ambient Light")]
    public Gradient ambientColorOverDay;
    public AnimationCurve ambientIntensityOverDay = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Fog Reference Only")]
    public Gradient fogColorOverDay;
    public AnimationCurve fogDensityOverDay = AnimationCurve.Linear(0f, 0.01f, 1f, 0.01f);

    [Header("Weather Multipliers")]
    public float sunIntensityMultiplier = 1f;
    public float ambientIntensityMultiplier = 1f;

    private void Update()
    {
        if (dayDurationInMinutes > 0f)
        {
            float hoursPerSecond = 24f / (dayDurationInMinutes * 60f);
            currentTime += hoursPerSecond * Time.deltaTime;

            if (currentTime >= 24f)
                currentTime -= 24f;
        }

        UpdateSunRotation();
        UpdateSunLighting();
        UpdateAmbientLighting();
    }

    private void UpdateSunRotation()
    {
        if (directionalLight == null) return;

        Vector3 targetRotation;

        if (currentTime >= 6f && currentTime < 12f)
        {
            float t = Mathf.InverseLerp(6f, 12f, currentTime);
            targetRotation = Vector3.Lerp(sunriseRotation, noonRotation, t);
        }
        else if (currentTime >= 12f && currentTime < 18f)
        {
            float t = Mathf.InverseLerp(12f, 18f, currentTime);
            targetRotation = Vector3.Lerp(noonRotation, sunsetRotation, t);
        }
        else if (currentTime >= 18f && currentTime < 24f)
        {
            float t = Mathf.InverseLerp(18f, 24f, currentTime);
            targetRotation = Vector3.Lerp(sunsetRotation, nightRotation, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0f, 6f, currentTime);
            targetRotation = Vector3.Lerp(nightRotation, sunriseRotation, t);
        }

        directionalLight.transform.rotation = Quaternion.Euler(targetRotation);
    }

    private void UpdateSunLighting()
    {
        if (directionalLight == null) return;

        float normalizedTime = currentTime / 24f;

        directionalLight.color = lightColorOverDay.Evaluate(normalizedTime);
        directionalLight.intensity =
            lightIntensityOverDay.Evaluate(normalizedTime) * sunIntensityMultiplier;
    }

    private void UpdateAmbientLighting()
    {
        float normalizedTime = currentTime / 24f;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColorOverDay.Evaluate(normalizedTime);
        RenderSettings.ambientIntensity =
            ambientIntensityOverDay.Evaluate(normalizedTime) * ambientIntensityMultiplier;
    }

    public Color GetCurrentFogColor()
    {
        float normalizedTime = currentTime / 24f;
        return fogColorOverDay.Evaluate(normalizedTime);
    }

    public float GetCurrentFogDensity()
    {
        float normalizedTime = currentTime / 24f;
        return fogDensityOverDay.Evaluate(normalizedTime);
    }
}