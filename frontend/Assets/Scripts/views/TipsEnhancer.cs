using System.Collections.Generic;
using Assets.Script.Controllers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds to the Tips screen: a free "Major System" tip, and an "I have read this" box on every tip page.
/// Ticking it earns that tip's badge; reading the Major System tip also unlocks Help in the games.
/// Everything is built in code, so the scene does not need to change.
/// </summary>
public static class TipsEnhancer
{
    private static readonly Dictionary<string, string> PageKeys = new Dictionary<string, string>
    {
        { "LinkMethodPage", "tip-link-method" }, { "FunPage", "tip-fun" }, { "MemoryPalacePage", "tip-memory-palace" },
        { "SensesPage", "tip-senses" }, { "EmotionsPage", "tip-emotions" }, { "RetrivalPage", "tip-retrieval" },
        { "MythPage", "tip-myth" }, { "MindsetPage", "tip-mindset" }, { "FocusPage", "tip-focus" },
        { "BornPage", "tip-born" }, { "DreamPage", "tip-dream" }, { "MajorSystemPage", "tip-major-system" },
    };

    private static readonly Color Purple = new Color32(123, 31, 255, 255);
    private static readonly Color Ink = new Color32(43, 43, 58, 255);

    private static Font font
    {
        get
        {
#if UNITY_2022_2_OR_NEWER
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }
    }

    public static void Ensure(MenuNavigation menu)
    {
        if (menu == null || menu.tipsScreen == null) return;
        var tips = menu.tipsScreen.transform;
        var screens = tips.Find("Screens");
        var content = tips.Find("Scroll View/Viewport/Content");
        if (screens == null || content == null) return;

        // the free Major System tip: a card in the list and its own page
        if (screens.Find("MajorSystemPage") == null)
        {
            var page = BuildMajorSystemPage(screens, menu);
            var template = content.Find("LinkMethod");
            if (template != null && content.Find("MajorSystem") == null)
            {
                var card = Object.Instantiate(template.gameObject, content, false);
                card.name = "MajorSystem";
                var image = card.GetComponent<Image>();
                var art = Resources.Load<Sprite>("Memotopia_UI/Cards and PopUps/Tips/major system-01");
                if (image != null && art != null) image.sprite = art;
                var button = card.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick = new Button.ButtonClickedEvent(); // drop the copied Link Method action
                    button.onClick.AddListener(() => { page.SetActive(true); page.transform.SetAsLastSibling(); });
                }
                // last of the open (free) tips: right after Fun, before the Premium tips
                var fun = content.Find("FunImage");
                card.transform.SetSiblingIndex(fun != null ? fun.GetSiblingIndex() + 1 : content.childCount - 1);
            }
        }

        // an "I have read this" box on every page
        foreach (Transform page in screens)
        {
            string key;
            if (!PageKeys.TryGetValue(page.name, out key)) continue;
            if (page.Find("ReadBar") != null) continue;
            BuildReadBar(page, key);
        }

