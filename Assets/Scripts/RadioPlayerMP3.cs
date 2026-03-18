using KikiNgao.SimpleBikeControl;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;

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

    [Header("Volume")]
    [SerializeField]
    private Image volumeFill;

    [SerializeField]
    private float volumeStep = 0.1f;

    private bool isShuffling = false;

    [SerializeField]
    private TextMeshProUGUI shufflingStatus;

    private AudioSource songAudioSource;


    void Start()
    {
        songIndex = UnityEngine.Random.Range(0, musicList.Length);
        songAudioSource = GetComponent<AudioSource>();
        songAudioSource.volume = 1f;

        if (volumeFill != null)
        {
            volumeFill.fillAmount = songAudioSource.volume;
        }

        PlaySong();
    }

    void Update()
    {
        if (songAudioSource.isPlaying)
        {
            songElapsed = songAudioSource.time;
            songFill.fillAmount = songElapsed / songDuration;
            timeTextUI.text = MathF.Round(songElapsed, 1).ToString() + " / " + MathF.Round(songDuration, 1).ToString();
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.left.wasPressedThisFrame)
            {
                PreviousSong();
            }

            if (Gamepad.current.dpad.right.wasPressedThisFrame)
            {
                NextSong();
            }

            if (Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                VolumeUp();
            }

            if (Gamepad.current.dpad.down.wasPressedThisFrame)
            {
                VolumeDown();
            }
        }

        if (MathF.Round(songElapsed, 0) +1 == MathF.Round(songDuration, 0))
        {
            NextSong();
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
            shufflingStatus.text = "OFF";
        }
        else
        {
            isShuffling = true;
            shufflingStatus.text = "ON";
        }
    }

    public void VolumeUp()
    {
        songAudioSource.volume = Mathf.Clamp01(songAudioSource.volume + volumeStep);

        if (volumeFill != null)
        {
            volumeFill.fillAmount = songAudioSource.volume;
        }
    }

    public void VolumeDown()
    {
        songAudioSource.volume = Mathf.Clamp01(songAudioSource.volume - volumeStep);

        if (volumeFill != null)
        {
            volumeFill.fillAmount = songAudioSource.volume;
        }
    }

    public void InstanceNewSong(string musicInstance)
    {
        songAudioSource.Stop();
        Music newSong = Resources.Load<Music>(musicInstance);
        songAudioSource.clip = newSong.musicAudioClip;
        songAlbumCoverImage.sprite = newSong.albumCover;
        phoneTextUI.text = newSong.name;
        songAudioSource.Play();
    }
}