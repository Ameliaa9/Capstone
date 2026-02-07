using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public NPCPatrol NPCPatrol;

    public GameObject greenLightCover;
    public GameObject redLightCover;

    public bool isGreen = false;
    public float lightInterval = 10f;
    private float currentTime = 0f;

    private void Start()
    {
        if (isGreen)
        {
            greenLightCover.SetActive(false);
            redLightCover.SetActive(true);
        }
        else
        {
            greenLightCover.SetActive(true);
            redLightCover.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Traffic" && !isGreen)
        {
            Debug.Log("Trigger Entered.");
            NPCPatrol = col.GetComponent<NPCPatrol>();
            NPCPatrol.moveSpeed = 0;
            NPCPatrol.vehicleStopped = true;
            Debug.Log(col);
        }

        
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.GetComponent<NPCPatrol>() == NPCPatrol)
        {
            NPCPatrol = null;
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime > lightInterval)
        {
            ChangeState();
            currentTime -= lightInterval;
        }

        if(isGreen)
        {
            int random = Random.Range(8, 13);
            NPCPatrol.moveSpeed = random;
            NPCPatrol.vehicleStopped = false;
        }
    }

    public void ChangeState()
    {
        if (isGreen)
        {
            isGreen = false;
            Debug.Log("STATE CHANGE RED");
            greenLightCover.SetActive(true);
            redLightCover.SetActive(false);
        }
        else
        {
            isGreen = true;
            Debug.Log("STATE CHANGE GREEN");
            greenLightCover.SetActive(false);
            redLightCover.SetActive(true);
        }
    }

}
