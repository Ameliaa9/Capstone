using UnityEngine;

public class PackageDrop : MonoBehaviour
{
    public GameObject landingVFXPrefab;
    public float cooldown = 0.1f; // Prevents spam during rolling
    private float lastImpactTime = -1f;

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastImpactTime < cooldown) return;
        lastImpactTime = Time.time;

        // Get impact info
        ContactPoint contact = collision.GetContact(0);
        Vector3 point = contact.point;
        Vector3 normal = contact.normal; // Direction the surface is facing

        // KEY: Rotate so particle "up" matches the surface normal
        // This means dust sprays AWAY from the wall/floor/ceiling
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        GameObject vfxInstance = Instantiate(landingVFXPrefab, point, rotation);
        vfxInstance.transform.SetParent(null); // Detach from moving projectile
    }
}