using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SaysNativeValidation {
 static object Get(object o,string name){return o.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static void Validate(){
  Check(SaysGame.Count(1)==1 && SaysGame.Count(8)==8 && SaysGame.Count(15)==22 && SaysGame.Count(19)==34 && SaysGame.Count(20)==37 && SaysGame.Count(21)==40,"Says level targets");
  Check(SaysGame.ChallengeCount(0)==5 && SaysGame.ChallengeCount(4)==13,"Says Challenge increments two");
  Check(FocusRules.FlashSeconds(3)==1 && FocusRules.FlashSeconds(5)==2 && FocusRules.FlashSeconds(7)==3,"Flash display duration");
  foreach(var key in SprintReactions.Keys){var sprite=SprintReactions.Icon(key);Check(sprite!=null && sprite.rect.width>=64 && sprite.rect.height>=64,"Missing emoji bitmap "+key);}
 }
 public static void Review(){
  Validate();var prior=UserConstants.s_user;UserConstants.s_user=new User{id=1999999931,username="Says review"};EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
  var root=new GameObject("Says review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1734);root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
  var cameraObject=new GameObject("Says review camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;root.GetComponent<Canvas>().worldCamera=camera;
  Action<string> capture=name=>typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,camera,name,945,2048});
  try{
   MemorySettings.ShowImageNumbers=true;CardFaces.Selected=CardFaces.Choice.Images;CardFaces.BeginSession();Check(object.ReferenceEquals(CardFaces.SaysFace(1,0),MajorSystemImages.Load(1)),"Says Images must ignore global number-label setting");var game=SaysGame.Open(rect);capture("says-native-levels");game.StartLevel(1);var keys=(System.Collections.IList)Get(game,"keys");Check(keys.Count==10,"Ten fixed keys from first level");var firstFaces=(int[])((int[])Get(game,"faceIds")).Clone();var zero=(Button)keys[0];var one=(Button)keys[1];var nine=(Button)keys[9];Check(((RectTransform)zero.transform).anchorMin.x==.5f && ((RectTransform)zero.transform).anchorMin.y<((RectTransform)nine.transform).anchorMin.y,"Zero below1–9");Check(((RectTransform)one.transform).anchorMin.x<((RectTransform)zero.transform).anchorMin.x,"Phone keypad left column");game.BeginPlayback();game.Tick(2.1f);Check((int)Get(game,"highlighted")>=0,"Playback highlights a key");Check(!game.Choose(0),"Recall cannot be entered during playback");capture("says-native-playback");game.Tick(4);Check((bool)Get(game,"input"),"Recall begins after playback");var sequence=(int[])Get(game,"sequence");int wrong=(sequence[0]+1)%10;game.Choose(wrong);Check((float)Get(game,"wrongRemaining")>0,"Wrong answer starts red shake");game.Tick(.5f);game.Choose(sequence[0]);Check(SaysGame.Complete(1),"Winning prototype level persists its own progress");game.Close();
   game.Show();game.ShowLevels();var challenge=root.GetComponentsInChildren<Button>().First(b=>b.name=="Challenge");challenge.onClick.Invoke();Check(((int[])Get(game,"sequence")).Length==5,"Challenge preview begins with five");var priorFaces=(int[])((int[])Get(game,"faceIds")).Clone();game.Tick(20);sequence=(int[])Get(game,"sequence");foreach(int key in sequence)game.Choose(key);Check(((int[])Get(game,"sequence")).Length==7,"Challenge preview advances to seven");Check(!priorFaces.Intersect((int[])Get(game,"faceIds")).Any(),"New level replaces every picture");capture("says-native-challenge");
   var reactions=SprintReactions.Attach(root.transform,33);Check(reactions.transform.Find("Six reactions").gameObject.activeSelf,"All six reactions must be visible by default");Check(reactions.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Emoji ") && i.gameObject.activeInHierarchy)==6,"Six visible emoji graphics");capture("says-native-reactions");game.Close();
   Debug.Log("SAYS_NATIVE_VALIDATION_PASSED: ten keys, autoplay, input gating, same-order recall, independent progress,37/40targets,5/7Challenge preview and six visible emoji bitmaps");
  }finally{PlayerPrefs.DeleteKey("content.choice.v1.1999999931");PlayerPrefs.DeleteKey("memory.settings.v1.1999999931.imageNumbers");PlayerPrefs.DeleteKey("says.preview.v1.1999999931");PlayerPrefs.DeleteKey("says.preview.v1.1999999931.stars.1");UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UserConstants.s_user=prior;CardFaces.EndSession();}
 }
}
