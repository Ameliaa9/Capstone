using UnityEngine;

public class HoldShow : MonoBehaviour
{
    public GameObject photo;

    void Start()
    {
        if (photo) photo.SetActive(false);
    }

    void Update()
    {
        if (!photo) return;

        bool pressed = Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Joystick2Button13);
        photo.SetActive(pressed);
    }
}
