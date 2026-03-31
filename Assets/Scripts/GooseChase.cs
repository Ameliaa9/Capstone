using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GooseChase : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Target")]
    public Transform player;
    public string bikeTag = "Bike";

    [Header("Chase")]
    public float chaseDuration = 8f;
    public float ignoreHitTimeAtStart = 0.5f;
    public float hitCooldown = 0.3f;

    private float lastHitTime = -999f;
    private Coroutine chaseRoutine;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.autoBraking = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < ignoreHitTimeAtStart)
            return;

        if (!collision.gameObject.CompareTag(bikeTag))
            return;

        if (Time.time - lastHitTime < hitCooldown)
            return;

        lastHitTime = Time.time;

        if (player == null)
            player = collision.transform;

        if (chaseRoutine != null)
            StopCoroutine(chaseRoutine);

        chaseRoutine = StartCoroutine(ChaseRoutine());
    }

    private IEnumerator ChaseRoutine()
    {
        if (agent == null || player == null)
            yield break;

        if (!agent.isOnNavMesh)
            yield break;

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(player.position);

        float timer = 0f;

        while (timer < chaseDuration)
        {
            timer += Time.deltaTime;

            if (agent != null && agent.isOnNavMesh && player != null)
            {
                agent.SetDestination(player.position);
            }

            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }
}