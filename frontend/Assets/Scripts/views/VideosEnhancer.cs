using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds the "how to play" videos to the Videos screen. They live on the YouTube channel youtube.com/idrizzogaj and
/// open in the YouTube app or the browser, so nothing has to be hosted or downloaded. The four videos that were
/// already there keep playing inside the app. The rows are cloned from the last existing row, in code, so the scene
/// does not change.
/// </summary>
public static class VideosEnhancer
{
    private static readonly string[][] Extra =
    {
        new[] { "How to play Pairs", "https://www.youtube.com/watch?v=HOgHXY4ge5s" },
        new[] { "How to play Boxes", "https://www.youtube.com/watch?v=PbAWYA0XV8Q" },
        new[] { "How to play Flash", "https://www.youtube.com/watch?v=JQCL0fgJ6s8" },
        new[] { "Challenge your friends", "https://www.youtube.com/watch?v=G9-f1MTSD3g" },
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

        float y = template.anchoredPosition.y;
        for (int i = 0; i < Extra.Length; i++)
        {
            string title = Extra[i][0], url = Extra[i][1];
            var clone = (RectTransform)Object.Instantiate(template.gameObject, content, false).transform;
            clone.name = "ExtraVideo" + i;
            clone.anchoredPosition = new Vector2(template.anchoredPosition.x, y - RowPitch * (i + 1));
            var text = clone.GetComponentInChildren<Text>(true);
            if (text != null) text.text = title;
            var button = clone.GetComponent<Button>();
            if (button != null)
            {
                button.onClick = new Button.ButtonClickedEvent(); // drop the copied in-app video action
                button.onClick.AddListener(() => Application.OpenURL(url));
                button.interactable = true;
            }
        }
        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + RowPitch * Extra.Length);
    }
}
