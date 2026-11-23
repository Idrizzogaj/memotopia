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
    const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    float started;
    float lastReport;
    ChainGame current;
    [SerializeField] bool mapOnly;
    static object Get(object source,string field){return source.GetType().GetField(field,Flags).GetValue(source);}
    static void Check(bool okay,string message){if(!okay)throw new Exception("Saga play: "+message);}
    public static void Begin(){Create(false);}
    public static void BeginMap(){Create(true);}
    static void Create(bool map){
        if(Application.productName!="MemotopiaSagaValidation")throw new Exception("Use isolated validation project.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        new GameObject("Saga runtime driver").AddComponent<SagaPlayDriver>().mapOnly=map;
        EditorApplication.EnterPlaymode();
    }
    void Update(){
        if(current!=null && Time.realtimeSinceStartup-lastReport>10){lastReport=Time.realtimeSinceStartup;var study=(SagaFlashStudy)Get(current,"flashStudy");var cell=study==null?null:study.CurrentCell;var animator=cell==null?null:cell.GetComponent<Animator>();Debug.Log("SAGA_PLAY_TICK: scale="+Time.timeScale+" recall="+Get(current,"recall")+" study="+(study==null?"none":study.ShownCount.ToString())+" active="+(cell!=null && cell.gameObject.activeInHierarchy)+" state="+(animator==null?"none":animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString()));}
        if(started>0 && Time.realtimeSinceStartup-started>140){Debug.LogError("Saga play validation timed out.");EditorApplication.Exit(1);}
    }
    void Error(string message,string trace,LogType type){if(type==LogType.Error || type==LogType.Exception){Debug.Log("SAGA_PLAY_FAILED: "+message);EditorApplication.Exit(1);}}
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
        var host=new GameObject("Runtime game",typeof(RectTransform));host.transform.SetParent(root.transform,false);
        var game=host.AddComponent<ChainGame>();current=game;
        Debug.Log("SAGA_PLAY_STARTED");
        var oldShow=CellController.onShowImage;var oldHide=CellController.afterShownImage;var oldFinish=CellController.onAnimationFinished;
        game.OpenSaga(SagaGame.Flash,20,()=>{});
        SagaFlashStudy study=null;
        while(study==null){study=(SagaFlashStudy)Get(game,"flashStudy");yield return null;}
        int[] sequence=(int[])((ChainState)Get(game,"state")).Sequence.Clone();
        while(!(bool)Get(game,"recall"))yield return null;
        Check(study.ShownCount==25 && study.ShownImages.SequenceEqual(sequence),"Original Flash animation must display all 25 pictures in order.");
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
        Check(state.Won,"Native Boxes drag/drop must solve the board.");
        Debug.Log("SAGA_PLAY_PASSED: original full-speed Flash animation, all 25 pictures before recall, untouched static events, native Boxes drag/drop.");
        EditorApplication.Exit(0);
    }
    IEnumerator ValidateMap(Transform canvas){
        UserConstants.s_user=new User{id=1999999818,username="Map runtime validation"};
        for(int n=1;n<=SagaRoute.Count;n++){var stage=SagaRoute.At(n);PlayerPrefs.SetInt("saga."+stage.Game.ToString().ToLowerInvariant()+".stars.v1.1999999818."+stage.Number,n<20?3:0);}
        int oldMotion=PlayerPrefs.GetInt("saga.motion.v1",1);PlayerPrefs.SetInt("saga.motion.v1",1);
        var menuObject=new GameObject("Isolated menu",typeof(RectTransform));menuObject.transform.SetParent(canvas,false);
        var menu=menuObject.AddComponent<MenuNavigation>();menu.enabled=false;
        menu.homeScreen=ChainGame.Panel(canvas,"Home",Vector2.zero,Vector2.one,Color.white);
        menu.trainingBackground=ChainGame.Panel(menu.homeScreen.transform,"Training",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
        menu.challengeBackground=ChainGame.Panel(menu.homeScreen.transform,"Challenge",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
        menu.statisticsBackground=ChainGame.Panel(menu.homeScreen.transform,"Statistics",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
        menu.videosBackground=ChainGame.Panel(menu.homeScreen.transform,"Videos",Vector2.zero,new Vector2(.48f,1),Color.white).GetComponent<Image>();
        menu.tipsBackground=ChainGame.Panel(menu.homeScreen.transform,"Tips",new Vector2(.52f,0),Vector2.one,Color.white).GetComponent<Image>();
        SagaPage.Attach(menu);menu.homeScreen.transform.Find("Memotopia Saga").GetComponent<Button>().onClick.Invoke();
        var page=menu.GetComponent<SagaPage>();var map=(GameObject)Get(page,"page");var scroll=(ScrollRect)Get(page,"mapScroll");
        Check(map.GetComponentsInChildren<Button>(true).Count(b=>b.name.StartsWith("Saga level "))==100,"100 map buttons.");
        var themes=(System.Collections.Generic.List<Material>)Get(page,"mapThemes");
        Check(themes[0].GetVectorArray("_Cells")[19].z==1 && themes[1].GetVectorArray("_Cells")[0].z==0,"Only completed/current cells are colored.");
        page.GetType().GetMethod("PlayStage",Flags).Invoke(page,new object[]{20});
        var says=(SaysGame)Get(page,"says");says.BeginPlayback();float limit=Time.realtimeSinceStartup+30;
        while(!(bool)Get(says,"input")){Check(Time.realtimeSinceStartup<limit,"Says playback timeout.");yield return null;}
        foreach(int digit in (int[])Get(says,"sequence"))Check(says.Choose(digit),"Says correct sequence.");
        bool sawPulse=false;limit=Time.realtimeSinceStartup+10;
        while(!map.activeSelf || Get(page,"victory")!=null){sawPulse|=Get(page,"pulse")!=null;Check(Time.realtimeSinceStartup<limit,"Auto return/unlock timeout.");yield return null;}
        Check(sawPulse && Mathf.Abs(scroll.verticalNormalizedPosition-.25f)<.002f,"20 to 21 scroll and electrical reveal.");
        Check(themes[1].GetVectorArray("_Cells")[0].z==1 && themes[1].GetVectorArray("_Cells")[1].z==0,"Next cell must be colored after unlock, future cell remains grey.");
        page.GetType().GetField("nextAmbientAt",Flags).SetValue(page,Time.unscaledTime-1);yield return null;yield return null;
        Check((int)Get(page,"ambientStop")>0,"Spontaneous visible-cell animation did not start.");
        page.GetType().GetMethod("ToggleMotion",Flags).Invoke(page,null);yield return null;
        Check((int)Get(page,"ambientStop")==0 && Get(page,"ambientPulse")==null,"Motion off cancels spontaneous effects.");
        page.GetType().GetMethod("PlayStage",Flags).Invoke(page,new object[]{21});var game=(ChainGame)Get(page,"chain");
        while(Get(game,"originalGame")==null)yield return null;
        while(!(bool)Get(game,"recall"))game.NextCard();foreach(int id in ((ChainState)Get(game,"state")).Sequence)game.Choose(id);
        limit=Time.realtimeSinceStartup+8;
        while(!map.activeSelf || Get(page,"victory")!=null){Check(Get(page,"pulse")==null,"Motion off must suppress unlock pulse.");Check(Time.realtimeSinceStartup<limit,"Reduced-motion return timeout.");yield return null;}
        Check(themes[1].GetVectorArray("_Cells")[1].z==1,"Reduced-motion completion still unlocks next stage.");
        PlayerPrefs.SetInt("saga.motion.v1",oldMotion);PlayerPrefs.Save();
        Debug.Log("SAGA_MAP_PLAY_PASSED: actual Says/Chain wins, automatic return, 20-21 scroll, grey/color reveal, electrical pulse, spontaneous discharge, motion-off cancellation.");
    }
}
#endif
