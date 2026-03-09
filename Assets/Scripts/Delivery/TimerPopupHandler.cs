using UnityEngine;

public class TimerPopupHandler : MonoBehaviour
{
    public GameObject popupImage;

    public void ShowPopup()
    {
        if (popupImage != null)
            popupImage.SetActive(true);
    }
}