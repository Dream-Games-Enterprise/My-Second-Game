using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpriteManager : MonoBehaviour
{
    [SerializeField] GameObject snakePanel;
    [SerializeField] GameObject tailPanel;
    [SerializeField] GameObject foodPanel;
    [SerializeField] TMP_Text snakeHeader; //Should say "SNAKE HEAD"
    [SerializeField] TMP_Text tailHeader; //Should say "SNAKE TAIL"
    [SerializeField] TMP_Text foodHeader; //Should say "Food"

    string pointDisplay;
    Coroutine toggleCoroutine;

    void Start()
    {
        DisablePanels();
        pointDisplay = "TOTAL POINTS " + PlayerPrefs.GetInt("currency").ToString();
    }

    public void DisablePanels()
    {
        snakePanel.SetActive(false);
        tailPanel.SetActive(false);
        foodPanel.SetActive(false);

        // Stop the toggle when all panels are closed
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        DisablePanels();
        panelToOpen.SetActive(true);

        if (panelToOpen == snakePanel)
        {
            snakeHeader.text = "TOTAL POINTS " + PlayerPrefs.GetInt("currency").ToString();
            StartToggle(snakeHeader, "SNAKE HEAD");
        }
        else if (panelToOpen == tailPanel)
        {
            tailHeader.text = "TOTAL POINTS " + PlayerPrefs.GetInt("currency").ToString();
            StartToggle(tailHeader, "SNAKE TAIL");
        }
        else if (panelToOpen == foodPanel)
        {
            foodHeader.text = "TOTAL POINTS " + PlayerPrefs.GetInt("currency").ToString();
            StartToggle(foodHeader, "FOOD");
        }
    }

    void StartToggle(TMP_Text header, string originalText)
    {
        // Stop any ongoing toggles
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }
        toggleCoroutine = StartCoroutine(ToggleHeaderText(header, originalText));
    }

    IEnumerator ToggleHeaderText(TMP_Text header, string originalText)
    {
        while (true)
        {
            // Fade out the current text
            yield return StartCoroutine(FadeOutText(header));

            // Update the text to the first string (originalText) after fading out
            header.text = originalText;

            // Fade in the new text
            yield return StartCoroutine(FadeInText(header));

            // Wait for 1 second
            yield return new WaitForSeconds(1f);

            // Fade out the current text
            yield return StartCoroutine(FadeOutText(header));

            // Update the text to the second string (pointDisplay) after fading out
            pointDisplay = "TOTAL POINTS " + PlayerPrefs.GetInt("currency").ToString();
            header.text = pointDisplay;

            // Fade in the new text
            yield return StartCoroutine(FadeInText(header));

            // Wait for 1 second
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator FadeOutText(TMP_Text header)
    {
        // Get the current color and target transparent color
        Color originalColor = header.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        // Fade out
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            header.color = Color.Lerp(originalColor, transparentColor, t);
            yield return null;
        }
        header.color = transparentColor;
    }

    IEnumerator FadeInText(TMP_Text header)
    {
        // Get the current color and target opaque color
        Color transparentColor = header.color;
        Color originalColor = new Color(transparentColor.r, transparentColor.g, transparentColor.b, 1);

        // Fade in
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            header.color = Color.Lerp(transparentColor, originalColor, t);
            yield return null;
        }
        header.color = originalColor;
    }
}
