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
 public static void Watch(Text label,int id,string name){label.text=name+(IsOnline(id)?" · Online":"");if(Current!=null)Current.labels.Add(new PresenceLabel{label=label,id=id,name=name});}
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
  SprintUi.Button(parent,"Friend",new Vector2(.05f,.015f),new Vector2(.30f,.065f),()=>ConfirmFriend(id));
  SprintUi.Button(parent,"Block",new Vector2(.32f,.015f),new Vector2(.68f,.065f),()=>ConfirmBlock(id));
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
  polling=true;api.Request<OnlinePlayers>("/social/online",null,value=>{online=value;labels.RemoveAll(p=>p.label==null);foreach(var p in labels)p.label.text=p.name+(IsOnline(p.id)?" · Online":"");if(panel!=null)Render();},error=>{});
  api.Request<SprintBattleList>("",null,value=>{
   polling=false;
   var invite=(value.battles??new SprintBattleView[0]).FirstOrDefault(b=>!b.done&&b.invitation=="pending"&&b.recipientId==account);
   if(invite==null){if(showingInvite>0)HideInvite();return;}
   if(!SprintRuntime.Active && panel==null && navigation!=null && showingInvite==0)ShowInvite(invite);
  },error=>{polling=false;});
 }
 private Transform Root(){return navigation==null?null:navigation.challengeGameScreen.GetComponentInParent<Canvas>().rootCanvas.transform;}
 public void Open(){
  if(navigation==null)return;HideInvite();Close();panel=ChainGame.Panel(Root(),"Online players",Vector2.zero,Vector2.one,Color.white);
  SprintUi.Back(panel.transform,Close);ChainGame.Label(panel.transform,"Online",new Vector2(.2f,.90f),new Vector2(.85f,.97f),40,ChainGame.Purple);
  SprintUi.Button(panel.transform,"Everyone",new Vector2(.62f,.83f),new Vector2(.94f,.89f),()=>{if(!busy && loaded){Heartbeat(!settings.visible);}});
  SprintUi.Button(panel.transform,"Friends",new Vector2(.62f,.76f),new Vector2(.94f,.82f),()=>{if(!busy && loaded){settings.friendsVisible=!settings.friendsVisible;Heartbeat(settings.visible);}});
  notice=ChainGame.Label(panel.transform,"",new Vector2(.06f,.76f),new Vector2(.60f,.89f),23,ChainGame.Ink);
  content=SprintUi.Scroll(panel.transform,"Players",new Vector2(.06f,.07f),new Vector2(.94f,.74f)).content;Render();pollAt=0;
 }
 private void Render(){
  if(content==null)return;for(int i=content.childCount-1;i>=0;i--){var child=content.GetChild(i).gameObject;child.SetActive(false);Remove(child);}
  foreach(string audience in new[]{"Everyone","Friends"}){var button=panel.transform.Find(audience).GetComponent<Button>();bool on=audience=="Everyone"?settings.visible:settings.friendsVisible;button.image.color=on?TrainingHubLayout.MemoryTone:(Color)new Color32(236,232,244,255);foreach(var text in button.GetComponentsInChildren<Text>())text.color=on?Color.white:ChainGame.Ink;}
  notice.text="Online · Everyone: "+(settings.visible?"ON":"OFF")+"\nFriends: "+(settings.friendsVisible?"ON":"OFF");
  SprintUi.Note(content,"Declined: "+settings.declined+" · Only you can see this");
  SprintUi.Note(content,"Friends share online status by default. You can turn it off above.");
  foreach(var player in settings.friends??new SocialPlayer[0]){
   var local=player;
   var row=ChainGame.Panel(content,"Friend",Vector2.zero,Vector2.one,Color.clear);row.AddComponent<LayoutElement>().preferredHeight=120;
   ChainGame.Label(row.transform,player.username+(IsOnline(player.id)?" · Online":"")+(player.accepted?" · Friend":player.requesterId==account?" · Sent":" · Friend request"),new Vector2(0,.5f),Vector2.one,24,ChainGame.Ink);
   if(!player.accepted && player.requesterId!=account){SprintUi.Button(row.transform,"Yes",Vector2.zero,new Vector2(.48f,.48f),()=>Friend(local.id,"accept"));SprintUi.Button(row.transform,"No",new Vector2(.52f,0),new Vector2(1,.48f),()=>Friend(local.id,"decline"));}
   else SprintUi.Button(row.transform,"Remove",Vector2.zero,new Vector2(1,.48f),()=>Friend(local.id,"remove"));
  }
  SprintUi.Note(content,"Online");
  foreach(var player in online.players??new SocialPlayer[0]){
   var item=ChainGame.Panel(content,"Player",Vector2.zero,Vector2.one,Color.clear);item.AddComponent<LayoutElement>().preferredHeight=100;
   var local=player;
   SprintUi.Button(item.transform,(player.favorite?"★ ":"")+player.username+" · Online",Vector2.zero,new Vector2(.76f,1),()=>{
    Close();UserConstants.s_challengeUser=new User{id=local.id,username=local.username};navigation.GoToChallengeGame();
   });
   SprintUi.Button(item.transform,"Block",new Vector2(.78f,0),Vector2.one,()=>ConfirmBlock(local.id));
  }
  SprintUi.Note(content,"Blocked");
  foreach(var player in settings.blocked??new SocialPlayer[0]){var local=player;var row=ChainGame.Panel(content,"Blocked player",Vector2.zero,Vector2.one,Color.clear);row.AddComponent<LayoutElement>().preferredHeight=90;SprintUi.Button(row.transform,player.username+" · Unblock",Vector2.zero,Vector2.one,()=>Block(local.id,false));}
 }
 private void Friend(int id,string action){if(acting)return;acting=true;api.Request<SocialSettings>("/social/friend",JsonUtility.ToJson(new SocialFriend{targetId=id,action=action}),value=>{acting=false;settings=value;if(panel==null)Open();else Render();},error=>{acting=false;if(notice!=null)notice.text=error;});}
 private Transform Dialog(string title){
  HideInvite();banner=ChainGame.Panel(Root(),title,Vector2.zero,Vector2.one,new Color(0,0,0,.55f));
  var box=ChainGame.Panel(banner.transform,"Dialog",new Vector2(.06f,.30f),new Vector2(.94f,.70f),Color.white);SprintUi.Edges(box.GetComponent<Image>());return box.transform;
 }
 private void ConfirmFriend(int id){
  if(Root()==null)return;var box=Dialog("Friend request");showingInvite=-1;
  ChainGame.Label(box,"Send a friend request?\nFriends can see when you are online. Turn off Friends visibility any time.",new Vector2(.06f,.38f),new Vector2(.94f,.94f),28,ChainGame.Ink);
  SprintUi.Button(box,"No",new Vector2(.05f,.08f),new Vector2(.47f,.30f),HideInvite);
  SprintUi.Button(box,"Send",new Vector2(.53f,.08f),new Vector2(.95f,.30f),()=>{HideInvite();Friend(id,"request");});
 }
 private void ConfirmBlock(int id){
  if(Root()==null)return;var box=Dialog("Block player");showingInvite=-1;
  ChainGame.Label(box,"Block this player?\nInvitations and online visibility stop both ways.",new Vector2(.06f,.40f),new Vector2(.94f,.94f),28,ChainGame.Ink);
  SprintUi.Button(box,"No",new Vector2(.05f,.08f),new Vector2(.47f,.31f),HideInvite);
  SprintUi.Button(box,"Block",new Vector2(.53f,.08f),new Vector2(.95f,.31f),()=>Block(id,true));
 }
 private void Block(int id,bool blocked){if(acting)return;acting=true;api.Request<SocialSettings>("/social/block",JsonUtility.ToJson(new SocialBlock{targetId=id,blocked=blocked}),value=>{acting=false;settings=value;HideInvite();pollAt=0;if(panel!=null)Render();},error=>{acting=false;if(notice!=null)notice.text=error;});}
 private void ShowInvite(SprintBattleView battle){
  var box=Dialog("Challenge invitation");showingInvite=battle.id;
  ChainGame.Label(box,battle.ownerName+"\n"+(battle.live?"Live · ":"")+battle.legs[0].game+" · "+(battle.mode=="match"?"Match!":"Single"),new Vector2(.05f,.52f),new Vector2(.95f,.95f),32,ChainGame.Purple);
  SprintUi.Button(box,"No",new Vector2(.05f,.25f),new Vector2(.47f,.47f),()=>Respond(battle,false));
  SprintUi.Button(box,"Yes",new Vector2(.53f,.25f),new Vector2(.95f,.47f),()=>Respond(battle,true));
  SprintUi.Button(box,"Block",new Vector2(.30f,.03f),new Vector2(.70f,.20f),()=>ConfirmBlock(battle.ownerId));
 }
 public void Respond(SprintBattleView battle,bool accept){if(acting)return;acting=true;api.Request<SprintBattleView>("/"+battle.id+"/respond",JsonUtility.ToJson(new SprintResponse{accept=accept}),value=>{acting=false;HideInvite();if(accept){var lobby=SprintLobby.Attach(navigation);lobby.OpenBattle(value);}pollAt=0;},error=>{acting=false;if(notice!=null)notice.text=error;});}
 private static void Remove(GameObject value){if(value!=null){value.SetActive(false);CardFaces.DestroyGenerated(value);}}
 private void HideInvite(){if(banner!=null)Remove(banner);banner=null;showingInvite=0;}
 private void Close(){if(panel!=null)Remove(panel);panel=null;content=null;notice=null;}
 private void OnDestroy(){if(Current==this)Current=null;}
}
