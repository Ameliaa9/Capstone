using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private string zoneName = "Zone 1";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            Debug.Log("Entered " + zoneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            Debug.Log("Exited " + zoneName);
        }
    }
}