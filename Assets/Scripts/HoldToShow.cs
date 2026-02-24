using UnityEngine;

public class HoldShow : MonoBehaviour
{
    public GameObject photo;

    private bool wasPressedLastFrame;

    void Start()
    {
        if (photo) photo.SetActive(false);
    }

    void Update()
    {
        if (!photo) return;

        bool pressed =
            Input.GetKey(KeyCode.M) ||
            Input.GetKey(KeyCode.Joystick2Button13);

        photo.SetActive(pressed);

        if (pressed && !wasPressedLastFrame)
        {
            TutorialManager.Instance.MapOpened();

            FindObjectOfType<TaskManager>()?.OnMapOpened();
        }

        wasPressedLastFrame = pressed;
    }
}
