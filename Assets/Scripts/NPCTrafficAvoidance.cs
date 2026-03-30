using UnityEngine;

public class NPCTrafficAvoidance : MonoBehaviour
{
    public NPCNavAI npcAI;
    public string trafficTag = "Traffic";

    private void Awake()
    {
        if (npcAI == null)
            npcAI = GetComponentInParent<NPCNavAI>();

        if (npcAI == null)
            npcAI = GetComponent<NPCNavAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(trafficTag))
        {
            npcAI.NotifyTrafficEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(trafficTag))
        {
            npcAI.NotifyTrafficExit();
        }
    }
}