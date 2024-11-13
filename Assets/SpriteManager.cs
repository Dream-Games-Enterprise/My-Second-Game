using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteManager : MonoBehaviour
{
    [SerializeField] GameObject snakePanel;
    [SerializeField] GameObject tailPanel;
    [SerializeField] GameObject foodPanel;

    void Start()
    {
        DisablePanels();    
    }

    public void DisablePanels()
    {
        snakePanel.SetActive(false);
        tailPanel.SetActive(false);
        foodPanel.SetActive(false);
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        panelToOpen.SetActive(true);
    }
}