        // a large touch area for the back button of every tip page
        foreach (Transform page in screens)
            if (PageKeys.ContainsKey(page.name)) EnlargeTopButtons(page);
    }

    // ---------- bigger touch areas

    private const float MinTouchUnits = 170f; // in canvas units, about 15% of the screen width

    /// <summary>Gives every button in the top strip of a page (the back button) a much larger invisible touch area.</summary>
    private static void EnlargeTopButtons(Transform page)
    {
        var pageRect = page as RectTransform;
        if (pageRect == null) return;
        var corners = new Vector3[4];
        pageRect.GetWorldCorners(corners);
        float top = corners[1].y, bottom = corners[0].y;
        float limit = top - (top - bottom) * 0.25f;
        var readBar = page.Find("ReadBar");
        foreach (var button in page.GetComponentsInChildren<Button>(true))
        {
            if (readBar != null && button.transform.IsChildOf(readBar)) continue;
            if (button.name == "BigHit" || button.transform.parent.name == "BigHit") continue;
            var rect = button.transform as RectTransform;
            if (rect == null) continue;
            rect.GetWorldCorners(corners);
            float centreY = (corners[0].y + corners[2].y) * 0.5f;
            if (centreY >= limit) EnlargeTouch(button, MinTouchUnits);
        }
    }

    private static void EnlargeTouch(Button button, float minUnits)
    {
        var rect = (RectTransform)button.transform;
        if (rect.Find("BigHit") != null) return;
        var canvas = button.GetComponentInParent<Canvas>();
        float canvasScale = canvas != null ? Mathf.Max(0.0001f, canvas.rootCanvas.transform.lossyScale.x) : 1f;
        float unitX = Mathf.Max(0.0001f, Mathf.Abs(rect.lossyScale.x) / canvasScale);
        float unitY = Mathf.Max(0.0001f, Mathf.Abs(rect.lossyScale.y) / canvasScale);
        var hit = new GameObject("BigHit", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        var hr = (RectTransform)hit.transform;
        hr.SetParent(rect, false);
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f);
        hr.pivot = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = Vector2.zero;
        hr.sizeDelta = new Vector2(Mathf.Max(rect.rect.width, minUnits / unitX), Mathf.Max(rect.rect.height, minUnits / unitY));
        var image = hit.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = true;
        hit.GetComponent<LayoutElement>().ignoreLayout = true;
    }

    // ---------- the "I have read this" box

    private static void BuildReadBar(Transform page, string key)
    {
        var bar = new GameObject("ReadBar", typeof(RectTransform), typeof(Image), typeof(TipReadBar));
        var rect = (RectTransform)bar.transform;
        rect.SetParent(page, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, 200f);
        rect.anchoredPosition = Vector2.zero;
        bar.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.96f);
        bar.GetComponent<TipReadBar>().Setup(key, font);
        rect.SetAsLastSibling();

        // leave room under the text so the bar never hides the last lines
        var scroll = page.GetComponentInChildren<ScrollRect>(true);
        if (scroll != null && scroll.content != null && scroll.content.GetComponent<LayoutGroup>() != null
            && scroll.content.Find("ReadSpacer") == null)
        {
            var spacer = new GameObject("ReadSpacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(scroll.content, false);
            spacer.GetComponent<LayoutElement>().minHeight = 220f;
            spacer.GetComponent<LayoutElement>().preferredHeight = 220f;
        }
    }

    // ---------- the Major System page

    private static GameObject BuildMajorSystemPage(Transform parent, MenuNavigation menu)
    {
        var page = new GameObject("MajorSystemPage", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)page.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        page.GetComponent<Image>().color = Color.white;

        // title
        var title = MakeText(rect, "Title", "The Major System", 60, FontStyle.Bold, Purple, TextAnchor.MiddleCenter);
        var tr = (RectTransform)title.transform;
        tr.anchorMin = new Vector2(0.2f, 0.915f); tr.anchorMax = new Vector2(0.8f, 0.985f); tr.offsetMin = tr.offsetMax = Vector2.zero;

        // back button
        var back = new GameObject("Back", typeof(RectTransform), typeof(Image), typeof(Button));
        var br = (RectTransform)back.transform;
        br.SetParent(rect, false);
        br.anchorMin = new Vector2(0.03f, 0.92f); br.anchorMax = new Vector2(0.22f, 0.98f); br.offsetMin = br.offsetMax = Vector2.zero;
        back.GetComponent<Image>().color = new Color(0.93f, 0.91f, 1f, 1f);
        // like the other tip pages: back goes through GoToTips, which shows the list of tips again
        back.GetComponent<Button>().onClick.AddListener(() => { page.SetActive(false); if (menu != null) menu.GoToTips(); });
        var backText = MakeText(br, "Label", "< Back", 34, FontStyle.Bold, Purple, TextAnchor.MiddleCenter);
        Stretch((RectTransform)backText.transform);

        // scrolling text
        var scrollObject = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        var sr = (RectTransform)scrollObject.transform;
        sr.SetParent(rect, false);
        sr.anchorMin = new Vector2(0.05f, 0.14f); sr.anchorMax = new Vector2(0.95f, 0.905f); sr.offsetMin = sr.offsetMax = Vector2.zero;
        scrollObject.GetComponent<Image>().color = Color.clear;
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        var vr = (RectTransform)viewport.transform;
        vr.SetParent(sr, false);
        Stretch(vr);
        var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var cr = (RectTransform)contentObject.transform;
        cr.SetParent(vr, false);
        cr.anchorMin = new Vector2(0f, 1f); cr.anchorMax = new Vector2(1f, 1f); cr.pivot = new Vector2(0.5f, 1f);
        cr.offsetMin = new Vector2(0f, 0f); cr.offsetMax = new Vector2(0f, 0f);
        var layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 22f; layout.padding = new RectOffset(8, 8, 8, 40);
        layout.childControlWidth = true; layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.viewport = vr; scroll.content = cr; scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic; scroll.scrollSensitivity = 30f;

        AddParagraph(cr, "Turn any number into a picture, and any picture back into a number. Memory champions use this system to remember long numbers, dates and phone numbers.", 36, false);
        AddParagraph(cr, "The idea", 44, true);
        AddParagraph(cr, "Every digit stands for a consonant sound. Put the sounds of a number together, add vowels, and you get a word you can SEE. That word is your picture.", 36, false);
        AddParagraph(cr, "The sounds", 44, true);
        AddParagraph(cr,
            "0  =  S  (also Z)\n1  =  T  (also D)\n2  =  N\n3  =  M\n4  =  R\n5  =  L\n6  =  J  (also SH, CH)\n7  =  K  (also hard G)\n8  =  F  (also V)\n9  =  P  (also B)", 40, false);
        AddParagraph(cr, "Easy ways to remember them", 44, true);
        AddParagraph(cr,
            "0 = S / Z:  \"zero\" starts with Z\n1 = T / D:  T has one downstroke\n2 = N:  N has two downstrokes\n3 = M:  M has three downstrokes\n"
            + "4 = R:  R is the last letter of \"four\"\n5 = L:  L is 50 in Roman numerals\n6 = J / SH / CH:  J looks like a mirrored 6\n"
            + "7 = K / G:  K can be drawn with two 7s\n8 = F / V:  a handwritten f looks like an 8\n9 = P / B:  P is a mirrored 9", 36, false);
        AddParagraph(cr, "Only the sounds count", 44, true);
        AddParagraph(cr, "Vowels and the letters W, H and Y are free, and silent letters do not count. So you can build real words.", 36, false);
        AddParagraph(cr, "Examples", 44, true);
        AddExample(cr, 0, "S + S", "SAUCE");
        AddExample(cr, 2, "S + N", "SIGN (the G is silent)");
        AddExample(cr, 11, "T + D", "TOAD");
        AddExample(cr, 18, "T + V", "TV");
        AddExample(cr, 27, "N + K", "NECK");
        AddExample(cr, 99, "B + B", "BABY");
        AddParagraph(cr, "In Memotopia", 44, true);
        AddParagraph(cr,
            "Every number from 00 to 99 has its own picture in the games. Number 02 is a SIGN. Link the pictures in a story or place them in a memory palace and long numbers become easy.", 36, false);
        AddParagraph(cr, "Tick the box below when you have read this. It gives you a badge and unlocks Help in the games: a small label on every picture shows its number and its sounds.", 36, false);

        BuildReadBar(rect, "tip-major-system");
        page.SetActive(false);
        return page;
    }

    /// <summary>One row that shows how a number turns into a picture: the picture, the number, its sounds and the word.</summary>
    private static void AddExample(RectTransform content, int number, string sounds, string word)
    {
        var row = new GameObject("Example " + number, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(content, false);
        row.GetComponent<Image>().color = new Color32(246, 243, 255, 255);
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 28f;
        layout.padding = new RectOffset(24, 24, 16, 16);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true; layout.childControlHeight = true;
        layout.childForceExpandWidth = false; layout.childForceExpandHeight = false;
        row.GetComponent<LayoutElement>().minHeight = 230f;

        var picture = new GameObject("Picture", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        picture.transform.SetParent(row.transform, false);
        var image = picture.GetComponent<Image>();
        image.sprite = MajorSystemImages.Load(number + 1);
        image.preserveAspect = true;
        image.raycastTarget = false;
        var pictureSize = picture.GetComponent<LayoutElement>();
        pictureSize.minWidth = pictureSize.preferredWidth = 190f;
        pictureSize.minHeight = pictureSize.preferredHeight = 190f;

        var text = MakeText(row.transform, "Text", "", 36, FontStyle.Normal, Ink, TextAnchor.MiddleLeft);
        text.supportRichText = true;
        text.text = "<size=64><b><color=#7B1FFF>" + number.ToString("D2") + "</color></b></size>\n" + sounds + "\n<b>" + word + "</b>";
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
    }

    private static void AddParagraph(RectTransform content, string text, int size, bool heading)
    {
        var t = MakeText(content, heading ? "Heading" : "Paragraph", text, size, heading ? FontStyle.Bold : FontStyle.Normal,
            heading ? Purple : Ink, TextAnchor.UpperLeft);
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private static Text MakeText(Transform parent, string name, string text, int size, FontStyle style, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = anchor;
        t.raycastTarget = false;
        return t;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}

/// <summary>
/// The box at the bottom of a tip page. It can only be ticked after the page has been scrolled all the way down.
/// Ticking earns the tip's badge once, and when the page is left a badge popup shows what was earned.
/// </summary>
public class TipReadBar : MonoBehaviour
{
    private string key;
    private Font font;
    private Image box;
    private Text label;
    private ScrollRect scroll;
    private bool reachedBottom, tickedThisVisit;

    public void Setup(string tipKey, Font labelFont)
    {
        key = tipKey;
        font = labelFont;
        var rect = (RectTransform)transform;

        var boxObject = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(Button));
        var br = (RectTransform)boxObject.transform;
        br.SetParent(rect, false);
        br.anchorMin = br.anchorMax = new Vector2(0f, 0.5f);
        br.pivot = new Vector2(0f, 0.5f);
        br.sizeDelta = new Vector2(130f, 130f);
        br.anchoredPosition = new Vector2(36f, 0f);
        box = boxObject.GetComponent<Image>();
        boxObject.GetComponent<Button>().onClick.AddListener(Tick);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Button));
        var lr = (RectTransform)labelObject.transform;
        lr.SetParent(rect, false);
        lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(1f, 1f);
        lr.offsetMin = new Vector2(190f, 8f); lr.offsetMax = new Vector2(-24f, -8f);
        label = labelObject.GetComponent<Text>();
        label.font = font;
        label.fontSize = 36;
        label.alignment = TextAnchor.MiddleLeft;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 20;
        label.resizeTextMaxSize = 36;
        labelObject.GetComponent<Button>().onClick.AddListener(Tick);
        // the whole bar is one big touch area
        var barButton = gameObject.AddComponent<Button>();
        barButton.targetGraphic = GetComponent<Image>();
        barButton.transition = Selectable.Transition.None;
        barButton.onClick.AddListener(Tick);
        Refresh();
    }

    private void OnEnable()
    {
        reachedBottom = false;
        tickedThisVisit = false;
        if (label != null) Refresh();
    }

    // Leaving the page after ticking it: show the badge that was earned.
    private void OnDisable()
    {
        if (!tickedThisVisit) return;
        tickedThisVisit = false;
        try
        {
            if (Application.isPlaying && gameObject.scene.isLoaded) BadgePopup.Show(this, key, font);
        }
        catch (System.Exception) { /* the badge is saved either way */ }
    }

    private bool Done { get { return GameManager.completedAchievements.Contains(key); } }

    private void Update()
    {
        if (Done || reachedBottom) return;
        if (scroll == null)
            foreach (var candidate in transform.parent.GetComponentsInChildren<ScrollRect>(true))
                if (candidate.vertical && candidate.content != null) { scroll = candidate; break; }
        if (scroll == null || AtBottom(scroll))
        {
            reachedBottom = true;
            Refresh();
        }
    }

    private static bool AtBottom(ScrollRect s)
    {
        var view = s.viewport != null ? s.viewport : (RectTransform)s.transform;
        if (s.content.rect.height <= view.rect.height + 1f) return true; // nothing to scroll
        return s.verticalNormalizedPosition <= 0.02f;
    }

    private void Tick()
    {
        if (Done) return;
        if (!reachedBottom) { Refresh(); return; } // not scrolled to the end yet
        if (!GameManager.UnlockAchievement(key)) return;
        tickedThisVisit = true;
        try { ProgressSync.Instance.GetApi<AchievementsAPIController>().FlushPending(); } catch (System.Exception) { /* synced later */ }
        Refresh();
    }

    private void Refresh()
    {
        bool done = Done;
        box.color = done ? new Color32(123, 31, 255, 255) : (reachedBottom ? new Color32(220, 212, 255, 255) : new Color32(236, 236, 240, 255));
        string badge = "";
        foreach (var def in ExtraAchievements.All) if (def.Key == key) badge = def.Title;
        if (done)
        {
            label.color = new Color32(123, 31, 255, 255);
            label.text = key == CardFaces.HelpKey
                ? "Read! Badge earned: " + badge + ". Help is unlocked in the games."
                : "Read! Badge earned: " + badge;
        }
        else if (!reachedBottom)
        {
            label.color = new Color32(130, 130, 145, 255);
            label.text = "Scroll to the end of the tip to continue";
        }
        else
        {
            label.color = new Color32(43, 43, 58, 255);
            label.text = "I have read this tip";
        }
    }
}

