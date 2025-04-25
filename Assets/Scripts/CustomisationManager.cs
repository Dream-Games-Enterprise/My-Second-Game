using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField] bool isInCustomizationScene;

    void Start()
    {
        currency = PlayerPrefs.GetInt("currency");
        Debug.Log(currency);
        spriteManager = Object.FindFirstObjectByType<SpriteManager>();

        if (isInCustomizationScene)
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
            int mapPrimaryColorIndex = PlayerPrefs.GetInt("SelectedMapPrimaryColorIndex", 0);
            int mapSecondaryColorIndex = PlayerPrefs.GetInt("SelectedMapSecondaryColorIndex", 1);

            SelectMapPrimaryColor(mapPrimaryColorIndex);
            SelectMapSecondaryColor(mapSecondaryColorIndex);
            SelectColour(snakeColorIndex);
            SelectTailColour(tailColorIndex);
            SelectFoodColour(foodColorIndex);
            SelectTrapColour(trapColorIndex);
        }
    }

    public void SelectMapPrimaryColor(int index)
    {
        if (index >= 0 && index < mapColours.Count)
        {
            mapPrimaryColor = mapColours[index];
            PlayerPrefs.SetInt("SelectedMapPrimaryColorIndex", index);
            PlayerPrefs.Save();

            // Apply to preview
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

            // Apply to preview
            previewSecondary.color = mapSecondaryColor;
        }
    }

    void InitializePanels()
    {
        for (int i = 0; i < snakeSkins.Count; i++)
        {
            snakeSkins[i].isUnlocked = PlayerPrefs.GetInt("SnakeSkinUnlocked_" + i, 0) == 1;
            GameObject panelObj = Instantiate(skinPanelPrefab, skinPanelContainer);
            SkinPanel panel = panelObj.GetComponent<SkinPanel>();
            panel.SkinSetup(snakeSkins[i], snakeSkins[i].isUnlocked);
            int index = skinPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockSkin(index));
            skinPanels.Add(panel);
        }

        for (int i = 0; i < tailSkins.Count; i++)
        {
            tailSkins[i].isUnlocked = PlayerPrefs.GetInt("TailSkinUnlocked_" + i, 0) == 1;
            GameObject panelObj = Instantiate(tailPanelPrefab, tailPanelContainer);
            TailPanel panel = panelObj.GetComponent<TailPanel>();
            panel.TailSetup(tailSkins[i], tailSkins[i].isUnlocked);
            int index = tailPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockTail(index));
            tailPanels.Add(panel);
        }

        for (int i = 0; i < foodSkins.Count; i++)
        {
            foodSkins[i].isUnlocked = PlayerPrefs.GetInt("FoodSkinUnlocked_" + i, 0) == 1;
            GameObject panelObj = Instantiate(foodPanelPrefab, foodPanelContainer);
            FoodPanel panel = panelObj.GetComponent<FoodPanel>();
            panel.FoodSetup(foodSkins[i], foodSkins[i].isUnlocked);
            int index = foodPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockFood(index));
            foodPanels.Add(panel);
        }

        for (int i = 0; i < trapSkins.Count; i++)
        {
            trapSkins[i].isUnlocked = PlayerPrefs.GetInt("TrapSkinUnlocked_" + i, 0) == 1;
            GameObject panelObj = Instantiate(trapPanelPrefab, trapPanelContainer);
            TrapPanel panel = panelObj.GetComponent<TrapPanel>();
            panel.TrapSetup(trapSkins[i], trapSkins[i].isUnlocked);
            int index = trapPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockTrap(index));
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
