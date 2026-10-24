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
public static class LiveSocialValidation {
 static object Get(object o,string name){return o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);}
 static void Set(object o,string name,object value){o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);}
 static void Check(bool value,string why){if(!value)throw new Exception(why);}
 public static void Review(){
  var old=UserConstants.s_user;UserConstants.s_user=new User{id=1999999977,username="Idriz"};
  if(LiveChallenges.Current!=null)UnityEngine.Object.DestroyImmediate(LiveChallenges.Current.gameObject);
  EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
  var nav=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().First(n=>n.gameObject.scene.IsValid());var canvas=nav.challengeGameScreen.GetComponentsInParent<Canvas>(true).First().rootCanvas;
  SprintLobby.Attach(nav);var live=LiveChallenges.Current;
  var state=new SocialSettings{friendsVisible=true,visible=false,declined=3,friends=new[]{new SocialPlayer{id=2,username="Hanushe",requesterId=2}},blocked=new SocialPlayer[0]};
  Set(live,"settings",state);Set(live,"loaded",true);Set(live,"account",1999999977);Set(live,"online",new OnlinePlayers{players=new[]{new SocialPlayer{id=2,username="Hanushe",favorite=true}}});
  var requests=new List<string>();SprintAPI.EditorTransport=(path,body,success,fail)=>{requests.Add(path+":"+body);if(path=="/social/heartbeat"){var heart=JsonUtility.FromJson<SocialHeartbeat>(body);state.visible=heart.visible;state.friendsVisible=heart.friendsVisible;}success(JsonUtility.ToJson(state));};
  live.Open();var panel=(GameObject)Get(live,"panel");
  for(var t=panel.transform;t!=canvas.transform;t=t.parent){foreach(Transform sibling in t.parent)if(sibling!=t)sibling.gameObject.SetActive(false);t.gameObject.SetActive(true);}
  var cameraObject=new GameObject("Live review camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;
  var texture=new RenderTexture(945,2048,24);camera.targetTexture=texture;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
  var events=UnityEngine.Object.FindObjectOfType<EventSystem>();if(events==null)events=new GameObject("Live events",typeof(EventSystem)).GetComponent<EventSystem>();
  Action<Button> tap=button=>{Canvas.ForceUpdateCanvases();foreach(var t in panel.GetComponentsInChildren<TouchTarget>())t.Refresh();Canvas.ForceUpdateCanvases();camera.Render();var rect=(RectTransform)button.transform;var point=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));var hits=new List<RaycastResult>();canvas.GetComponent<GraphicRaycaster>().Raycast(new PointerEventData(events){position=point},hits);var handler=hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);Check(handler==button.gameObject,"Blocked live control: "+button.name+" by "+(hits.Count==0?"nothing":hits[0].gameObject.name));ExecuteEvents.Execute(handler,new PointerEventData(events){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);};
  try{
   Check(panel.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Only you can see")),"Private decline label");
   Check(panel.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Friends share online")),"Friend acceptance disclosure");
   tap(panel.transform.Find("Everyone").GetComponent<Button>());Check(state.visible,"Public visibility can turn on");
   tap(panel.transform.Find("Friends").GetComponent<Button>());Check(!state.friendsVisible && state.visible,"Independent audiences");
   tap(panel.transform.Find("Everyone").GetComponent<Button>());Check(!state.visible && !state.friendsVisible,"Full invisibility");
   var yes=panel.GetComponentsInChildren<Button>().First(b=>b.name=="Yes");tap(yes);Check(requests.Any(r=>r.Contains("/social/friend")&&r.Contains("accept")),"Accept friend request reaches API");
   var block=panel.GetComponentsInChildren<Button>().First(b=>b.name=="Block");tap(block);var modal=(GameObject)Get(live,"banner");Check(modal!=null && modal.GetComponent<Image>().raycastTarget,"Blocking modal shields screen");tap(modal.GetComponentsInChildren<Button>().First(b=>b.name=="No"));
   Check(Get(live,"banner")==null,"No dismisses block without sending");
   canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;((RectTransform)canvas.transform).sizeDelta=new Vector2(800,2048*800f/945);canvas.GetComponent<CanvasScaler>().enabled=false;camera.orthographic=true;camera.cullingMask=1<<30;
   typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas.gameObject,camera,"challenge-online-friends",945,2048});
   Debug.Log("LIVE_SOCIAL_UI_PASSED: real visibility/friend/block taps, independent privacy, private declines, modal shielding");
  }finally{SprintAPI.EditorTransport=null;UserConstants.s_user=old;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(live.gameObject);}
 }
}
