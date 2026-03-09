using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupRespawn : MonoBehaviour
{
    public float respawnTime = 30f;

    private List<GameObject> pickups = new List<GameObject>();
    private Dictionary<GameObject, Vector3> pickupPositions = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        GameObject[] foundPickups = GameObject.FindGameObjectsWithTag("Pickup");

        foreach (GameObject pickup in foundPickups)
        {
            pickups.Add(pickup);
            pickupPositions[pickup] = pickup.transform.position;
        }
    }

    void Update()
    {
        foreach (GameObject pickup in pickups)
        {
            if (!pickup.activeSelf)
            {
                StartCoroutine(RespawnPickup(pickup));
            }
        }
    }

    IEnumerator RespawnPickup(GameObject pickup)
    {
        pickups.Remove(pickup);

        yield return new WaitForSeconds(respawnTime);

        pickup.transform.position = pickupPositions[pickup];
        pickup.SetActive(true);

        pickups.Add(pickup);
    }
}