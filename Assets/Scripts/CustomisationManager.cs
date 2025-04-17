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
    [SerializeField] public List<Color> snakeColours;

    int currency;

    void Start()
    {
        currency = PlayerPrefs.GetInt("currency");
        Debug.Log(currency);
        spriteManager = FindObjectOfType<SpriteManager>();
        InitializePanels();

        UpdateSelectedSkin(GetSelectedSnakeIndex());
        UpdateSelectedTail(GetSelectedTailIndex());
        UpdateSelectedFood(GetSelectedFoodIndex());
        UpdateSelectedTrap(GetSelectedTrapIndex());
    }

    void InitializePanels()
    {
        foreach (var skin in snakeSkins)
        {
            GameObject panelObj = Instantiate(skinPanelPrefab, skinPanelContainer);
            SkinPanel panel = panelObj.GetComponent<SkinPanel>();
            panel.SkinSetup(skin, skin.isUnlocked);

            int index = skinPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockSkin(index));

            skinPanels.Add(panel);
        }

        foreach (var tail in tailSkins)
        {
            GameObject panelObj = Instantiate(tailPanelPrefab, tailPanelContainer); 
            TailPanel panel = panelObj.GetComponent<TailPanel>();
            panel.TailSetup(tail, tail.isUnlocked);

            int index = tailPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockTail(index));

            tailPanels.Add(panel);
        }

        foreach (var food in foodSkins)
        {
            GameObject panelObj = Instantiate(foodPanelPrefab, foodPanelContainer);
            FoodPanel panel = panelObj.GetComponent<FoodPanel>();
            panel.FoodSetup(food, food.isUnlocked);

            int index = foodPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockFood(index));

            foodPanels.Add(panel);
        }
        
        foreach (var trap in trapSkins)
        {
            GameObject panelObj = Instantiate(trapPanelPrefab, trapPanelContainer);
            TrapPanel panel = panelObj.GetComponent<TrapPanel>();
            panel.TrapSetup(trap, trap.isUnlocked);

            int index = trapPanels.Count;
            panel.selectButton.onClick.AddListener(() => TryUnlockTrap(index));

            trapPanels.Add(panel);
        }
    }

    public void TryUnlockSkin(int index)
    {
        Debug.Log("STAGE 1");
        if (snakeSkins[index].isUnlocked)
        {
            Debug.Log("STAGE 2");
            selectedSnakeIndex = index;
            SelectSkin(index);
        }
        else if (currency >= snakeSkins[index].cost)
        {
            Debug.Log("STAGE 3");
            currency -= snakeSkins[index].cost;
            PlayerPrefs.SetInt("currency", currency);
            spriteManager.ShowUnlockSuccessfulMessage();
            snakeSkins[index].isUnlocked = true;
            skinPanels[index].UpdateSkinStatus(true, currency);
            UpdateSelectedSkin(index);
        }
        else
        {
            Debug.Log("Not enough currency to unlock this skin.");
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
            spriteManager.ShowUnlockSuccessfulMessage();
            tailSkins[index].isUnlocked = true;
            tailPanels[index].UpdateTailStatus(true, currency);
            UpdateSelectedTail(index);
        }
        else
        {
            Debug.Log("Not enough currency to unlock this tail.");
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
            spriteManager.ShowUnlockSuccessfulMessage();
            foodSkins[index].isUnlocked = true;
            foodPanels[index].UpdateFoodStatus(true, currency);
            UpdateSelectedFood(index);
        }
        else
        {
            Debug.Log("Not enough currency to unlock this food.");
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
            spriteManager.ShowUnlockSuccessfulMessage();
            trapSkins[index].isUnlocked = true;
            trapPanels[index].UpdateTrapStatus(true, currency);
            UpdateSelectedTrap(index);
        }
        else
        {
            Debug.Log("Not enough currency to unlock this trap.");
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

        Debug.Log("Skin Selected: " + snakeSkins[index].sprite);
    }

    void UpdateSelectedTail(int index)
    {
        PlayerPrefs.SetInt("SelectedTailIndex", index);
        PlayerPrefs.Save();

        previewTail.sprite = tailSkins[index].sprite;

        Debug.Log("Tail Skin Selected: " + tailSkins[index].sprite);
    }

    void UpdateSelectedFood(int index)
    {
        PlayerPrefs.SetInt("SelectedFoodIndex", index);
        PlayerPrefs.Save();

        previewFood.sprite = foodSkins[index].sprite;

        Debug.Log("Food Skin Selected: " + foodSkins[index].sprite);
    }

    void UpdateSelectedTrap(int index)
    {
        PlayerPrefs.SetInt("SelectedTrapIndex", index);
        PlayerPrefs.Save();

        previewTrap.sprite = trapSkins[index].sprite;

        Debug.Log("Trap Skin Selected: " + trapSkins[index].sprite);
    }

    public int GetSelectedSnakeIndex()
    {
        return PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
    }

    public int GetSelectedTailIndex()
    {
        return PlayerPrefs.GetInt("SelectedTailIndex", 0);
    }

    public int GetSelectedFoodIndex()
    {
        return PlayerPrefs.GetInt("SelectedFoodIndex", 0);
    }

    public int GetSelectedTrapIndex()
    {
        return PlayerPrefs.GetInt("SelectedTrapIndex", 0);
    }

    public void SelectColour(int index)
    {
        if (index >= 0 && index < snakeColours.Count)
        {
            snakeColour = snakeColours[index];
            PlayerPrefs.SetInt("SelectedColourIndex", index);
            PlayerPrefs.Save();

            previewSnake.color = snakeColour;

            Debug.Log("Colour Selected: " + snakeColour);
        }
        else
        {
            Debug.LogWarning("Invalid colour index selected: " + index);
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

            Debug.Log("Colour Selected: " + snakeTailColour);
        }
        else
        {
            Debug.LogWarning("Invalid colour index selected: " + index);
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

            Debug.Log("Colour Selected: " + foodColour);
        }
        else
        {
            Debug.LogWarning("Invalid colour index selected: " + index);
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

            Debug.Log("Colour Selected: " + trapColour);
        }
        else
        {
            Debug.LogWarning("Invalid colour index selected: " + index);
        }
    }
}
