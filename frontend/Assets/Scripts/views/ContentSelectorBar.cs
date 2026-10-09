using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The row of choices at the top of the Training screen, right where Pairs, Boxes and Flash are chosen:
/// Images, Help, Words, Numbers, Random. The choice is saved and applies to ALL games.
/// Only Images is open at first; Words, Numbers and Random unlock with the Memory Athlete badge and
/// Help unlocks by reading the Major System tip. Built in code, so the scene does not change.
/// </summary>
public class ContentSelectorBar : MonoBehaviour
{
    private class Chip
    {
        public CardFaces.Choice Choice;
        public string Name;
        public Image Back;
        public Text Label;
        public Outline Glow;
        public Image Icon;
    }

    private const float BarHeight = 170f;
    private static readonly Color Purple = new Color32(123, 31, 255, 255);
    private static readonly Color Lavender = new Color32(238, 234, 255, 255);
    private static readonly Color LockedBack = new Color32(228, 228, 234, 255);

    private readonly List<Chip> chips = new List<Chip>();
    private Text status;
    private Font font;
    private float hintUntil;
    private string hint;

    /// <summary>Call every time the Training screen is opened; builds the bar once.</summary>
    public static void AttachToTraining(MenuNavigation menu)
    {
        if (menu == null || menu.trainingScreen == null) return;
        var screen = menu.trainingScreen.transform as RectTransform;
        if (screen == null || screen.Find("ContentSelectorBar") != null) return;
        var host = new GameObject("ContentSelectorBar", typeof(RectTransform));
        host.transform.SetParent(screen, false);
        var bar = host.AddComponent<ContentSelectorBar>();
        bar.StartCoroutine(bar.Build(screen));
    }

    private IEnumerator Build(RectTransform screen)
    {
        yield return null; // let the canvas scaler settle first
        Canvas.ForceUpdateCanvases();
#if UNITY_2022_2_OR_NEWER
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        var tiles = new List<RectTransform>();
        foreach (var name in new[] { "Pairs", "Boxes", "Flash" })
        {
            var tile = screen.Find(name) as RectTransform;
            if (tile != null) tiles.Add(tile);
        }
        var canvas = screen.GetComponentInParent<Canvas>();
        if (tiles.Count == 0 || canvas == null) { Destroy(gameObject); yield break; }
        canvas = canvas.rootCanvas;
        float scale = Mathf.Max(0.01f, canvas.scaleFactor);

        // Place the bar BELOW the back arrow, then stack Pairs / Boxes / Flash right under it, close together.
        var corners = new Vector3[4];
        float screenTop = screen.rect.yMax;
        screen.GetWorldCorners(corners);
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        float topPixels = RectTransformUtility.WorldToScreenPoint(cam, corners[1]).y;
        float inset = Mathf.Max(0f, topPixels - Screen.safeArea.yMax) / scale;
        float barTop = screenTop - inset - 8f;
        foreach (var button in canvas.GetComponentsInChildren<Button>(false))
        {
            if (button.transform.IsChildOf(transform)) continue;
            string n = button.name.ToLowerInvariant();
            if (!n.Contains("back") && !n.Contains("arrow") && !n.Contains("return")) continue;
            var br = button.transform as RectTransform;
            if (br == null) continue;
            br.GetWorldCorners(corners);
            float lowest = float.PositiveInfinity, highest = float.NegativeInfinity;
            foreach (var c in corners) { float y = screen.InverseTransformPoint(c).y; lowest = Mathf.Min(lowest, y); highest = Mathf.Max(highest, y); }
            if (highest < screenTop - 400f) continue; // not in the top strip
            barTop = Mathf.Min(barTop, lowest - 14f);
        }

        const float TileGap = 20f;
        tiles.Sort((a, b) => TopOf(b, screen, corners).CompareTo(TopOf(a, screen, corners)));
        float cursor = barTop - BarHeight - TileGap;
        foreach (var tile in tiles)
        {
            float top = TopOf(tile, screen, corners);
            float bottom = BottomOf(tile, screen, corners);
            tile.anchoredPosition += new Vector2(0f, cursor - top);
            cursor -= (top - bottom) + TileGap;
        }

        var rect = (RectTransform)transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        float width = Mathf.Min(screen.rect.width * 0.94f, 1040f);
        rect.sizeDelta = new Vector2(width, BarHeight);
        rect.anchoredPosition = new Vector2(0f, barTop - screenTop);
        BuildChips(rect, width, BarHeight);
        transform.SetAsLastSibling();
        Refresh();
    }

