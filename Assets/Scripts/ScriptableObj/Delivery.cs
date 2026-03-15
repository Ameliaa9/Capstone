using UnityEngine;

[CreateAssetMenu(fileName = "Delivery")]
public class Delivery : ScriptableObject
{
    public int deliveryIndex; // Determines which delivery the customer is indexed as

    public float deliveryTime; // Total time for the delivery
    public Sprite customerIcon; // Depot Customer Icon
    public Sprite customerPhoneIcon; // Phone Customer Icon

    public string location;
    public string difficulty;
}
