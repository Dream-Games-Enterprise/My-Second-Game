using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dan.Main;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] GameObject leaderboardPanel;
    [SerializeField] GameObject bottomPanel;
    [SerializeField] UIPanelAnimator uiPanelAnimator;

    [SerializeField] TMP_Text personalBest;
    [SerializeField] TMP_InputField nameInputField;

    int highScore;

    bool isLeaderboardActive = false;

    void Start()
    {
        leaderboardPanel.SetActive(false);
        highScore = PlayerPrefs.GetInt("highScore", 0);
        Debug.Log(highScore);
    }

    public void ConfirmAndUploadScore()
    {
        string playerName = nameInputField.text;

        if (playerName.Length < 3 || playerName.Length > 20)
        {
            Debug.LogWarning("Player name must be between 3 and 20 characters.");
            return;
        }

        UploadScoreToLeaderboard(playerName, PlayerPrefs.GetInt("highScore", 0));
    }

    void UploadScoreToLeaderboard(string username, int score)
    {
        Leaderboards.FakeSnake.UploadNewEntry(username, score, success =>
        {
            if (success)
            {
                Debug.Log("Score uploaded successfully!");
                //CacheLeaderboardEntries(); // Refresh cached data
            }
            else
            {
                Debug.LogWarning("Failed to upload score.");
            }
        });
    }

    public void LoadPersonalBest()
    {
        int latestScore = PlayerPrefs.GetInt("highScore", 0);
        personalBest.text = $"HIGH SCORE  " + highScore;
    }
}