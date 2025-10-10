using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneLoader] Empty scene name.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            return;
        }

        Debug.Log($"[SceneLoader] Loading '{sceneName}'…");
        SceneManager.LoadScene(sceneName);
    }
}
