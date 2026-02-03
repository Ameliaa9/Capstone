using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public NPCPatrol NPCPatrol;
    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Traffic")
        {
            Debug.Log("Trigger Entered.");
            NPCPatrol = col.GetComponent<NPCPatrol>();
            NPCPatrol.moveSpeed = 0;
            NPCPatrol.vehicleStopped = true;
            Debug.Log(col);
        }
        
    }

}
