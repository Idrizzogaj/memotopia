using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Invisible child graphics keep the existing layout and normal Unity event routing.
public sealed class TouchTarget : MonoBehaviour, ICanvasRaycastFilter
{
    public const float MinimumDp = 48f;
    private static readonly List<TouchTarget> targets = new List<TouchTarget>();
    private Selectable owner;
    private RectTransform hit;
    private RectTransform coverage;
    private readonly Vector3[] corners = new Vector3[4];
    private Canvas rootCanvas;

    public static TouchTarget Ensure(Selectable control, RectTransform wholeCard = null)
    {
        var existing = control.transform.Find("Touch area");
        var target = existing != null ? existing.GetComponent<TouchTarget>() : null;
        if (target == null)
        {
            var child = new GameObject("Touch area", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.layer = control.gameObject.layer;
            child.transform.SetParent(control.transform, false);
            var image = child.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            image.canvasRenderer.cullTransparentMesh = false;
            var layout = child.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            target = child.AddComponent<TouchTarget>();
            target.owner = control;
            target.hit = (RectTransform)child.transform;
        }
        target.coverage = wholeCard != null ? wholeCard : (RectTransform)control.transform;
        var canvas = control.GetComponentInParent<Canvas>();
        target.rootCanvas = canvas != null ? canvas.rootCanvas : null;
        target.Refresh();
        return target;
    }

    private void OnEnable() { targets.Add(this); }
    private void OnDisable() { targets.Remove(this); }
    private void LateUpdate() { Refresh(); }

    public void Refresh()
    {
        if (owner == null || coverage == null || hit == null) return;
        coverage.GetWorldCorners(corners);
        var parent = owner.transform;
        Vector3 min = parent.InverseTransformPoint(corners[0]);
        Vector3 max = min;
        for (int i = 1; i < 4; i++)
        {
            var point = parent.InverseTransformPoint(corners[i]);
            min = Vector3.Min(min, point); max = Vector3.Max(max, point);
        }
        Camera camera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null;
        Vector2 origin = RectTransformUtility.WorldToScreenPoint(camera, parent.position);
        float xScale = Vector2.Distance(origin, RectTransformUtility.WorldToScreenPoint(camera, parent.TransformPoint(Vector3.right)));
        float yScale = Vector2.Distance(origin, RectTransformUtility.WorldToScreenPoint(camera, parent.TransformPoint(Vector3.up)));
        float pixels = MinimumDp * TouchTargetInstaller.Density;
        hit.anchorMin = hit.anchorMax = ((RectTransform)parent).pivot;
        hit.pivot = new Vector2(.5f, .5f);
        hit.anchoredPosition = (min + max) * .5f;
        hit.sizeDelta = new Vector2(Mathf.Max(max.x - min.x, pixels / Mathf.Max(.01f, xScale)),
                                   Mathf.Max(max.y - min.y, pixels / Mathf.Max(.01f, yScale)));
    }

    public bool IsRaycastLocationValid(Vector2 point, Camera camera)
    {
        if (owner == null || !owner.IsActive() || !owner.IsInteractable()) return false;
        // Existing visible control areas always retain their original ownership.
        if (RectTransformUtility.RectangleContainsScreenPoint(coverage, point, camera)) return true;
        float distance = DistanceToCoverage(point, camera);
        foreach (var other in targets)
        {
            if (other == this || other.owner == null || other.rootCanvas != rootCanvas ||
                !other.owner.IsActive() || !other.AllowsRaycasts()) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(other.coverage, point, camera)) return false;
            if (!other.owner.IsInteractable()) continue;
            if (!RectTransformUtility.RectangleContainsScreenPoint(other.hit, point, camera)) continue;
            float otherDistance = other.DistanceToCoverage(point, camera);
            if (otherDistance < distance || (Mathf.Approximately(otherDistance, distance) && other.GetInstanceID() < GetInstanceID())) return false;
        }
        return true;
    }

    private bool AllowsRaycasts()
    {
        for (Transform current = owner.transform; current != null; current = current.parent)
        {
            bool stop = false;
            foreach (var group in current.GetComponents<CanvasGroup>())
            {
                if (!group.blocksRaycasts) return false;
                stop |= group.ignoreParentGroups;
            }
            if (stop) break;
        }
        return true;
    }

    private float DistanceToCoverage(Vector2 point, Camera camera)
    {
        coverage.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 max = min;
        for (int i = 1; i < 4; i++)
        {
            var next = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
            min = Vector2.Min(min, next); max = Vector2.Max(max, next);
        }
        return (point - new Vector2(Mathf.Clamp(point.x, min.x, max.x), Mathf.Clamp(point.y, min.y, max.y))).sqrMagnitude;
    }
}

public sealed class TouchTargetInstaller : MonoBehaviour
{
    public static float Density { get; private set; } = 1f;
    private float nextScan;
    private Selectable[] controls = new Selectable[128];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        Density = Screen.dpi > 0 ? Mathf.Clamp(Screen.dpi / 160f, 1f, 4f) : Mathf.Max(1f, Screen.width / 360f);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var resources = activity.Call<AndroidJavaObject>("getResources"))
            using (var metrics = resources.Call<AndroidJavaObject>("getDisplayMetrics"))
                Density = metrics.Get<float>("density");
        }
        catch (System.Exception) { /* Use the screen-density fallback. */ }
#endif
        var root = new GameObject("Touch targets");
        DontDestroyOnLoad(root);
        root.AddComponent<TouchTargetInstaller>();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextScan) return;
        nextScan = Time.unscaledTime + .2f;
        if (controls.Length < Selectable.allSelectableCount) controls = new Selectable[Selectable.allSelectableCount + 32];
        int count = Selectable.AllSelectablesNoAlloc(controls);
        for (int i = 0; i < count; i++)
        {
            var control = controls[i];
            if (!(control is Button) && !(control is Toggle)) continue;
            if (control.transform.Find("Touch area") == null) TouchTarget.Ensure(control);
        }
    }
}
