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
    [SerializeField] TMP_Text multipliers;

    bool isVictory = false;

    void Awake()
    {
        scoreManager = GetComponent<ScoreManager>();
    }

    void Start()
    {
        gameOverUI.SetActive(false);    
    }

    public void ActivateUI(bool victory = false)
    {
        isVictory = victory;
        UpdateText();
        gameOverUI.SetActive(true);
    }

    void UpdateText()
    {
        totalScoreText.text = "TOTAL SCORE\n" + scoreManager.GetScore().ToString();

        string multiplierText = "MULTIPLIERS\n";

        if (scoreManager.SpeedMultiplier > 1.0f)
        {
            multiplierText += "Speed: x" + scoreManager.SpeedMultiplier.ToString("0.0") + "\n";
        }
        else
        {
            multiplierText += "Speed: N/A\n";
        }

        if (scoreManager.ObstacleMultiplier > 1.0f)
        {
            multiplierText += "Obstacles: x" + scoreManager.ObstacleMultiplier.ToString("0.0") + "\n";
        }
        else
        {
            multiplierText += "Obstacles: N/A\n";
        }

        if (isVictory)
        {
            multiplierText += "Win: x5\n";
        }

        multipliers.text = multiplierText;
    }

}
