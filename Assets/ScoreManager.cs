using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameScoreText;
    int currentScore;

    void Start()
    {
        currentScore = 0;
        UpdateScoreText();
    }

    public void AddScore()
    {
        int baseScore = 5;
        currentScore += baseScore;

        Debug.Log("Score added: " + baseScore + ", Current Score: " + currentScore);

        UpdateScoreText();
    }

    public void ApplyEndMultipliers()
    {
        bool obstaclesEnabled = PlayerPrefs.GetInt("obstacles", 0) == 1;
        int speedInt = PlayerPrefs.GetInt("speed", 1); // Default speed is set to 1

        float obstacleMultiplier = obstaclesEnabled ? 1.2f : 1.0f;
        float speedMultiplier = 1.0f + (speedInt * 0.1f);

        float totalMultiplier = obstacleMultiplier * speedMultiplier;

        currentScore = Mathf.RoundToInt(currentScore * totalMultiplier);

        Debug.Log("End-game multiplier applied: " + totalMultiplier + ", Final Score: " + currentScore);

        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (gameScoreText != null)
        {
            gameScoreText.text = currentScore.ToString();
        }
        else
        {
            Debug.LogWarning("gameScoreText is not assigned in the inspector.");
        }
    }

    public int GetScore()
    {
        return currentScore;
    }
}
