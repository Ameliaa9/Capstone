using UnityEngine;

public class DisappearOnTrigger : MonoBehaviour
{
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            gameObject.SetActive(false);
        }
    }
}
