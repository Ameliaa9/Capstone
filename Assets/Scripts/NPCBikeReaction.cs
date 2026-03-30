using UnityEngine;

public class NPCBikeReaction : MonoBehaviour
{
    public NPCNavAI npcAI;

    [Header("Tags")]
    public string bikeTag = "Bike";
    public string packageTag = "Package";

    private void Awake()
    {
        if (npcAI == null)
            npcAI = GetComponent<NPCNavAI>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(bikeTag))
        {
            npcAI.TriggerChase(collision.transform);
            return;
        }

        if (collision.gameObject.CompareTag(packageTag))
        {
            GameObject bike = GameObject.FindGameObjectWithTag(bikeTag);
            if (bike != null)
            {
                npcAI.TriggerChase(bike.transform);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(packageTag))
        {
            GameObject bike = GameObject.FindGameObjectWithTag(bikeTag);
            if (bike != null)
            {
                npcAI.TriggerChase(bike.transform);
            }
        }
    }
}