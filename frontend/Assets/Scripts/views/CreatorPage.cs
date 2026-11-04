using System;
using System.Collections;
using Assets.Script.Constants;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Meet the creator": same content and links as the page in Captain Zogaj. Free users get it now and then
/// (CreatorPromo); paying users only find it themselves, under Settings in the side menu.
/// Built in code, no scene changes.
/// </summary>
public class CreatorPage : MonoBehaviour
{
    // Store rules: Apple 3.1.1 and Google Payments policy may object to external tipping links inside the app.
    // The product links are always shown; the Ko-fi / Buy Me a Coffee buttons follow this switch.
    public static bool ShowTipLinks
    {
        get { return Application.platform != RuntimePlatform.IPhonePlayer; }
    }

    private const string Site = "https://zogaj.se/";
    private const string MemoGymIos = "https://apps.apple.com/se/app/zogaj-memo-gym/id392544473";
    private const string MemoGymAndroid = "https://play.google.com/store/apps/details?id=se.zogaj.memogym";
    private const string CaptainIos = "https://apps.apple.com/se/app/captain-zogajs-challenge/id6822024941";
    private const string CaptainAndroid = "https://play.google.com/store/apps/details?id=se.zogaj.extremechallenge";
    private const string CaptainWeb = "https://zogaj.se/captainzogaj/";
    private const string MemotopiaSite = "https://www.memotopia.com/";
    private const string Tedx = "https://www.youtube.com/watch?v=9ebJlcZMx3c";
    private const string Kofi = "https://ko-fi.com/captainzogaj";
    private const string BuyMeACoffee = "https://buymeacoffee.com/captainzogaj";

    private static GameObject overlay;
    private static Font font;

    public static void Open(MenuNavigation menu)
    {
        if (menu == null || menu.sideMenu == null) return;
        if (overlay != null) { overlay.SetActive(true); overlay.transform.SetAsLastSibling(); return; }
#if UNITY_2022_2_OR_NEWER
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        var canvas = menu.sideMenu.GetComponentInParent<Canvas>().rootCanvas;
        overlay = new GameObject("MeetTheCreator", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        Stretch((RectTransform)overlay.transform);
        overlay.GetComponent<Image>().color = Color.white;

        var close = Button(overlay.transform, "Close", Close, 72);
        var cr = (RectTransform)close.transform;
        cr.anchorMin = new Vector2(.2f, .03f); cr.anchorMax = new Vector2(.8f, .11f); cr.offsetMin = cr.offsetMax = Vector2.zero;

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(overlay.transform, false);
        var sr = (RectTransform)scrollGo.transform;
        sr.anchorMin = new Vector2(.06f, .13f); sr.anchorMax = new Vector2(.94f, .97f); sr.offsetMin = sr.offsetMax = Vector2.zero;
        scrollGo.GetComponent<Image>().color = Color.clear;
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(sr, false); Stretch((RectTransform)viewport.transform);
        var body = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        body.transform.SetParent(viewport.transform, false);
        var content = (RectTransform)body.transform;
        content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
        var layout = body.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 18; layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandHeight = false; layout.childAlignment = TextAnchor.UpperCenter;
        layout.padding = new RectOffset(12, 12, 24, 40);
        body.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = (RectTransform)viewport.transform; scroll.content = content;
        scroll.horizontal = false; scroll.scrollSensitivity = 35;

        Build(content);
    }

    public static void Close()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    private static void Build(RectTransform content)
    {
        Label(content, "Meet the creator", 56, 80, TextAnchor.MiddleCenter, true);

        var sprite = Resources.Load<Sprite>("Creator/idriz-creator");
        if (sprite != null)
        {
            var portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            portrait.transform.SetParent(content, false);
            var image = portrait.GetComponent<Image>();
            image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false;
            var element = portrait.GetComponent<LayoutElement>();
            element.preferredHeight = 330; element.preferredWidth = 282;
        }

        Label(content, "This game is free, courtesy of the creator.\nCreated by Idriz Zogaj\nZogaj of Sweden AB", 32, 150, TextAnchor.MiddleCenter, false);
        Button(content, "Meet Idriz at Zogaj.se", () => UrlOpener.Open(Site), 84);

        Button(content, "TEDx: How to become a memory master", () => UrlOpener.Open(Tedx), 84);

        Label(content, "More from Zogaj", 40, 64, TextAnchor.MiddleCenter, true);
        // Each platform only links to its own store.
        bool ios = Application.platform == RuntimePlatform.IPhonePlayer;
        Button(content, "Captain Zogaj\u2019s Challenge \u00b7 " + (ios ? "App Store" : "Google Play"),
            () => UrlOpener.Open(ios ? CaptainIos : CaptainAndroid), 84);
        Label(content, "A free arcade game. No ads.", 26, 44, TextAnchor.MiddleCenter, false);
        Button(content, "Zogaj Memo Gym \u00b7 " + (ios ? "App Store" : "Google Play"),
            () => UrlOpener.Open(ios ? MemoGymIos : MemoGymAndroid), 84);
        Label(content, "Give your memory a workout.", 26, 44, TextAnchor.MiddleCenter, false);
        Button(content, "Play Captain Zogaj in your browser", () => UrlOpener.Open(CaptainWeb), 84);
        Button(content, "memotopia.com", () => UrlOpener.Open(MemotopiaSite), 84);
        if (ShowTipLinks)
        {
            Label(content,
                "My TEDx talk has over 10 million views. If I had a dollar for every view, I'd have my own coffee plantation " +
                "and invite you all over for coffee. For now, I'll happily settle for you treating me to a cup!",
                28, 190, TextAnchor.MiddleCenter, false);
            Label(content, "Enjoyed the app? Sharing it is a lovely way to support my work. Thank you for playing!", 26, 100, TextAnchor.MiddleCenter, false);
            Button(content, "Ko-fi", () => UrlOpener.Open(Kofi), 84);
            Button(content, "Buy Me a Coffee", () => UrlOpener.Open(BuyMeACoffee), 84);
        }
        else
        {
            Label(content, "Enjoyed the app? Sharing it with a friend is a lovely way to support my work. Thank you for playing!", 28, 120, TextAnchor.MiddleCenter, false);
        }
    }

    private static Text Label(Transform parent, string value, int size, float height, TextAnchor anchor, bool bold)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font; t.text = value; t.fontSize = size; t.color = new Color32(43, 43, 58, 255);
        t.alignment = anchor; t.raycastTarget = false; t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        go.GetComponent<LayoutElement>().preferredHeight = height;
        return t;
    }

