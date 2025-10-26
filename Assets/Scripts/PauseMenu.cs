using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using KikiNgao.SimpleBikeControl;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject nextPagePanel;

    public Button resumeButton;
    public Button nextPageButton;
    public Button backButton;
    public Button loadSceneButton; 

    public string sceneToLoad; 

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        nextPagePanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        nextPageButton.onClick.AddListener(ShowNextPage);
        backButton.onClick.AddListener(ShowPausePage);
        loadSceneButton.onClick.AddListener(LoadNewScene); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        GameManager.UnlockCursor();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        nextPagePanel.SetActive(false);
        isPaused = false;
        GameManager.LockCursor();
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
}
