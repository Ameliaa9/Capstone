using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class DeliveryBoard : MonoBehaviour
{
    public DeliverySystem deliverySystem;
    public GameObject deliveryUI;
    public ScreenFader screenFader;

    public Image[] deliveryIcons;
    public TextMeshProUGUI[] deliveryTexts;

    public int randomRoll;
    public int myInt;

    public int[] buttons;

    public AudioSource audioSource;
    public AudioClip garageOpenSFX;
    public AudioClip garageCloseSFX;

    private bool isTransitioning = false;
    private bool boardOpen = false;

    private void Start()
    {
        deliveryUI = GameObject.Find("DepotUI");
        deliveryUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning || boardOpen) return;

        if (other.gameObject.CompareTag("Bike") && deliverySystem.currentPackages < deliverySystem.maxPackages)
        {
            StartCoroutine(OpenDeliveryBoardRoutine());
        }
    }

    private IEnumerator OpenDeliveryBoardRoutine()
    {
        isTransitioning = true;

        if (audioSource != null && garageOpenSFX != null)
        {
            audioSource.PlayOneShot(garageOpenSFX);
        }

        if (screenFader != null)
        {
            screenFader.SetSortOrder(50);
            yield return StartCoroutine(screenFader.FadeOut());
        }

        buttons = new int[3] { 999, 999, 999 };
        deliveryUI.SetActive(true);
        boardOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (myInt = 0; myInt < 3; myInt++)
        {
            RollDelivery();
        }

        isTransitioning = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Bike") && !boardOpen && !isTransitioning)
        {
            deliveryUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseDeliveryBoard()
    {
        if (!boardOpen || isTransitioning) return;

        if (audioSource != null && garageCloseSFX != null)
        {
            audioSource.PlayOneShot(garageCloseSFX);
        }

        StartCoroutine(CloseDeliveryBoardRoutine());
    }

    private IEnumerator CloseDeliveryBoardRoutine()
    {
        isTransitioning = true;

        deliveryUI.SetActive(false);
        boardOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeIn());
        }

        isTransitioning = false;
    }

    public void RollDelivery()
    {
        randomRoll = UnityEngine.Random.Range(0, deliverySystem.Deliveries.Length);
        Debug.Log(randomRoll);

        if (buttons.Contains(randomRoll) || deliverySystem.currentDelivery == randomRoll)
        {
            RollDelivery();
        }
        else
        {
            deliveryIcons[myInt].sprite = deliverySystem.Deliveries[randomRoll].customerIcon;
            buttons[myInt] = randomRoll;
            CompileDeliveryString();
        }
    }

    public void CompileDeliveryString()
    {
        string customerName = deliverySystem.Deliveries[randomRoll].name;
        string customerLocation = deliverySystem.Deliveries[randomRoll].location;
        string customerTime = deliverySystem.Deliveries[randomRoll].deliveryTime.ToString();
        string customerDifficulty = deliverySystem.Deliveries[randomRoll].difficulty;

        string finalString = customerName + " - " + customerLocation + " - Time: " + customerTime + " - " + customerDifficulty;
        deliveryTexts[myInt].text = finalString;
    }
}