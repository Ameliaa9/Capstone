using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class JoystickCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public RectTransform cursorRectTransform;
    public RectTransform canvasRectTransform;
    public float speed = 1000f;

    [Header("Bounds")]
    public Vector2 minBounds = new Vector2(-940, -530);
    public Vector2 maxBounds = new Vector2(940, 530);

    [Header("Player Settings")]
    public int playerIndex = 1;

    private Vector2 cursorPosition;
    private PointerEventData pointerEvent;
    private EventSystem eventSystem;

    void Start()
    {
        if (cursorRectTransform == null)
            cursorRectTransform = GetComponent<RectTransform>();

        if (canvasRectTransform == null && cursorRectTransform != null)
            canvasRectTransform = cursorRectTransform.root.GetComponent<RectTransform>();

        cursorPosition = cursorRectTransform.anchoredPosition;

        eventSystem = EventSystem.current;
        pointerEvent = new PointerEventData(eventSystem);

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "start screen" || currentScene == "InfoScene")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void Update()
    {
        MoveCursor();
        ClickUI();
    }

    private void MoveCursor()
    {
        string horizontalAxis = "Joystick" + playerIndex + "Horizontal";
        string verticalAxis = "Joystick" + playerIndex + "Vertical";

        float inputX = Input.GetAxis(horizontalAxis);
        float inputY = Input.GetAxis(verticalAxis) * -1f;

        if (playerIndex == 1 && inputX == 0 && inputY == 0)
        {
            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical") * -1f;
        }

        cursorPosition += new Vector2(inputX, inputY) * speed * Time.unscaledDeltaTime;

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, minBounds.x, maxBounds.x);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, minBounds.y, maxBounds.y);

        cursorRectTransform.anchoredPosition = cursorPosition;
    }

    private void ClickUI()
    {
        pointerEvent.position = cursorRectTransform.position;

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEvent, results);

        foreach (var result in results)
        {
            Selectable selectable = result.gameObject.GetComponent<Selectable>();
            if (selectable != null)
                selectable.OnSelect(pointerEvent);
        }

        KeyCode buttonKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + playerIndex + "Button0");
        if (Input.GetKeyDown(buttonKey))
        {
            pointerEvent.button = PointerEventData.InputButton.Left;
            foreach (var result in results)
            {
                ExecuteEvents.Execute(result.gameObject, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
        }
    }
}
