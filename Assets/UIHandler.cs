using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    [SerializeField] Button pauseButton;
    [SerializeField] Image pauseAndResume;
    [SerializeField] Sprite pauseIcon;      
    [SerializeField] Sprite resumeIcon;
    [SerializeField] GameObject pauseObject;
    [SerializeField] GameObject pauseMenuButtons;
    [SerializeField] TMP_Text swipeText;

    bool isPaused = false;   

    void Start()
    {
        int inputType = PlayerPrefs.GetInt("inputType", 1);  // Default to 1 (button control) if not set
        ToggleSwipeText(inputType == 1); // If inputType is 1 (button control), hide swipe text

        pauseButton.onClick.AddListener(TogglePause);

        pauseMenuButtons.SetActive(false);
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f; 

        pauseMenuButtons.SetActive(true);

        if (pauseAndResume != null)
        {
            pauseAndResume.sprite = resumeIcon;
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        pauseMenuButtons.SetActive(false);
        pauseObject.SetActive(true);

        if (pauseAndResume != null)
        {
            pauseAndResume.sprite = pauseIcon;
        }
    }

    public void GameEndMenu()
    {
        pauseObject.SetActive(false);
        pauseMenuButtons.SetActive(true);
    }

    public void ToggleSwipeText(bool isButtonControl)
    {
        swipeText.gameObject.SetActive(!isButtonControl);
    }


    public void WinMenu()
    {

    }
}