    private static Button Button(Transform parent, string value, UnityEngine.Events.UnityAction action, float height)
    {
        var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color32(238, 234, 255, 255);
        go.GetComponent<LayoutElement>().preferredHeight = height;
        var b = go.GetComponent<Button>(); b.onClick.AddListener(action);
        var t = Label(go.transform, value, 32, height, TextAnchor.MiddleCenter, false);
        Stretch(t.rectTransform);
        t.rectTransform.offsetMin = new Vector2(18, 4); t.rectTransform.offsetMax = new Vector2(-18, -4);
        t.resizeTextForBestFit = true; t.resizeTextMinSize = 24; t.resizeTextMaxSize = 32;
        return b;
    }

    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
    }
}

/// <summary>Shows the creator page now and then to users who have not paid; never to paying users.</summary>
public static class CreatorPromo
{
    private const string FirstKey = "creator.promo.first";
    private const string LastKey = "creator.promo.last";
    private const double FirstDelayHours = 24, RepeatHours = 72;
    private static bool shownThisRun;

    /// <summary>Pure rule, so it can be checked without a scene.</summary>
    public static bool ShouldShow(bool free, bool alreadyShownThisRun, double hoursSinceFirst, double hoursSinceLast)
    {
        if (!free || alreadyShownThisRun) return false;
        return hoursSinceFirst >= FirstDelayHours && hoursSinceLast >= RepeatHours;
    }

    public static void MaybeShow(MenuNavigation menu)
    {
        if (menu == null || UserConstants.s_user == null) return;
        bool free = UserConstants.s_user.paymentStatus == PaymentStatus.s_none;
        long now = DateTime.UtcNow.Ticks;
        long first = ReadTicks(FirstKey), last = ReadTicks(LastKey);
        if (first == 0) { first = now; WriteTicks(FirstKey, now); }
        if (!ShouldShow(free, shownThisRun, Hours(now, first), last == 0 ? double.MaxValue : Hours(now, last))) return;
        menu.StartCoroutine(ShowLater(menu));
    }

    private static IEnumerator ShowLater(MenuNavigation menu)
    {
        yield return new WaitForSeconds(2.5f);
        if (menu == null || shownThisRun) yield break;
        shownThisRun = true;
        WriteTicks(LastKey, DateTime.UtcNow.Ticks);
        CreatorPage.Open(menu);
    }

    private static double Hours(long now, long then) { return new TimeSpan(now - then).TotalHours; }

    private static long ReadTicks(string key)
    {
        long value;
        return long.TryParse(PlayerPrefs.GetString(key, "0"), out value) ? value : 0;
    }

    private static void WriteTicks(string key, long value)
    {
        PlayerPrefs.SetString(key, value.ToString()); PlayerPrefs.Save();
    }
}
