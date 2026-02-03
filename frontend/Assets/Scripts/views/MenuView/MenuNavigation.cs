using System;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using Facebook.Unity;
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
    }

    public void GoToStatistics()
    {
        homeScreen.SetActive(false);
        statisticsScreen.SetActive(true);
        statisticsView.CallLevelsAvg();
        statisticsView.TimePlayed();
    }

    public void GoAchievements()
    {
        statisticsScreen.SetActive(false);
        achievementsScreen.SetActive(true);
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

    public void GoToChallenge()
    {
        restrictionsAPIController.GetRestriction(
            (OnSuccess) =>
            {
                if (UserConstants.s_user.paymentStatus == PaymentStatus.s_none && OnSuccess.numberOfChallengeGames == 0)
                {
                    DateTime now = DateTime.Now;
                    int hours = 23 - now.Hour;
                    int minutes = 60 - now.Minute;

                    ActivateRestrictionsPopup(hours+"h "+minutes+"m left till next challenge", "Upgrade so you can have unlimited challenges");
                }
                else
                {
                    homeScreen.SetActive(false);
                    trainingScreen.SetActive(false);
                    challengeNotificationScreen.SetActive(false);
                    challengeScreen.SetActive(true);
                }
            },
            (OnFailure) =>
            {
                print(OnFailure);
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
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeNotificationScreen.SetActive(true);
    }

    public void GoToChallengeHome()
    {
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

        video.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;
        videoScreen.SetActive(true);
    }

    public void GoToVideo1()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        videoPlayer.url = "http://www.memotopia.com/video/1_Introduction.mp4";
        video.SetActive(true);
    }

    public void GoToVideo2()
    {
        if (UserConstants.s_user.paymentStatus != PaymentStatus.s_none)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            videoPlayer.url = "http://www.memotopia.com/video/2_Link_Method.mp4";
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

            videoPlayer.url = "http://www.memotopia.com/video/3_Associations.mp4";
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

            videoPlayer.url = "http://www.memotopia.com/video/4_MemoryPalace.mp4";
            video.SetActive(true);
        }
        else
            ActivateRestrictionsPopup("This content is not available for you current payment plan", "Upgrade so you can have access");
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
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
        }

        dataController.Token = null;
        _loadingScreen.GoToSceneWithLoading(SceneName.s_loginScene);
    }

    public void RateBtnClick()
    {
        try
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    Application.OpenURL("https://apps.apple.com/us/app/memotopia/id1442530846");
                    break;
                case RuntimePlatform.Android:
                    Application.OpenURL("http://play.google.com/store/apps/details?id=" + Application.identifier);
                    break;
            }
        }
        catch (Exception)
        {
            Application.Quit();
        }
    }

    public void FacebookShare()
    {
        FB.ShareLink(new System.Uri("https://www.memotopia.com/"), "Check it out!",
            "Memotopia - mobile game to master your memory. Play, have fun, learn, challenge friends or embark on a memorable journey that will change the way you think. So get ready to expand your mind and perfect your memory!",
            new System.Uri("https://www.memotopia.com/wp-content/themes/Memotopia/img/Asset_10.png")
            );
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

    private void GetCompleatedAchievements()
    {
        achievementsAPIController = gameObject.GetComponent<AchievementsAPIController>();
        try
        {
            achievementsAPIController.GetAchievements(
                (OnSuccess) =>
                {
                    GameManager.completedAchievements = OnSuccess.achievements.ToList();
                    string compleatedAchievements = GameManager.completedAchievements.Count.ToString();

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
}
