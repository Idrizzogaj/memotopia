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
 private long now;private string priorToken;private bool exiting,networkOffline;private SprintBattleView battle;private int resultAttempts;private string lastResult;
 void Start(){
  priorToken=PlayerPrefs.GetString("token");if(priorToken=="Bearer editor-sprint-test")priorToken="";SessionState.SetString("SprintPlayPriorToken",priorToken);PlayerPrefs.SetString("token","Bearer editor-sprint-test");
  UserConstants.s_user=new User{id=1999999963,username="Sprint test",userStatistics=new UserStatistics{xp=1000,level=10}};
  SprintRuntime.EditorNow=()=>now;SprintAPI.EditorTransport=Transport;
  Application.logMessageReceived+=Log;SceneManager.sceneLoaded+=SceneLoaded;StartCoroutine(Cases());
 }
 private void Log(string message,string stack,LogType type){if(type==LogType.Exception)Exit(1,"PLAY_EXCEPTION: "+message+" "+stack);}
 private void SceneLoaded(Scene scene,LoadSceneMode mode){if(scene.name=="GameMenu")foreach(var root in scene.GetRootGameObjects())foreach(var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))if(behaviour!=null && !(behaviour is UnityEngine.EventSystems.UIBehaviour))behaviour.enabled=false;}
 private void Transport(string path,string payload,Action<string> success,Action<string> fail){
  if(path.EndsWith("/start")){if(battle.live)Check(((Text)Field(SprintRuntime.Current,"cue")).text=="Waiting for opponent","Waiting must precede ready cue");Debug.Log("Mock start live="+battle.live+" now="+now);battle.serverTime=100000;battle.legs[0].ownerStart=battle.live?104000:100000;if(battle.live){battle.legs[0].liveStart=104000;battle.legs[0].recipientStart=104000;}success(JsonUtility.ToJson(battle));return;}
  if(path.EndsWith("/progress")){if(networkOffline){fail("Simulated connection loss");return;}battle.legs[0].recipientUnits=2;success(JsonUtility.ToJson(battle));return;}
  if(path.EndsWith("/cancel")){battle.done=true;battle.winner=battle.recipientId;battle.forfeitedId=battle.ownerId;battle.endReason="connection";success(JsonUtility.ToJson(battle));return;}
  if(path.EndsWith("/result")){lastResult=payload;resultAttempts++;if(resultAttempts==1)fail("Test offline retry");else{battle.done=true;success(JsonUtility.ToJson(battle));}return;}
  if(path=="" && payload==null){success(JsonUtility.ToJson(new SprintBattleList{battles=new[]{battle}}));return;}
  fail("Unexpected test request "+path);
 }
 private void Check(bool valid,string message){if(!valid)throw new Exception(message);}
 private IEnumerator Wait(Func<bool> valid,string description){float deadline=Time.realtimeSinceStartup+35;while(!valid()){if(Time.realtimeSinceStartup>deadline){Debug.Log("Wait diagnostic: "+description+" scene="+SceneManager.GetActiveScene().name+" active="+SprintRuntime.Active+" account="+GameManager.AchievementUserId+" state="+(SprintRuntime.Active&&SprintRuntime.Current.State!=null)+" intro="+(SprintRuntime.Active?Field(SprintRuntime.Current,"introStarting"):null)+" chain="+FindObjectOfType<ChainGame>());throw new Exception("Timed out: "+description);}yield return null;}}
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
  foreach(var game in SessionState.GetBool("SprintLiveOnly",false)?new string[0]:ChallengeSprintRules.Games){
   now=0;battle=new SprintBattleView{id=1,version=1,ownerId=1999999963,recipientId=2,mode="single",tier=18,legs=new[]{new SprintLegView{game=game,seed=12345}}};
   SprintRuntime.Begin(battle,null);yield return Wait(()=>SprintRuntime.Current!=null && SprintRuntime.Current.Ready,game+" intro");
   Check(SprintRuntime.Current.ClockText=="90","Intro must be outside play time");
   yield return null;yield return null;
   var reactions=SprintRuntime.Current.GetComponentInChildren<SprintReactions>();var reactionBar=(RectTransform)reactions.transform.Find("Six reactions");
   Check(reactionBar.gameObject.activeSelf && reactions.GetComponentsInChildren<Image>().Count(i=>i.name.StartsWith("Emoji ") && i.gameObject.activeInHierarchy)==6,"Six persistent reaction icons during "+game);
   if(game!="Flash"){
    var nextButton=FindObjectsOfType<Button>().Where(b=>!b.transform.IsChildOf(reactions.transform) && (b.name.ToLowerInvariant().Contains("next")||b.name.ToLowerInvariant().Contains("recall"))).OrderBy(b=>b.transform.position.y).FirstOrDefault();
    Check(nextButton!=null,"Original study control for reaction placement");var corners=new Vector3[4];((RectTransform)nextButton.transform).GetWorldCorners(corners);var canvas=nextButton.GetComponentInParent<Canvas>().rootCanvas;float nextBottom=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,corners[0]).y;reactionBar.GetWorldCorners(corners);Check(corners[1].y<nextBottom,"Compact reaction dock below original Next/Recall in "+game);
   }
   Debug.Log("REACTION_BAR_LAYOUT_PASSED: "+game);CaptureReactionLayout(game.ToLowerInvariant()+"-compact-reactions");
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
    var manager=FindObjectOfType<BoxesManager>();Check(manager.spawnNumber==5,"Boxes starts5");manager.recallBtn.GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);
    var trayCorners=new Vector3[4];((RectTransform)manager.bottomIconsContainer.transform).GetWorldCorners(trayCorners);var trayCanvas=manager.bottomIconsContainer.GetComponentInParent<Canvas>().rootCanvas;float trayBottom=RectTransformUtility.WorldToScreenPoint(trayCanvas.worldCamera,trayCorners[0]).y;reactionBar.GetWorldCorners(trayCorners);Check(trayBottom>trayCorners[1].y,"Boxes tray must stay above reaction dock");CaptureReactionLayout("boxes-compact-reactions-recall");
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
   var oldTimers=FindObjectsOfType<Text>().Where(t=>t==((game=="Boxes"?FindObjectOfType<BoxesManager>().countDownText:game=="Flash"?FindObjectOfType<FlashManager>().timerText:null))).ToArray();var timerValues=oldTimers.Select(t=>t.text).ToArray();
   SprintRuntime.Current.Close();yield return null;yield return null;
   for(int t=0;t<oldTimers.Length;t++)Check(oldTimers[t].text==timerValues[t],"Training timer must stay frozen after Challenge close");
  }
  if(SessionState.GetBool("SprintLiveOnly",false)){
  resultAttempts=2;
  if(SceneManager.GetActiveScene().name!="GameMenu")SceneManager.LoadSceneAsync("GameMenu");
  yield return Wait(()=>SceneManager.GetActiveScene().name=="GameMenu" && FindObjectOfType<MenuNavigation>()!=null,"Menu before live case");yield return null;yield return null;
  now=0;battle=new SprintBattleView{id=7,version=2,live=true,invitation="accepted",ownerId=1999999963,recipientId=2,mode="single",tier=6,legs=new[]{new SprintLegView{game="Chain",seed=12345}}};
  SprintRuntime.Begin(battle,null);yield return Wait(()=>battle.legs[0].liveStart>0,"Live readiness receipt");
  Check(SprintRuntime.Current.State==null,"Live countdown cannot start gameplay early");now=4000;
  yield return Wait(()=>SprintRuntime.Current!=null && SprintRuntime.Current.Ready,"Live shared start");Check(SprintRuntime.Current.ClockText=="90","Shared start has90seconds");
  now=5000;yield return new WaitForSecondsRealtime(1.2f);Check(((Text)Field(SprintRuntime.Current,"status")).text=="Behind -2","Live opponent progress reaches the original HUD");
  var liveRuntime=SprintRuntime.Current;networkOffline=true;now=34000;yield return new WaitForSecondsRealtime(1.3f);
  Check(!liveRuntime.Ready && (bool)Field(liveRuntime,"connecting"),"Connection loss blocks input");Check(((GameObject)Field(liveRuntime,"connectionOverlay")).activeSelf,"Large Connecting wifi overlay visible");
  int before=liveRuntime.State.Correct;liveRuntime.Solve("offline input");Check(before==liveRuntime.State.Correct,"No offline progress");
  now=64000;networkOffline=false;yield return new WaitForSecondsRealtime(1.3f);Check(liveRuntime.Ready && liveRuntime.ClockText=="30","Reconnect resumes same state with real30seconds left");Check(liveRuntime.State.Correct==before,"Reconnect must preserve board progress");
  networkOffline=true;now=90000;yield return new WaitForSecondsRealtime(1.3f);now=94000;yield return Wait(()=>!SprintRuntime.Active,"Offline deadline forfeits");Check(battle.endReason=="connection" && battle.winner==battle.recipientId,"Offline at90seconds loses match");Check(Mathf.Approximately(Time.timeScale,1),"Connection freeze must be restored on exit");Debug.Log("LIVE_SPRINT_PLAY_PASSED: shared clock, input freeze, visible wifi,30second reconnect preserves progress, offline deadline forfeit");
  }
  yield return Wait(()=>SceneManager.GetActiveScene().name=="GameMenu" && FindObjectOfType<MenuNavigation>()!=null,"Return to menu");
  var menu=FindObjectOfType<MenuNavigation>();var navigation=menu.GetComponent<ChallengeNavigation>();TrainingHubLayout.ApplyChallenge(navigation);
  menu.challengeScreen.SetActive(false);menu.GoToNotification();Check(menu.challengeScreen.activeSelf,"Bell activates Challenge before API request");
  var lobby=navigation.GetComponent<SprintLobby>();Check(Field(lobby,"panel")!=null,"Bell opens sprint inbox");Check(navigation.challengeGameScreen.transform.Find("Chain challenge")!=null,"Chain shares Challenge selection");
  Debug.Log("SPRINT_MENU_VALIDATION_PASSED");Exit(0,"SPRINT_PLAY_VALIDATION_PASSED");
 }
 // Immediate offscreen capture: ScreenCapture is deferred and does not produce files in batch play mode.
 private void CaptureReactionLayout(string name){
  var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas&&c.gameObject.activeInHierarchy).ToArray();
  var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var rects=canvases.Select(c=>(RectTransform)c.transform).ToArray();var positions=rects.Select(r=>r.position).ToArray();var scales=rects.Select(r=>r.localScale).ToArray();var sizes=rects.Select(r=>r.sizeDelta).ToArray();var scalers=canvases.Select(c=>c.GetComponent<CanvasScaler>()).ToArray();var enabled=scalers.Select(s=>s!=null&&s.enabled).ToArray();
  var transforms=canvases.SelectMany(c=>c.GetComponentsInChildren<Transform>(true)).Distinct().ToArray();var layers=transforms.Select(t=>t.gameObject.layer).ToArray();
  var layoutRects=transforms.OfType<RectTransform>().ToArray();var mins=layoutRects.Select(r=>r.anchorMin).ToArray();var maxs=layoutRects.Select(r=>r.anchorMax).ToArray();var layoutSizes=layoutRects.Select(r=>r.sizeDelta).ToArray();var layoutPositions=layoutRects.Select(r=>r.anchoredPosition3D).ToArray();
  var camera=new GameObject("Reaction review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;
  try{
   for(int i=0;i<canvases.Length;i++){if(scalers[i]!=null)scalers[i].enabled=false;canvases[i].renderMode=RenderMode.WorldSpace;canvases[i].worldCamera=camera;rects[i].position=Vector3.zero;rects[i].localScale=Vector3.one;}
   foreach(var t in transforms)t.gameObject.layer=30;
   foreach(bool tablet in new[]{false,true}){
    for(int i=0;i<layoutRects.Length;i++){layoutRects[i].anchorMin=mins[i];layoutRects[i].anchorMax=maxs[i];layoutRects[i].sizeDelta=layoutSizes[i];layoutRects[i].anchoredPosition3D=layoutPositions[i];}
    foreach(var rect in rects)rect.sizeDelta=tablet?new Vector2(800,1067):new Vector2(800,1734);Canvas.ForceUpdateCanvases();camera.orthographicSize=(tablet?1067:1734)/2f;
    var rt=new RenderTexture(800,tablet?1067:1734,24);camera.targetTexture=rt;foreach(var reactions in FindObjectsOfType<SprintReactions>())reactions.SendMessage("LateUpdate");Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();System.IO.File.WriteAllBytes("../ChainReuseFix/"+name+(tablet?"-ipad":"-phone")+".png",texture.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;DestroyImmediate(texture);DestroyImmediate(rt);
   }
  }finally{
   for(int i=0;i<layoutRects.Length;i++){layoutRects[i].anchorMin=mins[i];layoutRects[i].anchorMax=maxs[i];layoutRects[i].sizeDelta=layoutSizes[i];layoutRects[i].anchoredPosition3D=layoutPositions[i];}
   for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];rects[i].position=positions[i];rects[i].localScale=scales[i];rects[i].sizeDelta=sizes[i];if(scalers[i]!=null)scalers[i].enabled=enabled[i];}for(int i=0;i<transforms.Length;i++)transforms[i].gameObject.layer=layers[i];DestroyImmediate(camera.gameObject);Canvas.ForceUpdateCanvases();
  }
 }
 private void Exit(int code,string message){if(exiting)return;exiting=true;SessionState.SetBool("SprintLiveOnly",false);SessionState.SetBool("SprintPlayRunning",false);PlayerPrefs.SetString("token",SessionState.GetString("SprintPlayPriorToken",""));PlayerPrefs.Save();Application.logMessageReceived-=Log;SceneManager.sceneLoaded-=SceneLoaded;SprintAPI.EditorTransport=null;SprintRuntime.EditorNow=null;EditorApplication.ExecuteMenuItem("SprintReview/Restore");Debug.Log(message);EditorApplication.Exit(code);}
}
#endif
