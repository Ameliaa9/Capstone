using UnityEngine;
using UnityEngine.UI;

public class greenSmoke : MonoBehaviour
{
    [Header("UI Image to Show/Hide")]
    public Image targetImage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike") && targetImage != null)
        {
            Debug.Log("Player entered trigger - showing image");
            targetImage.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bike") && targetImage != null)
        {
            Debug.Log("Player exited trigger - hiding image");
            targetImage.gameObject.SetActive(false);
        }
    }
}
