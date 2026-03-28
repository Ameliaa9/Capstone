using Unity.VisualScripting;
using UnityEngine;

public class npcAlt : MonoBehaviour
{
    [SerializeField]
    private float npcSpeed;

    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Transform npcRelativity;

    private Vector3 npcUpdateDir;

    void Update()
    {
        Vector3 direction = (npcRelativity.position - transform.position).normalized;
        npcSpeed = Random.Range(npcSpeed - 0.2f, npcSpeed + 0.2f);
        if (npcRelativity)
        {
            transform.position += direction * npcSpeed * Time.deltaTime;
        }
        if (npcRelativity)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package") || other.CompareTag("Traffic"))
        {
            Destroy(gameObject);
        }
    }
}
