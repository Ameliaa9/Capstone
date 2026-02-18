using UnityEngine;

public class TrafficVehicle : MonoBehaviour
{
    public NPCPatrol NPCPatrol;
    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Traffic")
        {
            Debug.Log("Vehicle Trigger Entered.");
            NPCPatrol = col.GetComponent<NPCPatrol>();
            NPCPatrol.moveSpeed = 0;
            NPCPatrol.vehicleStopped = true;
            Debug.Log(col);
        }

    }

    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Traffic")
        {
            Debug.Log("Vehicle Trigger Exited.");
            NPCPatrol = col.GetComponent<NPCPatrol>();
            NPCPatrol.moveSpeed = 10;
            NPCPatrol.vehicleStopped = false;
            Debug.Log(col);
        }
    }
}