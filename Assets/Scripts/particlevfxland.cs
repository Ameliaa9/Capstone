using UnityEngine;

public class PackageDrop : MonoBehaviour
{
    public GameObject landingVFXPrefab;
    public float cooldown = 0.05f; // Prevents frame-perfect spam
    private float lastImpactTime = -1f;

    public enum WorldAxis { Y_Up, X_Right, Z_Forward }
    [Tooltip("Force VFX to align to this world axis, ignoring prefab rotation")]
    public WorldAxis rotationAxis = WorldAxis.Y_Up;

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastImpactTime < cooldown) return;
        lastImpactTime = Time.time;

        Vector3 impactPoint = collision.contacts[0].point;
        Quaternion forcedRotation = GetAxisRotation();

        GameObject vfxInstance = Instantiate(landingVFXPrefab, impactPoint, forcedRotation);
        vfxInstance.transform.SetParent(null);
    }

    Quaternion GetAxisRotation()
    {
        switch (rotationAxis)
        {
            case WorldAxis.X_Right:
                // Rotates so the particle system's "up" points world X+ (good for left/right wall hits)
                return Quaternion.Euler(0, 0, -90f);

            case WorldAxis.Z_Forward:
                // Rotates so the particle system's "up" points world Z+ (good for front/back wall hits)
                return Quaternion.Euler(90f, 0, 0);

            case WorldAxis.Y_Up:
            default:
                // Standard upright (good for ground/floor hits)
                return Quaternion.identity;
        }
    }
}