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
            StartToggle(snakeHeader, "SNAKE HEAD");
        }
        else if (panelToOpen == tailPanel)
        {
            StartToggle(tailHeader, "SNAKE TAIL");
        }
        else if (panelToOpen == foodPanel)
        {
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
            header.text = originalText;
            yield return new WaitForSeconds(1f);

            header.text = pointDisplay;
            yield return new WaitForSeconds(1f);
        }
    }
}
