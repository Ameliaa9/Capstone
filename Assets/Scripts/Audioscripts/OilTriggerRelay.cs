using UnityEngine;

public class OilTriggerRelay : MonoBehaviour
{
    public enum Type { Danger, Hit }
    public Type type;
    public OilAvoidDing owner;

    private void OnTriggerEnter(Collider other)
    {
        if (type == Type.Danger) owner.OnDangerEnter(other);
        if (type == Type.Hit) owner.OnHit(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (type == Type.Danger) owner.OnDangerExit(other);
    }
}