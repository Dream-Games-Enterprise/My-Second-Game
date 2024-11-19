using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodPanel : MonoBehaviour
{
    public Image foodImage;
    public TMP_Text costText;
    public Button selectButton;

    public void FoodSetup(FoodSkin skin, bool isUnlocked)
    {
        foodImage.sprite = skin.sprite;
        costText.text = skin.isUnlocked ? "" : skin.cost.ToString();
        costText.gameObject.SetActive(!skin.isUnlocked);
        selectButton.interactable = true;
    }

    public void UpdateFoodStatus(bool isUnlocked, int currentCurrency)
    {
        costText.gameObject.SetActive(!isUnlocked);
        selectButton.interactable = isUnlocked || currentCurrency >= int.Parse(costText.text);
    }
}