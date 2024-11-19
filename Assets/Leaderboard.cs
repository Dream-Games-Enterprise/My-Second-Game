using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] GameObject leaderboardPanel;

    void Start()
    {
        leaderboardPanel.SetActive(false);
    }

    public void ToggleLeaderboardPanel()
    {
        leaderboardPanel.SetActive(!leaderboardPanel.activeSelf);
    }
}
