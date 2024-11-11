using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameScoreText;
    int currentScore;

    public float ObstacleMultiplier { get; private set; } = 1.0f;
    public float SpeedMultiplier { get; private set; } = 1.0f;
    public float WinMultiplier { get; private set; } = 1.0f;
    public float TotalMultiplier { get; private set; } = 1.0f;

    void Start()
    {
        currentScore = 0;
        UpdateScoreText();
    }

    public void AddScore()
    {
        int baseScore = 5;
        currentScore += baseScore;
        UpdateScoreText();
    }

    public void ApplyEndMultipliers()
    {
        bool obstaclesEnabled = PlayerPrefs.GetInt("obstacles", 0) == 1;
        int speedInt = PlayerPrefs.GetInt("speed", 1);

        // Apply obstacle and speed multipliers based on player preferences
        ObstacleMultiplier = obstaclesEnabled ? 1.2f : 1.0f;
        SpeedMultiplier = 1.0f + (speedInt * 0.1f);

        // Calculate the total multiplier before applying the score adjustments
        TotalMultiplier = ObstacleMultiplier * SpeedMultiplier;

        // Apply the total multiplier to the score
        currentScore = Mathf.RoundToInt(currentScore * TotalMultiplier);
        UpdateScoreText();
    }

    // Apply the win multiplier and combine it with the other multipliers
    public void AddWinMultiplier()
    {
        float winMultiplier = 5.0f; // Set the win multiplier to 5

        // Apply the win multiplier on top of existing multipliers
        currentScore = Mathf.RoundToInt(currentScore * winMultiplier);
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (gameScoreText != null)
        {
            gameScoreText.text = currentScore.ToString();
        }
    }

    public int GetScore()
    {
        return currentScore;
    }
}
