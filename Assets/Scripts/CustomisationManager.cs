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

public class CustomisationManager : MonoBehaviour
{
    public List<SnakeSkin> snakeSkins;
    public Transform skinPanelContainer;  
    public GameObject skinPanelPrefab;   

    List<SkinPanel> skinPanels = new List<SkinPanel>();
    int selectedSnakeIndex = 0;

    void Start()
    {
        InitializePanels();
        //UpdateSelectedSkin(selectedSnakeIndex);
    }

    void InitializePanels()
    {
        foreach (var skin in snakeSkins)
        {
            GameObject panelObj = Instantiate(skinPanelPrefab, skinPanelContainer);
            SkinPanel panel = panelObj.GetComponent<SkinPanel>();
            panel.Setup(skin, skin.isUnlocked);

            int index = skinPanels.Count;
            panel.selectButton.onClick.AddListener(() => SelectSkin(index));

            skinPanels.Add(panel);
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

    void UpdateSelectedSkin(int index)
    {
        // Save the selected skin index to PlayerPrefs
        PlayerPrefs.SetInt("SelectedSnakeIndex", index);
        PlayerPrefs.Save();
        Debug.Log("Skin Selected: " + snakeSkins[index].sprite);  // Debug to check the sprite
    }

    public int GetSelectedSnakeIndex()
    {
        // Retrieve the selected snake index from PlayerPrefs
        return PlayerPrefs.GetInt("SelectedSnakeIndex", 0);  // Default to 0 if nothing is saved
    }
}
