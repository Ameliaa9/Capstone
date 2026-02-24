using UnityEngine;

public class TargetHitOnce : MonoBehaviour
{
    private bool hit = false;

    public bool TryMarkHit()
    {
        if (hit) return false;
        hit = true;
        return true;
    }
}