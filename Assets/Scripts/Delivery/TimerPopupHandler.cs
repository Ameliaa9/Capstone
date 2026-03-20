using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerPopupHandler : MonoBehaviour
{
    public GameObject popupImageObject;

    public Image popupImage;

    public TextMeshProUGUI popupTextName;
    public TextMeshProUGUI popupText;

    public void ShowPopup()
    {
        if (popupImageObject != null)
            popupImageObject.SetActive(true);
    }
}