using UnityEngine;
using System.Collections;

public class CarHit : MonoBehaviour
{
    public float launchSpeed = 12f;
    public float upwardSpeed = 6f;
    public float stunTime = 0.5f;

    private bool onCooldown = false;

    public AudioClip[] carHitSounds;         // honking sfx
    public AudioClip[] secondarySounds;     // yelling sfx

    [Range(0f, 1f)]
    public float secondarySoundChance = 0.4f; // 40% chance

    public float secondarySoundDelay = 0.15f; 

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

        onCooldown = true;
        StartCoroutine(HitBike(rb, bikeMovement));
    }

    IEnumerator HitBike(Rigidbody rb, BikeMovement bikeMovement)
    {
        PlayRandomMainSound();
        StartCoroutine(PlaySecondarySoundSometimes());

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

    void PlayRandomMainSound()
    {
        if (carHitSounds == null || carHitSounds.Length == 0) return;

        AudioClip randomClip = carHitSounds[Random.Range(0, carHitSounds.Length)];
        if (randomClip != null)
        {
            AudioSource.PlayClipAtPoint(randomClip, transform.position);
        }
    }

    IEnumerator PlaySecondarySoundSometimes()
    {
        if (secondarySounds == null || secondarySounds.Length == 0) yield break;

        if (Random.value <= secondarySoundChance)
        {
            yield return new WaitForSeconds(secondarySoundDelay);

            AudioClip randomClip = secondarySounds[Random.Range(0, secondarySounds.Length)];
            if (randomClip != null)
            {
                AudioSource.PlayClipAtPoint(randomClip, transform.position);
            }
        }
    }
}