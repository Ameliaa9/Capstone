using UnityEngine;

[CreateAssetMenu(fileName = "Music")]
public class Music : ScriptableObject
{
    public AudioClip musicAudioClip;
    public float musicDuration;
    public Sprite albumCover;
}
