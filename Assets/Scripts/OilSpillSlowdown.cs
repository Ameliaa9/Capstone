using UnityEngine;

public class OilSpillSlowdown : MonoBehaviour
{
    public float slowdownMultiplier = 0.5f;
    public float duration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered OilSpill Trigger: " + other.name);

        GameObject rootObj = other.transform.root.gameObject;

        if (rootObj.CompareTag("Bike"))
        {
            var bike = rootObj.GetComponent<KikiNgao.SimpleBikeControl.SimpleBike>();
            if (bike != null)
            {
                Debug.Log("? Slowdown applied to: " + rootObj.name);
                bike.ApplyTemporarySlowdown(slowdownMultiplier, duration);
            }
            else
            {
                Debug.LogWarning("?? PlayerBike tagged object found but no SimpleBike script on it: " + rootObj.name);
            }
        }
        else
        {
            Debug.LogWarning("? Object entered trigger but not tagged PlayerBike: " + rootObj.name);
        }
    }
}
