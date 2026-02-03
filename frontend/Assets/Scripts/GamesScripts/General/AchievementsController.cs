using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine;

public class AchievementsController : MonoBehaviour
{
    private UserStatisticsAPIController userStatisticsAPIController;
    private AchievementsAPIController achievementsAPIController;

    private List<Achievement> allAchievementsList = new List<Achievement>();
    private List<Achievement> achievementsToShowList = new List<Achievement>();

    private GameObject achievementsPanelPrefab;
    private GameObject winPanel;
    private string score;
    private string gameScene;
    private string game;
    private int nrStars;

    private bool isChallange;

    int achievementCount = 0;

    private void Start()
    {
        userStatisticsAPIController = gameObject.AddComponent<UserStatisticsAPIController>();
        achievementsAPIController = gameObject.AddComponent<AchievementsAPIController>();
    }

    private void ResetAll()
    {
        isChallange = false;
        allAchievementsList.Clear();
        achievementsToShowList.Clear();
        achievementCount = 0;

        achievementsPanelPrefab = null;
        winPanel = null;
        score = "";
        gameScene = "";
        game = "";
        nrStars = 0;
    }

    public void LevelCompletedAchievements(string thisScore, int thisNrStars,
        GameObject thisWinPanel, string thisGameScene, string thisGame,
        GameObject thisAchievementsPanelPrefab, List<Achievement> thisAchievementsList)
    {
        isChallange = false;
        ResetAll();

        allAchievementsList = thisAchievementsList;
        achievementsPanelPrefab = thisAchievementsPanelPrefab;
        winPanel = thisWinPanel;
        score = thisScore;
        gameScene = thisGameScene;
        game = thisGame;
        nrStars = thisNrStars;

        StartCoroutine(FirstGlobal());
        InTheBox();
        PairingUp();
        WithTheFlash();

        ChallengeThem();
        LookingAround();
        ThinkOutSideTheBox();
        TotalRecall();
        TrueMatchMaker();
    }

    public void ChallangecompletedAchievements(GameObject challangePanel, string thisGame,
        GameObject thisAchievementsPanelPrefab, List<Achievement> thisAchievementsList)
    {
        ResetAll();
        isChallange = true;
        winPanel = challangePanel;
        game = thisGame;
        achievementsPanelPrefab = thisAchievementsPanelPrefab;
        allAchievementsList = thisAchievementsList;

        WinTenChallanges();
        FirstChallengeGame();
    }

    private void AchievementsCheck()
    {
        achievementCount++;

        if (isChallange)
        {
            if (achievementCount >= 2)
            {
                if (achievementsToShowList.Count >= 1)
                    AchievementPopup();
                else
                    ChallangePopup();
            }
        }
        else
        {
            if (achievementCount >= 9)
            {
                if (achievementsToShowList.Count >= 1)
                    AchievementPopup();
                else
                    WinPopup();
            }
        }
    }

    #region Achievements

