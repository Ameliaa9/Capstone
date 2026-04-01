using UnityEngine;
using UnityEngine.AI;

public class ZoneNPCController : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private MonoBehaviour[] behaviourScripts;
    [SerializeField] private Animator animator;

    [Header("State")]
    [SerializeField] private bool startActive = false;

    private void Awake()
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        SetActiveState(startActive);
    }

    public void SetActiveState(bool isActive)
    {
        if (navMeshAgent != null)
            navMeshAgent.enabled = isActive;

        if (behaviourScripts != null)
        {
            foreach (MonoBehaviour script in behaviourScripts)
            {
                if (script != null)
                    script.enabled = isActive;
            }
        }

        if (animator != null)
            animator.enabled = isActive;
    }
}
