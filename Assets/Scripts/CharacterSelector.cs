using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI")]
    public Image portraitImage;
    public Sprite[] portraits;

    [Header("Characters on Bike Seat")]
    public GameObject[] charactersOnSeat;

    [Header("Lock-In UI")]
    public Image lockedInImage;

    [Header("Player Settings")]
    public int playerIndex = 0;

    private int currentIndex = 0;
    private bool lockedIn = false;
    private bool axisInUse = false;
    private bool submitInUse = false;

    void Start()
    {
        if (lockedInImage != null)
            lockedInImage.gameObject.SetActive(false);

        UpdateSelection();
    }

    void Update()
    {
        if (lockedIn) return;

        float horizontal = Input.GetAxisRaw(
            playerIndex == 0 ? "Joystick1Horizontal" : "Joystick2Horizontal"
        );

        if (!axisInUse)
        {
            if (horizontal > 0.5f)
            {
                Next();
                axisInUse = true;
            }
            else if (horizontal < -0.5f)
            {
                Previous();
                axisInUse = true;
            }
        }

        if (Mathf.Abs(horizontal) < 0.2f)
            axisInUse = false;

        float submit = Input.GetAxisRaw(
            playerIndex == 0 ? "P1_Submit" : "P2_Submit"
        );

        if (!submitInUse && submit > 0.8f)
        {
            LockIn();
            submitInUse = true;
        }

        if (submit < 0.2f)
            submitInUse = false;

        if (playerIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.Period))
                Next();

            if (Input.GetKeyDown(KeyCode.Comma))
                Previous();

            if (Input.GetKeyDown(KeyCode.L))
                LockIn();
        }
        else if (playerIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.X))
                Next();

            if (Input.GetKeyDown(KeyCode.Z))
                Previous();

            if (Input.GetKeyDown(KeyCode.S))
                LockIn();
        }
    }

    void Next()
    {
        currentIndex = (currentIndex + 1) % portraits.Length;
        UpdateSelection();
    }

    void Previous()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = portraits.Length - 1;

        UpdateSelection();
    }

    void UpdateSelection()
    {
        portraitImage.sprite = portraits[currentIndex];

        foreach (GameObject c in charactersOnSeat)
            c.SetActive(false);

        charactersOnSeat[currentIndex].SetActive(true);
    }

    void LockIn()
    {
        lockedIn = true;

        if (lockedInImage != null)
            lockedInImage.gameObject.SetActive(true);

        Debug.Log("P" + (playerIndex + 1) + " locked in");
    }

    public bool IsLockedIn()
    {
        return lockedIn;
    }
}
