using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Toggle))]
public class ToggleClickSoundHandler : MonoBehaviour
{
    bool initialised = false;

    void Awake()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);
        StartCoroutine(DelayInit());
    }

    IEnumerator DelayInit()
    {
        yield return null;
        initialised = true;
    }

    void OnToggleChanged(bool _)
    {
        if (!initialised) return;

        AudioManager.Instance?.PlayButtonClick();
    }
}
