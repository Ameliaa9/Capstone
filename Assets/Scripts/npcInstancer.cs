using UnityEngine;

public class npcInstancer : MonoBehaviour
{
    [SerializeField]
    private GameObject[] npcs;

    [SerializeField]
    private int npcListInstance;

    [SerializeField]
    private int instanceQuantity;

    [SerializeField]
    private float instanceRadius = 10f;

    [SerializeField]
    private float minimumRadius = 0f;

    private void Start()
    {
        for (int i = 0; i < instanceQuantity ; i++)
        {
            Instance(npcListInstance);
        }
    }

    public void Instance(int npcIndex)
    {
        float rand0 = Random.Range(-instanceRadius, instanceRadius);
        float rand1 = Random.Range(-instanceRadius, instanceRadius);
        Vector3 randomDir = new Vector3 (rand0, 0, rand1);

        if (minimumRadius > 0)
        {
            randomDir = randomDir.normalized * Random.Range(minimumRadius, instanceRadius);
        }

        Vector3 instancePos = transform.position + randomDir;

        GameObject newInstance = Instantiate(npcs[npcIndex], instancePos, Quaternion.identity);
        newInstance.SetActive(true);

    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, instanceRadius);
    }
}
