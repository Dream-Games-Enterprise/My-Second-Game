using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SplashScreenColourApplier : MonoBehaviour
{
    [SerializeField] Camera splashCamera;
    [SerializeField] List<Color> backgroundColors;
    [SerializeField] TMP_Text backgroundText;
    [SerializeField] List<string> backgroundNames;
    [SerializeField] List<Image> backgroundTintImages;

    void Start()
    {
        int bgIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);
        if (bgIndex < 0 || bgIndex >= backgroundColors.Count)
            bgIndex = 0;

        if (splashCamera != null)
            splashCamera.backgroundColor = backgroundColors[bgIndex];

        for (int i = 0; i < backgroundTintImages.Count; i++)
            if (backgroundTintImages[i] != null)
                backgroundTintImages[i].color = backgroundColors[bgIndex];

        if (backgroundText != null && backgroundNames != null && bgIndex < backgroundNames.Count)
            backgroundText.text = backgroundNames[bgIndex] + "\nBACKGROUND";
    }
}
