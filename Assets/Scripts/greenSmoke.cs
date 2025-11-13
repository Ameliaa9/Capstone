using KikiNgao.SimpleBikeControl;
using UnityEngine;
using UnityEngine.UI;

public class greenSmoke : MonoBehaviour
{
    [Header("UI Image to Show/Hide")]
    public Image targetImage;

    [SerializeField]
    private SimpleBike player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike") && targetImage != null)
        {
            Debug.Log("Player entered trigger - showing image");
            targetImage.gameObject.SetActive(true);
            player.legPower -= 10;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bike") && targetImage != null)
        {
            Debug.Log("Player exited trigger - hiding image");
            targetImage.gameObject.SetActive(false);
            player.legPower += 10;
        }
    }

}
