using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dan.Main;

namespace LeaderboardCreatorDemo
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] GameObject leaderboardPanel;
        [SerializeField] GameObject bottomPanel;
        [SerializeField] UIPanelAnimator uiPanelAnimator;

        [SerializeField] TMP_Text[] nameFields;
        [SerializeField] TMP_Text[] scoreFields;

        [SerializeField] TMP_Text personalBest;
        [SerializeField] TMP_InputField nameInputField;

        int highScore;

        bool isLeaderboardActive = false;

        private string[] cachedNames = new string[25];
        private string[] cachedScores = new string[25];

        void Start()
        {
            leaderboardPanel.SetActive(false);
            highScore = PlayerPrefs.GetInt("highScore", 0);
        }

        public void CacheLeaderboardEntries()
        {
            Debug.Log("Attempting to fetch leaderboard entries...");

            Leaderboards.FakeSnake.GetEntries(entries =>
            {
                Debug.Log($"Entries fetched from server: {entries.Length}");

                int total = Mathf.Min(25, entries.Length);

                for (int i = 0; i < total; i++)
                {
                    cachedNames[i] = entries[i].Username;
                    cachedScores[i] = entries[i].Score.ToString();

                    Debug.Log($"Cached Entry #{i + 1}: {cachedNames[i]} - {cachedScores[i]}");
                }

                for (int i = total; i < 25; i++)
                {
                    cachedNames[i] = "";
                    cachedScores[i] = "";
                }

                Debug.Log("Finished caching entries. Calling DisplayCachedEntries()...");
                DisplayCachedEntries();
            });
        }

        public void DisplayCachedEntries()
        {
            Debug.Log("Displaying cached leaderboard entries...");

            int count = Mathf.Min(25, nameFields.Length, scoreFields.Length);

            for (int i = 0; i < count; i++)
            {
                nameFields[i].text = cachedNames[i] ?? "";
                scoreFields[i].text = cachedScores[i] ?? "";

                Debug.Log($"Displayed Entry #{i + 1}: {nameFields[i].text} - {scoreFields[i].text}");
            }

            personalBest.text = $"High Score  {highScore}";
            Debug.Log($"Displayed personal best: {highScore}");
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

        public void LoadScores()
        {
            CacheLeaderboardEntries();
            DisplayCachedEntries();
        }

        void UploadScoreToLeaderboard(string username, int score)
        {
            Leaderboards.FakeSnake.UploadNewEntry(username, score, success =>
            {
                if (success)
                {
                    Debug.Log("Score uploaded successfully!");
                    CacheLeaderboardEntries(); 
                }
                else
                {
                    Debug.LogWarning("Failed to upload score.");
                }
            });
        }

        public void LoadPersonalBest()
        {
            highScore = PlayerPrefs.GetInt("highScore", 0);
            personalBest.text = $"HIGH SCORE  {highScore}";
        }
    }
}