using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    [SerializeField] Button pauseButton;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] Image pauseAndResume;
    [SerializeField] Sprite pauseIcon;      
    [SerializeField] Sprite resumeIcon;
    [SerializeField] GameObject pauseObject;
    [SerializeField] GameObject pauseMenuButtons;
    [SerializeField] TMP_Text swipeText;
    [SerializeField] GameObject leaderboardButton;

    bool isPaused = false;
    Coroutine swipeTextTweenCoroutine;

    void Start()
    {
        int inputType = PlayerPrefs.GetInt("inputType", 1);
        ToggleSwipeText(inputType == 1);

        pauseButton.onClick.AddListener(TogglePause);

        pauseMenuButtons.SetActive(false);
        leaderboardButton.SetActive(false);
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

        RectTransform pauseMenuRect = pauseMenu.GetComponent<RectTransform>();
        if (pauseMenuRect != null)
        {
            Vector3 currentPosition = pauseMenuRect.anchoredPosition;
            currentPosition.x = -375f;
            pauseMenuRect.anchoredPosition = currentPosition;
        }

        pauseMenuButtons.SetActive(true);
        leaderboardButton.SetActive(true); 
    }

    public void ToggleSwipeText(bool isButtonControl)
    {
        swipeText.gameObject.SetActive(!isButtonControl);

        if (swipeText.gameObject.activeSelf)
        {
            swipeTextTweenCoroutine = StartCoroutine(TweenSwipeText());
        }
        else if (swipeTextTweenCoroutine != null)
        {
            StopCoroutine(swipeTextTweenCoroutine);
            swipeTextTweenCoroutine = null;
            swipeText.transform.localScale = Vector3.one; 
        }
    }

    IEnumerator TweenSwipeText()
    {
        Vector3 minScale = Vector3.one * 1f; 
        Vector3 maxScale = Vector3.one * 1.8f; 
        float duration = 0.5f; 

        while (swipeText.gameObject.activeSelf)
        {
            yield return TweenScale(swipeText.transform, minScale, maxScale, duration);

            yield return TweenScale(swipeText.transform, maxScale, minScale, duration);
        }
    }

    IEnumerator TweenScale(Transform target, Vector3 start, Vector3 end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            target.localScale = Vector3.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.localScale = end;
    }
}