    private void FirstChallengeGame()
    {
        if (UserConstants.s_user.userStatistics.numberOfWinChallenges == 1)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "first-challenge" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("first-challenge");
                }
            }
        }
        AchievementsCheck();
    }

    private void WinTenChallanges()
    {
        if (UserConstants.s_user.userStatistics.numberOfWinChallenges == 10)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "win-ten-challenge" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("win-ten-challenge");
                }
            }
        }
        AchievementsCheck();
    }

    private void TrueMatchMaker()
    {
        if (game == StaticVar.s_gamePairs && LevelScript._selectedLevel == 20 &&
            GameLevelConstants.s_pairsLevels.Length == 19)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "pairs-all-levels" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("pairs-all-levels");
                }
            }
        }
        AchievementsCheck();
    }

    private void TotalRecall()
    {
        if (game == StaticVar.s_gameFlash && LevelScript._selectedLevel == 20 &&
            GameLevelConstants.s_flashLevels.Length == 19)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "flash-all-levels" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("flash-all-levels");
                }
            }
        }
        AchievementsCheck();
    }

    private void ThinkOutSideTheBox()
    {
        if (game == StaticVar.s_gameBoxes && LevelScript._selectedLevel == 20 &&
            GameLevelConstants.s_boxesLevels.Length == 19)
        {

            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "boxes-all-levels" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("boxes-all-levels");
                }
            }
        }
        AchievementsCheck();
    }

    private void LookingAround()
    {
        if(LookingAroundBoxes() || LookingAroundPairs() || LookingAroundFlash())
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "looking-around" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("looking-around");
                }
            }
        }



        if (LevelScript._selectedLevel == 1 && GameLevelConstants.s_boxesLevels.Length >= 1
            && GameLevelConstants.s_pairsLevels.Length >= 1
            && GameLevelConstants.s_flashLevels.Length >= 1)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "looking-around" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("looking-around");
                }
            }
        }
        AchievementsCheck();
    }

    private void ChallengeThem()
    {
        if(UserConstants.s_user.userStatistics.xp >= 400)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "unlock-challenge" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("unlock-challenge");
                }
            }
        }
        AchievementsCheck();
    }

    private void InTheBox()
    {
        if (game == StaticVar.s_gameBoxes && LevelScript._selectedLevel == 1 &&
            GameLevelConstants.s_boxesLevels.Length == 0)
        {

            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "in-the-box" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("in-the-box");
                }
            }
        }
        AchievementsCheck();
    }

    private void PairingUp()
    {
        if (game == StaticVar.s_gamePairs && LevelScript._selectedLevel == 1 &&
            GameLevelConstants.s_pairsLevels.Length == 0)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "pairing-up" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("pairing-up");
                }
            }
        }
        AchievementsCheck();
    }

    private void WithTheFlash()
    {
        if (game == StaticVar.s_gameFlash && LevelScript._selectedLevel == 1 &&
            GameLevelConstants.s_flashLevels.Length == 0)
        {
            foreach (var item in allAchievementsList)
            {
                if (item.constantString == "with-the-flash" &&
                    !GameManager.completedAchievements.Contains(item.constantString))
                {
                    achievementsToShowList.Add(item);
                    OnAchievementCompleted("with-the-flash");
                }
            }
        }
        AchievementsCheck();
    }

    private IEnumerator FirstGlobal()
    {
        bool checkedScore = false;
        userStatisticsAPIController.GetGlobalScore(
             (OnSuccess) =>
             {
                 if (OnSuccess.statistics[0].user.ID == UserConstants.s_user.ID)
                 {
                     foreach (var item in allAchievementsList)
                     {
                         if (item.constantString == "first-global-score" &&
                            !GameManager.completedAchievements.Contains(item.constantString))
                         {
                             achievementsToShowList.Add(item);
                             OnAchievementCompleted("first-global-score");
                         }
                     }
                 }
                 checkedScore = true;
             },
             (OnFailure) =>
             {
                 print("fail");
                 checkedScore = true;
             }
         );
        yield return new WaitUntil(() => checkedScore == true);
        AchievementsCheck();
    }

    #endregion

    #region Popup Controllers

    private void AchievementPopup()
    {
        GameObject achievementsPanel = Instantiate(achievementsPanelPrefab) as GameObject;
        achievementsPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);
        achievementsPanel.transform.SetAsLastSibling();

        AchievementPopupView achievementPopupView = achievementsPanel.GetComponent<AchievementPopupView>();

        achievementPopupView.closeButton.onClick.AddListener(() =>
        {
            NextPopup(achievementsPanel);
        });

        achievementPopupView.icon.sprite = achievementsToShowList[0].icon;
        achievementPopupView.title.text = achievementsToShowList[0].title;
        achievementPopupView.description.text = achievementsToShowList[0].description;
    }

    private void NextPopup(GameObject achievementsPanel)
    {
        Destroy(achievementsPanel);

        if (achievementsToShowList.Count > 0)
            achievementsToShowList.RemoveAt(0);

        if (achievementsToShowList.Count > 0)
            AchievementPopup();
        else
        {
            if(!isChallange)
                WinPopup();
            else
                ChallangePopup();
        }
    }

    private void WinPopup()
    {
        if (game == StaticVar.s_gameBoxes)
        {
            gameObject.GetComponent<BoxesManager>().ActivateWinPanel(
                score, nrStars, winPanel, gameScene, game);
        }
        else if (game == StaticVar.s_gameFlash)
        {
            gameObject.GetComponent<FlashManager>().ActivateWinPanel(
                score, nrStars, winPanel, gameScene, game);
        }
        else if (game == StaticVar.s_gamePairs)
        {
            gameObject.GetComponent<PairsManager>().ActivateWinPanel(
                score, nrStars, winPanel, gameScene, game);
        }
    }

    private void ChallangePopup()
    {
        if (game == StaticVar.s_gameBoxes)
        {
            gameObject.GetComponent<BoxesManager>().ActivateChallengePanel(winPanel);
        }
        else if (game == StaticVar.s_gameFlash)
        {
            gameObject.GetComponent<FlashManager>().ActivateChallengePanel(winPanel);
        }
        else if (game == StaticVar.s_gamePairs)
        {
            gameObject.GetComponent<PairsManager>().ActivateChallengePanel(winPanel);
        }
    }

    #endregion

    #region Add/Update Achievement

    private void OnAchievementCompleted(string achievementName)
    {
        try
        {
            achievementsAPIController.AddAchievements(
                new string[] { achievementName },
                (OnSuccess) =>
                {
                    print("success");
                    UpdatetcompletedAchievements();
                },
                (OnFailure) =>
                {
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void UpdatetcompletedAchievements()
    {
        try
        {
            achievementsAPIController.GetAchievements(
                (OnSuccess) =>
                {
                    GameManager.completedAchievements = OnSuccess.achievements.ToList();
                    print("success");
                },
                (OnFailure) =>
                {
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    #endregion

    #region ExtrasAchievements

    private bool LookingAroundBoxes()
    {
        return game == StaticVar.s_gameBoxes && LevelScript._selectedLevel == 1
            && GameLevelConstants.s_boxesLevels.Length == 0
            && GameLevelConstants.s_pairsLevels.Length >= 1
            && GameLevelConstants.s_flashLevels.Length >= 1;
    }

    private bool LookingAroundPairs()
    {
        return game == StaticVar.s_gamePairs && LevelScript._selectedLevel == 1
            && GameLevelConstants.s_boxesLevels.Length >= 1
            && GameLevelConstants.s_pairsLevels.Length == 0
            && GameLevelConstants.s_flashLevels.Length >= 1;
    }

    private bool LookingAroundFlash()
    {
        return game == StaticVar.s_gameFlash && LevelScript._selectedLevel == 1
            && GameLevelConstants.s_boxesLevels.Length >= 1
            && GameLevelConstants.s_pairsLevels.Length >= 1
            && GameLevelConstants.s_flashLevels.Length == 0;
    }

    #endregion
}
