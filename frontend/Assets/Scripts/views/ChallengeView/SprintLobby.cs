using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
// New protocol inbox coexists with old challenges, which retain their old rules.
public sealed class SprintLobby : MonoBehaviour {
 public static SprintBattleView Latest;
 private ChallengeNavigation navigation;private SprintAPI api;private bool match,creating;private GameObject panel,list;
 private Button singleButton,matchButton;private Text singleLabel,matchLabel;private float refreshAt;private int page,selectedId,requestVersion;private bool refreshing,starting;
 public static SprintLobby Attach(ChallengeNavigation navigation){
  var lobby=navigation.GetComponent<SprintLobby>();if(lobby!=null)return lobby;
  lobby=navigation.gameObject.AddComponent<SprintLobby>();lobby.navigation=navigation;lobby.api=navigation.gameObject.AddComponent<SprintAPI>();lobby.Build();return lobby;
 }
 private void Build(){
  var row=ChainGame.Panel(navigation.challengeGameScreen.transform,"Single or Match",new Vector2(.1f,.84f),new Vector2(.9f,.89f),Color.clear);
  var single=SprintUi.Button(row.transform,"Single game",Vector2.zero,new Vector2(.49f,1),()=>SetMode(false));singleButton=single;singleLabel=single.GetComponentInChildren<Text>();
  var multiple=SprintUi.Button(row.transform,"Match!",new Vector2(.51f,0),Vector2.one,()=>SetMode(true));matchButton=multiple;matchLabel=multiple.GetComponentInChildren<Text>();SetMode(false);
  SprintUi.Button(navigation.challengeHomeScreen.transform,"90 s Challenges / Match!",new Vector2(.12f,.83f),new Vector2(.88f,.88f),Open);
 }
 private void SetMode(bool value){
  match=value;StyleMode(singleButton,singleLabel,!value);StyleMode(matchButton,matchLabel,value);
 }
 private static void StyleMode(Button button,Text label,bool selected){
  button.transition=Selectable.Transition.None;button.image.sprite=ContentSelectorBar.Rounded();button.image.type=Image.Type.Sliced;
  button.image.color=selected?new Color32(254,191,75,255):new Color32(245,245,245,255);button.image.canvasRenderer.SetColor(Color.white);label.color=Color.black;
  var outline=button.GetComponent<Outline>();if(outline==null)outline=button.gameObject.AddComponent<Outline>();outline.effectColor=new Color32(210,210,210,255);outline.effectDistance=new Vector2(1,-1);
 }
 public void Create(string game,int tier,int opponent){
  if(creating || !CanStart())return;creating=true;
  api.Request<SprintBattleView>("",JsonUtility.ToJson(new SprintCreate{game=game,tier=tier,recipientId=opponent,mode=match?"match":"single"}),battle=>{creating=false;SprintRuntime.Begin(battle,null);},error=>{creating=false;Message(error);});
 }
 private void Start(){
  RetryPending();
  refreshAt=Time.unscaledTime;
  if(Latest!=null){var battle=Latest;Latest=null;Open();ShowBattle(battle);}
 }
 private bool CanStart(){
  if(!PlayerPrefs.HasKey("sprint.pending."+GameManager.AchievementUserId))return true;
  Message("Sync your previous result before starting another game.");RetryPending();return false;
 }
 private void RetryPending(){
  string key="sprint.pending."+GameManager.AchievementUserId;string pending=PlayerPrefs.GetString(key,"");if(string.IsNullOrEmpty(pending))return;
  try{int split=pending.IndexOf('\n');var battle=SprintAPI.Decode<SprintBattleView>(pending.Substring(0,split));string result=pending.Substring(split+1);
   api.Request<SprintBattleView>("/"+battle.id+"/result",result,updated=>{PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();Latest=updated;if(panel!=null)ShowBattle(updated);},error=>{if(panel!=null)Message(error);});
  }catch(Exception){Debug.LogWarning("Pending Challenge result could not be read; retained for recovery.");}
 }
 private void Update(){if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+20;Refresh();}}
 public void Open(){
  selectedId=0;page=0;requestVersion++;
  if(panel!=null)CardFaces.DestroyGenerated(panel);
  panel=ChainGame.Panel(navigation.challengeGameScreen.GetComponentInParent<Canvas>().rootCanvas.transform,"Sprint challenges",Vector2.zero,Vector2.one,Color.white);
  ChainGame.Label(panel.transform,"Challenge / Match!",new Vector2(.15f,.90f),new Vector2(.85f,.97f),44,ChainGame.Purple);
  SprintUi.Back(panel.transform,()=>{requestVersion++;CardFaces.DestroyGenerated(panel);panel=null;});
  list=ChainGame.Panel(panel.transform,"Results",new Vector2(.06f,.15f),new Vector2(.94f,.88f),Color.clear);
  SprintUi.Button(panel.transform,"Earlier challenges",new Vector2(.06f,.04f),new Vector2(.48f,.11f),()=>{CardFaces.DestroyGenerated(panel);panel=null;navigation.GoToLegacyNotification();});
  SprintUi.Button(panel.transform,"Earlier history",new Vector2(.52f,.04f),new Vector2(.94f,.11f),()=>{CardFaces.DestroyGenerated(panel);panel=null;navigation.GoToLegacyHistory();});
  refreshAt=Time.unscaledTime+20;Refresh();
 }
 private void ClearList(){foreach(Transform t in list.transform)CardFaces.DestroyGenerated(t.gameObject);}
 private void Refresh(){
  if(refreshing)return;refreshing=true;int version=requestVersion;
  api.Request<SprintBattleList>("",null,response=>{
   refreshing=false;var battles=response.battles??new SprintBattleView[0];
   navigation.SetSprintNotificationCount(battles.Count(Actionable));
   if(panel==null || version!=requestVersion)return;
   if(selectedId!=0){var selected=battles.FirstOrDefault(b=>b.id==selectedId);if(selected!=null){ShowBattle(selected);return;}}
   RenderList(battles);
  },error=>{refreshing=false;if(panel!=null && version==requestVersion)Message(error);});
 }
 private bool Actionable(SprintBattleView battle){
  int user=GameManager.AchievementUserId;if(battle.done)return false;if(battle.chooser==user)return true;if(battle.chooser!=0)return false;
  var leg=battle.legs.Last();int pacer=leg.pacer==0?battle.ownerId:leg.pacer;
  bool played=user==battle.ownerId?leg.ownerResult!=null:leg.recipientResult!=null;
  bool pace=pacer==battle.ownerId?leg.ownerResult!=null:leg.recipientResult!=null;
  return !played && (user==pacer || pace);
 }
 private void RenderList(SprintBattleView[] battles){
  ClearList();page=Math.Min(page,Math.Max(0,(battles.Length-1)/7));int row=0;
  foreach(var battle in battles.Skip(page*7).Take(7)){
   var selected=battle;float top=1-row++*.12f;
   SprintUi.Button(list.transform,Summary(battle),new Vector2(0,top-.11f),new Vector2(1,top),()=>ShowBattle(selected));
  }
  if(row==0)ChainGame.Label(list.transform,"No 90-second challenges yet",new Vector2(0,.4f),new Vector2(1,.6f),30,ChainGame.Ink);
  if(page>0)SprintUi.Button(list.transform,"Previous",new Vector2(0,0),new Vector2(.47f,.09f),()=>{page--;RenderList(battles);});
  if((page+1)*7<battles.Length)SprintUi.Button(list.transform,"Next",new Vector2(.53f,0),new Vector2(1,.09f),()=>{page++;RenderList(battles);});
 }
 private static string Points(SprintBattleView battle){
  float a=0,b=0;foreach(var leg in battle.legs){if(leg.ownerResult==null || leg.recipientResult==null)continue;if(leg.winner==0){a+=.5f;b+=.5f;}else if(leg.winner==battle.ownerId)a++;else b++;}
  return a.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)+" – "+b.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture);
 }
 private string Summary(SprintBattleView battle){string opponent=GameManager.AchievementUserId==battle.ownerId?battle.recipientName:battle.ownerName;return opponent+" • "+(battle.mode=="match"?"Match! "+Points(battle):battle.legs.Last().game)+" • "+(battle.done?(battle.winner==0?"DRAW":battle.winner==GameManager.AchievementUserId?"WIN":"LOSS"):"In progress");}
 private void ShowBattle(SprintBattleView battle){
  if(panel==null)Open();selectedId=battle.id;ClearList();ChainGame.Label(list.transform,Summary(battle),new Vector2(0,.78f),new Vector2(1,1),40,ChainGame.Purple);
  SprintUi.Button(list.transform,"All challenges",new Vector2(.15f,0),new Vector2(.85f,.09f),()=>{selectedId=0;Refresh();});
  int row=0;foreach(var result in battle.legs){if(result.ownerResult==null || result.recipientResult==null)continue;
   string verdict=result.winner==0?"DRAW":result.winner==GameManager.AchievementUserId?"WIN":"LOSS";
   string detail=result.game+" • "+verdict+"\n"+ResultText(result.ownerResult)+" / "+ResultText(result.recipientResult);
   float top=.79f-row++*.13f;ChainGame.Label(list.transform,detail,new Vector2(0,top-.12f),new Vector2(1,top),24,ChainGame.Ink);
  }
  if(battle.done)return;
  int owner=GameManager.AchievementUserId;
  if(battle.chooser==owner){
   int i=0;foreach(string game in ChallengeSprintRules.Games.Where(g=>!battle.legs.Any(l=>l.game==g))){string selected=game;float top=.55f-i++*.14f;SprintUi.Button(list.transform,game,new Vector2(.1f,top-.14f),new Vector2(.9f,top),()=>api.Request<SprintBattleView>("/"+battle.id+"/game",JsonUtility.ToJson(new SprintChoose{game=selected}),ShowBattle,Message));}return;
  }
  var leg=battle.legs.Last();int pacer=leg.pacer==0?battle.ownerId:leg.pacer;
  bool alreadyPlayed=owner==battle.ownerId?leg.ownerResult!=null:leg.recipientResult!=null;
  bool paceRecorded=pacer==battle.ownerId?leg.ownerResult!=null:leg.recipientResult!=null;
  bool canPlay=battle.chooser==0 && !alreadyPlayed && (owner==pacer || paceRecorded);
  if(canPlay)SprintUi.Button(list.transform,"Play "+leg.game+" •90s",new Vector2(.1f,.13f),new Vector2(.9f,.28f),()=>{if(starting || !CanStart())return;starting=true;SprintRuntime.Begin(battle,null);});
  else ChainGame.Label(list.transform,"Waiting for your opponent",new Vector2(0,.13f),new Vector2(1,.28f),32,ChainGame.Ink);
 }
 private static string ResultText(SprintResult result){return result.completed+" rounds + "+result.correct+" correct • "+result.turns+" flips • "+result.errors+" errors";}
 private void Message(string message){if(panel==null)Open();if(list!=null)ChainGame.Label(list.transform,message,new Vector2(0,0),new Vector2(1,.15f),24,ChainGame.Ink);}
}
public static class SprintUi {
 public static Button Back(Transform parent,UnityEngine.Events.UnityAction action){
  var original=Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall").GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Quit");
  var button=UnityEngine.Object.Instantiate(original,parent,false);button.name="Original back";
  var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=new Vector2(.08f,.94f);rect.anchoredPosition=Vector2.zero;rect.localScale=Vector3.one;
  button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(action);return button;
 }
 public static Button Button(Transform parent,string caption,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action){
  var original=Resources.Load<GameObject>("Prefabs/ExactPairsChainStudy").GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next");
  var button=UnityEngine.Object.Instantiate(original,parent,false);button.name=caption;
  var rect=(RectTransform)button.transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;
  button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(action);
  button.image.preserveAspect=false;
  foreach(var text in button.GetComponentsInChildren<Text>(true)){
   var bounds=text.rectTransform;bounds.anchorMin=new Vector2(.08f,.08f);bounds.anchorMax=new Vector2(.92f,.92f);bounds.offsetMin=bounds.offsetMax=Vector2.zero;
   text.text=caption;text.resizeTextForBestFit=true;text.resizeTextMinSize=14;text.resizeTextMaxSize=text.fontSize;
  }
  return button;
 }
}

