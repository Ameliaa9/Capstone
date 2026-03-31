using UnityEngine;

public class GooseSoundOnHit : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] gooseSounds;

    [Range(0f, 1f)]
    public float volume = 1f;

    public float cooldown = 0.3f;

    [Header("Hit Detection")]
    public string bikeTag = "Bike";

    [Header("Startup")]
    public float ignoreHitTimeAtStart = 0.5f;

    private float lastPlayTime = -999f;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < ignoreHitTimeAtStart)
            return;

        if (collision.gameObject.CompareTag(bikeTag))
        {
            TryPlaySound();
        }
    }

    private void TryPlaySound()
    {
        if (audioSource == null) return;
        if (gooseSounds == null || gooseSounds.Length == 0) return;

        if (Time.time - lastPlayTime < cooldown)
            return;

        AudioClip clip = gooseSounds[Random.Range(0, gooseSounds.Length)];
        audioSource.PlayOneShot(clip, volume);
        lastPlayTime = Time.time;
    }
}