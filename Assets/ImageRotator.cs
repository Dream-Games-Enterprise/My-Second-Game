using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageRotator : MonoBehaviour
{
    [SerializeField] Image[] arrows;

    public float rotationSpeed = 20f;

    void FixedUpdate()
    {
        foreach (var arrow in arrows)
        {
            arrow.transform.Rotate(0, 0, rotationSpeed * Time.fixedDeltaTime);
        }
    }

}
