using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] Button pauseButton;
    [SerializeField] Sprite pauseIcon;      
    [SerializeField] Sprite resumeIcon;
    [SerializeField] GameObject pauseObject;
    [SerializeField] GameObject pauseMenuButtons; 

    bool isPaused = false;   

    void Start()
    {
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

        if (pauseButton != null)
        {
            pauseButton.GetComponent<Image>().sprite = resumeIcon;
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        pauseMenuButtons.SetActive(false);
        pauseObject.SetActive(true);

        if (pauseButton != null)
        {
            pauseButton.GetComponent<Image>().sprite = pauseIcon;
        }
    }

    public void GameEndMenu()
    {
        pauseObject.SetActive(false);
        pauseMenuButtons.SetActive(true);
    }
}
