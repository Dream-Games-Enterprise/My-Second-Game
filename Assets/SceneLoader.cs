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
    }

    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
