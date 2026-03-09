using UnityEngine;

public class OilAvoidDing : MonoBehaviour
{
    [Header("Triggers")]
    [SerializeField] private Collider dangerTrigger;
    [SerializeField] private Collider hitTrigger;

    [Header("SFX")]
    [SerializeField] private AudioClip avoidClip;
    [Range(0f, 1f)][SerializeField] private float volume = 0.55f;

    [Header("Rules")]
    [SerializeField] private float avoidWindowSeconds = 1.0f;
    [SerializeField] private float perOilCooldownSeconds = 1.2f;

    private bool inDanger = false;
    private bool hitOccurred = false;
    private float enterDangerTime = -999f;
    private float nextPerOilAllowedTime = 0f;

    void Reset()
    {
        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (!c.isTrigger) continue;
            if (c.name.ToLower().Contains("danger")) dangerTrigger = c;
            if (c.name.ToLower().Contains("hit")) hitTrigger = c;
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Bike")) return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Bike");
    }

    public void OnDangerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (Time.time < nextPerOilAllowedTime) return;

        inDanger = true;
        hitOccurred = false;
        enterDangerTime = Time.time;
    }

    public void OnDangerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (!inDanger) return;

        inDanger = false;

        if (Time.time < nextPerOilAllowedTime) return;
        if (hitOccurred) return;

        float dt = Time.time - enterDangerTime;
        if (dt <= avoidWindowSeconds)
        {
            if (AvoidDingGlobalCooldown.I != null && !AvoidDingGlobalCooldown.I.CanPlay())
                return;

            AvoidDingGlobalCooldown.I?.Consume();
            nextPerOilAllowedTime = Time.time + perOilCooldownSeconds;

            AudioManager.I?.PlaySFX(avoidClip, volume);
        }
    }

    public void OnHit(Collider other)
    {
        if (!IsPlayer(other)) return;
        hitOccurred = true;
    }
}
