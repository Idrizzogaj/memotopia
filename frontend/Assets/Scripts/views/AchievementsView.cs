using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementsView : MonoBehaviour
{
    public GameObject content;
    public GameObject achievementTemplate;
    public GameObject achievementPopupView;
    public List<Achievement> achievements;
    public Button button;
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Image popupIcon;
    public TextMeshProUGUI popupTitle;
    public TextMeshProUGUI popupDescription;

    private static Material lockedMaterial;
    public static void SetBadgeState(Image badge, bool unlocked)
    {
        if (!unlocked && lockedMaterial == null)
            lockedMaterial = new Material(Resources.Load<Shader>("Shaders/UIGrayscale"));
        badge.material = unlocked ? null : lockedMaterial;
        badge.color = Color.white;
    }

    private readonly List<GameObject> rows = new List<GameObject>();
    private readonly List<string> detailKeys = new List<string>();
    private string selectedKey;
    private Achievement selectedAchievement;

    public static string DisplayTitle(Achievement achievement, string key)
    {
        if (achievement != null && !string.IsNullOrEmpty(achievement.title)) return achievement.title;
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase((key ?? "Achievement").Replace("-", " ").Replace("_", " "));
    }

    private void OnEnable()
    {
        GameManager.AchievementsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameManager.AchievementsChanged -= Refresh;
        if (achievementPopupView != null) achievementPopupView.SetActive(false);
        selectedKey = null;
    }

    public void Refresh()
    {
        if (achievementTemplate == null || content == null) return;
        foreach (var row in rows) { row.SetActive(false); if (Application.isPlaying) Destroy(row); else DestroyImmediate(row); }
        rows.Clear();
        detailKeys.Clear();
        var known = new HashSet<string>();
        var completed = new HashSet<string>(GameManager.completedAchievements);
        if (achievements != null)
            foreach (var achievement in achievements)
                if (achievement != null && !string.IsNullOrEmpty(achievement.constantString) && known.Add(achievement.constantString))
                    AddRow(achievement, achievement.constantString, completed.Contains(achievement.constantString));
        foreach (var key in completed)
            if (known.Add(key)) AddRow(null, key, true);
        achievementTemplate.SetActive(false);
        if (selectedKey != null && achievementPopupView.activeSelf) ShowDetails(selectedAchievement, selectedKey);
    }

    private void AddRow(Achievement achievement, string key, bool unlocked)
    {
        icon.sprite = achievement == null ? null : achievement.icon;
        icon.enabled = icon.sprite != null;
        SetBadgeState(icon, unlocked);
        title.text = DisplayTitle(achievement, key);
        title.enableAutoSizing = true; title.fontSizeMin = 18; title.fontSizeMax = 23;
        title.enableWordWrapping = true;
        FitRowText(title, 64);
        FitRowText(description, 80);
        description.text = (achievement == null ? "Achievement earned." : achievement.description);
        var row = Instantiate(achievementTemplate, content.transform);
        row.name = key;
        row.SetActive(true);
        var cover = row.transform.Find("VisibilityPanel");
        // Grey only the badge, keeping requirements readable and the row tappable.
        if (cover != null) cover.gameObject.SetActive(false);
        var rowButton = row.GetComponentInChildren<Button>(true);
        if (rowButton != null) rowButton.onClick.AddListener(() => ShowDetails(achievement, key));
        rows.Add(row);
        detailKeys.Add(key);
    }

    private static void FitRowText(TextMeshProUGUI label, float height)
    {
        var parent = label.transform.parent as RectTransform;
        parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        var rect = label.rectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(4, 0); rect.offsetMax = new Vector2(-4, 0);
        label.alignment = TextAlignmentOptions.Top;
        label.enableWordWrapping = true;
    }

    public void OnAchievementClick(Achievement achievement)
    {
        if (achievement != null) ShowDetails(achievement, achievement.constantString);
    }

    private void ShowDetails(Achievement achievement, string key)
    {
        selectedAchievement = achievement;
        selectedKey = key;
        achievementPopupView.SetActive(true);
        var gestures = achievementPopupView.GetComponent<AchievementDetailGestures>() ?? achievementPopupView.AddComponent<AchievementDetailGestures>();
        gestures.view = this;
        gestures.card = popupIcon.transform.parent as RectTransform;
        AchievementPopupView.ConfigureLayout(achievementPopupView, popupIcon, popupTitle, popupDescription, false);
        var progress = popupIcon.transform.parent.Find("ProgressPanel");
        if (progress != null) progress.gameObject.SetActive(false);
        popupIcon.sprite = achievement == null ? null : achievement.icon;
        popupIcon.enabled = popupIcon.sprite != null;
        SetBadgeState(popupIcon, GameManager.completedAchievements.Contains(key));
        popupTitle.enableWordWrapping = true;
        popupTitle.enableAutoSizing = true;
        popupTitle.fontSizeMin = 22;
        popupTitle.fontSizeMax = 36;
        popupDescription.enableWordWrapping = true;
        popupDescription.enableAutoSizing = true;
        popupDescription.fontSizeMin = 18;
        popupDescription.fontSizeMax = 30;
        popupDescription.color = new Color(0.3f, 0.3f, 0.3f);
        popupTitle.text = DisplayTitle(achievement, key);
        popupDescription.text = (achievement == null ? "Achievement earned." : achievement.description);
    }

    public void OnClosePopupClick() { selectedKey = null; achievementPopupView.SetActive(false); }

    public void MoveDetails(int direction)
    {
        if (selectedKey == null || detailKeys.Count < 2) return;
        int index = detailKeys.IndexOf(selectedKey);
        if (index < 0) return;
        string key = detailKeys[(index + direction + detailKeys.Count) % detailKeys.Count];
        ShowDetails(achievements == null ? null : achievements.Find(a => a != null && a.constantString == key), key);
    }
}
