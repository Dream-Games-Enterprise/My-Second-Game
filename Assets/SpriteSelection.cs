using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSelection : MonoBehaviour
{
    public List<Sprite> playerHeadSprites;  // List of player head sprites
    public List<Sprite> tailSprites;        // List of tail sprites
    public List<Sprite> foodSprites;        // List of food sprites

    private int selectedPlayerHeadIndex = 0;
    private int selectedTailIndex = 0;
    private int selectedFoodIndex = 0;

    void Start()
    {
        // Load saved selections when the scene starts
        LoadSpriteSelections();
    }

    public void OnPlayerHeadSelected(int index)
    {
        selectedPlayerHeadIndex = index;
        SaveSpriteSelections();
    }

    public void OnTailSelected(int index)
    {
        selectedTailIndex = index;
        SaveSpriteSelections();
    }

    public void OnFoodSelected(int index)
    {
        selectedFoodIndex = index;
        SaveSpriteSelections();
    }

    // Save the selected sprites to PlayerPrefs
    private void SaveSpriteSelections()
    {
        PlayerPrefs.SetInt("PlayerHeadIndex", selectedPlayerHeadIndex);
        PlayerPrefs.SetInt("TailIndex", selectedTailIndex);
        PlayerPrefs.SetInt("FoodIndex", selectedFoodIndex);
        PlayerPrefs.Save();
    }

    // Load saved sprite selections from PlayerPrefs
    private void LoadSpriteSelections()
    {
        selectedPlayerHeadIndex = PlayerPrefs.GetInt("PlayerHeadIndex", 0);  // Default to 0 if not set
        selectedTailIndex = PlayerPrefs.GetInt("TailIndex", 0);
        selectedFoodIndex = PlayerPrefs.GetInt("FoodIndex", 0);
    }
}

