using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Manually assign root GameObjects for Play and Help buttons")]
    public GameObject playButtonObj; // assign "Rectangle 1"
    public GameObject helpButtonObj; // assign "Rectangle 2"

    private Button playButton;
    private Button helpButton;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(UpdateButtonsRoutine());
    }

    private IEnumerator UpdateButtonsRoutine()
    {
        while (true)
        {
            // Update Play button
            if (playButton == null || !playButton.gameObject.activeInHierarchy)
            {
                if (playButtonObj != null)
                {
                    playButton = FindButtonDeep(playButtonObj.transform);
                    if (playButton != null)
                    {
                        playButton.onClick.RemoveAllListeners();
                        playButton.onClick.AddListener(PlayGame);
                        Debug.Log("Play button assigned successfully.");
                    }
                }
            }

            // Update Help button
            if (helpButton == null || !helpButton.gameObject.activeInHierarchy)
            {
                if (helpButtonObj != null)
                {
                    helpButton = FindButtonDeep(helpButtonObj.transform);
                    if (helpButton != null)
                    {
                        helpButton.onClick.RemoveAllListeners();
                        helpButton.onClick.AddListener(ShowHelp);
                        Debug.Log("Help button assigned successfully.");
                    }
                }
            }

            yield return new WaitForSeconds(0.5f); // check twice per second
        }
    }

    // Recursively searches a transform and its children for a Button component
    private Button FindButtonDeep(Transform parent)
    {
        Button btn = parent.GetComponent<Button>();
        if (btn != null) return btn;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            btn = child.GetComponent<Button>();
            if (btn != null) return btn;
        }
        return null;
    }

    private void PlayGame()
    {
        SceneManager.LoadScene("SampleScene 1");
    }

    private void ShowHelp()
    {
        // Implement help screen logic here
    }
}
