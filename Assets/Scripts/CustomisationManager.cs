using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class SnakeSkin
{
    public Sprite sprite;
    public int cost;
    public bool isUnlocked;
}

[System.Serializable]
public class TailSkin
{
    public Sprite sprite;
    public int cost;
    public bool isUnlocked;
}

[System.Serializable]
public class FoodSkin
{
    public Sprite sprite;
    public int cost;
    public bool isUnlocked;
}

[System.Serializable]
public class TrapSkin
{
    public Sprite sprite;
    public int cost;
    public bool isUnlocked;
}

public class CustomisationManager : MonoBehaviour
{
    // Ensure isInCustomisationScene is set appropriately in the Inspector.
    [SerializeField] Camera targetCamera;
    [SerializeField] SpriteManager spriteManager;

    public List<SnakeSkin> snakeSkins;
    public List<TailSkin> tailSkins;
    public List<FoodSkin> foodSkins;
    public List<TrapSkin> trapSkins;

    public Transform skinPanelContainer;
    public Transform tailPanelContainer;
    public Transform foodPanelContainer;
    public Transform trapPanelContainer;
    public GameObject skinPanelPrefab;
    public GameObject tailPanelPrefab;
    public GameObject foodPanelPrefab;
    public GameObject trapPanelPrefab;

    [SerializeField] Image previewSnake;
    [SerializeField] Image previewTail;
    [SerializeField] Image previewFood;
    [SerializeField] Image previewTrap;
    [SerializeField] Image previewPrimary;
    [SerializeField] Image previewSecondary;

    List<SkinPanel> skinPanels = new List<SkinPanel>();
    List<TailPanel> tailPanels = new List<TailPanel>();
    List<FoodPanel> foodPanels = new List<FoodPanel>();
    List<TrapPanel> trapPanels = new List<TrapPanel>();
    int selectedSnakeIndex = 0;
    int selectedTailIndex = 0;
    int selectedFoodIndex = 0;
    int selectedTrapIndex = 0;

    Color snakeColour;
    Color snakeTailColour;
    Color foodColour;
    Color trapColour;
    Color mapPrimaryColor;
    Color mapSecondaryColor;
    [SerializeField] public List<Color> snakeColours;
    [SerializeField] public List<Color> mapColours;

    int currency;
    [SerializeField] bool isInCustomisationScene;

    [SerializeField] List<Color> backgroundColors;
    [SerializeField] List<string> backgroundNames;
    [SerializeField] Button backgroundCycleButton;
    [SerializeField] TMP_Text backgroundText;
    int backgroundIndex = 0;
    [SerializeField] List<Image> backgroundTintImages;
    [SerializeField] List<Color> uiPanelColours;
    [SerializeField] Image inputPanelBackground;
    [SerializeField] Image pausePanelBackground;

    [SerializeField] private TMP_Text borderText;   
    [SerializeField] private Color obstacleColor;

    private int borderOptionIndex = 0;
    private const string BorderPrefKey = "SelectedBorderOption";

