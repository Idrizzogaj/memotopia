using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SprintLobbyValidation {
 static object Get(object obj,string field){return obj.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);}
 static void Set(object obj,string field,object value){obj.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(obj,value);}
 static void Call(object obj,string method,params object[] args){obj.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(obj,args);}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static Canvas canvas;static Camera camera;static EventSystem events;static GameObject root;
 static SprintBattleView Battle(int id,bool done,int winner,int chooser){
  return new SprintBattleView{id=id,ownerId=1999999977,recipientId=1999999978,ownerName="Idriz",recipientName="Hanushe",mode="match",done=done,winner=winner,chooser=chooser,legs=new[]{new SprintLegView{game="Flash",pacer=1999999977,winner=winner,ownerResult=new SprintResult{completed=1,correct=2,errors=1,roundMs=new[]{12500}},recipientResult=new SprintResult{completed=1,correct=1,errors=3,roundMs=new[]{14200}}}}};
 }
 static void UpdateLayout(){Canvas.ForceUpdateCanvases();foreach(var t in root.GetComponentsInChildren<TouchTarget>())t.Refresh();Canvas.ForceUpdateCanvases();camera.Render();}
 static void Tap(Button button){
  UpdateLayout();var rect=(RectTransform)button.transform;var point=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));
  var hits=new List<RaycastResult>();canvas.GetComponent<GraphicRaycaster>().Raycast(new PointerEventData(events){position=point},hits);
  var handler=hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
  Check(handler==button.gameObject,"Tap blocked on "+button.name+" by "+(hits.Count==0?"nothing":hits[0].gameObject.name));
  ExecuteEvents.Execute(handler,new PointerEventData(events){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
 }
 static void Capture(string name,int w=945,int h=2048){
  // Capture the actual Canvas, while the raycast checks above use its original screen camera.
  canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;var rect=(RectTransform)canvas.transform;rect.sizeDelta=new Vector2(800,h*800f/w);
  var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;
  foreach(var r in root.GetComponentsInChildren<RectTransform>(true)){var p=r.localPosition;p.z=0;r.localPosition=p;}
  camera.orthographic=true;camera.cullingMask=1<<30;
  typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas.gameObject,camera,name,w,h});
 }
 public static void Review(){
  var old=UserConstants.s_user;UserConstants.s_user=new User{id=1999999977,username="Idriz"};
  EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var nav=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().First(n=>n.gameObject.scene.IsValid());
  canvas=nav.challengeGameScreen.GetComponentsInParent<Canvas>(true).First().rootCanvas;
  var lobby=SprintLobby.Attach(nav);var fixtures=Enumerable.Range(1,12).Select(i=>Battle(i,false,0,1999999977)).Concat(Enumerable.Range(20,12).Select(i=>Battle(i,true,i%3==0?0:i%2==0?1999999977:1999999978,0))).ToArray();
  Set(lobby,"battles",fixtures);Call(lobby,"BuildPanel");Call(lobby,"RenderCurrent");root=(GameObject)Get(lobby,"panel");
  for(var t=root.transform;t!=canvas.transform;t=t.parent){foreach(Transform sibling in t.parent)if(sibling!=t)sibling.gameObject.SetActive(false);t.gameObject.SetActive(true);}
  var cameraObject=new GameObject("Lobby review camera",typeof(Camera));camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;
  var texture=new RenderTexture(945,2048,24);camera.targetTexture=texture;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
  events=UnityEngine.Object.FindObjectOfType<EventSystem>();if(events==null)events=new GameObject("Lobby events",typeof(EventSystem)).GetComponent<EventSystem>();
  try{
   foreach(var button in root.GetComponentsInChildren<Button>())TouchTarget.Ensure(button);UpdateLayout();
   var active=(ScrollRect)Get(lobby,"activeScroll");var history=(ScrollRect)Get(lobby,"historyScroll");
   Check(active!=history && active.content.childCount==12 && history.content.childCount==12,"Separated ongoing/history rows");
   Check(active.content.rect.height>active.viewport.rect.height && history.content.rect.height>history.viewport.rect.height,"Both sections must scroll");
   active.verticalNormalizedPosition=.6f;history.verticalNormalizedPosition=.4f;UpdateLayout();Check(Mathf.Abs(history.verticalNormalizedPosition-.4f)<.01f,"Independent scroll");active.verticalNormalizedPosition=1;UpdateLayout();
   Check(!root.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("rounds +") || t.text.Contains("Earlier") || t.text=="All challenges"),"Obsolete verbose text/buttons");
   var first=active.content.GetChild(0);Check(first.Find("Facts")==null,"Facts collapsed by default");Tap(first.Find("Summary/Details").GetComponent<Button>());
   active=(ScrollRect)Get(lobby,"activeScroll");history=(ScrollRect)Get(lobby,"historyScroll");first=active.content.GetChild(0);
   Check(((GameObject)Get(lobby,"list")).transform.childCount==4,"No retired list layers after expanding");
   Check(first.Find("Facts")!=null && first.GetComponent<LayoutElement>().preferredHeight>106,"Dropdown expands inside card");
   Check(first.Find("Facts").GetComponentsInChildren<Text>().Any(t=>t.text=="7"),"Correct count includes completed and current rounds");
   Check(Mathf.Abs(history.verticalNormalizedPosition-.4f)<.02f,"Dropdown retains other section scroll");
   Tap(first.Find("Summary/Details").GetComponent<Button>());active=(ScrollRect)Get(lobby,"activeScroll");Check(active.content.GetChild(0).Find("Facts")==null,"Dropdown collapses");
   history=(ScrollRect)Get(lobby,"historyScroll");var win=history.content.Find("sprint:20");var loss=history.content.Find("sprint:23");
   Check(win.Find("Summary/Open result/Outcome").GetComponent<Image>().color.g>win.Find("Summary/Open result/Outcome").GetComponent<Image>().color.r,"Win green");
   Check(loss.Find("Summary/Open result/Outcome").GetComponent<Image>().color.r>loss.Find("Summary/Open result/Outcome").GetComponent<Image>().color.g,"Loss red");
   var trophy=win.Find("Summary/Open result/Outcome/Result icon").GetComponent<Image>();Check(trophy.sprite==nav.challengeUserProfileScreen.GetComponentsInChildren<Image>(true).First(i=>i.name=="AchievementsImage").sprite,"Reuse trophy");
   history.verticalNormalizedPosition=1;active.verticalNormalizedPosition=1;UpdateLayout();
   Capture("challenge-lobby-collapsed");
   // Continue interaction using the screen-space camera after the capture.
   camera.cullingMask=-1;camera.orthographic=false;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=texture;
   first=((ScrollRect)Get(lobby,"activeScroll")).content.GetChild(0);Tap(first.Find("Summary/Open result").GetComponent<Button>());
   Check((int)Get(lobby,"selectedId")==1 && ((GameObject)Get(lobby,"list")).transform.childCount==1,"Row opens match without stale inbox layers");var detail=(ScrollRect)Get(lobby,"detailScroll");var choices=detail.content.Find("Games");Check(choices!=null && choices.childCount==3,"Only remaining games offered");
   foreach(var pair in new[]{new{game="Pairs",colour=(Color)new Color32(36,189,224,255)},new{game="Boxes",colour=(Color)new Color32(234,24,100,255)},new{game="Chain",colour=(Color)new Color32(123,31,255,255)}}){var button=choices.Find(pair.game).GetComponent<Button>();Check(button.image.color==pair.colour && button.transform.Find("Flat game symbol")!=null,"Original coloured game tile "+pair.game);}
   Tap(root.transform.Find("Safe area/Original back").GetComponent<Button>());Check((int)Get(lobby,"selectedId")==0 && Get(lobby,"activeScroll")!=null,"Back returns from match to inbox");
   var keys=(HashSet<string>)Get(lobby,"expanded");keys.Add("sprint:20");Call(lobby,"RenderCurrent");((ScrollRect)Get(lobby,"historyScroll")).verticalNormalizedPosition=1;UpdateLayout();Capture("challenge-lobby-expanded");
   Call(lobby,"ShowBattle",fixtures[0]);UpdateLayout();Capture("challenge-match-games");
   var stats=nav.GetComponent<StatisticsView>();var originalStats=(GameObject)Get(stats,"challengeStatsPanel");
   var clone=UnityEngine.Object.Instantiate(originalStats,canvas.transform,false);clone.name="Statistics review";clone.SetActive(true);var sr=(RectTransform)clone.transform;sr.anchorMin=sr.anchorMax=new Vector2(.5f,.5f);sr.sizeDelta=new Vector2(760,300);sr.anchoredPosition3D=Vector3.zero;sr.localScale=Vector3.one;Set(stats,"challengeStatsPanel",clone);
   stats.ApplyChallengeCounters(new UserStatistics{numberOfChallenges=12,numberOfMatchChallenges=4,numberOfSingleChallenges=8,numberOfLossChallenges=2,numberOfDrawChallenges=1,challengeFormatsAvailable=true});
   var formats=clone.transform.Find("Challenges statistics/Challenge format stats");Check(formats!=null && formats.childCount==5,"Five Statistics fields");
   Check(formats.Find("Matches/Value").GetComponent<Text>().text=="4" && formats.Find("Single/Value").GetComponent<Text>().text=="8","Format counters not relabelled old wins");
   Check(formats.GetComponentsInChildren<Text>().All(t=>t.color==Color.white),"Statistics text white");
   foreach(Transform child in canvas.transform)if(child!=clone.transform)child.gameObject.SetActive(false);Capture("challenge-statistics");
   stats.ApplyChallengeCounters(new UserStatistics{numberOfChallenges=12});formats=clone.transform.Find("Challenges statistics/Challenge format stats");Check(formats.Find("Matches/Value").GetComponent<Text>().text=="—" && formats.Find("Single/Value").GetComponent<Text>().text=="—","Old API must not invent format counts");Set(stats,"challengeStatsPanel",originalStats);UnityEngine.Object.DestroyImmediate(clone);
   Debug.Log("SPRINT_LOBBY_UI_VALIDATION_PASSED: real dropdown/back taps, independent scrolling, inline facts, colours, trophy and shared game tiles");
  }finally{UserConstants.s_user=old;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(texture);}
 }
}
