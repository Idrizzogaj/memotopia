using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;
using UnityEngine.UI;

// One inbox for ongoing and completed games. Original game rules and receipts stay unchanged.
public sealed class SprintLobby : MonoBehaviour {
 public static SprintBattleView Latest;
 private ChallengeNavigation navigation;private SprintAPI api;private bool match,creating,refreshing,starting,legacyRefreshing,recovering;
 private GameObject panel,list;private Text errorLabel;private Button singleButton,matchButton;private Text singleLabel,matchLabel;
 private float refreshAt;private int selectedId,requestVersion;private float activePosition=1,historyPosition=1,detailPosition=1;
 private ScrollRect activeScroll,historyScroll,detailScroll;
 private SprintBattleView[] battles=new SprintBattleView[0];private Challenge[] earlier=new Challenge[0];private History[] history=new History[0];
 private readonly HashSet<string> expanded=new HashSet<string>();private Action legacyDetail;
 public static SprintLobby Attach(ChallengeNavigation navigation){
  var lobby=navigation.GetComponent<SprintLobby>();if(lobby!=null)return lobby;
  lobby=navigation.gameObject.AddComponent<SprintLobby>();lobby.navigation=navigation;lobby.api=navigation.gameObject.AddComponent<SprintAPI>();lobby.Build();return lobby;
 }
 private void Build(){
  var row=ChainGame.Panel(navigation.challengeGameScreen.transform,"Single or Match",new Vector2(.1f,.84f),new Vector2(.9f,.89f),Color.clear);
  singleButton=SprintUi.Button(row.transform,"Single",Vector2.zero,new Vector2(.49f,1),()=>SetMode(false));singleLabel=singleButton.GetComponentInChildren<Text>();
  matchButton=SprintUi.Button(row.transform,"Match!",new Vector2(.51f,0),Vector2.one,()=>SetMode(true));matchLabel=matchButton.GetComponentInChildren<Text>();SetMode(false);
  LiveChallenges.Attach(navigation);
  SprintUi.Button(navigation.challengeHomeScreen.transform,"Challenges",new Vector2(.12f,.83f),new Vector2(.88f,.88f),Open);
  ArenaHomeLayout.Apply(navigation);
 }
 private void SetMode(bool value){match=value;StyleMode(singleButton,singleLabel,!value);StyleMode(matchButton,matchLabel,value);}
 private static void StyleMode(Button button,Text label,bool selected){
  button.image.color=selected?new Color32(254,191,75,255):new Color32(245,245,245,255);button.image.canvasRenderer.SetColor(Color.white);label.color=Color.black;
 }
 public void Create(string game,int tier,int opponent){
  if(creating || !CanStart())return;creating=true;
  api.Request<SprintBattleView>("",JsonUtility.ToJson(new SprintCreate{game=game,tier=tier,recipientId=opponent,mode=match?"match":"single",live=LiveChallenges.IsOnline(opponent)}),battle=>{creating=false;OpenBattle(battle);},error=>{creating=false;Message(error);});
 }
 private void Start(){RetryPending();refreshAt=Time.unscaledTime;if(Latest!=null){var battle=Latest;Latest=null;Open();ShowBattle(battle);}}
 private bool CanStart(){
  if(!PlayerPrefs.HasKey("sprint.pending."+GameManager.AchievementUserId) && !PlayerPrefs.HasKey("sprint.cancel."+GameManager.AchievementUserId))return true;
  Message("Sync your previous result before starting another game.");RetryPending();return false;
 }
 private void RetryPending(){
  if(recovering)return;
  string activeKey="sprint.active."+GameManager.AchievementUserId;
  if(!SprintRuntime.Active && PlayerPrefs.HasKey(activeKey)){
   if(!PlayerPrefs.HasKey("sprint.pending."+GameManager.AchievementUserId))PlayerPrefs.SetString("sprint.cancel."+GameManager.AchievementUserId,PlayerPrefs.GetString(activeKey));
   PlayerPrefs.DeleteKey(activeKey);PlayerPrefs.Save();
  }
  string cancelKey="sprint.cancel."+GameManager.AchievementUserId,cancel=PlayerPrefs.GetString(cancelKey,"");
  if(!string.IsNullOrEmpty(cancel)){
   try{var cancelled=SprintAPI.Decode<SprintBattleView>(cancel);recovering=true;
    api.Request<SprintBattleView>("/"+cancelled.id+"/cancel",JsonUtility.ToJson(new SprintCancel{reason=cancelled.endReason=="connection"?"connection":"gave_up"}),updated=>{recovering=false;PlayerPrefs.DeleteKey(cancelKey);PlayerPrefs.DeleteKey("sprint.pending."+GameManager.AchievementUserId);PlayerPrefs.Save();Latest=updated;if(panel!=null)ShowBattle(updated);},error=>{recovering=false;if(panel!=null)Message(error);});
   }catch(Exception){recovering=false;Debug.LogWarning("Pending give-up retained for recovery.");}return;
  }
  string key="sprint.pending."+GameManager.AchievementUserId;string pending=PlayerPrefs.GetString(key,"");if(string.IsNullOrEmpty(pending))return;
  recovering=true;
  try{int split=pending.IndexOf('\n');var battle=SprintAPI.Decode<SprintBattleView>(pending.Substring(0,split));string result=pending.Substring(split+1);
   Action<SprintBattleView> acknowledged=updated=>{recovering=false;PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();Latest=updated;if(panel!=null)ShowBattle(updated);};
   api.Request<SprintBattleView>("/"+battle.id,null,current=>{
    int leg=JsonUtility.FromJson<SprintSubmit>(result).legIndex;
    bool played=current.legs!=null && leg>=0 && leg<current.legs.Length && (GameManager.AchievementUserId==current.ownerId?current.legs[leg].ownerPlayed:current.legs[leg].recipientPlayed);
    if(current.done || played){acknowledged(current);return;}
    api.Request<SprintBattleView>("/"+battle.id+"/result",result,acknowledged,error=>{recovering=false;if(panel!=null)Message(error);});
   },error=>{recovering=false;if(panel!=null)Message(error);});
  }catch(Exception){recovering=false;Debug.LogWarning("Pending Challenge result could not be read; retained for recovery.");}
 }
 private void Update(){
  if(panel!=null && Input.GetKeyDown(KeyCode.Escape)){Back();return;}
  if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+2;RetryPending();Refresh();if(panel!=null)LoadEarlier();}
 }
 public void Open(){
  selectedId=0;legacyDetail=null;requestVersion++;if(panel!=null){panel.SetActive(false);CardFaces.DestroyGenerated(panel);}
  BuildPanel();RenderCurrent();refreshAt=Time.unscaledTime+2;Refresh();LoadEarlier();
 }
 private void BuildPanel(){
  var root=navigation.challengeGameScreen.GetComponentInParent<Canvas>().rootCanvas.transform;
  panel=ChainGame.Panel(root,"Sprint challenges",Vector2.zero,Vector2.one,Color.white);
  var safe=ChainGame.Panel(panel.transform,"Safe area",Vector2.zero,Vector2.one,Color.clear);safe.GetComponent<Image>().raycastTarget=false;
  if(Screen.width>0 && Screen.height>0){var area=Screen.safeArea;var r=(RectTransform)safe.transform;r.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);r.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);}
  ChainGame.Label(safe.transform,"Challenges",new Vector2(.17f,.91f),new Vector2(.88f,.98f),42,ChainGame.Ink).fontStyle=FontStyle.Bold;
  SprintUi.Back(safe.transform,Back);
  list=ChainGame.Panel(safe.transform,"Results",new Vector2(.05f,.05f),new Vector2(.95f,.89f),Color.clear);list.GetComponent<Image>().raycastTarget=false;
  errorLabel=ChainGame.Label(safe.transform,"",new Vector2(.05f,0),new Vector2(.95f,.05f),22,ChainGame.Ink);
 }
 private void Back(){
  var reactions=panel==null?null:panel.GetComponentInChildren<SprintReactions>();if(reactions!=null)reactions.gameObject.SetActive(false);
  if(selectedId!=0){SaveScroll();selectedId=0;legacyDetail=null;detailPosition=1;RenderList(battles);return;}
  requestVersion++;panel.SetActive(false);CardFaces.DestroyGenerated(panel);panel=null;list=null;activeScroll=historyScroll=detailScroll=null;var menu=navigation.GetComponent<MenuNavigation>();if(menu!=null)menu.GoToChallengeHome();navigation.GoToChallengeHome();
 }
 private void SaveScroll(){if(activeScroll!=null)activePosition=activeScroll.verticalNormalizedPosition;if(historyScroll!=null)historyPosition=historyScroll.verticalNormalizedPosition;if(detailScroll!=null)detailPosition=detailScroll.verticalNormalizedPosition;}
 private void ClearList(){SaveScroll();for(int i=list.transform.childCount-1;i>=0;i--){var child=list.transform.GetChild(i);child.gameObject.SetActive(false);CardFaces.DestroyGenerated(child.gameObject);}activeScroll=historyScroll=detailScroll=null;}
 private void Refresh(){
  if(refreshing)return;refreshing=true;int version=requestVersion;
  api.Request<SprintBattleList>("",null,response=>{
   refreshing=false;battles=response.battles??new SprintBattleView[0];navigation.SetSprintNotificationCount(battles.Count(Actionable));
   if(panel==null)return;RenderCurrent();
  },error=>{refreshing=false;if(panel!=null && version==requestVersion)Message(error);});
 }
 private void LoadEarlier(){
  if(legacyRefreshing || navigation.challengeAPIController==null)return;legacyRefreshing=true;int pending=2,owner=GameManager.AchievementUserId;
  Action finish=()=>{if(--pending==0)legacyRefreshing=false;if(this!=null && panel!=null && owner==GameManager.AchievementUserId && selectedId==0)RenderList(battles);};
  navigation.challengeAPIController.GetChallenges(response=>{if(owner==GameManager.AchievementUserId)earlier=response.results??new Challenge[0];finish();},error=>finish());
  navigation.challengeAPIController.GetHistory(response=>{if(owner==GameManager.AchievementUserId)history=response.history??new History[0];finish();},error=>finish());
 }
 private bool Actionable(SprintBattleView battle){
  int user=GameManager.AchievementUserId;if(battle.done)return false;if(battle.invitation=="pending")return battle.recipientId==user;if(battle.chooser==user)return true;if(battle.chooser!=0 || battle.legs==null || battle.legs.Length==0)return false;
  var leg=battle.legs.Last();int pacer=leg.pacer==0?battle.ownerId:leg.pacer;
  bool played=user==battle.ownerId?leg.ownerResult!=null:leg.recipientResult!=null;bool pace=pacer==battle.ownerId?leg.ownerResult!=null:leg.recipientResult!=null;
  return !played && (battle.live || user==pacer || pace);
 }
 private void RenderCurrent(){
  if(panel==null || list==null)return;
  if(selectedId>0){var battle=battles.FirstOrDefault(b=>b.id==selectedId);if(battle!=null){RenderBattle(battle);return;}selectedId=0;}
  if(selectedId<0 && legacyDetail!=null){legacyDetail();return;}RenderList(battles);
 }
 private void RenderList(SprintBattleView[] items){
  ClearList();Header(list.transform,"Challenges",.94f);Header(list.transform,"History",.44f);
  activeScroll=SprintUi.Scroll(list.transform,"Challenges scroll",new Vector2(0,.52f),new Vector2(1,.93f));
  historyScroll=SprintUi.Scroll(list.transform,"History scroll",new Vector2(0,0),new Vector2(1,.43f));
  foreach(var battle in items.Where(b=>!b.done))BattleRow(activeScroll.content,battle);
  foreach(var old in earlier.Where(c=>c!=null))EarlierRow(activeScroll.content,old);
  foreach(var battle in items.Where(b=>b.done))BattleRow(historyScroll.content,battle);
  for(int i=0;i<history.Length;i++)if(history[i]!=null)HistoryRow(string.Equals(history[i].winOrLose,"Pending",StringComparison.OrdinalIgnoreCase)?activeScroll.content:historyScroll.content,history[i],i);
  if(activeScroll.content.childCount==0)SprintUi.Note(activeScroll.content,"No challenges");
  if(historyScroll.content.childCount==0)SprintUi.Note(historyScroll.content,"No history");
  RestoreScroll(activeScroll,activePosition);RestoreScroll(historyScroll,historyPosition);
 }
 private static void Header(Transform parent,string caption,float bottom){
  var bar=ChainGame.Panel(parent,caption+" section",new Vector2(0,bottom),new Vector2(1,bottom+.045f),TrainingHubLayout.MemoryTone);
  bar.GetComponent<Image>().raycastTarget=false;ChainGame.Label(bar.transform,caption.ToUpperInvariant(),Vector2.zero,Vector2.one,28,Color.white).fontStyle=FontStyle.Bold;
 }
 private static void RestoreScroll(ScrollRect scroll,float position){Canvas.ForceUpdateCanvases();scroll.Rebuild(CanvasUpdate.PostLayout);scroll.verticalNormalizedPosition=Mathf.Clamp01(position);}
 private void Toggle(string key){if(!expanded.Add(key))expanded.Remove(key);RenderCurrent();}
 private string Contestants(SprintBattleView battle){return battle.ownerName+"  vs  "+battle.recipientName;}
 private string Outcome(SprintBattleView battle){if(battle.done && battle.endReason=="gave_up")return (battle.forfeitedId==battle.ownerId?battle.ownerName:battle.recipientName)+" · Gave up";if(battle.done && battle.endReason=="connection")return battle.winner==0?"Connection lost · Draw":(battle.forfeitedId==battle.ownerId?battle.ownerName:battle.recipientName)+" · Connection lost";return battle.invitation=="pending"?"Invitation":battle.invitation=="declined"?"Declined":battle.invitation=="expired"?"Expired":battle.invitation=="blocked"?"Unavailable":battle.invitation=="withdrawn"?"Withdrawn":!battle.done?"Waiting":battle.winner==0?"Draw":(battle.winner==battle.ownerId?battle.ownerName:battle.recipientName)+" wins";}
 private int Verdict(SprintBattleView battle){return !battle.done || (battle.invitation!="accepted" && !string.IsNullOrEmpty(battle.invitation)) || battle.winner==0?0:battle.winner==GameManager.AchievementUserId?1:-1;}
 private void BattleRow(Transform parent,SprintBattleView battle){
  string key="sprint:"+battle.id;bool open=expanded.Contains(key);
  var card=Row(parent,key,Contestants(battle),Outcome(battle),Verdict(battle),open,()=>{if(battle.done)Toggle(key);else ShowBattle(battle);},()=>Toggle(key));
  if(open)Facts(card,battle);
 }
 // Header has sibling controls, never a full-card Button that can cover its dropdown.
 private RectTransform Row(Transform parent,string key,string names,string outcome,int verdict,bool open,UnityEngine.Events.UnityAction choose,UnityEngine.Events.UnityAction toggle){
  var card=ChainGame.Panel(parent,key,Vector2.zero,Vector2.one,new Color32(242,240,248,255));SprintUi.Edges(card.GetComponent<Image>());
  var size=card.AddComponent<LayoutElement>();size.preferredHeight=106;size.flexibleHeight=0;
  var header=ChainGame.Panel(card.transform,"Summary",new Vector2(0,1),Vector2.one,Color.clear);var r=(RectTransform)header.transform;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,106);header.GetComponent<Image>().raycastTarget=false;
  var select=SprintUi.Button(header.transform,"Open",Vector2.zero,new Vector2(.87f,1),choose);select.name="Open result";select.image.color=Color.clear;foreach(var text in select.GetComponentsInChildren<Text>())text.gameObject.SetActive(false);
  var caption=ChainGame.Label(select.transform,names,new Vector2(.035f,.51f),new Vector2(.98f,.97f),29,ChainGame.Ink);caption.alignment=TextAnchor.MiddleLeft;caption.fontStyle=FontStyle.Bold;
  var result=ChainGame.Panel(select.transform,"Outcome",new Vector2(.025f,.08f),new Vector2(.98f,.47f),verdict>0?new Color32(28,145,89,255):verdict<0?new Color32(202,48,75,255):new Color32(222,217,235,255));result.GetComponent<Image>().raycastTarget=false;
  var label=ChainGame.Label(result.transform,outcome,new Vector2(verdict==0?.025f:.13f,0),new Vector2(.98f,1),25,verdict==0?ChainGame.Ink:Color.white);label.alignment=TextAnchor.MiddleLeft;
  if(verdict!=0){var symbol=ChainGame.Panel(result.transform,"Result icon",new Vector2(.015f,.12f),new Vector2(.115f,.88f),Color.white).GetComponent<Image>();symbol.sprite=OutcomeIcon(verdict>0);symbol.preserveAspect=true;symbol.raycastTarget=false;symbol.enabled=symbol.sprite!=null;}
  var arrow=SprintUi.Back(header.transform,toggle);arrow.name="Details";var ar=(RectTransform)arrow.transform;ar.anchorMin=ar.anchorMax=new Vector2(.94f,.5f);ar.sizeDelta=new Vector2(42,42);ar.anchoredPosition3D=Vector3.zero;ar.localRotation=Quaternion.Euler(0,0,open?-90:90);
  return (RectTransform)card.transform;
 }
 private Sprite OutcomeIcon(bool win){
  var root=navigation.challengeUserProfileScreen;
  if(root==null)return null;
  var icon=root.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.name==(win?"AchievementsImage":"TrainingImage"));return icon==null?null:icon.sprite;
 }
 private void Facts(RectTransform card,SprintBattleView battle){
  var legs=battle.legs??new SprintLegView[0];float height=65+legs.Length*235;card.GetComponent<LayoutElement>().preferredHeight=106+height;
  var box=ChainGame.Panel(card,"Facts",Vector2.zero,Vector2.one,Color.clear);box.GetComponent<Image>().raycastTarget=false;
  var rect=(RectTransform)box.transform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-106);rect.sizeDelta=new Vector2(0,height);
  ChainGame.Label(rect,battle.mode=="match"?"Match!  "+Points(battle):"Single",new Vector2(.03f,1-50/height),new Vector2(.97f,1),26,ChainGame.Ink).fontStyle=FontStyle.Bold;
  for(int i=0;i<legs.Length;i++){
   var leg=legs[i];float top=height-55-i*235;
   var block=ChainGame.Panel(rect,leg.game+" statistics",new Vector2(.025f,(top-225)/height),new Vector2(.975f,top/height),Color.white);block.GetComponent<Image>().raycastTarget=false;
   ChainGame.Label(block.transform,leg.game+" · "+(leg.ownerResult==null || leg.recipientResult==null?"Waiting":leg.winner==0?"Draw":(leg.winner==battle.ownerId?battle.ownerName:battle.recipientName)+" wins"),new Vector2(.02f,.80f),new Vector2(.98f,1),27,ChainGame.Purple).fontStyle=FontStyle.Bold;
   ChainGame.Label(block.transform,battle.ownerName,new Vector2(.31f,.60f),new Vector2(.64f,.79f),21,ChainGame.Ink);
   ChainGame.Label(block.transform,battle.recipientName,new Vector2(.65f,.60f),new Vector2(.98f,.79f),21,ChainGame.Ink);
   string[] metrics=leg.game=="Pairs"?new[]{"Rounds","Correct","Errors","Flips","Times"}:new[]{"Rounds","Correct","Errors","Times"};
   for(int m=0;m<metrics.Length;m++){float y=.60f-m*.105f;ChainGame.Label(block.transform,metrics[m],new Vector2(.03f,y-.105f),new Vector2(.30f,y),21,ChainGame.Ink).alignment=TextAnchor.MiddleLeft;ChainGame.Label(block.transform,Metric(leg.game,leg.ownerResult,m),new Vector2(.31f,y-.105f),new Vector2(.64f,y),22,ChainGame.Ink);ChainGame.Label(block.transform,Metric(leg.game,leg.recipientResult,m),new Vector2(.65f,y-.105f),new Vector2(.98f,y),22,ChainGame.Ink);}
  }
 }
 private static string Metric(string game,SprintResult result,int metric){
  if(result==null)return "—";
  if(metric<3)return (metric==0?result.completed:metric==1?ChallengeSprintRules.Units(game,result.completed,result.correct):result.errors).ToString();
  if(game=="Pairs" && metric==3)return result.turns.ToString();
  return result.roundMs==null || result.roundMs.Length==0?"—":string.Join(", ",result.roundMs.Select(ms=>(ms/1000f).ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)).ToArray())+" s";
 }
 private static string Points(SprintBattleView battle){float a=0,b=0;foreach(var leg in battle.legs){if(leg.ownerResult==null || leg.recipientResult==null)continue;if(leg.winner==0){a+=.5f;b+=.5f;}else if(leg.winner==battle.ownerId)a++;else b++;}return a.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)+" – "+b.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture);}
 public void OpenBattle(SprintBattleView battle){ShowBattle(battle);}
 private void ShowBattle(SprintBattleView battle){
  if(panel==null)Open();SaveScroll();selectedId=battle.id;legacyDetail=null;detailPosition=1;
  var index=Array.FindIndex(battles,b=>b.id==battle.id);if(index<0)battles=new[]{battle}.Concat(battles).ToArray();else battles[index]=battle;
  RenderBattle(battle);
 }
 private void RenderBattle(SprintBattleView battle){
  if(battle.invitation=="accepted" || string.IsNullOrEmpty(battle.invitation))SprintReactions.Attach(panel.transform,battle.id);
  ClearList();detailScroll=SprintUi.Scroll(list.transform,"Match scroll",Vector2.zero,Vector2.one);BattleRow(detailScroll.content,battle);
  if(!battle.done){
   int user=GameManager.AchievementUserId;
   if(battle.invitation=="pending"){
    if(user==battle.recipientId){
     Consent(detailScroll.content,"Yes",()=>LiveChallenges.Current.Respond(battle,true));
     Consent(detailScroll.content,"No",()=>LiveChallenges.Current.Respond(battle,false));
    }else SprintUi.Note(detailScroll.content,"Invitation sent · Waiting for acceptance");
   }else if(battle.chooser==user){
    var games=ChallengeSprintRules.Games.Where(g=>!battle.legs.Any(l=>l.game==g)).ToArray();GameChoices(detailScroll.content,games,game=>{if(creating)return;creating=true;api.Request<SprintBattleView>("/"+battle.id+"/game",JsonUtility.ToJson(new SprintChoose{game=game}),updated=>{creating=false;ShowBattle(updated);},error=>{creating=false;Message(error);});});
   }else if(Actionable(battle))GameChoices(detailScroll.content,new[]{battle.legs.Last().game},game=>{if(starting || !CanStart())return;starting=true;SprintRuntime.Begin(battle,null);});
   else SprintUi.Note(detailScroll.content,"Waiting for your opponent");
   if(battle.invitation!="pending" || user==battle.ownerId)Consent(detailScroll.content,battle.invitation=="pending"?"Withdraw":"Give up",()=>ConfirmGiveUp(battle));
  }
  RestoreScroll(detailScroll,detailPosition);
 }
 private void ConfirmGiveUp(SprintBattleView battle){
  var dialog=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/MidGameQuitPanel"),panel.transform,false);var content=dialog.transform.GetChild(0);
  foreach(var text in content.GetComponentsInChildren<Text>(true))text.text=text.text.Replace("quit?","give up?");
  var no=content.GetChild(2).GetComponent<Button>();var yes=content.GetChild(3).GetComponent<Button>();no.onClick=new Button.ButtonClickedEvent();yes.onClick=new Button.ButtonClickedEvent();
  no.onClick.AddListener(()=>CardFaces.DestroyGenerated(dialog));yes.onClick.AddListener(()=>{
   CardFaces.DestroyGenerated(dialog);if(creating)return;creating=true;
   string key="sprint.cancel."+GameManager.AchievementUserId;PlayerPrefs.SetString(key,JsonUtility.ToJson(battle));PlayerPrefs.Save();
   api.Request<SprintBattleView>("/"+battle.id+"/cancel","{}",updated=>{creating=false;PlayerPrefs.DeleteKey(key);PlayerPrefs.DeleteKey("sprint.pending."+GameManager.AchievementUserId);PlayerPrefs.Save();ShowBattle(updated);},error=>{creating=false;Message("Give up saved. Reconnecting…");});
  });
 }
 private static void Consent(Transform parent,string caption,UnityEngine.Events.UnityAction action){var row=ChainGame.Panel(parent,caption,Vector2.zero,Vector2.one,Color.clear);row.AddComponent<LayoutElement>().preferredHeight=90;SprintUi.Button(row.transform,caption,Vector2.zero,Vector2.one,action);}
 private static void GameChoices(Transform parent,string[] games,Action<string> choose){
  int rows=(games.Length+1)/2;var go=ChainGame.Panel(parent,"Games",Vector2.zero,Vector2.one,Color.clear);go.GetComponent<Image>().raycastTarget=false;go.AddComponent<LayoutElement>().preferredHeight=rows*270;
  for(int i=0;i<games.Length;i++){string game=games[i];float left=(i%2)*.51f,top=1-(i/2)/(float)rows;TrainingHubLayout.GameButton(go.transform,game,new Vector2(left,top-.96f/rows),new Vector2(left+.49f,top),()=>choose(game));}
 }
 private string MyName {get{return UserConstants.s_user==null?"You":UserConstants.s_user.Username;}}
 private Participant Opponent(Challenge item){return item.participants==null?null:item.participants.FirstOrDefault(p=>p.user!=null && p.user.ID!=GameManager.AchievementUserId)??item.participants.FirstOrDefault();}
 private void EarlierRow(Transform parent,Challenge item){
  var opponent=Opponent(item);string key="earlier:"+item.id;bool open=expanded.Contains(key);
  var card=Row(parent,key,MyName+"  vs  "+(opponent==null || opponent.user==null?"Player":opponent.user.Username),"Waiting",0,open,()=>ShowEarlier(item),()=>Toggle(key));
  if(open)EarlierFacts(card,item.game==null?"Challenge":item.game.gameName,item.level,opponent==null?null:("Score: "+opponent.score));
 }
 private void HistoryRow(Transform parent,History item,int index){
  string key="earlier-history:"+index;bool open=expanded.Contains(key);int verdict=string.Equals(item.winOrLose,"WIN",StringComparison.OrdinalIgnoreCase)?1:string.Equals(item.winOrLose,"LOSS",StringComparison.OrdinalIgnoreCase)?-1:0;
  string opponent=item.challenger==null?"Player":item.challenger.Username;string outcome=string.Equals(item.winOrLose,"Pending",StringComparison.OrdinalIgnoreCase)?"Waiting":verdict>0?MyName+" wins":verdict<0?opponent+" wins":"Draw";
  var card=Row(parent,key,MyName+"  vs  "+opponent,outcome,verdict,open,()=>Toggle(key),()=>Toggle(key));if(open)EarlierFacts(card,item.gameName,item.level,null);
 }
 private static void EarlierFacts(RectTransform card,string game,int tier,string score){
  card.GetComponent<LayoutElement>().preferredHeight=196;var text=ChainGame.Label(card,game+"  •  "+tier+(score==null?"":"\n"+score),Vector2.zero,Vector2.one,24,ChainGame.Ink);var r=text.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-106);r.sizeDelta=new Vector2(0,90);
 }
 private void ShowEarlier(Challenge item){
  SaveScroll();selectedId=-item.id;legacyDetail=()=>{
   ClearList();detailScroll=SprintUi.Scroll(list.transform,"Earlier match scroll",Vector2.zero,Vector2.one);EarlierRow(detailScroll.content,item);
   var opponent=Opponent(item);if(opponent!=null && item.game!=null)GameChoices(detailScroll.content,new[]{item.game.gameName},game=>navigation.StartChallenge(item.id,item.level,game,opponent.score,opponent.progress));RestoreScroll(detailScroll,detailPosition);
  };legacyDetail();
 }
 private void Message(string message){if(panel==null)Open();if(errorLabel!=null)errorLabel.text=message;}
}

