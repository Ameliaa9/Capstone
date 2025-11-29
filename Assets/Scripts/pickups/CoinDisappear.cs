using UnityEngine;

public class DisappearOnTrigger : MonoBehaviour
{
    public string playerTag = "Player";

    public TaskManager coinTaskManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (coinTaskManager != null)
        {
            coinTaskManager.OnCoinCollected();
        }

        gameObject.SetActive(false);
    }
}
