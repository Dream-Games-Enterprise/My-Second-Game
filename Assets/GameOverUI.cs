using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    ScoreManager scoreManager;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] TMP_Text totalScoreText;
    [SerializeField] TMP_Text obstacleMultiText;
    [SerializeField] TMP_Text speedMultiText;
    [SerializeField] TMP_Text multipliers;

    void Awake()
    {
        scoreManager = GetComponent<ScoreManager>();
    }

    void Start()
    {
        gameOverUI.SetActive(false);    
    }

    public void ActivateUI()
    {
        UpdateText();
        gameOverUI.SetActive(true);
    }

    void UpdateText()
    {
        totalScoreText.text = "TOTAL SCORE\n" + scoreManager.GetScore().ToString();

        // Initialize an empty string for the multipliers display
        string multiplierText = "MULTIPLIERS\n";

        // Check if obstacle multiplier is active and add it to the display string
        if (scoreManager.ObstacleMultiplier > 1.0f)
        {
            multiplierText += "Obstacles: x" + scoreManager.ObstacleMultiplier.ToString("0.0") + "\n";
        }
        else
        {
            multiplierText += "Obstacles: N/A\n";
        }

        // Check if speed multiplier is active and add it to the display string
        if (scoreManager.SpeedMultiplier > 1.0f)
        {
            multiplierText += "Speed: x" + scoreManager.SpeedMultiplier.ToString("0.0") + "\n";
        }
        else
        {
            multiplierText += "Speed: N/A\n";
        }

        // Set the combined multipliers text
        multipliers.text = multiplierText;
    }

}
