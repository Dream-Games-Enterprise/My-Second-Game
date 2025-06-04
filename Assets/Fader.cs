using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    SceneLoader sceneLoader;

    void Awake()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    void Start()
    {
        canvasGroup.alpha = 1;

        if (sceneLoader.currentSceneInt == 1)
        {
            StartCoroutine(FadeOut(1f));
        }
        else
        {
            StartCoroutine(FadeOut(0.75f));
        }
    }

    public IEnumerator FadeIn(float timeToFade)
    {
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += (1f / timeToFade) * Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public IEnumerator FadeOut(float timeToFade)
    {
        canvasGroup.alpha = 1;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= (1f / timeToFade) * Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }

    public void StartFadeIn(float timeToFade)
    {
        StartCoroutine(FadeIn(timeToFade));
    }

    public void StartFadeOut(float timeToFade)
    {
        StartCoroutine(FadeOut(timeToFade));
    }
}
