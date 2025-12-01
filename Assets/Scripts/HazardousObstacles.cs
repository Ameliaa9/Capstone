using UnityEngine;

public class HazardousObstacles : MonoBehaviour
{
    [SerializeField]
    private BikeHealth bikeHealth;

    [SerializeField]
    private int damageAmount = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            bikeHealth.SetHealth(-damageAmount);
        }
    }
}
