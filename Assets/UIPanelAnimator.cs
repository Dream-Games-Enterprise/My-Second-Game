using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    public float animationDuration = 0.3f;
    public Vector3 hiddenOffset = new Vector3(0, -2000, 0); // Off-screen offset
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Dictionary<Transform, Vector3> onScreenPositions = new Dictionary<Transform, Vector3>();

    public void AnimateIn(GameObject panel)
    {
        var tf = panel.transform;

        if (!onScreenPositions.ContainsKey(tf))
            onScreenPositions[tf] = tf.localPosition;

        Vector3 visiblePos = onScreenPositions[tf];
        tf.localPosition = visiblePos + hiddenOffset;
        panel.SetActive(true);

        StartCoroutine(MovePanel(tf, visiblePos));
    }

    public void AnimateOut(GameObject panel)
    {
        var tf = panel.transform;

        if (!onScreenPositions.ContainsKey(tf))
            onScreenPositions[tf] = tf.localPosition;

        Vector3 hiddenPos = onScreenPositions[tf] + hiddenOffset;
        StartCoroutine(MovePanel(tf, hiddenPos, () => panel.SetActive(false)));
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

    public void AnimateInFromTop(GameObject panel)
    {
        var tf = panel.transform;

        if (!onScreenPositions.ContainsKey(tf))
            onScreenPositions[tf] = tf.localPosition;

        Vector3 visiblePos = onScreenPositions[tf];
        Vector3 topOffset = new Vector3(0, 2000, 0); // Slide in from above
        tf.localPosition = visiblePos + topOffset;
        panel.SetActive(true);

        StartCoroutine(MovePanel(tf, visiblePos));
    }

    public void AnimateOutToTop(GameObject panel)
    {
        var tf = panel.transform;

        if (!onScreenPositions.ContainsKey(tf))
            onScreenPositions[tf] = tf.localPosition;

        Vector3 topOffset = new Vector3(0, 2000, 0); // Slide out above
        Vector3 hiddenPos = onScreenPositions[tf] + topOffset;
        StartCoroutine(MovePanel(tf, hiddenPos, () => panel.SetActive(false)));
    }
}
