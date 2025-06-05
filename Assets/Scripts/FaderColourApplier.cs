using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FaderColourApplier : MonoBehaviour
{
    [SerializeField] private Image faderImage;           
    [SerializeField] private List<Color> backgroundColors;

    void Start()
    {
        int bgIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);
        if (bgIndex < 0 || bgIndex >= backgroundColors.Count)
            bgIndex = 0;

        if (faderImage != null)
            faderImage.color = backgroundColors[bgIndex];
        else
            Debug.LogWarning("FaderColourApplier: no Image assigned to faderImage.");
    }
}
