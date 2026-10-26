using System;using System.Linq;using System.Reflection;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.UI;using Assets.Script.Constants;using Assets.Script.Models;
public static class RematchArenaValidation {
 static void Set(object o,string key,object value){o.GetType().GetField(key,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,value);}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Capture(GameObject screen,string name,int width,int height){
  var canvas=screen.GetComponentsInParent<Canvas>(true).Last().rootCanvas;
  for(var t=screen.transform;t!=canvas.transform;t=t.parent){foreach(Transform sibling in t.parent)if(sibling!=t)sibling.gameObject.SetActive(false);t.gameObject.SetActive(true);}
  canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;((RectTransform)canvas.transform).sizeDelta=new Vector2(800,height*800f/width);var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;
  var go=new GameObject("Arena details camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;
  typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{canvas.gameObject,camera,name,width,height});UnityEngine.Object.DestroyImmediate(go);
 }
 public static void Review(){
  var prior=UserConstants.s_user;try{
   UserConstants.s_user=new User{id=1999999918,username="Arena review",userStatistics=new UserStatistics{level=15,xp=1466}};EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var nav=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().First(n=>n.gameObject.scene.IsValid());SprintLobby.Attach(nav);var live=LiveChallenges.Current;
   var menu=nav.GetComponent<MenuNavigation>();TipsEnhancer.Ensure(menu);TipsEnhancer.Ensure(menu);var tips=menu.tipsScreen.transform.Find("Scroll View/Viewport/Content");var major=tips.Find("MajorSystem");var deck=tips.Find("TheDeckGuide");
   Check(deck.GetSiblingIndex()==major.GetSiblingIndex()+1,"Deck must immediately follow Major System, also after repeated Ensure");Check(deck.Find("Deck icon").GetComponentsInChildren<Image>().Length==3,"Deck icon reuses three actual card faces");Check(tips.GetComponentsInChildren<Button>(true).Count(b=>b.name=="TheDeckGuide")==1,"One Deck button only");
   var palace=Enumerable.Range(0,tips.childCount).Select(i=>tips.GetChild(i)).FirstOrDefault(t=>t.name.Contains("Palace"));if(palace!=null)Check(palace.GetSiblingIndex()>deck.GetSiblingIndex(),"Deck precedes Memory Palace");
   Set(live,"online",new OnlinePlayers{players=new[]{new SocialPlayer{id=202,username="Online player"}}});
   nav.SetFavorites(new UserSerializable{users=new[]{new User{id=201,username="Offline player",avatar="MaskGroup54",userStatistics=new UserStatistics{level=15,xp=1466}},new User{id=202,username="Online player",avatar="MaskGroup54",userStatistics=new UserStatistics{level=15,xp=1466}}}});
   nav.NoChallenge.SetActive(false);var content=nav.favoriteContent;Canvas.ForceUpdateCanvases();var first=content.GetChild(0);Check(first.name=="Online players divider","Online section first");Check(content.GetChild(1).GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("Online player")),"Online row first even if server returns offline first");Check(content.GetChild(2).name=="Offline players divider","Offline section below online rows");
   foreach(var icon in content.GetComponentsInChildren<PresenceIndicator>()){
    var label=icon.transform.parent.GetComponent<Text>();var corners=new Vector3[4];label.rectTransform.GetWorldCorners(corners);float end=corners[2].x;icon.rectTransform.GetWorldCorners(corners);Check(corners[0].x>end,"Wifi cannot overlap status text");Check(!icon.raycastTarget,"Wifi cannot block sword");
   }
   Capture(nav.challengeHomeScreen,"arena-rematch-phone",945,2048);Capture(nav.challengeHomeScreen,"arena-rematch-ipad",1024,1366);
   Capture(menu.tipsScreen,"tips-deck-icon-phone",945,2048);
   Set(live,"online",new OnlinePlayers{players=new[]{new SocialPlayer{id=201}}});ArenaHomeLayout.RefreshPlayers(content);Check(content.GetChild(1).GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("Offline player")),"Live status changes reorder existing rows");
   LiveRecoveryValidation.Review();Debug.Log("REMATCH_ARENA_VALIDATION_PASSED: Deck order and icon, online-first with green/red dividers, status badge clearance, presence reordering and pending recovery");
  }finally{UserConstants.s_user=prior;SprintAPI.EditorTransport=null;}
 }
}
