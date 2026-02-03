using System;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatisticsView : MonoBehaviour
{
    public UserStatisticsAPIController userStatisticsAPIController;

    public GameObject[] peoples;

    [SerializeField]
    private GameObject statisticsGameObject;
    [SerializeField]
    private List<GameObject> circleImages = new List<GameObject>();
    [SerializeField]
    private GameObject achievementsButton;
    [SerializeField]
    private GameObject globalScoreButton;
    [SerializeField]
    private GameObject challengeStatsPanel;
    [SerializeField]
    private GameObject lastTwoPanels;
    [SerializeField]
    private GameObject starsContainer;
    [SerializeField]
    private GameObject levelCircle;
    [SerializeField]
    private GameObject xpCircle;
    [SerializeField]
    private TextMeshProUGUI timePlayed;
    [SerializeField]
    private TextMeshProUGUI starsPerGame;

    void Start()
    {
        userStatisticsAPIController = gameObject.AddComponent<UserStatisticsAPIController>();
        userStatisticsAPIController.GetGlobalScore(
            (OnSuccess) =>
            {
                bool inFirstTen = false;
                int myUserOrderNr = 0;

                for (int i = 0; i < OnSuccess.statistics.Length; i++)
                {
                    if (i < 10 && OnSuccess.statistics[i].user.ID == UserConstants.s_user.ID)
                        inFirstTen = true;

                    if (i >= 10 && OnSuccess.statistics[i].user.ID == UserConstants.s_user.ID)
                        myUserOrderNr = i;
                }

                for (int i = 0; i < peoples.Length; i++)
                {
                    peoples[i].SetActive(true);
                    setupUserRaw(peoples[i], OnSuccess.statistics, i);
                }

                if (!inFirstTen)
                {
                    setupUserRaw(peoples[9], OnSuccess.statistics, myUserOrderNr);
                }
            },
            (OnFailure) =>
            {
                print("fail");
            }
        );

        string compleatedAchievements = GameManager.completedAchievements.Count.ToString();

        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            statisticsGameObject.GetComponent<VerticalLayoutGroup>().padding.top = 60;
            statisticsGameObject.GetComponent<VerticalLayoutGroup>().padding.top = 40;
            statisticsGameObject.GetComponent<VerticalLayoutGroup>().spacing = 20;
            foreach (var item in circleImages)
            {
                item.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
            }
            levelCircle.GetComponent<VerticalLayoutGroup>().padding.top = 50;
            levelCircle.GetComponent<VerticalLayoutGroup>().padding.bottom = 50;
            xpCircle.GetComponent<VerticalLayoutGroup>().padding.top = 50;
            xpCircle.GetComponent<VerticalLayoutGroup>().padding.bottom = 50;
            //achievementsButton.GetComponent<LayoutElement>().preferredHeight = 90;
            challengeStatsPanel.GetComponent<LayoutElement>().preferredHeight = 150;
            lastTwoPanels.GetComponent<LayoutElement>().preferredHeight = 450;
        }
        else
        {

            statisticsGameObject.GetComponent<VerticalLayoutGroup>().padding.top = 100;
            statisticsGameObject.GetComponent<VerticalLayoutGroup>().padding.top = 100;
            statisticsGameObject.GetComponent<VerticalLayoutGroup>().spacing = 70;
            foreach (var item in circleImages)
            {
                item.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 350);
            }
            levelCircle.GetComponent<VerticalLayoutGroup>().padding.top = 90;
            levelCircle.GetComponent<VerticalLayoutGroup>().padding.bottom = 90;
            xpCircle.GetComponent<VerticalLayoutGroup>().padding.top = 90;
            xpCircle.GetComponent<VerticalLayoutGroup>().padding.bottom = 90;
            //achievementsButton.GetComponent<LayoutElement>().preferredHeight = 100;
            challengeStatsPanel.GetComponent<LayoutElement>().preferredHeight = 300;
            lastTwoPanels.GetComponent<LayoutElement>().preferredHeight = 300;
        }
    }

    public void CallLevelsAvg()
    {
        InitStars();
        userStatisticsAPIController.GetLevelsAvg(
        (OnSuccess) =>
            {
                float decimalPart = OnSuccess.avg % 1;
                int starsAvg = (int)Math.Floor(OnSuccess.avg);
                if(decimalPart > 0.1)
                    FillStars(starsAvg, starsAvg+1);
                else
                    FillStars(starsAvg, 0);

                starsPerGame.text = OnSuccess.avg.ToString() + " / Game";
            },
            (OnFailure) =>
            {
                print("fail");
            }
        );
    }

    public void TimePlayed()
    {
        userStatisticsAPIController.GetTimePlayed(
            (OnSuccesss) =>
            {
                float gameTime = OnSuccesss.timePlayed;

                int seconds = (int)(gameTime % 60);
                int minutes = (int)(gameTime / 60) % 60;
                int hours = (int)(gameTime / 3600) % 24;
                string timeString = string.Format("{0:0}h {1:0}m {2:0}s", hours, minutes, seconds);
                timePlayed.text = timeString;
            },
            (OnFailuree) =>
            {
                print("fail");
            }
        );
    }

    private void FillStars(int starsNr, int halfStar)
    {
        for (int j = 0; j < starsNr; j++)
        {
            string starImage = "";

            if (j == 0)
                starImage = "GamesResources\\Levels\\Asset 15";
            else if (j == 1)
                starImage = "GamesResources\\Levels\\Asset 14";
            else if (j == 2)
                starImage = "GamesResources\\Levels\\Asset 13";
            starsContainer.transform.GetChild(j).GetComponent<Image>().sprite =
                Resources.Load<Sprite>(starImage) as Sprite;
        }
        if(halfStar > 0)
        {
            string halfStarImage = "";
            if (halfStar == 1)
                halfStarImage = "GamesResources\\Levels\\Asset 15-1";
            else if (halfStar == 2)
                halfStarImage = "GamesResources\\Levels\\Asset 14-1";
            else if (halfStar == 3)
                halfStarImage = "GamesResources\\Levels\\Asset 13-1";
            starsContainer.transform.GetChild(halfStar - 1).GetComponent<Image>().sprite =
                    Resources.Load<Sprite>(halfStarImage) as Sprite;
        }
    }

    private void InitStars()
    {
        for (int j = 0; j < 2; j++)
        {
            string starImage = "";

            if (j == 0)
                starImage = "GamesResources\\Levels\\Asset 12";
            else if (j == 1)
                starImage = "GamesResources\\Levels\\Asset 10";
            else if (j == 2)
                starImage = "GamesResources\\Levels\\Asset 11";
            starsContainer.transform.GetChild(j).GetComponent<Image>().sprite =
                Resources.Load<Sprite>(starImage) as Sprite;
        }
    }

    void setupUserRaw(GameObject people, UserStatistics[] statistics, int idx)
    {
        Text[] texts = people.GetComponentsInChildren<Text>();
        Image[] images = people.GetComponentsInChildren<Image>();

        texts[0].text = (idx + 1).ToString();
        texts[1].text = statistics[idx].user.username;
        texts[2].text = statistics[idx].xp + "xp";
        images[0].sprite = Resources.Load<Sprite>("Memotopia_UI/AccountAndSettings/" + statistics[idx].user.Avatar);

        if (statistics[idx].user.ID == UserConstants.s_user.ID)
        {
            texts[0].color = new Color(123f / 255f, 67f / 255f, 245f / 255f);
            texts[1].color = new Color(123f / 255f, 67f / 255f, 245f / 255f);
            texts[2].color = new Color(123f / 255f, 67f / 255f, 245f / 255f);
        }
    }
}
