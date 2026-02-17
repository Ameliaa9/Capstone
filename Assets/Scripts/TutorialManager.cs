using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;


    [Header("UI")]
    public TMP_Text tutorialText;

    [Header("Tutorial Choice Buttons")]
    public Button startTutorialButton;
    public Button skipTutorialButton;

    [Header("End Tutorial Choice UI")]
    public GameObject tutorialEndChoicePanel;
    public Button redoTutorialButton;
    public Button finishTutorialButton;

    [Header("Upgrade Shop")]
    public GameObject upgradeShopUI;

    [Header("Tutorial Choice UI")]
    public GameObject tutorialChoicePanel;

    [Header("Tutorial Panel")]
    public GameObject tutorialPanel;

    [Header("Input Axes")]
    public string p1MoveAxis = "Joystick1Horizontal";
    public string p2CamX = "Joystick2Horizontal";
    public string p2CamY = "Joystick2Vertical";

    private TutorialStep currentStep;
    private bool actionDone;
    private bool waitingForConfirm;
    private bool tutorialStarted;
    private bool waitingForTutorialChoice;
    private bool pauseOpened;
    private bool waitingForPauseClose;
    private bool upgradeShopOpened;
    private bool upgradeWasOpened;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        tutorialText.text = "";

        if (tutorialChoicePanel != null)
            tutorialChoicePanel.SetActive(false);

        if (startTutorialButton != null)
            startTutorialButton.onClick.AddListener(OnStartTutorialPressed);
        if (skipTutorialButton != null)
            skipTutorialButton.onClick.AddListener(OnSkipTutorialPressed);

        if (redoTutorialButton != null)
            redoTutorialButton.onClick.AddListener(OnRedoTutorialPressed);
        if (finishTutorialButton != null)
            finishTutorialButton.onClick.AddListener(OnFinishTutorialPressed);

        if (tutorialEndChoicePanel != null)
            tutorialEndChoicePanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

    }

    void Update()
    {
        if (waitingForConfirm && Input.GetKeyDown(KeyCode.JoystickButton2))
            waitingForConfirm = false;

        if (!actionDone)
        {
            switch (currentStep)
            {
                case TutorialStep.P1_Move:
                    if (Mathf.Abs(Input.GetAxis(p1MoveAxis)) > 0.2f)
                        ActionCompleted();
                    break;

                case TutorialStep.P2_Camera:
                    if (Mathf.Abs(Input.GetAxis(p2CamX)) > 0.2f ||
                        Mathf.Abs(Input.GetAxis(p2CamY)) > 0.2f)
                        ActionCompleted();
                    break;
            }
        }

        if (currentStep == TutorialStep.UpgradeShop && !actionDone && upgradeShopUI != null)
        {
          
            if (upgradeShopUI.activeSelf)
            {
                upgradeWasOpened = true;
            }
          
            else if (upgradeWasOpened)
            {
                ActionCompleted();
            }
        }

    }

    public void OnStartTutorialPressed()
    {
        if (tutorialChoicePanel != null)
            tutorialChoicePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        GameplayInputEnabled(true);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);


        StartTutorial();
    }

    public void OnSkipTutorialPressed()
    {
        if (tutorialChoicePanel != null)
            tutorialChoicePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        GameplayInputEnabled(true);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        tutorialStarted = true;
    }

    void GameplayInputEnabled(bool enabled)
    {
        foreach (var input in FindObjectsOfType<MonoBehaviour>())
        {
            if (input.CompareTag("Bike"))
                input.enabled = enabled;
        }
    }

    public void StartTutorial()
    {
        if (tutorialStarted) return;

        tutorialStarted = true;
        StartCoroutine(TutorialFlow());
    }

    IEnumerator TutorialFlow()
    {
        // Greeting
        currentStep = TutorialStep.None;
        ShowText($"Hello, Team {TeamNameManager.CurrentTeamName}!\n" + "<size=70%>Press O to continue</size>");
        yield return WaitForSquare();

        ShowText("Welcome to Hamilton Express.\n" + "<size=70%>Press O to continue</size>");
        yield return WaitForSquare();

        // Step 1: Roles
        ShowText(
            "Player 1 controls the bike & Player 2 controls the camera and throws packages\n" +
             "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();

        // Step 2: Player 1 move
        currentStep = TutorialStep.P1_Move;
        actionDone = false;
        ShowText("Player 1: Move the bike");
        yield return new WaitUntil(() => actionDone);

        ShowText("Nice!\n" + "<size=70%>Press O to continue</size>");
        yield return WaitForSquare();

        // Step 3: Player 2 camera
        currentStep = TutorialStep.P2_Camera;
        actionDone = false;
        ShowText("Player 2: Move the camera");
        yield return new WaitUntil(() => actionDone);

        ShowText("Nice!\n" + "<size=70%>Press O to continue</size>");
        yield return WaitForSquare();

        // Step 4: Manhole hazard
        currentStep = TutorialStep.Manhole;
        actionDone = false;
        ShowText("Drive through a manhole");
        yield return new WaitUntil(() => actionDone);

        ShowText(
            "Ouch! You have a health bar...Watch out for hazards around the map!\n" +
             "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();

        // Step 5: Health pickup
        currentStep = TutorialStep.HealthPickup;
        actionDone = false;
        ShowText("Find and pick up a health item");
        yield return new WaitUntil(() => actionDone);

        ShowText(
       "Nice! This restores your health.\n" +
       "<size=70%>Press O to continue</size>"
   );

        yield return WaitForSquare();

        // Step 6: Start delivery at depot
        currentStep = TutorialStep.StartDelivery;
        actionDone = false;
        ShowText("Go to the depot and start a delivery");
        yield return new WaitUntil(() => actionDone);

        ShowText(
            "Delivery started! Follow the minimap and deliver within the time limit.\n" +
             "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();

        // Step 7: Open Map
        currentStep = TutorialStep.OpenMap;
        actionDone = false;
        ShowText("Press touchpad to open the map");
        yield return new WaitUntil(() => actionDone);

        ShowText(
            "Here you can find the exact locations of Delivery points, Depot, Upgrade shop\n" +
             "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();

        // Step 8: Pause game
        currentStep = TutorialStep.PauseGame;
        actionDone = false;
        pauseOpened = false;
        waitingForPauseClose = true;

        ShowText("Press options to pause the game");
        yield return new WaitUntil(() => actionDone);

        ShowText(
            "This is where you can find: help screen, leaderboard, and more \n" +
            "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();

        // Step 9: Deliver package to outlined building
        currentStep = TutorialStep.DeliverPackage;
        actionDone = false;

        ShowText(
            "Go to the delivery location and throw the package at the outlined building"
        );
        yield return new WaitUntil(() => actionDone);

        ShowText(
            "Nice! This is how you earn stars.\n" +
            "The faster your deliveries are, the more stars you earn.\n" +
            "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();

        // Step 10: Upgrade Shop
        currentStep = TutorialStep.UpgradeShop;
        actionDone = false;
        upgradeWasOpened = false;

        ShowText("Go to the upgrade shop");
        yield return new WaitUntil(() => actionDone);

        ShowText(
            "This is where you can buy upgrades using the stars you earn\n" +
            "<size=70%>Press O to continue</size>"
        );
        yield return WaitForSquare();


        currentStep = TutorialStep.None;
        ShowText("Tutorial complete!");

        if (tutorialEndChoicePanel != null)
        {
            tutorialEndChoicePanel.SetActive(true);
            Time.timeScale = 0f;
            if (redoTutorialButton != null)
                redoTutorialButton.Select();
        }
    }

    public void ShowTutorialChoiceAfterDelay(float delay = 2f)
    {
        StartCoroutine(ShowTutorialChoiceCoroutine(delay));
    }

    private IEnumerator ShowTutorialChoiceCoroutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 0f;
        GameplayInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialChoicePanel != null)
        {
            tutorialChoicePanel.SetActive(true);
            if (startTutorialButton != null)
                startTutorialButton.Select();
        }
    }

    public void OnRedoTutorialPressed()
    {
        if (tutorialEndChoicePanel != null)
            tutorialEndChoicePanel.SetActive(false);

        Time.timeScale = 1f;

        StopAllCoroutines();
        tutorialStarted = false;
        actionDone = false;
        waitingForConfirm = false;
        currentStep = TutorialStep.None;

        tutorialText.text = "";

        StartTutorial();
    }

    public void OnFinishTutorialPressed()
    {
        if (tutorialEndChoicePanel != null)
            tutorialEndChoicePanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        tutorialText.text = "";
        tutorialStarted = true;
        Time.timeScale = 1f;
    }

    void ActionCompleted()
    {
        actionDone = true;
    }

    IEnumerator WaitForSquare()
    {
        waitingForConfirm = true;
        yield return new WaitUntil(() => waitingForConfirm == false);
    }

    void ShowText(string msg)
    {
        tutorialText.text = msg;
    }

    public void ManholeTriggered()
    {
        if (currentStep != TutorialStep.Manhole)
            return;

        ActionCompleted();
    }

    public void HealthPickupCollected()
    {
        if (currentStep != TutorialStep.HealthPickup)
            return;

        ActionCompleted();
    }

    public void DeliveryStarted()
    {
        if (currentStep != TutorialStep.StartDelivery)
            return;

        ActionCompleted();
    }

    public void MapOpened()
    {
        if (currentStep != TutorialStep.OpenMap)
            return;

        ActionCompleted();
    }

    public void PauseOpened()
    {
        if (currentStep != TutorialStep.PauseGame)
            return;

        pauseOpened = true;
    }

    public void PauseClosed()
    {
        if (currentStep != TutorialStep.PauseGame)
            return;

        if (!pauseOpened)
            return;

        waitingForPauseClose = false;
        ActionCompleted();
    }

    public void CorrectDeliveryHit()
    {
        if (currentStep != TutorialStep.DeliverPackage)
            return;

        ActionCompleted();
    }


    enum TutorialStep
    {
        None,
        P1_Move,
        P2_Camera,
        Manhole,
        HealthPickup,
        StartDelivery,
        OpenMap,
        PauseGame,
        DeliverPackage,
        UpgradeShop
    }
}
