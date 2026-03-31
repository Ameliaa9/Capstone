using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GooseKnockback : MonoBehaviour
{
    public Rigidbody rb;
    public NavMeshAgent agent;

    [Header("Hit Detection")]
    public string bikeTag = "Bike";
    public float ignoreHitTimeAtStart = 0.5f;

    [Header("Knockback")]
    public float knockbackDistance = 0.8f;
    public float knockbackDuration = 0.12f;
    public float cooldown = 0.3f;

    private float lastHitTime = -999f;
    private Coroutine knockbackRoutine;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
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

        if (hitDir.sqrMagnitude < 0.001f)
            hitDir = transform.forward;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(DoKnockback(hitDir.normalized));
        lastHitTime = Time.time;
    }

    private IEnumerator DoKnockback(Vector3 dir)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dir * knockbackDistance;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            targetPos = hit.position;
        }

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;
            float t = timer / knockbackDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
            agent.isStopped = false;
        }
    }
}