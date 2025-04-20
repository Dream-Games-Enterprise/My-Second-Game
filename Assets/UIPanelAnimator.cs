using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    public float animationDuration = 0.3f;
    public float buffer = 200f; // Extra space to ensure panel is fully off-screen
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Dictionary<Transform, Vector3> onScreenPositions = new Dictionary<Transform, Vector3>();

    public void AnimateIn(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, -panelHeight, 0);

        rt.localPosition = visiblePos + offset;
        panel.SetActive(true);

        StartCoroutine(MovePanel(rt, visiblePos));
    }

    public void AnimateOut(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, -panelHeight, 0);

        Vector3 hiddenPos = visiblePos + offset;
        StartCoroutine(MovePanel(rt, hiddenPos, () => panel.SetActive(false)));
    }

    public void AnimateInFromTop(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, panelHeight, 0);

        rt.localPosition = visiblePos + offset;
        panel.SetActive(true);

        StartCoroutine(MovePanel(rt, visiblePos));
    }

    public void AnimateOutToTop(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        if (!onScreenPositions.ContainsKey(rt))
            onScreenPositions[rt] = rt.localPosition;

        Vector3 visiblePos = onScreenPositions[rt];
        float panelHeight = GetDynamicOffset(rt);
        Vector3 offset = new Vector3(0, panelHeight, 0);

        Vector3 hiddenPos = visiblePos + offset;
        StartCoroutine(MovePanel(rt, hiddenPos, () => panel.SetActive(false)));
    }

    private IEnumerator MovePanel(Transform panel, Vector3 targetPos, System.Action onComplete = null)
    {
        Vector3 startPos = panel.localPosition;
        float time = 0;

        while (time < animationDuration)
        {
            float t = easeCurve.Evaluate(time / animationDuration);
            panel.localPosition = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }

        panel.localPosition = targetPos;
        onComplete?.Invoke();
    }

    private float GetDynamicOffset(RectTransform rt)
    {
        float height = rt.rect.height * rt.lossyScale.y;
        return height + buffer; // Add extra buffer to ensure full off-screen move
    }
}
