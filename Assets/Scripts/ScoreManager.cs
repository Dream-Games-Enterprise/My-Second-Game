using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameScoreText;
    int currentScore;
    int currency;

    public float ObstacleMultiplier { get; private set; } = 1.0f;
    public float SpeedMultiplier { get; private set; } = 1.0f;
    public float WinMultiplier { get; private set; } = 1.0f;
    public float TotalMultiplier { get; private set; } = 1.0f;

    int piecesEaten;
    int highScore;

    void Start()
    {
        currentScore = 0;
        highScore = PlayerPrefs.GetInt("highScore");
        UpdateScoreText();
    }

    public void AddScore()
    {
        piecesEaten++;
        int baseScore = 3;
        currentScore += baseScore;
        UpdateScoreText();
    }

    public void ApplyEndMultipliers()
    {
        bool obstaclesEnabled = PlayerPrefs.GetInt("obstacles", 0) == 1;
        int speedInt = PlayerPrefs.GetInt("speed", 1);

        ObstacleMultiplier = obstaclesEnabled ? 1.2f : 1.0f;
        SpeedMultiplier = 1.0f + (speedInt * 0.1f);

        TotalMultiplier = ObstacleMultiplier * SpeedMultiplier;

        currentScore = Mathf.RoundToInt(currentScore * TotalMultiplier);
        UpdateScoreText();
        Debug.Log("Food Pieces Eaten: " + piecesEaten);
        AddCurrency();
        CheckHighScore();
    }

    public void AddWinMultiplier()
    {
        float winMultiplier = 5.0f; 

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

    public void AddCurrency()
    {
        int stashedCurrency = PlayerPrefs.GetInt("currency");
        currency += currentScore;
        int newCurrencyTotal = stashedCurrency + currency;
        PlayerPrefs.SetInt("currency", newCurrencyTotal);

        Debug.Log("New CURRENCY Total: " + newCurrencyTotal);
    }

    public void RemoveCurrency(int currencyToRemove) //this is where buying sprites and such will remove the currency they cost
    {

    }

    void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("highScore", highScore);
            Debug.Log("HIGH SCORE: " + highScore);
        }
        else { Debug.Log("High Score not beaten...");
            Debug.Log("OLD HIGH SCORE: " + highScore);
        }
    }
}
