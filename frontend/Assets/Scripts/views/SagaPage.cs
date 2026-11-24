using UnityEngine;
using UnityEngine.UI;

// Saga reuses Chain and keeps its own local progress, independent of Training.
public class SagaPage : MonoBehaviour
{
    private GameObject page;
    private ChainGame chain;
    private int owner;
    private SaysGame says;
    private SagaBossGame boss;
    private FocusGame focus;
    private readonly System.Collections.Generic.List<Button> stageButtons = new System.Collections.Generic.List<Button>();
    private ScrollRect mapScroll;
    private RectTransform mapContent;
    private readonly System.Collections.Generic.List<RectTransform> sections = new System.Collections.Generic.List<RectTransform>();
    private float sectionHeight;
    private Sprite paintedHeader;
    private readonly System.Collections.Generic.List<Material> mapThemes = new System.Collections.Generic.List<Material>();
    private Coroutine victory;
    private int playingStop,frontierBefore,revealingStop;
    private float revealProgress;
    private SagaUnlockGraphic pulse;
    private SagaUnlockGraphic ambientPulse;
    private readonly System.Random ambientRandom=new System.Random();
    private float nextAmbientAt,ambientStarted;
    private int ambientStop;
    private Text motionCaption;
    private bool MotionEnabled {get{return PlayerPrefs.GetInt("saga.motion.v1",1)!=0;}}
    private static readonly Vector2[] Stops = {
        new Vector2(.478f,.067f),new Vector2(.761f,.175f),new Vector2(.488f,.204f),new Vector2(.198f,.223f),
        new Vector2(.166f,.335f),new Vector2(.426f,.331f),new Vector2(.709f,.355f),new Vector2(.718f,.463f),
        new Vector2(.439f,.487f),new Vector2(.190f,.480f),new Vector2(.129f,.593f),new Vector2(.418f,.620f),
        new Vector2(.728f,.598f),new Vector2(.788f,.730f),new Vector2(.544f,.749f),new Vector2(.295f,.746f),
        new Vector2(.106f,.790f),new Vector2(.270f,.852f),new Vector2(.514f,.860f),new Vector2(.804f,.873f)
    };

    public static void Attach(MenuNavigation menu)
    {
        if (menu.homeScreen == null || menu.trainingBackground == null || menu.challengeBackground == null) return;
        if (menu.homeScreen.transform.Find("Memotopia Saga") != null) return;
        var controller = menu.gameObject.AddComponent<SagaPage>();
        var training = menu.trainingBackground.rectTransform;
        // All existing home buttons remain, including Statistics, Videos and Tips.
        // Only the three tall rows move to make room below the unchanged title.
        if(menu.statisticsBackground!=null)Place(menu.statisticsBackground.rectTransform,.062f,.218f);
        Place(training, .578f, .734f);
        Place(menu.challengeBackground.rectTransform, .406f, .562f);
        PlaceVertical(menu.videosBackground,.234f,.390f);PlaceVertical(menu.tipsBackground,.234f,.390f);
        var tile = ChainGame.Panel(training.parent, "Memotopia Saga", new Vector2(.049f, .750f), new Vector2(.950f, .906f), ChainGame.Theme);
        tile.GetComponent<Image>().sprite = ContentSelectorBar.Rounded();
        tile.GetComponent<Image>().type = Image.Type.Sliced;
        tile.AddComponent<Button>().onClick.AddListener(() => controller.Open(menu));
        var symbol=ChainGame.Panel(tile.transform,"Saga neuron",new Vector2(.01f,.02f),new Vector2(.28f,.98f),Color.white).GetComponent<Image>();
        symbol.sprite=Resources.Load<Sprite>("Saga/Neuron");symbol.preserveAspect=true;symbol.raycastTarget=false;
        ChainGame.Label(tile.transform, "Memotopia Saga", new Vector2(.27f, .12f), new Vector2(.96f, .88f), 48, Color.white).fontStyle = FontStyle.Bold;
        var host=new GameObject("Saga Chain",typeof(RectTransform));
        host.transform.SetParent(menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas.transform,false);
        controller.chain=host.AddComponent<ChainGame>();
        controller.says=host.AddComponent<SaysGame>();
        controller.boss=host.AddComponent<SagaBossGame>();
        controller.focus=host.AddComponent<FocusGame>();
    }

