using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SwipeInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform swipeEffectPrefab;
    private RectTransform swipeEffectInstance;
    private Vector2 startTouchPosition;
    private Coroutine fadeCoroutine;
    private Coroutine trailCoroutine;
    private Vector2 currentTargetPos;

    public void OnPointerDown(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
        currentTargetPos = startTouchPosition;

        if (swipeEffectInstance == null)
        {
            swipeEffectInstance = Instantiate(swipeEffectPrefab, transform);
            swipeEffectInstance.pivot = new Vector2(0f, 0.5f);
        }

        swipeEffectInstance.gameObject.SetActive(true);
        UpdateSwipeEffect(startTouchPosition, startTouchPosition);

        if (trailCoroutine != null) StopCoroutine(trailCoroutine);
        trailCoroutine = StartCoroutine(UpdateTrailSmoothly());
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentTargetPos = eventData.position; 
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentTargetPos = eventData.position;

        if (trailCoroutine != null) StopCoroutine(trailCoroutine);
        trailCoroutine = null;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutEffect(swipeEffectInstance.GetComponent<Image>(), 0.5f));
    }

    void UpdateSwipeEffect(Vector2 start, Vector2 smoothedEnd)
    {
        if (swipeEffectInstance == null) return;

        Vector2 direction = smoothedEnd - start;
        float distance = direction.magnitude;
        direction.Normalize();

        swipeEffectInstance.sizeDelta = new Vector2(distance, 10f);
        swipeEffectInstance.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        swipeEffectInstance.position = start;

        var img = swipeEffectInstance.GetComponent<Image>();
        if (img != null)
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
    }

    IEnumerator UpdateTrailSmoothly()
    {
        Vector2 currentEnd = startTouchPosition;
        float lerpSpeed = 50f;

        while (true)
        {
            currentEnd = Vector2.Lerp(currentEnd, currentTargetPos, Time.deltaTime * lerpSpeed);
            UpdateSwipeEffect(startTouchPosition, currentEnd);
            yield return null;
        }
    }

    IEnumerator FadeOutEffect(Image img, float duration)
    {
        float elapsed = 0f;
        Color originalColor = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        swipeEffectInstance.gameObject.SetActive(false);
    }
}
