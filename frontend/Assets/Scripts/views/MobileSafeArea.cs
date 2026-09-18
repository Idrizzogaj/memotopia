using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Keep interactive UI clear of the camera/Dynamic Island and home indicator.
// World-space game backgrounds retain their full-screen presentation.
public class MobileSafeArea : MonoBehaviour
{
    private RectTransform content;
    private Rect lastArea;
    private Vector2 lastScreen;
    public RectTransform Content { get { return content; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace && canvas.name != "LoadingCanvas")
                {
                    var area = canvas.GetComponent<MobileSafeArea>() ?? canvas.gameObject.AddComponent<MobileSafeArea>();
                    area.Apply(Screen.safeArea, Screen.width, Screen.height);
                }
    }

    public static Rect Normalized(Rect area, float width, float height)
    {
        if (width <= 0 || height <= 0 || area.width <= 0 || area.height <= 0) return new Rect(0, 0, 1, 1);
        float x = Mathf.Clamp01(area.xMin / width), y = Mathf.Clamp01(area.yMin / height);
        return Rect.MinMaxRect(x, y, Mathf.Max(x, Mathf.Clamp01(area.xMax / width)), Mathf.Max(y, Mathf.Clamp01(area.yMax / height)));
    }

    public void Apply(Rect area, float width, float height)
    {
        if (content == null)
        {
            content = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            content.gameObject.layer = gameObject.layer;
            content.SetParent(transform, false);
        }
        var normalized = Normalized(area, width, height);
        content.anchorMin = normalized.min; content.anchorMax = normalized.max;
        content.offsetMin = content.offsetMax = Vector2.zero;
        lastArea = area; lastScreen = new Vector2(width, height);
        WrapNewChildren();
    }

    private void WrapNewChildren()
    {
        for (int i = 0; i < transform.childCount; )
        {
            var child = transform.GetChild(i);
            if (child == content || !(child is RectTransform)) { i++; continue; }
            // Standalone backgrounds have no controls and should continue beneath the camera.
            var name = child.name.ToLowerInvariant();
            if (child.childCount == 0 && (name.Contains("background") || name == "bg") && child.GetComponent<Button>() == null) { i++; continue; }
            // Expand only full-screen artwork; its controls keep their safe-area layout.
            foreach (var background in child.GetComponentsInChildren<Image>(true))
            {
                var rect = background.rectTransform;
                if (background.GetComponent<Button>() != null || rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one ||
                    rect.offsetMin.sqrMagnitude > 4 || rect.offsetMax.sqrMagnitude > 4) continue;
                var minimum = transform.InverseTransformPoint(rect.TransformPoint(rect.rect.min));
                var maximum = transform.InverseTransformPoint(rect.TransformPoint(rect.rect.max));
                var size = maximum - minimum;
                var screen = ((RectTransform)transform).rect;
                if (Mathf.Abs(size.x - screen.width) > 4 || Mathf.Abs(size.y - screen.height) > 4) continue;
                if (background.GetComponent<FullScreenArtwork>() == null) background.gameObject.AddComponent<FullScreenArtwork>();
            }
            child.SetParent(content, false);
        }
    }
    private void LateUpdate()
    {
        if (content == null || lastArea != Screen.safeArea || lastScreen != new Vector2(Screen.width, Screen.height))
            Apply(Screen.safeArea, Screen.width, Screen.height);
        else WrapNewChildren(); // Include result/achievement popups added after the scene was loaded.
    }
}
