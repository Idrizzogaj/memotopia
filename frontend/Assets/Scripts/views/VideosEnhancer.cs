using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Streams the game guides in the same player as the memory lessons.
/// </summary>
public static class VideosEnhancer
{
    private static readonly string[][] Extra =
    {
        new[] { "How to play Pairs", "5_HowToPlayPairs.mp4" },
        new[] { "How to play Boxes", "6_HowToPlayBoxes.mp4" },
        new[] { "How to play Flash", "7_HowToPlayFlash.mp4" },
        new[] { "Challenge your friends", "8_ChallengeFriends.mp4" },
    };

    private const float RowPitch = 200f; // the scene spaces its rows 200 units apart

    public static void Ensure(MenuNavigation menu)
    {
        if (menu == null || menu.videoScreen == null) return;
        RectTransform template = null;
        foreach (var rect in menu.videoScreen.GetComponentsInChildren<RectTransform>(true))
            if (rect.name == "MemoryPalaceVideo") { template = rect; break; }
        if (template == null) return;
        var content = template.parent as RectTransform;
        if (content == null || content.Find("ExtraVideo0") != null) return;

        var intro = content.Find("IntroVideo") as RectTransform;
        if (intro == null) return;
        foreach (RectTransform row in content)
            if (row != intro) row.anchoredPosition -= new Vector2(0f, RowPitch * Extra.Length);

        for (int i = 0; i < Extra.Length; i++)
        {
            string title = Extra[i][0], url = "https://www.memotopia.com/video/" + Extra[i][1];
            // Clone the free row so guides never inherit grey premium styling.
            var clone = (RectTransform)Object.Instantiate(intro.gameObject, content, false).transform;
            clone.name = "ExtraVideo" + i;
            clone.SetSiblingIndex(intro.GetSiblingIndex() + i + 1);
            clone.anchoredPosition = intro.anchoredPosition - new Vector2(0f, RowPitch * (i + 1));
            var text = clone.GetComponentInChildren<Text>(true);
            if (text != null) text.text = title;
            var button = clone.GetComponent<Button>();
            if (button != null)
            {
                button.onClick = new Button.ButtonClickedEvent(); // drop the copied in-app video action
                button.onClick.AddListener(() => menu.PlayTutorialVideo(url));
                button.interactable = true;
            }
        }
        // The final row must remain tappable without holding an elastic overscroll.
        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            // ContentSizeFitter owns the height in the shipped scene.
            layout.padding.bottom = Mathf.Max(layout.padding.bottom, 180);
            LayoutRebuilder.MarkLayoutForRebuild(content);
        }
        else
        {
            float lastBottom = -template.anchoredPosition.y + template.rect.height * template.pivot.y;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lastBottom + 180f);
        }
    }
}
