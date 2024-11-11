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
        //get the text and string logic here
    }
}
