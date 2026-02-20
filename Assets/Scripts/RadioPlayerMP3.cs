using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadioPlayerMP3 : MonoBehaviour
{
    [Header("List of Songs")]
    [SerializeField]
    private Music[] musicList;
    private int songIndex;
    private AudioSource songAudioSource;
    private Sprite songAlbumCover;
    private float songDuration;

    [SerializeField]
    private TextMeshProUGUI phoneTextUI;

    private bool isShuffling = false;
    

    void Start()
    {
        songIndex = Random.Range(0, musicList.Length);
        songAudioSource = GetComponent<AudioSource>();
        songAlbumCover = musicList[songIndex].albumCover;
        songDuration = musicList[songIndex].musicDuration;
        phoneTextUI.text = musicList[songIndex].name;

        songAudioSource.resource = musicList[songIndex].musicAudioClip;
        songAudioSource.Play();
    }

    void Update()
    {
        
    }

    public void NextSong()
    {

    }

    public void PreviousSong()
    {

    }

    public void ToggleShuffle()
    {
        if (isShuffling)
        {
            isShuffling = false;
        }
        else
        {
            isShuffling = true;
        }
    }
}
