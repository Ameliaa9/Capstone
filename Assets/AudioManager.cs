using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource voiceSource;   
    [SerializeField] private AudioSource sfxSource;     

    [Header("Audio Tables")]
    [SerializeField] private RatingVoiceTable ratingVoiceTable;

    [Header("Voice Settings")]
    [Tooltip("If true, new voice lines will interrupt the current one")]
    [SerializeField] private bool interruptVoice = true;

    [Tooltip("If true, avoids playing the same voice clip twice in a row")]
    [SerializeField] private bool avoidRepeat = true;

    private AudioClip lastVoiceClip;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        
        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
    }

   
    public void PlayRatingVoice(int stars)
    {
        if (ratingVoiceTable == null)
        {
            Debug.LogWarning("[AudioManager] RatingVoiceTable is not assigned.");
            return;
        }

        List<AudioClip> clips = ratingVoiceTable.GetClips(stars);
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning($"[AudioManager] No voice clips configured for {stars} stars.");
            return;
        }

        if (voiceSource == null)
        {
            Debug.LogWarning("[AudioManager] Voice AudioSource is not assigned.");
            return;
        }

        
        if (!interruptVoice && voiceSource.isPlaying)
            return;

        AudioClip chosenClip = PickClip(clips);

        if (interruptVoice)
            voiceSource.Stop();

        voiceSource.PlayOneShot(chosenClip);
        lastVoiceClip = chosenClip;
    }

    private AudioClip PickClip(List<AudioClip> clips)
    {
        if (!avoidRepeat || clips.Count == 1)
            return clips[0];

        
        for (int i = 0; i < 6; i++)
        {
            AudioClip clip = clips[Random.Range(0, clips.Count)];
            if (clip != lastVoiceClip)
                return clip;
        }

       
        return clips[Random.Range(0, clips.Count)];
    }

    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }
}

