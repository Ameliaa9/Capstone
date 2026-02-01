using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public Transform scoreListParent;
    public TMP_Text scoreEntryTemplate;

    [Header("Player Best")]
    public TMP_Text finalScoreText;

    [Header("Colors")]
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in scoreListParent)
        {
            if (child == scoreEntryTemplate.transform) continue;
            Destroy(child.gameObject);
        }

        var manager = LeaderboardManager.Instance;
        var leaderboard = manager.leaderboard;

        if (leaderboard.Count == 0)
        {
            TMP_Text empty = Instantiate(scoreEntryTemplate, scoreListParent);
            empty.gameObject.SetActive(true);
            empty.text = "No scores yet";
            empty.color = normalColor;

            finalScoreText.text = "";
            return;
        }

        var bestEntry = manager.GetBestEntryForTeam(TeamNameManager.CurrentTeamName);
        int bestRank = manager.GetRankOfEntry(bestEntry);

        int count = Mathf.Min(10, leaderboard.Count);
        for (int i = 0; i < count; i++)
        {
            var entry = leaderboard[i];

            TMP_Text row = Instantiate(scoreEntryTemplate, scoreListParent);
            row.gameObject.SetActive(true);
            row.text = $"{i + 1}. {entry.teamName} — {entry.score:F1}";

            if (entry == bestEntry)
                row.color = highlightColor;
            else
                row.color = normalColor;
        }

        if (bestEntry != null)
        {
            finalScoreText.text =
                $"Your Best\n{bestEntry.teamName} — {bestEntry.score:F1} (Rank {bestRank + 1})";
        }
        else
        {
            finalScoreText.text = "Your Best\n—";
        }
    }
}
