using UnityEngine;

public class HazardousObstacles : MonoBehaviour
{
    [SerializeField]
    private BikeHealth bikeHealth;


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bike"))
        {
            bikeHealth.SetHealth(-2);
        }
    }

}
