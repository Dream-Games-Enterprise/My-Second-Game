using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] GameObject leaderboardPanel;
    [SerializeField] GameObject bottomPanel;

    void Start()
    {
        leaderboardPanel.SetActive(false);
    }

    public void ToggleLeaderboardPanel()
    {
        bottomPanel.SetActive(!bottomPanel.activeSelf);
        leaderboardPanel.SetActive(!leaderboardPanel.activeSelf);
    }
}
