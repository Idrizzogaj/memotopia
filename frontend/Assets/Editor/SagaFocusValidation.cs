using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SagaFocusValidation
{
    const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    static object Get(object value,string field){return value.GetType().GetField(field,Flags).GetValue(value);}
    static void Call(object value,string method,params object[] args){value.GetType().GetMethod(method,Flags).Invoke(value,args);}
    static void Check(bool okay,string reason){if(!okay)throw new Exception("Saga Focus: "+reason);}
    public static void Validate(){
        Check(Application.productName=="MemotopiaSagaValidation","Use isolated project.");
        var oldUser=UserConstants.s_user;UserConstants.s_user=new User{id=1999999820,username="Focus Saga validation"};
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        var root=new GameObject("Saga Focus review",typeof(RectTransform),typeof(Canvas));var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;((RectTransform)root.transform).sizeDelta=new Vector2(800,1734);
        var cameraObject=new GameObject("Saga Focus camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=867;camera.transform.position=new Vector3(0,0,-20);camera.nearClipPlane=.1f;camera.farClipPlane=1000;canvas.worldCamera=camera;
        var host=new GameObject("Focus controller",typeof(RectTransform));host.transform.SetParent(root.transform,false);var game=host.AddComponent<FocusGame>();
        string stats=PlayerPrefs.GetString("focus.stats.device.v1.1999999820","");string badges=PlayerPrefs.GetString("achievements.verified.v2.1999999820","");
        try{
            int[][] targets={new[]{10,15,20,30},new[]{20,30,40,60},new[]{10,20,30,50},new[]{30,50,70,100}};
            for(int kind=0;kind<4;kind++)for(int n=1;n<=20;n++){
                var level=new SagaFocusLevel(kind,n);Check(level.Target==targets[kind][(n-1)/5],"Target groups.");
                Check(SagaRoute.At(SagaRoute.Stop(level.Game,n)).Game==level.Game,"Focus route identity.");
            }
            for(int kind=0;kind<4;kind++)foreach(int tier in new[]{1,5,6,11,16,20}){
                var config=new SagaFocusLevel(kind,tier);PlayerPrefs.SetInt("saga."+config.Game.ToString().ToLowerInvariant()+".stars.v1.1999999820."+tier,0);
                bool completed=false;game.OpenSaga(config,()=>{},()=>completed=true);Check((int)Get(game,"sagaHearts")==5 && !(bool)Get(game,"playing"),"Start screen, five hearts.");game.Begin();
                Check((int)Get(game,"level")==1 && (float)Get(game,"remaining")==config.Seconds,"Original level1 start and Saga timer.");
                if(tier==1)typeof(SagaValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas,camera,800,1734,"saga-focus-"+kind+"-phone"});
                if(kind==0){var sum=(FocusRules.Sum)Get(game,"sum");foreach(char c in sum.Answer.ToString())game.MathKey(c.ToString());game.MathKey("OK");Check((int)Get(game,"score")==1,"Actual math callback.");}
                while((bool)Get(game,"playing")){
                    if((bool)Get(game,"transition")){var advance=(IEnumerator)game.GetType().GetMethod("Advance",Flags).Invoke(game,null);advance.MoveNext();advance.MoveNext();}
                    if(kind==0){var sum=(FocusRules.Sum)Get(game,"sum");foreach(char c in sum.Answer.ToString())game.MathKey(c.ToString());game.MathKey("OK");}
                    else if(kind==1){object chosenArena=null,chosenBall=null;foreach(var arena in (IList)Get(game,"arenas")){foreach(var ball in (IList)Get(arena,"Balls")){var state=(FocusRules.Ball)Get(ball,"State");if(!state.Armed && state.Hits<(int)Get(arena,"Target")){chosenArena=arena;chosenBall=ball;break;}}if(chosenBall!=null)break;}if(chosenBall==null)game.Tick(.05f);else Call(game,"Wall",chosenArena,((FocusRules.Ball)Get(chosenBall,"State")).NextWall());}
                    else{if((bool)Get(game,"flashWatching"))game.Tick(3.1f);if(!(bool)Get(game,"playing"))break;var sequence=(int[])Get(game,"sequence");int cursor=(int)Get(game,"cursor");var button=((GameObject)Get(game,"body")).GetComponentsInChildren<Button>().First(b=>b.name=="Token "+sequence[cursor]);button.onClick.Invoke();}
                }
                Check(completed && SagaProgress.Stars(config.Game,tier)==3,"Target completes Saga round and saves only its progress.");game.Close();
            }
            var first=new SagaFocusLevel(0,1);game.OpenSaga(first,()=>{},()=>{throw new Exception("Five errors won.");});game.Begin();
            for(int i=0;i<5;i++)Call(game,"Award",false);
            Check(!(bool)Get(game,"playing") && (int)Get(game,"sagaHearts")==0 && (int)Get(game,"score")==-5,"Five errors fail, preserving minus1 score.");game.Close();
            game.OpenSaga(first,()=>{},()=>{throw new Exception("Timeout won.");});game.Begin();game.Tick(first.Seconds);Check(!(bool)Get(game,"playing"),"Saga Focus timeout.");game.Close();
            Check(PlayerPrefs.GetString("focus.stats.device.v1.1999999820","")==stats && PlayerPrefs.GetString("achievements.verified.v2.1999999820","")==badges,"Saga must not mutate Training Focus stats or badge keys.");
            Debug.Log("SAGA_FOCUS_VALIDATION_PASSED: all80 targets, native callbacks, level1 progression,5hearts, minus1, goal, timeout, isolated saves.");
        }finally{game.Close();UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UserConstants.s_user=oldUser;}
    }
}
