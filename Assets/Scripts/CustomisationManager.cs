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

public class CustomisationManager : MonoBehaviour
{
    public List<SnakeSkin> snakeSkins;
    public List<TailSkin> tailSkins;  
    public Transform skinPanelContainer;
    public Transform tailPanelContainer;
    public GameObject skinPanelPrefab;
    public GameObject tailPanelPrefab;  

    List<SkinPanel> skinPanels = new List<SkinPanel>();
    List<TailPanel> tailPanels = new List<TailPanel>();
    int selectedSnakeIndex = 0;
    int selectedTailIndex = 0;

    void Start()
    {
        InitializePanels();
    }

    void InitializePanels()
    {
        foreach (var skin in snakeSkins)
        {
            GameObject panelObj = Instantiate(skinPanelPrefab, skinPanelContainer);
            SkinPanel panel = panelObj.GetComponent<SkinPanel>();
            panel.SkinSetup(skin, skin.isUnlocked);

            int index = skinPanels.Count;
            panel.selectButton.onClick.AddListener(() => SelectSkin(index));

            skinPanels.Add(panel);
        }

        foreach (var tail in tailSkins)
        {
            GameObject panelObj = Instantiate(tailPanelPrefab, tailPanelContainer); 
            TailPanel panel = panelObj.GetComponent<TailPanel>();
            panel.TailSetup(tail, tail.isUnlocked);

            int index = tailPanels.Count;
            panel.selectButton.onClick.AddListener(() => SelectTail(index));

            tailPanels.Add(panel);
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

    void UpdateSelectedSkin(int index)
    {
        PlayerPrefs.SetInt("SelectedSnakeIndex", index);
        Debug.Log("sprite has changed...");
        PlayerPrefs.Save();
        Debug.Log("Skin Selected: " + snakeSkins[index].sprite); 
    }

    void UpdateSelectedTail(int index)
    {
        PlayerPrefs.SetInt("SelectedTailIndex", index);
        PlayerPrefs.Save();
        Debug.Log("Tail Skin Selected: " + tailSkins[index].sprite);
    }

    public int GetSelectedSnakeIndex()
    {
        return PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
    }

    public int GetSelectedTailIndex()
    {
        return PlayerPrefs.GetInt("SelectedTailIndex", 0);
    }
}
