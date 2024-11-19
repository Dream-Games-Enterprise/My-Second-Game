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

public class CustomisationManager : MonoBehaviour
{
    public List<SnakeSkin> snakeSkins;
    public List<TailSkin> tailSkins;
    public List<FoodSkin> foodSkins;
    public Transform skinPanelContainer;
    public Transform tailPanelContainer;
    public Transform foodPanelContainer;
    public GameObject skinPanelPrefab;
    public GameObject tailPanelPrefab;
    public GameObject foodPanelPrefab;

    [SerializeField] Image previewSnake;
    [SerializeField] Image previewTail;
    [SerializeField] Image previewFood;

    List<SkinPanel> skinPanels = new List<SkinPanel>();
    List<TailPanel> tailPanels = new List<TailPanel>();
    List<FoodPanel> foodPanels = new List<FoodPanel>();
    int selectedSnakeIndex = 0;
    int selectedTailIndex = 0;
    int selectedFoodIndex = 0;

    int playerCurrency;

    void Start()
    {
        InitializePanels();

        UpdateSelectedSkin(GetSelectedSnakeIndex());
        UpdateSelectedTail(GetSelectedTailIndex());
        UpdateSelectedFood(GetSelectedFoodIndex());
    }

    void InitializePanels()
    {
        foreach (var skin in snakeSkins)
        {
            GameObject panelObj = Instantiate(skinPanelPrefab, skinPanelContainer);
            SkinPanel panel = panelObj.GetComponent<SkinPanel>();
            panel.SkinSetup(skin, skin.isUnlocked);

            int index = skinPanels.Count;
            panel.selectButton.onClick.AddListener(() => AttemptToUnlockSkin(index, "snake"));

            skinPanels.Add(panel);
        }

        foreach (var tail in tailSkins)
        {
            GameObject panelObj = Instantiate(tailPanelPrefab, tailPanelContainer); 
            TailPanel panel = panelObj.GetComponent<TailPanel>();
            panel.TailSetup(tail, tail.isUnlocked);

            int index = tailPanels.Count;
            panel.selectButton.onClick.AddListener(() => AttemptToUnlockSkin(index, "tail"));

            tailPanels.Add(panel);
        }

        foreach (var food in foodSkins)
        {
            GameObject panelObj = Instantiate(foodPanelPrefab, foodPanelContainer);
            FoodPanel panel = panelObj.GetComponent<FoodPanel>();
            panel.FoodSetup(food, food.isUnlocked);

            int index = foodPanels.Count;
            panel.selectButton.onClick.AddListener(() => AttemptToUnlockSkin(index, "food"));

            foodPanels.Add(panel);
        }
    }

    void AttemptToUnlockSkin(int index, string category)
    {
        if (category == "snake")
        {
            var skin = snakeSkins[index];
            if (!skin.isUnlocked && playerCurrency >= skin.cost)
            {
                DeductCurrency(skin.cost);
                skin.isUnlocked = true;
                SelectSkin(index);
            }
            else if (skin.isUnlocked)
            {
                SelectSkin(index);
            }
            else
            {
                Debug.Log("Not enough currency to unlock this skin.");
            }
        }
        else if (category == "tail")
        {
            var tail = tailSkins[index];
            if (!tail.isUnlocked && playerCurrency >= tail.cost)
            {
                DeductCurrency(tail.cost);
                tail.isUnlocked = true;
                SelectTail(index);
            }
            else if (tail.isUnlocked)
            {
                SelectTail(index);
            }
            else
            {
                Debug.Log("Not enough currency to unlock this tail.");
            }
        }
        else if (category == "food")
        {
            var food = foodSkins[index];
            if (!food.isUnlocked && playerCurrency >= food.cost)
            {
                DeductCurrency(food.cost);
                food.isUnlocked = true;
                SelectFood(index);
            }
            else if (food.isUnlocked)
            {
                SelectFood(index);
            }
            else
            {
                Debug.Log("Not enough currency to unlock this food.");
            }
        }
    }


    void DeductCurrency(int amount)
    {
        playerCurrency -= amount;
        PlayerPrefs.SetInt("currency", playerCurrency);
        PlayerPrefs.Save();
        //UpdateCurrencyDisplay();
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
}
