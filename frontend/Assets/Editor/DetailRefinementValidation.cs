using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DetailRefinementValidation {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static void Review(){
  var random=new System.Random(41027);foreach(int wall in Enumerable.Range(0,4))for(int n=0;n<250;n++){
   Vector2 normal=wall==0?Vector2.down:wall==1?Vector2.left:wall==2?Vector2.up:Vector2.right;var ball=new FocusRules.Ball{Random=random,Position=new Vector2(.5f,.5f),Velocity=-normal*.25f};Check(ball.Step(2)==wall,"Expected physical wall");Check(Vector2.Dot(ball.Velocity.normalized,normal)>=Mathf.Cos(45*Mathf.Deg2Rad)-.00001f,"Random bounce outside approved45degree cone");Check(Mathf.Abs(ball.Velocity.magnitude-.25f)<.00001f,"Bounce changes speed");
  }
  Check(FocusRules.BoxZone(new Vector2(.1f,.8f),3)==0 && FocusRules.BoxZone(new Vector2(.9f,.8f),3)==1 && FocusRules.BoxZone(new Vector2(.1f,.1f),3)==2,"Three unambiguous area zones");
  NightReview.Review();FocusAchievementValidation.Review();
  var prior=UserConstants.s_user;string priorToken=PlayerPrefs.GetString("token","");const int user=1999999929;
  try{
   UserConstants.s_user=new User{id=user,username="Tip review",userStatistics=new UserStatistics()};PlayerPrefs.SetString("token","");EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var menu=Resources.FindObjectsOfTypeAll<MenuNavigation>().First(n=>n.gameObject.scene.IsValid());
   var canvas=menu.tipsScreen.GetComponentsInParent<Canvas>(true).First().rootCanvas;for(var t=menu.tipsScreen.transform;t!=canvas.transform;t=t.parent){foreach(Transform other in t.parent)if(other!=t)other.gameObject.SetActive(false);t.gameObject.SetActive(true);}menu.GoToTips();
   var content=menu.tipsScreen.transform.Find("Scroll View/Viewport/Content");var deck=content.Find("TheDeckGuide");Check(deck!=null && deck.gameObject.activeInHierarchy && deck.GetSiblingIndex()==0,"The Deck must be visible first in Tips");Check(content.GetComponent<ContentSizeFitter>().verticalFit==ContentSizeFitter.FitMode.PreferredSize,"Existing tip list must expand to preferred height");
   var camera=new GameObject("Tip review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.backgroundColor=Color.white;camera.clearFlags=CameraClearFlags.SolidColor;canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;canvas.GetComponent<CanvasScaler>().enabled=false;((RectTransform)canvas.transform).sizeDelta=new Vector2(800,1734);Canvas.ForceUpdateCanvases();var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);capture.Invoke(null,new object[]{canvas.gameObject,camera,"tips-deck-entry",945,2048});
   deck.GetComponent<Button>().onClick.Invoke();var page=menu.tipsScreen.transform.Find("Screens/DeckGuidePage");Check(page.gameObject.activeInHierarchy,"Deck entry must open explanation");var groups=page.GetComponentsInChildren<Button>(true).Where(b=>b.name.StartsWith("Expand ")).ToArray();Check(groups.Length==4,"Four suit tabs");foreach(var group in groups)group.onClick.Invoke();Check(page.GetComponentsInChildren<HorizontalLayoutGroup>().Count(r=>r.name.StartsWith("Card "))==52,"All52card associations");capture.Invoke(null,new object[]{canvas.gameObject,camera,"tips-deck-guide",945,2048});
   var read=page.GetComponentInChildren<TipReadBar>();Check((string)typeof(TipReadBar).GetField("key",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(read)=="tip-the-deck","Deck read badge key");Check(ExtraAchievements.All.Any(d=>d.Key=="tip-the-deck"),"Deck Decoder catalog entry");Check(GameManager.UnlockAchievement("tip-the-deck")&&!GameManager.UnlockAchievement("tip-the-deck"),"Deck Decoder awarded once");UnityEngine.Object.DestroyImmediate(camera.gameObject);Debug.Log("DETAIL_REFINEMENT_PASSED:1000approved random bounces; Says keypad/clean images/feedback; visible Deck entry, four tabs,52cards and one-time Deck Decoder");
  }finally{PlayerPrefs.SetString("token",priorToken);foreach(var key in new[]{"achievements.verified.v2.","achievements.pending.v2."})PlayerPrefs.DeleteKey(key+user);UserConstants.s_user=prior;}
 }
}
