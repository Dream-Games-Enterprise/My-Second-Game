using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TailPanel : MonoBehaviour
{
    public Image tailImage;
    public TMP_Text costText;
    public Button selectButton;

    public void TailSetup(TailSkin skin, bool isUnlocked)
    {
        tailImage.sprite = skin.sprite;
        costText.text = skin.isUnlocked ? "" : skin.cost.ToString();
        costText.gameObject.SetActive(!skin.isUnlocked);
        selectButton.interactable = true;
    }

    public void UpdateTailStatus(bool isUnlocked, int currentCurrency)
    {
        costText.gameObject.SetActive(!isUnlocked);
        selectButton.interactable = isUnlocked || currentCurrency >= int.Parse(costText.text);
    }
}