using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopupView : MonoBehaviour
{
    VerticalLayoutGroup layoutGroup;

    public Button closeButton;
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    [SerializeField]
    private GameObject firstGroupStars;
    [SerializeField]
    private GameObject secondGroupStars;

    public void Bind(Achievement achievement, List<Achievement> catalog)
    {
        icon.sprite = achievement.icon;
        title.text = AchievementsView.DisplayTitle(achievement, achievement.constantString);
        description.text = achievement.description;
        title.enableWordWrapping = true; title.enableAutoSizing = true; title.fontSizeMin = 22; title.fontSizeMax = 36;
        description.enableWordWrapping = true; description.enableAutoSizing = true; description.fontSizeMin = 18; description.fontSizeMax = 30;
        description.color = new Color(0.3f, 0.3f, 0.3f);
        ConfigureLayout(gameObject, icon, title, description, true);
        BindProgress(gameObject, catalog);
    }

    public static void BindProgress(GameObject panel, List<Achievement> catalog)
    {
        var known = new HashSet<string>();
        if (catalog != null)
            foreach (var entry in catalog)
                if (entry != null && !string.IsNullOrEmpty(entry.constantString)) known.Add(entry.constantString);
        int unlocked = 0;
        foreach (string key in GameManager.completedAchievements) if (known.Contains(key)) unlocked++;
        foreach (var label in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
            if (label.gameObject.name == "Progress (TMP)") label.text = unlocked + " / " + known.Count + " unlocked";
        foreach (var slider in panel.GetComponentsInChildren<Slider>(true))
        {
            slider.minValue = 0; slider.maxValue = Mathf.Max(1, known.Count); slider.value = unlocked;
            slider.interactable = false;
        }
    }

    public static void ConfigureLayout(GameObject panel, Image badge, TextMeshProUGUI heading, TextMeshProUGUI details, bool earned)
    {
        foreach (var layout in panel.GetComponentsInChildren<LayoutGroup>(true)) layout.enabled = false;
        var container = badge.transform.parent as RectTransform;
        Place(container, new Vector2(.075f, .12f), new Vector2(.925f, .88f));
        Place(badge.rectTransform, new Vector2(.20f, .47f), new Vector2(.80f, .80f));
        badge.preserveAspect = true;
        var textPanel = heading.transform.parent as RectTransform;
        Place(textPanel, new Vector2(.08f, .27f), new Vector2(.92f, .47f));
        Place(heading.rectTransform, new Vector2(0, .60f), Vector2.one);
        Place(details.rectTransform, Vector2.zero, new Vector2(1, .56f));
        heading.alignment = TextAlignmentOptions.Center;
        details.alignment = TextAlignmentOptions.Top;
        var progress = container.Find("ProgressPanel") as RectTransform;
        if (progress != null)
        {
            progress.gameObject.SetActive(true);
            Place(progress, new Vector2(.15f, earned ? .04f : .12f), new Vector2(.85f, .23f));
            foreach (var slider in progress.GetComponentsInChildren<Slider>(true))
            {
                slider.gameObject.SetActive(true);
                Place(slider.GetComponent<RectTransform>(), new Vector2(.05f, .85f), new Vector2(.95f, .9f));
            }
            foreach (var label in progress.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (label.gameObject.name == "Progress (TMP)")
                {
                    label.gameObject.SetActive(true);
                    Place(label.rectTransform, new Vector2(0, .57f), new Vector2(1, .82f));
                    label.fontSize = 24; label.color = new Color(.3f, .3f, .3f);
                }
                else if (label.text == "Achievement unlocked!")
                {
                    label.transform.SetParent(container, false);
                    Place(label.rectTransform, new Vector2(.1f, .84f), new Vector2(.9f, .92f));
                    label.fontSize = 30; label.fontStyle = FontStyles.Bold;
                }
            }
            foreach (var button in progress.GetComponentsInChildren<Button>(true))
                Place(button.GetComponent<RectTransform>(), new Vector2(.22f, .16f), new Vector2(.78f, .50f));
        }
        var close = container.Find("CloseButton") as RectTransform;
        if (close != null)
        {
            close.gameObject.SetActive(true);
            Place(close, new Vector2(.80f, .88f), new Vector2(.87f, .925f));
        }
        foreach (Transform child in container)
            if (child.name.StartsWith("Stars")) child.gameObject.SetActive(false);
    }

    private static void Place(RectTransform rect, Vector2 minimum, Vector2 maximum)
    {
        rect.anchorMin = minimum; rect.anchorMax = maximum;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Used to close/destroy the popup panel that is shown in the end of the level
    /// </summary>
    public void ClosePanel()
    {
        Destroy(gameObject);
    }
}
