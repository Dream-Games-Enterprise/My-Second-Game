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

    // New variables for Lerp effect
    [SerializeField] float lerpDuration = 0.5f; // Duration of the animation
    Vector3 targetPosition;
    Vector3 initialPosition;
    private bool isSettingsActive = false;

    int widthInt;
    int heightInt;
    int speedInt;
    bool obstacles;

    enum InputType { Swipe, Buttons }
    InputType currentInputType;

    void Awake()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Initially inactive

            // Set the initial position off-screen below the screen (adjust based on screen height)
            initialPosition = settingsPanel.transform.position;

            // Set the initial position dynamically off-screen (below the screen)
            initialPosition.y = -Screen.height; // Move it off-screen vertically based on screen height

            settingsPanel.transform.position = initialPosition; // Apply this initial position
        }
    }

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
        currentInputType = currentInputType == InputType.Swipe ? InputType.Buttons : InputType.Swipe;

        PlayerPrefs.SetInt("inputType", (int)currentInputType);

        UpdateInputTypeText();
    }

    void UpdateInputTypeText()
    {
        inputTypeText.text = "INPUT TYPE\n" + (currentInputType == InputType.Swipe ? "SWIPING" : "BUTTONS");
    }

    // New ToggleSettings function with Lerp for position
    public void ToggleSettings()
    {
        Debug.Log("BEING CLICKED");

        settingsPanel.SetActive(true);

        isSettingsActive = !isSettingsActive;

        // Set the target position (screen center for example)
        targetPosition = isSettingsActive
            ? new Vector3(0f, 0f, 0f) // Move it to the center of the screen (both x and y = 0)
            : new Vector3(0f, -Screen.height, 0f); // Move it off-screen vertically

        // Start the Lerp animation
        StartCoroutine(LerpPosition());
    }

    // Coroutine to Lerp the position of the settings panel
    private IEnumerator LerpPosition()
    {
        float timeElapsed = 0f;

        // While the time elapsed is less than the lerp duration, continue animating the position
        while (timeElapsed < lerpDuration)
        {
            settingsPanel.transform.position = Vector3.Lerp(initialPosition, targetPosition, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the final target position
        settingsPanel.transform.position = targetPosition;
    }
}