    private static float TopOf(RectTransform t, RectTransform screen, Vector3[] corners)
    {
        t.GetWorldCorners(corners);
        float y = float.NegativeInfinity;
        foreach (var c in corners) y = Mathf.Max(y, screen.InverseTransformPoint(c).y);
        return y;
    }

    private static float BottomOf(RectTransform t, RectTransform screen, Vector3[] corners)
    {
        t.GetWorldCorners(corners);
        float y = float.PositiveInfinity;
        foreach (var c in corners) y = Mathf.Min(y, screen.InverseTransformPoint(c).y);
        return y;
    }

    private static Sprite roundedSprite;
    /// <summary>A soft rounded rectangle (9-sliced), drawn once in code, for the chips.</summary>
    private static Sprite Rounded()
    {
        if (roundedSprite != null) return roundedSprite;
        const int size = 64; const float radius = 26f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x - 0.5f, x + 0.5f - (size - radius), 0f);
                float dy = Mathf.Max(radius - y - 0.5f, y + 0.5f - (size - radius), 0f);
                float a = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }

    private void BuildChips(RectTransform rect, float width, float height)
    {
        float chipHeight = height * 0.72f, spacing = 8f;
        float chipWidth = (width - spacing * 4f) / 5f;
        string[] names = { "Images", "Help", "Words", "Numbers", "Random" };
        CardFaces.Choice[] choices = { CardFaces.Choice.Images, CardFaces.Choice.Help, CardFaces.Choice.Words, CardFaces.Choice.Numbers, CardFaces.Choice.Random };
        for (int i = 0; i < names.Length; i++)
        {
            var chip = new Chip { Choice = choices[i], Name = names[i] };
            var go = new GameObject(names[i], typeof(RectTransform), typeof(Image), typeof(Button));
            var r = (RectTransform)go.transform;
            r.SetParent(rect, false);
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.sizeDelta = new Vector2(chipWidth, chipHeight);
            r.anchoredPosition = new Vector2(i * (chipWidth + spacing), 0f);
            chip.Back = go.GetComponent<Image>();
            // the chip is just the hexagon: it is drawn full size behind the icon and shows as a purple ring when chosen
            chip.Back.sprite = Resources.Load<Sprite>("ContentIcons/chip-" + names[i].ToLowerInvariant());
            chip.Back.preserveAspect = true;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color32(255, 196, 64, 0);
            outline.effectDistance = new Vector2(3f, -3f);
            chip.Glow = outline;
            var button = go.GetComponent<Button>();
            button.targetGraphic = chip.Back;
            var captured = chip;
            button.onClick.AddListener(() => OnChip(captured));
            // a hexagon icon on top, the name underneath
            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var ir = (RectTransform)iconObject.transform;
            ir.SetParent(r, false);
            ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.5f);
            ir.pivot = new Vector2(0.5f, 0.5f);
            ir.sizeDelta = new Vector2(chipHeight * 0.80f, chipHeight * 0.80f);
            ir.anchoredPosition = Vector2.zero;
            chip.Icon = iconObject.GetComponent<Image>();
            chip.Icon.sprite = Resources.Load<Sprite>("ContentIcons/chip-" + names[i].ToLowerInvariant());
            chip.Icon.preserveAspect = true;
            chip.Icon.raycastTarget = false;
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var tr = (RectTransform)textObject.transform;
            tr.SetParent(r, false);
            tr.anchorMin = new Vector2(0f, 0f); tr.anchorMax = new Vector2(1f, 0.38f); tr.offsetMin = tr.offsetMax = Vector2.zero;
            chip.Label = textObject.GetComponent<Text>();
            textObject.SetActive(false); // icons only: the name shows in the line below when a chip is tapped
            chip.Label.font = font;
            chip.Label.alignment = TextAnchor.MiddleCenter;
            chip.Label.resizeTextForBestFit = true;
            chip.Label.resizeTextMinSize = 12;
            chip.Label.resizeTextMaxSize = 26;
            chip.Label.raycastTarget = false;
            var textShadow = textObject.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
            textShadow.effectDistance = new Vector2(1.5f, -2f);
            chips.Add(chip);
        }

        var statusObject = new GameObject("Status", typeof(RectTransform), typeof(Text));
        var sr = (RectTransform)statusObject.transform;
        sr.SetParent(rect, false);
        sr.anchorMin = new Vector2(0f, 0f); sr.anchorMax = new Vector2(1f, 0f);
        sr.pivot = new Vector2(0.5f, 0f);
        sr.sizeDelta = new Vector2(0f, height - chipHeight - 4f);
        sr.anchoredPosition = Vector2.zero;
        status = statusObject.GetComponent<Text>();
        status.font = font;
        status.alignment = TextAnchor.MiddleCenter;
        status.resizeTextForBestFit = true;
        status.resizeTextMinSize = 12;
        status.resizeTextMaxSize = 40;
        status.fontStyle = FontStyle.Bold;
        status.color = new Color32(60, 30, 130, 255);
        status.raycastTarget = false;
    }

    private static bool IsLocked(CardFaces.Choice c)
    {
        if (c == CardFaces.Choice.Help) return !CardFaces.HelpUnlocked;
        if (c == CardFaces.Choice.Words || c == CardFaces.Choice.Numbers || c == CardFaces.Choice.Random) return !CardFaces.AdvancedUnlocked;
        return false;
    }

    private void OnChip(Chip chip)
    {
        if (IsLocked(chip.Choice))
        {
            hint = chip.Choice == CardFaces.Choice.Help
                ? "Help unlocks when you read the Major System tip (Tips)."
                : "Unlocked by the Memory Athlete badge: clear level 20 in Pairs, Flash or Boxes (level 20 is open from the start).";
            hintUntil = Time.unscaledTime + 4f;
            Refresh();
            return;
        }
        var current = CardFaces.Selected;
        switch (chip.Choice)
        {
            case CardFaces.Choice.Help:
                current ^= CardFaces.Choice.Help;
                break;
            case CardFaces.Choice.Random:
                current = (current & CardFaces.Choice.Random) != 0
                    ? (current & ~CardFaces.Choice.Random) | CardFaces.Choice.Images
                    : (current & CardFaces.Choice.Help) | CardFaces.Choice.Random;
                break;
            default:
                current &= ~CardFaces.Choice.Random;
                current ^= chip.Choice;
                break;
        }
        CardFaces.Selected = current; // sanitised on save: at least one kind of card always stays selected
        hint = chip.Name + " - cards in all games: " + CardFaces.Describe(CardFaces.Selected);
        hintUntil = Time.unscaledTime + 3f;
        Refresh();
    }

    private void OnEnable() { if (status != null) Refresh(); }

    private void Update()
    {
        if (hint != null && Time.unscaledTime > hintUntil) { hint = null; Refresh(); }
    }

    private void Refresh()
    {
        var selected = CardFaces.Selected;
        foreach (var chip in chips)
        {
            bool locked = IsLocked(chip.Choice);
            bool on = !locked && (selected & chip.Choice) != 0;
            chip.Back.color = on ? Purple : Color.clear;
            if (chip.Glow != null) chip.Glow.effectColor = on ? new Color32(255, 196, 64, 255) : new Color32(123, 31, 255, locked ? (byte)0 : (byte)70);
            chip.Label.color = locked ? new Color(0.58f, 0.58f, 0.64f) : (on ? Color.white : new Color(0.16f, 0.12f, 0.35f));
            chip.Label.fontStyle = on ? FontStyle.Bold : FontStyle.Normal;
            if (chip.Icon != null) chip.Icon.color = locked ? new Color(0.72f, 0.72f, 0.78f, 0.85f) : Color.white;
            chip.Label.text = chip.Name;
        }
        if (status != null) status.text = hint ?? "";
    }
}
