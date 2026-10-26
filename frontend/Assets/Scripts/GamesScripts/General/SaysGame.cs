using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Playable native design preview. No invented server game/achievement IDs.
public sealed class SaysGame : MonoBehaviour {
 public static int Count(int level){if(level==21)return 40;if(level<1||level>20)throw new ArgumentOutOfRangeException("level");return level<=8?level:level<=15?8+(level-8)*2:level<20?22+(level-15)*3:37;}
 public static int ChallengeCount(int round){return 5+2*round;}
 static string Key {get{return "says.preview.v1."+GameManager.AchievementUserId;}}
 public static bool Complete(int level){return level>=1 && level<=20 && (PlayerPrefs.GetInt(Key,0)&(1<<(level-1)))!=0;}
 public static int Stars(int level){return level<1||level>20?0:Mathf.Clamp(PlayerPrefs.GetInt(Key+".stars."+level,Complete(level)?1:0),0,3);}
 static bool Unlocked(int level){return level==1||level==20||Complete(20)||Complete(level)||Complete(level-1);}
 GameObject overlay,levels,popup,recallCover;ChainOriginalPresentation board;Material theme;Button play;Text cue;readonly List<Button> keys=new List<Button>();readonly List<CanvasGroup> frames=new List<CanvasGroup>();int[] faceIds=new int[0];float wrongRemaining,recallRemaining;int wrongKey=-1;bool pendingFailure,recallTransition;int[] sequence;int owner,level,position,hearts,round,errors;bool watching,input,challenge;float elapsed,clock,deadline;int highlighted=-1;
 public static SaysGame Open(RectTransform parent){var game=parent.GetComponent<SaysGame>()??parent.gameObject.AddComponent<SaysGame>();game.Show();return game;}
 public void Show(){Close();owner=GameManager.AchievementUserId;challenge=false;overlay=ChainGame.Panel(GetComponentInParent<Canvas>().rootCanvas.transform,"Says native preview",Vector2.zero,Vector2.one,ChainGame.Purple);theme=new Material(Resources.Load<Shader>("MemotopiaTheme"));theme.SetColor("_ThemeColor",ChainGame.Purple);ShowLevels();}
 void ClearBoard(){if(recallCover!=null)CardFaces.DestroyGenerated(recallCover);recallCover=null;recallTransition=false;if(board!=null)board.Destroy();board=null;keys.Clear();frames.Clear();wrongRemaining=0;wrongKey=-1;pendingFailure=false;if(play!=null)CardFaces.DestroyGenerated(play.gameObject);play=null;if(cue!=null)CardFaces.DestroyGenerated(cue.gameObject);cue=null;highlighted=-1;}
 public void ShowLevels(){ClearBoard();input=watching=false;if(levels!=null)CardFaces.DestroyGenerated(levels);levels=Instantiate(Resources.Load<GameObject>("Prefabs/ExactFlashChainLevels"),overlay.transform,false);var rect=(RectTransform)levels.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.anchoredPosition3D=Vector3.zero;
  foreach(var button in levels.GetComponentsInChildren<Button>(true)){button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(Close);}
  var background=levels.GetComponent<Image>();background.sprite=Resources.Load<Sprite>("GamesResources/Levels/FlashBackground");background.material=theme;
  foreach(var image in levels.GetComponentsInChildren<Image>(true)){
   if(image.name=="Chain title"){image.sprite=Resources.Load<Sprite>("GamesResources/Levels/FlashTop");image.material=theme;var cover=ChainGame.Panel(image.transform,"Says title",new Vector2(.23f,.53f),new Vector2(.77f,.83f),Color.white);ChainGame.Label(cover.transform,"SAYS",Vector2.zero,Vector2.one,60,ChainGame.Purple);}
   if(!image.name.StartsWith("Chain Level "))continue;int number=int.Parse(image.name.Substring(12));bool unlocked=Unlocked(number);image.sprite=Resources.Load<Sprite>(unlocked?"GamesResources/Levels/AAA-11":"GamesResources/Levels/FlashLevelCell");if(!unlocked)image.material=theme;
   var button=image.transform.GetChild(1).GetComponent<Button>();button.interactable=unlocked;button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(()=>StartLevel(number));button.GetComponentInChildren<Text>(true).color=ChainGame.Purple;button.GetComponentInChildren<Text>(true).text=number.ToString();
   var stars=image.transform.GetChild(0);stars.gameObject.SetActive(Stars(number)>0);for(int n=0;n<stars.childCount;n++){stars.GetChild(n).gameObject.SetActive(n<Stars(number));var star=stars.GetChild(n).GetComponent<Image>();if(star!=null)star.sprite=Resources.Load<Sprite>("GamesResources/Levels/Asset "+(15-n));}
  }
  var images=levels.GetComponentsInChildren<Image>(true);TrainingAthleteTile.Attach(images.First(i=>i.name=="Chain Level 20"),images.First(i=>i.name=="Chain Level 16"),ChainGame.Purple,()=>StartLevel(21),Complete(20));
 }
 public void StartLevel(int value){PracticeSession.Release(this);if(!challenge && (value==21?!Complete(20):!Unlocked(value)))return;level=value;position=0;hearts=value==21?1:5;watching=input=false;elapsed=0;clock=180;ClearBoard();if(levels!=null){CardFaces.DestroyGenerated(levels);levels=null;}CardFaces.BeginSession();
  int count=challenge?ChallengeCount(round):Count(value);sequence=new int[count];for(int n=0;n<count;n++){int key;do{key=UnityEngine.Random.Range(0,10);}while(n>0 && key==sequence[n-1]);sequence[n]=key;}
  board=new ChainOriginalPresentation(overlay.transform,false,Quit,()=>{},theme);board.Hearts(challenge?0:hearts);board.ShowRecallClock(challenge?Mathf.CeilToInt(deadline-Time.realtimeSinceStartup):180,!challenge&&PracticeMode.Untimed);
  int[] pool=Enumerable.Range(1,CardFaces.ContentCount).Where(id=>!faceIds.Contains(id)).ToArray();for(int n=pool.Length-1;n>0;n--){int other=UnityEngine.Random.Range(0,n+1),picked=pool[n];pool[n]=pool[other];pool[other]=picked;}faceIds=pool.Take(10).ToArray();
  var gridLayout=board.Grid.GetComponent<GridLayoutGroup>();if(gridLayout!=null)gridLayout.enabled=false;board.Grid.SetParent(board.Root.transform,false);board.Grid.anchorMin=new Vector2(.08f,.16f);board.Grid.anchorMax=new Vector2(.92f,.77f);board.Grid.offsetMin=board.Grid.offsetMax=Vector2.zero;Canvas.ForceUpdateCanvases();
  float side=Mathf.Min((board.Grid.rect.width-32)/3,(board.Grid.rect.height-48)/4);
  for(int n=0;n<10;n++){
   int digit=n;var key=Instantiate(board.Cell,board.Grid,false);key.name="Says key "+digit;key.image.overrideSprite=CardFaces.SaysFace(faceIds[n],digit);key.image.preserveAspect=true;key.onClick=new Button.ButtonClickedEvent();key.onClick.AddListener(()=>Choose(digit));key.gameObject.SetActive(true);keys.Add(key);
   int slot=n==0?10:n-1;var r=(RectTransform)key.transform;r.anchorMin=r.anchorMax=new Vector2((slot%3+.5f)/3,1-(slot/3+.5f)/4);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(side,side);r.anchoredPosition=Vector2.zero;
   var frame=ChainGame.Panel(key.transform,"Playback frame",Vector2.zero,Vector2.one,Color.clear);frame.GetComponent<Image>().raycastTarget=false;var group=frame.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;frames.Add(group);
   for(int edge=0;edge<4;edge++){Vector2 min=edge==0?new Vector2(0,.975f):edge==1?new Vector2(.975f,0):Vector2.zero,max=edge<2?Vector2.one:edge==2?new Vector2(1,.025f):new Vector2(.025f,1);ChainGame.Panel(frame.transform,"Frame edge",min,max,ChainGame.Purple).GetComponent<Image>().raycastTarget=false;}
  }

  cue=ChainGame.Label(overlay.transform,challenge?"Challenge preview · "+count:"Memorise · "+count,new Vector2(.15f,.795f),new Vector2(.85f,.825f),26,ChainGame.Ink);
  play=SprintUi.Button(overlay.transform,"PLAY",new Vector2(.30f,.025f),new Vector2(.70f,.095f),BeginPlayback);if(challenge)BeginPlayback();
 }
 public void BeginPlayback(){if(sequence==null || watching || input)return;play.gameObject.SetActive(false);if(!challenge)PracticeSession.Begin(this,"Memory.Says",level);elapsed=0;watching=true;cue.text="Watch";}
 void Update(){if(overlay==null)return;if(owner!=GameManager.AchievementUserId){Close();return;}if(Input.GetKeyDown(KeyCode.Escape))Quit();if(challenge && board!=null){board.ShowRecallClock(Mathf.Max(0,Mathf.CeilToInt(deadline-Time.realtimeSinceStartup)));if(Time.realtimeSinceStartup>=deadline){Finish(false);return;}}if(popup!=null)return;Tick(Time.deltaTime);}
 public void Tick(float dt){
  if(board==null || popup!=null)return;
  if(!challenge&&PracticeMode.Untimed)board.ShowPracticeClock(PracticeSession.Clock(this));
  if(recallTransition){recallRemaining=Mathf.Max(0,recallRemaining-dt);if(recallRemaining<=0){recallTransition=false;if(recallCover!=null)CardFaces.DestroyGenerated(recallCover);recallCover=null;board.Grid.gameObject.SetActive(true);input=true;cue.text="Recall";}return;}
  if(wrongRemaining>0){wrongRemaining=Mathf.Max(0,wrongRemaining-dt);if(wrongRemaining<=0){wrongKey=-1;if(pendingFailure){pendingFailure=false;Finish(false);return;}}}
  float strength=0;
  if(watching){elapsed+=dt;float phase=elapsed-2;int show=phase>=0 && phase<sequence.Length*2.25f && phase%2.25f<2?sequence[Mathf.Min(sequence.Length-1,(int)(phase/2.25f))]:-1;Highlight(show);if(show>=0){float t=phase%2.25f;strength=Mathf.SmoothStep(0,1,Mathf.Clamp01(Mathf.Min(t/.18f,(2-t)/.18f)));}if(phase>=sequence.Length*2.25f+.7f)BeginRecall();}
  else if(input&&!challenge&&PracticeMode.Untimed){board.ShowPracticeClock(PracticeSession.Clock(this));}else if(input&&!challenge){clock=Mathf.Max(0,clock-dt);board.ShowRecallClock(Mathf.CeilToInt(clock));if(clock<=0){Finish(false);return;}}
  AnimateKeys(strength);
 }
 void BeginRecall(){
  watching=input=false;Highlight(-1);recallTransition=true;recallRemaining=.9f;cue.text="";board.Grid.gameObject.SetActive(false);
  recallCover=ChainGame.Panel(overlay.transform,"Says recall cue",new Vector2(.08f,.16f),new Vector2(.92f,.77f),Color.white);
  var logo=ChainGame.Panel(recallCover.transform,"Existing Memotopia logo",new Vector2(.28f,.40f),new Vector2(.72f,.82f),Color.white).GetComponent<Image>();logo.sprite=Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01");logo.preserveAspect=true;logo.raycastTarget=false;
  ChainGame.Label(recallCover.transform,"RECALL",new Vector2(.10f,.18f),new Vector2(.90f,.36f),64,ChainGame.Purple).fontStyle=FontStyle.Bold;
 }
 void Highlight(int value){highlighted=value;}
 void AnimateKeys(float strength){
  for(int n=0;n<keys.Count;n++){
   bool wrong=n==wrongKey && wrongRemaining>0;float amount=n==highlighted?strength:0;keys[n].transform.localScale=Vector3.one*(1+.12f*amount);keys[n].image.color=wrong?new Color(1,.45f,.45f):Color.white;
   var r=(RectTransform)keys[n].transform;r.anchoredPosition=wrong?new Vector2(Mathf.Sin(wrongRemaining*90)*r.rect.width*.04f*(wrongRemaining/.45f),0):Vector2.zero;frames[n].alpha=wrong?1:amount;
   foreach(var edge in frames[n].GetComponentsInChildren<Image>())if(edge.name=="Frame edge")edge.color=wrong?Color.red:ChainGame.Purple;
  }
 }
 public bool Choose(int digit){if(!input||wrongRemaining>0||popup!=null||sequence==null||digit<0||digit>9)return false;if(sequence[position]!=digit){errors++;wrongKey=digit;wrongRemaining=.45f;if(Application.isMobilePlatform)Handheld.Vibrate();if(!challenge){hearts--;board.Hearts(hearts);if(hearts==0){input=false;pendingFailure=true;}}AnimateKeys(0);return false;}var practice=PracticeSession.Get(this);if(practice!=null)practice.Progress(level);position++;if(position==sequence.Length){input=false;if(challenge){round++;StartLevel(1);}else Finish(true);}return true;}
 void Finish(bool won){var practice=PracticeSession.Get(this);if(practice!=null)practice.Stop();if(board==null)return;input=watching=false;Highlight(-1);wrongRemaining=0;wrongKey=-1;AnimateKeys(0);if(!challenge&&won){if(level<=20){PlayerPrefs.SetInt(Key,PlayerPrefs.GetInt(Key,0)|(1<<(level-1)));int stars=hearts==5?3:hearts>=3?2:1;PlayerPrefs.SetInt(Key+".stars."+level,Mathf.Max(stars,PlayerPrefs.GetInt(Key+".stars."+level,0)));}else PlayerPrefs.SetInt(Key+".athlete",1);PlayerPrefs.Save();}
  if(!challenge){ShowOriginalResult(won);return;}
  string result=challenge?round+" rounds + "+position+" correct · "+errors+" errors":won?"Level "+level+" complete":"Try again";Dialog(challenge?"BRAINS OUT!":won?"Well done!":"Round over",result,"Levels","Replay",()=>{Dismiss();challenge=false;ShowLevels();},()=>{Dismiss();if(challenge){round=errors=0;deadline=Time.realtimeSinceStartup+90;}StartLevel(level);});
 }
 void ShowOriginalResult(bool won){
  popup=Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/"+(won?"WinPanel":"LosePanel")),overlay.transform,false);var content=popup.transform.GetChild(0);
  UnityEngine.Events.UnityAction levelsAction=()=>{Dismiss();ShowLevels();},replay=()=>{Dismiss();StartLevel(level);};
  if(!won){var a=content.GetChild(2).GetComponent<Button>();var b=content.GetChild(3).GetComponent<Button>();a.onClick=new Button.ButtonClickedEvent();b.onClick=new Button.ButtonClickedEvent();a.onClick.AddListener(replay);b.onClick.AddListener(levelsAction);return;}
  bool athlete=level==21;content.GetChild(0).GetChild(0).GetComponent<Text>().text=athlete?"MEMORY ATHLETE":"LEVEL "+level;content.GetChild(10).GetChild(0).GetComponent<Text>().text=position.ToString();
  foreach(var stars in popup.GetComponentsInChildren<StarFillScript>(true)){int earned=athlete?0:hearts==5?3:hearts>=3?2:1;stars.EarnedStarsOverride=earned;stars.enabled=!athlete;if(!Application.isPlaying){var fills=new[]{stars.leftStar,stars.midStar,stars.rightStar};var visible=new[]{stars.leftStarG,stars.midStarG,stars.rightStarG};for(int n=0;n<3;n++){visible[n].SetActive(n<earned);fills[n].GetComponent<Image>().fillAmount=n<earned?1:0;}}}
  if(athlete)foreach(var image in popup.GetComponentsInChildren<Image>(true))if(image.name.IndexOf("Star",StringComparison.OrdinalIgnoreCase)>=0)image.gameObject.SetActive(false);
  GamesScript.ConfigureWinButtons(popup,level,replay,()=>{Dismiss();StartLevel(level+1);},levelsAction);
 }
 void Dialog(string first,string second,string left,string right,UnityEngine.Events.UnityAction no,UnityEngine.Events.UnityAction yes){if(popup!=null)return;popup=Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/MidGameQuitPanel"),overlay.transform,false);var content=popup.transform.GetChild(0);var texts=content.GetComponentsInChildren<Text>(true).Where(t=>!t.transform.IsChildOf(content.GetChild(2))&&!t.transform.IsChildOf(content.GetChild(3))).ToArray();if(texts.Length>0)texts[0].text=first+"\n"+second;for(int n=1;n<texts.Length;n++)texts[n].text="";var a=content.GetChild(2).GetComponent<Button>();var b=content.GetChild(3).GetComponent<Button>();a.onClick=new Button.ButtonClickedEvent();b.onClick=new Button.ButtonClickedEvent();a.GetComponentInChildren<Text>().text=left;b.GetComponentInChildren<Text>().text=right;a.onClick.AddListener(no);b.onClick.AddListener(yes);}
 void Dismiss(){if(popup!=null)CardFaces.DestroyGenerated(popup);popup=null;}
 void Quit(){if(board==null){Close();return;}Dialog("Are you sure you","want to quit?","No","Yes",Dismiss,Close);}
 public void Close(){PracticeSession.Release(this);ClearBoard();Dismiss();if(overlay!=null)CardFaces.DestroyGenerated(overlay);overlay=levels=null;if(theme!=null)CardFaces.DestroyGenerated(theme);theme=null;CardFaces.EndSession();}
 void OnDisable(){if(overlay!=null)Close();}
}