    void Start()
    {
        currency = PlayerPrefs.GetInt("currency");
        Debug.Log(currency);
        spriteManager = Object.FindFirstObjectByType<SpriteManager>();

        if (isInCustomisationScene)
        {
            InitializePanels();

            UpdateSelectedSkin(GetSelectedSnakeIndex());
            UpdateSelectedTail(GetSelectedTailIndex());
            UpdateSelectedFood(GetSelectedFoodIndex());
            UpdateSelectedTrap(GetSelectedTrapIndex());

            int snakeColorIndex = PlayerPrefs.GetInt("SelectedColourIndex", 0);
            int tailColorIndex = PlayerPrefs.GetInt("SelectedTailColourIndex", 0);
            int foodColorIndex = PlayerPrefs.GetInt("SelectedFoodColourIndex", 0);
            int trapColorIndex = PlayerPrefs.GetInt("SelectedTrapColourIndex", 0);
            int mapPrimaryColorIndex = PlayerPrefs.GetInt("SelectedMapPrimaryColorIndex", 2);
            if (mapPrimaryColorIndex < 0 || mapPrimaryColorIndex >= mapColours.Count)
                mapPrimaryColorIndex = 2;
            int mapSecondaryColorIndex = PlayerPrefs.GetInt("SelectedMapSecondaryColorIndex", 1);

            backgroundIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);
            if (backgroundIndex >= backgroundColors.Count)
                backgroundIndex = 0;

            if (targetCamera != null)
                targetCamera.backgroundColor = backgroundColors[backgroundIndex];
            else
                Debug.LogWarning("Camera reference is missing or index out of range.");

            foreach (var img in backgroundTintImages)
                if (img != null)
                    img.color = backgroundColors[backgroundIndex];

            backgroundText.text = backgroundNames[backgroundIndex] + "\nBACKGROUND";

            if (inputPanelBackground != null &&
                backgroundIndex >= 0 &&
                backgroundIndex < uiPanelColours.Count)
                inputPanelBackground.color = uiPanelColours[backgroundIndex];

            SelectMapPrimaryColor(mapPrimaryColorIndex);
            SelectMapSecondaryColor(mapSecondaryColorIndex);
            SelectColour(snakeColorIndex);
            SelectTailColour(tailColorIndex);
            SelectFoodColour(foodColorIndex);
            SelectTrapColour(trapColorIndex);

            // ——— load border choice ———
            borderOptionIndex = PlayerPrefs.GetInt(BorderPrefKey, 0);
            ApplyBorderOption(borderOptionIndex);
        }
        else
        {
            Debug.Log("== APPLYING UI PANEL COLOURS ==");

            int savedIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);
            if (savedIndex < 0 || savedIndex >= uiPanelColours.Count)
            {
                Debug.LogWarning($"Saved background index {savedIndex} is out of range; forcing to 0.");
                savedIndex = 0;
            }

            if (inputPanelBackground != null)
                inputPanelBackground.color = uiPanelColours[savedIndex];
            else
                Debug.LogWarning("InputPanelBackground reference is missing.");

            if (pausePanelBackground != null)
                pausePanelBackground.color = uiPanelColours[savedIndex];
            else
                Debug.LogWarning("PausePanelBackground reference is missing.");

