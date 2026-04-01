using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private string zoneName = "Zone 1";
    [SerializeField] private ZoneNPCController[] zoneNPCs;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            Debug.Log("Entered " + zoneName);

            foreach (ZoneNPCController npc in zoneNPCs)
            {
                if (npc != null)
                    npc.SetActiveState(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            Debug.Log("Exited " + zoneName);

            foreach (ZoneNPCController npc in zoneNPCs)
            {
                if (npc != null)
                    npc.SetActiveState(false);
            }
        }
    }
}