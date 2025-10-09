using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "SampleScene 1"; // Default scene name

    public void LoadScene()
    {
        // Check if the scene exists in the build settings
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{targetSceneName}' not found in build settings.");
        }
    }
}
