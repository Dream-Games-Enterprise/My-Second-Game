using UnityEngine;

public class Pauser : MonoBehaviour
{
    bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log("Paused: " + isPaused);
    }
}
