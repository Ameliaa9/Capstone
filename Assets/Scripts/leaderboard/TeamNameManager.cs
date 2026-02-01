using UnityEngine;

public class TeamNameManager : MonoBehaviour
{
    public static string CurrentTeamName = "Unknown Team";

    private void Awake()
    {
        if (FindObjectsOfType<TeamNameManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
