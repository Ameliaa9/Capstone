using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpButtonScript : MonoBehaviour
{
    public string instructionsSceneName = "Instructions Scene"; // Manually input the scene name

    public void OnHelpButtonPressed()
    {
        SceneManager.LoadScene(instructionsSceneName);
    }
}
