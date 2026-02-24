using UnityEngine;

public class LandmarkTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            FindObjectOfType<TaskManager>()?.OnLandmarkVisited();
        }
    }
}