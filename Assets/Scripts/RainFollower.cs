using UnityEngine;

public class RainFollower : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float heightOffset = 18f;
    public bool followX = true;
    public bool followZ = true;
    public bool smoothFollow = true;
    public float followSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = transform.position;

        if (followX)
            targetPosition.x = target.position.x;

        if (followZ)
            targetPosition.z = target.position.z;

        targetPosition.y = target.position.y + heightOffset;

        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * followSpeed
            );
        }
        else
        {
            transform.position = targetPosition;
        }
    }
}