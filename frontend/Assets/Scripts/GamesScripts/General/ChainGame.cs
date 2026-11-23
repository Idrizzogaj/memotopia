using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Same identity/position/hearts rules as Minnessagor chain-engine.js. No server game IDs invented.
public sealed class ChainState
{
    public readonly int[] Sequence;
    public int Position {get;private set;}
    public int Hearts {get;private set;}
    public readonly int MaxHearts;
    private readonly HashSet<int> solved = new HashSet<int>();
    public bool Won { get {return Position==Sequence.Length;} }
    public bool TimedOut {get;private set;}
    public bool Finished { get {return Won || Hearts==0 || TimedOut;} }
    public void Expire(){if(!Finished)TimedOut=true;}
    public ChainState(int[] sequence,int hearts=5) {
        if(hearts<1 || hearts>5)throw new ArgumentOutOfRangeException("hearts");
        Hearts=MaxHearts=hearts;
        if(sequence==null || sequence.Length==0 || sequence.Distinct().Count()!=sequence.Length)throw new ArgumentException("Chain needs unique cards.");
        Sequence=(int[])sequence.Clone();
    }
    public bool Choose(int id) {
        if(Finished || Array.IndexOf(Sequence,id,0,Position)>=0)return false;
        if(Sequence[Position]==id){Position++;return true;}
        Hearts--;return false;
    }
    public bool ChoosePair(int first,int second){
        if(Finished || first==second || solved.Contains(first) || solved.Contains(second))return false;
        int index=Array.IndexOf(Sequence,first);
        if(index>=0 && Sequence[index%2==0?index+1:index-1]==second){solved.Add(first);solved.Add(second);Position+=2;return true;}
        Hearts--;return false;
    }
    public bool Place(int id,int slot){
        if(Finished || slot<0 || slot>=Sequence.Length || solved.Contains(Sequence[slot]))return false;
        if(Sequence[slot]==id){solved.Add(id);Position++;return true;}
        Hearts--;return false;
    }
    public static int CountForLevel(int level) {
        if(level<1 || level>20)throw new ArgumentOutOfRangeException("level");
        return 3+Mathf.RoundToInt((level-1)*49f/19f);
    }
}

public static class ChainProgress
{
    private static string Key {get {return "chain.completed.v1."+GameManager.AchievementUserId;}}
    public static int Mask {get {return PlayerPrefs.GetInt(Key,0)&((1<<20)-1);}}
    public static bool Completed(int level) {return (Mask & (1<<(level-1)))!=0;}
    public static bool Unlocked(int level) {return level>=1 && level<=20 && (level==1 || level==20 || Completed(20) || Completed(level) || Completed(level-1));}
    public static bool All {get {return Mask==((1<<20)-1);}}
    public static int Stars(int level) {return PlayerPrefs.GetInt("chain.stars.v1."+GameManager.AchievementUserId+"."+level,0);}
    public static void SaveStars(int level,int hearts) {
        int earned=hearts==5?3:hearts>=3?2:1;
        PlayerPrefs.SetInt("chain.stars.v1."+GameManager.AchievementUserId+"."+level,Mathf.Max(Stars(level),earned));PlayerPrefs.Save();
    }
    public static void Complete(int level) {
        if(level<1 || level>20)throw new ArgumentOutOfRangeException("level");
        PlayerPrefs.SetInt(Key,Mask | (1<<(level-1)));PlayerPrefs.Save();
    }
}

// Account scoped deck-only presentation. Independent of generic Words/Random unlocks.
public sealed class DeckModeBar : MonoBehaviour
{
    private readonly List<Button> buttons=new List<Button>();
    private GameObject row;
    public static void Attach(RectTransform parent) { } // One shared icon row controls deck presentation.
    private void Build() {
        row=gameObject;
        string[] labels={"Cards","Words","?","Random"};
        for(int i=0;i<4;i++) {
            int mode=i;
            var b=ChainGame.Button(transform,labels[i],new Vector2(i*.25f+.008f,0),new Vector2((i+1)*.25f-.008f,1),()=>MemorySettings.DeckMode=(DeckDisplay)mode);
            buttons.Add(b);
        }
    }
    private void Update() {
        bool deck=(CardFaces.Selected & CardFaces.Choice.Deck)!=0;
        for(int i=0;i<buttons.Count;i++) {
            buttons[i].gameObject.SetActive(deck);
            buttons[i].image.color=(int)MemorySettings.DeckMode==i ? ChainGame.Purple : new Color(.87f,.84f,.96f);
            buttons[i].GetComponentInChildren<Text>().color=(int)MemorySettings.DeckMode==i ? Color.white : ChainGame.Ink;
        }
    }
}

