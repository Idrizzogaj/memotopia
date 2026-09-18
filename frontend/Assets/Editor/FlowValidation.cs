using System;
using System.IO;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class FlowValidation
{
    private static void Require(bool condition, string reason) { if (!condition) throw new Exception("Flow validation: " + reason); }
    public static void Validate()
    {
        var user = UserConstants.s_user;
        const int id = 2147483590;
        PlayerPrefs.DeleteKey("levels.v1." + id); PlayerPrefs.DeleteKey("levels.v1." + (id - 1));
        try
        {
            UserConstants.s_user = new User { id = id };
            LevelProgress.EnsureUser();
            Require(LevelProgress.Get("Pairs") == null, "unfetched progress must not pretend to be empty");
            LevelProgress.Merge("Pairs", new[] { new GameLevel { id = 17, level = 2, score = 50, stars = 1 }, new GameLevel { id = 16, level = 1, score = 100, stars = 2 } });
            Require(LevelProgress.Get("Pairs")[0].level == 1, "level ordering");
            LevelProgress.Record("Pairs", 3, 3, 160);
            var sent = LevelProgress.Pending.Single();
            LevelProgress.Merge("Pairs", new GameLevel[0]);
            Require(LevelProgress.Get("Pairs").Length == 3 && LevelProgress.Pending.Count == 1, "stale GET must not erase a local win");
            LevelProgress.Record("Pairs", 3, 3, 170);
            LevelProgress.Confirm(sent);
            Require(LevelProgress.Pending.Count == 1, "in-flight response cannot clear a newer result");
            UserConstants.s_user = new User { id = id - 1 };
            Require(LevelProgress.Get("Pairs") == null && LevelProgress.Pending.Count == 0, "account isolation");
            UserConstants.s_user = new User { id = id };
            Require(LevelProgress.Pending.Single().score == 170, "restart retains pending score");
            LevelProgress.Confirm(LevelProgress.Pending.Single());
            Require(LevelProgress.Pending.Count == 0, "acknowledgement clears pending upload");
            LevelProgress.Record("Pairs", 3, 1, 20);
            Require(LevelProgress.Get("Pairs").Last().score == 170, "replay preserves best score");
        }
        finally
        {
            UserConstants.s_user = user; LevelProgress.EnsureUser();
            PlayerPrefs.DeleteKey("levels.v1." + id); PlayerPrefs.DeleteKey("levels.v1." + (id - 1)); PlayerPrefs.Save();
        }
        var safe = MobileSafeArea.Normalized(new Rect(0, 102, 1179, 2277), 1179, 2556);
        Require(safe.yMin > 0 && safe.yMax < 1 && safe.width == 1, "iPhone camera and home indicator insets");
        Require(MobileSafeArea.Normalized(new Rect(0, 0, 800, 600), 800, 600) == new Rect(0, 0, 1, 1), "no-inset screen unchanged");
        Require(MobileSafeArea.Normalized(Rect.zero, 0, 0) == new Rect(0, 0, 1, 1), "invalid screen fallback");
        Debug.Log("Flow validation passed: score outbox/restart/account isolation, level ordering, stale-response protection and safe-area bounds.");
        Require(SideMenuAvailability.IsAvailable("GoToAccount", false, false), "account remains active");
        Require(SideMenuAvailability.IsAvailable("Logout", false, false), "logout remains active");
        Require(!SideMenuAvailability.IsAvailable("FacebookShare", true, false), "missing Facebook integration disabled");
        Require(!SideMenuAvailability.IsAvailable("RateBtnClick", true, false), "unpublished review page disabled");
        Require(!SideMenuAvailability.IsAvailable("GoToPayment", false, false), "purchases wait for store initialization");
        Require(SideMenuAvailability.IsAvailable("GoToPayment", true, false), "available subscriptions enabled");
        Require(!SideMenuAvailability.IsAvailable("GoToPayment", true, true), "unimplemented iOS upgrade disabled");
        Require(MenuNavigation.AppStoreReviewUrl.EndsWith("?action=write-review"), "future public review deep link");
        var ranking = new[] {
            new UserStatistics { user = new User { id = 10, username = "Idriz" }, xp = 200 },
            new UserStatistics { user = new User { id = 20, username = "Hana" }, xp = 100 },
            new UserStatistics { user = new User { id = 30 }, xp = 50 }
        };
        Require(StatisticsView.RankingRows(ranking, 99, 10).SequenceEqual(new[] { 0, 1, 2 }), "missing account must not duplicate first place");
        Require(StatisticsView.RankingRows(ranking, 30, 2).SequenceEqual(new[] { 0, 2 }), "own lower rank retains actual index");
        Require(StatisticsView.RankingRows(new UserStatistics[0], 20, 10).Length == 0, "empty ranking hides placeholder rows");
        Require(StatisticsView.RankingRows(null, 20, 10).Length == 0, "unavailable ranking has no invented winner");
        Debug.Log("Unity Pro available: " + UnityEditorInternal.InternalEditorUtility.HasPro());
    }

    public static void Review()
    {
        Validate();
        Directory.CreateDirectory("../docs/game-flow/review");
        foreach (string scene in new[] { "GameMenu", "LevelsScene", "PairsScene", "BoxesScene", "FlashScene" })
        {
            EditorSceneManager.OpenScene("Assets/_Scenes/" + scene + ".unity");
            var canvas = Resources.FindObjectsOfTypeAll<Canvas>().First(c => c.gameObject.scene.IsValid() && c.name == "Canvas");
            var area = canvas.gameObject.AddComponent<MobileSafeArea>();
            area.Apply(new Rect(0, 102, 1179, 2277), 1179, 2556);
            Require(area.Content.childCount > 0, "safe-area content in " + scene);
            foreach (var tutorial in canvas.GetComponentsInChildren<TutorialController>(true)) tutorial.DisableTutorialObjects(true);
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true)) if (t.name == "HowToPlay") t.gameObject.SetActive(false);
            LevelScript._selectedLevel = 3;
            if (scene == "PairsScene")
            {
                var manager = UnityEngine.Object.FindObjectOfType<PairsManager>();
                manager.linkedPairsScreen.SetActive(true); manager.linkedPairsScreenIpad.SetActive(false); manager.gameScreen.SetActive(false); manager.rsg.SetActive(false);
                manager.LevelSelector(); manager.SetNames(); manager.NextPair(); manager.firstCountDown.text = "90";
            }
            if (scene == "BoxesScene")
            {
                var manager = UnityEngine.Object.FindObjectOfType<BoxesManager>();
                foreach (var panel in manager.levelPanels) panel.SetActive(false);
                manager.LevelSelector(); Invoke(manager, "InitArrays"); Invoke(manager, "SpawnImages");
                manager.rsg.SetActive(false); manager.countDownText.text = "90";
                var platform = canvas.GetComponentInChildren<MajorSystemPlatform>();
                var picture = platform.transform.parent.GetComponentInChildren<BoxesPicturePerspective>().GetComponent<Image>();
                int withShadow = PlatformVertices(platform);
                picture.enabled = false;
                int withoutShadow = PlatformVertices(platform);
                Require(withoutShadow > 12 && withShadow == withoutShadow + 25, "rounded platform retains geometry but removes contact shadow in recall");
                picture.enabled = true;
                picture.type = Image.Type.Filled; picture.fillAmount = 0;
                Require(PlatformVertices(platform) == withoutShadow, "dragged picture leaves no contact shadow");
                picture.type = Image.Type.Simple; picture.fillAmount = 1;
                Require(PlatformVertices(platform) == withShadow, "placed picture restores its shadow");
            }
            if (scene == "FlashScene")
            {
                var manager = UnityEngine.Object.FindObjectOfType<FlashManager>();
                manager.PositionHeader();
                manager.activeCellsPanel.SetActive(true); manager.deactiveCellsPanel.SetActive(true); manager.recallPanel.SetActive(false); manager.rsg.SetActive(false);
                Save(canvas, "Flash-presentation");
                manager.LevelSelector(); Invoke(manager, "InitArrays");
                var field = typeof(FlashManager).GetField("imagesNumberForRecall", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                int count = ((int[])field.GetValue(manager)).Length;
                field.SetValue(manager, Enumerable.Range(1, count).ToArray());
                manager.activeCellsPanel.SetActive(false); manager.deactiveCellsPanel.SetActive(false); manager.recallPanel.SetActive(true); manager.rsg.SetActive(false);
                Invoke(manager, "PopulateGamePanel"); Invoke(manager, "EnableHeartsPerLevel"); manager.timerText.text = "180";
            }
            if (scene == "LevelsScene")
            {
                StaticVar.s_game = "Boxes";
                LevelProgress.Merge("Boxes", Enumerable.Range(1, 9).Select(n => new GameLevel { level = n, stars = 3 }).ToArray());
                var loading = new GameObject("LoadingCanvas", typeof(LoadingManager));
                Invoke(UnityEngine.Object.FindObjectOfType<LevelScript>(), "Start");
                UnityEngine.Object.DestroyImmediate(loading);
            }
            if (scene == "GameMenu")
            {
                var home = area.Content.Find("Home");
                foreach (Transform child in area.Content) child.gameObject.SetActive(child == home);
            }
            Save(canvas, scene);
            if (scene == "BoxesScene")
            {
                area.Apply(new Rect(0, 0, 768, 1024), 768, 1024);
                Save(canvas, "Boxes-iPad", 1067);
            }
            if (scene == "GameMenu")
            {
                var menu = UnityEngine.Object.FindObjectOfType<MenuAnimations>();
                menu.OpenMenu();
                menu.homeScreen.transform.position = menu.openMenuHomePosition.transform.position;
                Save(canvas, "side-menu-open");
                var navigation = UnityEngine.Object.FindObjectOfType<MenuNavigation>();
                menu.sideMenuScreen.SetActive(false); menu.homeScreen.SetActive(false);
                navigation.accountScreen.SetActive(true);
                Save(canvas, "account-screen");
                navigation.accountScreen.SetActive(false); navigation.paymentScreen.SetActive(true);
                Save(canvas, "payment-screen");
            }
            if (scene == "PairsScene")
            {
                LevelScript._selectedLevel = 10;
                var host = new GameObject("Result review");
                host.AddComponent<GamesScript>().ActivateWinPanel("150", 3, Resources.Load<GameObject>("Prefabs/GamePopUps/WinPanel"), "PairsScene", "Pairs");
                area.Apply(new Rect(0, 102, 1179, 2277), 1179, 2556);
                var levels = area.Content.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "LevelsButton");
                Require(levels != null, "result has direct next level and separate levels button");
                Save(canvas, "result-next-level");
                UnityEngine.Object.DestroyImmediate(host);
            }
        }
        Debug.Log("Flow visual reviews saved for menu, levels, all games and result navigation.");
    }

    private static void Invoke(object target, string method)
    {
        target.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(target, null);
    }

    private static int PlatformVertices(MajorSystemPlatform platform)
    {
        using (var mesh = new VertexHelper())
        {
            typeof(MajorSystemPlatform).GetMethod("OnPopulateMesh", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(VertexHelper) }, null).Invoke(platform, new object[] { mesh });
            return mesh.currentVertCount;
        }
    }

    private static void Save(Canvas canvas, string name, int height = 1734)
    {
        var scaler = canvas.GetComponent<CanvasScaler>(); if (scaler != null) scaler.enabled = false;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.position = Vector3.zero; canvas.transform.localScale = Vector3.one;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(800, height);
        var cameraObject = new GameObject("Flow review camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = height / 2f;
        camera.transform.position = new Vector3(0, 0, -100); camera.cullingMask = 1 << 30;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(244, 249, 250, 255);
        var target = new RenderTexture(800, height, 24); camera.targetTexture = target; canvas.worldCamera = camera;
        foreach (var transform in canvas.GetComponentsInChildren<Transform>(true)) transform.gameObject.layer = 30;
        Canvas.ForceUpdateCanvases();
        foreach (var rect in canvas.GetComponentsInChildren<RectTransform>()) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(800, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 800, height), 0, 0); image.Apply();
        File.WriteAllBytes("../docs/game-flow/review/" + name + ".png", image.EncodeToPNG());
        RenderTexture.active = previous;
        UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(target);
    }

    public static void ReviewAndBuild()
    {
        AchievementValidation.Review();
        Review();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MemotopiaBuild.BuildIOS();
    }
}
