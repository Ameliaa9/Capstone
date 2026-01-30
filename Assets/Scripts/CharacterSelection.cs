using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelection : MonoBehaviour
{
    public GameObject selectionCanvas;

    [Header("Background Images")]
    public GameObject backgroundP1;
    public GameObject backgroundP2;

    public Transform frontSeatCharacters;
    public Transform backSeatCharacters;

    [Header("Player 2 default selection")]
    public GameObject player2DefaultButton;

    private bool p1Picked = false;

    void Start()
    {
        selectionCanvas.SetActive(true);

        if (backgroundP1) backgroundP1.SetActive(true);
        if (backgroundP2) backgroundP2.SetActive(false);

        ActivateOnly(frontSeatCharacters, -1);
        ActivateOnly(backSeatCharacters, -1);
    }

    public void OnCharacterButtonPressed(int characterIndex)
    {
        bool p1Confirm =
            Input.GetKeyDown(KeyCode.Joystick1Button0) ||
            Input.GetKeyDown(KeyCode.Space);

        bool p2Confirm =
            Input.GetKeyDown(KeyCode.Joystick2Button0) ||
            Input.GetKeyDown(KeyCode.Return);

        if (!p1Picked && !p1Confirm) return;
        if (p1Picked && !p2Confirm) return;

        if (!p1Picked)
        {
            ActivateOnly(frontSeatCharacters, characterIndex);
            p1Picked = true;

            if (backgroundP1) backgroundP1.SetActive(false);
            if (backgroundP2) backgroundP2.SetActive(true);

            if (EventSystem.current != null && player2DefaultButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(player2DefaultButton);
            }
        }
        else
        {
            ActivateOnly(backSeatCharacters, characterIndex);
            selectionCanvas.SetActive(false);
        }
    }

    private void ActivateOnly(Transform parent, int indexToActivate)
    {
        if (parent == null) return;

        for (int i = 0; i < parent.childCount; i++)
            parent.GetChild(i).gameObject.SetActive(i == indexToActivate);
    }
}