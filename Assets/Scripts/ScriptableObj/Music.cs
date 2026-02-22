using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Music")]
public class Music : ScriptableObject
{
    public AudioClip musicAudioClip;
    public float musicDuration;
    public Sprite albumCover;
}
