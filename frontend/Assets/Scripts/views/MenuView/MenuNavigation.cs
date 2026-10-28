using System;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;
public class MenuNavigation : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public GameObject homeScreen;
    public GameObject trainingScreen;

    public GameObject accountScreen;
    public GameObject sideMenu;
    public GameObject tipsScreen;
    public GameObject challengeScreen;
    public GameObject paymentScreen;
    public GameObject statisticsScreen;
    public GameObject achievementsScreen;
    public GameObject globalScoreScreen;

    // challenge game objects
    public GameObject challengeHomeScreen;
    public GameObject challengeNotificationScreen;
    public GameObject challengeRandomScreen;

    public GameObject linkMethodScreen, funImageScreen, memoryPalaceScreen, sensesScreen, emotionScreen, retrivalScreen, mythScreen, mindsetScreen, focusScreen, bornScreen, dreamScreen;
    public GameObject memoryPalaceBtn, sensesBtn, emotionBtn, retrivalBtn, mythBtn, mindsetBtn, focusBtn, bornBtn, dreamBtn;

    public GameObject videoScreen;
    public GameObject video;
    public GameObject video2Image, video3Image, video4Image;

    public GameObject restrictionsPanel;

    public GameObject achievementPrefab;

    public Text userName;
    public Text level;
    public Text xp;
    public Text levelStatisticsScreen;
    public Text xpStatisticsScreen;

    public Text restrictionsTxt;

    public GameAPIController gameApiController;
    public DataController dataController;
    public UserAPIController _userAPIController;
    public RestrictionsAPIController restrictionsAPIController;
    private AchievementsAPIController achievementsAPIController;
    private StatisticsView statisticsView;

    public Image statisticsBackground;
    public Image trainingBackground;
    public Image challengeBackground;
    public Image videosBackground;
    public Image tipsBackground;

    public VerticalLayoutGroup achievementsPopupLayoutGroup;

    private LoadingManager _loadingScreen;

    // Subscriptions
    SubscriptionInfo monthlySubscription;
    SubscriptionInfo yearlySubscription;

    private void Awake()
    {
        dataController = gameObject.AddComponent<DataController>();
        _userAPIController = gameObject.AddComponent<UserAPIController>();
        restrictionsAPIController = gameObject.AddComponent<RestrictionsAPIController>();
        achievementsAPIController = gameObject.AddComponent<AchievementsAPIController>();
        statisticsView = gameObject.GetComponent<StatisticsView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // For Ipad
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            trainingBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 28");
            challengeBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 27");
            achievementsPopupLayoutGroup.spacing = 300;


            levelStatisticsScreen.fontSize = xpStatisticsScreen.fontSize = 28;
            levelStatisticsScreen.resizeTextMaxSize = xpStatisticsScreen.resizeTextMaxSize = 28;
        } 
        else if (Screen.height > 2200)
        {
            videosBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 31");
            tipsBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 31");
            //statisticsBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 31");
            trainingBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 25");
            challengeBackground.sprite = Resources.Load<Sprite>("GamesResources/Home/Asset 26");
            achievementsPopupLayoutGroup.spacing = 250;

            levelStatisticsScreen.fontSize = xpStatisticsScreen.fontSize = 47;
            levelStatisticsScreen.resizeTextMaxSize = xpStatisticsScreen.resizeTextMaxSize = 47;
        }

        PaymentRestrictions();

        if (dataController.MenuLastPage == NavigationConstants.s_trainingNav)
        {
            GoToTraining();
        }
        else if (dataController.MenuLastPage == NavigationConstants.s_homeNav)
        {
            GoToHome();
        }
        else if (dataController.MenuLastPage == NavigationConstants.s_challengeNav)
        {
            GoToChallenge();
        }

        Application.targetFrameRate = 120;
        _loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        _loadingScreen.EndLoadingWithDelay();

        gameApiController = gameObject.AddComponent<GameAPIController>();
        _updateDataInHomeScreen();

        ProgressSync.Instance.RetryPending();
        GetCompleatedAchievements();
    }

    public void PaymentRestrictions()
    {
        if (UserConstants.s_user.paymentStatus == PaymentStatus.s_none)
        {
            restrictionsAPIController.GetRestriction(
                (OnSuccess) =>
                {
                    restrictionsTxt.text = "(" + OnSuccess.numberOfChallengeGames + "/3 challenges left)";
                },
                (OnFailure) =>
                {
                    print(OnFailure);
                }
            );
        }
        else
        {
            restrictionsTxt.gameObject.SetActive(false);
        }
    }

    public void GoToTraining()
    {
        homeScreen.SetActive(false);
        trainingScreen.SetActive(true);
        // what the cards show (Images / Help / Words / Numbers / Random): one choice for all games
        ContentSelectorBar.AttachToTraining(this);
    }

    public void GoToStatistics()
    {
        homeScreen.SetActive(false);
        statisticsScreen.SetActive(true);
        statisticsView.ApplyGameStatistics();
        statisticsView.CallLevelsAvg();
        statisticsView.TimePlayed();
    }

    public void GoAchievements()
    {
        statisticsScreen.SetActive(false);
        achievementsScreen.SetActive(true);
        GetCompleatedAchievements();
    }
    public void GoGlobalScore()
    {
        statisticsScreen.SetActive(false);
        globalScoreScreen.SetActive(true);
    }

    public void GoBackToStatistics()
    {
        achievementsScreen.SetActive(false);
        globalScoreScreen.SetActive(false);
        statisticsScreen.SetActive(true);
    }

    private bool challengeCheckRunning;

    private void FinishChallengeCheck()
    {
        challengeCheckRunning = false;
        if (_loadingScreen != null) _loadingScreen.EndLoading();
    }

    private System.Collections.IEnumerator ReleaseChallengeCheck(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (challengeCheckRunning) FinishChallengeCheck();
    }

    private void OpenChallengeScreen()
    {
        homeScreen.SetActive(false);
        trainingScreen.SetActive(false);
        challengeNotificationScreen.SetActive(false);
        challengeScreen.SetActive(true);
    }

    public void GoToChallenge()
    {
        // The screen only opens after the daily-limit check. Ignore repeat taps while it runs, show the
        // spinner if it is slow, and never leave the button dead when the request fails.
        if (challengeCheckRunning) return;
        challengeCheckRunning = true;
        if (_loadingScreen != null) _loadingScreen.StartLoading();
        StartCoroutine(ReleaseChallengeCheck(15f));
        restrictionsAPIController.GetRestriction(
            (OnSuccess) =>
            {
                FinishChallengeCheck();
                if (UserConstants.s_user.paymentStatus == PaymentStatus.s_none && OnSuccess.numberOfChallengeGames == 0)
                {
                    DateTime now = DateTime.Now;
                    int hours = 23 - now.Hour;
                    int minutes = 60 - now.Minute;

                    ActivateRestrictionsPopup(hours+"h "+minutes+"m left till next challenge", "Upgrade so you can have unlimited challenges");
                }
                else
                {
                    OpenChallengeScreen();
                }
            },
            (OnFailure) =>
            {
                FinishChallengeCheck();
                if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none) OpenChallengeScreen();
                else ActivateRestrictionsPopup("Can't reach the server", "Check your connection and try again.");
            }
        );
    }
    public void GoToPayment()
    {
        homeScreen.SetActive(false);
        trainingScreen.SetActive(false);
        challengeNotificationScreen.SetActive(false);
        challengeScreen.SetActive(false);
        restrictionsPanel.SetActive(false);
        paymentScreen.SetActive(true);
    }

    public void GoToHome()
    {
        _updateDataInHomeScreen();
        trainingScreen.SetActive(false);
        accountScreen.SetActive(false);
        paymentScreen.SetActive(false);
        challengeScreen.SetActive(false);
        tipsScreen.SetActive(false);
        videoScreen.SetActive(false);
        statisticsScreen.SetActive(false);
        homeScreen.SetActive(true);
    }

    public void GoToAccount()
    {
        homeScreen.SetActive(false);
        accountScreen.SetActive(true);
        challengeScreen.SetActive(false);
        homeScreen.SetActive(true);
    }

    public void GoToNotification() {
        OpenChallengeScreen();
        var navigation=GetComponent<ChallengeNavigation>();
        if(navigation!=null)SprintLobby.Attach(navigation).Open();
    }

    public void GoToChallengeHome()
    {
        OpenChallengeScreen();
        challengeNotificationScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeHomeScreen.SetActive(true);
    }

    public void GoToChallengeRandom()
    {
        challengeNotificationScreen.SetActive(false);
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(true);
    }

    public void GoToTips()
    {
        if (UserConstants.s_user.paymentStatus == PaymentStatus.s_none)
        {
            Color c = memoryPalaceBtn.GetComponent<Image>().color;
            c.a = 0.2f;

            memoryPalaceBtn.GetComponent<Image>().color = c;
            sensesBtn.GetComponent<Image>().color = c;
            emotionBtn.GetComponent<Image>().color = c;
            retrivalBtn.GetComponent<Image>().color = c;
            mythBtn.GetComponent<Image>().color = c;
            mindsetBtn.GetComponent<Image>().color = c;
            focusBtn.GetComponent<Image>().color = c;
            bornBtn.GetComponent<Image>().color = c;
            dreamBtn.GetComponent<Image>().color = c;

        }

        tipsScreen.SetActive(true);
        linkMethodScreen.SetActive(false);
        funImageScreen.SetActive(false);
        memoryPalaceScreen.SetActive(false);
        sensesScreen.SetActive(false);
        emotionScreen.SetActive(false);
        retrivalScreen.SetActive(false);
        mythScreen.SetActive(false);
        mindsetScreen.SetActive(false);
        focusScreen.SetActive(false);
        bornScreen.SetActive(false);
        dreamScreen.SetActive(false);

        // free Major System tip + "I have read this" boxes (earns the tip badges)
        TipsEnhancer.Ensure(this);
    }

    public void GoToLinkMethodScreen()
    {
        linkMethodScreen.SetActive(true);
    }

    public void GoToFunScreen()
    {
        funImageScreen.SetActive(true);
    }

    public void GoToMemoryPalaceScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            memoryPalaceScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToSensesScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            sensesScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToEmotionScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            emotionScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToRetrivalScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            retrivalScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToMythScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            mythScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToMindsetScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            mindsetScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToFocusScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            focusScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToBornScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            bornScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToDreamScreen()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
            dreamScreen.SetActive(true);
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToVideosScreen()
    {
        VideosEnhancer.Ensure(this);
        if (UserConstants.s_user.paymentStatus == PaymentStatus.s_none)
        {
            Color c = memoryPalaceBtn.GetComponent<Image>().color;
            c.a = 0.2f;

            video2Image.transform.GetChild(0).GetComponent<Text>().color = new Color(216f / 255f, 213f / 255f, 213f / 255f);
            video2Image.transform.GetChild(1).GetComponent<Image>().color = c;

            video3Image.transform.GetChild(0).GetComponent<Text>().color = new Color(216f / 255f, 213f / 255f, 213f / 255f);
            video3Image.transform.GetChild(1).GetComponent<Image>().color = c;

            video4Image.transform.GetChild(0).GetComponent<Text>().color = new Color(216f / 255f, 213f / 255f, 213f / 255f);
            video4Image.transform.GetChild(1).GetComponent<Image>().color = c;
        }

        currentVideoKey = null;
        if (videoPlayer != null) videoPlayer.Stop();
        video.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;
        videoScreen.SetActive(true);
    }

    public void PlayTutorialVideo(string url)
    {
        if (videoPlayer == null || video == null) return;
        videoPlayer.Stop();
        // A game guide must not award the badge for the previous memory lesson.
        HookVideoEnd(null);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        video.SetActive(true);
        videoPlayer.Play();
    }

    public void GoToVideo1()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        HookVideoEnd("video-intro");
        videoPlayer.url = "https://www.memotopia.com/video/1_Introduction.mp4";
        video.SetActive(true);
    }

    public void GoToVideo2()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            HookVideoEnd("video-link");
            videoPlayer.url = "https://www.memotopia.com/video/2_Link_Method.mp4";
            video.SetActive(true);
        }
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToVideo3()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            HookVideoEnd("video-associations");
            videoPlayer.url = "https://www.memotopia.com/video/3_Associations.mp4";
            video.SetActive(true);
        }
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    public void GoToVideo4()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            HookVideoEnd("video-palace");
            videoPlayer.url = "https://www.memotopia.com/video/4_MemoryPalace.mp4";
            video.SetActive(true);
        }
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
    }

    // A video earns its badge when it has been watched to the end.
    private string currentVideoKey;
    private bool videoEndHooked;

    private void HookVideoEnd(string key)
    {
        currentVideoKey = key;
        if (videoEndHooked || videoPlayer == null) return;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoEndHooked = true;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        string key = currentVideoKey;
        currentVideoKey = null;
        if (string.IsNullOrEmpty(key)) return;
        try
        {
            if (!GameManager.UnlockAchievement(key)) return;
            ProgressSync.Instance.GetApi<AchievementsAPIController>().FlushPending();
            BadgePopup.ShowFor(this, key);
        }
        catch (Exception) { /* the badge is saved either way */ }
    }

    private void _updateDataInHomeScreen()
    {
        userName.text = "Hi " + UserConstants.s_user.username;
        level.text = UserConstants.s_user.userStatistics.level.ToString();
        xp.text = UserConstants.s_user.userStatistics.xp.ToString();
        levelStatisticsScreen.text = UserConstants.s_user.userStatistics.level.ToString();
        xpStatisticsScreen.text = UserConstants.s_user.userStatistics.xp.ToString();
    }

    public void Logout()
    {
        // Facebook SDK disabled.

        dataController.Token = null;
        GameLevelConstants.s_boxesLevels = null;
        GameLevelConstants.s_pairsLevels = null;
        GameLevelConstants.s_flashLevels = null;
        _loadingScreen.GoToSceneWithLoading(SceneName.s_loginScene);
    }

    // Enable after the public listing is available; TestFlight alone does not create it.
    public const bool PublicStoreListingAvailable = false;
    public const string AppStoreReviewUrl = "https://apps.apple.com/app/id1442530846?action=write-review";

    public void RateBtnClick()
    {
        if (!PublicStoreListingAvailable) return;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            Application.OpenURL(AppStoreReviewUrl);
        else if (Application.platform == RuntimePlatform.Android)
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
    }

    public void FacebookShare()
    {
        // The menu entry keeps its old method name (the scene refers to it) but now invites friends
        // through the phone's share sheet instead of Facebook.
        InviteFriends();
    }

    public void InviteFriends()
    {
        string me = (UserConstants.s_user != null && !string.IsNullOrEmpty(UserConstants.s_user.username)) ? UserConstants.s_user.username : "";
        // The store listings are not public yet, so the invitation points to the website.
        string text = "Train your memory with me on Memotopia and challenge me!" +
                      (me.Length > 0 ? " My username is " + me + "." : "") + " https://memotopia.com";
        NativeShare.Share(text, "Invite friends");
    }

    private void setPaymentStatusToNone()
    {
        try
        {
            _userAPIController.UpdatePaymentStatus(
                PaymentStatus.s_none,
                (OnSuccess) =>
                {
                    print(OnSuccess);
                    //_loadingScreen.EndLoadingWithDelay();
                    //SceneManager.LoadScene(SceneName.s_gameMenu);
                },
                (OnFailure) =>
                {
                    //_loadingScreen.EndLoadingWithDelay();
                    //HandleConditionWithAlertMessages(OnFailure);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void ActivateRestrictionsPopup(string title, string desc)
    {
        restrictionsPanel.SetActive(true);

        restrictionsPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = title;
        restrictionsPanel.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = desc;

        restrictionsPanel.transform.GetChild(0).transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
        {
            restrictionsPanel.SetActive(false);
        });

        restrictionsPanel.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
        {
            GoToPayment();
        });
    }

    public void ActivateAchievementsPrefab()
    {
        GameObject prefab = Instantiate(achievementPrefab) as GameObject;

        prefab.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => {
            Destroy(prefab);
        });

        prefab.transform.SetParent(GameObject.Find("Canvas").transform, false);
        prefab.transform.SetAsLastSibling();
    }

    private void ReconcileAchievements()
    {
        LevelProgress.EnsureUser();
        var view = achievementsScreen.GetComponentInChildren<AchievementsView>(true);
        var stats = UserConstants.s_user == null ? null : UserConstants.s_user.userStatistics;
        if (view == null || view.achievements == null) return;
        foreach (string key in AchievementRules.Eligible(null, 0,
            GameLevelConstants.s_boxesLevels, GameLevelConstants.s_pairsLevels, GameLevelConstants.s_flashLevels,
            stats == null ? 0 : stats.xp, stats == null ? 0 : stats.numberOfWinChallenges))
            if (view.achievements.Any(a => a != null && a.constantString == key)) GameManager.UnlockAchievement(key);
    }

    private bool fetchingAchievements;
    private void GetCompleatedAchievements()
    {
        if (fetchingAchievements) return;
        achievementsAPIController = gameObject.GetComponent<AchievementsAPIController>();
        if (achievementsAPIController == null) achievementsAPIController = gameObject.AddComponent<AchievementsAPIController>();
        fetchingAchievements = true;
        int owner = GameManager.AchievementUserId;
        // Fetch actual performance even when the old achievement list is empty, wrong or unavailable.
        foreach (var game in new[] { "Boxes", "Pairs", "Flash" })
            ProgressSync.Instance.GetLevels(game, ok =>
            {
                if (!ok || this == null || GameManager.AchievementUserId != owner) return;
                ReconcileAchievements();
                achievementsAPIController.FlushPending();
            });
        var statsApi = ProgressSync.Instance.GetApi<UserStatisticsAPIController>();
        statsApi.GetUserStatisticsChallengeGames(stats =>
        {
            if (this == null || GameManager.AchievementUserId != owner || stats == null) return;
            if (stats.user != null && stats.user.ID > 0 && stats.user.ID != owner) return;
            UserConstants.s_user.userStatistics = stats;
            ReconcileAchievements();
            achievementsAPIController.FlushPending();
        }, error => Debug.LogWarning("Achievement statistics could not refresh. Keeping verified progress."));
        statsApi.GetGlobalScore(result =>
        {
            if (this == null || GameManager.AchievementUserId != owner || result == null || result.statistics == null || result.statistics.Length == 0) return;
            var leader = result.statistics[0];
            if (leader != null && leader.xp > 0 && leader.user != null && leader.user.ID == owner)
                GameManager.UnlockAchievement("first-global-score");
            achievementsAPIController.FlushPending();
        }, error => Debug.LogWarning("Achievement leaderboard could not refresh."));
        achievementsAPIController.GetAchievements(result =>
        {
            fetchingAchievements = false;
            if (GameManager.AchievementUserId != owner) return;
            GameManager.completedAchievements = result.achievements.ToList();
            ReconcileAchievements();
            achievementsAPIController.FlushPending();
        }, error =>
        {
            fetchingAchievements = false;
            if (GameManager.AchievementUserId == owner) achievementsAPIController.FlushPending();
            Debug.LogWarning(error.ErrorMessage);
        });
    }
}
