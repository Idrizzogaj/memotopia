using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unmistakable feedback for a wrong answer: phone vibration, a quick shake and a red flash of the tapped picture.
/// Focus games use Vibrate() alone so the play is not disturbed.
/// </summary>
public class WrongFeedback : MonoBehaviour
{
    private const float Duration = .38f;

    public static void Vibrate()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (!Application.isEditor) Handheld.Vibrate();
#endif
    }

    public static void Mistake(GameObject target, bool vibrate = true)
    {
        if (vibrate) Vibrate();
        if (target == null || !target.activeInHierarchy) return;
        var runner = target.GetComponent<WrongFeedback>() ?? target.AddComponent<WrongFeedback>();
        runner.StopAllCoroutines();
        runner.StartCoroutine(runner.Shake());
    }

    private IEnumerator Shake()
    {
        var rect = transform as RectTransform;
        var image = GetComponent<Image>();
        Vector2 home = rect != null ? rect.anchoredPosition : Vector2.zero;
        Color tint = image != null ? image.color : Color.white;
        float width = rect != null ? Mathf.Max(40f, rect.rect.width) : 100f;
        for (float t = 0; t < Duration; t += Time.unscaledDeltaTime)
        {
            float k = 1f - t / Duration;
            if (rect != null) rect.anchoredPosition = home + new Vector2(Mathf.Sin(t * 80f) * width * .07f * k, 0);
            if (image != null) image.color = Color.Lerp(tint, new Color(1f, .3f, .3f, tint.a), k);
            yield return null;
        }
        if (rect != null) rect.anchoredPosition = home;
        if (image != null) image.color = tint;
    }

    private void OnDisable()
    {
        // Never leave a picture tinted or displaced if it is hidden mid-shake.
        StopAllCoroutines();
    }
}
