using UnityEngine;

public class ForwardLooper : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float loopDistance = 20f;

    [Header("Direction")]
    public Space space = Space.Self;

    private Vector3 originPosition;
    private float travelledDistance;

    void Start()
    {
        originPosition = transform.position;
        travelledDistance = 0f;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        transform.Translate(Vector3.forward * step, space);
        travelledDistance += step;

        if (travelledDistance >= loopDistance)
        {
            transform.position = originPosition;
            travelledDistance = 0f;
        }
    }

    // Call this if the object gets repositioned and you want a new origin
    public void ResetOrigin()
    {
        originPosition = transform.position;
        travelledDistance = 0f;
    }
}