using UnityEngine;

public class GooseKnockback : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Hit Detection")]
    public string bikeTag = "Bike";
    public float ignoreHitTimeAtStart = 0.5f;

    [Header("Knockback")]
    public float knockbackForce = 4f;
    public float upwardForce = 0f;
    public float cooldown = 0.3f;

    private float lastHitTime = -999f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < ignoreHitTimeAtStart)
            return;

        if (!collision.gameObject.CompareTag(bikeTag))
            return;

        if (Time.time - lastHitTime < cooldown)
            return;

        Vector3 hitDir = (transform.position - collision.transform.position).normalized;
        hitDir.y = 0f;

        if (hitDir.sqrMagnitude < 0.01f)
            hitDir = transform.forward;

        Vector3 force = hitDir * knockbackForce + Vector3.up * upwardForce;
        rb.AddForce(force, ForceMode.Impulse);

        lastHitTime = Time.time;
    }
}