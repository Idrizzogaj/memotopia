using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class MajorSystemValidation
{
    public static void Review()
    {
        Validate();
        Directory.CreateDirectory("../docs/major-system/normalized-review");
        GameObject cameraObject = new GameObject("Review camera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 640;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.92f, 0.96f, 0.97f);
        camera.cullingMask = 1 << 30;
        RenderTexture target = new RenderTexture(1024, 1280, 24);
        camera.targetTexture = target;
        GameObject root = new GameObject("Review canvas", typeof(RectTransform), typeof(Canvas));
        root.layer = 30;
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(1024, 1280);
        for (int id = 1; id <= 100; id++)
        {
            float x = -450 + ((id - 1) % 10) * 100;
            float y = 570 - ((id - 1) / 10) * 125;
            Image picture = ReviewImage(root.transform, new Vector2(x, y), new Vector2(96, 96));
            picture.sprite = MajorSystemImages.Load(id);
            GameObject label = new GameObject("Number", typeof(RectTransform), typeof(Text));
            label.layer = 30; label.transform.SetParent(root.transform, false);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90, 25); rect.anchoredPosition = new Vector2(x, y - 53);
            Text text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.black; text.text = (id - 1).ToString("D2");
        }
        SaveReview(camera, target, "../docs/major-system/normalized-review/all-100.png");
        for (int i = root.transform.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
        camera.backgroundColor = new Color32(77, 211, 240, 255);
        int[] examples = { 60, 53, 47, 78, 74, 52 };
        for (int i = 0; i < examples.Length; i++)
        {
            Image cell = ReviewImage(root.transform, new Vector2(i % 2 == 0 ? -245 : 245, 400 - (i / 2) * 350), new Vector2(430, 260));
            cell.sprite = Resources.Load<Sprite>("GamesResources/Boxes/boxes-ring"); cell.preserveAspect = true;
            Image picture = ReviewImage(cell.transform, Vector2.zero, new Vector2(200, 200));
            picture.sprite = MajorSystemImages.Load(examples[i] + 1);
            BoxesPicturePerspective.Attach(picture);
            foreach (Transform child in cell.transform) child.gameObject.layer = 30;
        }
        SaveReview(camera, target, "../docs/major-system/normalized-review/boxes.png");
        UnityEngine.Object.DestroyImmediate(root);
        UnityEngine.Object.DestroyImmediate(cameraObject);
        UnityEngine.Object.DestroyImmediate(target);
        Debug.Log("Visual reviews saved for all 100 motifs and Boxes platforms.");
    }

    private static Image ReviewImage(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject("Review image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.layer = 30; obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = size; rect.anchoredPosition = position;
        return obj.GetComponent<Image>();
    }

    private static void SaveReview(Camera camera, RenderTexture target, string path)
    {
        Canvas.ForceUpdateCanvases(); camera.Render();
        RenderTexture previous = RenderTexture.active; RenderTexture.active = target;
        Texture2D screenshot = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); screenshot.Apply();
        File.WriteAllBytes(path, screenshot.EncodeToPNG());
        RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(screenshot);
    }

    [MenuItem("Memotopia/Validate Major System images")]
    public static void Validate()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("MajorSystem");
        if (sprites.Length != MajorSystemImages.Count)
            throw new InvalidOperationException("Expected exactly 100 Major System sprites, got " + sprites.Length);

        for (int id = 1; id <= MajorSystemImages.Count; id++)
        {
            Sprite sprite = Resources.Load<Sprite>(MajorSystemImages.ResourcePath(id));
            string expectedPath = "Assets/Resources/" + MajorSystemImages.ResourcePath(id) + ".png";
            if (AssetDatabase.GetAssetPath(sprite) != expectedPath)
                throw new InvalidOperationException("Wrong image for game ID " + id);
            Sprite clean = MajorSystemImages.Load(id);
            if (clean.rect.width != 256 || clean.rect.height != 256)
                throw new InvalidOperationException("Invalid clean image: " + id);
            if (sprite.rect.width <= 0 || sprite.rect.height <= 0)
                throw new InvalidOperationException("Empty sprite: " + expectedPath);
        }

        Debug.Log("Major System validation passed: all 100 sprites (00–99) load correctly.");
    }
}
