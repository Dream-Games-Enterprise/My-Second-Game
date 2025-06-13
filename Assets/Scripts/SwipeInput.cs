using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SwipeInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Swipe Effect Settings")]
    public RectTransform swipeEffectPrefab;
    public float trailSmoothness = 50f;
    public float fadeOutDuration = 0.5f;
    public float lineThickness = 10f;

    RectTransform swipeEffectInstance;
    Vector2 startTouchPosition;
    Vector2 currentTargetPos;
    Coroutine fadeCoroutine;
    Coroutine trailCoroutine;
    Canvas parentCanvas;
    RectTransform canvasRectTransform;

    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint
        );

        startTouchPosition = localPoint;
        currentTargetPos = startTouchPosition;

        if (swipeEffectInstance == null)
        {
            swipeEffectInstance = Instantiate(swipeEffectPrefab, canvasRectTransform);
            swipeEffectInstance.pivot = new Vector2(0f, 0.5f);
            swipeEffectInstance.anchorMin = new Vector2(0.5f, 0.5f);
            swipeEffectInstance.anchorMax = new Vector2(0.5f, 0.5f);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        swipeEffectInstance.gameObject.SetActive(true);
        var img = swipeEffectInstance.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        }

        UpdateSwipeEffect(startTouchPosition, startTouchPosition);

        if (trailCoroutine != null)
            StopCoroutine(trailCoroutine);
        trailCoroutine = StartCoroutine(UpdateTrailSmoothly());
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint
        );

        currentTargetPos = localPoint;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint
        );

        currentTargetPos = localPoint;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        UpdateSwipeEffect(startTouchPosition, currentTargetPos);

        if (swipeEffectInstance != null)
        {
            var img = swipeEffectInstance.GetComponent<Image>();
            if (img != null)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeOutEffect(img, fadeOutDuration));
            }
        }
    }

    void UpdateSwipeEffect(Vector2 start, Vector2 end)
    {
        if (swipeEffectInstance == null) return;

        Vector2 direction = end - start;
        float distance = direction.magnitude;

         if (distance < 0.1f)
        {
            swipeEffectInstance.sizeDelta = new Vector2(0f, lineThickness);
            return;
        }

        direction.Normalize();

        swipeEffectInstance.sizeDelta = new Vector2(distance, lineThickness);
        swipeEffectInstance.rotation = Quaternion.FromToRotation(Vector3.right, direction);

        swipeEffectInstance.anchoredPosition = start;
    }

    IEnumerator UpdateTrailSmoothly()
    {
        Vector2 currentEnd = startTouchPosition;

        while (true)
        {
            currentEnd = Vector2.Lerp(currentEnd, currentTargetPos, Time.deltaTime * trailSmoothness);
            UpdateSwipeEffect(startTouchPosition, currentEnd);

            yield return null;
        }
    }

    IEnumerator FadeOutEffect(Image img, float duration)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        Color originalColor = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        swipeEffectInstance.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (trailCoroutine != null)
            StopCoroutine(trailCoroutine);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
    }
}