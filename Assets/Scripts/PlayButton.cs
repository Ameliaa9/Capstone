using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonScript : MonoBehaviour
{
    public string gameSceneName = "Game Scene"; // Manually input the scene name

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
