using UnityEngine;

public class UIButtonClickSFX : MonoBehaviour
{
    [SerializeField] private float volume = 0.8f;

    public void PlayClickSFX()
    {
        AudioManager.I?.PlayMenuButtonClick(volume);
    }
}