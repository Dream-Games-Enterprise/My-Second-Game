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
    [SerializeField] GameObject trapPanel;

    [SerializeField] TMP_Text snakeHeader;
    [SerializeField] TMP_Text tailHeader; 
    [SerializeField] TMP_Text foodHeader; 
    [SerializeField] TMP_Text trapHeader; 
    [SerializeField] GameObject bottomPanel;

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
        trapPanel.SetActive(false);

        bottomPanel.SetActive(true);

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
        bottomPanel.SetActive(false);

        if (panelToOpen == snakePanel)
        {
            snakeHeader.text = "HEAD\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
            //StartToggle(snakeHeader, "SNAKE HEAD");
        }
        else if (panelToOpen == tailPanel)
        {
            tailHeader.text = "TAIL\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
            //StartToggle(tailHeader, "SNAKE TAIL");
        }
        else if (panelToOpen == foodPanel)
        {
            foodHeader.text = "FOOD\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
            //StartToggle(foodHeader, "FOOD");
        }
        else if (panelToOpen == trapPanel)
        {
            trapHeader.text = "TRAP\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
            //StartToggle(trapHeader, "TRAP");
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

            yield return new WaitForSeconds(1.5f);

            yield return StartCoroutine(FadeOutText(header));

            pointDisplay = "TOTAL POINTS\n" + PlayerPrefs.GetInt("currency").ToString();
            header.text = pointDisplay;

            yield return StartCoroutine(FadeInText(header));

            yield return new WaitForSeconds(1.5f);
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

    public void ShowUnlockSuccessfulMessage()
    {
        snakeHeader.text = "UNLOCK SUCCESSFUL";
        tailHeader.text = "UNLOCK SUCCESSFUL";
        foodHeader.text = "UNLOCK SUCCESSFUL";
        trapHeader.text = "UNLOCK SUCCESSFUL";

        StartCoroutine(ResetHeaderAfterDelay());
    }

    public void UpdateCurrencyHeader(int newCurrency)
    {
        snakeHeader.text = "HEAD\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
        tailHeader.text = "TAIL\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
        foodHeader.text = "FOOD\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
        trapHeader.text = "TRAP\nTOTAL POINTS: " + PlayerPrefs.GetInt("currency").ToString();
    }

    public void ShowNotEnoughPointsMessage()
    {
        snakeHeader.text = "NOT ENOUGH POINTS";
        tailHeader.text = "NOT ENOUGH POINTS";
        foodHeader.text = "NOT ENOUGH POINTS";
        trapHeader.text = "NOT ENOUGH POINTS";

        StartCoroutine(ResetHeaderAfterDelay());
    }

    IEnumerator ResetHeaderAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        UpdateCurrencyHeader(PlayerPrefs.GetInt("currency"));
    }
}
