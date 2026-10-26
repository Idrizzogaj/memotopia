using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;
using UnityEngine.UI;

[Serializable] public class SocialPlayer {public int id;public string username;public bool favorite,friend,accepted;public int requesterId;}
[Serializable] public class SocialSettings {public bool visible,friendsVisible=true;public int declined;public SocialPlayer[] blocked,friends;}
[Serializable] public class OnlinePlayers {public SocialPlayer[] players;}
[Serializable] public class SocialHeartbeat {public bool active,visible,friendsVisible;}
[Serializable] public class SocialBlock {public int targetId;public bool blocked;}
[Serializable] public class SocialFriend {public int targetId;public string action;}
[Serializable] public class SprintResponse {public bool accept;}
[Serializable] public class SprintProgress {public int legIndex,ms,units;}

// Survives scene changes, but only leases presence while the authenticated app is foregrounded.
public sealed class LiveChallenges : MonoBehaviour {
 public static LiveChallenges Current {get;private set;}
 private SprintAPI api;private ChallengeNavigation navigation;private SocialSettings settings=new SocialSettings();
 private OnlinePlayers online=new OnlinePlayers();private bool focused=true,paused,busy,loaded,polling,acting;private int account;
 private readonly List<PresenceLabel> labels=new List<PresenceLabel>();
 private sealed class PresenceLabel {public Text label;public int id;public string name;}
 private static string Status(int id,string name){return (string.IsNullOrEmpty(name)?"":name+" · ")+(IsOnline(id)?"Online":"Offline");}
 public static void Watch(Text label,int id,string name){label.text=Status(id,name);PresenceIndicator.Set(label,IsOnline(id));if(Current!=null)Current.labels.Add(new PresenceLabel{label=label,id=id,name=name});}
 private string sessionToken;
 private float beatAt,pollAt;private GameObject panel,banner;private Transform content;private Text notice;private int showingInvite;
 public static LiveChallenges Attach(ChallengeNavigation navigation){
  if(Current==null){var root=new GameObject("Live Challenges");if(Application.isPlaying)DontDestroyOnLoad(root);Current=root.AddComponent<LiveChallenges>();Current.api=root.AddComponent<SprintAPI>();}
  Current.navigation=navigation;
  SprintUi.Button(navigation.challengeHomeScreen.transform,"Online",new Vector2(.12f,.77f),new Vector2(.88f,.82f),Current.Open);
  return Current;
 }
 public static bool IsOnline(int id){return Current!=null && Current.online.players!=null && Current.online.players.Any(p=>p.id==id);}
 public static bool Visible {get{return Current!=null && Current.loaded && (Current.settings.visible || Current.settings.friendsVisible);}}
 public void Profile(Transform parent,int id){
  var old=parent.Find("Block");if(old!=null)Remove(old.gameObject);
  var friendOld=parent.Find("Friend");if(friendOld!=null)Remove(friendOld.gameObject);
  var relation=(settings.friends??new SocialPlayer[0]).FirstOrDefault(p=>p.id==id);
  var friend=SprintUi.Button(parent,"Friend",new Vector2(.05f,.015f),new Vector2(.30f,.065f),()=>ConfirmFriend(id));
  var friendText=friend.GetComponentInChildren<Text>();friendText.text=relation==null?"Friend":relation.accepted?"Friends":relation.requesterId==account?"Already sent":"Request";
  friend.interactable=relation==null || (!relation.accepted && relation.requesterId!=account);
  SprintUi.Button(parent,"Block",new Vector2(.32f,.015f),new Vector2(.68f,.065f),()=>ConfirmBlock(id));
  var oldStatus=parent.Find("Player presence");if(oldStatus!=null)Remove(oldStatus.gameObject);
  var name=parent.GetComponentsInChildren<Text>(true).First();
  var status=ChainGame.Label(parent,"",new Vector2(.2f,.48f),new Vector2(.8f,.53f),26,Muted);status.name="Player presence";
  if(name.transform.parent==parent){var r=status.rectTransform;var original=name.rectTransform;r.anchorMin=original.anchorMin;r.anchorMax=original.anchorMax;r.pivot=original.pivot;r.sizeDelta=original.sizeDelta;r.anchoredPosition=original.anchoredPosition-new Vector2(0,original.rect.height*.75f);}
  Watch(status,id,"");
 }
 private void Update(){
  int user=GameManager.AchievementUserId;
  string token=PlayerPrefs.GetString("token");
  if(user!=account || token!=sessionToken){account=user;sessionToken=token;busy=polling=acting=false;loaded=false;settings=new SocialSettings();online=new OnlinePlayers();beatAt=pollAt=0;Close();HideInvite();}
  if(user<=0 || string.IsNullOrEmpty(PlayerPrefs.GetString("token")) || paused || !focused)return;
  if(!loaded && !busy && Time.unscaledTime>=beatAt){busy=true;api.Request<SocialSettings>("/social",null,value=>{settings=value;loaded=true;busy=false;beatAt=0;},error=>{busy=false;beatAt=Time.unscaledTime+15;});}
  if(loaded && !busy && Time.unscaledTime>=beatAt){beatAt=Time.unscaledTime+15;Heartbeat(settings.visible);}
  if(Time.unscaledTime>=pollAt && !polling){pollAt=Time.unscaledTime+2;Poll();}
  if(panel!=null && Input.GetKeyDown(KeyCode.Escape))Close();
 }
 private void Heartbeat(bool visible){
  busy=true;api.Request<SocialSettings>("/social/heartbeat",JsonUtility.ToJson(new SocialHeartbeat{visible=visible,friendsVisible=settings.friendsVisible,active=focused&&!paused}),value=>{settings=value;busy=false;if(panel!=null)Render();},error=>{busy=false;if(notice!=null)notice.text=error;});
 }
 private void OnApplicationPause(bool value){paused=value;LeaseChanged();}
 private void OnApplicationFocus(bool value){focused=value;LeaseChanged();}
 private void LeaseChanged(){if(api!=null && loaded && account==GameManager.AchievementUserId && !busy)Heartbeat(settings.visible);beatAt=0;pollAt=0;}
 private void Poll(){
  polling=true;api.Request<OnlinePlayers>("/social/online",null,value=>{online=value;labels.RemoveAll(p=>p.label==null);foreach(var p in labels){p.label.text=Status(p.id,p.name);PresenceIndicator.Set(p.label,IsOnline(p.id));}if(navigation!=null)ArenaHomeLayout.RefreshPlayers(navigation.favoriteContent);if(panel!=null)Render();},error=>{});
  api.Request<SprintBattleList>("",null,value=>{
   polling=false;
   var invite=(value.battles??new SprintBattleView[0]).FirstOrDefault(b=>!b.done&&b.invitation=="pending"&&b.recipientId==account);
   if(invite==null){if(showingInvite>0)HideInvite();return;}
   if(!SprintRuntime.Active && panel==null && navigation!=null && showingInvite==0)ShowInvite(invite);
  },error=>{polling=false;});
 }
 private Transform Root(){return navigation==null?null:navigation.challengeGameScreen.GetComponentInParent<Canvas>().rootCanvas.transform;}
 private static readonly Color Paper=new Color32(246,243,252,255), Muted=new Color32(102,94,119,255), Danger=new Color32(234,24,100,255);
 private static Button ActionButton(Transform parent,string caption,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action,bool primary=false,bool danger=false){
  var button=SprintUi.Button(parent,caption,min,max,action);button.image.color=primary?(danger?Danger:TrainingHubLayout.MemoryTone):(Color)new Color32(233,228,242,255);
  foreach(var label in button.GetComponentsInChildren<Text>()){label.fontSize=28;label.resizeTextMaxSize=28;label.color=primary?Color.white:ChainGame.Ink;}
  if(caption=="Challenge" && Current!=null && Current.navigation!=null && Current.navigation.challengeOnProfile!=null){
   var original=Current.navigation.challengeOnProfile;var source=original.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.transform!=original.transform && i.sprite!=null);
   if(source==null && Current.navigation.challengeUserProfileScreen!=null)source=Current.navigation.challengeUserProfileScreen.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.name=="TrainingImage" && i.sprite!=null);
   if(source!=null){var icon=ChainGame.Panel(button.transform,"Original challenge icon",new Vector2(.04f,.16f),new Vector2(.21f,.84f),Color.white).GetComponent<Image>();icon.sprite=source.sprite;icon.preserveAspect=true;icon.raycastTarget=false;foreach(var label in button.GetComponentsInChildren<Text>())label.rectTransform.anchorMin=new Vector2(.22f,.08f);}
  }
  return button;
 }
 private static void Section(Transform parent,string caption,int count){
  var bar=ChainGame.Panel(parent,caption+" section",Vector2.zero,Vector2.one,TrainingHubLayout.MemoryTone);bar.GetComponent<Image>().raycastTarget=false;bar.AddComponent<LayoutElement>().preferredHeight=40;
  var title=ChainGame.Label(bar.transform,caption.ToUpperInvariant(),new Vector2(.035f,0),new Vector2(.85f,1),25,Color.white);title.alignment=TextAnchor.MiddleLeft;title.fontStyle=FontStyle.Bold;
  ChainGame.Label(bar.transform,count.ToString(),new Vector2(.86f,0),new Vector2(.97f,1),23,Color.white);
 }
 private static RectTransform PlayerCard(Transform parent,SocialPlayer player,string status,bool? presence=null){
  var card=ChainGame.Panel(parent,"Player "+player.id,Vector2.zero,Vector2.one,Paper);SprintUi.Edges(card.GetComponent<Image>());card.AddComponent<LayoutElement>().preferredHeight=150;
  var name=ChainGame.Label(card.transform,player.username,new Vector2(.04f,.59f),new Vector2(.96f,.97f),31,ChainGame.Ink);name.alignment=TextAnchor.MiddleLeft;name.fontStyle=FontStyle.Bold;name.supportRichText=false;
  var label=ChainGame.Label(card.transform,status,new Vector2(.04f,.38f),new Vector2(.96f,.60f),22,Muted);label.alignment=TextAnchor.MiddleLeft;if(presence.HasValue)PresenceIndicator.Set(label,presence.Value);
  return (RectTransform)card.transform;
 }
 private static void Note(Transform parent,string caption,int height=54){
  var label=ChainGame.Label(parent,caption,Vector2.zero,Vector2.one,22,Muted);label.gameObject.AddComponent<LayoutElement>().preferredHeight=height;
 }
 private void Audience(string name,bool on){
  var button=panel.transform.Find(name).GetComponent<Button>();button.image.color=on?TrainingHubLayout.MemoryTone:(Color)new Color32(233,228,242,255);
  var title=button.GetComponentInChildren<Text>();title.text=name;title.fontSize=30;title.resizeTextMaxSize=30;title.color=on?Color.white:ChainGame.Ink;title.rectTransform.anchorMin=new Vector2(.05f,.38f);title.rectTransform.anchorMax=new Vector2(.95f,.94f);
  var status=button.transform.Find("Visibility state");if(status==null)status=ChainGame.Label(button.transform,"",new Vector2(.05f,.05f),new Vector2(.95f,.40f),20,Color.white).transform;
  status.name="Visibility state";var text=status.GetComponent<Text>();text.text=on?"ON":"OFF";text.color=on?Color.white:Muted;
 }
 public void Open(){
  if(navigation==null)return;HideInvite();Close();panel=ChainGame.Panel(Root(),"Online players",Vector2.zero,Vector2.one,Color.white);
  SprintUi.Back(panel.transform,Close);var title=ChainGame.Label(panel.transform,"ONLINE",new Vector2(.2f,.91f),new Vector2(.80f,.98f),40,TrainingHubLayout.MemoryTone);title.fontStyle=FontStyle.Bold;
  var bar=ChainGame.Panel(panel.transform,"Visibility section",new Vector2(.06f,.855f),new Vector2(.94f,.88f),TrainingHubLayout.MemoryTone);bar.GetComponent<Image>().raycastTarget=false;ChainGame.Label(bar.transform,"VISIBILITY",Vector2.zero,Vector2.one,22,Color.white).fontStyle=FontStyle.Bold;
  ActionButton(panel.transform,"Everyone",new Vector2(.06f,.765f),new Vector2(.49f,.845f),()=>{if(!busy && loaded)Heartbeat(!settings.visible);});
  ActionButton(panel.transform,"Friends",new Vector2(.51f,.765f),new Vector2(.94f,.845f),()=>{if(!busy && loaded){settings.friendsVisible=!settings.friendsVisible;Heartbeat(settings.visible);}});
  ChainGame.Label(panel.transform,"Choose who can see you online",new Vector2(.06f,.725f),new Vector2(.94f,.76f),22,Muted);
  notice=ChainGame.Label(panel.transform,"",new Vector2(.06f,.025f),new Vector2(.94f,.065f),20,Muted);
  content=SprintUi.Scroll(panel.transform,"Players",new Vector2(.06f,.085f),new Vector2(.94f,.715f)).content;Render();pollAt=0;
 }
 private void Render(){
  if(content==null)return;var scroll=content.GetComponentInParent<ScrollRect>();float position=content.childCount==0?1:scroll.verticalNormalizedPosition;
  for(int i=content.childCount-1;i>=0;i--){var child=content.GetChild(i).gameObject;child.SetActive(false);Remove(child);}
  Audience("Everyone",settings.visible);Audience("Friends",settings.friendsVisible);
  notice.text="Declined: "+settings.declined+" · Only you can see this";
  var friends=settings.friends??new SocialPlayer[0];var requests=friends.Where(p=>!p.accepted).ToArray();var accepted=friends.Where(p=>p.accepted).ToArray();
  if(requests.Length>0){
   Section(content,"Requests",requests.Length);Note(content,"Friends share online status by default. You can turn it off above.",72);
   foreach(var player in requests){var local=player;bool incoming=player.requesterId!=account;var row=PlayerCard(content,player,incoming?"Friend request":"Request sent");
    if(incoming){ActionButton(row,"Yes",new Vector2(.04f,.07f),new Vector2(.48f,.36f),()=>Friend(local.id,"accept"),true);ActionButton(row,"No",new Vector2(.52f,.07f),new Vector2(.96f,.36f),()=>Friend(local.id,"decline"));}
    else ActionButton(row,"Remove",new Vector2(.52f,.07f),new Vector2(.96f,.36f),()=>Friend(local.id,"remove"));
   }
  }
  Section(content,"Friends",accepted.Length);
  if(accepted.Length==0)Note(content,"Accept a friend request to connect");
  foreach(var player in accepted){var local=player;var row=PlayerCard(content,player,IsOnline(player.id)?"Online":"Offline",IsOnline(player.id));ActionButton(row,"Remove",new Vector2(.52f,.07f),new Vector2(.96f,.36f),()=>Friend(local.id,"remove"));}
  var players=online.players??new SocialPlayer[0];Section(content,"Online",players.Length);
  if(players.Length==0)Note(content,"No players showing online");
  foreach(var player in players){var local=player;var row=PlayerCard(content,player,player.friend?"Online · Friend":player.favorite?"Online · Favourite":"Online",true);
   ActionButton(row,"Challenge",new Vector2(.04f,.07f),new Vector2(.61f,.36f),()=>{Close();UserConstants.s_challengeUser=new User{id=local.id,username=local.username};navigation.GoToChallengeGame();},true);
   ActionButton(row,"Block",new Vector2(.65f,.07f),new Vector2(.96f,.36f),()=>ConfirmBlock(local.id));
  }
  var blocked=settings.blocked??new SocialPlayer[0];Section(content,"Blocked",blocked.Length);
  if(blocked.Length==0)Note(content,"No blocked players");
  foreach(var player in blocked){var local=player;var row=PlayerCard(content,player,"Blocked");ActionButton(row,"Unblock",new Vector2(.52f,.07f),new Vector2(.96f,.36f),()=>Block(local.id,false));}
  Canvas.ForceUpdateCanvases();scroll.Rebuild(CanvasUpdate.PostLayout);scroll.verticalNormalizedPosition=Mathf.Clamp01(position);
 }
 private void Friend(int id,string action){if(acting)return;acting=true;api.Request<SocialSettings>("/social/friend",JsonUtility.ToJson(new SocialFriend{targetId=id,action=action}),value=>{acting=false;settings=value;if(panel==null)Open();else Render();},error=>{acting=false;if(notice!=null)notice.text=error;});}
 private Transform Dialog(string title){
  HideInvite();banner=ChainGame.Panel(Root(),title,Vector2.zero,Vector2.one,new Color(0,0,0,.55f));
  var box=ChainGame.Panel(banner.transform,"Dialog",new Vector2(.06f,.30f),new Vector2(.94f,.70f),Color.white);SprintUi.Edges(box.GetComponent<Image>());
  var bar=ChainGame.Panel(box.transform,"Dialog heading",new Vector2(.03f,.84f),new Vector2(.97f,.97f),TrainingHubLayout.MemoryTone);bar.GetComponent<Image>().raycastTarget=false;ChainGame.Label(bar.transform,title.ToUpperInvariant(),Vector2.zero,Vector2.one,24,Color.white).fontStyle=FontStyle.Bold;return box.transform;
 }
 private void ConfirmFriend(int id){
  var relation=(settings.friends??new SocialPlayer[0]).FirstOrDefault(p=>p.id==id);
  if(relation!=null){if(!relation.accepted && relation.requesterId!=account){Open();return;}var info=Dialog("Friend");showingInvite=-1;ChainGame.Label(info,relation.accepted?"Already friends":"Already sent",new Vector2(.06f,.4f),new Vector2(.94f,.8f),28,ChainGame.Ink);ActionButton(info,"OK",new Vector2(.2f,.08f),new Vector2(.8f,.3f),HideInvite);return;}
  if(Root()==null)return;var box=Dialog("Friend request");showingInvite=-1;
  ChainGame.Label(box,"Send a friend request?\nFriends can see when you are online. Turn off Friends visibility any time.",new Vector2(.06f,.38f),new Vector2(.94f,.80f),28,ChainGame.Ink);
  ActionButton(box,"No",new Vector2(.05f,.08f),new Vector2(.47f,.30f),HideInvite);
  ActionButton(box,"Send",new Vector2(.53f,.08f),new Vector2(.95f,.30f),()=>{HideInvite();Friend(id,"request");},true);
 }
 private void ConfirmBlock(int id){
  if(Root()==null)return;var box=Dialog("Block player");showingInvite=-1;
  ChainGame.Label(box,"Block this player?\nInvitations and online visibility stop both ways.",new Vector2(.06f,.40f),new Vector2(.94f,.80f),28,ChainGame.Ink);
  ActionButton(box,"No",new Vector2(.05f,.08f),new Vector2(.47f,.31f),HideInvite);
  ActionButton(box,"Block",new Vector2(.53f,.08f),new Vector2(.95f,.31f),()=>Block(id,true),true,true);
 }
 private void Block(int id,bool blocked){if(acting)return;acting=true;api.Request<SocialSettings>("/social/block",JsonUtility.ToJson(new SocialBlock{targetId=id,blocked=blocked}),value=>{acting=false;settings=value;HideInvite();pollAt=0;if(panel!=null)Render();},error=>{acting=false;if(notice!=null)notice.text=error;});}
 private void ShowInvite(SprintBattleView battle){
  var box=Dialog("Challenge invitation");showingInvite=battle.id;
  ChainGame.Label(box,battle.ownerName+"\n"+(battle.live?"Live · ":"")+battle.legs[0].game+" · "+(battle.mode=="match"?"Match!":"Single"),new Vector2(.05f,.52f),new Vector2(.95f,.80f),30,ChainGame.Ink);
  ActionButton(box,"No",new Vector2(.05f,.25f),new Vector2(.47f,.47f),()=>Respond(battle,false));
  ActionButton(box,"Yes",new Vector2(.53f,.25f),new Vector2(.95f,.47f),()=>Respond(battle,true),true);
  ActionButton(box,"Block",new Vector2(.30f,.03f),new Vector2(.70f,.20f),()=>ConfirmBlock(battle.ownerId));
 }
 public void Respond(SprintBattleView battle,bool accept){if(acting)return;acting=true;api.Request<SprintBattleView>("/"+battle.id+"/respond",JsonUtility.ToJson(new SprintResponse{accept=accept}),value=>{acting=false;HideInvite();if(accept){var lobby=SprintLobby.Attach(navigation);lobby.OpenBattle(value);}pollAt=0;},error=>{acting=false;if(notice!=null)notice.text=error;});}
 private static void Remove(GameObject value){if(value!=null){value.SetActive(false);CardFaces.DestroyGenerated(value);}}
 private void HideInvite(){if(banner!=null)Remove(banner);banner=null;showingInvite=0;}
 private void Close(){if(panel!=null)Remove(panel);panel=null;content=null;notice=null;}
 private void OnDestroy(){if(Current==this)Current=null;}
}
