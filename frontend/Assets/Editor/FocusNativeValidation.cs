using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class FocusNativeValidation {
 static object Get(object o,string name){return o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(o);}
 static void Set(object o,string name,object value){o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).SetValue(o,value);}
 static void Check(bool okay,string reason){if(!okay)throw new Exception(reason);}
 public static void Review(){
  var random=new System.Random(1107);
  for(int level=1;level<=8;level++)for(int i=0;i<250;i++){var sum=FocusRules.Question(level,random);Check(sum.Answer>=0,"Negative answer");Check(sum.Operator!="÷" || sum.A==sum.B*sum.Answer,"Division must be integral");Check(sum.Operator!="×" || sum.A*sum.B<=sum.Limit,"Product exceeds level");Check(sum.Operator!="+" || sum.Answer<=sum.Limit,"Sum exceeds level");}
  for(int kind=2;kind<=3;kind++)for(int level=1;level<=FocusRules.Maximum(kind);level++){var sequence=FocusRules.Sequence(kind,level);Check(sequence.Distinct().Count()==sequence.Length,"Duplicate number/colour target");if(FocusRules.NumberMode(kind,level)==3)Check(sequence[0]==1 && sequence[1]==1000+FocusRules.NumberCount(kind,level),"Alternating stream order");}
  var physics=new FocusRules.Ball{Position=new Vector2(.5f,.5f),Velocity=new Vector2(.3f,.1f)};Check(physics.NextWall()==1,"Next wall prediction");Check(physics.Step(2)==1 && physics.Velocity.x<0,"Wall reflection");
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var root=new GameObject("Native Focus review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1734);root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
  var cameraObject=new GameObject("Native review camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;
  try{
   for(int kind=0;kind<4;kind++){
    var game=FocusGame.Open(rect,kind);Set(game,"startLevel",kind==1?4:kind==2?9:kind==3?10:1);game.Begin();
    if(kind==0){var sum=(FocusRules.Sum)Get(game,"sum");foreach(char digit in sum.Answer.ToString())game.MathKey(digit.ToString());game.MathKey("OK");Check((int)Get(game,"score")==1 && (int)Get(game,"questionIndex")==1,"Math answer callback");}
    else if(kind==1){
     var arenas=(IList)Get(game,"arenas");var arena=arenas[0];var balls=(IList)Get(arena,"Balls");var state=(FocusRules.Ball)Get(balls[0],"State");int wall=state.NextWall();
     var area=((Image)Get(balls[0],"Image")).transform.parent;area.GetComponentsInChildren<Button>().First(b=>b.name=="Tap wall "+wall).onClick.Invoke();Check((int)Get(game,"score")==1 && state.Armed,"Early wall tap must arm one future impact and score once");
     float travel=state.Arrival();for(int n=0;n<Mathf.CeilToInt((travel+.1f)/.05f);n++)game.Tick(.05f);Check(!state.Armed && (int)Get(game,"correct")==1,"Covered impact must not score twice");
    }else{
     var sequence=(int[])Get(game,"sequence");foreach(int key in sequence.Take(2)){var button=root.GetComponentsInChildren<Button>().First(b=>b.name=="Token "+key);button.onClick.Invoke();}Check((int)Get(game,"score")==2,"Native number ordering callback");
    }
    Canvas.ForceUpdateCanvases();Check(root.GetComponentsInChildren<Text>().Any(t=>t.text=="90"||t.text=="89"||t.text=="88"||t.text=="87"||t.text=="86"||t.text=="85"),"Visible native timer missing");
    if(kind==1 || kind==2)Check(root.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Ball ")||i.name.StartsWith("Token ")).All(i=>i.sprite!=null),"Geometric circles must be present on both Unity versions");
    typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,camera,"focus-native-"+FocusRules.Names[kind],945,2048});
    game.Tick(100);Check(!(bool)Get(game,"playing"),"90second clock must finish");var popup=(GameObject)Get(game,"popup");Check(popup!=null && popup.GetComponentsInChildren<Text>(true).Any(t=>t.text.Contains("Score")),"Native result dialog must display score");game.Close();
   }
   Debug.Log("FOCUS_NATIVE_VALIDATION_PASSED: arithmetic ranges, all number stages, wall anticipation/reflection, real native callbacks,90second end, score popup and four scene captures");
  }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);}
 }
}
