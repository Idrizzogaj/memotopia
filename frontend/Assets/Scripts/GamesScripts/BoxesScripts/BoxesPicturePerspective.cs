using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Places an upright motif on a separate isometric platform.</summary>
[RequireComponent(typeof(Image))]
public class BoxesPicturePerspective : BaseMeshEffect
{
    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || mesh.currentVertCount == 0 || transform.parent == null)
            return;

        Image cell = transform.parent.GetComponent<Image>();
        if (cell == null || cell.sprite == null || !cell.sprite.name.StartsWith("boxes-ring"))
            return; // Cards in the answer tray remain upright and readable.

        RectTransform pictureRect = graphic.rectTransform;
        Rect source = pictureRect.rect;
        Rect face = cell.rectTransform.rect;
        float aspect = cell.sprite.rect.width / cell.sprite.rect.height;
        float width = Mathf.Min(face.width, face.height * aspect);
        float height = width / aspect;
        if (source.width <= 0 || source.height <= 0)
            return;

        UIVertex vertex = new UIVertex();
        for (int i = 0; i < mesh.currentVertCount; i++)
        {
            mesh.PopulateUIVertex(ref vertex, i);
            float x = (vertex.position.x - source.center.x) / source.width;
            float y = (vertex.position.y - source.center.y) / source.height;
            // The platform supplies perspective. Never shear the actual motif.
            float size = Mathf.Min(width * 0.46f, height * 0.82f);
            float visibleHeight = MajorSystemImages.VisibleSize(((Image)graphic).sprite).y;
            float baseline = -height * 0.18f + visibleHeight * size * 0.5f;
            Vector3 point = new Vector3(face.center.x + x * size,
                face.center.y + baseline + y * size, 0);
            vertex.position = pictureRect.InverseTransformPoint(cell.rectTransform.TransformPoint(point));
            mesh.SetUIVertex(vertex, i);
        }
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        EnsurePlatform();
        if (graphic != null)
            graphic.SetVerticesDirty();
    }

    public static void Attach(Image picture)
    {
        // Board geometry is set by the projection; avoid letterboxing the source quad.
        picture.preserveAspect = false;
        if (picture.GetComponent<BoxesPicturePerspective>() == null)
            picture.gameObject.AddComponent<BoxesPicturePerspective>();
        picture.GetComponent<BoxesPicturePerspective>().EnsurePlatform();
    }

    private void EnsurePlatform()
    {
        if (transform.parent == null) return;
        Image cell = transform.parent.GetComponent<Image>();
        if (cell == null || cell.sprite == null || !cell.sprite.name.StartsWith("boxes-ring")) return;
        if (transform.parent.Find("MajorSystemPlatform") != null) return;
        GameObject platform = new GameObject("MajorSystemPlatform", typeof(RectTransform), typeof(CanvasRenderer), typeof(MajorSystemPlatform));
        platform.transform.SetParent(transform.parent, false);
        platform.transform.SetAsFirstSibling();
        RectTransform rect = (RectTransform)platform.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        platform.GetComponent<MajorSystemPlatform>().raycastTarget = false;
    }
}

/// <summary>Shared UI geometry: a rounded, gently skewed platform following the active cell outline.</summary>
public class MajorSystemPlatform : MaskableGraphic
{
    private bool shadowVisible;

    private bool HasVisiblePicture()
    {
        if (transform.parent == null) return false;
        foreach (Transform child in transform.parent)
        {
            Image picture = child.GetComponent<Image>();
            if (picture != null && picture.isActiveAndEnabled && picture.sprite != null &&
                picture.color.a > 0 && child.GetComponent<BoxesPicturePerspective>() != null &&
                (picture.type != Image.Type.Filled || picture.fillAmount > 0)) return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        bool visible = HasVisiblePicture();
        if (shadowVisible == visible) return;
        shadowVisible = visible;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Image cell = transform.parent.GetComponent<Image>();
        if (cell == null || cell.sprite == null) return;
        Rect rect = rectTransform.rect;
        float aspect = cell.sprite.rect.width / cell.sprite.rect.height;
        float width = Mathf.Min(rect.width, rect.height * aspect), height = width / aspect;
        Vector2 center = rect.center + Vector2.up * height * 0.035f;
        // Match the slight skew in boxes-ring rather than rotating the upright motif.
        Vector2[] corners = {
            center + new Vector2(-width * .44f, height * .022f),
            center + new Vector2(-width * .026f, height * .40f),
            center + new Vector2(width * .44f, -height * .022f),
            center + new Vector2(width * .026f, -height * .40f)
        };
        var outline = new List<Vector2>();
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 corner = corners[i];
            Vector2 incoming = Vector2.Lerp(corner, corners[(i + 3) % 4], .10f);
            Vector2 outgoing = Vector2.Lerp(corner, corners[(i + 1) % 4], .10f);
            for (int step = 0; step <= 6; step++)
            {
                float t = step / 6f;
                outline.Add((1 - t) * (1 - t) * incoming + 2 * (1 - t) * t * corner + t * t * outgoing);
            }
        }
        Vector2 depth = Vector2.down * height * .075f;
        for (int i = 0; i < outline.Count; i++)
        {
            Vector2 a = outline[i], b = outline[(i + 1) % outline.Count];
            if (b.x >= a.x) continue; // Only the front half has visible thickness.
            Color shade = Color.Lerp(new Color32(211, 247, 253, 255), new Color32(158, 224, 240, 255),
                Mathf.Clamp01(((a.x + b.x) * .5f - center.x) / (width * .15f) + .5f));
            Quad(mesh, a, b, b + depth, a + depth, shade);
        }
        int faceStart = mesh.currentVertCount;
        mesh.AddVert(center, Color.white, Vector2.zero);
        foreach (Vector2 point in outline) mesh.AddVert(point, Color.white, Vector2.zero);
        for (int i = 0; i < outline.Count; i++)
            mesh.AddTriangle(faceStart, faceStart + 1 + i, faceStart + 1 + (i + 1) % outline.Count);
        shadowVisible = HasVisiblePicture();
        if (!shadowVisible) return;
        // A soft-looking small contact shadow grounds the upright motif.
        int centerIndex = mesh.currentVertCount;
        Vector2 shadowCenter = rect.center + Vector2.down * height * 0.17f;
        Color shadow = new Color(0.18f, 0.39f, 0.44f, 0.13f);
        mesh.AddVert(shadowCenter, shadow, Vector2.zero);
        const int segments = 24;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            mesh.AddVert(shadowCenter + new Vector2(Mathf.Cos(angle) * width * 0.16f, Mathf.Sin(angle) * height * 0.035f), shadow, Vector2.zero);
        }
        for (int i = 0; i < segments; i++) mesh.AddTriangle(centerIndex, centerIndex + 1 + i, centerIndex + 1 + (i + 1) % segments);
    }

    private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero); mesh.AddVert(b, color, Vector2.zero);
        mesh.AddVert(c, color, Vector2.zero); mesh.AddVert(d, color, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }
}
