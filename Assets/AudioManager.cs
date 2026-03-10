using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Gameplay SFX")]
    [SerializeField] private AudioClip deliveryMissedClip;
    [SerializeField] private AudioClip packagePickupClip;
    [SerializeField] private AudioClip menuButtonClickClip;
    [SerializeField] private AudioClip menuPopupOpenClip;
    [SerializeField] private AudioClip menuToggleClip;

    [Header("Customer Voice Tables")]
    [Tooltip("Map customerId (e.g. Junior/Valarie/Zarah) -> CustomerVoiceTable asset")]
    [SerializeField] private List<CustomerVoiceEntry> customerVoiceTables = new();

    [Header("Voice Settings")]
    [Tooltip("If true, new voice lines will interrupt the current one")]
    [SerializeField] private bool interruptVoice = true;

    [Tooltip("If true, avoids playing the same voice clip twice in a row")]
    [SerializeField] private bool avoidRepeat = true;

    private AudioClip lastVoiceClip;

    private Dictionary<string, CustomerVoiceTable> tableLookup;

    [Serializable]
    public class CustomerVoiceEntry
    {
        public string customerId;
        public CustomerVoiceTable voiceTable;
    }

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

        BuildLookup();
    }

    private void BuildLookup()
    {
        tableLookup = new Dictionary<string, CustomerVoiceTable>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in customerVoiceTables)
        {
            if (entry == null) continue;
            if (string.IsNullOrWhiteSpace(entry.customerId)) continue;
            if (entry.voiceTable == null) continue;

            tableLookup[entry.customerId.Trim()] = entry.voiceTable;
        }
    }

    public void PlayCustomerVoice(string customerId, int stars)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            Debug.LogWarning("[AudioManager] customerId is null/empty.");
            return;
        }

        if (voiceSource == null)
        {
            Debug.LogWarning("[AudioManager] Voice AudioSource is not assigned.");
            return;
        }

        if (tableLookup == null)
            BuildLookup();

        if (!tableLookup.TryGetValue(customerId.Trim(), out var table) || table == null)
        {
            Debug.LogWarning($"[AudioManager] No CustomerVoiceTable found for customerId='{customerId}'.");
            return;
        }

        AudioClip clip = table.GetClipForStars(stars);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No clip configured for customerId='{customerId}', stars={stars}.");
            return;
        }

        if (!interruptVoice && voiceSource.isPlaying)
            return;

        if (avoidRepeat && clip == lastVoiceClip)
        {
        }

        if (interruptVoice)
            voiceSource.Stop();

        voiceSource.PlayOneShot(clip);
        lastVoiceClip = clip;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayDeliveryMissed(float volume = 0.8f)
    {
        PlaySFX(deliveryMissedClip, volume);
    }

    public void PlayPackagePickup(float volume = 0.8f)
    {
        PlaySFX(packagePickupClip, volume);
    }

    public void PlayMenuButtonClick(float volume = 0.8f)
    {
        PlaySFX(menuButtonClickClip, volume);
    }

    public void PlayMenuPopupOpen(float volume = 0.8f)
    {
        PlaySFX(menuPopupOpenClip, volume);
    }

    public void PlayMenuToggle(float volume = 0.8f)
    {
        PlaySFX(menuToggleClip, volume);
    }
}