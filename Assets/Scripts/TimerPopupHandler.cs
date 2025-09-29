using UnityEngine;
using System.Collections;

public class TimerPopupHandler : MonoBehaviour
{
    public GameObject popupImage; 

    public void ShowPopup()
    {
        StartCoroutine(ShowThenHide());
    }

    IEnumerator ShowThenHide()
    {
        popupImage.SetActive(true);
        yield return new WaitForSeconds(5f);
        popupImage.SetActive(false);
    }
}
