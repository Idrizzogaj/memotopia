using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Editor-only checks. Run in the isolated Saga validation project, never against production accounts.
public static class SagaValidation
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static object Get(object source, string field) { return source.GetType().GetField(field, Flags).GetValue(source); }
    static void Call(object source, string method, params object[] args) { source.GetType().GetMethod(method, Flags).Invoke(source, args); }
    static void Check(bool value, string message) { if (!value) throw new Exception("Saga: " + message); }
    static ChainState State(ChainGame game) { return (ChainState)Get(game, "state"); }
    static void Recall(ChainGame game) { for (int i = 0; i < State(game).Sequence.Length; i++) game.NextCard(); }

    public static void Validate()
    {
        Check(Application.productName == "MemotopiaSagaValidation", "Use the isolated validation project.");
        var oldUser = UserConstants.s_user;
        bool oldChallenge = ChallengeConstants.s_isChallenge;
        UserConstants.s_user = new User { id = 1999999801, username = "Saga validation" };
        ChallengeConstants.s_isChallenge = false;
        try
        {
            for(int n=1;n<=20;n++){
                PlayerPrefs.SetInt("saga.chain.stars.v1.1999999801."+n,0);
                PlayerPrefs.SetInt("saga.chain.stars.v1.1999999802."+n,0);
            }
            Rules();
            EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
            var menu = Resources.FindObjectsOfTypeAll<MenuNavigation>().First(n => n.gameObject.scene.IsValid());
            var canvas = menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.position = Vector3.zero; canvas.transform.localScale = Vector3.one;
            var scaler = canvas.GetComponent<CanvasScaler>(); if (scaler != null) scaler.enabled = false;
            var canvasRect = (RectTransform)canvas.transform; canvasRect.sizeDelta = new Vector2(800, 1734);
            for (var t = menu.homeScreen.transform; t != canvas.transform; t = t.parent)
            {
                foreach (Transform sibling in t.parent) if (sibling != t) sibling.gameObject.SetActive(false);
                t.gameObject.SetActive(true);
            }
            var originals = menu.homeScreen.GetComponentsInChildren<Button>(true);
            var originalActive = originals.Select(b=>b.gameObject.activeSelf).ToArray();
            var originalCallbacks = originals.Select(b=>b.onClick).ToArray();
            if(menu.userName!=null)menu.userName.text="Saga preview";
            if(menu.level!=null)menu.level.text="1";
            if(menu.xp!=null)menu.xp.text="0";
            SagaPage.Attach(menu); SagaPage.Attach(menu);
            Check(originals.All(b => b != null && b.transform.IsChildOf(menu.homeScreen.transform)), "An existing menu button was removed.");
            Check(originals.Where((b,i)=>!ReferenceEquals(b.onClick,originalCallbacks[i])).Count()==0,"An existing menu callback changed.");
            Check(menu.homeScreen.transform.Cast<Transform>().Count(t => t.name == "Memotopia Saga") == 1, "Duplicate Saga tile.");
            foreach (string name in new[] { "Statistics", "Training", "Challenge", "Videos", "Tips" })
                Check(menu.homeScreen.transform.Find(name) != null, "Missing home tile " + name);
            var camera = new GameObject("Saga validation camera", typeof(Camera)).GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 867;
            camera.transform.position = new Vector3(0, 0, -20); camera.nearClipPlane = .1f; camera.farClipPlane = 1000;
            camera.backgroundColor = Color.white; camera.clearFlags = CameraClearFlags.SolidColor;
            canvas.worldCamera = camera;
            Capture(canvas, camera, 800, 1734, "saga-home-phone");
            menu.homeScreen.transform.Find("Memotopia Saga").GetComponent<Button>().onClick.Invoke();
            var page = menu.GetComponent<SagaPage>();
            var mapPage = (GameObject)Get(page, "page");
            Check(mapPage.activeSelf, "Saga tile did not open map.");
            var levels = mapPage.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("Saga level ")).ToArray();
            Check(levels.Length == 20 && levels.All(b => b.interactable), "All 20 development stops must be open.");
            Check(Resources.Load<Sprite>("Saga/Neuron") != null, "Neuron sprite not imported.");
            Capture(canvas, camera, 800, 1734, "saga-map-phone");
            canvasRect.sizeDelta = new Vector2(1200, 1600); camera.orthographicSize = 800;
            Capture(canvas, camera, 1200, 1600, "saga-map-tablet");
            canvasRect.sizeDelta = new Vector2(800, 1734); camera.orthographicSize = 867;
            levels.First(b => b.name == "Saga level 1").onClick.Invoke();
            var game = (ChainGame)Get(page, "chain");
            Check(game.IsSagaOpen && !mapPage.activeSelf, "Map did not start Saga Chain.");
            Check(State(game).Sequence.Length == 10 && State(game).Hearts == 3, "Level 1 count/hearts.");
            Capture(canvas, camera, 800, 1734, "saga-study-phone");
            canvasRect.sizeDelta=new Vector2(1200,1600);camera.orthographicSize=800;Canvas.ForceUpdateCanvases();
            game.StartLevel(1);
            var presentation=(ChainOriginalPresentation)Get(game,"originalGame");
            Check(presentation.Root.GetComponentsInChildren<Text>().Any(t=>t.name=="Exact study timer" && t.text=="90"),"Visible tablet study timer.");
            Capture(canvas,camera,1200,1600,"saga-study-tablet");
            canvasRect.sizeDelta=new Vector2(800,1734);camera.orthographicSize=867;Canvas.ForceUpdateCanvases();
            Gameplay(game, canvas, camera);
            game.ShowLevels(); Check(mapPage.activeSelf, "Return to map failed.");
            Check(ChainProgress.Mask == 0 && Enumerable.Range(1,20).All(n => ChainProgress.Stars(n) == 0), "Saga modified Training progress.");
            Check(!GameManager.PendingAchievements.Any(k => k.StartsWith("chain-")), "Saga granted Chain achievements.");
            Call(page, "Close");
            Check(originals.Where((b,i)=>b==null || b.gameObject.activeSelf!=originalActive[i]).Count()==0, "Existing home controls changed after Saga.");
            // Ordinary Chain still has five hearts, its original counts and its Practice behavior.
            CardFaces.Selected = CardFaces.Choice.Images; MemorySettings.Practice = true;
            var trainingHost = new GameObject("Training validation", typeof(RectTransform)); trainingHost.transform.SetParent(canvas.transform, false);
            var training = trainingHost.AddComponent<ChainGame>(); training.Open(); training.StartLevel(1);
            Check(State(training).Hearts == 5 && State(training).Sequence.Length == ChainState.CountForLevel(1), "Ordinary Chain rules changed.");
            Recall(training); Call(training, "TickTrainingClock", 1000f);
            Check(!State(training).Finished, "Training Practice timer changed."); Call(training, "Close");
            Debug.Log("SAGA_VALIDATION_PASSED: 20 configurations, original menu controls, neuron map, 3 hearts, image-only timed Chain, shuffles, last-ten precedence, timeout, replay, next, local progress, account isolation, unchanged Training.");
        }
        finally { CardFaces.EndSession(); UserConstants.s_user = oldUser; ChallengeConstants.s_isChallenge = oldChallenge; }
    }

    static void Rules()
    {
        int[] seconds = {90,88,86,84,80,80,78,76,74,72,70,68,66,64,62,60,58,56,54,52};
        for (int n = 1; n <= 20; n++)
        {
            var level = new SagaLevel(n);
            Check(level.Seconds == seconds[n-1] && level.ImageCount == 10 + (n-1)/5*5, "Configuration " + n);
            Check(level.TimedShuffles == (n-1)/5, "Shuffle count " + n);
        }
        var six = new SagaLevel(6);
        Check(!six.TimedShuffleDue(39.9f,0,15) && six.TimedShuffleDue(40,0,15), "One shuffle at halfway.");
        var eleven = new SagaLevel(11);
        Check(eleven.TimedShuffleDue(24,0,20) && eleven.TimedShuffleDue(47,1,20) && !eleven.TimedShuffleDue(69,2,20), "Two timed shuffles.");
        var sixteen = new SagaLevel(16);
        Check(sixteen.TimedShuffleDue(15,0,25) && sixteen.TimedShuffleDue(30,1,25) && sixteen.TimedShuffleDue(45,2,25), "Three timed shuffles.");
        Check(!sixteen.TimedShuffleDue(59,0,10) && sixteen.ShuffleAfterAnswer(10) && sixteen.ShuffleAfterAnswer(1) && !sixteen.ShuffleAfterAnswer(0), "Last-ten override.");
        bool rejected = false; try { new SagaLevel(21); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Level 21 must not exist.");
    }

    static void Gameplay(ChainGame game, Canvas canvas, Camera camera)
    {
        var mapClosed=(Action)Get(game,"sagaClosed");
        MemorySettings.Practice = true; CardFaces.Selected = CardFaces.Choice.Deck;
        game.StartLevel(6);
        Check(CardFaces.ContentCount == MajorSystemImages.Count && CardFaces.Selected == CardFaces.Choice.Deck, "Saga must use pictures without changing Training choice.");
        Check(Get(game,"studySeconds").Equals(80f), "Study budget.");
        Call(game,"TickSagaStudyClock",80f); Check((bool)Get(game,"recall"), "Study timeout did not start recall.");
        Check(Get(game,"recallSeconds").Equals(80f), "Recall needs its own full budget.");
        var state = State(game); var sequence = (int[])state.Sequence.Clone();
        var tiles = (System.Collections.Generic.List<Button>)Get(game,"tiles");
        var initialPositions = tiles.Select(t=>t.transform.GetSiblingIndex()).ToArray();
        Call(game,"TickTrainingClock",40f);
        Check((int)Get(game,"sagaShuffles")==1 && tiles.Where((t,i)=>t.transform.GetSiblingIndex()==initialPositions[i]).Count()==0, "Recall tiles must move.");
        Check(sequence.SequenceEqual(state.Sequence) && state.Position==0 && state.Hearts==3, "Shuffle changed the answer or hearts.");
        Capture(canvas,camera,800,1734,"saga-recall-phone");
        game.Choose(sequence[1]); Check(state.Hearts==2 && state.Position==0,"Wrong answer must cost one heart.");
        foreach(int id in sequence)game.Choose(id);
        Check(state.Won && SagaProgress.Stars(6)==2,"Saga win/stars not saved.");
        Capture(canvas,camera,800,1734,"saga-win-phone");
        var result=(GameObject)Get(game,"resultPanel");
        result.transform.GetChild(0).GetChild(12).GetComponent<Button>().onClick.Invoke();
        Check(((SagaLevel)Get(game,"saga")).Number==7 && State(game).Hearts==3,"NEXT LEVEL must stay in Saga.");
        game.StartLevel(11);Recall(game);Call(game,"TickTrainingClock",24f);Call(game,"TickTrainingClock",23f);
        Check((int)Get(game,"sagaShuffles")==2,"Level 11 needs two timed shuffles.");
        game.StartLevel(16);Recall(game);state=State(game);
        foreach(int id in state.Sequence.Take(15).ToArray())game.Choose(id);
        Check(state.Position==15 && (int)Get(game,"sagaShuffles")==0,"Fast answers must not force the timed shuffles.");
        tiles=(System.Collections.Generic.List<Button>)Get(game,"tiles");
        initialPositions=tiles.Select(t=>t.transform.GetSiblingIndex()).ToArray();
        Call(game,"TickTrainingClock",46f);
        Check((int)Get(game,"sagaShuffles")==0 && tiles.Select(t=>t.transform.GetSiblingIndex()).SequenceEqual(initialPositions),"No timed shuffle after last ten begins.");
        game.Choose(state.Sequence[15]);
        Check(tiles.Where((t,i)=>t.transform.GetSiblingIndex()==initialPositions[i]).Count()==0,"Each correct last-ten answer must move the cells.");
        initialPositions=tiles.Select(t=>t.transform.GetSiblingIndex()).ToArray();
        game.Choose(state.Sequence[17]);
        Check(state.Hearts==2 && tiles.Select(t=>t.transform.GetSiblingIndex()).SequenceEqual(initialPositions),"Wrong answer must not shuffle.");
        game.StartLevel(16);Recall(game);Call(game,"TickTrainingClock",15f);Call(game,"TickTrainingClock",15f);Call(game,"TickTrainingClock",15f);
        Check((int)Get(game,"sagaShuffles")==3,"Three timed shuffles while more than ten remain.");
        game.StartLevel(1);Recall(game);state=State(game);
        game.Choose(state.Sequence[1]);game.Choose(state.Sequence[1]);game.Choose(state.Sequence[1]);
        Check(state.Finished && !state.Won && state.Hearts==0,"Three errors must lose.");
        result=(GameObject)Get(game,"resultPanel");result.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.Invoke();
        Check(State(game).Hearts==3 && State(game).Sequence.Length==10,"Replay must reset the round.");
        Call(game,"Back"); Check(Get(game,"quitDialog")!=null,"Quit confirmation missing.");
        float seconds=(float)Get(game,"studySeconds");Call(game,"TickSagaStudyClock",10f);
        Check((float)Get(game,"studySeconds")==seconds,"Study timer must pause at quit confirmation.");
        var dialog=(GameObject)Get(game,"quitDialog");dialog.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.Invoke();
        Call(game,"TickSagaStudyClock",90f);state=State(game);Call(game,"TickTrainingClock",91f);
        Check(state.TimedOut && !state.Won && SagaProgress.Stars(1)==0,"Recall timeout must lose without progress.");
        game.StartLevel(20);Recall(game);state=State(game);foreach(int id in state.Sequence)game.Choose(id);
        result=(GameObject)Get(game,"resultPanel");
        Check(result.transform.GetChild(0).GetChild(12).GetComponentInChildren<Text>().text=="LEVELS","Level 20 must not offer 21.");
        game.StartLevel(5);Check((float)Get(game,"studySeconds")==80f,"Level 5 exception.");
        var originalUser=UserConstants.s_user;UserConstants.s_user=new User{id=1999999802};
        game.Choose(State(game).Sequence[0]);
        Check(!game.IsSagaOpen && SagaProgress.Stars(6)==0,"Account change must abort without credit.");
        UserConstants.s_user=originalUser;
        // Restart so the caller can check the normal map-return callback.
        game.OpenSaga(1,mapClosed);
    }

    static void Capture(Canvas canvas, Camera camera, int width, int height, string name)
    {
        foreach(var child in canvas.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
        camera.cullingMask=1<<30;
        foreach(var graphic in canvas.GetComponentsInChildren<Graphic>(true))graphic.SetAllDirty();
        Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(width,height,24);camera.targetTexture=target;camera.Render();
        var prior=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
        if(name.StartsWith("saga-map-"))
            Check(texture.GetPixels32().Count(p=>p.r>200 && p.g>140 && p.g<250 && p.b<180)>200,"Axons/myelin missing from map render.");
        string path=Environment.GetEnvironmentVariable("SAGA_REVIEW_OUTPUT");
        if(string.IsNullOrEmpty(path))path="../SagaReview";
        Directory.CreateDirectory(path);File.WriteAllBytes(Path.Combine(path,name+".png"),texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
    }
}
