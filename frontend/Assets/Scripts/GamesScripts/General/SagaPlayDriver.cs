#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Isolated editor play-mode validation, excluded from all player builds.
public sealed class SagaPlayDriver : MonoBehaviour
{
    const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    float started;
    float lastReport;
    ChainGame current;
    [SerializeField] bool mapOnly;
    [SerializeField] bool bossOnly;
    [SerializeField] bool focusBossOnly;
    static object Get(object source,string field){return source.GetType().GetField(field,Flags).GetValue(source);}
    static void Check(bool okay,string message){if(!okay)throw new Exception("Saga play: "+message);}
    public static void Begin(){Create(false);}
    public static void BeginMap(){Create(true);}
    public static void BeginBoss(){Create(false,true);}
    public static void BeginFocusBoss(){Create(false,false,true);}
    static void Create(bool map,bool boss=false,bool focusBoss=false){
        if(Application.productName!="MemotopiaSagaValidation")throw new Exception("Use isolated validation project.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        var driver=new GameObject("Saga runtime driver").AddComponent<SagaPlayDriver>();driver.mapOnly=map;driver.bossOnly=boss;driver.focusBossOnly=focusBoss;
        EditorApplication.EnterPlaymode();
    }
    void Update(){
        if(current!=null && Time.realtimeSinceStartup-lastReport>10){lastReport=Time.realtimeSinceStartup;var study=(SagaFlashStudy)Get(current,"flashStudy");var cell=study==null?null:study.CurrentCell;var animator=cell==null?null:cell.GetComponent<Animator>();Debug.Log("SAGA_PLAY_TICK: scale="+Time.timeScale+" recall="+Get(current,"recall")+" study="+(study==null?"none":study.ShownCount.ToString())+" active="+(cell!=null && cell.gameObject.activeInHierarchy)+" state="+(animator==null?"none":animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString()));}
        if(started>0 && Time.realtimeSinceStartup-started>140){Debug.LogError("Saga play validation timed out.");EditorApplication.Exit(1);}
    }
    void Error(string message,string trace,LogType type){if(type==LogType.Error || type==LogType.Exception){Debug.Log("SAGA_PLAY_FAILED: "+message+"\n"+trace);EditorApplication.Exit(1);}}
    IEnumerator Start(){
        // Dependency resolver updates must not reload the domain mid-test and
        // discard the running game/coroutines in this isolated batch session.
        EditorApplication.LockReloadAssemblies();
        AssetDatabase.DisallowAutoRefresh();
        started=Time.realtimeSinceStartup;Application.runInBackground=true;
        Application.logMessageReceived+=Error;
        UserConstants.s_user=new User{id=1999999807,username="Runtime validation"};ChallengeConstants.s_isChallenge=false;
        var root=new GameObject("Saga runtime canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(800,1734);
        if(mapOnly){yield return ValidateMap(root.transform);EditorApplication.Exit(0);yield break;}
        if(bossOnly){yield return ValidateBoss(root.transform);EditorApplication.Exit(0);yield break;}
        if(focusBossOnly){yield return ValidateFocusBoss(root.transform);EditorApplication.Exit(0);yield break;}
        var host=new GameObject("Runtime game",typeof(RectTransform));host.transform.SetParent(root.transform,false);
        var game=host.AddComponent<ChainGame>();current=game;
        Debug.Log("SAGA_PLAY_STARTED");
        var oldShow=CellController.onShowImage;var oldHide=CellController.afterShownImage;var oldFinish=CellController.onAnimationFinished;
        game.OpenSaga(SagaGame.Flash,20,()=>{});
        SagaFlashStudy study=null;
        while(study==null){study=(SagaFlashStudy)Get(game,"flashStudy");yield return null;}
        int[] sequence=(int[])((ChainState)Get(game,"state")).Sequence.Clone();
        while(!(bool)Get(game,"recall"))yield return null;
        Check(study.ShownCount==32 && study.ShownImages.SequenceEqual(sequence),"Original Flash animation must display all 32 pictures in order.");
        Check((float)Get(game,"recallSeconds")>49 && (float)Get(game,"recallSeconds")<=52,"Recall countdown starts after the entire sequence.");
        Check(CellController.onShowImage==oldShow && CellController.afterShownImage==oldHide && CellController.onAnimationFinished==oldFinish,"Flash global event handlers changed.");
        foreach(int id in sequence)game.Choose(id);
        Check(((ChainState)Get(game,"state")).Won,"Flash win.");game.ShowLevels();
        game.OpenSaga(SagaGame.Boxes,1,()=>{});
        while(Get(game,"boxesBoard")==null)yield return null;
        var board=(SagaBoxesBoard)Get(game,"boxesBoard");
        var layout=(SagaBoxesLayout)Get(board,"layout");layout.RecallButton.GetComponent<Button>().onClick.Invoke();
        var state=(ChainState)Get(game,"state");sequence=(int[])state.Sequence.Clone();
        var system=new GameObject("Runtime events",typeof(EventSystem),typeof(StandaloneInputModule)).GetComponent<EventSystem>();
        for(int i=0;i<sequence.Length;i++){
            var item=board.Root.GetComponentsInChildren<DragAndDropItem>(true).First(d=>d.name==sequence[i].ToString());
            var data=new PointerEventData(system){position=new Vector2(100,400),pressPosition=new Vector2(100,100),pointerDrag=item.gameObject};
            item.OnPointerDown(data);item.OnBeginDrag(data);
            Check(DragAndDropItem.draggedItem==item,"Native tray item did not begin dragging.");
            var drop=board.Root.GetComponentsInChildren<SagaBoxesDrop>(true).First(d=>d.Slot==i);
            drop.OnDrop(data);item.OnEndDrag(data);yield return null;
        }
        Check(state.Won && sequence.Length==5,"Native Boxes level1 must use five images and solve by drag/drop.");
        var result=(GameObject)Get(game,"resultPanel");
        Check(result.GetComponent<SagaResultView>().Stop==4 && result.GetComponentsInChildren<Text>().Any(t=>t.text=="SAGA LEVEL 4"),"Boxes result global Saga level4.");
        var mapButton=result.transform.GetChild(0).GetChild(12).GetComponent<Button>();
        Check(mapButton.IsInteractable(),"Boxes MAP button enabled.");mapButton.OnPointerClick(new PointerEventData(system));
        Check(!game.IsSagaOpen,"Boxes MAP pointer click closes result.");
        Debug.Log("SAGA_PLAY_PASSED: original full-speed Flash animation, all 32 pictures before recall, untouched static events, native Boxes drag/drop.");
        EditorApplication.Exit(0);
    }
    IEnumerator ValidateMap(Transform canvas){
        UserConstants.s_user=new User{id=1999999818,username="Map runtime validation"};
        for(int n=1;n<=SagaRoute.Count;n++){var stage=SagaRoute.At(n);PlayerPrefs.SetInt("saga."+stage.Game.ToString().ToLowerInvariant()+".stars.v1.1999999818."+stage.Number,n<20?3:0);}
        int oldMotion=PlayerPrefs.GetInt("saga.motion.v1",1);PlayerPrefs.SetInt("saga.motion.v1",1);
        // Match GameMenu: MenuNavigation is a scene root, outside the Canvas.
        var menuObject=new GameObject("Isolated menu",typeof(RectTransform));
        var menu=menuObject.AddComponent<MenuNavigation>();menu.enabled=false;
        menu.homeScreen=ChainGame.Panel(canvas,"Home",Vector2.zero,Vector2.one,Color.white);
        menu.trainingBackground=ChainGame.Panel(menu.homeScreen.transform,"Training",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
        menu.challengeBackground=ChainGame.Panel(menu.homeScreen.transform,"Challenge",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
        menu.statisticsBackground=ChainGame.Panel(menu.homeScreen.transform,"Statistics",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
        menu.videosBackground=ChainGame.Panel(menu.homeScreen.transform,"Videos",Vector2.zero,new Vector2(.48f,1),Color.white).GetComponent<Image>();
        menu.tipsBackground=ChainGame.Panel(menu.homeScreen.transform,"Tips",new Vector2(.52f,0),Vector2.one,Color.white).GetComponent<Image>();
        SagaPage.Attach(menu);menu.homeScreen.transform.Find("Memotopia Saga").GetComponent<Button>().onClick.Invoke();
        var page=menu.GetComponent<SagaPage>();var map=(GameObject)Get(page,"page");var scroll=(ScrollRect)Get(page,"mapScroll");
        Check(map.GetComponentsInChildren<Button>(true).Count(b=>b.name.StartsWith("Saga level "))==SagaRoute.Count,"All map buttons.");
        var themes=(System.Collections.Generic.List<Material>)Get(page,"mapThemes");
        Check(themes[0].GetVectorArray("_Cells")[19].z==1 && themes[1].GetVectorArray("_Cells")[0].z==0,"Only completed/current cells are colored.");
        page.GetType().GetMethod("PlayStage",Flags).Invoke(page,new object[]{20});
        var focus=(FocusGame)Get(page,"focus");focus.Begin();yield return SolveFocus(focus);
        bool sawPulse=false;float limit=Time.realtimeSinceStartup+10;
        while(!map.activeSelf || Get(page,"victory")!=null){sawPulse|=Get(page,"pulse")!=null;Check(Time.realtimeSinceStartup<limit,"Auto return/unlock timeout.");yield return null;}
        Check(sawPulse && Mathf.Abs(scroll.verticalNormalizedPosition-1f/11)<.002f,"20 to 21 scroll and electrical reveal.");
        Check(themes[1].GetVectorArray("_Cells")[0].z==1 && themes[1].GetVectorArray("_Cells")[1].z==0,"Next cell must be colored after unlock, future cell remains grey.");
        page.GetType().GetField("nextAmbientAt",Flags).SetValue(page,Time.unscaledTime-1);yield return null;yield return null;
        Check((int)Get(page,"ambientStop")>0,"Spontaneous visible-cell animation did not start.");
        page.GetType().GetMethod("ToggleMotion",Flags).Invoke(page,null);yield return null;
        Check((int)Get(page,"ambientStop")==0 && Get(page,"ambientPulse")==null,"Motion off cancels spontaneous effects.");
        page.GetType().GetMethod("PlayStage",Flags).Invoke(page,new object[]{21});focus.Begin();yield return SolveFocus(focus);
        limit=Time.realtimeSinceStartup+8;
        while(!map.activeSelf || Get(page,"victory")!=null){Check(Get(page,"pulse")==null,"Motion off must suppress unlock pulse.");Check(Time.realtimeSinceStartup<limit,"Reduced-motion return timeout.");yield return null;}
        Check(themes[1].GetVectorArray("_Cells")[1].z==1,"Reduced-motion completion still unlocks next stage.");
        // A manual replay must cancel the completed round's pending auto-return.
        page.GetType().GetMethod("PlayStage",Flags).Invoke(page,new object[]{8});focus.Begin();yield return SolveFocus(focus);
        var result=(GameObject)Get(focus,"popup");var view=result.GetComponent<SagaResultView>();
        Check(view.Stop==8 && result.GetComponentsInChildren<Text>().Any(t=>t.text=="SAGA LEVEL 8"),"Focus shared Saga result title.");
        var replay=result.transform.GetChild(0).GetChild(11).GetComponent<Button>();Check(replay.IsInteractable(),"Focus REPLAY enabled.");replay.OnPointerClick(new PointerEventData(null));
        yield return new WaitForSecondsRealtime(5.5f);
        Check(focus.IsSagaOpen && (bool)Get(focus,"playing") && !map.activeSelf && Get(page,"victory")==null,"Old victory must not close a replay.");focus.Close();
        page.GetType().GetMethod("PlayStage",Flags).Invoke(page,new object[]{8});focus.Begin();yield return SolveFocus(focus);
        result=(GameObject)Get(focus,"popup");var mapButton=result.transform.GetChild(0).GetChild(12).GetComponent<Button>();
        Check(mapButton.IsInteractable(),"Focus MAP enabled.");mapButton.OnPointerClick(new PointerEventData(null));yield return null;
        Check(map.activeSelf && !focus.IsSagaOpen && Get(page,"victory")==null,"Manual MAP must return immediately and stop pending auto-return.");
        PlayerPrefs.SetInt("saga.motion.v1",oldMotion);PlayerPrefs.Save();
        Debug.Log("SAGA_MAP_PLAY_PASSED:240stops, actual Focus Pairs/Boxes wins, automatic return, 20-21 scroll, grey/color reveal, electrical pulse, spontaneous discharge, motion-off cancellation.");
    }
    IEnumerator SolveFocus(FocusGame game){
        float limit=Time.realtimeSinceStartup+95;
        while(game.IsSagaOpen && (bool)Get(game,"playing")){
            Check(Time.realtimeSinceStartup<limit,"Focus native solve watchdog.");
            if((bool)Get(game,"transition") || (bool)Get(game,"flashWatching")){yield return null;continue;}
            int kind=(int)Get(game,"kind");
            if(kind==0){var sum=(FocusRules.Sum)Get(game,"sum");foreach(char c in sum.Answer.ToString())game.MathKey(c.ToString());game.MathKey("OK");}
            else if(kind==1){
                object chosenArena=null,chosenBall=null;
                foreach(var arena in (System.Collections.IList)Get(game,"arenas")){foreach(var ball in (System.Collections.IList)Get(arena,"Balls")){var state=(FocusRules.Ball)Get(ball,"State");if(!state.Armed && state.Hits<(int)Get(arena,"Target")){chosenArena=arena;chosenBall=ball;break;}}if(chosenBall!=null)break;}
                if(chosenBall!=null)game.GetType().GetMethod("Wall",Flags).Invoke(game,new object[]{chosenArena,((FocusRules.Ball)Get(chosenBall,"State")).NextWall()});
            }else{var sequence=(int[])Get(game,"sequence");int cursor=(int)Get(game,"cursor");((GameObject)Get(game,"body")).GetComponentsInChildren<Button>().First(b=>b.name=="Token "+sequence[cursor]).onClick.Invoke();}
            yield return null;
        }
    }
    IEnumerator ValidateFocusBoss(Transform canvas){
        UserConstants.s_user=new User{id=1999999821,username="Focus boss runtime"};
        foreach(SagaGame kind in Enum.GetValues(typeof(SagaGame)))for(int n=1;n<=20;n++)PlayerPrefs.SetInt("saga."+kind.ToString().ToLowerInvariant()+".stars.v1.1999999821."+n,0);
        string stats=PlayerPrefs.GetString("focus.stats.device.v1.1999999821","");
        var host=new GameObject("Focus boss host",typeof(RectTransform));host.transform.SetParent(canvas,false);var boss=host.AddComponent<SagaBossGame>();bool won=false;
        boss.Open(20,()=>{},()=>won=true,true);boss.StartBoss();var order=(SagaGame[])Get(boss,"order");Check(order.Length==4 && order.Distinct().Count()==4,"Four unique Focus boss games.");
        var focus=(FocusGame)Get(boss,"focus");int prior=-1;bool mistake=false;float previous=90,limit=Time.realtimeSinceStartup+92;
        while((bool)Get(boss,"running")){
            Check(Time.realtimeSinceStartup<limit,"Focus boss watchdog.");Check(boss.Remaining<=previous+.001f,"Focus boss deadline reset.");previous=boss.Remaining;
            int round=(int)Get(boss,"index");
            if(round!=prior && focus.IsSagaOpen){Debug.Log("FOCUS_BOSS_ROUND: "+round+" remaining="+boss.Remaining);prior=round;Check((int)Get(focus,"sagaHearts")== (mistake?4:5),"Five shared Focus boss hearts must carry between games.");if(!mistake){focus.GetType().GetMethod("Award",Flags).Invoke(focus,new object[]{false});mistake=true;}}
            if(focus.IsSagaOpen && (bool)Get(focus,"playing"))yield return SolveFocus(focus);else yield return null;
        }
        Check(won && SagaProgress.Stars(SagaGame.FocusBoss,20)==2,"Focus boss20 should complete with carried four hearts.");
        foreach(var game in SagaRoute.FocusGames)Check(SagaProgress.Stars(game,20)==0,"Focus boss child progress must remain separate.");
        Check(PlayerPrefs.GetString("focus.stats.device.v1.1999999821","")==stats,"Focus boss must not save Training sessions.");
        Check(((System.Collections.Generic.Dictionary<SagaGame,Image>)Get(boss,"fills")).Values.All(i=>i.fillAmount==1),"All four Focus icons must fill.");
        boss.Close();boss.Open(1,()=>{},()=>{throw new Exception("Five errors won Focus boss.");},true);boss.StartBoss();focus=(FocusGame)Get(boss,"focus");for(int i=0;i<5;i++)focus.GetType().GetMethod("Award",Flags).Invoke(focus,new object[]{false});
        Check(!(bool)Get(boss,"running") && SagaProgress.Stars(SagaGame.FocusBoss,1)==0,"Five errors end the Focus boss.");boss.Close();
        Debug.Log("SAGA_FOCUS_BOSS_PLAY_PASSED: four native Focus games, shared90second deadline, shared5hearts, goals, icon fill, separate progress, no Training sessions.");
    }
    IEnumerator ValidateBoss(Transform canvas){
        UserConstants.s_user=new User{id=1999999819,username="Boss runtime validation"};
        foreach(SagaGame kind in Enum.GetValues(typeof(SagaGame)))for(int n=1;n<=20;n++)PlayerPrefs.SetInt("saga."+kind.ToString().ToLowerInvariant()+".stars.v1.1999999819."+n,0);
        var host=new GameObject("Isolated boss",typeof(RectTransform));host.transform.SetParent(canvas,false);var boss=host.AddComponent<SagaBossGame>();bool won=false;
        boss.Open(20,()=>{},()=>won=true);boss.StartBoss();var order=(SagaGame[])Get(boss,"order");
        Check(order.Length==6 && order.Distinct().Count()==6 && order.Contains(SagaGame.Sort),"Six unique random games.");
        var eventSystem=new GameObject("Boss drag events",typeof(EventSystem),typeof(StandaloneInputModule)).GetComponent<EventSystem>();
        float previous=90,limit=Time.realtimeSinceStartup+92;int flashCount=0;
        while((bool)Get(boss,"running")){
            Check(Time.realtimeSinceStartup<limit,"Boss watchdog.");Check(boss.Remaining<=previous+.001f,"Shared clock reset between games.");previous=boss.Remaining;
            var chain=(ChainGame)Get(boss,"chain");var says=(SaysGame)Get(boss,"says");
            if(says.IsSagaOpen && (bool)Get(says,"input")){foreach(int digit in (int[])Get(says,"sequence"))says.Choose(digit);}
            if(chain.IsSagaOpen){
                var state=(ChainState)Get(chain,"state");
                if(state!=null && !state.Finished){
                    var flash=(SagaFlashStudy)Get(chain,"flashStudy");if(flash!=null){flashCount=Mathf.Max(flashCount,flash.ShownCount);Check(flash.CurrentCell==null || flash.CurrentCell.GetComponent<Animator>().speed==SagaRoundContext.FlashPlaybackSpeed,"Boss-only Flash speed.");}
                    if(!(bool)Get(chain,"recall")){
                        if(Get(chain,"boxesBoard")!=null)chain.GetType().GetMethod("ShowRecall",Flags).Invoke(chain,null);
                        else if(Get(chain,"originalGame")!=null){int guard=0;while(!(bool)Get(chain,"recall") && guard++<60)chain.NextCard();}
                    }
                    if((bool)Get(chain,"recall")){
                        int[] sequence=(int[])state.Sequence.Clone();var pairs=(SagaPairsBoard)Get(chain,"pairsBoard");var boxes=(SagaBoxesBoard)Get(chain,"boxesBoard");var sort=(SagaSortBoard)Get(chain,"sortBoard");
                        if(pairs!=null){for(int i=0;i<sequence.Length;i+=2){pairs.Choose(sequence[i]);pairs.Choose(sequence[i+1]);}}
                        else if(boxes!=null || sort!=null){
                            GameObject root=boxes!=null?boxes.Root:sort.Root;
                            if(sort!=null){var before=sort.CurrentOrder;sort.Submit();Check(state.Hearts==2 && state.Position==0 && sort.CurrentOrder.SequenceEqual(before),"Wrong DONE loses one heart without changing the order.");while((float)Get(sort,"shake")>0)yield return null;}
                            for(int i=0;i<sequence.Length;i++){
                                int[] before=sort!=null?sort.CurrentOrder:null;int source=sort!=null?Array.IndexOf(before,sequence[i]):-1;
                                var item=root.GetComponentsInChildren<DragAndDropItem>(true).First(d=>d.name==sequence[i].ToString());
                                var data=new PointerEventData(eventSystem){position=new Vector2(100,400),pressPosition=new Vector2(100,100),pointerDrag=item.gameObject};item.OnPointerDown(data);item.OnBeginDrag(data);
                                Check(DragAndDropItem.draggedItem==item,"Native boss drag failed.");
                                if(sort!=null)root.GetComponentsInChildren<SagaSortSlot>(true).First(d=>d.Slot==i).OnDrop(data);else root.GetComponentsInChildren<SagaBoxesDrop>(true).First(d=>d.Slot==i).OnDrop(data);
                                item.OnEndDrag(data);
                                if(sort!=null){var after=sort.CurrentOrder;Check(after[i]==sequence[i] && after[source]==before[i],"SORT must swap both occupied positions.");Check(state.Position==0 && state.Hearts==2,"SORT swaps must not validate before DONE.");}
                            }
                            if(sort!=null)sort.Submit();
                        }else foreach(int id in sequence)chain.Choose(id);
                    }
                }
            }
            yield return null;
        }
        Check(won && SagaProgress.Stars(SagaGame.Boss,20)==2,"Boss level20 must be completable inside90seconds and preserve the lost heart.");
        foreach(var game in SagaRoute.Games)Check(SagaProgress.Stars(game,20)==0,"Boss child must not save ordinary Saga progress.");
        Check(flashCount>=31,"Boss Flash must display entire32-image sequence before recall.");
        var fills=(System.Collections.Generic.Dictionary<SagaGame,Image>)Get(boss,"fills");Check(fills.Values.All(i=>i.fillAmount==1),"All six completion icons filled.");
        boss.Close();
        var carry=new SagaRoundContext(Time.realtimeSinceStartup+90,(success,lives)=>{},null,2);
        var carriedChain=(ChainGame)Get(boss,"chain");carriedChain.OpenSaga(SagaGame.Flash,1,()=>{},null,null,carry);
        Check(((ChainState)Get(carriedChain,"state")).Hearts==2,"Boss child must inherit the remaining hearts.");
        var flashStudy=(SagaFlashStudy)Get(carriedChain,"flashStudy");Check(flashStudy.Root.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Exact heart "))==2,"Flash study must show carried hearts.");carriedChain.Close();
        var carriedSays=(SaysGame)Get(boss,"says");carriedSays.OpenSaga(1,()=>{},null,null,carry);Check((int)Get(carriedSays,"hearts")==2,"Says must inherit remaining boss hearts.");carriedSays.Close();
        boss.Open(1,()=>{},()=>{throw new Exception("Expired boss won.");});boss.StartBoss();boss.GetType().GetField("deadline",Flags).SetValue(boss,Time.realtimeSinceStartup-1);yield return null;yield return null;
        Check(!(bool)Get(boss,"running") && SagaProgress.Stars(SagaGame.Boss,1)==0,"Boss deadline must fail without saving progress.");boss.Close();
        Debug.Log("SAGA_BOSS_PLAY_PASSED: six randomized native games, level20 actual animation/drag-drop, one90second deadline, all six icons, boss-only progress, expiration.");
    }
}
#endif
