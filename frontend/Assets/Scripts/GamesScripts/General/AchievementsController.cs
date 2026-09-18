using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine;

public class AchievementsController : MonoBehaviour
{
    private UserStatisticsAPIController statisticsApi;
    private AchievementsAPIController achievementsApi;
    private List<Achievement> catalog = new List<Achievement>();
    private readonly Queue<Achievement> awards = new Queue<Achievement>();
    private GameObject achievementsPanelPrefab, winPanel, activePopup;
    private string score, gameScene, game;
    private int nrStars, generation;
    private bool isChallange, finished;

    private void Awake()
    {
        statisticsApi = gameObject.AddComponent<UserStatisticsAPIController>();
        achievementsApi = gameObject.AddComponent<AchievementsAPIController>();
    }

    private void Begin(GameObject panel, string mode, GameObject prefab, List<Achievement> achievements)
    {
        generation++;
        StopAllCoroutines();
        if (activePopup != null) Destroy(activePopup);
        activePopup = null;
        awards.Clear();
        finished = false;
        winPanel = panel;
        game = mode;
        achievementsPanelPrefab = prefab;
        // Never clear or mutate the game's serialized catalog.
        catalog = achievements == null ? new List<Achievement>() : achievements.Where(a => a != null).ToList();
    }

    public void LevelCompletedAchievements(string thisScore, int thisNrStars,
        GameObject thisWinPanel, string thisGameScene, string thisGame,
        GameObject thisAchievementsPanelPrefab, List<Achievement> thisAchievementsList)
    {
        Begin(thisWinPanel, thisGame, thisAchievementsPanelPrefab, thisAchievementsList);
        isChallange = false;
        score = thisScore;
        nrStars = thisNrStars;
        gameScene = thisGameScene;
        CheckProgress();
        achievementsApi.FlushPending();
        ShowNext();
        StartCoroutine(CheckRemoteProgress(generation));
    }

    public void ChallangecompletedAchievements(GameObject panel, string mode,
        GameObject prefab, List<Achievement> achievements)
    {
        Begin(panel, mode, prefab, achievements);
        isChallange = true;
        CheckProgress();
        achievementsApi.FlushPending();
        ShowNext();
        StartCoroutine(CheckRemoteProgress(generation));
    }

    private void CheckProgress()
    {
        var user = UserConstants.s_user;
        var stats = user == null ? null : user.userStatistics;
        foreach (string key in AchievementRules.Eligible(isChallange ? null : game,
            isChallange ? 0 : LevelScript._selectedLevel,
            GameLevelConstants.s_boxesLevels, GameLevelConstants.s_pairsLevels, GameLevelConstants.s_flashLevels,
            stats == null ? 0 : stats.xp, stats == null ? 0 : stats.numberOfWinChallenges)) Award(key);
    }

    private void Award(string key)
    {
        var achievement = catalog.Find(a => a.constantString == key);
        if (achievement != null && GameManager.UnlockAchievement(key))
        {
            // Remote awards update the collection without blocking or reopening the result screen.
            if (!finished) awards.Enqueue(achievement);
        }
    }

    private IEnumerator CheckRemoteProgress(int run)
    {
        bool checkedStats = false, checkedLeaderboard = isChallange;
        bool accepting = true;
        int owner = GameManager.AchievementUserId;
        statisticsApi.GetUserStatisticsChallengeGames(stats =>
        {
            if (!accepting || generation != run || GameManager.AchievementUserId != owner) return;
            if (stats != null && UserConstants.s_user != null) UserConstants.s_user.userStatistics = stats;
            CheckProgress();
            achievementsApi.FlushPending();
            checkedStats = true;
        }, error => checkedStats = true);
        if (!isChallange)
            statisticsApi.GetGlobalScore(result =>
            {
                if (!accepting || generation != run || GameManager.AchievementUserId != owner) return;
                if (result != null && result.statistics != null && result.statistics.Length > 0 &&
                    result.statistics[0] != null && result.statistics[0].user != null && result.statistics[0].user.ID == owner)
                    Award("first-global-score");
                achievementsApi.FlushPending();
                checkedLeaderboard = true;
            }, error => checkedLeaderboard = true);
        float deadline = Time.realtimeSinceStartup + 12f;
        while ((!checkedStats || !checkedLeaderboard) && Time.realtimeSinceStartup < deadline) yield return null;
        accepting = false;
        if (generation != run) yield break;
        achievementsApi.FlushPending();
    }

    private void ShowNext()
    {
        if (finished) return;
        if (awards.Count == 0 || achievementsPanelPrefab == null)
        {
            finished = true;
            if (isChallange) ChallangePopup(); else WinPopup();
            return;
        }
        var canvas = GetComponentInParent<Canvas>();
        var canvasObject = canvas == null ? GameObject.Find("Canvas") : canvas.gameObject;
        if (canvasObject == null) { awards.Clear(); ShowNext(); return; }
        var award = awards.Dequeue();
        activePopup = Instantiate(achievementsPanelPrefab, canvasObject.transform, false);
        activePopup.transform.SetAsLastSibling();
        var view = activePopup.GetComponent<AchievementPopupView>();
        view.Bind(award, catalog);
        var popup = activePopup;
        foreach (var close in activePopup.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            close.onClick.AddListener(() =>
        {
            if (activePopup != popup) return;
            activePopup = null;
            popup.SetActive(false);
            Destroy(popup);
            ShowNext();
        });
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


}
