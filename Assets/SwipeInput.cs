using System.Collections;
using UnityEngine;
using RD;

public class SwipeInput : MonoBehaviour
{
    public LineRenderer swipeLine; // Assign in Unity Inspector
    public float fadeDuration = 0.5f; // Time for fade-out

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private Coroutine fadeCoroutine;
    GameManager gameManager; // Reference to GameManager

    void Start()
    {
        // Get reference to GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
        }
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        Debug.Log("swipe input is handling touch input");

        if (!gameManager.isButtonControl)
        {
            // Check if touch is detected at all
            Debug.Log("Touch count: " + Input.touchCount);

            // Touch Input (Mobile)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Debug.Log("Touch detected with phase: " + touch.phase);

                if (touch.phase == TouchPhase.Began)
                {
                    Debug.Log("Touch Began - Calling StartSwipeEffect()");
                    touchStartPos = touch.position;
                    StartSwipeEffect(touchStartPos);
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Debug.Log("Touch Moved - Calling UpdateSwipeEffect()");
                    touchEndPos = touch.position;
                    gameManager.DetectSwipe(touchStartPos, touchEndPos);
                    UpdateSwipeEffect(touchEndPos);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Debug.Log("Touch Ended - Calling EndSwipeEffect()");
                    touchStartPos = Vector2.zero;
                    EndSwipeEffect();
                }
            }

            // Mouse Input (PC Testing)
            if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
            {
                Debug.Log("Mouse Button Down - Calling StartSwipeEffect()");
                touchStartPos = Input.mousePosition;
                StartSwipeEffect(touchStartPos);
            }
            else if (Input.GetMouseButton(0)) // Mouse is being dragged
            {
                Debug.Log("Mouse Button Held - Calling UpdateSwipeEffect()");
                touchEndPos = Input.mousePosition;
                gameManager.DetectSwipe(touchStartPos, touchEndPos);
                UpdateSwipeEffect(touchEndPos);
            }
            else if (Input.GetMouseButtonUp(0)) // Left mouse button released
            {
                Debug.Log("Mouse Button Up - Calling EndSwipeEffect()");
                touchStartPos = Vector2.zero;
                EndSwipeEffect();
            }
        }
    }


    void StartSwipeEffect(Vector2 startPos)
    {
        Debug.Log("Should be swiping broski");
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        swipeLine.gameObject.SetActive(true);
        swipeLine.positionCount = 2;
        swipeLine.SetPosition(0, Camera.main.ScreenToWorldPoint(new Vector3(startPos.x, startPos.y, 10)));
        swipeLine.SetPosition(1, swipeLine.GetPosition(0));
        swipeLine.startColor = swipeLine.endColor = new Color(1, 1, 1, 1); // White, full alpha
    }

    void UpdateSwipeEffect(Vector2 endPos)
    {
        Debug.Log("Should be updating line");
        swipeLine.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(endPos.x, endPos.y, 10)));
    }

    void EndSwipeEffect()
    {
        fadeCoroutine = StartCoroutine(FadeSwipeEffect());
    }

    IEnumerator FadeSwipeEffect()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            swipeLine.startColor = swipeLine.endColor = new Color(1, 1, 1, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        swipeLine.gameObject.SetActive(false);
    }
}
