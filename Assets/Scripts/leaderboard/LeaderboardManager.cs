using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LeaderboardEntry
{
    public string teamName;
    public float score;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
    private string currentTeamName;

    const string SAVE_KEY = "LEADERBOARD";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else Destroy(gameObject);
    }

    public void SetCurrentTeamName(string name)
    {
        currentTeamName = name;
    }

    public void AddScore(float score)
    {
        leaderboard.Add(new LeaderboardEntry
        {
            teamName = currentTeamName,
            score = score
        });

        leaderboard.Sort((a, b) => a.score.CompareTo(b.score));
        Save();
    }

    void Save()
    {
        string json = JsonUtility.ToJson(new Wrapper { list = leaderboard });
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;
        leaderboard = JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString(SAVE_KEY)).list;
    }

    public void ResetLeaderboard()
    {
        leaderboard.Clear();             
        PlayerPrefs.DeleteKey(SAVE_KEY); 
        PlayerPrefs.Save();                
        Debug.Log("Leaderboard reset!");   
    }

    public LeaderboardEntry GetBestEntryForTeam(string teamName)
    {
        LeaderboardEntry best = null;

        foreach (var entry in leaderboard)
        {
            if (entry.teamName != teamName) continue;

            if (best == null || entry.score < best.score)
                best = entry;
        }

        return best;
    }

    public int GetRankOfEntry(LeaderboardEntry entry)
    {
        if (entry == null) return -1;
        return leaderboard.IndexOf(entry); 
    }



    [System.Serializable]
    class Wrapper
    {
        public List<LeaderboardEntry> list;
    }
}
