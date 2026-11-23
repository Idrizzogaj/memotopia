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
    static object Get(object source,string field){return source.GetType().GetField(field,Flags).GetValue(source);}
    static void Check(bool okay,string message){if(!okay)throw new Exception("Saga play: "+message);}
    public static void Begin(){
        if(Application.productName!="MemotopiaSagaValidation")throw new Exception("Use isolated validation project.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        new GameObject("Saga runtime driver").AddComponent<SagaPlayDriver>();
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
}
#endif
