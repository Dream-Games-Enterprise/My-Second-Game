using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] GameObject leaderboardPanel;
    [SerializeField] GameObject bottomPanel;
    [SerializeField] TMP_Text personalBest;
    [SerializeField] UIPanelAnimator uiPanelAnimator;

    private bool isLeaderboardActive = false;

    void Start()
    {
        leaderboardPanel.SetActive(false);
    }

    public void ToggleLeaderboardPanel()
    {
        isLeaderboardActive = !isLeaderboardActive;

        personalBest.text = "HIGH SCORE  " + PlayerPrefs.GetInt("highScore");

        if (isLeaderboardActive)
        {
            bottomPanel.SetActive(false);
            uiPanelAnimator.AnimateIn(leaderboardPanel);
        }
        else
        {
            uiPanelAnimator.AnimateOut(leaderboardPanel);
            bottomPanel.SetActive(true);
        }
    }
}
