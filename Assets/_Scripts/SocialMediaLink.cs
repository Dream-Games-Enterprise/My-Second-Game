using UnityEngine;

public class SocialMediaLink : MonoBehaviour
{
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
