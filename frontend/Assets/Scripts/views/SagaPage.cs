using UnityEngine;
using UnityEngine.UI;

// Saga reuses Chain and keeps its own local progress, independent of Training.
public class SagaPage : MonoBehaviour
{
    private GameObject page;
    private Text selection;
    private ChainGame chain;
    private int owner;
    private SaysGame says;
    private readonly System.Collections.Generic.List<Button> stageButtons = new System.Collections.Generic.List<Button>();
    private Button previous,next;
    private int chapter;
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
        if(menu.statisticsBackground!=null)Place(menu.statisticsBackground.rectTransform,.578f,.734f);
        Place(training, .406f, .562f);
        Place(menu.challengeBackground.rectTransform, .234f, .390f);
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
    }

    private static void Place(RectTransform rect, float bottom, float top)
    {
        rect.anchorMin = new Vector2(.049f, bottom);
        rect.anchorMax = new Vector2(.950f, top);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void Open(MenuNavigation menu)
    {
        owner=GameManager.AchievementUserId;
        if(page!=null){ShowMap();return;}
        var canvas=menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas;
        var area=canvas.GetComponent<MobileSafeArea>();
        Transform parent=area!=null && area.Content!=null?area.Content:canvas.transform;
        page=ChainGame.Panel(parent,"Saga map",Vector2.zero,Vector2.one,Color.white);
        var frame=ChainGame.Panel(page.transform,"Approved map frame",new Vector2(.02f,.10f),new Vector2(.98f,.99f),Color.white);
        frame.GetComponent<Image>().raycastTarget=false;
        var art=ChainGame.Panel(frame.transform,"Approved map 1-20",Vector2.zero,Vector2.one,Color.white);
        var image=art.GetComponent<Image>();image.sprite=Resources.Load<Sprite>("Saga/Map20");image.raycastTarget=false;
        var fit=art.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio=image.sprite.rect.width/image.sprite.rect.height;
        // The painted Back area and nuclei have invisible native button hit targets.
        var back=ChainGame.Panel(art.transform,"Back",new Vector2(.01f,.945f),new Vector2(.20f,1),Color.clear);
        back.AddComponent<Button>().onClick.AddListener(Close);
        for(int i=0;i<Stops.Length;i++){
            int local=i+1;Vector2 point=Stops[i];
            var node=ChainGame.Panel(art.transform,"Saga level "+local,point-new Vector2(.065f,.04f),point+new Vector2(.065f,.04f),Color.clear);
            var button=node.AddComponent<Button>();button.transition=Selectable.Transition.None;
            button.onClick.AddListener(()=>SelectLevel(local));stageButtons.Add(button);
        }
        previous=ChainGame.Button(page.transform,"<",new Vector2(.18f,.025f),new Vector2(.32f,.085f),()=>ChangeChapter(-1));
        next=ChainGame.Button(page.transform,">",new Vector2(.68f,.025f),new Vector2(.82f,.085f),()=>ChangeChapter(1));
        selection=ChainGame.Label(page.transform,"",new Vector2(.34f,.025f),new Vector2(.66f,.085f),28,ChainGame.Theme);
        RefreshProgress();
    }
    private void ChangeChapter(int delta){
        int target=Mathf.Clamp(chapter+delta,0,(SagaRoute.Count-1)/20);
        if(!SagaRoute.Unlocked(target*20+1))return;
        chapter=target;RefreshProgress();
    }
    private void SelectLevel(int local){PlayStage(chapter*20+local);}
    private void PlayStage(int stop){
        if(!SagaRoute.Unlocked(stop) || chain.IsSagaOpen || says.IsSagaOpen)return;
        chapter=(stop-1)/20;var stage=SagaRoute.At(stop);
        page.SetActive(false);
        if(stage.Game==SagaGame.Says)says.OpenSaga(stage.Number,ShowMap,()=>{says.Close();PlayStage(stop+1);});
        else chain.OpenSaga(stage.Game,stage.Number,ShowMap,following=>PlayStage(SagaRoute.Stop(following.Game,following.Number)));
        if(!chain.IsSagaOpen && !says.IsSagaOpen)ShowMap();
    }
    private void ShowMap(){
        if(page==null || owner!=GameManager.AchievementUserId)return;
        page.SetActive(true);page.transform.SetAsLastSibling();RefreshProgress();
    }
    private void RefreshProgress(){
        for(int i=0;i<stageButtons.Count;i++)stageButtons[i].interactable=SagaRoute.Unlocked(chapter*20+i+1);
        selection.text=(chapter+1)+" / "+Mathf.CeilToInt(SagaRoute.Count/20f);
        previous.interactable=chapter>0;
        next.interactable=(chapter+1)*20<SagaRoute.Count && SagaRoute.Unlocked((chapter+1)*20+1);
    }
    private void Close(){if(page!=null)page.SetActive(false);}
    private void Update(){
        if(page==null || !page.activeSelf)return;
        if(owner!=GameManager.AchievementUserId || Input.GetKeyDown(KeyCode.Escape))Close();
    }
    private void OnDestroy(){if(page!=null)Destroy(page);}
}
