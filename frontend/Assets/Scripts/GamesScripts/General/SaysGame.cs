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
 static bool Unlocked(int level){return level==1||level==20||Complete(20)||Complete(level)||Complete(level-1);}
 GameObject overlay,levels,popup;ChainOriginalPresentation board;Material theme;Button play;Text cue;readonly List<Button> keys=new List<Button>();int[] sequence;int owner,level,position,hearts,round,errors;bool watching,input,challenge;float elapsed,clock,deadline;int highlighted=-1;
 public static SaysGame Open(RectTransform parent){var game=parent.GetComponent<SaysGame>()??parent.gameObject.AddComponent<SaysGame>();game.Show();return game;}
 public void Show(){Close();owner=GameManager.AchievementUserId;challenge=false;overlay=ChainGame.Panel(GetComponentInParent<Canvas>().rootCanvas.transform,"Says native preview",Vector2.zero,Vector2.one,ChainGame.Purple);theme=new Material(Resources.Load<Shader>("MemotopiaTheme"));theme.SetColor("_ThemeColor",ChainGame.Purple);ShowLevels();}
 void ClearBoard(){if(board!=null)board.Destroy();board=null;keys.Clear();if(play!=null)CardFaces.DestroyGenerated(play.gameObject);play=null;if(cue!=null)CardFaces.DestroyGenerated(cue.gameObject);cue=null;highlighted=-1;}
 public void ShowLevels(){ClearBoard();input=watching=false;if(levels!=null)CardFaces.DestroyGenerated(levels);levels=Instantiate(Resources.Load<GameObject>("Prefabs/ExactFlashChainLevels"),overlay.transform,false);var rect=(RectTransform)levels.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.anchoredPosition3D=Vector3.zero;
  foreach(var button in levels.GetComponentsInChildren<Button>(true)){button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(Close);}
  var background=levels.GetComponent<Image>();background.sprite=Resources.Load<Sprite>("GamesResources/Levels/FlashBackground");background.material=theme;
  foreach(var image in levels.GetComponentsInChildren<Image>(true)){
   if(image.name=="Chain title"){image.sprite=Resources.Load<Sprite>("GamesResources/Levels/FlashTop");image.material=theme;var cover=ChainGame.Panel(image.transform,"Says title",new Vector2(.23f,.53f),new Vector2(.77f,.83f),Color.white);ChainGame.Label(cover.transform,"SAYS",Vector2.zero,Vector2.one,60,ChainGame.Purple);}
   if(!image.name.StartsWith("Chain Level "))continue;int number=int.Parse(image.name.Substring(12));bool unlocked=Unlocked(number);image.sprite=Resources.Load<Sprite>(unlocked?"GamesResources/Levels/AAA-11":"GamesResources/Levels/FlashLevelCell");if(!unlocked)image.material=theme;
   var button=image.transform.GetChild(1).GetComponent<Button>();button.interactable=unlocked;button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(()=>StartLevel(number));button.GetComponentInChildren<Text>(true).color=ChainGame.Purple;button.GetComponentInChildren<Text>(true).text=number.ToString();
   var stars=image.transform.GetChild(0);stars.gameObject.SetActive(Complete(number));for(int n=0;n<stars.childCount;n++)stars.GetChild(n).gameObject.SetActive(n<PlayerPrefs.GetInt(Key+".stars."+number,0));
  }
  var images=levels.GetComponentsInChildren<Image>(true);TrainingAthleteTile.Attach(images.First(i=>i.name=="Chain Level 20"),images.First(i=>i.name=="Chain Level 16"),ChainGame.Purple,()=>StartLevel(21),Complete(20));
  SprintUi.Button(levels.transform,"Challenge",new Vector2(.30f,.81f),new Vector2(.70f,.855f),()=>{challenge=true;round=errors=0;deadline=Time.realtimeSinceStartup+90;StartLevel(1);});
 }
 public void StartLevel(int value){if(!challenge && (value==21?!Complete(20):!Unlocked(value)))return;level=value;position=0;hearts=value==21?1:5;watching=input=false;elapsed=0;clock=180;ClearBoard();if(levels!=null){CardFaces.DestroyGenerated(levels);levels=null;}CardFaces.BeginSession();
  int count=challenge?ChallengeCount(round):Count(value);sequence=new int[count];for(int n=0;n<count;n++){int key;do{key=UnityEngine.Random.Range(0,10);}while(n>0 && key==sequence[n-1]);sequence[n]=key;}
  board=new ChainOriginalPresentation(overlay.transform,false,Quit,()=>{},theme);board.Hearts(challenge?0:hearts);board.ShowRecallClock(challenge?Mathf.CeilToInt(deadline-Time.realtimeSinceStartup):180);
  for(int n=0;n<10;n++){int digit=n;var key=Instantiate(board.Cell,board.Grid,false);key.name="Says key "+digit;key.image.overrideSprite=CardFaces.KindOf(n+1)==CardKind.Number?CardFaces.SingleDigit(n):CardFaces.SmallFace(n+1);key.image.preserveAspect=true;key.onClick=new Button.ButtonClickedEvent();key.onClick.AddListener(()=>Choose(digit));key.gameObject.SetActive(true);keys.Add(key);}board.FitOverflow(10);
  cue=ChainGame.Label(overlay.transform,challenge?"Challenge preview · "+count:"Memorise · "+count,new Vector2(.15f,.855f),new Vector2(.85f,.885f),26,ChainGame.Ink);
  play=SprintUi.Button(overlay.transform,"PLAY",new Vector2(.30f,.025f),new Vector2(.70f,.095f),BeginPlayback);if(challenge)BeginPlayback();
 }
 public void BeginPlayback(){if(sequence==null || watching || input)return;play.gameObject.SetActive(false);elapsed=0;watching=true;cue.text="Watch";}
 void Update(){if(overlay==null)return;if(owner!=GameManager.AchievementUserId){Close();return;}if(Input.GetKeyDown(KeyCode.Escape))Quit();if(challenge && board!=null){board.ShowRecallClock(Mathf.Max(0,Mathf.CeilToInt(deadline-Time.realtimeSinceStartup)));if(Time.realtimeSinceStartup>=deadline){Finish(false);return;}}if(popup!=null)return;Tick(Time.deltaTime);}
 public void Tick(float dt){if(board==null || popup!=null)return;if(watching){elapsed+=dt;float phase=elapsed-2;int show=phase>=0 && phase<sequence.Length*2.25f && phase%2.25f<2?sequence[Mathf.Min(sequence.Length-1,(int)(phase/2.25f))]:-1;Highlight(show);if(phase>=sequence.Length*2.25f+.7f){watching=false;input=true;Highlight(-1);cue.text="Recall";}}else if(input&&!challenge){clock=Mathf.Max(0,clock-dt);board.ShowRecallClock(Mathf.CeilToInt(clock));if(clock<=0)Finish(false);}}
 void Highlight(int value){if(highlighted==value)return;highlighted=value;for(int n=0;n<keys.Count;n++){keys[n].transform.localScale=Vector3.one*(n==value?1.12f:1);keys[n].image.color=n==value?new Color(.55f,1,.35f):Color.white;}}
 public bool Choose(int digit){if(!input||popup!=null||sequence==null||digit<0||digit>9)return false;if(sequence[position]!=digit){errors++;if(!challenge){hearts--;board.Hearts(hearts);if(hearts==0)Finish(false);}return false;}position++;if(position==sequence.Length){input=false;if(challenge){round++;StartLevel(1);}else Finish(true);}return true;}
 void Finish(bool won){if(board==null)return;input=watching=false;Highlight(-1);if(!challenge&&won){if(level<=20){PlayerPrefs.SetInt(Key,PlayerPrefs.GetInt(Key,0)|(1<<(level-1)));int stars=hearts==5?3:hearts>=3?2:1;PlayerPrefs.SetInt(Key+".stars."+level,Mathf.Max(stars,PlayerPrefs.GetInt(Key+".stars."+level,0)));}else PlayerPrefs.SetInt(Key+".athlete",1);PlayerPrefs.Save();}
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
 public void Close(){ClearBoard();Dismiss();if(overlay!=null)CardFaces.DestroyGenerated(overlay);overlay=levels=null;if(theme!=null)CardFaces.DestroyGenerated(theme);theme=null;CardFaces.EndSession();}
 void OnDisable(){if(overlay!=null)Close();}
}
