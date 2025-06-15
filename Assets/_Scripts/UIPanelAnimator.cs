using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    public float animationDuration = 0.3f;
    public float buffer = 200f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Dictionary<Transform, Vector3> onScreenPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Coroutine> activeAnimations = new Dictionary<Transform, Coroutine>();

    public void AnimateIn(GameObject panel, System.Action onStart = null)
    {
        var rt = panel.GetComponent<RectTransform>();

        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        CancelExistingAnimation(rt);

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, -panelHeight, 0);

        rt.localPosition = visiblePos + offset;
        panel.SetActive(true);

        onStart?.Invoke(); // <-- NEW: trigger optional logic before animation
        activeAnimations[rt] = StartCoroutine(MovePanel(rt, visiblePos));
    }

    public void AnimateOut(GameObject panel, System.Action onComplete = null)
    {
        var rt = panel.GetComponent<RectTransform>();

        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        CancelExistingAnimation(rt);

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, -panelHeight, 0);

        Vector3 hiddenPos = visiblePos + offset;
        activeAnimations[rt] = StartCoroutine(MovePanel(rt, hiddenPos, () =>
        {
            panel.SetActive(false);
            onComplete?.Invoke(); // <-- NEW: trigger optional logic after animation
        }));
    }

    public void AnimateInFromTop(GameObject panel, System.Action onStart = null)
    {
        var rt = panel.GetComponent<RectTransform>();

        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        CancelExistingAnimation(rt);

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, panelHeight, 0);

        rt.localPosition = visiblePos + offset;
        panel.SetActive(true);

        onStart?.Invoke();
        activeAnimations[rt] = StartCoroutine(MovePanel(rt, visiblePos));
    }

    public void AnimateOutToTop(GameObject panel, System.Action onComplete = null)
    {
        var rt = panel.GetComponent<RectTransform>();

        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        CancelExistingAnimation(rt);

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, panelHeight, 0);

        Vector3 hiddenPos = visiblePos + offset;
        activeAnimations[rt] = StartCoroutine(MovePanel(rt, hiddenPos, () =>
        {
            panel.SetActive(false);
            onComplete?.Invoke();
        }));
    }

    private IEnumerator MovePanel(Transform panel, Vector3 targetPos, System.Action onComplete = null)
    {
        Vector3 startPos = panel.localPosition;
        float time = 0f;

        while (time < animationDuration)
        {
            float t = easeCurve.Evaluate(time / animationDuration);
            panel.localPosition = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }

        panel.localPosition = targetPos;
        onComplete?.Invoke();
        activeAnimations.Remove(panel);
    }

    private float GetDynamicOffset(RectTransform rt)
    {
        float height = rt.rect.height * rt.lossyScale.y;
        return height + buffer;
    }

    private void CancelExistingAnimation(Transform tf)
    {
        if (activeAnimations.TryGetValue(tf, out Coroutine running))
        {
            StopCoroutine(running);
            activeAnimations.Remove(tf);
        }
    }
}
