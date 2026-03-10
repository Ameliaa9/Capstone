using UnityEngine;
using System.Collections;

public class CarHit : MonoBehaviour
{
    public float launchSpeed = 12f;
    public float upwardSpeed = 6f;
    public float stunTime = 0.5f;

    private bool onCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (onCooldown) return;
        if (!other.CompareTag("Bike")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody>();

        if (rb == null) return;

        BikeMovement bikeMovement = rb.GetComponent<BikeMovement>();
        if (bikeMovement == null) return;

        StartCoroutine(HitBike(rb, bikeMovement));
    }

    IEnumerator HitBike(Rigidbody rb, BikeMovement bikeMovement)
    {
        onCooldown = true;

        bikeMovement.enabled = false;

        Vector3 launchDirection = (rb.transform.position - transform.position).normalized;
        launchDirection.y = 0f;
        launchDirection.Normalize();

        rb.linearVelocity = launchDirection * launchSpeed + Vector3.up * upwardSpeed;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(stunTime);

        bikeMovement.enabled = true;

        yield return new WaitForSeconds(1f);
        onCooldown = false;
    }
}