/// <summary>A small full-screen popup that shows a newly earned tip badge.</summary>
public static class BadgePopup
{
    /// <summary>Shows the popup for a badge key with the built-in font.</summary>
    public static void ShowFor(MonoBehaviour from, string key)
    {
#if UNITY_2022_2_OR_NEWER
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        Show(from, key, font);
    }

    public static void Show(MonoBehaviour from, string key, Font font)
    {
        var canvas = from.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var root = canvas.rootCanvas.transform;

        var catalog = new List<Achievement>();
        ExtraAchievements.AppendTo(catalog);
        var achievement = catalog.Find(a => a != null && a.constantString == key);
        if (achievement == null) return;

        var overlay = new GameObject("BadgePopup", typeof(RectTransform), typeof(Image));
        var orect = (RectTransform)overlay.transform;
        orect.SetParent(root, false);
        orect.anchorMin = Vector2.zero; orect.anchorMax = Vector2.one;
        orect.offsetMin = orect.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0.08f, 0.04f, 0.2f, 0.78f);
        orect.SetAsLastSibling();

        var card = Panel("Card", orect, new Vector2(0.07f, 0.22f), new Vector2(0.93f, 0.78f), Color.white);

        var badge = new GameObject("Badge", typeof(RectTransform), typeof(Image));
        var brect = (RectTransform)badge.transform;
        brect.SetParent(card, false);
        brect.anchorMin = new Vector2(0.2f, 0.48f); brect.anchorMax = new Vector2(0.8f, 0.96f);
        brect.offsetMin = brect.offsetMax = Vector2.zero;
        var badgeImage = badge.GetComponent<Image>();
        badgeImage.sprite = achievement.icon;
        badgeImage.preserveAspect = true;
        badgeImage.raycastTarget = false;

