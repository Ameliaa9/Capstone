using UnityEngine;
using UnityEngine.AI;

public class NPCNavAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Wait
    }

    [Header("References")]
    public NavMeshAgent agent;

    [Header("Wander Settings")]
    public float wanderRadius = 40f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 2f;
    public float normalSpeed = 1.8f;

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
        homePosition = transform.position;
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
}