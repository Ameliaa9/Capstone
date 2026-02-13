using UnityEngine;
using TMPro;

public class TeamNameInput : MonoBehaviour
{
    public TMP_Text teamNameText;
    public int maxLength = 12;

    private string teamName = "";

    public void AddLetter(string letter)
    {
        if (teamName.Length >= maxLength) return;

        teamName += letter;
        teamNameText.text = teamName;
    }

    public void AddSpace()
    {
        if (teamName.Length >= maxLength) return;

        teamName += " ";
        teamNameText.text = teamName;
    }

    public void Backspace()
    {
        if (teamName.Length == 0) return;

        teamName = teamName.Substring(0, teamName.Length - 1);
        teamNameText.text = teamName;
    }

    public void ConfirmName()
    {
        string teamName = teamNameText.text;

        if (string.IsNullOrWhiteSpace(teamName))
            return;

        TeamNameManager.CurrentTeamName = teamName;

        LeaderboardManager.Instance.SetCurrentTeamName(teamName);
        TeamNameOverlayManager.Instance.ConfirmName();


        TutorialManager.Instance.ShowTutorialChoiceAfterDelay(2f);

    }

}
