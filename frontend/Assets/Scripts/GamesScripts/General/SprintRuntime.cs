using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Session controller only. All study/recall controls remain the original game scenes.
public sealed class SprintRuntime : MonoBehaviour {
 public static SprintRuntime Current {get;private set;}
 public static bool Active {get{return Current!=null;}}
 public ChallengeSprintState State {get;private set;}
 public SprintBattleView Battle {get;private set;}
 public int LegIndex {get;private set;}
 public int Size {get{return ChallengeSprintRules.StageSize(Game,State==null?0:State.Stage);}}
 public string Game {get{return Battle.legs[LegIndex].game;}}
 public int StageSeed {get{return unchecked(Battle.legs[LegIndex].seed+(State==null?0:State.Stage)*104729);}}
 public SprintRandom Random {get;private set;}
 public bool Ready {get{return State!=null && !State.Finished && !transition && !ending && !connecting && quitDialog==null;}}
 private bool connecting;private float savedScale=1;private GameObject connectionOverlay;
 private GameObject stageRoot,stageTemplate,templateHolder;private ChainGame chainGame;
 private int owner;private bool transition,ending,introStarting,paced,cancelling,closed;private SprintAPI api;private GameObject quitDialog;private Button retry;
 private float progressAt;private bool progressBusy;private int liveUnits;
 private Button waitingBack;private Canvas hud;private Text timer,cue,status;private Image lead;private RectTransform paceTrack,paceCentre;private bool verticalPace;private GameObject introCover;private float cueUntil;
 private readonly HashSet<string> solved=new HashSet<string>();
 private SprintTrace[] opponent=new SprintTrace[0];private Action<SprintBattleView> finished;
#if UNITY_EDITOR
 public static Func<long> EditorNow;
#endif
 private long Now {get{
#if UNITY_EDITOR
  if(EditorNow!=null)return EditorNow();
#endif
  return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
 }}
 public string ClockText {get{return State==null?"90":Mathf.CeilToInt(State.Remaining(Now)/1000f).ToString();}}
 public static void Begin(SprintBattleView battle,Action<SprintBattleView> finished){
  if(Active)throw new InvalidOperationException("A sprint is already active");
  var root=new GameObject("Challenge Sprint90");DontDestroyOnLoad(root);
  var runtime=root.AddComponent<SprintRuntime>();Current=runtime;runtime.Battle=battle;runtime.LegIndex=battle.legs.Length-1;runtime.owner=GameManager.AchievementUserId;runtime.finished=finished;runtime.api=root.AddComponent<SprintAPI>();
  var leg=battle.legs[runtime.LegIndex];var result=runtime.owner==battle.ownerId?leg.recipientResult:leg.ownerResult;
  runtime.paced=result!=null;if(result!=null && result.trace!=null)runtime.opponent=result.trace;
  PlayerPrefs.SetString("sprint.active."+runtime.owner,JsonUtility.ToJson(battle));PlayerPrefs.Save();
  runtime.Random=new SprintRandom(runtime.StageSeed);runtime.BuildHud();SceneManager.sceneLoaded+=runtime.SceneLoaded;runtime.LoadGame();
 }
 private void BuildHud(){
  var root=new GameObject("Sprint HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform,false);
  hud=root.GetComponent<Canvas>();hud.renderMode=RenderMode.ScreenSpaceOverlay;hud.sortingOrder=100;
  var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(800,1400);
  timer=ChainGame.Label(root.transform,"90",new Vector2(.43f,.91f),new Vector2(.57f,.96f),38,ChainGame.Ink);timer.gameObject.SetActive(Game=="Chain");
  status=ChainGame.Label(root.transform,"",new Vector2(.1f,.86f),new Vector2(.9f,.90f),22,ChainGame.Ink);
  cue=ChainGame.Label(root.transform,"",new Vector2(.05f,.40f),new Vector2(.95f,.63f),72,ChainGame.Ink);cue.fontStyle=FontStyle.Bold;
  var track=ChainGame.Panel(root.transform,"Opponent pace",new Vector2(.18f,.90f),new Vector2(.82f,.908f),new Color(.8f,.8f,.8f));
  paceTrack=(RectTransform)track.transform;
  lead=ChainGame.Panel(track.transform,"Lead",new Vector2(.5f,0),new Vector2(.5f,1),Color.green).GetComponent<Image>();lead.raycastTarget=false;
  var centre=ChainGame.Panel(track.transform,"Centre",new Vector2(.497f,-.4f),new Vector2(.503f,1.4f),ChainGame.Ink);centre.GetComponent<Image>().raycastTarget=false;track.GetComponent<Image>().raycastTarget=false;
  paceCentre=(RectTransform)centre.transform;
  introCover=ChainGame.Panel(root.transform,"Original round intro cover",Vector2.zero,Vector2.one,Color.white);cue.transform.SetAsLastSibling();
  retry=SprintUi.Button(root.transform,"Retry",new Vector2(.25f,.20f),new Vector2(.75f,.29f),()=>{retry.gameObject.SetActive(false);if(cancelling)Cancel();else if(State==null)StartCoroutine(Intro());else StartCoroutine(Submit());});retry.gameObject.SetActive(false);
  waitingBack=SprintUi.Back(root.transform,Quit);
  SprintUi.Button(root.transform,"Give up",new Vector2(.73f,.925f),new Vector2(.98f,.975f),Quit);
  SprintReactions.Attach(root.transform,Battle.id);
  connectionOverlay=ChainGame.Panel(root.transform,"Connection recovery",Vector2.zero,Vector2.one,new Color(1,1,1,.12f));
  var wifi=new GameObject("Connection wifi",typeof(RectTransform),typeof(CanvasRenderer),typeof(PresenceIndicator));wifi.transform.SetParent(connectionOverlay.transform,false);var wifiRect=(RectTransform)wifi.transform;wifiRect.anchorMin=wifiRect.anchorMax=new Vector2(.5f,.55f);wifiRect.sizeDelta=new Vector2(250,250);wifi.GetComponent<PresenceIndicator>().color=new Color(.8f,.05f,.12f,.24f);wifi.GetComponent<PresenceIndicator>().raycastTarget=false;
  ChainGame.Label(connectionOverlay.transform,"Connecting",new Vector2(.15f,.38f),new Vector2(.85f,.46f),44,new Color(.65f,.08f,.12f));connectionOverlay.SetActive(false);
 }
 private void SceneLoaded(Scene scene,LoadSceneMode mode){
  if(!Active || ending)return;
  transition=false;
  if(Game=="Chain"){StartCoroutine(OpenChain());return;}
  // Capture the actual scene before Start initialises the first round. Inactive parent
  // prevents the stored template's behaviours from running or subscribing to events.
  var roots=scene.GetRootGameObjects();stageRoot=new GameObject("Sprint original scene roots");
  foreach(var root in roots)root.transform.SetParent(stageRoot.transform,true);
  templateHolder=new GameObject("Inactive original scene template");templateHolder.transform.SetParent(transform,false);templateHolder.SetActive(false);
  stageTemplate=Instantiate(stageRoot,templateHolder.transform,false);stageTemplate.name="Sprint stage template";
 }
 private IEnumerator OpenChain(){
  yield return null;var menu=FindObjectOfType<MenuNavigation>();if(menu==null){Fail("Could not open Chain");yield break;}
  menu.GoToTraining();ChainGame.Attach((RectTransform)menu.trainingScreen.transform);
  chainGame=menu.trainingScreen.GetComponentInChildren<ChainGame>(true);chainGame.Open();chainGame.StartLevel(20);
 }
 private void LoadGame(){
  ChallengeConstants.s_isChallenge=true;LevelScript._selectedLevel=Battle.tier;
  Random=new SprintRandom(StageSeed);solved.Clear();
  SceneManager.LoadSceneAsync(Game=="Chain"?"GameMenu":Game+"Scene");
 }
 public IEnumerator Intro(){
  if(State!=null)yield break;
  if(introStarting){while(State==null && !ending)yield return null;yield break;}
  introStarting=true;
  if(Battle.live)cue.text="Waiting for opponent";
  else {cue.text="Neurons on the ready";yield return new WaitForSecondsRealtime(2);}
  bool waiting=true;string failure=null;
  api.Request<SprintBattleView>("/"+Battle.id+"/start","{}",battle=>{Battle=battle;waiting=false;},error=>{failure=error;waiting=false;});
  while(waiting)yield return null;
  if(failure!=null){introStarting=false;Fail(failure);while(State==null && !ending)yield return null;yield break;}
  if(Concluded(Battle)){ReturnToLobby(Battle);yield break;}
  if(Battle.live){
   // Both originals must be loaded before the server schedules one common deadline.
   while(Battle.legs[LegIndex].liveStart==0){
    cue.text="Waiting for opponent";yield return new WaitForSecondsRealtime(1);
    waiting=true;failure=null;api.Request<SprintBattleView>("/"+Battle.id,null,battle=>{Battle=battle;waiting=false;},error=>{failure=error;waiting=false;});
    while(waiting)yield return null;
    if(failure!=null){introStarting=false;Fail(failure);while(State==null && !ending)yield return null;yield break;}
    if(Concluded(Battle)){ReturnToLobby(Battle);yield break;}
   }
  }
  long start=owner==Battle.ownerId?Battle.legs[LegIndex].ownerStart:Battle.legs[LegIndex].recipientStart;
  long target=Now+(start-Battle.serverTime)-Battle.roundTripMs/2;
  if(Battle.live){
   // The common server deadline stays fixed while the ready cue is shown.
   cue.text="Neurons on the ready";long until=Math.Min(target,Now+2000);
   while(Now<until)yield return null;
  }
  while(Battle.live && Now<target){cue.text=Mathf.CeilToInt((target-Now)/1000f).ToString();yield return null;}
  State=new ChallengeSprintState(Game,Battle.tier,target);
  introCover.SetActive(false);
  waitingBack.gameObject.SetActive(false);
  cue.text="GO!";cueUntil=Time.realtimeSinceStartup+.6f;
 }
 public static IEnumerator ReadyIntro(){if(Active)yield return Current.Intro();}
 public bool Turn(){return Ready && State.Turn(Now);}
 public void Error(){if(Ready)State.Error(Now);}
 public bool Solve(string identity){
  if(!Ready || !solved.Add(identity))return false;
  bool complete=State.Solve(Now);if(complete){transition=true;StartCoroutine(NextStage());}return complete;
 }
 private IEnumerator NextStage(){
  yield return null;if(ending || !State.Tick(Now))yield break;
  Random=new SprintRandom(StageSeed);solved.Clear();
  if(Game=="Chain"){transition=false;chainGame.StartLevel(20);yield break;}
  stageRoot.SetActive(false);Destroy(stageRoot);
  var next=Instantiate(stageTemplate,templateHolder.transform,false);next.SetActive(false);next.transform.SetParent(null,true);
  SceneManager.MoveGameObjectToScene(next,SceneManager.GetActiveScene());stageRoot=next;transition=false;next.SetActive(true);
 }
 private void Update(){
  if(GameManager.AchievementUserId!=owner){Close();return;}
  if(State==null || ending)return;
  State.Tick(Now);int remaining=State.Remaining(Now);
  if(Battle.live && Application.internetReachability==NetworkReachability.NotReachable)SetConnecting(true);
  if(connecting && State.Finished){Cancel(true);return;}
  timer.text=Mathf.CeilToInt(remaining/1000f).ToString();
  string announcement=ChallengeSprintRules.Cue(lastRemaining,remaining);lastRemaining=remaining;
  if(announcement!=null){cue.text=announcement;cueUntil=Time.realtimeSinceStartup+1;}
  if(Time.realtimeSinceStartup>cueUntil)cue.text="";
  int own=ChallengeSprintRules.Units(Game,State.Stage,State.Correct);
  if(Battle.live && !progressBusy && Time.unscaledTime>=progressAt){
   progressAt=Time.unscaledTime+1;progressBusy=true;
   api.Request<SprintBattleView>("/"+Battle.id+"/progress",JsonUtility.ToJson(new SprintProgress{legIndex=LegIndex,ms=90000-remaining,units=own}),value=>{
    if(this==null || closed)return;progressBusy=false;
    if(connecting && State.Remaining(Now)<=0){Cancel(true);return;}
    SetConnecting(false);Battle=value;var leg=value.legs[LegIndex];liveUnits=owner==value.ownerId?leg.recipientUnits:leg.ownerUnits;
    if(value.done && !ending){ending=true;SprintLobby.Latest=value;Close();SceneManager.LoadSceneAsync("GameMenu");}
   },error=>{if(this==null || closed)return;progressBusy=false;SetConnecting(true);},3);
  }
  int gap=own-(Battle.live?liveUnits:ChallengeSprintRules.OpponentAt(opponent,90000-remaining));
  if(paced || Battle.live){float fraction=Mathf.Clamp(gap/20f,-1,1);lead.rectTransform.anchorMin=verticalPace?new Vector2(0,.5f+Mathf.Min(0,fraction)*.5f):new Vector2(.5f+Mathf.Min(0,fraction)*.5f,0);lead.rectTransform.anchorMax=verticalPace?new Vector2(1,.5f+Mathf.Max(0,fraction)*.5f):new Vector2(.5f+Mathf.Max(0,fraction)*.5f,1);lead.color=Color.Lerp(new Color(.65f,.65f,.65f),gap>=0?new Color(.05f,.7f,.25f):new Color(.9f,.1f,.15f),Mathf.Abs(fraction));status.text=gap==0?"Level":gap>0?"Ahead +"+gap:"Behind "+gap;}
  else status.text="Set the pace!";
  if(State.Finished){if(connecting)Cancel(true);else{ending=true;StartCoroutine(Submit());}}
 }
 private void LateUpdate(){
  if(hud==null || paceTrack==null || status==null)return;var space=(RectTransform)hud.transform;bool vertical=space.rect.height<space.rect.width*1.6f;
  if(vertical!=verticalPace){var a=lead.rectTransform.anchorMin;var b=lead.rectTransform.anchorMax;lead.rectTransform.anchorMin=new Vector2(a.y,a.x);lead.rectTransform.anchorMax=new Vector2(b.y,b.x);verticalPace=vertical;}
  var label=status.rectTransform;
  if(vertical){paceTrack.anchorMin=new Vector2(.98f,.32f);paceTrack.anchorMax=new Vector2(.99f,.78f);paceCentre.anchorMin=new Vector2(-.4f,.497f);paceCentre.anchorMax=new Vector2(1.4f,.503f);label.anchorMin=label.anchorMax=new Vector2(.95f,.55f);label.sizeDelta=new Vector2(330,24);label.anchoredPosition=Vector2.zero;label.localRotation=Quaternion.Euler(0,0,90);status.fontSize=20;status.color=Game=="Flash"?ChainGame.Ink:Color.white;}
  else{paceTrack.anchorMin=new Vector2(.18f,.90f);paceTrack.anchorMax=new Vector2(.82f,.908f);paceCentre.anchorMin=new Vector2(.497f,-.4f);paceCentre.anchorMax=new Vector2(.503f,1.4f);label.localRotation=Quaternion.identity;label.anchorMin=new Vector2(.1f,.86f);label.anchorMax=new Vector2(.9f,.90f);label.offsetMin=label.offsetMax=Vector2.zero;status.fontSize=22;status.color=ChainGame.Ink;}
 }
 private int lastRemaining=90000;
 private IEnumerator Submit(){
  if(quitDialog!=null){Destroy(quitDialog);quitDialog=null;}
  cue.text="BRAINS OUT!";
  foreach(var canvas in FindObjectsOfType<Canvas>())if(canvas.gameObject.scene==SceneManager.GetActiveScene()){var group=canvas.GetComponent<CanvasGroup>();if(group==null)group=canvas.gameObject.AddComponent<CanvasGroup>();group.interactable=false;group.blocksRaycasts=false;}
  string payload=JsonUtility.ToJson(new SprintSubmit{legIndex=LegIndex,result=State.Result()});
  PlayerPrefs.SetString("sprint.pending."+owner,JsonUtility.ToJson(Battle)+"\n"+payload);PlayerPrefs.Save();
  PlayerPrefs.DeleteKey("sprint.active."+owner);PlayerPrefs.Save();
  yield return new WaitForSecondsRealtime(1);
  api.Request<SprintBattleView>("/"+Battle.id+"/result",payload,battle=>{
   PlayerPrefs.DeleteKey("sprint.pending."+owner);PlayerPrefs.Save();SprintLobby.Latest=battle;var callback=finished;Close();SceneManager.LoadSceneAsync("GameMenu");if(callback!=null)callback(battle);
  },Fail);
 }
 private bool Concluded(SprintBattleView battle){return battle.done || (battle.legs!=null && LegIndex<battle.legs.Length && (owner==battle.ownerId?battle.legs[LegIndex].ownerPlayed:battle.legs[LegIndex].recipientPlayed));}
 private void ReturnToLobby(SprintBattleView battle){if(closed)return;PlayerPrefs.DeleteKey("sprint.pending."+owner);if(battle.done)PlayerPrefs.DeleteKey("sprint.cancel."+owner);PlayerPrefs.Save();SprintLobby.Latest=battle;Close();SceneManager.LoadSceneAsync("GameMenu");}
 private void Fail(string error){
  cue.text="";status.text=error;retry.gameObject.SetActive(true);retry.transform.SetAsLastSibling();waitingBack.gameObject.SetActive(true);
  api.Request<SprintBattleView>("/"+Battle.id,null,battle=>{if(this!=null && Concluded(battle))ReturnToLobby(battle);},ignored=>{});
 }
 public void Quit(){
  if(quitDialog!=null)return;
  quitDialog=Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/MidGameQuitPanel"),hud.transform,false);
  var content=quitDialog.transform.GetChild(0);var no=content.GetChild(2).GetComponent<Button>();var yes=content.GetChild(3).GetComponent<Button>();
  foreach(var text in content.GetComponentsInChildren<Text>(true))text.text=text.text.Replace("quit?","give up?").Replace("Quit?","Give up?");
  no.onClick=new Button.ButtonClickedEvent();yes.onClick=new Button.ButtonClickedEvent();
  no.onClick.AddListener(()=>{Destroy(quitDialog);quitDialog=null;});
  yes.onClick.AddListener(()=>{Destroy(quitDialog);quitDialog=null;Cancel();});
 }
 private void Cancel(bool connectionLost=false){
  ending=cancelling=true;
  Battle.endReason=connectionLost?"connection":"gave_up";
  PlayerPrefs.SetString("sprint.cancel."+owner,JsonUtility.ToJson(Battle));PlayerPrefs.Save();
  api.Request<SprintBattleView>("/"+Battle.id+"/cancel",JsonUtility.ToJson(new SprintCancel{reason=Battle.endReason}),ReturnToLobby,error=>{if(closed)return;Close();SceneManager.LoadSceneAsync("GameMenu");});
 }
 private void SetConnecting(bool value){
  if(connecting==value)return;connecting=value;
  if(value){savedScale=Time.timeScale;Time.timeScale=0;}else Time.timeScale=savedScale;
  if(connectionOverlay!=null){connectionOverlay.SetActive(value);connectionOverlay.transform.SetAsLastSibling();}
 }
 private void OnApplicationPause(bool paused){if(Battle!=null && Battle.live && State!=null && !ending){SetConnecting(true);progressAt=0;}}
 public void Close(){
  if(closed)return;closed=true;
  SetConnecting(false);
  PlayerPrefs.DeleteKey("sprint.active."+owner);PlayerPrefs.Save();
  SceneManager.sceneLoaded-=SceneLoaded;
  // The old scene can survive until the asynchronous menu load completes. Do not
  // let its normal180second training timers or coroutines resume in that gap.
  if(stageRoot!=null)stageRoot.SetActive(false);
  foreach(var game in FindObjectsOfType<GamesScript>())if(game.gameObject.scene==SceneManager.GetActiveScene()){game.StopAllCoroutines();game.enabled=false;}
  if(chainGame!=null)chainGame.enabled=false;
  ChallengeConstants.s_isChallenge=false;CardFaces.EndSession();Current=null;Destroy(gameObject);
 }
 private void OnDestroy(){SceneManager.sceneLoaded-=SceneLoaded;if(Current==this)Current=null;}
}

