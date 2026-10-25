using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable] public class SprintReaction {public int sender;public string key;public long at;}
[Serializable] public class SprintReactionFeed {public bool friends,playing;public long serverTime,cooldownUntil;public SprintReaction[] events;}
[Serializable] public class SprintReact {public string key;}

// Cosmetic only: the server decides when a recipient may see each reaction.
public sealed class SprintReactions : MonoBehaviour {
 public static readonly string[] Keys={"happy","angry","love","brain","clap","thumb"};
 SprintAPI api;int battle,account;GameObject tray,toast;Image toastImage;Text notice;Button[] buttons;float pollAt,unlockAt,flyStarted;Vector2 flyStart;CanvasGroup flyGroup;bool polling,sending;string deliveredKey;
 public static SprintReactions Attach(Transform parent,int id){
  var old=parent.GetComponentInChildren<SprintReactions>(true);
  if(old!=null && old.battle==id){old.gameObject.SetActive(true);old.transform.SetAsLastSibling();return old;}
  if(old!=null){old.gameObject.SetActive(false);Destroy(old.gameObject);}
  var go=new GameObject("Match reactions",typeof(RectTransform));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
  var component=go.AddComponent<SprintReactions>();component.battle=id;component.account=GameManager.AchievementUserId;component.api=go.AddComponent<SprintAPI>();component.Build();return component;
 }
 public static Sprite Icon(string key){return Resources.Load<Sprite>("Reactions/"+key);}
 void Build(){
  tray=ChainGame.Panel(transform,"Six reactions",new Vector2(.05f,.015f),new Vector2(.95f,.075f),new Color(1,1,1,.96f));SprintUi.Edges(tray.GetComponent<Image>());
  buttons=new Button[Keys.Length];for(int i=0;i<Keys.Length;i++){
   string key=Keys[i];var button=SprintUi.Button(tray.transform,key,new Vector2(i/6f+.008f,.24f),new Vector2((i+1)/6f-.008f,.98f),()=>Send(key));
   foreach(var text in button.GetComponentsInChildren<Text>())text.gameObject.SetActive(false);button.image.color=new Color32(245,239,255,255);SprintUi.Edges(button.image);Picture(button.transform,key,new Vector2(.21f,.17f),new Vector2(.79f,.83f));buttons[i]=button;
  }
  notice=ChainGame.Label(tray.transform,"",new Vector2(.66f,0),new Vector2(.99f,.24f),18,ChainGame.Ink);
  var credit=SprintUi.Button(tray.transform,"Twemoji · CC BY4.0",new Vector2(.01f,0),new Vector2(.55f,.24f),()=>Application.OpenURL("https://github.com/twitter/twemoji/blob/v14.0.2/LICENSE-GRAPHICS"));credit.image.color=Color.clear;var label=credit.GetComponentInChildren<Text>();label.fontSize=14;label.resizeTextMaxSize=14;label.color=ChainGame.Ink;
  toastImage=Picture(transform,"happy",new Vector2(.5f,.5f),new Vector2(.5f,.5f));toast=toastImage.gameObject;toast.name="Opponent reaction";toastImage.rectTransform.sizeDelta=new Vector2(62,62);flyGroup=toast.AddComponent<CanvasGroup>();flyGroup.blocksRaycasts=false;toast.SetActive(false);
  deliveredKey="sprint.reaction.seen."+account+"."+battle;
 }
 static Image Picture(Transform parent,string key,Vector2 min,Vector2 max){var image=ChainGame.Panel(parent,"Emoji "+key,min,max,Color.white).GetComponent<Image>();image.sprite=Icon(key);image.preserveAspect=true;image.raycastTarget=false;return image;}
 void LateUpdate(){
  var host=(RectTransform)transform;var canvas=host.GetComponentInParent<Canvas>().rootCanvas;float scale=Mathf.Max(.01f,canvas.scaleFactor),height=host.rect.height;if(height<=0)return;
  float bottom=Screen.height>0?(Screen.safeArea.yMin+8)/(float)Screen.height:.015f;var rect=(RectTransform)tray.transform;rect.anchorMin=new Vector2(.05f,bottom);rect.anchorMax=new Vector2(.95f,bottom+90/height);
  if(!SprintRuntime.Active)return;
  if(SprintRuntime.Current.Game=="Boxes"){
   var boxes=UnityEngine.Object.FindObjectOfType<BoxesManager>();if(boxes!=null && boxes.scrollViewContent!=null){var scroll=boxes.scrollViewContent.GetComponentInParent<ScrollRect>();if(scroll!=null && scroll.horizontalScrollbar!=null){var bar=scroll.horizontalScrollbar;scroll.horizontalScrollbar=null;bar.gameObject.SetActive(false);}float floor=Screen.safeArea.yMin+8+102*scale;if(boxes.nextPosBottom!=null)KeepAboveDock(boxes.nextPosBottom.transform as RectTransform,floor);if(boxes.recallBtn!=null && !boxes.recallBtn.activeInHierarchy)KeepAboveDock(boxes.bottomIconsContainer.transform as RectTransform,floor);}
  }
  if(SprintRuntime.Current.Game=="Flash")return;
  var next=UnityEngine.Object.FindObjectsOfType<Button>().Where(b=>!b.transform.IsChildOf(transform) && (b.name.ToLowerInvariant().Contains("next") || b.name.ToLowerInvariant().Contains("recall"))).Where(b=>b.gameObject.activeInHierarchy).OrderBy(b=>b.transform.position.y).FirstOrDefault();
  if(next==null)return;
  var corners=new Vector3[4];((RectTransform)next.transform).GetWorldCorners(corners);var source=next.GetComponentInParent<Canvas>().rootCanvas;var camera=source.renderMode==RenderMode.ScreenSpaceOverlay?null:source.worldCamera;
  float controlBottom=RectTransformUtility.WorldToScreenPoint(camera,corners[0]).y;float dockTop=Screen.safeArea.yMin+8+90*scale;
  if(controlBottom<dockTop+12*scale){var pivot=RectTransformUtility.WorldToScreenPoint(camera,next.transform.position);pivot.y+=dockTop+12*scale-controlBottom;Vector3 world;RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)next.transform.parent,pivot,camera,out world);next.transform.position=world;}
 }
 static void KeepAboveDock(RectTransform target,float floor){
  if(target==null)return;var canvas=target.GetComponentInParent<Canvas>().rootCanvas;var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;var corners=new Vector3[4];target.GetWorldCorners(corners);float bottom=RectTransformUtility.WorldToScreenPoint(camera,corners[0]).y;if(bottom>=floor)return;var point=RectTransformUtility.WorldToScreenPoint(camera,target.position);point.y+=floor-bottom;Vector3 world;RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)target.parent,point,camera,out world);target.position=world;
 }

 void Update(){
  if(account!=GameManager.AchievementUserId){gameObject.SetActive(false);return;}
  float left=Mathf.Max(0,unlockAt-Time.unscaledTime);foreach(var button in buttons)button.interactable=!sending && left<=0;
  if(left>0)notice.text=Mathf.CeilToInt(left)+"s";else if(!sending && notice.text.EndsWith("s"))notice.text="";
  if(toast.activeSelf){float t=Mathf.Clamp01((Time.unscaledTime-flyStarted)/1.4f);toastImage.rectTransform.anchoredPosition=flyStart+Vector2.up*(150*t);flyGroup.alpha=1-t;if(t>=1)toast.SetActive(false);}
  if(!polling && Time.unscaledTime>=pollAt){polling=true;pollAt=Time.unscaledTime+2;api.Request<SprintReactionFeed>("/"+battle+"/reactions",null,value=>{if(this==null)return;polling=false;Receive(value);},error=>{if(this==null)return;polling=false;pollAt=Time.unscaledTime+5;});}
 }
 void Send(string key){
  if(sending || Time.unscaledTime<unlockAt)return;sending=true;unlockAt=Time.unscaledTime+5;notice.text="Sending";
  api.Request<SprintReactionFeed>("/"+battle+"/reactions",JsonUtility.ToJson(new SprintReact{key=key}),value=>{if(this==null)return;sending=false;notice.text=value.friends?"Sent":"Sent · Start/end";Receive(value);},error=>{if(this==null)return;sending=false;notice.text=error;});
 }
 void Receive(SprintReactionFeed feed){
  unlockAt=Mathf.Max(unlockAt,Time.unscaledTime+Mathf.Max(0,(feed.cooldownUntil-feed.serverTime)/1000f));
  // A forced app close leaves a persistent seen cursor; messages never replay on reopen.
  if(toast.activeSelf || (feed.playing && !feed.friends))return;
  long seen;long.TryParse(PlayerPrefs.GetString(deliveredKey,"0"),out seen);
  var reaction=(feed.events??new SprintReaction[0]).Where(r=>r.at>seen && r.sender!=account && Keys.Contains(r.key)).OrderBy(r=>r.at).FirstOrDefault();if(reaction==null)return;
  toastImage.sprite=Icon(reaction.key);int slot=Array.IndexOf(Keys,reaction.key);flyStart=((RectTransform)transform).InverseTransformPoint(buttons[slot].transform.position);toastImage.rectTransform.anchoredPosition=flyStart;flyGroup.alpha=1;flyStarted=Time.unscaledTime;toast.SetActive(true);PlayerPrefs.SetString(deliveredKey,reaction.at.ToString());PlayerPrefs.Save();
 }
}
