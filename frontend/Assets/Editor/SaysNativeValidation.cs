using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SaysNativeValidation {
 public static void ReviewWithSession(){StartupSessionValidation.Review();Review();PracticeValidation.Review();}
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
   MemorySettings.Practice=false;MemorySettings.ShowImageNumbers=true;CardFaces.Selected=CardFaces.Choice.Images;CardFaces.BeginSession();Check(object.ReferenceEquals(CardFaces.SaysFace(1,0),MajorSystemImages.Load(1)),"Says Images must ignore global number-label setting");var game=SaysGame.Open(rect);capture("says-native-levels");game.StartLevel(1);var keys=(System.Collections.IList)Get(game,"keys");Check(keys.Count==10,"Ten fixed keys from first level");var firstFaces=(int[])((int[])Get(game,"faceIds")).Clone();var zero=(Button)keys[0];var one=(Button)keys[1];var nine=(Button)keys[9];Check(((RectTransform)zero.transform).anchorMin.x==.5f && ((RectTransform)zero.transform).anchorMin.y<((RectTransform)nine.transform).anchorMin.y,"Zero below1–9");Check(((RectTransform)one.transform).anchorMin.x<((RectTransform)zero.transform).anchorMin.x,"Phone keypad left column");game.BeginPlayback();game.Tick(2.1f);Check((int)Get(game,"highlighted")>=0,"Playback highlights a key");Check(!game.Choose(0),"Recall cannot be entered during playback");capture("says-native-playback");
   game.Tick(4);Check(!(bool)Get(game,"input") && (bool)Get(game,"recallTransition"),"Recall cue gates input after playback");
   var board=(ChainOriginalPresentation)Get(game,"board");Check(!board.Grid.gameObject.activeSelf,"Cue hides every picture");Check((float)Get(game,"clock")==180,"No recall time consumed by memorisation");Check(!game.Choose(0),"Cue cannot accept an answer");capture("says-recall-cue");
   game.Tick(.45f);Check(!(bool)Get(game,"input") && (float)Get(game,"clock")==180,"Timer stays paused during cue");game.Tick(.5f);Check((bool)Get(game,"input") && board.Grid.gameObject.activeSelf,"Pictures return and recall opens");game.Tick(2);Check((float)Get(game,"clock")==178,"Clock starts only after the cue");
   var sequence=(int[])Get(game,"sequence");int wrong=(sequence[0]+1)%10;game.Choose(wrong);Check((float)Get(game,"wrongRemaining")>0,"Wrong answer starts red shake");game.Tick(.5f);game.Choose(sequence[0]);Check(SaysGame.Complete(1) && SaysGame.Stars(1)==2,"Winning with one mistake saves two stars");game.Close();
   game.Show();Check(!root.GetComponentsInChildren<Button>().Any(b=>b.name=="Challenge"),"No Challenge box in Says level menu");game.StartLevel(2);Check(!firstFaces.Intersect((int[])Get(game,"faceIds")).Any(),"New level replaces every picture");game.BeginPlayback();game.Tick(20);game.Tick(1);sequence=(int[])Get(game,"sequence");foreach(int key in sequence)game.Choose(key);Check(SaysGame.Complete(2) && SaysGame.Stars(2)==3,"Perfect level two saves three stars");game.Close();
   PlayerPrefs.SetInt("says.preview.v1.1999999931",7);PlayerPrefs.DeleteKey("says.preview.v1.1999999931.stars.3");Check(SaysGame.Stars(3)==1,"Old proven completion gets minimum star, never invented perfect stars");game.Show();
   for(int level=1;level<=3;level++){var cell=root.GetComponentsInChildren<Image>(true).First(i=>i.name=="Chain Level "+level);var stars=cell.transform.GetChild(0);Check(stars.gameObject.activeSelf,"Star group visible");for(int n=0;n<stars.childCount;n++){var star=stars.GetChild(n).GetComponent<Image>();Check(star.gameObject.activeSelf==(n<SaysGame.Stars(level)),"Correct count after reopening");Check(star.sprite==Resources.Load<Sprite>("GamesResources/Levels/Asset "+(15-n)),"Exact existing level star artwork");}}
   capture("says-earned-stars");
   var reactions=SprintReactions.Attach(root.transform,33);Check(reactions.transform.Find("Six reactions").gameObject.activeSelf,"All six reactions must be visible by default");Check(reactions.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Emoji ") && i.gameObject.activeInHierarchy)==6,"Six visible emoji graphics");capture("says-native-reactions");game.Close();
   Debug.Log("SAYS_NATIVE_VALIDATION_PASSED: keypad, original animation, recall cue and timer gating, no Challenge box, saved stars and legacy completion");
  }finally{PlayerPrefs.DeleteKey("content.choice.v1.1999999931");PlayerPrefs.DeleteKey("memory.settings.v1.1999999931.imageNumbers");PlayerPrefs.DeleteKey("memory.settings.v1.1999999931.practice");PlayerPrefs.DeleteKey("says.preview.v1.1999999931");for(int n=1;n<=3;n++)PlayerPrefs.DeleteKey("says.preview.v1.1999999931.stars."+n);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UserConstants.s_user=prior;CardFaces.EndSession();}
 }
}
