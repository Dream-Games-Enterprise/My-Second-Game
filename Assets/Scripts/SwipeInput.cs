using System.Collections;
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public LineRenderer swipeLine;
    public float fadeDuration = 0.5f;

    Vector2 touchStartPos;
    Coroutine fadeCoroutine;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool isTouching = false;
        Vector2 currentPos = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            currentPos = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = currentPos;
                    StartSwipeEffect(touchStartPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    UpdateSwipeEffect(currentPos);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndSwipeEffect();
                    break;
            }

            isTouching = true;
        }

        if (!isTouching)
        {
            currentPos = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = currentPos;
                StartSwipeEffect(touchStartPos);
            }
            else if (Input.GetMouseButton(0))
            {
                UpdateSwipeEffect(currentPos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndSwipeEffect();
            }
        }
    }

    void StartSwipeEffect(Vector2 startPos)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        swipeLine.gameObject.SetActive(true);
        swipeLine.positionCount = 2;
        Vector3 worldPos = ScreenToWorld(startPos);
        swipeLine.SetPosition(0, worldPos);
        swipeLine.SetPosition(1, worldPos);
        SetLineAlpha(1f);
    }

    void UpdateSwipeEffect(Vector2 endPos)
    {
        swipeLine.SetPosition(1, ScreenToWorld(endPos));
    }

    void EndSwipeEffect()
    {
        fadeCoroutine = StartCoroutine(FadeSwipeEffect());
    }

    IEnumerator FadeSwipeEffect()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetLineAlpha(alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        swipeLine.gameObject.SetActive(false);
    }

    void SetLineAlpha(float alpha)
    {
        Color start = swipeLine.startColor;
        Color end = swipeLine.endColor;
        start.a = end.a = alpha;
        swipeLine.startColor = start;
        swipeLine.endColor = end;
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 screenToWorld = new Vector3(screenPos.x, screenPos.y, 10f); // z = distance from camera
        return Camera.main.ScreenToWorldPoint(screenToWorld);
    }
}
