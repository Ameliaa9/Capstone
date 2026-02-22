using KikiNgao.SimpleBikeControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadioPlayerMP3 : MonoBehaviour
{
    [Header("List of Songs")]
    [SerializeField]
    private Music[] musicList;

    private int songIndex;
    private int songPreviousIndex;
    private float songDuration;

    [SerializeField]
    private Image songAlbumCoverImage;

    [SerializeField]
    private TextMeshProUGUI phoneTextUI;

    private bool isShuffling = false;

    [SerializeField]
    private TextMeshProUGUI shufflingStatus;

    private AudioSource songAudioSource;


    void Start()
    {
        songIndex = Random.Range(0, musicList.Length);
        songAudioSource = GetComponent<AudioSource>();
        PlaySong();
    }

    void Update()
    {
        GameManager.UnlockCursor();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlaySong()
    {
        songPreviousIndex = songIndex;
        songAlbumCoverImage.sprite = musicList[songIndex].albumCover;
        songDuration = musicList[songIndex].musicDuration;
        phoneTextUI.text = musicList[songIndex].name;

        songAudioSource.clip = musicList[songIndex].musicAudioClip;
        songAudioSource.Play();
    }

    public void NextSong()
    {
        songAudioSource.Stop();

        if (isShuffling)
        {
            songIndex = Random.Range(0, musicList.Length);
            if (songIndex != songPreviousIndex)
            {
                PlaySong();
                Debug.Log("CASE 0 : Shuffle On, New song picked");
            }
            else
            {
                NextSong();
                Debug.Log("CASE 1 : Shuffle On, Next song re-called");
            }
        }
        else
        {
            if (songIndex < musicList.Length - 1)
            {
                songIndex++;
                PlaySong();
                Debug.Log("CASE 2 : Shuffle Off, Song index increased");
            }
            else
            {
                songIndex = 0;
                PlaySong();
                Debug.Log("CASE 3 : Shuffle Off, Song index reset");
            }
        }
    }

    public void PreviousSong()
    {
        songAudioSource.Stop();
        if (songIndex > 0)
        {
            songIndex--;
            PlaySong();
            Debug.Log("CASE 4 : Previous Song, Song index decreased");
        }
        else
        {
            songIndex = musicList.Length - 1;
            PlaySong();
            Debug.Log("CASE 5 : Previous Song, Song index maxed");
        }
    }

    public void ToggleShuffle()
    {
        if (isShuffling)
        {
            isShuffling = false;
            shufflingStatus.text = "Shuffle is OFF";
        }
        else
        {
            isShuffling = true;
            shufflingStatus.text = "Shuffle is ON";
        }
    }
}
