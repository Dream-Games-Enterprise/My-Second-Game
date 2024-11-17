using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinPanel : MonoBehaviour
{
    public Image skinImage;
    public TMP_Text costText;
    public Button selectButton;

    public void SkinSetup(SnakeSkin skin, bool isUnlocked)
    {
        skinImage.sprite = skin.sprite;
        costText.text = skin.isUnlocked ? "" : skin.cost.ToString();
        costText.gameObject.SetActive(!skin.isUnlocked);
        selectButton.interactable = skin.isUnlocked;
    }
}
