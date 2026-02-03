using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class GamesScript : MonoBehaviour
{
    readonly System.Random _random = new System.Random();
    GameAPIController gameAPIController;
    ChallengeAPIController challengeAPIController;
    RestrictionsAPIController restrictionsAPIController;
    LoadingManager _loadingScreen;
    UserStatisticsAPIController userStatisticsAPIController;

    public DataController dataController;

    public void InitializeGameScript()
    {
        Debug.Log("AAA InitializeGameScript");
        gameAPIController = gameObject.AddComponent<GameAPIController>();
        challengeAPIController = gameObject.AddComponent<ChallengeAPIController>();
        userStatisticsAPIController = gameObject.AddComponent<UserStatisticsAPIController>();
        dataController = gameObject.AddComponent<DataController>();

    }

    public IEnumerator ShowRSG(GameObject rsg)
    {
        rsg.SetActive(true);
        yield return new WaitForSeconds(3);
        rsg.SetActive(false);
    }

    public void Shuffle(int[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = _random.Next(0, n);
            int q = array[r];
            array[r] = array[n];
            array[n] = q;
        }
    }

    public void Shuffle(string[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = _random.Next(0, n);
            string q = array[r];
            array[r] = array[n];
            array[n] = q;
        }
    }

    public int setStars(int score)
    {
        float threeStar = 75f / 100f * 180f;
        float twoStar = 50f / 100f * 180f;
        int star = 0;

        if (score >= threeStar)
        {
            star = 3;
        }
        else if (score >= twoStar)
        {
            star = 2;
        }
        else
        {
            star = 1;
        }

        return star;
    }

    #region Instatiate Gameobjects from prefabs

    public void MidGameQuit(GameObject midGameQuitPanel)
    {
        GameObject QuitPanel = Instantiate(midGameQuitPanel) as GameObject;

        QuitPanel.transform.GetChild(0).transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
            Destroy(QuitPanel);
        });

        Debug.Log("AAA s_isChallenge: " + ChallengeConstants.s_isChallenge);

        if (ChallengeConstants.s_isChallenge)
        {
            dataController.MenuLastPage = NavigationConstants.s_challengeNav;
            QuitPanel.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                initLoadingAndStart();
                ChallengeConstants.s_isChallenge = false;
                SceneManager.LoadScene(SceneName.s_gameMenu);
            });
        }
        else
        {
            QuitPanel.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                initLoadingAndStart();
                SceneManager.LoadScene(SceneName.s_levelsScene);
            });
        }

        QuitPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);
        QuitPanel.transform.SetAsLastSibling();
    }

    public void ActivateLosePanel(GameObject LosePanel, string gameScene)
    {
        GameObject LosePanelCopy = Instantiate(LosePanel) as GameObject;

        LosePanelCopy.transform.GetChild(0).transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.UnloadSceneAsync(gameScene);
            SceneManager.LoadScene(gameScene);
        });
        LosePanelCopy.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
        {
            initLoadingAndStart();
            SceneManager.LoadScene(SceneName.s_levelsScene);
        });

        LosePanelCopy.transform.SetParent(GameObject.Find("Canvas").transform, false);
        LosePanelCopy.transform.SetAsLastSibling();
    }

    public void ActivateWinPanel(string score, int nrStars, GameObject WinPanel, string gameScene, string game)
    {
        GameObject WinPanelCopy = Instantiate(WinPanel) as GameObject;

        WinPanelCopy.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "LEVEL " + LevelScript._selectedLevel;
        WinPanelCopy.transform.GetChild(0).transform.GetChild(10).transform.GetChild(0).GetComponent<Text>().text = score;

        WinPanelCopy.transform.GetChild(0).transform.GetChild(11).GetComponent<Button>().onClick.AddListener(() => {
            try
            {
                gameAPIController.GetLevels(game,
                    (OnSuccess) =>
                    {
                        var responseObject = OnSuccess;

                        if (responseObject.levels.Length > -1)
                        {
                            switch (game)
                            {
                                case "Pairs":
                                    GameLevelConstants.s_pairsLevels = responseObject.levels;
                                    break;
                                case "Boxes":
                                    GameLevelConstants.s_boxesLevels = responseObject.levels;
                                    break;
                                case "Flash":
                                    GameLevelConstants.s_flashLevels = responseObject.levels;
                                    break;
                            }
                        }

                        SceneManager.UnloadSceneAsync(gameScene);
                        SceneManager.LoadScene(gameScene);
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
        });
        WinPanelCopy.transform.GetChild(0).transform.GetChild(12).GetComponent<Button>().onClick.AddListener(() => {
            initLoadingAndStart();
            try
            {
                gameAPIController.GetLevels(game,
                    (OnSuccess) =>
                    {
                        var responseObject = OnSuccess;

                        if (responseObject.levels.Length > -1)
                        {
                            switch (game)
                            {
                                case "Pairs":
                                    GameLevelConstants.s_pairsLevels = responseObject.levels;
                                    break;
                                case "Boxes":
                                    GameLevelConstants.s_boxesLevels = responseObject.levels;
                                    break;
                                case "Flash":
                                    GameLevelConstants.s_flashLevels = responseObject.levels;
                                    break;
                            }
                        }
                        TrainingNavigation.goToScene(SceneName.s_levelsScene);
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
        });

        WinPanelCopy.transform.SetParent(GameObject.Find("Canvas").transform, false);
        WinPanelCopy.transform.SetAsLastSibling();
    }

    public void ActivateChallengePanel(GameObject panel)
    {
        GameObject panelCopy = Instantiate(panel) as GameObject;

        panelCopy.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => {
            initLoadingAndStart();
            dataController = gameObject.AddComponent<DataController>();
            dataController.MenuLastPage = NavigationConstants.s_challengeNav;
            SceneManager.LoadScene(SceneName.s_gameMenu);
        });

        panelCopy.transform.SetParent(GameObject.Find("Canvas").transform, false);
        panelCopy.transform.SetAsLastSibling();
    }

    public void ActivateChallengePanelDelayHalfSecond(GameObject panel, string thisGame, GameObject thisAchievementsPanelPrefab,
        List<Achievement> thisAchievementsList,AchievementsController achievementsController)
    {
        StartCoroutine(ActivateChallengePanelDelayHalfSecondEnum(panel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController));
    }

    IEnumerator ActivateChallengePanelDelayHalfSecondEnum(GameObject panel, string thisGame, GameObject thisAchievementsPanelPrefab,
        List<Achievement> thisAchievementsList,AchievementsController achievementsController)
    {
        yield return new WaitForSeconds(0.5f);
        achievementsController.ChallangecompletedAchievements(panel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList);
        StopAllCoroutines();
    }

    public void UpdateRestrictions()
    {
        restrictionsAPIController = gameObject.AddComponent<RestrictionsAPIController>();
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

        restrictionsAPIController.updateRestrictions(
            currentDate + "T22:31:59.076Z",
                (OnSuccess) =>
                {
                    print("Restriction updated");
                },
                (OnFailure) =>
                {
                    print(OnFailure);
                }
            );
    }

    #endregion

    private void initLoadingAndStart()
    {
        _loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        _loadingScreen.StartLoading();
    }

    public void ChallangeFinished(string timeText, GameObject challengeFirstPlayPanel,
        GameObject challengeLossPanel, GameObject challengeWinPanel, GameObject challengeDrawPanel, 
        string thisGame, GameObject thisAchievementsPanelPrefab, List<Achievement> thisAchievementsList, AchievementsController achievementsController)
    {
        challengeAPIController.UpdateChallenge(float.Parse(timeText), "DONE", ChallengeConstants.s_challengeId,
            (OnSuccess) => print("success"), (OnFailure) => print("fail"));
        UpdateRestrictions();
        ChallengeConstants.s_isChallenge = false;

        if (!ChallengeConstants.s_isAcceptor)
            ActivateChallengePanelDelayHalfSecond(challengeFirstPlayPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
        else
        {
            if (ChallengeConstants.s_challengerScore > float.Parse(timeText))
                ActivateChallengePanelDelayHalfSecond(challengeLossPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
            else if (ChallengeConstants.s_challengerScore < float.Parse(timeText))
                ActivateChallengePanelDelayHalfSecond(challengeWinPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
            else if (ChallengeConstants.s_challengerScore == float.Parse(timeText))
                ActivateChallengePanelDelayHalfSecond(challengeDrawPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
            ChallengeConstants.s_isAcceptor = false;
        }
    }

    public void LevelCompleted(string timeText, GameLevel[] gameLevels, string game)
    {
        bool levelExists = false;
        int levelId = 0;
        for (int i = 0; i < gameLevels.Length; i++)
        {
            if (gameLevels[i].level == LevelScript._selectedLevel)
            {
                levelExists = true;
                levelId = gameLevels[i].id;
            }
        }
        if (levelExists)
        {
            try
            {
                gameAPIController.UpdateGameLevel(levelId, setStars(int.Parse(timeText)), int.Parse(timeText),
                    (OnSuccess) => print("success"), (OnFailure) => print("fail"));
            }
            catch (UserException e)
            {
                Debug.Log(e.Message);
            }
        } 
        else
        {
            try
            {
                gameAPIController.CreateGameLevel(setStars(int.Parse(timeText)), int.Parse(timeText), LevelScript._selectedLevel, game,
                    (OnSuccess) => print("success"), (OnFailure) => print("fail"));
            }
            catch (UserException e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    public void GameOver(GameObject challengeLossPanel, GameObject losePanel, string scene)
    {
        if (ChallengeConstants.s_isChallenge)
        {
            challengeAPIController.UpdateChallenge(0, "DONE", ChallengeConstants.s_challengeId,
                (OnSuccess) => print("success"), (OnFailure) => print("fail"));
            UpdateRestrictions();
            ChallengeConstants.s_isChallenge = false;
            ChallengeConstants.s_isAcceptor = false;
            ActivateChallengePanel(challengeLossPanel);
        }
        else
            ActivateLosePanel(losePanel, scene);
    }

    public void IncreaseTime(float timeToIncrease)
    {
        userStatisticsAPIController.IncreaseTimePlayed(timeToIncrease,
            (OnSuccess) =>
            {
                
            },
            (OnFailure) =>
            {
                print("fail");
            }
        );
    }
}
