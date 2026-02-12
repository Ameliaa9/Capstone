using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Customer Voice Tables")]
    [Tooltip("Map customerId (e.g. Junior/Valarie/Zarah) -> CustomerVoiceTable asset")]
    [SerializeField] private List<CustomerVoiceEntry> customerVoiceTables = new();

    [Header("Voice Settings")]
    [Tooltip("If true, new voice lines will interrupt the current one")]
    [SerializeField] private bool interruptVoice = true;

    [Tooltip("If true, avoids playing the same voice clip twice in a row")]
    [SerializeField] private bool avoidRepeat = true;

    private AudioClip lastVoiceClip;

    // runtime lookup
    private Dictionary<string, CustomerVoiceTable> tableLookup;

    [Serializable]
    public class CustomerVoiceEntry
    {
        public string customerId;              // "Junior"
        public CustomerVoiceTable voiceTable;  // Junior_VoiceTable
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

            // last one wins if duplicates
            tableLookup[entry.customerId.Trim()] = entry.voiceTable;
        }
    }

    /// <summary>
    /// Plays the customer's voice line based on star rating (1~5).
    /// customerId example: "Junior"
    /// </summary>
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

        // avoid repeat (only matters if the chosen clip equals last)
        if (avoidRepeat && clip == lastVoiceClip)
        {
            // if repeat happens and you have alternatives, you'd handle here.
            // since this system is 1 clip per star, we just allow it.
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
}

