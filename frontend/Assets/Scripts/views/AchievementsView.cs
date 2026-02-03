using System.Collections;
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
    private List<GameObject> achievementsList = new List<GameObject>();

    public Button button;

    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public Image popupIcon;
    public TextMeshProUGUI popupTitle;
    public TextMeshProUGUI popupDescription;

    private void Start()
    {
        foreach (var achievement in achievements)
        {
            icon.sprite = achievement.icon;
            title.text = achievement.title;
            description.text = achievement.description;

            GameObject go = Instantiate(achievementTemplate, content.transform);
            go.name = achievement.constantString;
            achievementsList.Add(go);
            go.GetComponentInChildren<Button>().onClick.AddListener(() => OnAchievementClick(achievement));
        }
        achievementTemplate.SetActive(false);

        foreach (string item in GameManager.completedAchievements)
        {
            ActivateAchivement(item);
        }
    }

    public void OnAchievementClick(Achievement achievement)
    {
        achievementPopupView.SetActive(true);
        popupIcon.sprite = achievement.icon;
        popupTitle.text = achievement.title;
        popupDescription.text = achievement.description;
    }

    public void OnClosePopupClick()
    {
        achievementPopupView.SetActive(false);
    }

    private void ActivateAchivement(string s)
    {
        foreach (var item in achievementsList)
        {
            if(s == item.name)
            {
                GameObject g = item.transform.Find("VisibilityPanel").gameObject;
                Image image = g.GetComponent<Image>();
                var tempColor = image.color;
                tempColor.a = 0f;
                image.color = tempColor;
            }
        }
    }
}
