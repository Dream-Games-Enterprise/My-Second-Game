using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSettings : MonoBehaviour
{
    [SerializeField] TMP_Text widthValue;
    [SerializeField] TMP_Text heightValue;
    [SerializeField] TMP_Text speedValue;
    [SerializeField] Slider widthSlider;
    [SerializeField] Slider heightSlider;
    [SerializeField] Slider speedSlider;
    [SerializeField] Toggle obstaclesToggle;

    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject bottomPanel;
    [SerializeField] TMP_Text inputTypeText;
    [SerializeField] Button toggleInputTypeButton;

    private bool isSettingsActive = false;

    int widthInt;
    int heightInt;
    int speedInt;
    bool obstacles;

    enum InputType { Swipe, Buttons, Joystick }  // Add Joystick input type
    InputType currentInputType;

    void Start()
    {
        Time.timeScale = 1f;
        LoadData();

        UpdateWidthText(widthSlider.value);
        UpdateHeightText(heightSlider.value);
        UpdateSpeedText(speedSlider.value);

        widthSlider.onValueChanged.AddListener(UpdateWidthText);
        heightSlider.onValueChanged.AddListener(UpdateHeightText);
        speedSlider.onValueChanged.AddListener(UpdateSpeedText);

        obstaclesToggle.onValueChanged.AddListener(OnObstaclesToggleChanged);
        obstaclesToggle.isOn = obstacles;

        toggleInputTypeButton.onClick.AddListener(ToggleInputType);
        UpdateInputTypeText();
    }

    void LoadData()
    {
        widthInt = PlayerPrefs.GetInt("width", 10);
        heightInt = PlayerPrefs.GetInt("height", 10);
        speedInt = PlayerPrefs.GetInt("speed", 5);
        obstacles = PlayerPrefs.GetInt("obstacles", 1) == 1;

        currentInputType = (InputType)PlayerPrefs.GetInt("inputType", (int)InputType.Swipe);

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
        toggleInputTypeButton.onClick.RemoveListener(ToggleInputType);
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

    void ToggleInputType()
    {
        if (currentInputType == InputType.Swipe)
            currentInputType = InputType.Buttons;
        else if (currentInputType == InputType.Buttons)
            currentInputType = InputType.Joystick;
        else
            currentInputType = InputType.Swipe;

        PlayerPrefs.SetInt("inputType", (int)currentInputType);
        UpdateInputTypeText();
    }

    void UpdateInputTypeText()
    {
        switch (currentInputType)
        {
            case InputType.Swipe:
                inputTypeText.text = "INPUT TYPE\nSWIPING";
                break;
            case InputType.Buttons:
                inputTypeText.text = "INPUT TYPE\nBUTTONS";
                break;
            case InputType.Joystick:
                inputTypeText.text = "INPUT TYPE\nJOYSTICK";
                break;
        }
    }
    // New ToggleSettings function with Lerp for position
    public void ToggleSettings()
    {
        Debug.Log("BEING CLICKED");

        settingsPanel.SetActive(true);

        isSettingsActive = !isSettingsActive;

    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}
