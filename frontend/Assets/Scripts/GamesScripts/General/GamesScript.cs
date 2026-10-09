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
        // the time allowed for 3 and 2 stars grows with the level (see StarRules)
        return StarRules.Stars(score, LevelScript._selectedLevel);
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

        WinPanelCopy.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = MemoryAthlete.IsRound ? "MEMORY ATHLETE" : "LEVEL " + LevelScript._selectedLevel;
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
        string thisGame, GameObject thisAchievementsPanelPrefab, List<Achievement> thisAchievementsList, AchievementsController achievementsController,
        int correct = -1, float secondsLeft = 0f, bool completed = true)
    {
        MemoryAthlete.LastChallengeCompleted = completed && MemoryAthlete.IsRound;
        float score = float.Parse(timeText);
        // progress = correct * 1000 + seconds left: most correct first, then least time used.
        // This also decides rounds that were lost, so a result is no longer a draw just because both failed.
        int progress = correct >= 0 ? correct * 1000 + Mathf.Clamp(Mathf.RoundToInt(secondsLeft), 0, 999) : -1;
        challengeAPIController.UpdateChallenge(score, "DONE", ChallengeConstants.s_challengeId, progress,
            (OnSuccess) => print("success"), (OnFailure) => print("fail"));
        UpdateRestrictions();
        ChallengeConstants.s_isChallenge = false;

        if (!ChallengeConstants.s_isAcceptor)
            ActivateChallengePanelDelayHalfSecond(challengeFirstPlayPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
        else
        {
            int order = ChallengeOrder(progress, score, ChallengeConstants.s_challengerProgress, ChallengeConstants.s_challengerScore);
            if (order < 0)
                ActivateChallengePanelDelayHalfSecond(challengeLossPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
            else if (order > 0)
                ActivateChallengePanelDelayHalfSecond(challengeWinPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
            else
                ActivateChallengePanelDelayHalfSecond(challengeDrawPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController);
            ChallengeConstants.s_isAcceptor = false;
        }
    }

    /// <summary>Positive = I did better. Uses progress when both sides have it, otherwise the old score rule.</summary>
    public static int ChallengeOrder(int myProgress, float myScore, int theirProgress, float theirScore)
    {
        if (myProgress >= 0 && theirProgress >= 0) return myProgress.CompareTo(theirProgress);
        return myScore.CompareTo(theirScore);
    }

    /// <summary>
    /// A round that was lost (hearts used up or time out). In a challenge the result is still reported, with
    /// how far the player got, so the other player's result can be compared fairly.
    /// </summary>
    public void RoundFailed(GameObject challengeFirstPlayPanel, GameObject challengeLossPanel, GameObject challengeWinPanel,
        GameObject challengeDrawPanel, GameObject losePanel, string scene, string thisGame,
        GameObject thisAchievementsPanelPrefab, List<Achievement> thisAchievementsList, AchievementsController achievementsController,
        int correct, float secondsLeft)
    {
        if (ChallengeConstants.s_isChallenge)
            ChallangeFinished("0", challengeFirstPlayPanel, challengeLossPanel, challengeWinPanel, challengeDrawPanel,
                thisGame, thisAchievementsPanelPrefab, thisAchievementsList, achievementsController, correct, secondsLeft, false);
        else
            ActivateLosePanel(losePanel, scene);
    }

    public void LevelCompleted(string timeText, GameLevel[] gameLevels, string game)
    {
        int score;
        if (!int.TryParse(timeText, out score)) return;
        if (MemoryAthlete.IsRound) return; // the open Memory Athlete level is outside the 20-level progress
        int stars = setStars(score);
        int level = LevelScript._selectedLevel;
        // The FIRST time level 20 of a game is cleared, every lower level counts as cleared with the same number of
        // stars (all levels unlock and their XP is paid). Replaying only counts for the level itself.
        var known = LevelProgress.Get(game);
        if (level == 20 && known != null && StarRules.StarsOf(known, 20) == 0)
            for (int lower = 1; lower < 20; lower++)
                if (StarRules.StarsOf(known, lower) < stars) ProgressSync.Instance.Complete(game, lower, stars, 0);
        ProgressSync.Instance.Complete(game, level, stars, score);
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
