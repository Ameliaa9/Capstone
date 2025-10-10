using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PackageImpactSound : MonoBehaviour
{
    public AudioClip impactClip;
    private AudioSource audioSource;
    private bool hasPlayed = false; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only play the first impact
        if (!hasPlayed && impactClip)
        {
            audioSource.PlayOneShot(impactClip);
            hasPlayed = true;
        }
    }
}
