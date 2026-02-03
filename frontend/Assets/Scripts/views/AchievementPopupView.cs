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

    private void Start()
    {
        layoutGroup = GetComponentInChildren<VerticalLayoutGroup>();
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            layoutGroup.spacing = 70;
            if(firstGroupStars !=null && secondGroupStars)
            {
                firstGroupStars.transform.localPosition = new Vector3(0, 223.59f, 0);
                secondGroupStars.transform.localPosition = new Vector3(0, 61f, 0);
            }
        }
        else
        {
            layoutGroup.spacing = 130;
            if (firstGroupStars != null && secondGroupStars)
            {
                firstGroupStars.transform.localPosition = new Vector3(0f, 393.03f, 0);
                secondGroupStars.transform.localPosition = new Vector3(0f, 159.68f, 0);

            }
        }
    }

    /// <summary>
    /// Used to close/destroy the popup panel that is shown in the end of the level
    /// </summary>
    public void ClosePanel()
    {
        Destroy(gameObject);
    }
}
