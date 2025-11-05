using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject helpPanel;


        [Header("Custom Cursor Objects (UI)")]
        public GameObject player1Cursor;
        public GameObject player2Cursor;

        void Start()
        {
            mainMenuPanel.SetActive(true);
            helpPanel.SetActive(false);


            if (player1Cursor != null) player1Cursor.SetActive(true);
            if (player2Cursor != null) player2Cursor.SetActive(true);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }

        public void OnPlayButton()
        {
            mainMenuPanel.SetActive(false);
            helpPanel.SetActive(false);


            if (player1Cursor != null) player1Cursor.SetActive(false);
            if (player2Cursor != null) player2Cursor.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            GameManager.Instance.GetInputManager.enterExitVehicle = true;
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
    }
}
