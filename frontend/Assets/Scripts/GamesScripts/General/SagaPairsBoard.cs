using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class SagaPairsBoard
{
    public GameObject Root {get;private set;}
    private readonly Dictionary<int,Button> cards=new Dictionary<int,Button>();
    private readonly List<Image> lives=new List<Image>();
    private readonly Func<int,int,bool> compare;
    private readonly Text clock;
    private readonly Func<int,Sprite> face;
    private readonly Func<bool> ready;
    private readonly RectTransform content;
    private bool studying;
    private readonly List<Text> studyLabels=new List<Text>();
    private int? first,second;
    private float wait;
    private bool disposed;
    public SagaPairsBoard(Transform parent,int[] sequence,UnityEngine.Events.UnityAction quit,Material theme,Func<int,int,bool> compare,bool study=false,Func<int,Sprite> face=null,Func<bool> ready=null,int maximumHearts=3)
    {
        this.compare=compare;this.face=face??new Func<int,Sprite>(CardFaces.Face);this.ready=ready;studying=study;
        Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ExactPairsChainStudy"),parent,false);
        var rect=(RectTransform)Root.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.anchoredPosition3D=Vector3.zero;
        var all=Root.GetComponentsInChildren<Transform>(true);
        foreach(var t in all){
            if(t.name=="Exact study screen" || t.name=="Unused tablet study" || t.name=="Original countdown" || t.name=="HowToPlay")t.gameObject.SetActive(false);
            if(t.name=="Unused pairs recall")t.gameObject.SetActive(true);
        }
        foreach(var button in Root.GetComponentsInChildren<Button>(true)){button.onClick=new Button.ButtonClickedEvent();if(button.name=="Exact Quit")button.onClick.AddListener(quit);}
        clock=Root.GetComponentsInChildren<Text>(true).First(t=>t.name=="SecondScreenTimer");
        // The native scene retains an inactive legacy "CardContent" object.
        // Use the actual live ScrollRect content reference, as PairsManager does.
        content=all.First(t=>t.name=="Unused pairs recall").GetComponentInChildren<ScrollRect>(true).content;
        var prefab=Resources.Load<GameObject>("Prefabs/Pairs/btn/CardButton");
        foreach(int id in study?sequence:sequence.OrderBy(x=>UnityEngine.Random.value).ToArray()){
            int captured=id;var card=UnityEngine.Object.Instantiate(prefab,content,false).GetComponent<Button>();
            card.name="Saga pair "+id;card.image.overrideSprite=study?this.face(id):Resources.Load<Sprite>("GamesResources/Pairs/M");
            card.image.preserveAspect=true;card.interactable=!study;
            card.onClick=new Button.ButtonClickedEvent();card.onClick.AddListener(()=>Choose(captured));cards.Add(id,card);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
        Canvas.ForceUpdateCanvases();
        var heart=Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall").GetComponentsInChildren<Image>(true).First(i=>i.name=="Exact heart 0").sprite;
        for(int i=0;i<maximumHearts;i++){
            var image=ChainGame.Panel(Root.transform,"Saga pair heart "+i,new Vector2(.58f+i*.075f,.93f),new Vector2(.64f+i*.075f,.97f),Color.white).GetComponent<Image>();
            image.sprite=heart;image.preserveAspect=true;image.material=theme;lives.Add(image);
        }
    }
    public void Choose(int id){
        Button button;if(disposed || studying || (ready!=null && !ready()) || wait>0 || !cards.TryGetValue(id,out button) || !button.interactable)return;
        button.image.overrideSprite=face(id);button.image.preserveAspect=true;button.interactable=false;
        var animator=button.GetComponent<Animator>();if(animator!=null && Application.isPlaying)animator.SetTrigger("Pressed");
        if(!first.HasValue){first=id;return;}
        second=id;bool correct=compare(first.Value,id);if(disposed)return;
        if(correct){Hide(cards[first.Value]);Hide(button);first=second=null;}
        else wait=.5f;
    }
    private static void Hide(Button button){var group=button.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;}
    public void Tick(float elapsed){
        if(wait<=0 || disposed)return;wait=Mathf.Max(0,wait-elapsed);if(wait>0)return;
        foreach(int id in new[]{first.Value,second.Value}){cards[id].image.overrideSprite=Resources.Load<Sprite>("GamesResources/Pairs/M");cards[id].interactable=true;}
        first=second=null;
    }
    public void FitAll(int columns){
        Canvas.ForceUpdateCanvases();var scroll=content.GetComponentInParent<ScrollRect>();
        var viewport=scroll.viewport!=null?scroll.viewport:(RectTransform)scroll.transform;
        var layout=content.GetComponent<GridLayoutGroup>();
        layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=columns;layout.spacing=new Vector2(10,10);
        int rows=Mathf.CeilToInt(cards.Count/(float)columns);
        float width=viewport.rect.width-layout.padding.horizontal,height=viewport.rect.height-layout.padding.vertical;
        float side=Mathf.Min((width-(columns-1)*10)/columns,(height-(rows-1)*10)/rows);
        layout.cellSize=new Vector2(side,side);LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
    public void LabelStudyPairs(){
        int index=0;foreach(var pair in cards){var label=ChainGame.Label(pair.Value.transform,(index++/2+1).ToString(),new Vector2(.03f,.03f),new Vector2(.25f,.23f),18,ChainGame.Ink);label.raycastTarget=false;studyLabels.Add(label);}
    }
    public void BeginRecall(bool shuffle){
        if(disposed || !studying)return;studying=false;
        foreach(var label in studyLabels)if(label!=null)CardFaces.DestroyGenerated(label.gameObject);studyLabels.Clear();
        IEnumerable<int> order=cards.Keys;if(shuffle)order=order.OrderBy(x=>UnityEngine.Random.value);
        foreach(int id in order){var card=cards[id];card.image.overrideSprite=Resources.Load<Sprite>("GamesResources/Pairs/M");card.interactable=true;if(shuffle)card.transform.SetAsLastSibling();}
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
    public void ShowClock(int seconds){clock.text=seconds.ToString();}
    public void Hearts(int remaining){for(int i=0;i<lives.Count;i++)lives[i].gameObject.SetActive(i<remaining);}
    public void Destroy(){disposed=true;if(Root!=null){Root.SetActive(false);CardFaces.DestroyGenerated(Root);}}
}
