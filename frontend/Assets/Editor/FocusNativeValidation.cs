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
  Check(FocusRules.Maximum(2)==12,"Flash must stop at level12");
  for(int level=1;level<=12;level++){
   int n=level+2;var green=FocusRules.Sequence(2,level,0);var red=FocusRules.Sequence(2,level,1);var mixed=FocusRules.Sequence(2,level,2);
   Check(green.SequenceEqual(Enumerable.Range(1,n)),"Flash green ascending");Check(red.SequenceEqual(Enumerable.Range(1,n).Reverse().Select(v=>1000+v)),"Flash red descending");
   Check(mixed.SequenceEqual(Enumerable.Range(1,n).SelectMany(v=>new[]{v,1000+n-v+1})),"Flash mixed order");
  }
  for(int level=1;level<=8;level++)for(int i=0;i<250;i++){var sum=FocusRules.Question(level,random);Check(sum.Answer>=0,"Negative answer");Check(sum.Operator!="÷" || sum.A==sum.B*sum.Answer,"Division must be integral");Check(sum.Operator!="×" || sum.A*sum.B<=sum.Limit,"Product exceeds level");Check(sum.Operator!="+" || sum.Answer<=sum.Limit,"Sum exceeds level");}
  for(int kind=2;kind<=3;kind++)for(int level=1;level<=FocusRules.Maximum(kind);level++){var sequence=FocusRules.Sequence(kind,level);Check(sequence.Distinct().Count()==sequence.Length,"Duplicate number/colour target");if(FocusRules.NumberMode(kind,level)==3)Check(sequence[0]==1 && sequence[1]==1000+FocusRules.NumberCount(kind,level),"Alternating stream order");}
  var physics=new FocusRules.Ball{Position=new Vector2(.5f,.5f),Velocity=new Vector2(.3f,.1f)};Check(physics.NextWall()==1,"Next wall prediction");Check(physics.Step(2)==1 && physics.Velocity.x<0,"Wall reflection");
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var root=new GameObject("Native Focus review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1734);root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
  var cameraObject=new GameObject("Native review camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;camera.orthographicSize=rect.rect.height/2;root.GetComponent<Canvas>().worldCamera=camera;
  try{
   for(int kind=0;kind<4;kind++){
    var game=FocusGame.Open(rect,kind);Set(game,"startLevel",kind==1?4:kind==2?9:kind==3?10:1);game.Begin();
    if(kind==0){var sum=(FocusRules.Sum)Get(game,"sum");foreach(char digit in sum.Answer.ToString())game.MathKey(digit.ToString());game.MathKey("OK");Check((int)Get(game,"score")==1 && (int)Get(game,"questionIndex")==1,"Math answer callback");}
    else if(kind==1){
     var arenas=(IList)Get(game,"arenas");var arena=arenas[0];var balls=(IList)Get(arena,"Balls");var state=(FocusRules.Ball)Get(balls[0],"State");int wall=state.NextWall();
     var wallImages=(Image[])Get(arena,"Walls");Check(wallImages[0].rectTransform.anchorMin.y<=.95f && wallImages[1].rectTransform.anchorMin.x<=.95f,"Walls must be at least double original thickness");Check(wallImages[0].transform.parent.GetComponentsInChildren<FocusWallTarget>().Length==4,"Four broad touch zones");
     var area=((Image)Get(balls[0],"Image")).transform.parent;var white=(RectTransform)((GameObject)Get(game,"body")).transform;var uv=wall==0?new Vector2(.25f,.98f):wall==1?new Vector2(.49f,.52f):wall==2?new Vector2(.25f,.02f):new Vector2(.01f,.52f);var point=white.TransformPoint(white.rect.min+Vector2.Scale(uv,white.rect.size));game.TapBoxArea(RectTransformUtility.WorldToScreenPoint(camera,point));Check((int)Get(game,"score")==1 && state.Armed,"Early wall tap must arm one future impact and score once");
     float travel=state.Arrival();for(int n=0;n<Mathf.CeilToInt((travel+.1f)/.05f);n++)game.Tick(.05f);Check(!state.Armed && (int)Get(game,"correct")==1,"Covered impact must not score twice");
    }else{
     if(kind==2){Check((bool)Get(game,"flashWatching"),"Flash must begin by showing numbers");var board=(GameObject)Get(game,"body");var token=board.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith("Token "));token.onClick.Invoke();Check((int)Get(game,"score")==0,"No recall input while numbers are visible");typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,camera,"focus-native-Flash-memorise",945,2048});game.Tick(3.1f);Check(!(bool)Get(game,"flashWatching"),"Flash numbers must hide before recall");Check(board.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Token ")).All(b=>b.GetComponentInChildren<Text>().text==""),"Flash recall must have blank circles");}
     var sequence=(int[])Get(game,"sequence");foreach(int key in sequence.Take(2)){var button=root.GetComponentsInChildren<Button>().First(b=>b.name=="Token "+key);button.onClick.Invoke();}Check((int)Get(game,"score")==2,"Native number ordering callback");
    }
    Canvas.ForceUpdateCanvases();Check(root.GetComponentsInChildren<Text>().Any(t=>t.text=="90"||t.text=="89"||t.text=="88"||t.text=="87"||t.text=="86"||t.text=="85"),"Visible native timer missing");
    if(kind==1 || kind==2)Check(root.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Ball ")||i.name.StartsWith("Token ")).All(i=>i.sprite!=null),"Geometric circles must be present on both Unity versions");
    typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,camera,"focus-native-"+FocusRules.Names[kind],945,2048});
    if(kind==2){Set(game,"level",1);Set(game,"flashPart",0);for(int part=0;part<3;part++){var advance=(IEnumerator)typeof(FocusGame).GetMethod("Advance",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);Check(advance.MoveNext(),"Flash transition delay");advance.MoveNext();Check((int)Get(game,"level")== (part<2?1:2) && (int)Get(game,"flashPart")== (part<2?part+1:0),"All three parts before advancing level");}}
    game.Tick(100);Check(!(bool)Get(game,"playing"),"90second clock must finish");var popup=(GameObject)Get(game,"popup");Check(popup!=null && popup.GetComponentsInChildren<Text>(true).Any(t=>t.text.Contains("Score")),"Native result dialog must display score");game.Close();
   }
   Debug.Log("FOCUS_NATIVE_VALIDATION_PASSED: arithmetic ranges, all number stages, wall anticipation/reflection, real native callbacks,90second end, score popup and four scene captures");
  }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);}
 }
}
