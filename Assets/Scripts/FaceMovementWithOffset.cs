using UnityEngine;
using UnityEngine.AI;

public class FaceMovementWithOffset : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform visualRoot;
    public float turnSpeed = 12f;
    public float yOffset = 180f;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent == null || visualRoot == null)
            return;

        Vector3 moveDir = agent.velocity;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized);
            targetRot *= Quaternion.Euler(0f, yOffset, 0f);

            visualRoot.rotation = Quaternion.Slerp(
                visualRoot.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }
    }
}