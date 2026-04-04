using UnityEngine;

public class TimeOfDayManager : MonoBehaviour
{
    [Header("References")]
    public Light directionalLight;

    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float currentTime = 12f;
    public float dayDurationInMinutes = 5f;

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

    [Header("Fog")]
    public bool useFog = true;
    public Gradient fogColorOverDay;
    public AnimationCurve fogDensityOverDay = AnimationCurve.Linear(0f, 0.01f, 1f, 0.01f);

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
        UpdateFog();
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
        directionalLight.intensity = lightIntensityOverDay.Evaluate(normalizedTime);
    }

    private void UpdateAmbientLighting()
    {
        float normalizedTime = currentTime / 24f;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColorOverDay.Evaluate(normalizedTime);
        RenderSettings.ambientIntensity = ambientIntensityOverDay.Evaluate(normalizedTime);
    }

    private void UpdateFog()
    {
        float normalizedTime = currentTime / 24f;

        RenderSettings.fog = useFog;
        if (!useFog) return;

        RenderSettings.fogColor = fogColorOverDay.Evaluate(normalizedTime);
        RenderSettings.fogDensity = fogDensityOverDay.Evaluate(normalizedTime);
    }

    private void Reset()
    {
        lightColorOverDay = new Gradient();
        lightColorOverDay.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.15f, 0.18f, 0.28f), 0f),
                new GradientColorKey(new Color(1f, 0.7f, 0.45f), 0.25f),
                new GradientColorKey(new Color(1f, 0.98f, 0.92f), 0.5f),
                new GradientColorKey(new Color(1f, 0.55f, 0.35f), 0.75f),
                new GradientColorKey(new Color(0.15f, 0.18f, 0.28f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        lightIntensityOverDay = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 0.35f),
            new Keyframe(0.3f, 1f),
            new Keyframe(0.5f, 1.1f),
            new Keyframe(0.75f, 0.5f),
            new Keyframe(1f, 0f)
        );

        ambientColorOverDay = new Gradient();
        ambientColorOverDay.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.06f, 0.08f, 0.14f), 0f),
                new GradientColorKey(new Color(0.4f, 0.35f, 0.3f), 0.25f),
                new GradientColorKey(new Color(0.75f, 0.78f, 0.82f), 0.5f),
                new GradientColorKey(new Color(0.45f, 0.32f, 0.28f), 0.75f),
                new GradientColorKey(new Color(0.06f, 0.08f, 0.14f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        ambientIntensityOverDay = new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.25f, 0.45f),
            new Keyframe(0.5f, 0.95f),
            new Keyframe(0.75f, 0.5f),
            new Keyframe(1f, 0.2f)
        );

        fogColorOverDay = new Gradient();
        fogColorOverDay.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.05f, 0.07f, 0.12f), 0f),
                new GradientColorKey(new Color(0.75f, 0.55f, 0.45f), 0.25f),
                new GradientColorKey(new Color(0.75f, 0.85f, 0.95f), 0.5f),
                new GradientColorKey(new Color(0.8f, 0.5f, 0.4f), 0.75f),
                new GradientColorKey(new Color(0.05f, 0.07f, 0.12f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        fogDensityOverDay = new AnimationCurve(
            new Keyframe(0f, 0.01f),
            new Keyframe(0.25f, 0.006f),
            new Keyframe(0.5f, 0.003f),
            new Keyframe(0.75f, 0.006f),
            new Keyframe(1f, 0.01f)
        );
    }
}