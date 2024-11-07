using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameSettings : MonoBehaviour
{
    [SerializeField] TMP_Text widthValue;
    [SerializeField] TMP_Text heightValue;
    [SerializeField] TMP_Text speedValue;
    [SerializeField] Slider widthSlider;
    [SerializeField] Slider heightSlider;
    [SerializeField] Slider speedSlider;
    [SerializeField] Toggle obstaclesToggle;

    int widthInt;
    int heightInt;
    int speedInt;
    bool obstacles;

    void Start()
    {
        LoadData();

        UpdateWidthText(widthSlider.value);
        UpdateHeightText(heightSlider.value);
        UpdateSpeedText(speedSlider.value);

        widthSlider.onValueChanged.AddListener(UpdateWidthText);
        heightSlider.onValueChanged.AddListener(UpdateHeightText);
        speedSlider.onValueChanged.AddListener(UpdateSpeedText);

        obstaclesToggle.onValueChanged.AddListener(OnObstaclesToggleChanged);
        obstaclesToggle.isOn = obstacles;
    }

    void LoadData()
    {
        widthInt = PlayerPrefs.GetInt("width");
        heightInt = PlayerPrefs.GetInt("height");
        speedInt = PlayerPrefs.GetInt("speed");
        obstacles = PlayerPrefs.GetInt("obstacles", 1) == 1; 

        widthSlider.value = widthInt;
        heightSlider.value = heightInt;
        speedSlider.value = speedInt;
    }

    void OnDestroy()
    {
        widthSlider.onValueChanged.RemoveListener(UpdateWidthText);
        heightSlider.onValueChanged.RemoveListener(UpdateHeightText);
        speedSlider.onValueChanged.RemoveListener(UpdateSpeedText);
        obstaclesToggle.onValueChanged.RemoveListener(OnObstaclesToggleChanged);
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

    void UpdateSpeedText(float value)
    {
        speedValue.text = value.ToString("SPEED | 0");
        int currentSpeedChosen = (int)value;
        PlayerPrefs.SetInt("speed", currentSpeedChosen);
        Debug.Log(currentSpeedChosen);
    }

    void OnObstaclesToggleChanged(bool isOn)
    {
        obstacles = isOn;
        PlayerPrefs.SetInt("obstacles", isOn ? 1 : 0);
        Debug.Log("Obstacles enabled: " + obstacles);
    }
}
