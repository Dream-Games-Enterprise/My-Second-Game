using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSettings : MonoBehaviour
{
    [SerializeField] TMP_Text widthValue;
    [SerializeField] TMP_Text heightValue;
    [SerializeField] Slider widthSlider;
    [SerializeField] Slider heightSlider;

    int widthInt;
    int heightInt;

    void Start()
    {
        LoadData();

        UpdateWidthText(widthSlider.value);
        UpdateHeightText(heightSlider.value);

        widthSlider.onValueChanged.AddListener(UpdateWidthText);
        heightSlider.onValueChanged.AddListener(UpdateHeightText);
    }

    void UpdateWidthText(float value)
    {
        widthValue.text = value.ToString("WIDTH | 0");
        int currentWidthChosen = (int)value;
        PlayerPrefs.SetInt("width", currentWidthChosen);
    }

    void UpdateHeightText(float value)
    {
        heightValue.text = value.ToString("HEIGHT | 0");
        int currentHeightChosen = (int)value;
        PlayerPrefs.SetInt("height", currentHeightChosen);
    }

    void LoadData()
    {
        widthInt = PlayerPrefs.GetInt("width");
        heightInt = PlayerPrefs.GetInt("height");

        widthSlider.value = widthInt;
        heightSlider.value = heightInt;
    }

    void OnDestroy()
    {
        widthSlider.onValueChanged.RemoveListener(UpdateWidthText);
        heightSlider.onValueChanged.RemoveListener(UpdateHeightText);
    }
}
