using UnityEngine;

public class DepotEntranceTrigger : MonoBehaviour
{
    public DepotUIManager depotUIManager;
    public string triggerTag = "bike";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.name + " | Root: " + other.transform.root.name + " | Root Tag: " + other.transform.root.tag);

        if (hasTriggered) return;

        if (other.transform.root.CompareTag(triggerTag))
        {
            hasTriggered = true;
            depotUIManager.EnterDepot();
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}