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
 SprintAPI api;int battle,account;GameObject tray,toast;Image toastImage;Text notice;Button opener;Button[] buttons;float pollAt,unlockAt,toastUntil;bool polling,sending;string deliveredKey;
 public static SprintReactions Attach(Transform parent,int id){
  var old=parent.GetComponentInChildren<SprintReactions>(true);
  if(old!=null && old.battle==id){old.gameObject.SetActive(true);old.transform.SetAsLastSibling();return old;}
  if(old!=null){old.gameObject.SetActive(false);Destroy(old.gameObject);}
  var go=new GameObject("Match reactions",typeof(RectTransform));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
  var component=go.AddComponent<SprintReactions>();component.battle=id;component.account=GameManager.AchievementUserId;component.api=go.AddComponent<SprintAPI>();component.Build();return component;
 }
 public static Sprite Icon(string key){return Resources.Load<Sprite>("Reactions/"+key);}
 void Build(){
  opener=SprintUi.Button(transform,"Reactions",new Vector2(.90f,.76f),new Vector2(.99f,.815f),()=>tray.SetActive(!tray.activeSelf));
  Picture(opener.transform,"happy",new Vector2(.12f,.10f),new Vector2(.88f,.90f));foreach(var text in opener.GetComponentsInChildren<Text>())text.gameObject.SetActive(false);SprintUi.Edges(opener.image);
  tray=ChainGame.Panel(transform,"Six reactions",new Vector2(.54f,.43f),new Vector2(.99f,.75f),Color.white);SprintUi.Edges(tray.GetComponent<Image>());
  buttons=new Button[Keys.Length];for(int i=0;i<Keys.Length;i++){
   string key=Keys[i];int col=i%2,row=i/2;var button=SprintUi.Button(tray.transform,key,new Vector2(.04f+col*.48f,.34f+(2-row)*.215f),new Vector2(.48f+col*.48f,.54f+(2-row)*.215f),()=>Send(key));
   foreach(var text in button.GetComponentsInChildren<Text>())text.gameObject.SetActive(false);button.image.color=new Color32(245,239,255,255);SprintUi.Edges(button.image);Picture(button.transform,key,new Vector2(.18f,.1f),new Vector2(.82f,.9f));buttons[i]=button;
  }
  notice=ChainGame.Label(tray.transform,"",new Vector2(.03f,.13f),new Vector2(.97f,.32f),19,ChainGame.Ink);
  var credit=SprintUi.Button(tray.transform,"Twemoji · CC BY4.0",new Vector2(.02f,.01f),new Vector2(.98f,.12f),()=>Application.OpenURL("https://github.com/twitter/twemoji/blob/v14.0.2/LICENSE-GRAPHICS"));credit.image.color=Color.clear;var label=credit.GetComponentInChildren<Text>();label.fontSize=14;label.resizeTextMaxSize=14;label.color=ChainGame.Ink;
  tray.SetActive(false);
  toast=ChainGame.Panel(transform,"Opponent reaction",new Vector2(.76f,.82f),new Vector2(.89f,.89f),new Color(1,1,1,.95f));SprintUi.Edges(toast.GetComponent<Image>());toast.GetComponent<Image>().raycastTarget=false;toastImage=Picture(toast.transform,"happy",new Vector2(.1f,.1f),new Vector2(.9f,.9f));toast.SetActive(false);
  deliveredKey="sprint.reaction.seen."+account+"."+battle;
 }
 static Image Picture(Transform parent,string key,Vector2 min,Vector2 max){var image=ChainGame.Panel(parent,"Emoji "+key,min,max,Color.white).GetComponent<Image>();image.sprite=Icon(key);image.preserveAspect=true;image.raycastTarget=false;return image;}
 void Update(){
  if(account!=GameManager.AchievementUserId){gameObject.SetActive(false);return;}
  float left=Mathf.Max(0,unlockAt-Time.unscaledTime);foreach(var button in buttons)button.interactable=!sending && left<=0;
  if(left>0)notice.text=Mathf.CeilToInt(left)+"s";else if(!sending && notice.text.EndsWith("s"))notice.text="";
  if(toast.activeSelf && Time.unscaledTime>=toastUntil)toast.SetActive(false);
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
  toastImage.sprite=Icon(reaction.key);toast.SetActive(true);toastUntil=Time.unscaledTime+2.8f;PlayerPrefs.SetString(deliveredKey,reaction.at.ToString());PlayerPrefs.Save();
 }
}
