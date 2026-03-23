using UnityEngine;

public class PackageDrop : MonoBehaviour
{
    public ParticleSystem landingVFX;
    private bool hasLanded = false; // Optional: prevents double-triggering on bounces

    void OnCollisionEnter(Collision collision)
    {
        // Remove the "if (collision.gameObject.CompareTag("Ground"))" line completely

        // Optional safety check: only trigger once
        if (hasLanded) return;
        hasLanded = true;

        // Move VFX to impact point and play
        landingVFX.transform.position = collision.contacts[0].point;
        landingVFX.Play();

        // Optional: Destroy this script so it never triggers again
        // Destroy(this);
    }
}