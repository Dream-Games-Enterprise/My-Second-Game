using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCustomisationSettings : MonoBehaviour
{
    public Sprite customPlayerSprite;
    public Sprite customTailSprite;

    void Start()
    {
        int snakeIndex = PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
        //int tailIndex = PlayerPrefs.GetInt("SelectedTailIndex", 0);

        CustomisationManager customisation = FindObjectOfType<CustomisationManager>(); // Or load it if it’s in a different scene

        if (customisation != null)
        {
            customPlayerSprite = customisation.snakeSprites[snakeIndex];
            //customTailSprite = customisation.tailSprites[tailIndex];
        }

        // Use these sprites in PlacePlayer() and CreateTailNode() methods as described in the previous response
    }
}
