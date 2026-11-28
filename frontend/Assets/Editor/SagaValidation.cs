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
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static object Get(object source, string field) { return source.GetType().GetField(field, Flags).GetValue(source); }
    static void Call(object source, string method, params object[] args) { source.GetType().GetMethod(method, Flags).Invoke(source, args); }
    static void Check(bool value, string message) { if (!value) throw new Exception("Saga: " + message); }
    static ChainState State(ChainGame game) { return (ChainState)Get(game, "state"); }
    static void Recall(ChainGame game) {
        var flash=(SagaFlashStudy)Get(game,"flashStudy");
        if(flash!=null){int guard=0;while(Get(game,"flashStudy")!=null && guard++<60){var cell=flash.CurrentCell;cell.HideImage();cell.AnimationFinished();}return;}
        if(Get(game,"boxesBoard")!=null){Call(game,"ShowRecall");return;}
        if(Get(game,"sortBoard")!=null){Call(game,"TickSagaStudyClock",(float)Get(game,"studySeconds"));return;}
        int count=0;while(!(bool)Get(game,"recall") && count++<60)game.NextCard();
    }

    public static void Validate()
    {
        Check(Application.productName == "MemotopiaSagaValidation", "Use the isolated validation project.");
        var oldUser = UserConstants.s_user;
        bool oldChallenge = ChallengeConstants.s_isChallenge;
        UserConstants.s_user = new User { id = 1999999801, username = "Saga validation" };
        ChallengeConstants.s_isChallenge = false;
        try
        {
            foreach(SagaGame kind in Enum.GetValues(typeof(SagaGame)))for(int n=1;n<=20;n++){
                PlayerPrefs.SetInt("saga."+kind.ToString().ToLowerInvariant()+".stars.v1.1999999801."+n,0);
                PlayerPrefs.SetInt("saga."+kind.ToString().ToLowerInvariant()+".stars.v1.1999999802."+n,0);
            }
            for(int n=1;n<=20;n++){
                PlayerPrefs.SetInt("saga.chain.stars.v1.1999999801."+n,0);
                PlayerPrefs.SetInt("saga.chain.stars.v1.1999999802."+n,0);
                PlayerPrefs.SetInt("saga.flash.stars.v1.1999999801."+n,0);
                PlayerPrefs.SetInt("saga.flash.stars.v1.1999999802."+n,0);
            }
            SagaProgress.Complete(SagaGame.Flash,1999999801,2,3);
            Check(SagaProgress.Stars(SagaGame.Flash,2)==3 && SagaProgress.Stars(SagaGame.Chain,2)==0,"Flash progress must not overwrite Chain.");
            SagaProgress.Complete(SagaGame.Chain,1999999801,3,2);
            Check(SagaProgress.Stars(SagaGame.Chain,3)==2 && SagaProgress.Stars(SagaGame.Flash,3)==0,"Chain progress must not overwrite Flash.");
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
            Check(levels.Length == SagaRoute.Count && levels.All(b => b.interactable) && Enumerable.Range(1,SagaRoute.Count).All(SagaRoute.Unlocked), "All development stops must be open.");
            var scroll=(ScrollRect)Get(page,"mapScroll");
            Check(menu.statisticsBackground.rectTransform.anchorMax.y<menu.videosBackground.rectTransform.anchorMin.y && menu.statisticsBackground.rectTransform.anchorMax.y<menu.tipsBackground.rectTransform.anchorMin.y,"Statistics must be below Videos and Tips.");
            Check(!scroll.horizontal && scroll.vertical && scroll.verticalNormalizedPosition<.001f,"Map must start at bottom, then scroll upwards.");
            int sectionCount=Mathf.CeilToInt(SagaRoute.Count/20f);
            for(int block=0;block<sectionCount;block++){
                scroll.verticalNormalizedPosition=block/(float)(sectionCount-1);Canvas.ForceUpdateCanvases();
                for(int n=block*20+1;n<=block*20+20;n++)Check(levels.First(b=>b.name=="Saga level "+n).GetComponentInChildren<Text>().text==n.ToString(),"Map stop number must not restart at 1.");
                Capture(canvas,camera,800,1734,"saga-map-section-"+(block+1));
            }
            scroll.verticalNormalizedPosition=0;
            Check(Resources.Load<Sprite>("Saga/Neuron") != null, "Neuron sprite not imported.");
            foreach(var node in levels){
                int stop=int.Parse(node.name.Substring("Saga level ".Length));var kind=SagaRoute.At(stop).Game;
                bool boss=kind==SagaGame.Boss || kind==SagaGame.FocusBoss;
                Check((node.transform.Find("Boss label")!=null)==boss,"Only boss stops get the larger hit area and boss label.");
                Check(node.transform.Find("Boss marker")==null,"No redundant star on the boss cell.");
                if(boss)Check(node.transform.Find("Boss label").GetComponentInChildren<Text>().text=="BOSS","Boss label remains identifiable without color.");
            }
            Capture(canvas, camera, 800, 1734, "saga-map-phone");
            Capture(canvas,camera,800,1734,"saga-map-20-visible");
            int[] beforeBoss=SagaRoute.Games.Select(g=>SagaProgress.Stars(g,1)).ToArray();
            foreach(var kind in SagaRoute.Games)SagaProgress.Complete(kind,GameManager.AchievementUserId,1,3);
            Call(page,"RefreshProgress");Capture(canvas,camera,800,1734,"saga-map-boss-frontier-phone");
            for(int mode=0;mode<SagaRoute.Games.Length;mode++)PlayerPrefs.SetInt("saga."+SagaRoute.Games[mode].ToString().ToLowerInvariant()+".stars.v1.1999999801.1",beforeBoss[mode]);
            Call(page,"RefreshProgress");
            Check(!mapPage.GetComponentsInChildren<Text>(true).Any(t=>SagaRoute.Games.Any(g=>t.text.ToUpperInvariant().Contains(g.ToString().ToUpperInvariant()))),"No individual game captions on map.");
            var captions=mapPage.GetComponentsInChildren<Text>(true).Select(t=>t.text).ToArray();
            for(int tier=1;tier<=20;tier++){Check(captions.Count(t=>t=="Memory "+tier)==1 && captions.Count(t=>t=="Focus "+tier)==1,"One Memory/Focus block heading per tier.");}
            var gates=(System.Collections.Generic.Dictionary<int,RectTransform>)Get(page,"synapseGates");
            Check(gates.Count==39 && Resources.Load<Sprite>("Saga/SynapseGate")!=null,"Synapse gate at each inter-block boss connection.");
            var axons=mapPage.GetComponentsInChildren<SagaAxonGraphic>(true);Check(axons.Length==11,"Connect all twelve map sections with myelin.");
            foreach(var axon in axons)Check(axon.Source==Resources.Load<Sprite>("Saga/MapUnnumbered").texture && axon.UnitWidth>0 && Vector2.Distance(axon.From,axon.To)>axon.EndPadding*2,"Each section connection reuses approved map art and has a real path.");
            scroll.verticalNormalizedPosition=.5f/11;Capture(canvas,camera,800,1734,"saga-map-seam-20-21-phone");
            scroll.verticalNormalizedPosition=1.5f/11;Capture(canvas,camera,800,1734,"saga-map-seam-40-41-phone");
            scroll.verticalNormalizedPosition=2.5f/11;Capture(canvas,camera,800,1734,"saga-map-seam-60-61-boss-phone");
            scroll.verticalNormalizedPosition=0;
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
            FiveGames(page,game,canvas,camera);
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
            Debug.Log("SAGA_VALIDATION_PASSED: 240 stages, Memory/Focus blocks and bosses, original menu controls, neuron map, memory3hearts, image-only timed Chain, shuffles, last-ten precedence, timeout, replay, next, local progress, account isolation, unchanged Training.");
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
            var flash=new SagaLevel(SagaGame.Flash,n);
            Check(flash.Game==SagaGame.Flash && flash.Seconds==level.Seconds && flash.ImageCount==new[]{7,12,20,30}[(n-1)/5] && flash.TimedShuffles==level.TimedShuffles,"Approved Flash count groups and recall timing " + n);
        }
        var six = new SagaLevel(6);
        Check(!six.TimedShuffleDue(39.9f,0,15) && six.TimedShuffleDue(40,0,15), "One shuffle at halfway.");
        var eleven = new SagaLevel(11);
        Check(eleven.TimedShuffleDue(24,0,20) && eleven.TimedShuffleDue(47,1,20) && !eleven.TimedShuffleDue(69,2,20), "Two timed shuffles.");
        var sixteen = new SagaLevel(16);
        Check(sixteen.TimedShuffleDue(15,0,25) && sixteen.TimedShuffleDue(30,1,25) && sixteen.TimedShuffleDue(45,2,25), "Three timed shuffles.");
        Check(!sixteen.TimedShuffleDue(59,0,10) && sixteen.ShuffleAfterAnswer(10) && sixteen.ShuffleAfterAnswer(1) && !sixteen.ShuffleAfterAnswer(0), "Last-ten override.");
        Check(SagaRoute.Count==240,"Memory seven-stop block and Focus five-stop block, twenty tiers.");
        for(int stop=1;stop<=SagaRoute.Count;stop++){
            var stage=SagaRoute.At(stop);
            int position=(stop-1)%12;var expected=position<6?SagaRoute.Games[position]:position==6?SagaGame.Boss:position==11?SagaGame.FocusBoss:SagaRoute.FocusGames[position-7];
            Check(stage.Game==expected && stage.Number==(stop-1)/12+1,"Route order "+stop);
            Check(SagaRoute.Stop(stage.Game,stage.Number)==stop,"Stable game/level route identity.");
        }
        bool rejected = false; try { new SagaLevel(21); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Level 21 must not exist.");
        int[] sortSeconds={10,9,8,7,5,20,19,18,17,15,25,24,23,22,20,30,28,26,24,20};
        for(int n=1;n<=20;n++){
            var sort=new SagaLevel(SagaGame.Sort,n);Check(sort.StudySeconds==sortSeconds[n-1] && sort.ImageCount==5+5*((n-1)/5),"SORT configuration.");
            var boxes=new SagaLevel(SagaGame.Boxes,n);Check(boxes.ImageCount==5+5*((n-1)/5),"Approved Boxes five-level count groups.");
            var pairs=new SagaLevel(SagaGame.Pairs,n);Check(pairs.ImageCount==2*Mathf.Max(5,n) && pairs.Seconds==(n<=10?30:n<=15?45:60) && pairs.SharedStudyRecallClock,"Pairs approved count/shared clock.");
        }
        float now=0;int reports=0;var context=new SagaRoundContext(90,(win,lives)=>reports++,()=>now);
        now=17;Check(context.Remaining==73,"Boss clock consumes study and recall together.");now=91;Check(context.Remaining==0,"Boss clock expires.");context.Report(false,0);context.Report(true,3);Check(reports==1,"One boss round result only.");
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
        CheckResult(result,SagaRoute.Stop(SagaGame.Chain,6),true);
        Click(result.transform.GetChild(0).GetChild(12).GetComponent<Button>());
        Check(!game.IsSagaOpen,"MAP must close the completed Saga game.");
        game.OpenSaga(SagaGame.Chain,11,mapClosed);Recall(game);Call(game,"TickTrainingClock",24f);Call(game,"TickTrainingClock",23f);
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
        result=(GameObject)Get(game,"resultPanel");CheckResult(result,1,false);Click(result.transform.GetChild(0).GetChild(2).GetComponent<Button>());
        Check(State(game).Hearts==3 && State(game).Sequence.Length==10,"Replay must reset the round.");
        Call(game,"Back"); Check(Get(game,"quitDialog")!=null,"Quit confirmation missing.");
        float seconds=(float)Get(game,"studySeconds");Call(game,"TickSagaStudyClock",10f);
        Check((float)Get(game,"studySeconds")==seconds,"Study timer must pause at quit confirmation.");
        var dialog=(GameObject)Get(game,"quitDialog");dialog.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.Invoke();
        Call(game,"TickSagaStudyClock",90f);state=State(game);Call(game,"TickTrainingClock",91f);
        Check(state.TimedOut && !state.Won && SagaProgress.Stars(1)==0,"Recall timeout must lose without progress.");
        game.StartLevel(20);Recall(game);state=State(game);foreach(int id in state.Sequence)game.Choose(id);
        result=(GameObject)Get(game,"resultPanel");
        CheckResult(result,SagaRoute.Stop(SagaGame.Chain,20),true);
        game.ShowLevels();game.OpenSaga(SagaGame.Chain,5,mapClosed);Check((float)Get(game,"studySeconds")==80f,"Level 5 exception.");
        var originalUser=UserConstants.s_user;UserConstants.s_user=new User{id=1999999802};
        game.Choose(State(game).Sequence[0]);
        Check(!game.IsSagaOpen && SagaProgress.Stars(6)==0,"Account change must abort without credit.");
        UserConstants.s_user=originalUser;
        // Restart so the caller can check the normal map-return callback.
        game.OpenSaga(1,mapClosed);
    }


    static void FiveGames(SagaPage page,ChainGame game,Canvas canvas,Camera camera)
    {
        game.ShowLevels();
        var mapPage=(GameObject)Get(page,"page");
        float bossTime=120;
        game.OpenSaga(SagaGame.Pairs,1,()=>{},null,null,new SagaRoundContext(()=>bossTime,(won,lives)=>{}));
        Check(((ChainOriginalPresentation)Get(game,"originalGame")).Root.GetComponentsInChildren<Text>().Any(t=>t.name=="Exact study timer" && t.text=="120"),"The initial Pairs boss view must already show the boss clock, not30seconds.");
        Call(game,"TickSagaStudyClock",31f);
        Check(!State(game).Finished && !(bool)Get(game,"recall") && (float)Get(game,"studySeconds")==120,"Pairs cannot impose its ordinary 30s cap on the shared boss clock.");
        bossTime=97;Recall(game);
        Check((float)Get(game,"recallSeconds")==97,"Pairs boss recall keeps the same total clock.");game.Close();
        game.OpenSaga(SagaGame.Sort,1,()=>{},null,null,new SagaRoundContext(()=>bossTime,(won,lives)=>{}));
        Check((float)Get(game,"studySeconds")==5 && ((SagaSortBoard)Get(game,"sortBoard")).Root.GetComponentsInChildren<Text>().Any(t=>t.name=="Exact recall timer" && t.text=="97"),"SORT retains its short study phase but displays the same boss time from the first frame.");game.Close();
        foreach(var kind in SagaRoute.Games){
            foreach(int level in new[]{1,5,6,11,16,20}){
                int stop=SagaRoute.Stop(kind,level);
                Call(page,"PlayStage",stop);
                if(kind==SagaGame.Says){
                    var says=(SaysGame)Get(page,"says");
                    Check(says.IsSagaOpen && (int)Get(says,"hearts")==3,"Says entry and hearts.");
                    var sequence=(int[])Get(says,"sequence");Check(sequence.Length==new SagaLevel(kind,level).ImageCount,"Says count.");
                    says.BeginPlayback();says.Tick(2+sequence.Length*2.25f+.8f);says.Tick(1);
                    Check((bool)Get(says,"input"),"Says playback and RECALL transition.");
                    foreach(int digit in sequence)says.Choose(digit);
                    Check(SagaProgress.Stars(kind,level)==3,"Says progress.");
                    if(level==20){
                        var popup=(GameObject)Get(says,"popup");
                        CheckResult(popup,SagaRoute.Stop(SagaGame.Says,20),true);
                    }
                    says.Close();continue;
                }
                var config=new SagaLevel(kind,level);
                Check(State(game).Sequence.Length==config.ImageCount && State(game).Hearts==3,"Entry count/hearts "+kind+" "+level);
                if(kind==SagaGame.Flash){
                    var flash=(SagaFlashStudy)Get(game,"flashStudy");
                    Check(flash!=null && flash.CurrentCell!=null && flash.CurrentCell.GetComponent<Animator>().speed==1,"Native Flash speed.");
                    Call(game,"TickSagaStudyClock",1000f);Call(game,"TickTrainingClock",1000f);
                    Check(!(bool)Get(game,"recall") && !State(game).Finished,"Flash study must not time out.");
                    if(level==1)Capture(canvas,camera,800,1734,"saga-flash-study-phone");
                    Recall(game);Check(flash.ShownCount==config.ImageCount,"Flash must show every picture.");
                    Check((float)Get(game,"recallSeconds")==config.Seconds,"Flash gets a fresh recall budget.");
                    var clock=((ChainOriginalPresentation)Get(game,"originalGame")).Root.GetComponentsInChildren<Text>().First(t=>t.name=="Exact recall timer");
                    Canvas.ForceUpdateCanvases();
                    Check(clock.enabled && clock.text==config.Seconds.ToString() && clock.cachedTextGenerator.vertexCount>4,"Visible Flash recall digits.");
                    Call(game,"TickTrainingClock",1f);Check(clock.text==(config.Seconds-1).ToString(),"Flash recall digits count down.");
                    if(level==1)Capture(canvas,camera,800,1734,"saga-flash-recall-phone");
                }else {
                    if(kind==SagaGame.Pairs){Call(game,"TickSagaStudyClock",17f);Check((float)Get(game,"studySeconds")==new SagaLevel(kind,level).Seconds-17,"Pairs study consumes shared clock.");}
                    if(kind==SagaGame.Sort && level==1)Capture(canvas,camera,800,1734,"saga-sort-study-phone");
                    if(kind==SagaGame.Boxes){var board=(SagaBoxesBoard)Get(game,"boxesBoard");Check(board.Root.GetComponentsInChildren<Image>().Count(i=>i.name=="ImgPref" && i.enabled)==config.ImageCount,"Every Boxes picture must be visible during study.");}
                    Recall(game);
                    if(kind==SagaGame.Pairs)Check((float)Get(game,"recallSeconds")==new SagaLevel(kind,level).Seconds-17,"Pairs recall must not reset the clock.");
                }
                var state=State(game);var images=(int[])state.Sequence.Clone();
                if(kind==SagaGame.Pairs){
                    var board=(SagaPairsBoard)Get(game,"pairsBoard");
                    var visibleCards=board.Root.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Saga pair ")).ToArray();
                    Check(visibleCards.Length==config.ImageCount && visibleCards.All(b=>b.image.rectTransform.rect.width>100),"Pairs cards must be active and visible.");
                    if(level==1){
                        Capture(canvas,camera,800,1734,"saga-pairs-recall-phone");
                        for(int error=0;error<8;error++){board.Choose(images[0]);board.Choose(images[2]);board.Tick(.6f);}
                        Check(state.Hearts==3 && state.Position==0 && !state.Finished,"Saga Pairs mismatches must never cost hearts.");
                        Check(!board.Root.GetComponentsInChildren<Image>().Any(i=>i.name.StartsWith("Saga pair heart")),"Saga Pairs hides unused hearts.");
                    }
                    for(int i=0;i<images.Length;i+=2){board.Choose(images[i]);board.Choose(images[i+1]);}
                }else if(kind==SagaGame.Boxes){
                    var board=(SagaBoxesBoard)Get(game,"boxesBoard");
                    if(level==1)Capture(canvas,camera,800,1734,"saga-boxes-recall-phone");
                    for(int i=0;i<images.Length;i++)board.Place(images[i],i);
                }else if(kind==SagaGame.Sort){
                    var board=(SagaSortBoard)Get(game,"sortBoard");if(level==1)Capture(canvas,camera,800,1734,"saga-sort-recall-phone");
                    Check(!board.CurrentOrder.SequenceEqual(images),"SORT recall must be shuffled.");
                    Check(board.Root.GetComponentsInChildren<DragAndDropItem>().Length==images.Length,"SORT must use the same picture grid, no separate tray.");
                    int expectedHearts=3;
                    if(level==1){var scrambled=board.CurrentOrder;board.Submit();expectedHearts=2;Check(state.Hearts==2 && state.Position==0 && board.CurrentOrder.SequenceEqual(scrambled),"Wrong DONE costs exactly one heart and retains the puzzle.");Check((float)Get(board,"shake")>0,"Wrong DONE must shake.");board.Tick(.5f);}
                    for(int i=0;i<images.Length;i++)board.Place(images[i],i);
                    Check(!state.Won && state.Hearts==expectedHearts,"SORT swaps must not score before DONE.");
                    board.Submit();
                }else foreach(int id in images)game.Choose(id);
                Check(state.Won && SagaProgress.Stars(kind,level)>0,"Win and separate progress "+kind+" "+level);
                var result=(GameObject)Get(game,"resultPanel");
                CheckResult(result,stop,true);
                Click(result.transform.GetChild(0).GetChild(12).GetComponent<Button>());
                Check(mapPage.activeSelf && !game.IsSagaOpen,"MAP returns from "+kind+" "+level);
                Call(page,"PlayStage",stop+1);
                var next=SagaRoute.At(stop+1);
                if(next.Game==SagaGame.Boss){var boss=(SagaBossGame)Get(page,"boss");Check(boss.IsOpen,"SORT next opens boss.");if(level==1){
                    Capture(canvas,camera,800,1734,"saga-boss-start-phone");boss.StartBoss();Call(boss,"RoundFinished",true,3);
                    Check(boss.WaitingForNext,"Boss must pause between completed games.");
                    Capture(canvas,camera,800,1734,"saga-boss-transition-phone");
                    ((RectTransform)canvas.transform).sizeDelta=new Vector2(1200,1600);camera.orthographicSize=800;
                    Capture(canvas,camera,1200,1600,"saga-boss-transition-tablet");
                    ((RectTransform)canvas.transform).sizeDelta=new Vector2(800,1734);camera.orthographicSize=867;
                }boss.Close();}
                else if(next.Game==SagaGame.Says){
                    var says=(SaysGame)Get(page,"says");Check(says.IsSagaOpen,"Map entry must open Says.");says.Close();
                }else{
                    var active=(SagaLevel)Get(game,"saga");
                    Check(active.Game==next.Game && active.Number==next.Number,"Map follows route from "+kind+" "+level);game.ShowLevels();
                }
            }
        }
        // The newly approved Boxes count groups must fit the existing iPad boards too.
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(1200,1600);camera.orthographicSize=800;Canvas.ForceUpdateCanvases();
        foreach(int level in new[]{1,6,11,16,20}){
            int stop=SagaRoute.Stop(SagaGame.Boxes,level);Call(page,"PlayStage",stop);
            var board=(SagaBoxesBoard)Get(game,"boxesBoard");var state=State(game);
            Check(board.Root.name.StartsWith("BoxesSceneIpad") && board.Root.GetComponentsInChildren<Image>().Count(i=>i.name=="ImgPref" && i.enabled)==new SagaLevel(SagaGame.Boxes,level).ImageCount,"Tablet Boxes study count/layout.");
            if(level==1)Capture(canvas,camera,1200,1600,"saga-boxes-study-tablet");
            Recall(game);var images=(int[])state.Sequence.Clone();for(int i=0;i<images.Length;i++)board.Place(images[i],i);
            Check(state.Won,"Tablet Boxes layout is solvable.");var result=(GameObject)Get(game,"resultPanel");CheckResult(result,stop,true);Click(result.transform.GetChild(0).GetChild(12).GetComponent<Button>());
        }
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(800,1734);camera.orthographicSize=867;Canvas.ForceUpdateCanvases();
        // Restore a visible map for the caller's existing navigation assertions.
        Call(page,"ShowMap");
        mapPage.GetComponentsInChildren<Button>(true).First(b=>b.name=="Saga level 1").onClick.Invoke();
    }

    static void Click(Button button){
        Check(button.gameObject.activeInHierarchy && button.IsInteractable(),"Result button must accept a pointer click.");
        button.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(null){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left});
    }
    static void CheckResult(GameObject panel,int stop,bool won){
        var view=panel.GetComponent<SagaResultView>();
        Check(view!=null && view.Stop==stop && view.Won==won,"Shared result identity.");
        Check(panel.GetComponentsInChildren<Text>().Any(t=>t.text=="SAGA LEVEL "+stop),"Result uses global Saga number.");
        if(won)Check(panel.GetComponentsInChildren<Text>().Any(t=>t.text=="You won!"),"All Saga wins use the same victory caption.");
        var buttons=panel.GetComponentsInChildren<Button>();
        Check(buttons.Length==2 && buttons.All(b=>b.IsInteractable()),"REPLAY and MAP remain usable.");
    }

    static void Capture(Canvas canvas, Camera camera, int width, int height, string name)
    {
        foreach(var child in canvas.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
        camera.cullingMask=1<<30;
        foreach(var graphic in canvas.GetComponentsInChildren<Graphic>(true))graphic.SetAllDirty();
        Canvas.ForceUpdateCanvases();
        foreach(var sagaPage in Resources.FindObjectsOfTypeAll<SagaPage>().Where(p=>p.gameObject.scene.IsValid()))Call(sagaPage,"LayoutMap");
        Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(width,height,24);camera.targetTexture=target;camera.Render();
        var prior=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
        if(name=="saga-map-phone")
            Check(texture.GetPixels32().Count(p=>p.r>200 && p.g>140 && p.g<250 && p.b<180)>200,"Active neuron's myelin missing from map render.");
        if(name=="saga-pairs-recall-phone")Check(texture.GetPixels32().Count(p=>p.r<245 && p.r>215 && p.g>220 && p.b>220)>1000,"Pairs card backs missing from render.");
        string path=Environment.GetEnvironmentVariable("SAGA_REVIEW_OUTPUT");
        if(string.IsNullOrEmpty(path))path="../SagaReview";
        Directory.CreateDirectory(path);File.WriteAllBytes(Path.Combine(path,name+".png"),texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
    }
}
