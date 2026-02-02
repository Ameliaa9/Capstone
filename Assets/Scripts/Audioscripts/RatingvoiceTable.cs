using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Rating Voice Table")]
public class RatingVoiceTable : ScriptableObject
{
    [Serializable]
    public class RatingClips
    {
        [Range(1, 5)]
        public int stars = 5;

        public List<AudioClip> clips = new List<AudioClip>();
    }

    public List<RatingClips> table = new List<RatingClips>();

    public List<AudioClip> GetClips(int stars)
    {
        RatingClips entry = table.Find(e => e.stars == stars);
        return entry != null ? entry.clips : null;
    }
}
