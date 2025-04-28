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
        int rawScore = scoreManager.GetRawScore();
        int pickups = scoreManager.GetPiecesEaten();

        string combinedText = "";
        combinedText += "Consumed " + pickups + "x  =  " + rawScore + "\n";
        combinedText += "Speed " + scoreManager.SpeedMultiplier.ToString("0.0") + "x\n";

        if (scoreManager.ObstacleMultiplier > 1.0f)
        {
            combinedText += "Obstacles " + scoreManager.ObstacleMultiplier.ToString("0.0") + "x\n";
        }

        if (isVictory)
        {
            combinedText += "Win 5x";
        }

        multipliersText.text = combinedText;
    }
}