    private static void Place(RectTransform rect, float bottom, float top)
    {
        rect.anchorMin = new Vector2(.049f, bottom);
        rect.anchorMax = new Vector2(.950f, top);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
    private static void PlaceVertical(Image image,float bottom,float top){
        if(image==null)return;var rect=image.rectTransform;
        rect.anchorMin=new Vector2(rect.anchorMin.x,bottom);rect.anchorMax=new Vector2(rect.anchorMax.x,top);
        rect.offsetMin=new Vector2(rect.offsetMin.x,0);rect.offsetMax=new Vector2(rect.offsetMax.x,0);
    }

    private void Open(MenuNavigation menu)
    {
        owner=GameManager.AchievementUserId;
        if(page!=null){ShowMap();return;}
        var canvas=menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas;
        var area=canvas.GetComponent<MobileSafeArea>();
        Transform parent=area!=null && area.Content!=null?area.Content:canvas.transform;
        page=ChainGame.Panel(parent,"Saga map",Vector2.zero,Vector2.one,Color.white);
        var backdrop=new GameObject("Saga neural backdrop",typeof(RectTransform),typeof(CanvasRenderer));backdrop.transform.SetParent(page.transform,false);
        var backdropRect=(RectTransform)backdrop.transform;backdropRect.anchorMin=Vector2.zero;backdropRect.anchorMax=Vector2.one;backdropRect.offsetMin=backdropRect.offsetMax=Vector2.zero;
        backdrop.AddComponent<SagaBackdropGraphic>().raycastTarget=false;
        // Keep the approved painted header fixed while all five sections scroll.
        var header=ChainGame.Panel(page.transform,"Saga header",new Vector2(.02f,.91f),new Vector2(.98f,.995f),Color.clear);
        var heading=header.GetComponent<Image>();var headerTexture=Resources.Load<Sprite>("Saga/Map20").texture;
        paintedHeader=Sprite.Create(headerTexture,new Rect(0,headerTexture.height*.945f,headerTexture.width,headerTexture.height*.055f),new Vector2(.5f,.5f));
        heading.sprite=paintedHeader;heading.color=Color.white;heading.raycastTarget=false;
        var headerTheme=new Material(Resources.Load<Shader>("Saga/MapProgress"));var headerCells=new Vector4[20];for(int i=0;i<20;i++)headerCells[i]=new Vector4(Stops[i].x,Stops[i].y,1,0);headerTheme.SetVectorArray("_Cells",headerCells);heading.material=headerTheme;paintedHeaderTheme=headerTheme;
        var headerFit=header.AddComponent<AspectRatioFitter>();headerFit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;headerFit.aspectRatio=1024f/(1536f*.055f);
        // Fit the painted strip inside a fixed header frame, not the full page.
        var headerFrame=ChainGame.Panel(page.transform,"Saga header frame",new Vector2(.02f,.91f),new Vector2(.98f,.995f),Color.clear);
        headerFrame.GetComponent<Image>().raycastTarget=false;header.transform.SetParent(headerFrame.transform,false);
        var back=ChainGame.Panel(header.transform,"Back",Vector2.zero,new Vector2(.20f,1),Color.clear);
        back.AddComponent<Button>().onClick.AddListener(Close);
        var viewport=ChainGame.Panel(page.transform,"Saga scroll viewport",new Vector2(.02f,.05f),new Vector2(.98f,.90f),Color.clear);
        viewport.AddComponent<RectMask2D>();
        mapScroll=viewport.AddComponent<ScrollRect>();mapScroll.horizontal=false;mapScroll.vertical=true;mapScroll.movementType=ScrollRect.MovementType.Clamped;
        mapScroll.viewport=(RectTransform)viewport.transform;
        mapContent=(RectTransform)ChainGame.Panel(viewport.transform,"Saga route 1-100",Vector2.zero,new Vector2(1,0),Color.clear).transform;
        mapContent.pivot=new Vector2(.5f,0);mapContent.GetComponent<Image>().raycastTarget=false;mapScroll.content=mapContent;
        var mapSprite=Resources.Load<Sprite>("Saga/MapUnnumbered");var texture=mapSprite.texture;
        for(int block=0;block<Mathf.CeilToInt(SagaRoute.Count/20f);block++){
            var section=(RectTransform)ChainGame.Panel(mapContent,"Saga section "+(block*20+1)+"-"+Mathf.Min(block*20+20,SagaRoute.Count),Vector2.zero,new Vector2(1,0),Color.clear).transform;
            section.pivot=new Vector2(.5f,0);section.GetComponent<Image>().raycastTarget=false;sections.Add(section);
            var art=ChainGame.Panel(section,"Approved neuron artwork",Vector2.zero,Vector2.one,Color.clear);
            var image=art.GetComponent<Image>();image.sprite=mapSprite;image.color=Color.white;image.raycastTarget=false;
            var theme=new Material(Resources.Load<Shader>("Saga/MapProgress"));image.material=theme;mapThemes.Add(theme);
            var fit=art.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;
            for(int i=0;i<Stops.Length && block*20+i<SagaRoute.Count;i++){
                int stop=block*20+i+1;Vector2 point=Stops[i];
                var node=ChainGame.Panel(art.transform,"Saga level "+stop,point-new Vector2(.065f,.04f),point+new Vector2(.065f,.04f),Color.clear);
                var button=node.AddComponent<Button>();button.transition=Selectable.Transition.None;
                button.onClick.AddListener(()=>PlayStage(stop));stageButtons.Add(button);
                var label=ChainGame.Label(node.transform,stop.ToString(),new Vector2(.26f,.20f),new Vector2(.74f,.80f),38,Color.white);
                label.fontStyle=FontStyle.Bold;label.resizeTextForBestFit=true;label.resizeTextMinSize=12;label.resizeTextMaxSize=38;label.raycastTarget=false;
                if(SagaRoute.At(stop).Game==SagaGame.Boss || SagaRoute.At(stop).Game==SagaGame.FocusBoss){var badge=ChainGame.Panel(node.transform,"Boss marker",new Vector2(.66f,.66f),new Vector2(.98f,.98f),Color.white).GetComponent<Image>();badge.sprite=Resources.Load<Sprite>("GamesResources/Levels/Asset 15");badge.preserveAspect=true;badge.raycastTarget=false;}
            }
        }
        Canvas.ForceUpdateCanvases();LayoutMap();mapScroll.verticalNormalizedPosition=0;
        var motion=ChainGame.Button(page.transform,"",new Vector2(.66f,.008f),new Vector2(.97f,.043f),ToggleMotion);
        motion.image.color=Color.clear;motionCaption=motion.GetComponentInChildren<Text>();motionCaption.resizeTextForBestFit=false;motionCaption.fontSize=20;motionCaption.color=ChainGame.Theme;UpdateMotionCaption();
        nextAmbientAt=Time.unscaledTime+3;
        RefreshProgress();
    }
    private void LayoutMap(){
        if(mapScroll==null || mapContent==null)return;
        float height=mapScroll.viewport.rect.height;if(height<=0 || Mathf.Approximately(height,sectionHeight))return;
        float position=mapScroll.verticalNormalizedPosition;sectionHeight=height;
        mapContent.sizeDelta=new Vector2(0,height*sections.Count);
        for(int i=0;i<sections.Count;i++){sections[i].sizeDelta=new Vector2(0,height);sections[i].anchoredPosition=new Vector2(0,i*height);}
        Canvas.ForceUpdateCanvases();mapScroll.verticalNormalizedPosition=position;
    }
    private void PlayStage(int stop){
        if(!SagaRoute.Unlocked(stop) || chain.IsSagaOpen || says.IsSagaOpen || boss.IsOpen || focus.IsSagaOpen)return;
        if(victory!=null){StopCoroutine(victory);victory=null;}revealingStop=0;
        playingStop=stop;frontierBefore=CurrentStop();
        StopAmbient();
        var stage=SagaRoute.At(stop);
        page.SetActive(false);
        int focusKind=System.Array.IndexOf(SagaRoute.FocusGames,stage.Game);
        if(stage.Game==SagaGame.Boss || stage.Game==SagaGame.FocusBoss)boss.Open(stage.Number,ShowMap,OnWon,stage.Game==SagaGame.FocusBoss);
        else if(focusKind>=0)focus.OpenSaga(new SagaFocusLevel(focusKind,stage.Number),ShowMap,OnWon);
        else if(stage.Game==SagaGame.Says)says.OpenSaga(stage.Number,ShowMap,()=>{says.Close();PlayStage(stop+1);},OnWon);
        else chain.OpenSaga(stage.Game,stage.Number,ShowMap,following=>PlayStage(SagaRoute.Stop(following.Game,following.Number)),OnWon);
        if(!chain.IsSagaOpen && !says.IsSagaOpen && !boss.IsOpen && !focus.IsSagaOpen)ShowMap();
    }
    private void ShowMap(){
        if(page==null || owner!=GameManager.AchievementUserId)return;
        page.SetActive(true);page.transform.SetAsLastSibling();RefreshProgress();
    }
    private void RefreshProgress(){
        int current=CurrentStop();
        for(int i=0;i<stageButtons.Count;i++){
            stageButtons[i].interactable=SagaRoute.Unlocked(i+1);
            var badge=stageButtons[i].transform.Find("Boss marker");if(badge!=null)badge.GetComponent<Image>().color=Completed(i+1) || i+1==current?new Color(1,.78f,.20f):new Color(.65f,.65f,.65f);
        }
        for(int block=0;block<mapThemes.Count;block++){
            var cells=new Vector4[20];
            for(int i=0;i<20;i++){
                int stop=block*20+i+1;float visible=stop<=SagaRoute.Count && (Completed(stop) || stop==current)?1:0;
                if(stop==revealingStop)visible=revealProgress;
                cells[i]=new Vector4(Stops[i].x,Stops[i].y,visible,0);
            }
            mapThemes[block].SetVectorArray("_Cells",cells);
        }
    }
    private static bool Completed(int stop){var stage=SagaRoute.At(stop);return SagaProgress.Stars(stage.Game,stage.Number)>0;}
    private static int CurrentStop(){for(int stop=1;stop<=SagaRoute.Count;stop++)if(!Completed(stop))return stop;return SagaRoute.Count+1;}
    private void OnWon(){if(Application.isPlaying){if(victory!=null)StopCoroutine(victory);victory=StartCoroutine(ReturnAndUnlock());}}
    private System.Collections.IEnumerator ReturnAndUnlock(){
        int completed=playingStop;bool unlock=completed==frontierBefore && completed<SagaRoute.Count && !Completed(completed+1);
        // Let the existing victory/stars play before returning to the route.
        var canvas=page.GetComponentInParent<Canvas>().rootCanvas;
        var result=System.Linq.Enumerable.FirstOrDefault(canvas.GetComponentsInChildren<SagaResultView>(),view=>view.Won);
        var stars=result!=null?result.GetComponentsInChildren<StarFillScript>():new StarFillScript[0];
        for(float elapsed=0;elapsed<5;elapsed+=Time.unscaledDeltaTime){
            if(result==null || result.Consumed || !result.gameObject.activeInHierarchy){victory=null;yield break;}
            bool finished=true;
            foreach(var fill in stars)if(fill!=null && fill.EarnedStarsOverride>0){var last=fill.EarnedStarsOverride>=3?fill.rightStar:fill.EarnedStarsOverride==2?fill.midStar:fill.leftStar;if(last.GetComponent<Image>().fillAmount<.99f)finished=false;}
            if(elapsed>=1 && finished)break;yield return null;
        }
        if(owner!=GameManager.AchievementUserId){victory=null;yield break;}
        revealingStop=unlock?completed+1:0;revealProgress=0;
        if(chain.IsSagaOpen)chain.Close();if(says.IsSagaOpen)says.Close();if(boss.IsOpen)boss.Close();if(focus.IsSagaOpen)focus.Close();ShowMap();
        int destination=unlock?completed+1:completed;
        float from=mapScroll.verticalNormalizedPosition,to=((destination-1)/20)/(float)Mathf.Max(1,sections.Count-1);
        mapScroll.StopMovement();
        for(float elapsed=0;MotionEnabled && elapsed<.45f;elapsed+=Time.unscaledDeltaTime){mapScroll.verticalNormalizedPosition=Mathf.Lerp(from,to,Mathf.SmoothStep(0,1,elapsed/.45f));yield return null;}
        mapScroll.verticalNormalizedPosition=to;Canvas.ForceUpdateCanvases();
        if(unlock && MotionEnabled){
            var obj=new GameObject("Saga electrical unlock",typeof(RectTransform),typeof(CanvasRenderer));obj.transform.SetParent(mapContent,false);
            var rect=(RectTransform)obj.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,0);rect.offsetMin=rect.offsetMax=Vector2.zero;
            pulse=obj.AddComponent<SagaUnlockGraphic>();pulse.raycastTarget=false;
            pulse.From=mapContent.InverseTransformPoint(stageButtons[completed-1].transform.position);pulse.To=mapContent.InverseTransformPoint(stageButtons[completed].transform.position);
            pulse.Bend=(pulse.From+pulse.To)*.5f;
            for(float elapsed=0;MotionEnabled && elapsed<1.25f;elapsed+=Time.unscaledDeltaTime){float t=elapsed/1.25f;pulse.SetProgress(t);revealProgress=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1,t));RefreshProgress();yield return null;}
            Destroy(obj);pulse=null;
        }
        revealingStop=0;RefreshProgress();victory=null;nextAmbientAt=Time.unscaledTime+3;
    }
    private Material paintedHeaderTheme;
    private void ToggleMotion(){PlayerPrefs.SetInt("saga.motion.v1",MotionEnabled?0:1);PlayerPrefs.Save();StopAmbient();UpdateMotionCaption();}
    private void UpdateMotionCaption(){if(motionCaption!=null)motionCaption.text="Motion: "+(MotionEnabled?"On":"Off");}
    private void StopAmbient(){ambientStop=0;if(ambientPulse!=null)Destroy(ambientPulse.gameObject);ambientPulse=null;foreach(var theme in mapThemes)theme.SetVector("_Charge",Vector4.zero);}
    private void TickAmbient(){
        if(!MotionEnabled || victory!=null || !Application.isPlaying)return;
        float now=Time.unscaledTime;
        if(ambientStop>0){
            float elapsed=now-ambientStarted;Vector2 point=Stops[(ambientStop-1)%20];
            float strength=Mathf.Sin(Mathf.Clamp01(elapsed/.65f)*Mathf.PI);
            mapThemes[(ambientStop-1)/20].SetVector("_Charge",new Vector4(point.x,point.y,strength,elapsed));
            if(ambientPulse!=null)ambientPulse.SetProgress(Mathf.Clamp01((elapsed-.3f)/.8f)*.35f);
            if(elapsed>1.1f)StopAmbient();return;
        }
        if(now<nextAmbientAt)return;
        nextAmbientAt=now+3+(float)ambientRandom.NextDouble()*6;
        var candidates=new System.Collections.Generic.List<int>();int current=CurrentStop();
        for(int i=0;i<stageButtons.Count;i++)if((i+1==current || Completed(i+1)) && mapScroll.viewport.rect.Contains(mapScroll.viewport.InverseTransformPoint(stageButtons[i].transform.position)))candidates.Add(i+1);
        if(candidates.Count==0)return;
        ambientStop=candidates[ambientRandom.Next(candidates.Count)];ambientStarted=now;
        if(ambientStop<SagaRoute.Count){
            var obj=new GameObject("Saga spontaneous discharge",typeof(RectTransform),typeof(CanvasRenderer));obj.transform.SetParent(mapContent,false);
            var rect=(RectTransform)obj.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,0);rect.offsetMin=rect.offsetMax=Vector2.zero;
            ambientPulse=obj.AddComponent<SagaUnlockGraphic>();ambientPulse.raycastTarget=false;
            ambientPulse.From=mapContent.InverseTransformPoint(stageButtons[ambientStop-1].transform.position);ambientPulse.To=mapContent.InverseTransformPoint(stageButtons[ambientStop].transform.position);ambientPulse.Bend=(ambientPulse.From+ambientPulse.To)*.5f;
        }
    }
    private void Close(){if(victory!=null){StopCoroutine(victory);victory=null;}StopAmbient();if(pulse!=null)Destroy(pulse.gameObject);revealingStop=0;if(page!=null)page.SetActive(false);}
    private void Update(){
        if(page==null || !page.activeSelf)return;
        LayoutMap();
        TickAmbient();
        if(owner!=GameManager.AchievementUserId || Input.GetKeyDown(KeyCode.Escape))Close();
    }
    private void OnDestroy(){if(page!=null)Destroy(page);if(paintedHeader!=null)Destroy(paintedHeader);if(paintedHeaderTheme!=null)Destroy(paintedHeaderTheme);foreach(var theme in mapThemes)if(theme!=null)Destroy(theme);}
}
