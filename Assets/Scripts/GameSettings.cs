using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GameSettings : MonoBehaviour
{
    [SerializeField] LeaderboardCreatorDemo.LeaderboardManager leaderboardManager;
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
    [SerializeField] Button soundToggleButton;
    [SerializeField] TMP_Text soundButtonLabel;
    bool soundEnabled;

    bool isSettingsActive = false;
    bool isWidgetActive = false;
    bool isLeaderboardActive = false;

    int widthInt;
    int heightInt;
    int speedInt;
    bool obstacles;

    enum InputType { Swipe = 0, TwoButtons = 1, FourButtons = 2 }
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

        soundEnabled = PlayerPrefs.GetInt("sound", 1) == 1;
        AudioManager.Instance.SetSoundEnabled(soundEnabled);
        UpdateSoundButtonUI();

        soundToggleButton.onClick.AddListener(OnSoundButtonClicked);

        toggleInputTypeButton.onClick.AddListener(ToggleInputType);
        UpdateInputTypeText();
    }

    void OnSoundButtonClicked()
    {
        soundEnabled = !soundEnabled;
        PlayerPrefs.SetInt("sound", soundEnabled ? 1 : 0);
        AudioManager.Instance.SetSoundEnabled(soundEnabled);
        UpdateSoundButtonUI();
    }

    void UpdateSoundButtonUI()
    {
        if (soundButtonLabel != null)
            soundButtonLabel.text = "SFX " + (soundEnabled ? "ON" : "OFF");
    }

    void LoadData()
    {
        widthInt = PlayerPrefs.GetInt("width", 10);
        heightInt = PlayerPrefs.GetInt("height", 10);
        speedInt = PlayerPrefs.GetInt("speed", 5);
        obstacles = PlayerPrefs.GetInt("obstacles", 1) == 1;

        int raw = PlayerPrefs.GetInt("inputType", (int)InputType.Swipe);
        currentInputType = (InputType)raw;
        Debug.Log($"[GameSettings] Loaded inputType = {raw} ({currentInputType})");

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
        currentInputType = (InputType)(((int)currentInputType + 1)
                             % System.Enum.GetValues(typeof(InputType)).Length);
        PlayerPrefs.SetInt("inputType", (int)currentInputType);
        PlayerPrefs.Save();
        Debug.Log($"[GameSettings] Saved inputType = {(int)currentInputType} ({currentInputType})");
        UpdateInputTypeText();
    }


    void UpdateInputTypeText()
    {
        switch (currentInputType)
        {
            case InputType.Swipe:
                inputTypeText.text = "INPUT TYPE\nSWIPING";
                break;
            case InputType.TwoButtons:
                inputTypeText.text = "INPUT TYPE\n2 BUTTONS";
                break;
            case InputType.FourButtons:
                inputTypeText.text = "INPUT TYPE\n4 BUTTONS";
                break;
        }
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
        {
            panelAnimator.AnimateIn(leaderboardPanel);
            leaderboardManager.LoadPersonalBest();
            leaderboardManager.LoadScores();
        }
        else
            panelAnimator.AnimateOut(leaderboardPanel);
    }
}