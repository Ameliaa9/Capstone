using UnityEngine;

[CreateAssetMenu(fileName = "CustomerVoiceTable", menuName = "Audio/Customer Voice Table")]
public class CustomerVoiceTable : ScriptableObject
{
    [Header("Voice Lines (per star)")]
    public AudioClip star1;
    public AudioClip star2;
    public AudioClip star3;
    public AudioClip star4;
    public AudioClip star5;

    public AudioClip GetClipForStars(int stars)
    {
        switch (stars)
        {
            case 1: return star1;
            case 2: return star2;
            case 3: return star3;
            case 4: return star4;
            case 5: return star5;
            default: return null;
        }
    }
}

