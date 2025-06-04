using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int currentSceneInt;
    public string currentSceneStr;

    Fader fader;
    [SerializeField] float fadeDuration = 0.5f;

    void Awake()
    {
        fader = FindObjectOfType<Fader>();
    }

    void Start()
    {
        UpdateCurrentSceneInfo();

        if (currentSceneInt == 0)
            StartCoroutine(WaitThenLoad(3f, 1));
    }

    void Update()
    {
        if (currentSceneInt == 0 && Input.anyKeyDown)
            LoadSceneWithFade(1);
    }

    void UpdateCurrentSceneInfo()
    {
        currentSceneInt = SceneManager.GetActiveScene().buildIndex;
        currentSceneStr = SceneManager.GetActiveScene().name;
    }

    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadSceneWithFade(int sceneToLoad)
    {
        UpdateCurrentSceneInfo();

        if (fader != null)
            StartCoroutine(DoFadeThenLoad(sceneToLoad));
        else
            SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator DoFadeThenLoad(int sceneToLoad)
    {
        yield return StartCoroutine(fader.FadeIn(fadeDuration));
        SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator WaitThenLoad(float delay, int sceneIndex)
    {
        yield return new WaitForSeconds(delay);
        LoadSceneWithFade(sceneIndex);
    }
}
