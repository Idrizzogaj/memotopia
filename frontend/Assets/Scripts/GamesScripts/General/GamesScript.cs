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
        gameAPIController = ProgressSync.Instance.GetApi<GameAPIController>();
        challengeAPIController = ProgressSync.Instance.GetApi<ChallengeAPIController>();
        userStatisticsAPIController = ProgressSync.Instance.GetApi<UserStatisticsAPIController>();
        dataController = gameObject.AddComponent<DataController>();

    }

    public IEnumerator ShowRSG(GameObject rsg)
    {
        rsg.SetActive(true);
        var animator = rsg.GetComponentInChildren<Animator>();
        float originalSpeed = animator == null ? 1f : animator.speed;
        if (animator != null && !ChallengeConstants.s_isChallenge) animator.speed = originalSpeed * 1.5f;
        yield return new WaitForSeconds(ChallengeConstants.s_isChallenge ? 3f : 2f);
        if (animator != null) animator.speed = originalSpeed;
        rsg.SetActive(false);
    }

    protected IEnumerator MovePanel(Transform panel, Transform target)
    {
        Vector3 start = panel.position;
        float elapsed = 0;
        const float duration = .2f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            panel.position = Vector3.Lerp(start, target.position, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        panel.position = target.position;
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
                LoadingManager.Navigate(SceneName.s_gameMenu);
            });
        }
        else
        {
            QuitPanel.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                initLoadingAndStart();
                LoadingManager.Navigate(SceneName.s_levelsScene);
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

            LoadingManager.Navigate(gameScene);
        });
        LosePanelCopy.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
        {
            initLoadingAndStart();
            LoadingManager.Navigate(SceneName.s_levelsScene);
        });

        LosePanelCopy.transform.SetParent(GameObject.Find("Canvas").transform, false);
        LosePanelCopy.transform.SetAsLastSibling();
    }

    public void ActivateWinPanel(string score, int nrStars, GameObject WinPanel, string gameScene, string game)
    {
        GameObject WinPanelCopy = Instantiate(WinPanel) as GameObject;

        WinPanelCopy.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "LEVEL " + LevelScript._selectedLevel;
        WinPanelCopy.transform.GetChild(0).transform.GetChild(10).transform.GetChild(0).GetComponent<Text>().text = score;

        var container = WinPanelCopy.transform.GetChild(0);
        var replay = container.GetChild(11).GetComponent<Button>();
        var next = container.GetChild(12).GetComponent<Button>();
        int completedLevel = LevelScript._selectedLevel;
        var replayRect = replay.GetComponent<RectTransform>();
        var nextRect = next.GetComponent<RectTransform>();
        replayRect.anchoredPosition = new Vector2(replayRect.anchoredPosition.x, -215);
        nextRect.anchoredPosition = new Vector2(nextRect.anchoredPosition.x, -215);
        replay.onClick.AddListener(() => LoadingManager.Navigate(gameScene));
        next.GetComponentInChildren<Text>().text = completedLevel < 20 ? "NEXT LEVEL" : "LEVELS";
        next.onClick.AddListener(() =>
        {
            if (completedLevel < 20)
            {
                LevelScript._selectedLevel = completedLevel + 1;
                LoadingManager.Navigate(gameScene);
            }
            else LoadingManager.Navigate(SceneName.s_levelsScene);
        });
        if (completedLevel < 20)
        {
            var levels = Instantiate(next.gameObject, container);
            levels.name = "LevelsButton";
            var button = levels.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => LoadingManager.Navigate(SceneName.s_levelsScene));
            levels.GetComponentInChildren<Text>().text = "LEVELS";
            levels.GetComponentInChildren<Text>().color = new Color(0, .65f, .78f);
            levels.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            var rect = levels.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -315);
            rect.sizeDelta = new Vector2(300, 80);
        }

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
            LoadingManager.Navigate(SceneName.s_gameMenu);
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
        restrictionsAPIController = ProgressSync.Instance.GetApi<RestrictionsAPIController>();
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
        int score;
        if (!int.TryParse(timeText, out score)) return;
        ProgressSync.Instance.Complete(game, LevelScript._selectedLevel, setStars(score), score);
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