        Label(card, font, "NEW BADGE", new Vector2(0.05f, 0.39f), new Vector2(0.95f, 0.48f), 30, new Color32(123, 31, 255, 255), FontStyle.Bold);
        Label(card, font, AchievementsView.DisplayTitle(achievement, key), new Vector2(0.05f, 0.27f), new Vector2(0.95f, 0.39f), 44, new Color32(43, 43, 58, 255), FontStyle.Bold);
        Label(card, font, achievement.description, new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.27f), 28, new Color32(90, 90, 105, 255), FontStyle.Normal);

        var button = Panel("Ok", card, new Vector2(0.28f, 0.03f), new Vector2(0.72f, 0.13f), new Color32(123, 31, 255, 255));
        button.gameObject.AddComponent<Button>().onClick.AddListener(() => Object.Destroy(overlay));
        Label(button, font, "OK", Vector2.zero, Vector2.one, 36, Color.white, FontStyle.Bold);
    }

    private static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return rect;
    }

    private static void Label(Transform parent, Font font, string text, Vector2 min, Vector2 max, int size, Color color, FontStyle style)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = new Vector2(8f, 4f); rect.offsetMax = new Vector2(-8f, -4f);
        var t = go.GetComponent<Text>();
        t.font = font; t.text = text; t.color = color; t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.resizeTextForBestFit = true; t.resizeTextMinSize = 16; t.resizeTextMaxSize = size;
        t.raycastTarget = false;
    }
}
