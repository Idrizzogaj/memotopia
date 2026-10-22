#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using Assets.Script.Constants;
using Assets.Script.Models;
public sealed class SprintPlayDriver : MonoBehaviour {
 private long now;private string priorToken;private bool exiting;private SprintBattleView battle;private int resultAttempts;private string lastResult;
 void Start(){
  priorToken=PlayerPrefs.GetString("token");if(priorToken=="Bearer editor-sprint-test")priorToken="";SessionState.SetString("SprintPlayPriorToken",priorToken);PlayerPrefs.SetString("token","Bearer editor-sprint-test");
  UserConstants.s_user=new User{id=1999999963,username="Sprint test",userStatistics=new UserStatistics{xp=1000,level=10}};
  SprintRuntime.EditorNow=()=>now;SprintAPI.EditorTransport=Transport;
  Application.logMessageReceived+=Log;SceneManager.sceneLoaded+=SceneLoaded;StartCoroutine(Cases());
 }
 private void Log(string message,string stack,LogType type){if(type==LogType.Exception)Exit(1,"PLAY_EXCEPTION: "+message+" "+stack);}
 private void SceneLoaded(Scene scene,LoadSceneMode mode){if(scene.name=="GameMenu")foreach(var root in scene.GetRootGameObjects())foreach(var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))if(behaviour!=null && !(behaviour is UnityEngine.EventSystems.UIBehaviour))behaviour.enabled=false;}
 private void Transport(string path,string payload,Action<string> success,Action<string> fail){
  if(path.EndsWith("/start")){battle.serverTime=100000;battle.legs[0].ownerStart=100000;success(JsonUtility.ToJson(battle));return;}
  if(path.EndsWith("/result")){lastResult=payload;resultAttempts++;if(resultAttempts==1)fail("Test offline retry");else{battle.done=true;success(JsonUtility.ToJson(battle));}return;}
  if(path=="" && payload==null){success(JsonUtility.ToJson(new SprintBattleList{battles=new[]{battle}}));return;}
  fail("Unexpected test request "+path);
 }
 private void Check(bool valid,string message){if(!valid)throw new Exception(message);}
 private IEnumerator Wait(Func<bool> valid,string description){float deadline=Time.realtimeSinceStartup+35;while(!valid()){if(Time.realtimeSinceStartup>deadline)throw new Exception("Timed out: "+description);yield return null;}}
 private object Field(object instance,string name){return instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance);}
 private void ContentCases(){
  var missing=SprintAPI.Decode<SprintBattleView>("{\"legs\":[{\"game\":\"Pairs\",\"ownerPlayed\":false,\"recipientPlayed\":false,\"ownerResult\":null,\"recipientResult\":null}]}");
  Check(missing.legs[0].ownerResult==null && missing.legs[0].recipientResult==null,"Unplayed JSON result stays null");
  var zero=SprintAPI.Decode<SprintBattleView>("{\"legs\":[{\"ownerPlayed\":true,\"recipientPlayed\":false,\"ownerResult\":{\"completed\":0,\"correct\":0,\"roundMs\":[],\"trace\":[]}}]}");
  Check(zero.legs[0].ownerResult!=null && zero.legs[0].recipientResult==null,"Zero score is still a played result");
  string preference=CardFaces.Describe(CardFaces.Selected);
  foreach(int tier in new[]{6,12,18,21}){
   CardFaces.BeginSprintSession(tier,12345);var kinds=Enumerable.Range(1,100).Select(CardFaces.KindOf).ToArray();var numbers=Enumerable.Range(1,100).Select(CardFaces.NumberText).ToArray();
   var deck=(int[])typeof(CardFaces).GetField("sprintDeckIndex",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);var cards=(int[])deck.Clone();
   Check(kinds.Distinct().Count()==ChallengeSprintRules.KindCount(tier),"Allowed content kinds");
   Check(kinds.Where(k=>k==CardKind.PlayingCard).Count()==(tier==21?25:0),"Cards only Athlete");
   Check(Enumerable.Range(1,100).Where(i=>kinds[i-1]==CardKind.PlayingCard).Select(i=>cards[i]).Distinct().Count()==(tier==21?25:0),"Unique playing cards");
   CardFaces.BeginSprintSession(tier,12345);Check(kinds.SequenceEqual(Enumerable.Range(1,100).Select(CardFaces.KindOf)) && numbers.SequenceEqual(Enumerable.Range(1,100).Select(CardFaces.NumberText)) && cards.SequenceEqual(deck),"Same seed reproduces content");
  }
  CardFaces.EndSession();Check(preference==CardFaces.Describe(CardFaces.Selected),"Challenge does not change training preferences");Debug.Log("SPRINT_CONTENT_VALIDATION_PASSED");
 }
 private IEnumerator Cases(){
  ContentCases();
  foreach(var game in ChallengeSprintRules.Games){
   now=0;battle=new SprintBattleView{id=1,version=1,ownerId=1999999963,recipientId=2,mode="single",tier=18,legs=new[]{new SprintLegView{game=game,seed=12345}}};
   SprintRuntime.Begin(battle,null);yield return Wait(()=>SprintRuntime.Current!=null && SprintRuntime.Current.Ready,game+" intro");
   Check(SprintRuntime.Current.ClockText=="90","Intro must be outside play time");
   if(game=="Pairs"){
    var manager=FindObjectOfType<PairsManager>();Check(manager!=null,"Original PairsManager");for(int i=0;i<3;i++)manager.NextPair();
    Check(manager.gameScreen.activeSelf,"Original Pairs recall");
    var visited=new System.Collections.Generic.HashSet<string>();
    foreach(var pair in manager.pairs.ToArray()){
     if(!visited.Add(pair.Key))continue;visited.Add(pair.Value);now+=500;
     var buttons=manager.gameScreen.GetComponentsInChildren<Button>(true);buttons.First(b=>b.name==pair.Key).onClick.Invoke();yield return null;
     buttons.First(b=>b.name==pair.Value).onClick.Invoke();yield return new WaitForSecondsRealtime(.1f);
    }
   }else if(game=="Flash"){
    var manager=FindObjectOfType<FlashManager>();yield return Wait(()=>manager.recallPanel.activeSelf,"Flash animated study");
    var sequence=(int[])Field(manager,"imagesNumberForRecallNoShuffle");Check(sequence.Length==5,"Flash starts5");
    foreach(int id in sequence){now+=500;manager.recallPanel.GetComponentsInChildren<Button>(true).First(b=>b.name==id.ToString()).onClick.Invoke();}
   }else if(game=="Boxes"){
    var manager=FindObjectOfType<BoxesManager>();Check(manager.spawnNumber==5,"Boxes starts5");manager.recallBtn.GetComponent<Button>().onClick.Invoke();yield return null;
    var items=manager.scrollViewContent.GetComponentsInChildren<DragAndDropItem>().ToArray();Check(items.Length==5,"Five tray items");
    var wrongCell=FindObjectsOfType<DragAndDropCell>().First(c=>c.name.EndsWith(" Number"));string cellName=wrongCell.name,itemName=items[0].name;wrongCell.name="10 Number";items[0].name="1";
    var wrongData=new PointerEventData(EventSystem.current);items[0].OnBeginDrag(wrongData);wrongCell.OnDrop(wrongData);items[0].OnEndDrag(wrongData);
    Check(SprintRuntime.Current.State.Correct==0 && SprintRuntime.Current.State.Errors==1,"Prefix collision is an error, never progress");wrongCell.name=cellName;items[0].name=itemName;
    foreach(var item in items){now+=500;var target=FindObjectsOfType<DragAndDropCell>().First(c=>c.name==item.name+" Number");var data=new PointerEventData(EventSystem.current);item.OnBeginDrag(data);target.OnDrop(data);item.OnEndDrag(data);yield return new WaitForSecondsRealtime(.1f);}
   }else{
    var manager=FindObjectOfType<ChainGame>();Check(manager!=null,"Original Chain");for(int i=0;i<5;i++)manager.NextCard();
    var state=(ChainState)Field(manager,"state");Check(state.Sequence.Length==5,"Chain starts5");foreach(int id in state.Sequence){now+=500;manager.Choose(id);}
   }
   yield return Wait(()=>SprintRuntime.Current.State.Stage==1 && SprintRuntime.Current.Ready,game+" warm reset");
   yield return null;yield return null;
   Check(SprintRuntime.Current.Size==(game=="Pairs"?5:7),"Next stage size");
   Check(SprintRuntime.Current.State.Result().roundMs.Length==1,"Round duration retained");
   var runtime=SprintRuntime.Current;runtime.Quit();Check(!runtime.Ready,"Quit blocks input");int correct=runtime.State.Correct;runtime.Solve("behind dialog");Check(runtime.State.Correct==correct,"No solving behind quit dialog");
   var dialog=(GameObject)Field(runtime,"quitDialog");dialog.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.Invoke();yield return null;Check(runtime.Ready,"NO returns to game");
   Debug.Log("SPRINT_PLAY_STAGE_PASSED: "+game);
   if(game=="Chain"){
    now=90000;yield return Wait(()=>resultAttempts==1,"Deadline submits result");Check(!runtime.Ready,"Time over blocks input");Check(PlayerPrefs.HasKey("sprint.pending."+GameManager.AchievementUserId),"Result persisted before sync");
    var submitted=JsonUtility.FromJson<SprintSubmit>(lastResult);Check(submitted.result.completed==1,"Submitted completed stage");
    var retry=(Button)Field(runtime,"retry");Check(retry.gameObject.activeSelf,"Offline retry shown");retry.onClick.Invoke();
    yield return Wait(()=>!SprintRuntime.Active,"Result acknowledged");Check(!PlayerPrefs.HasKey("sprint.pending."+GameManager.AchievementUserId),"Receipt clears outbox");Debug.Log("SPRINT_DEADLINE_RETRY_VALIDATION_PASSED");yield return null;continue;
   }
   SprintRuntime.Current.Close();yield return null;
  }
  yield return Wait(()=>SceneManager.GetActiveScene().name=="GameMenu" && FindObjectOfType<MenuNavigation>()!=null,"Return to menu");
  var menu=FindObjectOfType<MenuNavigation>();var navigation=menu.GetComponent<ChallengeNavigation>();TrainingHubLayout.ApplyChallenge(navigation);
  menu.challengeScreen.SetActive(false);menu.GoToNotification();Check(menu.challengeScreen.activeSelf,"Bell activates Challenge before API request");
  var lobby=navigation.GetComponent<SprintLobby>();Check(Field(lobby,"panel")!=null,"Bell opens sprint inbox");Check(navigation.challengeGameScreen.transform.Find("Chain challenge")!=null,"Chain shares Challenge selection");
  Debug.Log("SPRINT_MENU_VALIDATION_PASSED");Exit(0,"SPRINT_PLAY_VALIDATION_PASSED");
 }
 private void Exit(int code,string message){if(exiting)return;exiting=true;SessionState.SetBool("SprintPlayRunning",false);PlayerPrefs.SetString("token",SessionState.GetString("SprintPlayPriorToken",""));PlayerPrefs.Save();Application.logMessageReceived-=Log;SceneManager.sceneLoaded-=SceneLoaded;SprintAPI.EditorTransport=null;SprintRuntime.EditorNow=null;EditorApplication.ExecuteMenuItem("SprintReview/Restore");Debug.Log(message);EditorApplication.Exit(code);}
}
#endif
