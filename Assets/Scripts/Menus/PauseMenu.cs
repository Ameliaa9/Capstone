using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using KikiNgao.SimpleBikeControl;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject nextPagePanel;
    public GameObject mainMenuPanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button nextPageButton;
    public Button backButton;
    public Button loadSceneButton;
    public Button exitButton;
    public Button mainmenuButton;
    public string sceneToLoad;

    [Header("Main Menu")]
    public GameObject mainMenuCanvas;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        nextPagePanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        nextPageButton.onClick.AddListener(ShowNextPage);
        backButton.onClick.AddListener(ShowPausePage);
       // loadSceneButton.onClick.AddListener(LoadNewScene);
        exitButton.onClick.AddListener(ExitGame);

        mainmenuButton.onClick.AddListener(GoToMainMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        nextPagePanel.SetActive(false);
        isPaused = true;

        AudioManager.I?.PlayMenuToggle();

        GameManager.UnlockCursor();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        TutorialManager.Instance.PauseOpened();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        nextPagePanel.SetActive(false);
        isPaused = false;

        AudioManager.I?.PlayMenuToggle();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.LockCursor();
        TutorialManager.Instance.PauseClosed();
    }

    public void ShowNextPage()
    {
        pausePanel.SetActive(false);
        nextPagePanel.SetActive(true);
    }

    public void ShowPausePage()
    {
        pausePanel.SetActive(true);
        nextPagePanel.SetActive(false);
    }

    public void LoadNewScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.UnlockCursor();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}