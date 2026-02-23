using KikiNgao.SimpleBikeControl;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

public class RadioPlayerMP3 : MonoBehaviour
{
    [Header("List of Songs")]
    [SerializeField]
    private Music[] musicList;

    private int songIndex;
    private int songPreviousIndex;
    private float songDuration;
    private float songElapsed;

    [SerializeField]
    private Image songAlbumCoverImage;

    [SerializeField]
    private TextMeshProUGUI phoneTextUI;
    [SerializeField]
    private TextMeshProUGUI timeTextUI;

    [SerializeField]
    private Image songFill;

    private bool isShuffling = false;

    [SerializeField]
    private TextMeshProUGUI shufflingStatus;

    private AudioSource songAudioSource;


    void Start()
    {
        songIndex = UnityEngine.Random.Range(0, musicList.Length);
        songAudioSource = GetComponent<AudioSource>();
        PlaySong();
    }

    void Update()
    {
        if (songAudioSource.isPlaying)
        {
            songElapsed = songAudioSource.time;
            songFill.fillAmount = songElapsed / songDuration;
            timeTextUI.text = MathF.Round(songElapsed,1).ToString() + " / " + MathF.Round(songDuration,1).ToString();
        }

        // Next three lines are only here for PC testing
        GameManager.UnlockCursor();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlaySong()
    {
        songPreviousIndex = songIndex;
        songAlbumCoverImage.sprite = musicList[songIndex].albumCover;
        phoneTextUI.text = musicList[songIndex].name;

        songAudioSource.clip = musicList[songIndex].musicAudioClip;
        songDuration = songAudioSource.clip.length;
        songAudioSource.Play();
    }

    public void NextSong()
    {
        songAudioSource.Stop();

        if (isShuffling)
        {
            songIndex = UnityEngine.Random.Range(0, musicList.Length);
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
