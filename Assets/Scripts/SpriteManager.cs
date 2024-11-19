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
        pointDisplay = "TOTAL POINTS\n" + PlayerPrefs.GetInt("currency").ToString();
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
            snakeHeader.text = "TOTAL POINTS\n" + PlayerPrefs.GetInt("currency").ToString();
            StartToggle(snakeHeader, "SNAKE HEAD");
        }
        else if (panelToOpen == tailPanel)
        {
            tailHeader.text = "TOTAL POINTS\n" + PlayerPrefs.GetInt("currency").ToString();
            StartToggle(tailHeader, "SNAKE TAIL");
        }
        else if (panelToOpen == foodPanel)
        {
            foodHeader.text = "TOTAL POINTS\n" + PlayerPrefs.GetInt("currency").ToString();
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
            yield return StartCoroutine(FadeOutText(header));

            header.text = originalText;

            yield return StartCoroutine(FadeInText(header));

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(FadeOutText(header));

            pointDisplay = "TOTAL POINTS\n" + PlayerPrefs.GetInt("currency").ToString();
            header.text = pointDisplay;

            yield return StartCoroutine(FadeInText(header));

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator FadeOutText(TMP_Text header)
    {
        Color originalColor = header.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            header.color = Color.Lerp(originalColor, transparentColor, t);
            yield return null;
        }
        header.color = transparentColor;
    }

    IEnumerator FadeInText(TMP_Text header)
    {
        Color transparentColor = header.color;
        Color originalColor = new Color(transparentColor.r, transparentColor.g, transparentColor.b, 1);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            header.color = Color.Lerp(transparentColor, originalColor, t);
            yield return null;
        }
        header.color = originalColor;
    }

    public void UpdateCurrencyHeader(int newCurrency)
    {
        snakeHeader.text = "TOTAL POINTS\n" + newCurrency.ToString();
        tailHeader.text = "TOTAL POINTS\n" + newCurrency.ToString();
        foodHeader.text = "TOTAL POINTS\n" + newCurrency.ToString();
    }

    public void ShowNotEnoughPointsMessage()
    {
        // Show a "Not Enough Points" message in the header
        snakeHeader.text = "NOT ENOUGH POINTS";
        tailHeader.text = "NOT ENOUGH POINTS";
        foodHeader.text = "NOT ENOUGH POINTS";

        // Optionally, you can show a temporary message or fade it back after some time
        StartCoroutine(ResetHeaderAfterDelay());
    }

    IEnumerator ResetHeaderAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        UpdateCurrencyHeader(PlayerPrefs.GetInt("currency"));
    }
}
