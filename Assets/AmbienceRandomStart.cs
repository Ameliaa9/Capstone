using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbienceRandomStart : MonoBehaviour
{
    private AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (src.clip != null)
        {
            src.time = Random.Range(0f, src.clip.length);
            src.Play();
        }
    }
}
