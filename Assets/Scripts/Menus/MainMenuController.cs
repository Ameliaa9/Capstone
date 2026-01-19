using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject helpPanel;

        void Start()
        {
            mainMenuPanel.SetActive(true);
            helpPanel.SetActive(false);

            Time.timeScale = 0f;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }

        void Update()
        {
            if (mainMenuPanel.activeSelf && Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                StartGame();
            }
        }

        void StartGame()
        {
            mainMenuPanel.SetActive(false);
            helpPanel.SetActive(false);

            Time.timeScale = 1f;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            GameManager.Instance.GetInputManager.enterExitVehicle = true;
        }

        public void OnPlayButton()
        {
            StartGame();
        }

        public void OnHelpButton()
        {
            mainMenuPanel.SetActive(false);
            helpPanel.SetActive(true);
        }

        public void OnBackButton()
        {
            helpPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }

        public void OnQuitButton()
        {
            Application.Quit();
            Debug.Log("Quit Game");
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }
    }
}
