using System.Collections;
using UnityEngine;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine.UI;
using Assets.Script.Models.ResponseModels;
using Assets.Script.Models;
using System;

public class ChallengeNavigation : MonoBehaviour
{
    public UserAPIController userAPIController;
    public ChallengeAPIController challengeAPIController;
    public UserStatisticsAPIController userStatisticsAPIController;
    public GameObject challengeHomeScreen;
    public GameObject challengeNotificationScreen;
    public GameObject challengeRandomScreen;
    public GameObject challengeGameScreen;
    public GameObject challengeLevelScreen;
    public GameObject challengeHistoryScreen;
    public GameObject challengeUserProfileScreen;
    public GameObject challengeCart;
    public GameObject challengeHistoryCart;
    public GameObject challengeFavoritesCart;
    public GameObject challengeSearchCart;
    public Transform notificationContent;
    public Transform historyContent;
    public Transform favoriteContent;
    public InputField searchInput;
    public GameObject notificationNumber;
    public GameObject notificationNumberOnMainScreen;
    public Button addFavorite;
    public Button deleteFavorite;
    public Button challengeOnProfile;

    public GameObject FavoritesScrollView;
    public GameObject NoFavoritesScreen;
    public GameObject NoChallenge;

    public Text gamesPlayedNr;
    public Text winsNr;
    public Text lossesNr;
    public Text drawsNr;
    public Text gamesPlayedNrStatistics;
    public Text winsNrStatistics;
    public Text lossesNrStatistics;
    public Text drawsNrStatistics;

    public Image myImage;
    public Image historyOfChallengesImage;
    public Image pairsImage;
    public Image flashImage;
    public Image boxesImage;

    public Slider statisticsSlider;

    private string gameName = null;
    private int level;

    private Debouncer StepperDeboucer = new Debouncer(1000);
    private float wait3s = 3f;
    private bool waitCondition = false;

    //test names
    private string[] names = { "Arber Mulla", "Edmond Laja", "Flamur Krasniqi", "Idriz Zogaj" };
    private string[] avatars = { "MaskGroup54", "MaskGroup55", "MaskGroup56", "MaskGroup58", "Group16016", "MaskGroup57" };
    private int namesCount = 0;
    private int avatarCount = 0;
    public Text textNames;
    public Image avatar;
    private string username;
    private string avatarImageName;

    LoadingManager _loadingScreen;

    void Start()
    {
        if (UserConstants.s_user.userStatistics.xp >= 400)
        {
            NoChallenge.SetActive(false);
        }

        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            pairsImage.sprite = Resources.Load<Sprite>("GamesResources/Training/Mask Group 57");
            flashImage.sprite = Resources.Load<Sprite>("GamesResources/Training/Mask Group 55");
            boxesImage.sprite = Resources.Load<Sprite>("GamesResources/Training/Mask Group 56");
            historyOfChallengesImage.sprite = Resources.Load<Sprite>("GamesResources/Challenge/Asset 35");

            //pairsImage.rectTransform.sizeDelta = new Vector2(720, 285);
            //flashImage.rectTransform.sizeDelta = new Vector2(720, 285);
            //boxesImage.rectTransform.sizeDelta = new Vector2(720, 285);
        }

        TrainingHubLayout.ApplyChallenge(this);
        _loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        EnsureBellHitArea();
        userAPIController = gameObject.AddComponent<UserAPIController>();
        challengeAPIController = gameObject.AddComponent<ChallengeAPIController>();
        userStatisticsAPIController = gameObject.AddComponent<UserStatisticsAPIController>();

