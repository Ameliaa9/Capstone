using UnityEngine;

public class ThrowSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip throwClip;

    void Start()
    {
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Joystick2Button7))
        {
            PlayThrowSound();
        }
    }

    public void PlayThrowSound()
    {
        if (throwClip != null)
            audioSource.PlayOneShot(throwClip);
    }
}
