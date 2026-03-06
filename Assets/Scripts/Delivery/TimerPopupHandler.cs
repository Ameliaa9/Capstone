using UnityEngine;
using System.Collections;

public class TimerPopupHandler : MonoBehaviour
{
    public GameObject popupImage;
    public Animator popupAnimator;

    public void ShowPopup()
    {
        if (popupImage != null)
            popupImage.SetActive(true);

        if (popupAnimator != null)
            popupAnimator.Play("MessageSlideCombined", 0, 0f); 
    }
}