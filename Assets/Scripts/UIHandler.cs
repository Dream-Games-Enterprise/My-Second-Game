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

    bool isPaused = false;
    Coroutine swipeTextTweenCoroutine;

    void Start()
    {
        int inputType = PlayerPrefs.GetInt("inputType", 1);
        ToggleSwipeText(inputType == 1);

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
        // Disable the pauseObject (likely for hiding the in-game UI)
        pauseObject.SetActive(false);

        // Set the pauseMenu's X position to -290 when the game ends
        RectTransform pauseMenuRect = pauseMenu.GetComponent<RectTransform>();
        if (pauseMenuRect != null)
        {
            Vector3 currentPosition = pauseMenuRect.anchoredPosition;
            currentPosition.x = -200f;  // Set the X position to -290
            pauseMenuRect.anchoredPosition = currentPosition;
        }

        // Enable the pauseMenu buttons, likely for showing the end game options
        pauseMenuButtons.SetActive(true);
    }


    public void ToggleSwipeText(bool isButtonControl)
    {
        swipeText.gameObject.SetActive(!isButtonControl);

        // Start or stop the tween effect based on swipeText's active state
        if (swipeText.gameObject.activeSelf)
        {
            swipeTextTweenCoroutine = StartCoroutine(TweenSwipeText());
        }
        else if (swipeTextTweenCoroutine != null)
        {
            StopCoroutine(swipeTextTweenCoroutine);
            swipeTextTweenCoroutine = null;
            swipeText.transform.localScale = Vector3.one; // Reset scale when not active
        }
    }

    IEnumerator TweenSwipeText()
    {
        Vector3 minScale = Vector3.one * 1f; // Minimum scale factor
        Vector3 maxScale = Vector3.one * 1.8f; // Maximum scale factor
        float duration = 0.5f; // Duration for one expand/shrink cycle

        while (swipeText.gameObject.activeSelf)
        {
            // Scale up
            yield return TweenScale(swipeText.transform, minScale, maxScale, duration);

            // Scale down
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

    public void WinMenu()
    {

    }
}
