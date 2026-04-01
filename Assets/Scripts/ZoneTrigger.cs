using System.Collections;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private string zoneName = "Zone 1";
    [SerializeField] private ZoneNPCController[] zoneNPCs;
    [SerializeField] private float initialDelay = 3f;

    private bool playerInside = false;

    private void Start()
    {
        StartCoroutine(InitialZoneCheck());
    }

    private IEnumerator InitialZoneCheck()
    {
        yield return new WaitForSeconds(initialDelay);

        if (!playerInside)
        {
            foreach (ZoneNPCController npc in zoneNPCs)
            {
                if (npc != null)
                    npc.SetActiveState(false);
            }

            Debug.Log(zoneName + " NPCs set to sleep after initial delay.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            playerInside = true;
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
            playerInside = false;
            Debug.Log("Exited " + zoneName);

            foreach (ZoneNPCController npc in zoneNPCs)
            {
                if (npc != null)
                    npc.SetActiveState(false);
            }
        }
    }
}