using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    int currentSceneInt;
    string currentSceneStri;

    void Start()
    {
        currentSceneInt = SceneManager.GetActiveScene().buildIndex;
        SplashScreen();
    }

    void Update()
    {
        if (currentSceneInt == 0)
        {
            if (Input.anyKey)
            {
                LoadScene(1);
            }
        }    
    }

    void SplashScreen()
    {
        if (currentSceneInt == 0)
        {
            StartCoroutine("LoadMenu");
        }
    }

    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator LoadMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
    }
}