public static class SprintUi {
 public static void Edges(Image image){image.sprite=ContentSelectorBar.Rounded();image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=4;}
 public static Button Back(Transform parent,UnityEngine.Events.UnityAction action){
  var original=Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall").GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Quit");
  var button=UnityEngine.Object.Instantiate(original,parent,false);button.name="Original back";button.gameObject.SetActive(true);
  var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=new Vector2(.07f,.945f);rect.anchoredPosition3D=Vector3.zero;rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;rect.sizeDelta=new Vector2(64,64);
  foreach(var text in button.GetComponentsInChildren<Text>(true))text.gameObject.SetActive(false);
  // Standard UI material keeps the reused arrow clipped by each independent RectMask2D.
  foreach(var image in button.GetComponentsInChildren<Image>(true)){image.material=null;image.color=ChainGame.Ink;}
  button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(action);return button;
 }
 public static Button Button(Transform parent,string caption,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action){
  var original=Resources.Load<GameObject>("Prefabs/ExactPairsChainStudy").GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next");
  var button=UnityEngine.Object.Instantiate(original,parent,false);button.name=caption;button.gameObject.SetActive(true);
  var rect=(RectTransform)button.transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.anchoredPosition3D=new Vector3(rect.anchoredPosition.x,rect.anchoredPosition.y,0);rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;
  button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(action);button.transition=Selectable.Transition.None;button.image.preserveAspect=false;button.image.material=null;Edges(button.image);button.image.color=new Color32(236,232,244,255);
  foreach(var text in button.GetComponentsInChildren<Text>(true)){var bounds=text.rectTransform;bounds.anchorMin=new Vector2(.06f,.08f);bounds.anchorMax=new Vector2(.94f,.92f);bounds.offsetMin=bounds.offsetMax=Vector2.zero;text.text=caption;text.color=ChainGame.Ink;text.resizeTextForBestFit=true;text.resizeTextMinSize=14;text.resizeTextMaxSize=text.fontSize;}
  return button;
 }
 public static ScrollRect Scroll(Transform parent,string name,Vector2 min,Vector2 max){
  var root=ChainGame.Panel(parent,name,min,max,Color.clear);var scroll=root.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;
  var viewport=ChainGame.Panel(root.transform,"Viewport",Vector2.zero,Vector2.one,Color.clear);viewport.AddComponent<RectMask2D>();
  var content=new GameObject("Content",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter));var rect=(RectTransform)content.transform;rect.SetParent(viewport.transform,false);rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);rect.offsetMin=rect.offsetMax=Vector2.zero;
  var layout=content.GetComponent<VerticalLayoutGroup>();layout.spacing=12;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
  scroll.viewport=(RectTransform)viewport.transform;scroll.content=rect;return scroll;
 }
 public static void Note(Transform parent,string caption){var text=ChainGame.Label(parent,caption,Vector2.zero,Vector2.one,27,ChainGame.Ink);text.gameObject.AddComponent<LayoutElement>().preferredHeight=80;}
}
