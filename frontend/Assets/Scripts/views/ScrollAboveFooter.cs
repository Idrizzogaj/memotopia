using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps scrollable text above a fixed footer, including after rotation.</summary>
public sealed class ScrollAboveFooter : MonoBehaviour
{
    private RectTransform viewport;
    private RectTransform footer;
    private float originalBottom;
    private readonly Vector3[] corners = new Vector3[4];

    public void Configure(ScrollRect scroll, RectTransform fixedFooter)
    {
        footer = fixedFooter;
        viewport = scroll.viewport;
        if (viewport == null || viewport == scroll.transform)
        {
            // Legacy tip pages scroll directly on the page, without a separate viewport.
            viewport = new GameObject("TextViewport", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
            viewport.SetParent(scroll.transform, false);
            viewport.SetAsFirstSibling();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            scroll.content.SetParent(viewport, false);
            scroll.viewport = viewport;
        }
        originalBottom = viewport.offsetMin.y;
        Apply();
    }

    private void LateUpdate() { Apply(); }

    public void Apply()
    {
        if (viewport == null || footer == null) return;
        var parent = viewport.parent as RectTransform;
        if (parent == null || Mathf.Abs(parent.lossyScale.y) < 0.00001f) return;
        footer.GetWorldCorners(corners);
        float footerTop = parent.InverseTransformPoint(corners[1]).y;
        float gap = Mathf.Abs(footer.lossyScale.y / parent.lossyScale.y) * 24f;
        viewport.GetWorldCorners(corners);
        float bottom = parent.InverseTransformPoint(corners[0]).y;
        var offset = viewport.offsetMin;
        float desired = Mathf.Max(originalBottom, offset.y + footerTop + gap - bottom);
        if (Mathf.Abs(desired - offset.y) > 0.1f)
            viewport.offsetMin = new Vector2(offset.x, desired);
    }
}
