using UnityEngine;
using UnityEngine.AI;

public class GooseAI : MonoBehaviour
{
    public Transform player;

    public float wanderRadius = 5f;
    public float pauseTime = 2f;
    public float chaseDistance = 4f;
    public float turnSpeed = 10f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool waiting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        agent.updateRotation = false;

        PickNewDestination();
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < chaseDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!waiting)
                {
                    waiting = true;
                    Invoke(nameof(PickNewDestination), pauseTime);
                }
            }
        }

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 8f
            );
        }

        animator.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
    }

    void HandleRotation()
    {
        Vector3 dir = agent.desiredVelocity;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * turnSpeed
            );
        }
    }

    void PickNewDestination()
    {
        Vector3 rand = Random.insideUnitSphere * wanderRadius + transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(rand, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        waiting = false;
    }
}
