using UnityEngine;
using UnityEngine.UI;

public class ControllerScrollRect : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 1.5f;

    void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    void Update()
    {
        if (!scrollRect || !scrollRect.gameObject.activeInHierarchy)
            return;

        float p1 = Input.GetAxis("Joystick1Vertical");
        float p2 = Input.GetAxis("Joystick2Vertical");

        float input = Mathf.Abs(p1) > Mathf.Abs(p2) ? p1 : p2;

        if (Mathf.Abs(input) < 0.1f)
            return;

        float value = scrollRect.verticalNormalizedPosition + (input * scrollSpeed * Time.unscaledDeltaTime);
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(value);
    }
}
