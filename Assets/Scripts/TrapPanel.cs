using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrapPanel : MonoBehaviour
{
    public Image trapImage;
    public TMP_Text costText;
    public Button selectButton;

    public void TrapSetup(TrapSkin skin, bool isUnlocked)
    {
        trapImage.sprite = skin.sprite;
        costText.text = skin.isUnlocked ? "" : skin.cost.ToString();
        costText.gameObject.SetActive(!skin.isUnlocked);
        selectButton.interactable = true;
    }

    public void UpdateTrapStatus(bool isUnlocked, int currentCurrency)
    {
        costText.gameObject.SetActive(!isUnlocked);
        selectButton.interactable = isUnlocked || currentCurrency >= int.Parse(costText.text);
    }
}