using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>Shared 00–99 Major System picture set for all three games.</summary>
public static class MajorSystemImages
{
    public const int Count = 100;
    private static readonly Sprite[] cleanSprites = new Sprite[Count];
    private static string[] sourcePaths;
    private static readonly Dictionary<Sprite, Vector2> visibleSizes = new Dictionary<Sprite, Vector2>();

    public static Vector2 VisibleSize(Sprite sprite)
    {
        Vector2 size;
        return sprite != null && visibleSizes.TryGetValue(sprite, out size) ? size : Vector2.one;
    }

    // Game logic uses IDs 1–100; the corresponding Major System numbers are 00–99.
    public static string ResourcePath(int imageNumber)
    {
        if (imageNumber < 1 || imageNumber > Count)
            throw new ArgumentOutOfRangeException(nameof(imageNumber));

        return "MajorSystem/" + (imageNumber - 1).ToString("D2");
    }

    public static Sprite Load(int imageNumber)
    {
        ResourcePath(imageNumber); // Validate before indexing the cache.
        int index = imageNumber - 1;
        if (cleanSprites[index] != null) return cleanSprites[index];
        if (sourcePaths == null)
        {
            sourcePaths = Resources.Load<TextAsset>("MajorSystemSourceMap").text.Trim().Split('\n');
            if (sourcePaths.Length != Count) throw new InvalidOperationException("Expected 100 clean image sources.");
        }
        string path = sourcePaths[index].Trim();
        Sprite source = Resources.Load<Sprite>(path);
        if (source == null) throw new InvalidOperationException("Missing image source: " + path);
        cleanSprites[index] = MakeCleanSprite(source, path.StartsWith("MajorSystem/"), index);
        return cleanSprites[index];
    }

    public static Sprite LoadPath(string path)
    {
        if (path.StartsWith("MajorSystem/"))
            return Load(int.Parse(path.Substring("MajorSystem/".Length)) + 1);
        return Resources.Load<Sprite>(path);
    }

    // Presentation-only normalization. The approved source PNGs remain unchanged.
    // Older cards use their matching transparent original; new flat illustrations
    // lose only the near-white background connected to the outside of the image.
    private static Sprite MakeCleanSprite(Sprite source, bool removeWhite, int index)
    {
        Texture2D texture = source.texture;
        int width = texture.width, height = texture.height;
        Color32[] pixels = texture.GetPixels32();
        if (removeWhite)
        {
            bool[] visited = new bool[pixels.Length];
            int[] queue = new int[pixels.Length];
            int read = 0, write = 0;
            Action<int> visit = p =>
            {
                if (visited[p]) return;
                visited[p] = true;
                Color32 c = pixels[p];
                if (c.a == 0 || (c.r >= 242 && c.g >= 242 && c.b >= 242))
                    queue[write++] = p;
            };
            for (int x = 0; x < width; x++) { visit(x); visit((height - 1) * width + x); }
            for (int y = 0; y < height; y++) { visit(y * width); visit(y * width + width - 1); }
            while (read < write)
            {
                int p = queue[read++];
                pixels[p].a = 0;
                int x = p % width, y = p / width;
                if (x > 0) visit(p - 1);
                if (x + 1 < width) visit(p + 1);
                if (y > 0) visit(p - width);
                if (y + 1 < height) visit(p + width);
            }
        }
        int left = width, bottom = height, right = -1, top = -1;
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            if (pixels[y * width + x].a < 16) continue;
            left = Math.Min(left, x); right = Math.Max(right, x);
            bottom = Math.Min(bottom, y); top = Math.Max(top, y);
        }
        if (right < left) throw new InvalidOperationException("Empty motif: " + index);
        Texture2D cutout = new Texture2D(width, height, TextureFormat.RGBA32, false);
        cutout.SetPixels32(pixels);
        cutout.Apply();
        const int size = 256, margin = 24;
        float scale = (size - margin * 2f) / Mathf.Max(right - left + 1, top - bottom + 1);
        float centerX = (left + right) * 0.5f, centerY = (bottom + top) * 0.5f;
        Color[] normalized = new Color[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float sx = centerX + (x - (size - 1) * 0.5f) / scale;
            float sy = centerY + (y - (size - 1) * 0.5f) / scale;
            if (sx >= left && sx <= right && sy >= bottom && sy <= top)
                normalized[y * size + x] = cutout.GetPixelBilinear((sx + 0.5f) / width, (sy + 0.5f) / height);
        }
        if (Application.isPlaying) UnityEngine.Object.Destroy(cutout);
        else UnityEngine.Object.DestroyImmediate(cutout);
        Texture2D result = new Texture2D(size, size, TextureFormat.RGBA32, false);
        result.name = "MajorSystem clean " + index.ToString("D2");
        result.SetPixels(normalized);
        result.Apply(false, true);
        Sprite sprite = Sprite.Create(result, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        sprite.name = index.ToString("D2");
        visibleSizes[sprite] = new Vector2((right - left + 1) * scale / size, (top - bottom + 1) * scale / size);
        return sprite;
    }
}
