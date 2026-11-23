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
    private int? first,second;
    private float wait;
    private bool disposed;
    public SagaPairsBoard(Transform parent,int[] sequence,UnityEngine.Events.UnityAction quit,Material theme,Func<int,int,bool> compare)
    {
        this.compare=compare;
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
        var content=all.First(t=>t.name=="Unused pairs recall").GetComponentInChildren<ScrollRect>(true).content;
        var prefab=Resources.Load<GameObject>("Prefabs/Pairs/btn/CardButton");
        foreach(int id in sequence.OrderBy(x=>UnityEngine.Random.value)){
            int captured=id;var card=UnityEngine.Object.Instantiate(prefab,content,false).GetComponent<Button>();
            card.name="Saga pair "+id;card.image.overrideSprite=Resources.Load<Sprite>("GamesResources/Pairs/M");
            card.onClick=new Button.ButtonClickedEvent();card.onClick.AddListener(()=>Choose(captured));cards.Add(id,card);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
        Canvas.ForceUpdateCanvases();
        var heart=Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall").GetComponentsInChildren<Image>(true).First(i=>i.name=="Exact heart 0").sprite;
        for(int i=0;i<3;i++){
            var image=ChainGame.Panel(Root.transform,"Saga pair heart "+i,new Vector2(.58f+i*.075f,.93f),new Vector2(.64f+i*.075f,.97f),Color.white).GetComponent<Image>();
            image.sprite=heart;image.preserveAspect=true;image.material=theme;lives.Add(image);
        }
    }
    public void Choose(int id){
        Button button;if(disposed || wait>0 || !cards.TryGetValue(id,out button) || !button.interactable)return;
        button.image.overrideSprite=CardFaces.Face(id);button.image.preserveAspect=true;button.interactable=false;
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
    public void ShowClock(int seconds){clock.text=seconds.ToString();}
    public void Hearts(int remaining){for(int i=0;i<lives.Count;i++)lives[i].gameObject.SetActive(i<remaining);}
    public void Destroy(){disposed=true;if(Root!=null){Root.SetActive(false);CardFaces.DestroyGenerated(Root);}}
}
