using UnityEngine;
using UnityEngine.AI;

public class NPCNavAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Wait,
        Chase
    }

    [Header("References")]
    public NavMeshAgent agent;
    public Transform playerTarget;

    [Header("Spawn Settings")]
    public float initialScatterRadius = 20f;

    [Header("Wander Settings")]
    public float wanderRadius = 40f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 2f;
    public float normalSpeed = 1.8f;

    [Header("Chase Settings")]
    public float chaseSpeed = 3.8f;
    public float chaseDuration = 3f;
    public float loseDistance = 20f;

    [Header("Area")]
    public Vector3 homePosition;

    [Header("Debug")]
    public State currentState = State.Wander;

    private float stateTimer;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        ScatterOnStart();
        agent.speed = normalSpeed;
        PickNewWanderPoint();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Wander:
                UpdateWander();
                break;

            case State.Wait:
                UpdateWait();
                break;

            case State.Chase:
                UpdateChase();
                break;
        }
    }

    void ScatterOnStart()
    {
        Vector3 basePos = transform.position;

        for (int i = 0; i < 12; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * initialScatterRadius;
            Vector3 testPoint = basePos + new Vector3(random2D.x, 0f, random2D.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(testPoint, out hit, 3f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                homePosition = hit.position;
                return;
            }
        }

        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(basePos, out fallbackHit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(fallbackHit.position);
            homePosition = fallbackHit.position;
        }
        else
        {
            homePosition = transform.position;
        }
    }

    void UpdateWander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Wait;
            stateTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    void UpdateWait()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = State.Wander;
            PickNewWanderPoint();
        }
    }

    void UpdateChase()
    {
        if (playerTarget == null)
        {
            ReturnToWander();
            return;
        }

        stateTimer -= Time.deltaTime;
        agent.SetDestination(playerTarget.position);

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (stateTimer <= 0f || dist > loseDistance)
        {
            ReturnToWander();
        }
    }

    void PickNewWanderPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * wanderRadius;
            Vector3 randomPoint = homePosition + new Vector3(random2D.x, 0f, random2D.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
            {
                agent.speed = normalSpeed;
                agent.SetDestination(hit.position);
                return;
            }
        }

        agent.SetDestination(homePosition);
    }

    public void TriggerChase(Transform target)
    {
        if (target == null) return;

        playerTarget = target;
        currentState = State.Chase;
        stateTimer = chaseDuration;
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTarget.position);
    }

    void ReturnToWander()
    {
        playerTarget = null;
        currentState = State.Wander;
        agent.speed = normalSpeed;
        PickNewWanderPoint();
    }
}