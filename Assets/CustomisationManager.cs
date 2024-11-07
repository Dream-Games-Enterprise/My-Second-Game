using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomisationManager : MonoBehaviour
{
    public List<Sprite> snakeSprites; // Drag your snake sprites here in the Inspector
    public List<Sprite> tailSprites;  // Drag your tail sprites here in the Inspector

    public Image snakePreview; // Image to show the selected snake sprite
    public Image tailPreview;  // Image to show the selected tail sprite

    private int selectedSnakeIndex = 0;
    private int selectedTailIndex = 0;

    void Start()
    {
        // Load saved selections from PlayerPrefs (if available)
        selectedSnakeIndex = PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
        selectedTailIndex = PlayerPrefs.GetInt("SelectedTailIndex", 0);

        // Update the preview images based on saved selections
        //UpdatePreview();
    }

    // Call this function when changing snake sprite
    public void ChangeSnakeSprite(int index)
    {
        selectedSnakeIndex = index;
        ApplySettings();
        //UpdatePreview();
    }

    // Call this function when changing tail sprite
    public void ChangeTailSprite(int index)
    {
        selectedTailIndex = index;
        ApplySettings();
        //UpdatePreview();
    }

    // Apply the selected sprites and save to PlayerPrefs
    public void ApplySettings()
    {
        PlayerPrefs.SetInt("SelectedSnakeIndex", selectedSnakeIndex);
        PlayerPrefs.SetInt("SelectedTailIndex", selectedTailIndex);
        PlayerPrefs.Save();
    }

    // Update preview images
    private void UpdatePreview()
    {
        //snakePreview.sprite = snakeSprites[selectedSnakeIndex];
        //tailPreview.sprite = tailSprites[selectedTailIndex];
    }

    // Get the selected snake sprite
    public Sprite GetSelectedSnakeSprite()
    {
        return snakeSprites[selectedSnakeIndex];
    }

    // Get the selected tail sprite
    public Sprite GetSelectedTailSprite()
    {
        return tailSprites[selectedTailIndex];
    }
}
