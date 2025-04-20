using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    public float animationDuration = 0.3f;
    public Vector3 hiddenOffset = new Vector3(0, -2000, 0);
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Cache original positions
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

    public void AnimateIn(GameObject panel)
    {
        var tf = panel.transform;

        if (!originalPositions.ContainsKey(tf))
            originalPositions[tf] = tf.localPosition;

        Vector3 originalPos = originalPositions[tf];
        tf.localPosition = originalPos + hiddenOffset;
        panel.SetActive(true);

        StartCoroutine(MovePanel(tf, originalPos));
    }

    public void AnimateOut(GameObject panel)
    {
        var tf = panel.transform;

        if (!originalPositions.ContainsKey(tf))
            originalPositions[tf] = tf.localPosition;

        Vector3 targetPos = originalPositions[tf] + hiddenOffset;
        StartCoroutine(MovePanel(tf, targetPos, () => panel.SetActive(false)));
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
}


//ORIGINAL POS SHOULD BE OFF SCREEN THEN ANIMATE IN ONTO THE SCREEN AND VICE VERSA