        challengeAPIController.GetChallenges(
            (OnSuccess) =>
            {
                SetNotificationNumber(OnSuccess);
            },
            (OnFailure) =>
            {
                _loadingScreen.EndLoading();
                    print("fail");
            }
        );
        userStatisticsAPIController.GetUserStatisticsChallengeGames(
            (OnSuccess) =>
            {
                gamesPlayedNr.text = OnSuccess.numberOfChallenges.ToString();
                winsNr.text = OnSuccess.numberOfWinChallenges.ToString();
                lossesNr.text = OnSuccess.numberOfLossChallenges.ToString();
                drawsNr.text = OnSuccess.numberOfDrawChallenges.ToString();
                gamesPlayedNrStatistics.text = OnSuccess.numberOfChallenges.ToString();
                winsNrStatistics.text = OnSuccess.numberOfWinChallenges.ToString();
                lossesNrStatistics.text = OnSuccess.numberOfLossChallenges.ToString();
                drawsNrStatistics.text = OnSuccess.numberOfDrawChallenges.ToString();

                statisticsSlider.maxValue = OnSuccess.numberOfChallenges;
                statisticsSlider.minValue = 0;
                statisticsSlider.value = OnSuccess.numberOfWinChallenges;
            },
            (OnFailure) =>
            {
                _loadingScreen.EndLoading();
                    print("fail");
            }
        );
        GetFavoriteUsers();
    }

    private void Update()
    {
        if (waitCondition)
        {
            wait3s -= Time.deltaTime;
            //emrat i ndron
            if (namesCount < names.Length)
            {
                textNames.text = names[namesCount++];
            }
            else
            {
                namesCount = 0;
            }

            if (avatarCount < avatars.Length)
            {
                avatar.sprite = Resources.Load<Sprite>("Memotopia_UI\\AccountAndSettings\\" + avatars[avatarCount++].ToString()) as Sprite;
            }
            else
            {
                avatarCount = 0;
            }

            if (wait3s <= 0)
            {
                textNames.text = username;
                avatar.sprite = Resources.Load<Sprite>("Memotopia_UI\\AccountAndSettings\\" + avatarImageName.ToString()) as Sprite;
                waitCondition = false;
                Application.targetFrameRate = 40;
                StartCoroutine(Wait3000MillisecondToReturn());
            }
        }
    }

    public void ChallengeRandomUser()
    {
        try
        {
            userAPIController.GetRandomUser(
                (OnSuccess) =>
                {
                    username = OnSuccess.username;
                    avatarImageName = OnSuccess.avatar;
                    GoToChallengeRandom();
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void StartChallenge(int level, string game, int challengerId) {
        try
        {
            // TODO: move api call to gamescirpt and call at the end of each game
            // move two lines after success out of this call
            challengeAPIController.CreateChallenge(
                level,
                game,
                challengerId,
                (OnSuccess) =>
                {
                    print("success");
                    ChallengeConstants.s_isChallenge = true;
                    LevelScript.ChallengeStartGame(game, level);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void GetFavoriteUsers()
    {
        try
        {
            challengeAPIController.GetFavorites(
                (OnSuccess) =>
                {
                    print("success");
                    SetFavorites(OnSuccess);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void SearchUsers(string search)
    {
        try
        {
            challengeAPIController.Search(
                search,
                (OnSuccess) =>
                {
                    print("success");
                    SetUserSearch(OnSuccess);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void AddFavorite(string id, GameObject btn)
    {
        try
        {
            challengeAPIController.AddFavorite(
                id,
                (OnSuccess) =>
                {
                    print(OnSuccess);
                    print("success");
                    GetFavoriteUsers();
                    btn.SetActive(false);

                    if (searchInput.text.Trim().Length != 0)
                        SearchUsers(searchInput.text.Trim());
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void DeleteFavorite(string id)
    {
        try
        {
            challengeAPIController.DeleteFavorite(
                id,
                (OnSuccess) =>
                {
                    print(OnSuccess);
                    print("success");
                    GetFavoriteUsers();

                    if (searchInput.text.Trim().Length != 0)
                        SearchUsers(searchInput.text.Trim());
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void StartChallenge(int challengeId, int level, string gameName, float score, int progress = -1) {
        ChallengeConstants.s_challengerProgress = progress;
        ChallengeConstants.s_isChallenge = true;
        ChallengeConstants.s_isAcceptor = true;
        ChallengeConstants.s_challengerScore = score;
        ChallengeConstants.s_challengeId = challengeId;
        LevelScript.ChallengeStartGame(gameName, level);
    }

    public void GoToChallengeHome()
    {
        HideAthleteToastNow();
        waitCondition = false;
        challengeNotificationScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeUserProfileScreen.SetActive(false);
        challengeGameScreen.SetActive(false);
        challengeHomeScreen.SetActive(true);

        addFavorite.onClick.RemoveAllListeners();
        deleteFavorite.onClick.RemoveAllListeners();

        if (searchInput.text.Trim().Length == 0)
            GetFavoriteUsers();
    }

    public void GoToChallengeRandom()
    {
        HideAthleteToastNow();
        myImage.sprite = Resources.Load<Sprite>("Memotopia_UI/AccountAndSettings/" + UserConstants.s_user.Avatar);

        challengeNotificationScreen.SetActive(false);
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(true);
        Application.targetFrameRate = 20;
        waitCondition = true;
    }

    public void GoToChallengeGame()
    {
        TrainingHubLayout.ApplyChallenge(this);
        HideAthleteToastNow();
        challengeNotificationScreen.SetActive(false);
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeUserProfileScreen.SetActive(false);
        challengeHistoryScreen.SetActive(false);
        challengeLevelScreen.SetActive(false);
        challengeGameScreen.SetActive(true);
    }

    // ---- Memory Athlete: a fourth tier under Easy / Medium / Hard, unlocked by clearing all 20 levels of one game.
    private Image athleteImage;
    private GameObject athleteToast;

    private void EnsureAthleteButton()
    {
        if (challengeLevelScreen == null) return;
        var root = challengeLevelScreen.transform;
        var easy = root.Find("Easy") as RectTransform;
        var medium = root.Find("Medium") as RectTransform;
        var hard = root.Find("Hard") as RectTransform;
        if (easy == null || medium == null || hard == null) return;

        var existing = root.Find("MemoryAthlete") as RectTransform;
        RectTransform athlete = existing;
        if (athlete == null)
        {
            var clone = Instantiate(hard.gameObject, root, false);
            clone.name = "MemoryAthlete";
            athlete = (RectTransform)clone.transform;
            var sprite = Resources.Load<Sprite>("GamesResources/Challenge/MemoryAthlete");
            athleteImage = clone.GetComponent<Image>();
            if (sprite != null) athleteImage.sprite = sprite;
            var button = clone.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent(); // drop the copied "Hard" action
            button.onClick.AddListener(OnAthletePressed);
        }
        else athleteImage = athlete.GetComponent<Image>();

        // four rows instead of three; the artwork keeps its proportions
        float[] top = { 0.865f, 0.655f, 0.445f, 0.235f };
        var rows = new[] { easy, medium, hard, athlete };
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].anchorMin = new Vector2(0.02f, top[i] - 0.20f);
            rows[i].anchorMax = new Vector2(0.98f, top[i]);
            rows[i].offsetMin = rows[i].offsetMax = Vector2.zero;
            var image = rows[i].GetComponent<Image>();
            if (image != null) image.preserveAspect = true;
        }
        athleteImage.color = MemoryAthlete.Unlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.8f);
    }

    private void OnAthletePressed()
    {
        if (!MemoryAthlete.Unlocked) { ShowAthleteToast(MemoryAthlete.LockedHint); return; }
        SetLevel(MemoryAthlete.Level);
    }

    private void ShowAthleteToast(string message)
    {
        var root = challengeGameScreen.transform;
        if (athleteToast == null)
        {
            athleteToast = new GameObject("AthleteToast", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)athleteToast.transform;
            rect.SetParent(root, false);
            rect.anchorMin = new Vector2(0.06f, 0.44f);
            rect.anchorMax = new Vector2(0.94f, 0.56f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            athleteToast.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.25f, 0.92f);
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var textRect = (RectTransform)textObject.transform;
            textRect.SetParent(rect, false);
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 8f); textRect.offsetMax = new Vector2(-24f, -8f);
            var text = textObject.GetComponent<Text>();
#if UNITY_2022_2_OR_NEWER
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = 40;
            text.raycastTarget = false;
        }
        athleteToast.GetComponentInChildren<Text>().text = message;
        athleteToast.transform.SetAsLastSibling();
        athleteToast.SetActive(true);
        StopCoroutine("HideAthleteToast");
        StartCoroutine("HideAthleteToast");
    }

    private void HideAthleteToastNow()
    {
        StopCoroutine("HideAthleteToast");
        if (athleteToast != null) athleteToast.SetActive(false);
    }

    private IEnumerator HideAthleteToast()
    {
        yield return new WaitForSecondsRealtime(4f);
        if (athleteToast != null) athleteToast.SetActive(false);
    }

    public void GoToChallengeLevel()
    {
        GoToChallengeGame();
    }

    public void ChooseGameAndLevel(string game,int tier) {
        if(game!="Pairs" && game!="Boxes" && game!="Flash" && game!="Chain")return;
        if(tier!=6 && tier!=12 && tier!=18 && tier!=MemoryAthlete.Level)return;
        if(tier==MemoryAthlete.Level && !MemoryAthlete.Unlocked){ShowAthleteToast(MemoryAthlete.LockedHint);return;}
        if(UserConstants.s_challengeUser==null)return;
        SprintLobby.Attach(this).Create(game,tier,UserConstants.s_challengeUser.id);
    }

    public void GoToNotification() { SprintLobby.Attach(this).Open(); }

    public void GoToLegacyNotification() {
        HideAthleteToastNow();
        DeleteNotification();
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeGameScreen.SetActive(false);
        challengeHistoryScreen.SetActive(false);
        _loadingScreen.StartLoading();
        challengeNotificationScreen.SetActive(true);
        challengeAPIController.GetChallenges(
            (OnSuccess) =>
                {
                    SetNotifications(OnSuccess);
                    _loadingScreen.EndLoadingWithDelay();
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
        );
    }
    
    public void GoToHistory() { SprintLobby.Attach(this).Open(); }

    public void GoToLegacyHistory() {
        HideAthleteToastNow();
        DeleteHistory();
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeGameScreen.SetActive(false);
        challengeNotificationScreen.SetActive(false);
        _loadingScreen.StartLoading();
        challengeHistoryScreen.SetActive(true);
        challengeAPIController.GetHistory(
            (OnSuccess) =>
                {
                    print(OnSuccess);
                    SetHistories(OnSuccess);
                    _loadingScreen.EndLoadingWithDelay();
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    print("fail");
                }
        );
    }

    private int profileRequest;
    public void goToUserProfileScreen(User user, bool isFavorite)
    {
        challengeHomeScreen.SetActive(false);
        challengeRandomScreen.SetActive(false);
        challengeGameScreen.SetActive(false);
        challengeNotificationScreen.SetActive(false);
        challengeHistoryScreen.SetActive(false);
        challengeUserProfileScreen.SetActive(true);

        Text[] texts = challengeUserProfileScreen.GetComponentsInChildren<Text>();
        Image[] images = challengeUserProfileScreen.GetComponentsInChildren<Image>();
        images[2].sprite = Resources.Load<Sprite>("Memotopia_UI/AccountAndSettings/" + user.Avatar);
        texts[0].text = user.Username;
        texts[4].text = user.userStatistics.level.ToString();
        texts[5].text = user.userStatistics.xp.ToString();
        texts[6].text = "…";
        int request=++profileRequest;
        var achievements=GetComponent<AchievementsAPIController>() ?? gameObject.AddComponent<AchievementsAPIController>();
        achievements.GetProfileCount(user.ID, count=> {
            if(this!=null && request==profileRequest && challengeUserProfileScreen.activeInHierarchy)texts[6].text=count.ToString();
        }, error=> {
            if(this!=null && request==profileRequest && challengeUserProfileScreen.activeInHierarchy)texts[6].text="—";
        });

        challengeOnProfile.onClick.AddListener(() => {
            UserConstants.s_challengeUser = user;
            GoToChallengeGame();
        });

        deleteFavorite.onClick.AddListener(() =>
        {
            DeleteFavorite(user.ID.ToString());
            addFavorite.gameObject.SetActive(true);
            deleteFavorite.gameObject.SetActive(false);
        });

        addFavorite.onClick.AddListener(() =>
        {
            AddFavorite(user.ID.ToString(), addFavorite.gameObject);
            deleteFavorite.gameObject.SetActive(true);
        });

        if (isFavorite)
        {
            addFavorite.gameObject.SetActive(false);
            deleteFavorite.gameObject.SetActive(true);
        }
        else
        {
            deleteFavorite.gameObject.SetActive(false);
            addFavorite.gameObject.SetActive(true);
        }
    }

    //TODO: list win loss notification - talk to flamur

    // The bell at the top right of the Arena screen is a small button that other layers can end up
    // covering. Give it a generous invisible tap area that is the LAST (topmost) child of the screen
    // and opens the same notification list.
    private void EnsureBellHitArea()
    {
        if (challengeHomeScreen == null || challengeHomeScreen.transform.Find("BellHitArea") != null) return;
        var bell = challengeHomeScreen.transform.Find("ChallengesNotification") as RectTransform;
        if (bell == null) return;
        var go = new GameObject("BellHitArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(challengeHomeScreen.transform, false);
        rt.anchorMin = bell.anchorMin;
        rt.anchorMax = bell.anchorMax;
        rt.pivot = bell.pivot;
        rt.anchoredPosition = bell.anchoredPosition;
        // At least ~56 points on each side, centred on the bell.
        float scale = Mathf.Max(1f, Screen.dpi > 0 ? Screen.dpi / 163f : 2f);
        rt.sizeDelta = new Vector2(Mathf.Max(bell.sizeDelta.x, 0f) + 14f * scale, Mathf.Max(bell.sizeDelta.y, 0f) + 14f * scale);
        var image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(GoToNotification);
        rt.SetAsLastSibling();
    }

    // The red count badge sits on the corner of the bell/challenge button. It must never swallow
    // the tap that belongs to the button underneath.
    private static void LetTapsPassThrough(GameObject badge)
    {
        if (badge == null) return;
        foreach (var graphic in badge.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            graphic.raycastTarget = false;
    }

    private int legacyNotificationCount,sprintNotificationCount;
    public void SetSprintNotificationCount(int count) { sprintNotificationCount=count;UpdateNotificationBadge(); }
    public void SetNotificationNumber(GetChallengeResponsePayload obj) {
        legacyNotificationCount=obj.results.Length;UpdateNotificationBadge();
    }
    private void UpdateNotificationBadge() {
        int count=legacyNotificationCount+sprintNotificationCount;
        foreach(var badge in new[]{notificationNumber,notificationNumberOnMainScreen}) {
            if(badge==null)continue;LetTapsPassThrough(badge);badge.SetActive(count>0);
            foreach(var label in badge.GetComponentsInChildren<Text>(true))label.text=count.ToString();
        }
    }

    public void SetNotifications(GetChallengeResponsePayload obj) {
        for (int i=0; i < obj.results.Length; i++) {
            int challengeId = obj.results[i].id;
            int challengeLevel = obj.results[i].level;
            string gameName = obj.results[i].game.gameName;
            string userName = obj.results[i].participants[0].user.username;
            string imageName = obj.results[i].participants[0].user.avatar;
            float score = obj.results[i].participants[0].score;
            int challengerProgress = obj.results[i].participants[0].progress;
            GameObject item = Instantiate(challengeCart) as GameObject;
            item.SetActive(true);
            Text[] texts = item.GetComponentsInChildren<Text>();
            Button[] buttons = item.GetComponentsInChildren<Button>();
            Image[] images = item.GetComponentsInChildren<Image>();
            images[0].sprite = Resources.Load <Sprite>("Memotopia_UI/AccountAndSettings/"+imageName);
            texts[0].text = userName;
            texts[1].text = "Level " + obj.results[i].participants[0].user.userStatistics.level.ToString() + " / " + obj.results[i].participants[0].user.userStatistics.xp.ToString() + "xp";
            texts[1].color = Color.grey;
            texts[2].text = gameName;
            buttons[0].onClick.AddListener(() => StartChallenge(challengeId, challengeLevel, gameName, score, challengerProgress));
            buttons[1].onClick.AddListener(() => {
                challengeAPIController.UpdateChallenge(0, "CANCELED", challengeId,
                    (OnSuccess) =>
                    {
                        print("success");
                        DeleteNotification();

                        challengeAPIController.GetChallenges(
                            (OnSuccessgetChallenges) =>
                            {
                                SetNotifications(OnSuccessgetChallenges);
                                SetNotificationNumber(OnSuccessgetChallenges);
                            },
                            (OnFailure) =>
                            {
                                _loadingScreen.EndLoading();
                    print("fail");
                            }
                        );
                    },
                    (OnFailure) =>
                    {
                        _loadingScreen.EndLoading();
                    print("fail");
                    }
                );
            });
            item.transform.SetParent(notificationContent, false);
        }
    }

    public void SetHistories(GetChallengeHistoryResponsePayload obj) {
        for (int i = 0; i < obj.history.Length; i++)
        {
            History history = obj.history[i];
            GameObject item;
            Text[] texts;
            Button[] buttons;
            Image[] images;
            item = Instantiate(challengeHistoryCart) as GameObject;
            item.SetActive(true);
            texts = item.GetComponentsInChildren<Text>();
            buttons = item.GetComponentsInChildren<Button>();
            images = item.GetComponentsInChildren<Image>();
            images[0].sprite = Resources.Load<Sprite>("Memotopia_UI/AccountAndSettings/" + history.challenger.Avatar);
            texts[0].text = history.challenger.Username;
            texts[1].text = history.gameName;
            texts[2].text = history.winOrLose;

            if (history.winOrLose == "Draw")
            {
                texts[2].color = Color.yellow;
            }
            else if (history.winOrLose == "Loss")
            {
                texts[2].color = Color.red;
            }
            else if (history.winOrLose == "Win")
            {
                texts[2].color = Color.green;
            }
            else if (history.winOrLose == "Pending")
            {
                texts[2].color = Color.grey;
            }

            buttons[0].onClick.AddListener(() => {
                UserConstants.s_challengeUser = history.challenger;
                GoToChallengeGame();
            });
            item.transform.SetParent(historyContent, false);
        }
    }

    public void SetFavorites(UserSerializable obj)
    {
        DeleteFavorites();
        for (int i = 0; i < obj.users.Length; i++)
        {
            User user = obj.users[i];
            GameObject item = Instantiate(challengeFavoritesCart) as GameObject;
            item.SetActive(true);
            Text[] texts = item.GetComponentsInChildren<Text>();
            Button[] buttons = item.GetComponentsInChildren<Button>();
            Image[] images = item.GetComponentsInChildren<Image>();
            images[0].sprite = Resources.Load<Sprite>("Memotopia_UI/AccountAndSettings/" + obj.users[i].Avatar);
            texts[0].text = obj.users[i].Username;
            texts[1].text = "Level " + obj.users[i].userStatistics.level.ToString()  + " / " + obj.users[i].userStatistics.xp.ToString() + "xp";
            texts[1].color = Color.grey;
            buttons[0].onClick.AddListener(() => goToUserProfileScreen(user, true));
            buttons[1].onClick.AddListener(() => {
                UserConstants.s_challengeUser = user;
                GoToChallengeGame();
            });
            item.transform.SetParent(favoriteContent, false);
        }

        if (obj.users.Length >= 1)
        {
            NoFavoritesScreen.SetActive(false);
            FavoritesScrollView.SetActive(true);
        } else
        {
            FavoritesScrollView.SetActive(false);
            NoFavoritesScreen.SetActive(true);
        }
    }

    public void SetUserSearch(UserSerializable obj)
    {
        DeleteFavorites();
        for (int i = 0; i < obj.users.Length; i++)
        {
            User user = obj.users[i];
            GameObject item = Instantiate(challengeSearchCart) as GameObject;
            item.SetActive(true);
            Text[] texts = item.GetComponentsInChildren<Text>();
            Button[] buttons = item.GetComponentsInChildren<Button>();
            Image[] images = item.GetComponentsInChildren<Image>();
            images[0].sprite = Resources.Load<Sprite>("Memotopia_UI/AccountAndSettings/" + obj.users[i].Avatar);
            texts[0].text = obj.users[i].Username;
            texts[1].text = "Level " + obj.users[i].userStatistics.level.ToString() + " / " + obj.users[i].userStatistics.xp.ToString() + "xp";
            buttons[1].onClick.AddListener(() => {
                UserConstants.s_challengeUser = user;
                GoToChallengeGame();
            });
            bool isFavorite = false;

            for (int j = 0; j < UserConstants.s_favoriteUser.users.Length; j++)
            {
                if(UserConstants.s_favoriteUser.users[j].ID == obj.users[i].ID)
                {
                    buttons[2].gameObject.SetActive(false);
                    isFavorite = true;
                }
            }
            buttons[0].onClick.AddListener(() => goToUserProfileScreen(user, isFavorite));
            buttons[2].onClick.AddListener(() => AddFavorite(user.ID.ToString(), buttons[2].gameObject) );
            item.transform.SetParent(favoriteContent, false);
        }

        if (obj.users.Length >= 1)
        {
            NoFavoritesScreen.SetActive(false);
            FavoritesScrollView.SetActive(true);
        }
        else
        {
            FavoritesScrollView.SetActive(false);
            NoFavoritesScreen.SetActive(true);
        }
    }

    private void DeleteNotification() {
        foreach(Transform child in notificationContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void DeleteHistory() {
        foreach(Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void DeleteFavorites()
    {
        foreach (Transform child in favoriteContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetGame(string gameName) {
        this.gameName = gameName;
        GoToChallengeLevel();
    }

    public void SetLevel(int level) {
        this.level = level;
        // level te start duhet me kqyr
        StartChallenge(level, gameName, UserConstants.s_challengeUser.id);
    }

    IEnumerator Wait3000MillisecondToReturn()
    {
        yield return new WaitForSeconds(3F);
        // send to pick game
        GoToChallengeGame();
    }

    public void search()
    {
        if (searchInput.text.Trim().Length != 0)
            StepperDeboucer.Debouce(() => { SearchUsers(searchInput.text.Trim()); });
        else
            StepperDeboucer.Debouce(() => { SetFavorites(UserConstants.s_favoriteUser); });
    }
}
