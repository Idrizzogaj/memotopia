using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Native Focus training with account-scoped device records and standard achievement sync.
public sealed class FocusGame : MonoBehaviour {
 public static readonly Color Teal=new Color32(0,112,122,255),Green=new Color32(22,131,84,255),Red=new Color32(206,56,88,255);
 readonly System.Random random=new System.Random();readonly List<string> earned=new List<string>();
 GameObject overlay,body,popup;Text clock,levelText,scoreText,message,rule,answerText,questionText,roundText;Button start,minus,plus,help;
 int owner,runOwner,flashPart;string runId;int kind,level,startLevel=1,score,correct,errors,streak,cursor,questionIndex,priorFrameRate;float remaining=90,last,messageUntil;bool playing,transition,flashWatching;float flashRemaining;string answer="";
 FocusRules.Sum sum;int[] sequence;readonly List<Arena> arenas=new List<Arena>();
 sealed class NativeBall {public FocusRules.Ball State;public Image Image,Halo;}
 sealed class Arena {public int Target;public RectTransform Box;public List<NativeBall> Balls=new List<NativeBall>();public Image[] Walls=new Image[4];public Text Count;}
 public static FocusGame Open(RectTransform parent,int kind){var game=parent.GetComponent<FocusGame>()??parent.gameObject.AddComponent<FocusGame>();game.Show(kind);return game;}
 Text Label(Transform parent,string value,Vector2 min,Vector2 max,int size,Color color){var text=ChainGame.Label(parent,value,min,max,size,color);text.fontStyle=FontStyle.Bold;text.raycastTarget=false;return text;}
 Button Button(Transform parent,string caption,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction click){var button=SprintUi.Button(parent,caption,min,max,click);button.image.color=new Color32(254,191,75,255);return button;}
 void Show(int index){
  if(SprintRuntime.Active)return;Close();owner=GameManager.AchievementUserId;priorFrameRate=Application.targetFrameRate;Application.targetFrameRate=60;kind=index;startLevel=level=1;flashPart=0;score=correct=errors=streak=0;remaining=90;
  overlay=ChainGame.Panel(transform,"Native Focus "+FocusRules.Names[kind],Vector2.zero,Vector2.one,Teal);
  // Reuse the actual Flash header frame, not a lookalike drawn for Focus.
  var template=Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall");var source=template.GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Exact recall header");
  var header=UnityEngine.Object.Instantiate(source,overlay.transform,false);header.gameObject.SetActive(true);header.name="Original Focus header";header.anchorMin=new Vector2(.13f,.82f);header.anchorMax=new Vector2(.94f,.96f);header.offsetMin=header.offsetMax=Vector2.zero;header.localScale=Vector3.one;
  var frame=header.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.name=="header")??header.GetComponentInChildren<Image>(true);
  foreach(var graphic in header.GetComponentsInChildren<Graphic>(true))graphic.enabled=graphic==frame;
  if(frame.transform!=header){frame.rectTransform.SetParent(header,false);frame.rectTransform.anchorMin=Vector2.zero;frame.rectTransform.anchorMax=Vector2.one;frame.rectTransform.offsetMin=frame.rectTransform.offsetMax=Vector2.zero;}
  frame.preserveAspect=false;frame.material=null;frame.color=Color.white;
  Label(header,FocusRules.Names[kind].ToUpperInvariant(),new Vector2(.05f,.69f),new Vector2(.95f,.88f),42,Teal);
  Label(header,"Timer",new Vector2(.02f,.42f),new Vector2(.32f,.59f),23,Color.gray);clock=Label(header,"90",new Vector2(.02f,.12f),new Vector2(.32f,.44f),44,Teal);
  Label(header,"Level",new Vector2(.35f,.42f),new Vector2(.65f,.59f),23,Color.gray);levelText=Label(header,"1",new Vector2(.35f,.12f),new Vector2(.65f,.44f),44,Teal);
  Label(header,"Score",new Vector2(.68f,.42f),new Vector2(.98f,.59f),23,Color.gray);scoreText=Label(header,"0",new Vector2(.68f,.12f),new Vector2(.98f,.44f),44,Teal);
  var back=SprintUi.Back(overlay.transform,Quit);foreach(var image in back.GetComponentsInChildren<Image>(true))image.color=Color.white;
  rule=Label(overlay.transform,"",new Vector2(.04f,.76f),new Vector2(.96f,.82f),24,Color.white);
  minus=Button(overlay.transform,"−",new Vector2(.19f,.70f),new Vector2(.34f,.75f),()=>SelectLevel(-1));plus=Button(overlay.transform,"+",new Vector2(.66f,.70f),new Vector2(.81f,.75f),()=>SelectLevel(1));
  roundText=Label(overlay.transform,"",new Vector2(.35f,.70f),new Vector2(.65f,.75f),25,Color.white);
  body=ChainGame.Panel(overlay.transform,"Focus board",new Vector2(.06f,.20f),new Vector2(.94f,.68f),Color.white);SprintUi.Edges(body.GetComponent<Image>());
  message=Label(overlay.transform,"",new Vector2(.04f,.135f),new Vector2(.96f,.19f),27,Color.white);
  start=Button(overlay.transform,"START",new Vector2(.27f,.045f),new Vector2(.73f,.115f),StartWithGuide);help=Button(overlay.transform,"Help",new Vector2(.78f,.045f),new Vector2(.96f,.115f),()=>FocusHowTo.Open(overlay.transform,owner,kind,null));Prepare();
 }
 void SelectLevel(int delta){if(playing)return;startLevel=level=Mathf.Clamp(startLevel+delta,1,FocusRules.Maximum(kind));flashPart=0;Prepare();}
 public void StartWithGuide(){if(overlay==null||playing)return;if(FocusHowTo.Needed(owner,kind))FocusHowTo.Open(overlay.transform,owner,kind,Begin);else Begin();}
 public void Begin(){if(overlay==null)return;SaveRun(false);flashPart=0;runOwner=owner;runId=FocusStats.Begin(runOwner,kind,startLevel);StopAllCoroutines();Remove(popup);popup=null;level=startLevel;score=correct=errors=streak=0;remaining=90;playing=true;transition=false;last=Time.realtimeSinceStartup;start.gameObject.SetActive(false);help.gameObject.SetActive(false);minus.interactable=plus.interactable=false;Earn("focus-first");Earn(FocusAchievements.First[kind]);Prepare();Feedback("GO!");}
 void Update(){if(overlay!=null && owner!=GameManager.AchievementUserId){Close();return;}float now=Time.realtimeSinceStartup;if(playing && popup==null)Tick(Mathf.Max(0,now-last));last=now;if(message!=null && now>messageUntil)message.text="";}
 public void Tick(float elapsed){
  if(!playing)return;remaining=Mathf.Max(0,remaining-elapsed);if(remaining<=0){Finish();return;}
  if(kind==2 && flashWatching){flashRemaining=Mathf.Max(0,flashRemaining-elapsed);if(flashRemaining<=0)HideFlashNumbers();}
  if(kind==1 && !transition)foreach(var arena in arenas)foreach(var ball in arena.Balls){
   bool armed=ball.State.Armed;int wall=ball.State.Step(Mathf.Min(elapsed,.06f));
   if(wall>=0){if(!armed && ball.State.Hits<arena.Target){Award(false);Flash(arena.Walls[wall],Red);}ball.State.Armed=false;}
   PlaceBall(ball,arena.Target);
  }
  Hud();
 }
 void Hud(){if(clock==null)return;clock.text=Mathf.CeilToInt(remaining).ToString();clock.color=remaining<=10?Red:Teal;levelText.text=level.ToString();scoreText.text=score.ToString();roundText.text="Level "+(playing?level:startLevel)+(kind==2?" · "+(flashPart+1)+"/3":"");}
 void Feedback(string value){if(message!=null)message.text=value;messageUntil=Time.realtimeSinceStartup+1.2f;}
 void Award(bool right){if(right){score++;correct++;streak=0;Feedback("+1");}else{errors++;streak++;if(kind==1)score--;Feedback(kind==1?"−1":"Try again");}Hud();}
 void Prepare(){
  transition=false;flashWatching=false;arenas.Clear();for(int i=body.transform.childCount-1;i>=0;i--)Remove(body.transform.GetChild(i).gameObject);Hud();
  if(kind==0){MathBoard();return;}if(kind==1){BoxesBoard();return;}NumbersBoard();
 }
 void NextLevel(){if(transition)return;transition=true;Feedback(kind==2 && flashPart<2?"Round complete!":"Level complete!");StartCoroutine(Advance());}
 IEnumerator Advance(){yield return new WaitForSecondsRealtime(.4f);if(!playing)yield break;if(kind==2 && flashPart<2){flashPart++;Prepare();yield break;}flashPart=0;int before=level;level=Mathf.Min(level+1,FocusRules.Maximum(kind));Earn(FocusAchievements.Reached(kind,startLevel,before,level));Prepare();}
 void MathBoard(){
  questionIndex=0;questionText=Label(body.transform,"",new Vector2(.03f,.76f),new Vector2(.97f,.96f),68,ChainGame.Ink);answerText=Label(body.transform,"?",new Vector2(.12f,.62f),new Vector2(.88f,.78f),52,Teal);
  var keys=new[]{"1","2","3","4","5","6","7","8","9","Clear","0","OK"};for(int i=0;i<keys.Length;i++){string key=keys[i];int col=i%3,row=i/3;var button=Button(body.transform,key,new Vector2(.04f+col*.31f,.04f+(3-row)*.14f),new Vector2(.32f+col*.31f,.165f+(3-row)*.14f),()=>MathKey(key));if(key!="OK")button.image.color=new Color32(237,245,245,255);}NewQuestion();
 }
 void NewQuestion(){sum=FocusRules.Question(level,random);answer="";questionText.text=sum.A+" "+sum.Operator+" "+sum.B;answerText.text="?";rule.text="Up to "+sum.Limit+"  ·  "+(questionIndex+1)+" / "+sum.Count;}
 public void MathKey(string key){if(!playing || transition)return;
  if(key=="Clear")answer="";
  else if(key=="OK"){int parsed;if(!int.TryParse(answer,out parsed))return;if(parsed==sum.Answer){Award(true);questionIndex++;if(questionIndex>=sum.Count){NextLevel();return;}NewQuestion();}else{Award(false);answer="";}}
  else if(answer.Length<4)answer+=key;
  answerText.text=answer.Length==0?"?":answer;
 }
 static Sprite circle;
 public static Sprite Circle {get{
  if(circle!=null)return circle;const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.hideFlags=HideFlags.HideAndDontSave;texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
  var pixels=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++){float distance=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32));pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(32-distance));}texture.SetPixels(pixels);texture.Apply();
  circle=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);circle.hideFlags=HideFlags.HideAndDontSave;return circle;
 }}
 public static Button NumberToken(Transform parent,int key,Vector2 centre,float diameter,bool circle){
  Color colour=key>1000?Red:Green;var go=ChainGame.Panel(parent,"Token "+key,centre,centre,colour);var rect=(RectTransform)go.transform;rect.sizeDelta=new Vector2(diameter,diameter);var image=go.GetComponent<Image>();image.sprite=circle?Circle:ContentSelectorBar.Rounded();image.type=circle?Image.Type.Simple:Image.Type.Sliced;image.pixelsPerUnitMultiplier=4;
  var inner=ChainGame.Panel(go.transform,"White centre",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();inner.rectTransform.offsetMin=new Vector2(6,6);inner.rectTransform.offsetMax=new Vector2(-6,-6);inner.sprite=image.sprite;inner.type=image.type;inner.pixelsPerUnitMultiplier=4;inner.raycastTarget=false;
  var text=ChainGame.Label(go.transform,(key%1000).ToString(),Vector2.zero,Vector2.one,Mathf.RoundToInt(diameter*.42f),colour);text.fontStyle=FontStyle.Bold;text.raycastTarget=false;var button=go.AddComponent<Button>();button.transition=Selectable.Transition.None;return button;
 }
 Button Token(int key,Vector2 centre,float diameter,bool circle){var button=NumberToken(body.transform,key,centre,diameter,circle);var text=button.GetComponentInChildren<Text>();button.onClick.AddListener(()=>Choose(key,button,text));return button;}
 void NumbersBoard(){
  sequence=FocusRules.Sequence(kind,level,flashPart);cursor=0;int mode=FocusRules.NumberMode(kind,level,flashPart),n=FocusRules.NumberCount(kind,level);rule.text=mode==0?"Green ↑":mode==1?"Red ↓":mode==2?"Green ↑ first. Then red ↓":"Alternate: Green ↑, Red ↓";Canvas.ForceUpdateCanvases();var size=((RectTransform)body.transform).rect.size;
  if(kind==2){
   var keys=sequence.OrderBy(_=>random.Next()).ToArray();var positions=new List<Vector3>();
   float baseSize=Mathf.Min(105,Mathf.Sqrt(size.x*size.y/keys.Length)*.52f);
   foreach(int key in keys){
    float diameter=baseSize*(.76f+(float)random.NextDouble()*.45f);Vector2 point=Vector2.zero;bool placed=false;
    for(int attempt=0;attempt<2500;attempt++){
     point=new Vector2(diameter*.6f+(float)random.NextDouble()*(size.x-diameter*1.2f),diameter*.6f+(float)random.NextDouble()*(size.y-diameter*1.2f));
     if(positions.All(p=>Vector2.Distance(point,new Vector2(p.x,p.y))>(diameter+p.z)*.5f+10)){placed=true;break;}
    }
    if(!placed){diameter*=.75f;for(int attempt=0;attempt<2500;attempt++){point=new Vector2(diameter*.6f+(float)random.NextDouble()*(size.x-diameter*1.2f),diameter*.6f+(float)random.NextDouble()*(size.y-diameter*1.2f));if(positions.All(p=>Vector2.Distance(point,new Vector2(p.x,p.y))>(diameter+p.z)*.5f+6)){placed=true;break;}}}
    if(!placed)throw new InvalidOperationException("Could not place non-overlapping Focus circles");
    positions.Add(new Vector3(point.x,point.y,diameter));Token(key,new Vector2(point.x/size.x,point.y/size.y),diameter,true);
   }
   flashWatching=true;flashRemaining=FocusRules.FlashSeconds(keys.Length);rule.text="Memorise · "+flashRemaining+"s";

  }else{
   var colours=mode==0?new[]{false}:mode==1?new[]{true}:new[]{false,true};int columns=n==15?5:6,rows=Mathf.CeilToInt(n/(float)columns);float band=colours.Length==1?.7f:.38f,diameter=Mathf.Min(size.x*.84f/columns,size.y*band/rows)*.86f;
   for(int group=0;group<colours.Length;group++){
    float top=colours.Length==1?.8f:group==0?.84f:.39f;Label(body.transform,colours[group]?"RED ↓":"GREEN ↑",new Vector2(.04f,top+.02f),new Vector2(.96f,top+.10f),24,colours[group]?Red:Green);
    var keys=Enumerable.Range(1,n).OrderBy(_=>random.Next()).ToArray();for(int i=0;i<n;i++){float x=.09f+(i%columns)*.82f/(columns-1),y=top-(i/columns+.5f)*band/rows;Token(keys[i]+(colours[group]?1000:0),new Vector2(x,y),diameter,false);}
   }
  }
 }
 void HideFlashNumbers(){flashWatching=false;foreach(var button in body.GetComponentsInChildren<Button>())if(button.name.StartsWith("Token ")){var text=button.GetComponentInChildren<Text>();if(text!=null)text.text="";}rule.text="Recall · "+(FocusRules.NumberMode(kind,level,flashPart)==0?"Green ↑":FocusRules.NumberMode(kind,level,flashPart)==1?"Red ↓":"Alternate Green ↑, Red ↓");}
 public void Choose(int key,Button button,Text text){if(!playing || transition || flashWatching || !button.interactable)return;
  if(sequence[cursor]==key){text.text="";button.interactable=false;var color=button.image.color;color.a=.25f;button.image.color=color;cursor++;Award(true);if(cursor==sequence.Length)NextLevel();}
  else{Award(false);if(kind==2 && streak>=5){level=Mathf.Max(1,level-1);flashPart=0;streak=0;Prepare();Feedback("Five errors · Level "+level);}}
 }
 void BoxesBoard(){
  var spec=FocusRules.Boxes(level);rule.text="Tap the next wall before impact  ·  +1 / −1";Canvas.ForceUpdateCanvases();var size=((RectTransform)body.transform).rect.size;
  float side=spec[0]==1?Mathf.Min(size.x*.86f,size.y*.80f):Mathf.Min(size.x*.42f,size.y*(spec[0]==3?.29f:.68f));
  var catcher=body.GetComponent<FocusWallTarget>()??body.AddComponent<FocusWallTarget>();catcher.Tap=TapBoxArea;
  for(int a=0;a<spec[0];a++){
   var arena=new Arena{Target=spec[2]};arenas.Add(arena);Vector2 centre=spec[0]==1?new Vector2(.5f,.52f):a==2?new Vector2(.5f,.19f):new Vector2(a==0?.25f:.75f,spec[0]==3?.70f:.52f);
   var box=ChainGame.Panel(body.transform,"Box "+(a+1),centre,centre,new Color32(249,252,252,255));((RectTransform)box.transform).sizeDelta=new Vector2(side,side);arena.Box=(RectTransform)box.transform;
   for(int s=0;s<4;s++){
    int wall=s;Vector2 min=s==0?new Vector2(0,.95f):s==1?new Vector2(.95f,0):Vector2.zero,max=s==0?Vector2.one:s==1?Vector2.one:s==2?new Vector2(1,.05f):new Vector2(.05f,1);
    var visible=ChainGame.Panel(box.transform,"Wall "+s,min,max,new Color32(192,215,217,255)).GetComponent<Image>();visible.raycastTarget=false;arena.Walls[s]=visible;
    float hit=Mathf.Clamp(120f/side,.22f,.45f);min=s==0?new Vector2(0,1-hit):s==1?new Vector2(1-hit,0):Vector2.zero;max=s==0||s==1?Vector2.one:s==2?new Vector2(1,hit):new Vector2(hit,1);
    var target=ChainGame.Panel(box.transform,"Tap wall "+s,min,max,Color.clear);var hitTarget=target.AddComponent<FocusWallTarget>();hitTarget.Tap=TapBoxArea;
   }
   for(int b=0;b<spec[1];b++){
    float angle=(25+(float)random.NextDouble()*40)*Mathf.Deg2Rad;var state=new FocusRules.Ball{Random=random,Position=new Vector2(.22f+b*.21f,.35f+b*.14f),Velocity=new Vector2(Mathf.Cos(angle)*(b%2==0?1:-1),Mathf.Sin(angle)*(b%3==0?1:-1))*.25f};
    var image=ChainGame.Panel(box.transform,"Ball "+b,state.Position,state.Position,ChainGame.Ink).GetComponent<Image>();image.sprite=Circle;image.raycastTarget=false;image.rectTransform.sizeDelta=new Vector2(28,28);var halo=ChainGame.Panel(image.transform,"Saved halo",Vector2.zero,Vector2.one,new Color(Green.r,Green.g,Green.b,.24f)).GetComponent<Image>();halo.sprite=Circle;halo.raycastTarget=false;halo.rectTransform.offsetMin=new Vector2(-9,-9);halo.rectTransform.offsetMax=new Vector2(9,9);var ball=new NativeBall{State=state,Image=image,Halo=halo};arena.Balls.Add(ball);PlaceBall(ball,arena.Target);
   }
   arena.Count=Label(box.transform,"",new Vector2(0,-.13f),new Vector2(1,-.02f),20,ChainGame.Ink);Counts(arena);
  }
 }
 public void TapBoxArea(Vector2 screenPoint){
  if(!playing || transition || arenas.Count==0)return;var canvas=body.GetComponentInParent<Canvas>().rootCanvas;Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;Vector2 local;var rect=(RectTransform)body.transform;RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,screenPoint,camera,out local);Vector2 uv=new Vector2((local.x-rect.rect.xMin)/rect.rect.width,(local.y-rect.rect.yMin)/rect.rect.height);int zone=FocusRules.BoxZone(uv,arenas.Count);var arena=arenas[zone];RectTransformUtility.ScreenPointToLocalPointInRectangle(arena.Box,screenPoint,camera,out local);Wall(arena,FocusRules.WallAt(local-arena.Box.rect.center));
 }
 void PlaceBall(NativeBall ball,int target){ball.Image.rectTransform.anchorMin=ball.Image.rectTransform.anchorMax=ball.State.Position;ball.Image.rectTransform.anchoredPosition=Vector2.zero;ball.Image.color=ball.State.Armed?Green:ball.State.Hits>=target?Color.gray:Red;if(ball.Halo!=null)ball.Halo.gameObject.SetActive(ball.State.Armed);}
 void Counts(Arena arena){arena.Count.text=string.Join("  ",arena.Balls.Select(b=>b.State.Hits+"/"+arena.Target).ToArray());}
 void Wall(Arena arena,int wall){if(!playing || transition)return;var ball=arena.Balls.Where(b=>b.State.Hits<arena.Target && !b.State.Armed && b.State.NextWall()==wall).OrderBy(b=>b.State.Arrival()).FirstOrDefault();if(ball==null)return;ball.State.Armed=true;ball.State.Hits++;Award(true);Flash(arena.Walls[wall],Green);PlaceBall(ball,arena.Target);Counts(arena);if(arenas.All(a=>a.Balls.All(b=>b.State.Hits>=a.Target)))NextLevel();}
 void Flash(Image wall,Color colour){StartCoroutine(Blink(wall,colour));}
 IEnumerator Blink(Image wall,Color colour){wall.color=colour;yield return new WaitForSecondsRealtime(.25f);if(wall!=null)wall.color=new Color32(192,215,217,255);}
 void Dialog(string first,string second,string left,string right,UnityEngine.Events.UnityAction no,UnityEngine.Events.UnityAction yes){
  Remove(popup);popup=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/MidGameQuitPanel"),overlay.transform,false);var content=popup.transform.GetChild(0);
  // Preserve the original frame and two buttons; replace only their labels and message.
  var prompt=content.GetComponentsInChildren<Text>(true).Where(t=>!t.transform.IsChildOf(content.GetChild(2)) && !t.transform.IsChildOf(content.GetChild(3))).ToArray();if(prompt.Length>0)prompt[0].text=prompt.Length==1?first+"\n"+second:first;if(prompt.Length>1)prompt[1].text=second;
  var a=content.GetChild(2).GetComponent<Button>();var b=content.GetChild(3).GetComponent<Button>();a.onClick=new Button.ButtonClickedEvent();b.onClick=new Button.ButtonClickedEvent();a.GetComponentInChildren<Text>().text=left;b.GetComponentInChildren<Text>().text=right;a.onClick.AddListener(no);b.onClick.AddListener(yes);
 }
 void Earn(string key){if(!string.IsNullOrEmpty(key) && GameManager.AchievementUserId>0 && GameManager.UnlockAchievement(key))earned.Add(key);}
 void SaveRun(bool complete){if(string.IsNullOrEmpty(runId))return;FocusStats.End(runOwner,runId,level,score,correct,errors,90-remaining,complete);runId=null;}
 void Finish(){SaveRun(true);playing=false;transition=false;StopAllCoroutines();Hud();Dialog("BRAINS OUT!","Score "+score+"\n"+correct+" correct · "+errors+" errors","Home","Replay",Close,Begin);if(Application.isPlaying){foreach(string key in earned)BadgePopup.ShowFor(this,key);earned.Clear();if(!string.IsNullOrEmpty(PlayerPrefs.GetString("token")))ProgressSync.Instance.GetApi<Assets.Script.Controllers.AchievementsAPIController>().FlushPending();}}
 void Quit(){if(!playing){Close();return;}if(popup!=null)return;Dialog("Are you sure you","want to quit?","No","Yes",()=>{Remove(popup);popup=null;last=Time.realtimeSinceStartup;},Close);}
 public void Close(){SaveRun(false);bool wasOpen=overlay!=null;playing=false;StopAllCoroutines();Remove(overlay);overlay=body=popup=null;arenas.Clear();if(wasOpen)Application.targetFrameRate=priorFrameRate;}
 static void Remove(GameObject value){if(value!=null){value.SetActive(false);CardFaces.DestroyGenerated(value);}}
 void OnDisable(){Close();}
}