// Uses the existing GamesScript shuffle, CardFaces presenter and BadgePopup catalog.
// Study progression is factored with Pairs through StudySteps rather than invoking Pairs' two-card matching rules.
public static class StudySteps
{
    public static bool Take(ref int position,int count,int step,out int index) {
        index=position;
        if(position>=count)return false;
        position+=step;return true;
    }
}

public sealed class ChainGame : GamesScript
{
    public static readonly Color Theme=new Color32(0,150,160,255); // Chain has its own colour, Says keeps purple
    public static readonly Color Purple=new Color32(123,31,255,255), Ink=new Color32(45,30,75,255);
    private RectTransform overlay,body,safe;
    private GameObject exactLevels;
    private ChainOriginalPresentation originalGame;private GameObject quitDialog,countdownPanel,resultPanel;
    private Material exactTheme;
    private Text title,progress,hearts,feedback;
    private ChainState state;
    private int level,studyIndex,owner;
    private bool recall;
    private float recallSeconds;
    private Button next;
    private Image studyCard;
    private readonly List<Button> tiles=new List<Button>();
    private SagaLevel saga;
    private SagaFlashStudy flashStudy;
    private SagaPairsBoard pairsBoard;
    private SagaBoxesBoard boxesBoard;
    private SagaSortBoard sortBoard;
    private SagaRoundContext sagaContext;
    private float recallBudget;
    private Action<SagaLevel> sagaNext;
    private Action sagaWon;
    private Color RoundTheme { get { return saga==null?Theme:SagaRoute.ColorFor(saga.Game); } }
    private Action sagaClosed;
    private float studySeconds;
    private int sagaShuffles;
    public bool IsSagaOpen { get { return saga != null && overlay != null; } }
    public static void Attach(RectTransform screen) {
        if(screen.Find("Chain")!=null)return;
        var root=Panel(screen,"Chain",new Vector2(.08f,.1f),new Vector2(.92f,.27f),Theme);
        // Match existing stacked training tile geometry; ContentSelectorBar fits all four together.
        var prototype=screen.Find("Flash") as RectTransform;
        if(prototype!=null) {
            var r=(RectTransform)root.transform;
            r.anchorMin=prototype.anchorMin;r.anchorMax=prototype.anchorMax;r.pivot=prototype.pivot;
            r.sizeDelta=prototype.sizeDelta;r.anchoredPosition=prototype.anchoredPosition-new Vector2(0,prototype.rect.height+20);
        }
        root.GetComponent<Image>().sprite=ContentSelectorBar.Rounded();root.GetComponent<Image>().type=Image.Type.Sliced;
        var label=Label(root.transform,"CHAIN",new Vector2(.08f,.02f),new Vector2(.85f,.40f),72,Color.white);
        label.fontStyle=FontStyle.Bold;label.alignment=TextAnchor.MiddleLeft;
        var btn=root.AddComponent<Button>();btn.targetGraphic=root.GetComponent<Image>();
        var game=root.AddComponent<ChainGame>();
        btn.onClick.AddListener(game.Open);
    }
    public void Open() {
        if(overlay!=null)return;
        CreateOverlay();
        ShowLevels();
    }
    public void OpenSaga(int value, Action closed) { OpenSaga(SagaGame.Chain,value,closed); }
    public void OpenSaga(SagaGame game,int value, Action closed,Action<SagaLevel> nextStage=null,Action won=null,SagaRoundContext context=null) {
        int stop=SagaRoute.Stop(game,value);
        if(overlay!=null || SprintRuntime.Active || !SagaRoute.Unlocked(stop))return;
        if(game==SagaGame.Says || Array.IndexOf(SagaRoute.Games,game)<0)throw new InvalidOperationException("This Saga game uses its own controller.");
        saga=new SagaLevel(game,value);sagaClosed=closed;sagaNext=nextStage;sagaWon=won;sagaContext=context;
        CreateOverlay();
        StartLevel(value);
    }
    private void CreateOverlay() {
        owner=GameManager.AchievementUserId;
        var canvas=GetComponentInParent<Canvas>().rootCanvas;
        overlay=(RectTransform)Panel(canvas.transform,"Chain overlay",Vector2.zero,Vector2.one,RoundTheme).transform;
        if(sagaContext!=null)overlay.anchorMax=new Vector2(1,.87f);
        safe=(RectTransform)Panel(overlay,"Safe area",Vector2.zero,Vector2.one,Color.clear).transform;
        ApplySafeArea();
        title=Label(safe,"",Vector2.zero,Vector2.zero,14,Purple);title.gameObject.SetActive(false);
        body=(RectTransform)Panel(safe,"Content",new Vector2(.03f,.03f),new Vector2(.97f,.84f),Color.clear).transform;
    }
    private void ApplySafeArea() {
        var area=Screen.safeArea;
        if(Screen.width<=0 || Screen.height<=0)return;
        safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);
        safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
    }
    private void Update() {
        if(overlay!=null && GameManager.AchievementUserId!=owner){Close();return;}
        if(sagaContext!=null && state!=null && !state.Finished && sagaContext.Remaining<=0){state.Expire();EndRound();return;}
        if(overlay!=null && Input.GetKeyDown(KeyCode.Escape))Back();
        if(quitDialog==null){if(pairsBoard!=null)pairsBoard.Tick(Time.deltaTime);if(boxesBoard!=null)boxesBoard.Tick();if(sortBoard!=null)sortBoard.Tick();}
        if(TickSagaStudyClock(Time.deltaTime))return;
        if(saga==null && PracticeMode.Untimed&&originalGame!=null)originalGame.ShowPracticeClock(PracticeSession.Clock(this));
        TickTrainingClock(Time.deltaTime);
    }
    private bool TickSagaStudyClock(float elapsed){
        if(saga==null || saga.Game==SagaGame.Flash || recall || (originalGame==null && boxesBoard==null && sortBoard==null) || state==null || state.Finished || quitDialog!=null)return false;
        studySeconds=Mathf.Max(0,studySeconds-elapsed);
        int shown=Mathf.CeilToInt(sagaContext!=null?sagaContext.Remaining:studySeconds);
        if(boxesBoard!=null)boxesBoard.ShowClock(shown);else if(sortBoard!=null)sortBoard.ShowClock(shown);else originalGame.ShowStudyClock(shown);
        if(studySeconds>0)return false;
        if(saga.SharedStudyRecallClock){state.Expire();EndRound();}else ShowRecall();return true;
    }
    private void TickTrainingClock(float elapsed){
        if(SprintRuntime.Active || !recall || state==null || state.Finished || (originalGame==null && pairsBoard==null && boxesBoard==null && sortBoard==null) || quitDialog!=null)return;
        if(saga==null && PracticeMode.Untimed){originalGame.ShowPracticeClock(PracticeSession.Clock(this));return;}
        recallSeconds=sagaContext!=null?sagaContext.Remaining:Mathf.Max(0,recallSeconds-elapsed);ShowRecallClock();
        if(recallSeconds<=0){state.Expire();EndRound();return;}
        if(saga!=null && saga.TimedShuffleDue(recallBudget-recallSeconds,sagaShuffles,state.Sequence.Length-state.Position)){
            sagaShuffles++;ShuffleRecallPositions();
        }
    }
    private void Back() {
        if(SprintRuntime.Active){SprintRuntime.Current.Quit();return;}
        if(state!=null && !state.Finished) {
            if(quitDialog!=null)return;
            quitDialog=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/MidGameQuitPanel"),overlay,false);
            if(flashStudy!=null)flashStudy.SetPaused(true);
            var content=quitDialog.transform.GetChild(0);
            var no=content.GetChild(2).GetComponent<Button>();var yes=content.GetChild(3).GetComponent<Button>();
            no.onClick=new Button.ButtonClickedEvent();yes.onClick=new Button.ButtonClickedEvent();
            no.onClick.AddListener(()=>{CardFaces.DestroyGenerated(quitDialog);quitDialog=null;if(flashStudy!=null)flashStudy.SetPaused(false);});
            yes.onClick.AddListener(()=>{CardFaces.DestroyGenerated(quitDialog);quitDialog=null;StopAllCoroutines();CardFaces.EndSession();ShowLevels();});
        } else Close();
    }
    private void ClearOriginalGame(){if(sortBoard!=null){sortBoard.Destroy();sortBoard=null;}if(boxesBoard!=null){boxesBoard.Destroy();boxesBoard=null;}if(pairsBoard!=null){pairsBoard.Destroy();pairsBoard=null;}if(flashStudy!=null){flashStudy.Destroy();flashStudy=null;}if(originalGame!=null){originalGame.Destroy();originalGame=null;}if(quitDialog!=null){CardFaces.DestroyGenerated(quitDialog);quitDialog=null;}if(countdownPanel!=null){CardFaces.DestroyGenerated(countdownPanel);countdownPanel=null;}if(resultPanel!=null){CardFaces.DestroyGenerated(resultPanel);resultPanel=null;}}
    public void Close() {
        PracticeSession.Release(this);
        ClearOriginalGame();
        if(exactTheme!=null){CardFaces.DestroyGenerated(exactTheme);exactTheme=null;}
        StopAllCoroutines();CardFaces.EndSession();state=null;
        if(overlay!=null)CardFaces.DestroyGenerated(overlay.gameObject);
        overlay=null;
        var closed=sagaClosed;sagaClosed=null;sagaNext=null;sagaWon=null;sagaContext=null;saga=null;recall=false;
        if(closed!=null)closed();
    }
    private void OnDisable(){if(overlay!=null)Close();}
    private void ClearBody() {
        for(int i=body.childCount-1;i>=0;i--){var c=body.GetChild(i);c.gameObject.SetActive(false);CardFaces.DestroyGenerated(c.gameObject);}
        tiles.Clear();
    }
    public void ShowLevels() {
        if(saga!=null){Close();return;}
        ClearOriginalGame();
        if(!ShowExactFlashLevels())throw new InvalidOperationException("Missing exact Flash level template.");
    }
    private bool ShowExactFlashLevels() {
        var template=Resources.Load<GameObject>("Prefabs/ExactFlashChainLevels");if(template==null)return false;
        ClearBody();state=null;
        if(exactLevels!=null)CardFaces.DestroyGenerated(exactLevels);
        if(exactTheme!=null)CardFaces.DestroyGenerated(exactTheme);
        exactLevels=UnityEngine.Object.Instantiate(template,overlay,false);
        var rect=(RectTransform)exactLevels.transform;rect.localScale=Vector3.one;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.anchoredPosition3D=Vector3.zero;safe.gameObject.SetActive(false);
        var shader=Resources.Load<Shader>("MemotopiaTheme");exactTheme=new Material(shader);exactTheme.SetColor("_ThemeColor",Theme);
        var background=exactLevels.GetComponent<Image>();background.sprite=Resources.Load<Sprite>("GamesResources/Levels/FlashBackground");background.color=Color.white;background.material=exactTheme;
        foreach(var button in exactLevels.GetComponentsInChildren<Button>(true)) {button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(Close);}
        foreach(var image in exactLevels.GetComponentsInChildren<Image>(true)) {
            if(image.name=="Chain title") {
                // Same header art as Pairs/Boxes/Flash but drawn white (not tinted), with the word cut out in the game colour.
                // The white patch only covers the old word in the middle and never catches taps, so the back button stays usable.
                image.sprite=Resources.Load<Sprite>("GamesResources/Levels/FlashTop");image.color=Color.white;
                var whiteTheme=new Material(Resources.Load<Shader>("MemotopiaTheme"));whiteTheme.SetColor("_ThemeColor",Color.white);image.material=whiteTheme;
                var cover=Panel(image.transform,"Chain title text",new Vector2(.23f,.53f),new Vector2(.77f,.83f),Color.white);cover.GetComponent<Image>().raycastTarget=false;
                var caption=Label(cover.transform,"CHAIN",Vector2.zero,Vector2.one,66,Theme);caption.fontStyle=FontStyle.Bold;caption.raycastTarget=false;
                var existing=exactLevels.GetComponentInChildren<Text>(true);if(existing!=null && existing!=caption)caption.font=existing.font;
            }
            if(!image.name.StartsWith("Chain Level "))continue;
            int n=int.Parse(image.name.Substring(12));
            bool unlocked=ChainProgress.Unlocked(n);
            image.sprite=Resources.Load<Sprite>(unlocked?"GamesResources/Levels/AAA-11":"GamesResources/Levels/FlashLevelCell");
            if(!unlocked)image.material=exactTheme;
            var number=image.transform.GetChild(1).GetComponentInChildren<Text>(true);number.text=n.ToString();number.color=Theme;
            var button=image.transform.GetChild(1).GetComponent<Button>();button.interactable=unlocked;button.onClick=new Button.ButtonClickedEvent();if(unlocked)button.onClick.AddListener(()=>StartLevel(n));
            var stars=image.transform.GetChild(0);stars.gameObject.SetActive(ChainProgress.Stars(n)>0);
            for(int i=0;i<stars.childCount;i++) {
                stars.GetChild(i).gameObject.SetActive(i<ChainProgress.Stars(n));
                var star=stars.GetChild(i).GetComponent<Image>();if(star!=null)star.sprite=Resources.Load<Sprite>("GamesResources/Levels/Asset "+(15-i));
            }
        }
        var originals=exactLevels.GetComponentsInChildren<Image>(true);
        var last=Array.Find(originals,i=>i.name=="Chain Level 20");
        var previous=Array.Find(originals,i=>i.name=="Chain Level 16");
        TrainingAthleteTile.Attach(last,previous,Theme,()=>StartLevel(MemoryAthlete.Level),TrainingAthleteTile.Eligible("Chain"));
        return true;
    }

    public void StartLevel(int value) {
        if(saga!=null){if(!SagaRoute.Unlocked(SagaRoute.Stop(saga.Game,value)))return;saga=new SagaLevel(saga.Game,value);}
        else if(!SprintRuntime.Active && value!=MemoryAthlete.Level && !ChainProgress.Unlocked(value))return;
        if(value==MemoryAthlete.Level && !TrainingAthleteTile.Eligible("Chain"))return;
        ClearOriginalGame();safe.gameObject.SetActive(true);
        overlay.GetComponent<Image>().color=RoundTheme;
        if(exactLevels!=null){CardFaces.DestroyGenerated(exactLevels);exactLevels=null;}
        if(exactTheme!=null){CardFaces.DestroyGenerated(exactTheme);exactTheme=null;}
        if(owner!=GameManager.AchievementUserId){Close();return;}
        Assets.Script.Constants.ChallengeConstants.s_isChallenge=SprintRuntime.Active;
        StopAllCoroutines();if(saga!=null || PracticeMode.Untimed)firstEarned=false;level=value;recall=false;studyIndex=0;sagaShuffles=0;
        if(saga!=null)CardFaces.BeginImageSession();else CardFaces.BeginSession();
        int[] pool=Enumerable.Range(1,CardFaces.ContentCount).ToArray();Shuffle(pool);
        state=new ChainState(pool.Take(saga!=null?saga.ImageCount:SprintRuntime.Active?SprintRuntime.Current.Size:ChainState.CountForLevel(level==MemoryAthlete.Level?20:level)).ToArray(),sagaContext!=null?sagaContext.StartingHearts:saga!=null?SagaLevel.Hearts:level==MemoryAthlete.Level?1:5);
        ClearBody();body.GetComponent<Image>().color=new Color32(248,246,255,255);title.text=level==MemoryAthlete.Level?"MEMORY ATHLETE":"CHAIN • "+level;
        if(Application.isPlaying && sagaContext==null)StartCoroutine(Countdown());else ShowStudy();
    }
    private IEnumerator Countdown() {
        if(SprintRuntime.Active){yield return SprintRuntime.ReadyIntro();ShowStudy();yield break;}
        safe.gameObject.SetActive(false);
        countdownPanel=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ReadySteadyGo"),overlay,false);
        // Behind READY/STEADY/GO use the countdown's own colour, so the screen is not split in two colours.
        var overlayImage=overlay.GetComponent<Image>();var countdownImage=countdownPanel.GetComponent<Image>();
        if(overlayImage!=null && countdownImage!=null && countdownImage.sprite==null)overlayImage.color=countdownImage.color;
        yield return StartCoroutine(ShowRSG(countdownPanel));
        if(overlayImage!=null)overlayImage.color=RoundTheme;
        while(quitDialog!=null)yield return null;
        if(saga==null && GameManager.UnlockAchievement("chain-first"))firstEarned=true;
        ShowStudy();
    }
    private bool firstEarned;
    private void ShowStudy() {
        if(saga==null)PracticeSession.Begin(this,"Memory.Chain",level);
        ClearBody();ClearOriginalGame();safe.gameObject.SetActive(false);
        if(exactTheme==null){exactTheme=new Material(Resources.Load<Shader>("MemotopiaTheme"));exactTheme.SetColor("_ThemeColor",RoundTheme);}
        if(saga!=null && saga.Game==SagaGame.Flash){
            flashStudy=new SagaFlashStudy(overlay,state.Sequence,Back,exactTheme,ShowRecall,sagaContext!=null?SagaRoundContext.FlashPlaybackSpeed:1,state.Hearts);return;
        }
        if(saga!=null && saga.Game==SagaGame.Sort){studySeconds=sagaContext!=null?sagaContext.StudyLimit(saga):saga.StudySeconds;sortBoard=new SagaSortBoard(overlay,state.Sequence,Back,exactTheme,ChooseSort,CanRecallInput);sortBoard.ShowClock(Mathf.CeilToInt(studySeconds));sortBoard.Hearts(state.Hearts);return;}
        if(saga!=null && saga.Game==SagaGame.Boxes){
            studySeconds=saga.StudySeconds;boxesBoard=new SagaBoxesBoard(overlay,saga,state.Sequence,Back,ShowRecall,ChooseBox,CanRecallInput);boxesBoard.ShowClock(saga.Seconds);boxesBoard.Hearts(state.Hearts);return;
        }
        originalGame=new ChainOriginalPresentation(overlay,true,Back,NextCard,exactTheme,saga!=null && saga.Game==SagaGame.Pairs);
        studyCard=originalGame.Study;next=originalGame.Next;NextCard();
        if(saga!=null){studySeconds=saga.StudySeconds;originalGame.ShowStudyClock(saga.StudySeconds);originalGame.Hearts(state.Hearts);}
    }
    public void NextCard() {
        if(saga!=null && (saga.Game==SagaGame.Flash || saga.Game==SagaGame.Sort))return;
        if(SprintRuntime.Active && !SprintRuntime.Current.Ready)return;
        if(quitDialog!=null)return;
        int index;
        bool paired=saga!=null && saga.Game==SagaGame.Pairs;
        if(!StudySteps.Take(ref studyIndex,state.Sequence.Length,paired?2:1,out index)){ShowRecall();return;}
        studyCard.sprite=CardFaces.Face(state.Sequence[index]);
        studyCard.preserveAspect=true; // playing cards keep their own proportions instead of filling the square
        CardFaces.SmallFace(state.Sequence[index]); // Prepare the recall face progressively, not all 52 on the last Next.
        if(paired){originalGame.StudySecond.sprite=CardFaces.Face(state.Sequence[index+1]);originalGame.StudySecond.preserveAspect=true;}


    }
    private void ShowRecall() {
        float budget=sagaContext!=null?sagaContext.Remaining:saga!=null && saga.SharedStudyRecallClock?studySeconds:saga!=null?saga.Seconds:180;
        recallBudget=budget;
        if(sortBoard!=null){recall=true;sortBoard.BeginRecall();recallSeconds=budget;ShowRecallClock();UpdateProgress();return;}
        if(boxesBoard!=null){recall=true;boxesBoard.BeginRecall();recallSeconds=budget;ShowRecallClock();UpdateProgress();return;}
        recall=true;ClearBody();ClearOriginalGame();safe.gameObject.SetActive(false);
        if(saga!=null && saga.Game==SagaGame.Pairs){pairsBoard=new SagaPairsBoard(overlay,state.Sequence,Back,exactTheme,ChoosePair);recallSeconds=budget;ShowRecallClock();UpdateProgress();return;}
        originalGame=new ChainOriginalPresentation(overlay,false,Back,NextCard,exactTheme);
        if(!SprintRuntime.Active){recallSeconds=budget;originalGame.ShowRecallClock((int)recallSeconds,saga==null && PracticeMode.Untimed);}
        int[] shuffled=(int[])state.Sequence.Clone();Shuffle(shuffled);
        foreach(int id in shuffled){int captured=id;
            var tile=UnityEngine.Object.Instantiate(originalGame.Cell,originalGame.Grid,false);tile.name="Recall "+id;
            tile.image.overrideSprite=CardFaces.SmallFace(id);tile.image.preserveAspect=true;
            tile.onClick=new Button.ButtonClickedEvent();tile.onClick.AddListener(()=>Choose(captured));tile.gameObject.SetActive(true);tiles.Add(tile);
        }
        originalGame.FitOverflow(shuffled.Length);
        UpdateProgress();
    }
    public void Choose(int id) {
        if(SprintRuntime.Active){
            if(!SprintRuntime.Current.Ready || quitDialog!=null || !recall || state==null || state.Finished)return;
            if(state.Sequence.Take(state.Position).Contains(id))return;
            if(id!=state.Sequence[state.Position]){SprintRuntime.Current.Error();return;}
        }
        if(owner!=GameManager.AchievementUserId){Close();return;}
        if(quitDialog!=null || !recall || state==null || state.Finished)return;
        int oldHearts=state.Hearts;
        bool correct=state.Choose(id);
        if(correct) {var practice=PracticeSession.Get(this);if(practice!=null)practice.Progress(level);
            if(SprintRuntime.Active)SprintRuntime.Current.Solve(id.ToString());
            var tile=tiles.Find(t=>t.name=="Recall "+id);
            if(tile!=null){tile.interactable=false;var hidden=tile.gameObject.AddComponent<CanvasGroup>();hidden.alpha=0;hidden.blocksRaycasts=false;}

        }
        UpdateProgress();
        if(state.Finished)EndRound();
        else if(correct && saga!=null && saga.ShuffleAfterAnswer(state.Sequence.Length-state.Position))ShuffleRecallPositions();
    }
    private void ShuffleRecallPositions() {
        // Move the existing cells, including cleared slots; never change Sequence, faces or callbacks.
        if(originalGame==null || tiles.Count<2)return;
        var current=tiles.OrderBy(t=>t.transform.GetSiblingIndex()).ToArray();
        int[] order=Enumerable.Range(0,current.Length).ToArray();
        int attempts=0;
        do {Shuffle(order);attempts++;} while(order.Where((value,index)=>value==index).Any() && attempts<16);
        if(order.Where((value,index)=>value==index).Any())
            order=Enumerable.Range(1,current.Length-1).Concat(new[]{0}).ToArray();
        for(int i=0;i<order.Length;i++)current[order[i]].transform.SetSiblingIndex(i+1);
        LayoutRebuilder.MarkLayoutForRebuild(originalGame.Grid);
    }
    private bool CanRecallInput(){return recall && quitDialog==null && state!=null && !state.Finished && owner==GameManager.AchievementUserId;}
    private bool ChoosePair(int first,int second){if(!CanRecallInput())return false;bool correct=state.ChoosePair(first,second);UpdateProgress();if(state.Finished)EndRound();return correct;}
    private bool ChooseBox(int id,int slot){if(!CanRecallInput())return false;bool correct=state.Place(id,slot);UpdateProgress();if(state.Finished)EndRound();return correct;}
    private bool ChooseSort(int[] order){
        if(!CanRecallInput() || order.Length!=state.Sequence.Length)return false;
        bool correct=order.SequenceEqual(state.Sequence);
        if(correct){for(int i=0;i<order.Length;i++)state.Place(order[i],i);}
        else{int wrong=0;while(order[wrong]==state.Sequence[wrong])wrong++;state.Place(order[wrong],wrong);}
        UpdateProgress();if(state.Finished)EndRound();return correct;
    }
    private void ShowRecallClock(){int seconds=Mathf.CeilToInt(recallSeconds);if(originalGame!=null)originalGame.ShowRecallClock(seconds,saga==null && PracticeMode.Untimed);if(pairsBoard!=null)pairsBoard.ShowClock(seconds);if(boxesBoard!=null)boxesBoard.ShowClock(seconds);if(sortBoard!=null)sortBoard.ShowClock(seconds);}
    private void UpdateProgress() {if(originalGame!=null)originalGame.Hearts(SprintRuntime.Active?0:state.Hearts);if(pairsBoard!=null)pairsBoard.Hearts(state.Hearts);if(boxesBoard!=null)boxesBoard.Hearts(state.Hearts);if(sortBoard!=null)sortBoard.Hearts(state.Hearts);}
    private void EndRound() {
        var practice=PracticeSession.Get(this);if(practice!=null)practice.Stop();
        if(SprintRuntime.Active)return;
        if(sagaContext!=null){var context=sagaContext;ClearOriginalGame();recall=false;context.Report(state.Won,state.Hearts);return;}
        if(saga!=null){
            if(state.Won)SagaProgress.Complete(saga.Game,owner,level,state.Hearts);
            ClearOriginalGame();safe.gameObject.SetActive(true);recall=false;ClearBody();ShowOriginalResult();if(state.Won && sagaWon!=null)sagaWon();return;
        }
        ClearOriginalGame();safe.gameObject.SetActive(true);
        recall=false;
        bool athleteBadge=false;
        if(state.Won){if(level==MemoryAthlete.Level)athleteBadge=TrainingAthleteTile.Complete("Chain");else{ChainProgress.Complete(level);ChainProgress.SaveStars(level,state.Hearts);}}
        if(athleteBadge && Application.isPlaying)ProgressSync.Instance.GetApi<Assets.Script.Controllers.AchievementsAPIController>().FlushPending();
        bool finalBadge=state.Won && ChainProgress.All && GameManager.UnlockAchievement("chain-all");
        ClearBody();
        ShowOriginalResult();
        if(athleteBadge)BadgePopup.ShowFor(this,TrainingAthleteTile.Badge("Chain"));
        else if(finalBadge)BadgePopup.ShowFor(this,"chain-all");
        else if(firstEarned){firstEarned=false;BadgePopup.ShowFor(this,"chain-first");}
    }
    private void ShowOriginalResult() {
        safe.gameObject.SetActive(false);
        resultPanel=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/"+(state.Won?"WinPanel":"LosePanel")),overlay,false);
        var content=resultPanel.transform.GetChild(0);
        UnityEngine.Events.UnityAction levels=()=>{CardFaces.EndSession();ShowLevels();};
        if(!state.Won) {
            var replay=content.GetChild(2).GetComponent<Button>();var home=content.GetChild(3).GetComponent<Button>();
            replay.onClick=new Button.ButtonClickedEvent();replay.onClick.AddListener(()=>StartLevel(level));
            home.onClick=new Button.ButtonClickedEvent();home.onClick.AddListener(levels);
            return;
        }
        bool athlete=saga==null && level==MemoryAthlete.Level;
        content.GetChild(0).GetChild(0).GetComponent<Text>().text=athlete?"MEMORY ATHLETE":saga!=null?saga.Game.ToString().ToUpperInvariant()+" · LEVEL "+level:"LEVEL "+level;
        var score=content.GetChild(10).GetChild(0).GetComponent<Text>();
        score.text=athlete?TrainingAthleteTile.RewardText("Chain"):(saga!=null && saga.Game==SagaGame.Pairs?state.Position/2:state.Position).ToString();
        if(athlete){score.fontSize=28;score.resizeTextForBestFit=true;score.resizeTextMinSize=14;score.resizeTextMaxSize=28;}
        foreach(var stars in resultPanel.GetComponentsInChildren<StarFillScript>(true)) {
            int earned=athlete?0:saga!=null?state.Hearts:state.Hearts==5?3:state.Hearts>=3?2:1;
            stars.EarnedStarsOverride=earned;stars.enabled=!athlete;
            if(!Application.isPlaying) {
                // Editor still captures the final state; play mode uses the original animation.
                var fills=new[]{stars.leftStar,stars.midStar,stars.rightStar};
                var visible=new[]{stars.leftStarG,stars.midStarG,stars.rightStarG};
                for(int i=0;i<3;i++){visible[i].SetActive(i<earned);fills[i].GetComponent<Image>().fillAmount=i<earned?1:0;}
            }
        }
        if(athlete)foreach(var image in resultPanel.GetComponentsInChildren<Image>(true))if(image.name.IndexOf("Star",StringComparison.OrdinalIgnoreCase)>=0)image.gameObject.SetActive(false);
        if(saga!=null)ConfigureWinButtons(resultPanel,SagaRoute.Stop(saga.Game,level),()=>StartLevel(level),StartNextSaga,levels,SagaRoute.Count);
        else ConfigureWinButtons(resultPanel,level,()=>StartLevel(level),()=>StartLevel(level+1),levels);
        if(sagaWon!=null)foreach(var button in resultPanel.GetComponentsInChildren<Button>(true))button.interactable=false;
    }
    private void StartNextSaga(){
        if(saga==null)return;
        int stop=SagaRoute.Stop(saga.Game,level);
        if(stop>=SagaRoute.Count){ShowLevels();return;}
        var nextLevel=SagaRoute.At(stop+1);
        if(sagaNext!=null){var nextAction=sagaNext;Close();nextAction(nextLevel);return;}
        if(nextLevel.Game==SagaGame.Says || Array.IndexOf(SagaRoute.Games,nextLevel.Game)<0){ShowLevels();return;}
        saga=nextLevel;StartLevel(nextLevel.Number);
    }
    internal static Font Font {get {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }}
    public static GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max,Color color) {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var rect=(RectTransform)go.transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
        go.GetComponent<Image>().color=color;return go;
    }
    public static Text Label(Transform parent,string value,Vector2 min,Vector2 max,int size,Color color) {
        var go=new GameObject(value,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
        var text=go.GetComponent<Text>();var r=text.rectTransform;r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
        text.font=Font;text.fontSize=size;text.resizeTextForBestFit=true;text.resizeTextMinSize=14;text.resizeTextMaxSize=size;
        text.text=value;text.color=color;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return text;
    }
    public static Button Button(Transform parent,string value,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action) {
        var go=Panel(parent,value,min,max,new Color32(230,222,249,255));
        var button=go.AddComponent<Button>();button.image.sprite=ContentSelectorBar.Rounded();button.image.type=Image.Type.Sliced;button.onClick.AddListener(action);
        Label(go.transform,value,new Vector2(.03f,.04f),new Vector2(.97f,.96f),38,Ink);return button;
    }
}
