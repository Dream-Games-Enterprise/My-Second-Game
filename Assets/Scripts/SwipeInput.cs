using UnityEngine.EventSystems;
using UnityEngine;

public class SwipeInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform swipeEffectPrefab; // The swipe effect to spawn
    private Vector2 startTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Optional: live preview while dragging
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 endTouchPosition = eventData.position;
        Vector2 swipeDirection = (endTouchPosition - startTouchPosition).normalized;

        ShowSwipeEffect(startTouchPosition, endTouchPosition);
        // Trigger your movement logic here
    }

    void ShowSwipeEffect(Vector2 start, Vector2 end)
    {
        var swipe = Instantiate(swipeEffectPrefab, transform);
        swipe.position = start;
        var direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        swipe.sizeDelta = new Vector2(distance, 10f); // Adjust height as needed
        swipe.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        Destroy(swipe.gameObject, 0.5f); // Optional: auto-destroy after fade
    }
}
