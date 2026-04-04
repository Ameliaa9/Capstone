using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RainAudioController : MonoBehaviour
{
    [Header("References")]
    public WeatherManager weatherManager;
    public AudioSource rainAudioSource;

    [Header("Volume Settings")]
    public float rainyTargetVolume = 0.5f;
    public float fadeSpeed = 1.5f;

    private void Reset()
    {
        rainAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (weatherManager == null || rainAudioSource == null) return;

        float targetVolume =
            weatherManager.currentWeather == WeatherManager.WeatherType.Rainy
            ? rainyTargetVolume
            : 0f;

        rainAudioSource.volume = Mathf.Lerp(
            rainAudioSource.volume,
            targetVolume,
            Time.deltaTime * fadeSpeed
        );

        if (!rainAudioSource.isPlaying)
            rainAudioSource.Play();
    }
}
