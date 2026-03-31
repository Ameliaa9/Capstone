using UnityEngine;
using UnityEngine.AI;

public class GooseAnimatorSync : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    public float moveThreshold = 0.05f;

    private void Update()
    {
        if (agent == null || animator == null)
            return;

        bool isWalking = agent.velocity.magnitude > moveThreshold;
        animator.SetBool("isWalking", isWalking);
    }
}