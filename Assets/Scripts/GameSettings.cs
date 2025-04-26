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
    [SerializeField] GameObject trapSpriteSelector;

    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject widgetPanel;
    [SerializeField] GameObject leaderboardPanel;
    [SerializeField] GameObject bottomPanel;
    [SerializeField] TMP_Text inputTypeText;
    [SerializeField] Button toggleInputTypeButton;

    bool isSettingsActive = false;
    bool isWidgetActive = false;
    bool isLeaderboardActive = false;

    int widthInt;
    int heightInt;
    int speedInt;
    bool obstacles;

    enum InputType { Swipe, Buttons }
    InputType currentInputType;

    [SerializeField] UIPanelAnimator panelAnimator;


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
        trapSpriteSelector.SetActive(obstacles);

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
        widthValue.text = value.ToString("MAP WIDTH | 0");
        int currentWidthChosen = (int)value;
        PlayerPrefs.SetInt("width", currentWidthChosen);
    }

    void UpdateHeightText(float value)
    {
        heightValue.text = value.ToString("MAP HEIGHT | 0");
        int currentHeightChosen = (int)value;
        PlayerPrefs.SetInt("height", currentHeightChosen);
    }

    void UpdateSpeedText(float value)
    {
        speedValue.text = value.ToString("PLAYER SPEED | 0");
        int currentSpeedChosen = (int)value;
        PlayerPrefs.SetInt("speed", currentSpeedChosen);
        Debug.Log(currentSpeedChosen);
    }

    void OnObstaclesToggleChanged(bool isOn)
    {
        obstacles = isOn;
        PlayerPrefs.SetInt("obstacles", isOn ? 1 : 0);
        trapSpriteSelector.SetActive(isOn);
        Debug.Log("Obstacles enabled: " + obstacles);
    }


    void ToggleInputType()
    {
        currentInputType = currentInputType == InputType.Swipe ? InputType.Buttons : InputType.Swipe;

        PlayerPrefs.SetInt("inputType", (int)currentInputType);

        UpdateInputTypeText();
    }

    void UpdateInputTypeText()
    {
        inputTypeText.text = "INPUT TYPE\n" + (currentInputType == InputType.Swipe ? "SWIPING" : "BUTTONS");
    }

    public void ToggleSettings()
    {
        isSettingsActive = !isSettingsActive;
        if (isSettingsActive)
            panelAnimator.AnimateIn(settingsPanel);
        else
            panelAnimator.AnimateOut(settingsPanel);
    } 
    
    public void ToggleWidget()
    {
        isWidgetActive = !isWidgetActive;
        if (isWidgetActive)
            panelAnimator.AnimateIn(widgetPanel);
        else
            panelAnimator.AnimateOut(widgetPanel);
    }   
    
    public void ToggleLeaderboard()
    {
        isLeaderboardActive = !isLeaderboardActive;
        if (isLeaderboardActive)
            panelAnimator.AnimateIn(leaderboardPanel);
        else
            panelAnimator.AnimateOut(leaderboardPanel);
    }
}