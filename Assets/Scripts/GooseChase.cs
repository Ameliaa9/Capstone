using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GooseChase : MonoBehaviour
{
    public enum GooseState
    {
        Wandering,
        Chasing
    }

    public NavMeshAgent agent;

    [Header("Target")]
    public Transform player;
    public string bikeTag = "Bike";

    [Header("Chase")]
    public float chaseDuration = 8f;
    public float ignoreHitTimeAtStart = 0.5f;
    public bool allowChaseRefresh = true;

    [Header("Wander")]
    public float wanderDistance = 8f;
    public float wanderMinWait = 1f;
    public float wanderMaxWait = 2.5f;
    public float navMeshSampleRadius = 4f;

    private Coroutine stateRoutine;
    private GooseState currentState = GooseState.Wandering;
    private float chaseEndTime = -1f;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agent == null)
            return;

        agent.autoBraking = true;

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        stateRoutine = StartCoroutine(WanderRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < ignoreHitTimeAtStart)
            return;

        if (!collision.gameObject.CompareTag(bikeTag))
            return;

        if (player == null)
            player = collision.transform;

        if (currentState == GooseState.Chasing)
        {
            if (allowChaseRefresh)
            {
                chaseEndTime = Time.time + chaseDuration;
            }
            return;
        }

        StartChasing();
    }

    private void StartChasing()
    {
        if (agent == null || player == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        currentState = GooseState.Chasing;
        chaseEndTime = Time.time + chaseDuration;

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        stateRoutine = StartCoroutine(ChaseRoutine());
    }

    private IEnumerator ChaseRoutine()
    {
        agent.isStopped = false;
        agent.ResetPath();

        while (Time.time < chaseEndTime)
        {
            if (agent != null && agent.isOnNavMesh && player != null)
            {
                agent.SetDestination(player.position);
            }

            yield return null;
        }

        currentState = GooseState.Wandering;

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        stateRoutine = StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        while (currentState == GooseState.Wandering)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                Vector3 nextPoint = GetRandomWanderPoint();

                agent.isStopped = false;
                agent.SetDestination(nextPoint);

                while (agent.pathPending)
                    yield return null;

                while (agent.remainingDistance > agent.stoppingDistance + 0.05f)
                    yield return null;

                agent.ResetPath();
                agent.isStopped = true;
            }

            float waitTime = Random.Range(wanderMinWait, wanderMaxWait);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector3 GetRandomWanderPoint()
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderDistance;
            randomDirection.y = 0f;

            Vector3 target = transform.position + randomDirection;

            if (NavMesh.SamplePosition(target, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return transform.position;
    }
}