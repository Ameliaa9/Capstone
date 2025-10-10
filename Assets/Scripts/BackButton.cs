using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonScript : MonoBehaviour
{
    public string mainMenuSceneName = "Main Menu"; // Manually input the scene name

    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