            // ——— load border choice ———
            borderOptionIndex = PlayerPrefs.GetInt(BorderPrefKey, 0);
            ApplyBorderOption(borderOptionIndex);
        }
    }

    public void CycleBackgroundColor()
    {
        backgroundIndex = (backgroundIndex + 1) % backgroundColors.Count;
        Color selectedColor = backgroundColors[backgroundIndex];

        backgroundText.text = backgroundNames[backgroundIndex] + "\nBACKGROUND";

        if (targetCamera != null)
            targetCamera.backgroundColor = selectedColor;

        foreach (var img in backgroundTintImages)
            if (img != null)
                img.color = selectedColor;

        if (inputPanelBackground != null &&
            backgroundIndex >= 0 &&
            backgroundIndex < uiPanelColours.Count)
            inputPanelBackground.color = uiPanelColours[backgroundIndex];

        PlayerPrefs.SetInt("SelectedBackgroundIndex", backgroundIndex);
        PlayerPrefs.Save();
    }

    public void CycleBorderOption()
    {
        borderOptionIndex = (borderOptionIndex + 1) % 3;
        ApplyBorderOption(borderOptionIndex);

        PlayerPrefs.SetInt(BorderPrefKey, borderOptionIndex);
        PlayerPrefs.Save();
    }

    void ApplyBorderOption(int idx)
    {
        switch (idx)
        {
            case 0:
                borderText.text = "Border: White";
                break;
            case 1:
                borderText.text = "Border: Match Background";
                break;
            case 2:
                borderText.text = "Border: Match Obstacles";
                break;
        }
    }

    public void SelectMapPrimaryColor(int index)
    {
        if (index >= 0 && index < mapColours.Count)
        {
            mapPrimaryColor = mapColours[index];
            PlayerPrefs.SetInt("SelectedMapPrimaryColorIndex", index);
            PlayerPrefs.Save();
            previewPrimary.color = mapPrimaryColor;
        }
    }

    public void SelectMapSecondaryColor(int index)
    {
        if (index >= 0 && index < mapColours.Count)
        {
            mapSecondaryColor = mapColours[index];
            PlayerPrefs.SetInt("SelectedMapSecondaryColorIndex", index);
            PlayerPrefs.Save();
            previewSecondary.color = mapSecondaryColor;
        }
    }

    void InitializePanels()
    {
        for (int i = 0; i < snakeSkins.Count; i++)
        {
            snakeSkins[i].isUnlocked = PlayerPrefs.GetInt("SnakeSkinUnlocked_" + i, 0) == 1;
            var obj = Instantiate(skinPanelPrefab, skinPanelContainer);
            var panel = obj.GetComponent<SkinPanel>();
            panel.SkinSetup(snakeSkins[i], snakeSkins[i].isUnlocked);
            int idx = skinPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockSkin(idx));
            skinPanels.Add(panel);
        }

        for (int i = 0; i < tailSkins.Count; i++)
        {
            tailSkins[i].isUnlocked = PlayerPrefs.GetInt("TailSkinUnlocked_" + i, 0) == 1;
            var obj = Instantiate(tailPanelPrefab, tailPanelContainer);
            var panel = obj.GetComponent<TailPanel>();
            panel.TailSetup(tailSkins[i], tailSkins[i].isUnlocked);
            int idx = tailPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockTail(idx));
            tailPanels.Add(panel);
        }

        for (int i = 0; i < foodSkins.Count; i++)
        {
            foodSkins[i].isUnlocked = PlayerPrefs.GetInt("FoodSkinUnlocked_" + i, 0) == 1;
            var obj = Instantiate(foodPanelPrefab, foodPanelContainer);
            var panel = obj.GetComponent<FoodPanel>();
            panel.FoodSetup(foodSkins[i], foodSkins[i].isUnlocked);
            int idx = foodPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockFood(idx));
            foodPanels.Add(panel);
        }

        for (int i = 0; i < trapSkins.Count; i++)
        {
            trapSkins[i].isUnlocked = PlayerPrefs.GetInt("TrapSkinUnlocked_" + i, 0) == 1;
            var obj = Instantiate(trapPanelPrefab, trapPanelContainer);
            var panel = obj.GetComponent<TrapPanel>();
            panel.TrapSetup(trapSkins[i], trapSkins[i].isUnlocked);
            int idx = trapPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockTrap(idx));
            trapPanels.Add(panel);
        }
    }


    public void TryUnlockSkin(int index)
    {
        if (snakeSkins[index].isUnlocked)
        {
            selectedSnakeIndex = index;
            SelectSkin(index);
        }
        else if (currency >= snakeSkins[index].cost)
        {
            currency -= snakeSkins[index].cost;
            PlayerPrefs.SetInt("currency", currency);
            PlayerPrefs.SetInt("SnakeSkinUnlocked_" + index, 1);
            spriteManager.ShowUnlockSuccessfulMessage();
            snakeSkins[index].isUnlocked = true;
            skinPanels[index].UpdateSkinStatus(true, currency);
            UpdateSelectedSkin(index);
        }
        else
        {
            spriteManager.ShowNotEnoughPointsMessage();
        }
    }

    public void TryUnlockTail(int index)
    {
        if (tailSkins[index].isUnlocked)
        {
            selectedTailIndex = index;
            SelectTail(index);
        }
        else if (currency >= tailSkins[index].cost)
        {
            currency -= tailSkins[index].cost;
            PlayerPrefs.SetInt("currency", currency);
            PlayerPrefs.SetInt("TailSkinUnlocked_" + index, 1);
            spriteManager.ShowUnlockSuccessfulMessage();
            tailSkins[index].isUnlocked = true;
            tailPanels[index].UpdateTailStatus(true, currency);
            UpdateSelectedTail(index);
        }
        else
        {
            spriteManager.ShowNotEnoughPointsMessage();
        }
    }

    public void TryUnlockFood(int index)
    {
        if (foodSkins[index].isUnlocked)
        {
            selectedFoodIndex = index;
            SelectFood(index);
        }
        else if (currency >= foodSkins[index].cost)
        {
            currency -= foodSkins[index].cost;
            PlayerPrefs.SetInt("currency", currency);
            PlayerPrefs.SetInt("FoodSkinUnlocked_" + index, 1);
            spriteManager.ShowUnlockSuccessfulMessage();
            foodSkins[index].isUnlocked = true;
            foodPanels[index].UpdateFoodStatus(true, currency);
            UpdateSelectedFood(index);
        }
        else
        {
            spriteManager.ShowNotEnoughPointsMessage();
        }
    }

    public void TryUnlockTrap(int index)
    {
        if (trapSkins[index].isUnlocked)
        {
            selectedTrapIndex = index;
            SelectTrap(index);
        }
        else if (currency >= trapSkins[index].cost)
        {
            currency -= trapSkins[index].cost;
            PlayerPrefs.SetInt("currency", currency);
            PlayerPrefs.SetInt("TrapSkinUnlocked_" + index, 1);
            spriteManager.ShowUnlockSuccessfulMessage();
            trapSkins[index].isUnlocked = true;
            trapPanels[index].UpdateTrapStatus(true, currency);
            UpdateSelectedTrap(index);
        }
        else
        {
            spriteManager.ShowNotEnoughPointsMessage();
        }
    }

    public void SelectSkin(int index)
    {
        if (snakeSkins[index].isUnlocked)
        {
            selectedSnakeIndex = index;
            UpdateSelectedSkin(index);
        }
    }

    public void SelectTail(int index)
    {
        if (tailSkins[index].isUnlocked)
        {
            selectedTailIndex = index;
            UpdateSelectedTail(index);
        }
    }

    public void SelectFood(int index)
    {
        if (foodSkins[index].isUnlocked)
        {
            selectedFoodIndex = index;
            UpdateSelectedFood(index);
        }
    }

    public void SelectTrap(int index)
    {
        if (trapSkins[index].isUnlocked)
        {
            selectedTrapIndex = index;
            UpdateSelectedTrap(index);
        }
    }

    void UpdateSelectedSkin(int index)
    {
        PlayerPrefs.SetInt("SelectedSnakeIndex", index);
        PlayerPrefs.Save();
        previewSnake.sprite = snakeSkins[index].sprite;
    }

    void UpdateSelectedTail(int index)
    {
        PlayerPrefs.SetInt("SelectedTailIndex", index);
        PlayerPrefs.Save();
        previewTail.sprite = tailSkins[index].sprite;
    }

    void UpdateSelectedFood(int index)
    {
        PlayerPrefs.SetInt("SelectedFoodIndex", index);
        PlayerPrefs.Save();
        previewFood.sprite = foodSkins[index].sprite;
    }

    void UpdateSelectedTrap(int index)
    {
        PlayerPrefs.SetInt("SelectedTrapIndex", index);
        PlayerPrefs.Save();
        previewTrap.sprite = trapSkins[index].sprite;
    }

    public int GetSelectedSnakeIndex() => PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
    public int GetSelectedTailIndex() => PlayerPrefs.GetInt("SelectedTailIndex", 0);
    public int GetSelectedFoodIndex() => PlayerPrefs.GetInt("SelectedFoodIndex", 0);
    public int GetSelectedTrapIndex() => PlayerPrefs.GetInt("SelectedTrapIndex", 0);

    public void SelectColour(int index)
    {
        if (index >= 0 && index < snakeColours.Count)
        {
            snakeColour = snakeColours[index];
            PlayerPrefs.SetInt("SelectedColourIndex", index);
            PlayerPrefs.Save();
            previewSnake.color = snakeColour;
        }
    }

    public void SelectTailColour(int index)
    {
        if (index >= 0 && index < snakeColours.Count)
        {
            snakeTailColour = snakeColours[index];
            PlayerPrefs.SetInt("SelectedTailColourIndex", index);
            PlayerPrefs.Save();
            previewTail.color = snakeTailColour;
        }
    }

    public void SelectFoodColour(int index)
    {
        if (index >= 0 && index < snakeColours.Count)
        {
            foodColour = snakeColours[index];
            PlayerPrefs.SetInt("SelectedFoodColourIndex", index);
            PlayerPrefs.Save();
            previewFood.color = foodColour;
        }
    }

    public void SelectTrapColour(int index)
    {
        if (index >= 0 && index < snakeColours.Count)
        {
            trapColour = snakeColours[index];
            PlayerPrefs.SetInt("SelectedTrapColourIndex", index);
            PlayerPrefs.Save();
            previewTrap.color = trapColour;
        }
    }
}
