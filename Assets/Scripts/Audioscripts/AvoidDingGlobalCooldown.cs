using UnityEngine;

public class AvoidDingGlobalCooldown : MonoBehaviour
{
    public static AvoidDingGlobalCooldown I { get; private set; }

    [SerializeField] private float cooldownSeconds = 0.8f; 
    private float nextAllowedTime = 0f;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool CanPlay()
    {
        return Time.time >= nextAllowedTime;
    }

    public void Consume()
    {
        nextAllowedTime = Time.time + cooldownSeconds;
    }

    public void SetCooldown(float seconds)
    {
        cooldownSeconds = Mathf.Max(0f, seconds);
    }
}