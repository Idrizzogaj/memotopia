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
    private float sectionHeight,sectionWidth;
    private readonly System.Collections.Generic.List<RectTransform> sectionArt=new System.Collections.Generic.List<RectTransform>();
    private readonly System.Collections.Generic.List<Vector4[]> cellBuffers=new System.Collections.Generic.List<Vector4[]>();
    private readonly System.Collections.Generic.List<Vector4[]> cutCenters=new System.Collections.Generic.List<Vector4[]>();
    private readonly System.Collections.Generic.List<Vector4[]> cutAxes=new System.Collections.Generic.List<Vector4[]>();
    private readonly System.Collections.Generic.Dictionary<int,SagaAxonGraphic> sectionAxons=new System.Collections.Generic.Dictionary<int,SagaAxonGraphic>();
    private readonly System.Collections.Generic.Dictionary<int,Image> bossLabels=new System.Collections.Generic.Dictionary<int,Image>();
    private readonly bool[] completedStages=new bool[SagaRoute.Count];
    private int frontier=1,queuedMapStop;
    private bool queuedUnlock,runningMapReturn;
    private Sprite paintedHeader;
    private readonly Material[] bossThemes=new Material[2];
    private readonly System.Collections.Generic.Dictionary<int,RectTransform> synapseGates=new System.Collections.Generic.Dictionary<int,RectTransform>();
    private SagaUnlockGraphic synapseSignal;
    private readonly System.Collections.Generic.List<Material> mapThemes = new System.Collections.Generic.List<Material>();
    private Coroutine victory;
    private int playingStop,frontierBefore,revealingStop;
    private float revealProgress;
    private SagaUnlockGraphic pulse;
    private SagaUnlockGraphic ambientPulse;
    private readonly System.Random ambientRandom=new System.Random();
    private float nextAmbientAt,ambientStarted;
    private int ambientStop,ambientEndStop;
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
        SagaProgress.PrepareTestInstall(owner);
        if(page!=null){ShowMap();return;}
        var canvas=menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas;
        var area=canvas.GetComponent<MobileSafeArea>();
        Transform parent=area!=null && area.Content!=null?area.Content:canvas.transform;
        page=ChainGame.Panel(parent,"Saga map",Vector2.zero,Vector2.one,Color.white);
        var backdrop=new GameObject("Saga neural backdrop",typeof(RectTransform),typeof(CanvasRenderer));backdrop.transform.SetParent(page.transform,false);
        var backdropRect=(RectTransform)backdrop.transform;backdropRect.anchorMin=Vector2.zero;backdropRect.anchorMax=Vector2.one;backdropRect.offsetMin=backdropRect.offsetMax=Vector2.zero;
        backdrop.AddComponent<SagaBackdropGraphic>().raycastTarget=false;
        // Keep the approved painted header fixed while all sections scroll.
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
        mapContent=(RectTransform)ChainGame.Panel(viewport.transform,"Saga route 1-"+SagaRoute.Count,Vector2.zero,new Vector2(1,0),Color.clear).transform;
        mapContent.pivot=new Vector2(.5f,0);mapContent.GetComponent<Image>().raycastTarget=false;mapScroll.content=mapContent;
        var mapSprite=Resources.Load<Sprite>("Saga/MapUnnumbered");var texture=mapSprite.texture;
        for(int state=0;state<2;state++){
            bossThemes[state]=new Material(Resources.Load<Shader>("Saga/MapProgress"));
            var cells=new Vector4[20];for(int n=0;n<cells.Length;n++)cells[n]=new Vector4(.5f,.5f,state,0);
            bossThemes[state].SetVectorArray("_Cells",cells);
        }
        for(int block=0;block<Mathf.CeilToInt(SagaRoute.Count/20f);block++){
            var section=(RectTransform)ChainGame.Panel(mapContent,"Saga section "+(block*20+1)+"-"+Mathf.Min(block*20+20,SagaRoute.Count),Vector2.zero,new Vector2(1,0),Color.clear).transform;
            section.pivot=new Vector2(.5f,0);section.GetComponent<Image>().raycastTarget=false;sections.Add(section);
            var art=ChainGame.Panel(section,"Approved neuron artwork",Vector2.zero,Vector2.one,Color.clear);
            sectionArt.Add((RectTransform)art.transform);cellBuffers.Add(new Vector4[20]);cutCenters.Add(new Vector4[8]);cutAxes.Add(new Vector4[8]);
            var image=art.GetComponent<Image>();image.sprite=mapSprite;image.color=Color.white;image.raycastTarget=false;
            var theme=new Material(Resources.Load<Shader>("Saga/MapProgress"));image.material=theme;mapThemes.Add(theme);
            var fit=art.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;
            for(int i=0;i<Stops.Length && block*20+i<SagaRoute.Count;i++){
                int stop=block*20+i+1;Vector2 point=Stops[i];
                int phase=(stop-1)%12;
                if(phase==0 || phase==7){
                    string caption=(phase==0?"Memory ":"Focus ")+((stop-1)/12+1);
                    Vector2 at=BlockCaptionPosition(i);
                    var ribbon=ChainGame.Panel(art.transform,"Block "+caption,at-new Vector2(.105f,.018f),at+new Vector2(.105f,.018f),new Color(1,1,1,.94f));
                    ribbon.GetComponent<Image>().sprite=ContentSelectorBar.Rounded();ribbon.GetComponent<Image>().type=Image.Type.Sliced;ribbon.GetComponent<Image>().raycastTarget=false;
                    ChainGame.Label(ribbon.transform,caption,Vector2.zero,Vector2.one,28,phase==0?ChainGame.Purple:FocusGame.Teal).fontStyle=FontStyle.Bold;
                }
                bool isBoss=SagaRoute.At(stop).Game==SagaGame.Boss || SagaRoute.At(stop).Game==SagaGame.FocusBoss;
                Vector2 radius=isBoss?new Vector2(.092f,.062f):new Vector2(.065f,.04f);
                var node=ChainGame.Panel(art.transform,"Saga level "+stop,point-radius,point+radius,Color.clear);
                var button=node.AddComponent<Button>();button.transition=Selectable.Transition.None;
                button.onClick.AddListener(()=>PlayStage(stop));stageButtons.Add(button);
                var label=ChainGame.Label(node.transform,stop.ToString(),new Vector2(.26f,.20f),new Vector2(.74f,.80f),38,Color.white);
                label.fontStyle=FontStyle.Bold;label.resizeTextForBestFit=true;label.resizeTextMinSize=12;label.resizeTextMaxSize=38;label.raycastTarget=false;
                if(isBoss){
                    var tag=ChainGame.Panel(node.transform,"Boss label",new Vector2(.22f,-.10f),new Vector2(.78f,.16f),ChainGame.Purple);
                    tag.GetComponent<Image>().sprite=ContentSelectorBar.Rounded();tag.GetComponent<Image>().type=Image.Type.Sliced;tag.GetComponent<Image>().raycastTarget=false;
                    ChainGame.Label(tag.transform,"BOSS",Vector2.zero,Vector2.one,22,Color.white).fontStyle=FontStyle.Bold;bossLabels.Add(stop,tag.GetComponent<Image>());
                }
            }
        }
        for(int boundary=20;boundary<SagaRoute.Count;boundary+=20){
            var obj=new GameObject("Myelinated page connection "+boundary+"-"+(boundary+1),typeof(RectTransform),typeof(CanvasRenderer));obj.transform.SetParent(mapContent,false);
            var axon=obj.AddComponent<SagaAxonGraphic>();axon.raycastTarget=false;axon.Source=mapSprite.texture;sectionAxons.Add(boundary,axon);
        }
        for(int stop=1;stop<SagaRoute.Count;stop++){
            var kind=SagaRoute.At(stop).Game;if(kind!=SagaGame.Boss && kind!=SagaGame.FocusBoss)continue;
            var gate=(RectTransform)ChainGame.Panel(mapContent,"Synapse gate "+stop,new Vector2(.5f,0),new Vector2(.5f,0),Color.white).transform;
            gate.pivot=new Vector2(.5f,.5f);var image=gate.GetComponent<Image>();image.sprite=Resources.Load<Sprite>("Saga/SynapseGate");image.preserveAspect=true;image.raycastTarget=false;
            synapseGates.Add(stop,gate);
        }
        Canvas.ForceUpdateCanvases();LayoutMap();mapScroll.verticalNormalizedPosition=0;
        var motion=ChainGame.Button(page.transform,"",new Vector2(.66f,.008f),new Vector2(.97f,.043f),ToggleMotion);
        motion.image.color=Color.clear;motionCaption=motion.GetComponentInChildren<Text>();motionCaption.resizeTextForBestFit=false;motionCaption.fontSize=20;motionCaption.color=ChainGame.Theme;UpdateMotionCaption();
        nextAmbientAt=Time.unscaledTime+3;
        RefreshProgress();
    }
    private static Vector2 BlockCaptionPosition(int index){
        switch(index){
            case 0:return new Vector2(.22f,.115f);case 12:return new Vector2(.80f,.65f);
            case 4:return new Vector2(.15f,.395f);case 16:return new Vector2(.115f,.855f);
            case 8:return new Vector2(.48f,.54f);case 7:return new Vector2(.78f,.52f);
            case 19:return new Vector2(.61f,.925f);case 11:return new Vector2(.51f,.68f);
            case 3:return new Vector2(.16f,.16f);default:return new Vector2(.36f,.69f);
        }
    }
    private void LayoutSynapses(){
        foreach(var pair in synapseGates){
            Vector2 from=mapContent.InverseTransformPoint(stageButtons[pair.Key-1].transform.position);
            Vector2 to=mapContent.InverseTransformPoint(stageButtons[pair.Key].transform.position);
            var rect=pair.Value;bool curved=pair.Key%20==0;rect.anchoredPosition=SagaAxonGraphic.RoutePoint(from,to,.57f,curved);
            float width=Mathf.Min(mapScroll.viewport.rect.width*.19f,Vector2.Distance(from,to)*.70f);
            rect.sizeDelta=new Vector2(width,width/3f);Vector2 direction=SagaAxonGraphic.RoutePoint(from,to,.58f,curved)-SagaAxonGraphic.RoutePoint(from,to,.56f,curved);
            rect.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        }
        foreach(var pair in sectionAxons){
            Vector2 from=mapContent.InverseTransformPoint(stageButtons[pair.Key-1].transform.position),to=mapContent.InverseTransformPoint(stageButtons[pair.Key].transform.position);
            var center=(from+to)*.5f;float padding=sectionArt[(pair.Key-1)/20].rect.width*.05f;
            var rect=(RectTransform)pair.Value.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=center;
            rect.sizeDelta=new Vector2(Mathf.Abs(to.x-from.x)+padding*2,Mathf.Abs(to.y-from.y)+padding*2);
            pair.Value.From=from-center;pair.Value.To=to-center;pair.Value.UnitWidth=sectionArt[(pair.Key-1)/20].rect.width*43f/1024f;pair.Value.EndPadding=padding;
            RectTransform gate;pair.Value.HasGate=synapseGates.TryGetValue(pair.Key,out gate);
            if(gate!=null){pair.Value.GateCenter=gate.anchoredPosition-center;pair.Value.GateWidth=gate.rect.width;}
            pair.Value.SetVerticesDirty();
        }
        LayoutSynapseCuts();
    }
    private void LayoutSynapseCuts(){
        for(int block=0;block<sectionArt.Count;block++){
            int count=0;var art=sectionArt[block];var centers=cutCenters[block];var axes=cutAxes[block];
            foreach(var pair in synapseGates){
                if(pair.Key%20==0 || (pair.Key-1)/20!=block)continue;
                Vector2 local=art.InverseTransformPoint(pair.Value.position);
                Vector2 axis=art.InverseTransformVector(pair.Value.TransformVector(Vector3.right));axis.Normalize();
                centers[count]=new Vector4((local.x-art.rect.xMin)/art.rect.width,(local.y-art.rect.yMin)/art.rect.height,pair.Value.rect.width/art.rect.width*.53f,Mathf.Max((pair.Key%20==4 || pair.Key%20==7 || pair.Key%20==10 || pair.Key%20==13)?.10f:.028f,pair.Value.rect.height/art.rect.width*.9f));
                bool curved=pair.Key%20==4 || pair.Key%20==7 || pair.Key%20==10 || pair.Key%20==13;
                axes[count]=new Vector4(axis.x,axis.y,curved?.075f:.035f,curved?.095f:.055f);count++;
            }
            mapThemes[block].SetVectorArray("_CutCenters",centers);mapThemes[block].SetVectorArray("_CutAxes",axes);mapThemes[block].SetInt("_CutCount",count);
        }
    }
    private void LayoutMap(){
        if(mapScroll==null || mapContent==null)return;
        float height=mapScroll.viewport.rect.height,width=mapScroll.viewport.rect.width;
        if(height<=0 || width<=0 || (Mathf.Approximately(height,sectionHeight) && Mathf.Approximately(width,sectionWidth)))return;
        float position=mapScroll.verticalNormalizedPosition;sectionHeight=height;sectionWidth=width;
        mapContent.sizeDelta=new Vector2(0,height*sections.Count);
        for(int i=0;i<sections.Count;i++){sections[i].sizeDelta=new Vector2(0,height);sections[i].anchoredPosition=new Vector2(0,i*height);}
        Canvas.ForceUpdateCanvases();LayoutSynapses();mapScroll.verticalNormalizedPosition=position;
    }
    private void PlayStage(int stop){
        if(owner!=GameManager.AchievementUserId){Close();return;}
        if(!SagaRoute.Unlocked(stop) || chain.IsSagaOpen || says.IsSagaOpen || boss.IsOpen || focus.IsSagaOpen)return;
        CancelMapReturn();queuedMapStop=0;revealingStop=0;
        RefreshProgress();playingStop=stop;frontierBefore=CurrentStop();
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
        page.SetActive(true);page.transform.SetAsLastSibling();
        int destination=queuedMapStop;bool unlock=queuedUnlock;queuedMapStop=0;
        revealingStop=unlock?destination+1:0;revealProgress=0;RefreshProgress();
        if(destination>0 && Application.isPlaying){if(victory!=null)StopCoroutine(victory);runningMapReturn=true;var animation=StartCoroutine(ReturnAndUnlock(destination,unlock));victory=runningMapReturn?animation:null;}
        else if(destination>0){revealingStop=0;RefreshProgress();}
    }
    private void RefreshProgress(){
        frontier=SagaRoute.Count+1;
        for(int stop=1;stop<=SagaRoute.Count;stop++){var stage=SagaRoute.At(stop);completedStages[stop-1]=SagaProgress.Stars(stage.Game,owner,stage.Number)>0;if(!completedStages[stop-1] && frontier>SagaRoute.Count)frontier=stop;}
        int current=CurrentStop();
        for(int i=0;i<stageButtons.Count;i++){
            stageButtons[i].interactable=SagaRoute.Unlocked(i+1);
            Image label;if(bossLabels.TryGetValue(i+1,out label))label.color=Completed(i+1) || i+1==current?ChainGame.Purple:new Color(.45f,.45f,.45f);
        }
        foreach(var pair in synapseGates)pair.Value.GetComponent<Image>().material=bossThemes[Completed(pair.Key)?1:0];
        foreach(var pair in sectionAxons)pair.Value.material=bossThemes[Completed(pair.Key)?1:0];
        for(int block=0;block<mapThemes.Count;block++){
            var cells=cellBuffers[block];
            for(int i=0;i<20;i++){
                int stop=block*20+i+1;float visible=stop<=SagaRoute.Count && (Completed(stop) || stop==current)?1:0;
                if(stop==revealingStop)visible=revealProgress;
                bool bossCell=stop<=SagaRoute.Count && (SagaRoute.At(stop).Game==SagaGame.Boss || SagaRoute.At(stop).Game==SagaGame.FocusBoss);
                cells[i]=new Vector4(Stops[i].x,Stops[i].y,visible,bossCell?1.2f:.78f);
            }
            mapThemes[block].SetVectorArray("_Cells",cells);
        }
    }
    private void ClearSynapseSignal(){if(synapseSignal!=null)Destroy(synapseSignal.gameObject);synapseSignal=null;}
    private bool Completed(int stop){return stop>=1 && stop<=completedStages.Length && completedStages[stop-1];}
    private int CurrentStop(){return frontier;}
    private void ApplyReveal(){if(revealingStop<=0)return;int block=(revealingStop-1)/20,index=(revealingStop-1)%20;cellBuffers[block][index].z=revealProgress;mapThemes[block].SetVectorArray("_Cells",cellBuffers[block]);}
    private void OnWon(){
        if(!Application.isPlaying || page==null)return;
        int completed=playingStop,originalOwner=owner;bool unlock=completed==frontierBefore && completed<SagaRoute.Count && !Completed(completed+1);
        var canvas=page.GetComponentInParent<Canvas>().rootCanvas;
        var result=System.Linq.Enumerable.FirstOrDefault(canvas.GetComponentsInChildren<SagaResultView>(),view=>view.Won && view.Stop==completed);
        if(result!=null)result.BeforeMap=()=>{if(page!=null && owner==originalOwner && owner==GameManager.AchievementUserId && playingStop==completed){queuedMapStop=completed;queuedUnlock=unlock;}};
    }
    private System.Collections.IEnumerator ReturnAndUnlock(int completed,bool unlock){
        // The player has explicitly chosen MAP; leave all result timing to them.
        int destination=unlock?completed+1:completed;
        float from=mapScroll.verticalNormalizedPosition,to=((destination-1)/20)/(float)Mathf.Max(1,sections.Count-1);
        mapScroll.StopMovement();
        for(float elapsed=0;MotionEnabled && elapsed<.65f && !Mathf.Approximately(from,to);elapsed+=Time.unscaledDeltaTime){mapScroll.verticalNormalizedPosition=Mathf.Lerp(from,to,Mathf.SmoothStep(0,1,elapsed/.65f));yield return null;}
        mapScroll.verticalNormalizedPosition=to;Canvas.ForceUpdateCanvases();
        if(unlock && MotionEnabled){
            var obj=new GameObject("Saga electrical unlock",typeof(RectTransform),typeof(CanvasRenderer));obj.transform.SetParent(mapContent,false);
            var rect=(RectTransform)obj.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,0);rect.offsetMin=rect.offsetMax=Vector2.zero;
            pulse=obj.AddComponent<SagaUnlockGraphic>();pulse.raycastTarget=false;
            pulse.From=mapContent.InverseTransformPoint(stageButtons[completed-1].transform.position);pulse.To=mapContent.InverseTransformPoint(stageButtons[completed].transform.position);
            pulse.Bend=(pulse.From+pulse.To)*.5f;pulse.Cubic=completed%20==0;
            RectTransform gate;bool crossesGate=synapseGates.TryGetValue(completed,out gate);
            if(crossesGate){
                var spark=new GameObject("Synaptic discharge",typeof(RectTransform),typeof(CanvasRenderer));spark.transform.SetParent(gate,false);
                var sparkRect=(RectTransform)spark.transform;sparkRect.anchorMin=Vector2.zero;sparkRect.anchorMax=Vector2.one;sparkRect.offsetMin=sparkRect.offsetMax=Vector2.zero;
                synapseSignal=spark.AddComponent<SagaUnlockGraphic>();synapseSignal.raycastTarget=false;
                float gap=gate.rect.width*.08f;synapseSignal.From=new Vector2(-gap,0);synapseSignal.To=new Vector2(gap,0);synapseSignal.Bend=new Vector2(0,gate.rect.height*.15f);
            }
            float duration=crossesGate?1.8f:1.25f;
            for(float elapsed=0;MotionEnabled && elapsed<duration;elapsed+=Time.unscaledDeltaTime){
                float t=elapsed/duration;pulse.SetProgress(t);
                if(synapseSignal!=null){synapseSignal.gameObject.SetActive(t>=.35f && t<=.72f);synapseSignal.SetProgress(Mathf.InverseLerp(.35f,.72f,t));}
                revealProgress=Mathf.SmoothStep(0,1,Mathf.InverseLerp(crossesGate?.72f:.65f,1,t));ApplyReveal();yield return null;
            }
            ClearSynapseSignal();Destroy(obj);pulse=null;
        }
        revealingStop=0;RefreshProgress();runningMapReturn=false;victory=null;nextAmbientAt=Time.unscaledTime+3;
    }
    private Material paintedHeaderTheme;
    private void ToggleMotion(){PlayerPrefs.SetInt("saga.motion.v1",MotionEnabled?0:1);PlayerPrefs.Save();StopAmbient();ClearSynapseSignal();UpdateMotionCaption();}
    private void UpdateMotionCaption(){if(motionCaption!=null)motionCaption.text="Motion: "+(MotionEnabled?"On":"Off");}
    private void StopAmbient(){ambientStop=ambientEndStop=0;if(ambientPulse!=null)Destroy(ambientPulse.gameObject);ambientPulse=null;foreach(var theme in mapThemes)theme.SetVector("_Charge",Vector4.zero);}
    private bool VisibleStop(int stop){
        return stop>=1 && stop<=stageButtons.Count && mapScroll.viewport.rect.Contains(mapScroll.viewport.InverseTransformPoint(stageButtons[stop-1].transform.position));
    }
    private void BeginAmbientSegment(float now){
        ambientStarted=now;
        foreach(var theme in mapThemes)theme.SetVector("_Charge",Vector4.zero);
        if(ambientStop>=ambientEndStop){if(ambientPulse!=null)ambientPulse.gameObject.SetActive(false);return;}
        if(ambientPulse==null){
            var obj=new GameObject("Saga spontaneous discharge",typeof(RectTransform),typeof(CanvasRenderer));obj.transform.SetParent(mapContent,false);
            var rect=(RectTransform)obj.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,0);rect.offsetMin=rect.offsetMax=Vector2.zero;
            ambientPulse=obj.AddComponent<SagaUnlockGraphic>();ambientPulse.raycastTarget=false;
        }
        ambientPulse.gameObject.SetActive(true);
        ambientPulse.From=mapContent.InverseTransformPoint(stageButtons[ambientStop-1].transform.position);
        ambientPulse.To=mapContent.InverseTransformPoint(stageButtons[ambientStop].transform.position);
        ambientPulse.Bend=(ambientPulse.From+ambientPulse.To)*.5f;ambientPulse.Cubic=ambientStop%20==0;
        ambientPulse.SetProgress(0);
    }
    private void TickAmbient(){
        if(!MotionEnabled || victory!=null || !Application.isPlaying)return;
        float now=Time.unscaledTime;
        if(ambientStop>0){
            float elapsed=now-ambientStarted;Vector2 point=Stops[(ambientStop-1)%20];
            float strength=Mathf.Sin(Mathf.Clamp01(elapsed/.65f)*Mathf.PI);
            mapThemes[(ambientStop-1)/20].SetVector("_Charge",new Vector4(point.x,point.y,strength,elapsed));
            // Travel the full connection, then carry the same signal to the next cleared cell.
            if(ambientPulse!=null)ambientPulse.SetProgress(Mathf.Clamp01((elapsed-.3f)/.8f));
            if(elapsed>1.1f){
                if(ambientStop<ambientEndStop && Completed(ambientStop+1) && VisibleStop(ambientStop+1)){
                    ambientStop++;BeginAmbientSegment(now);
                }else{StopAmbient();nextAmbientAt=now+3+(float)ambientRandom.NextDouble()*6;}
            }
            return;
        }
        if(now<nextAmbientAt)return;
        nextAmbientAt=now+3+(float)ambientRandom.NextDouble()*6;
        var candidates=new System.Collections.Generic.List<int>();int current=CurrentStop();
        for(int stop=1;stop<=stageButtons.Count;stop++)if((stop==current || Completed(stop)) && VisibleStop(stop))candidates.Add(stop);
        if(candidates.Count==0)return;
        int connected=candidates.Find(stop=>Completed(stop) && stop<SagaRoute.Count && Completed(stop+1) && VisibleStop(stop+1));
        ambientStop=connected>0?connected:candidates[ambientRandom.Next(candidates.Count)];
        ambientEndStop=ambientStop;
        while(Completed(ambientEndStop) && ambientEndStop<SagaRoute.Count && Completed(ambientEndStop+1) && VisibleStop(ambientEndStop+1))ambientEndStop++;
        BeginAmbientSegment(now);
    }
    private void CancelMapReturn(){if(victory!=null)StopCoroutine(victory);victory=null;runningMapReturn=false;ClearSynapseSignal();if(pulse!=null)Destroy(pulse.gameObject);pulse=null;}
    private void Close(){CancelMapReturn();StopAmbient();queuedMapStop=0;revealingStop=0;if(page!=null)page.SetActive(false);}
    private void Update(){
        if(page==null || !page.activeSelf)return;
        LayoutMap();
        TickAmbient();
        if(owner!=GameManager.AchievementUserId || Input.GetKeyDown(KeyCode.Escape))Close();
    }
    private void OnDestroy(){if(page!=null)Destroy(page);if(paintedHeader!=null)Destroy(paintedHeader);if(paintedHeaderTheme!=null)Destroy(paintedHeaderTheme);foreach(var theme in mapThemes)if(theme!=null)Destroy(theme);foreach(var theme in bossThemes)if(theme!=null)Destroy(theme);}
}
