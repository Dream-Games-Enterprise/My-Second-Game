using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleClickSoundHandler : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool _)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
}
