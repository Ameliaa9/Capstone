using System.Collections;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private string zoneName = "Zone";
    [SerializeField] private ZoneNPCController[] zoneNPCs;
    [SerializeField] private float initialDelay = 3f;

    [Header("Linked Zones")]
    [SerializeField] private ZoneTrigger[] linkedZones;

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
            SetZoneState(false);
            Debug.Log(zoneName + " NPCs set to sleep after initial delay.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            playerInside = true;
            Debug.Log("Entered " + zoneName);

            SetZoneState(true);

            if (linkedZones != null)
            {
                foreach (ZoneTrigger linkedZone in linkedZones)
                {
                    if (linkedZone != null)
                    {
                        linkedZone.SetZoneState(true);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            playerInside = false;
            Debug.Log("Exited " + zoneName);

            SetZoneState(false);

            if (linkedZones != null)
            {
                foreach (ZoneTrigger linkedZone in linkedZones)
                {
                    if (linkedZone != null && !linkedZone.IsPlayerInside())
                    {
                        linkedZone.SetZoneState(false);
                    }
                }
            }
        }
    }

    public void SetZoneState(bool isActive)
    {
        if (zoneNPCs != null)
        {
            foreach (ZoneNPCController npc in zoneNPCs)
            {
                if (npc != null)
                    npc.SetActiveState(isActive);
            }
        }
    }

    public bool IsPlayerInside()
    {
        return playerInside;
    }
}