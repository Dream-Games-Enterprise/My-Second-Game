using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundColourApplier : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField] List<Color> backgroundColors;
    [SerializeField] List<Image> backgroundTintImages;

    void Start()
    {
        int index = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);
        if (index >= backgroundColors.Count)
            index = 0;

        if (targetCamera != null && index < backgroundColors.Count)
        {
            targetCamera.backgroundColor = backgroundColors[index];

            foreach (var img in backgroundTintImages)
            {
                if (img != null)
                    img.color = backgroundColors[index];
            }
        }
        else
        {
            Debug.LogWarning("Camera reference is missing or index is out of range.");
        }
    }
}
