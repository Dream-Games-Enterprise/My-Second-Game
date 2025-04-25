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
    [SerializeField] TMP_Text multipliersText;
    [SerializeField] TMP_Text foodScoreText;

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
        totalScoreText.text = "TOTAL SCORE  " + scoreManager.GetScore().ToString();

        string multiplierText = "MULTIPLIERS  |  ";
        multiplierText += "Speed " + scoreManager.SpeedMultiplier.ToString("0.0") + "x  |  ";
        multiplierText += "Obstacles " + scoreManager.ObstacleMultiplier.ToString("0.0") + "x";

        if (isVictory)
        {
            multiplierText += "  |  Win 5x";
        }

        multipliersText.text = multiplierText;

        int rawScore = scoreManager.GetRawScore();
        int pickups = scoreManager.GetPiecesEaten();
        foodScoreText.text = "Consumed  " + pickups.ToString() + "x  =  " + rawScore.ToString();
    }

}
