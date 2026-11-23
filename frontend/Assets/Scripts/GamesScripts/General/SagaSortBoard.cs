using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Saga-only order reconstruction, using the existing card and drag/drop components.
public sealed class SagaSortBoard
{
    public GameObject Root {get{return view.Root;}}
    readonly ChainOriginalPresentation view;
    readonly int[] sequence;
    readonly Func<int[],bool> submit;
    readonly Func<bool> ready;
    readonly List<Button> slots=new List<Button>();
    readonly List<Text> numbers=new List<Text>();
    int[] current;
    float shake;
    Vector3 restPosition;
    bool recall,disposed;
    public int[] CurrentOrder {get{return (int[])current.Clone();}}
    public SagaSortBoard(Transform parent,int[] sequence,UnityEngine.Events.UnityAction quit,Material theme,Func<int[],bool> submit,Func<bool> ready){
        this.sequence=(int[])sequence.Clone();this.submit=submit;this.ready=ready;
        view=new ChainOriginalPresentation(parent,false,quit,()=>{},theme);
        var grid=view.Grid;grid.anchorMin=new Vector2(.06f,.17f);grid.anchorMax=new Vector2(.94f,.82f);grid.offsetMin=grid.offsetMax=Vector2.zero;
        for(int i=0;i<sequence.Length;i++){
            var button=UnityEngine.Object.Instantiate(view.Cell,grid,false);button.name="Sort slot "+i;button.gameObject.SetActive(true);button.transition=Selectable.Transition.None;button.interactable=true;
            button.image.overrideSprite=CardFaces.SmallFace(sequence[i]);button.image.preserveAspect=true;button.onClick=new Button.ButtonClickedEvent();slots.Add(button);
            var drop=button.gameObject.AddComponent<SagaSortSlot>();drop.Owner=this;drop.Slot=i;
            var label=ChainGame.Label(button.transform,(i+1).ToString(),new Vector2(.02f,.01f),new Vector2(.30f,.22f),18,ChainGame.Ink);label.raycastTarget=false;numbers.Add(label);
        }
        FitGrid(grid,5,sequence.Length);
    }
    static void FitGrid(RectTransform grid,int columns,int count){
        Canvas.ForceUpdateCanvases();var layout=grid.GetComponent<GridLayoutGroup>()??grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.enabled=true;layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=columns;layout.spacing=new Vector2(12,12);layout.childAlignment=TextAnchor.MiddleCenter;
        float side=Mathf.Min((grid.rect.width-12*(columns-1))/columns,(grid.rect.height-12*(Mathf.CeilToInt(count/(float)columns)-1))/Mathf.Max(1,Mathf.CeilToInt(count/(float)columns)));
        layout.cellSize=new Vector2(side,side);LayoutRebuilder.ForceRebuildLayoutImmediate(grid);
    }
    public void BeginRecall(){
        if(disposed || recall)return;recall=true;
        // Keep exactly the study grid and card positions. Only identities change.
        current=sequence.OrderBy(x=>UnityEngine.Random.value).ToArray();if(current.SequenceEqual(sequence))current=sequence.Skip(1).Concat(sequence.Take(1)).ToArray();
        for(int i=0;i<slots.Count;i++){slots[i].gameObject.AddComponent<DragAndDropItem>();RefreshCard(i);}
        var nativeNext=Resources.Load<GameObject>("Prefabs/ExactPairsChainStudy").GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next");
        var done=UnityEngine.Object.Instantiate(nativeNext,Root.transform,false);done.name="Sort DONE";done.gameObject.SetActive(true);done.onClick=new Button.ButtonClickedEvent();done.onClick.AddListener(Submit);
        var rect=(RectTransform)done.transform;rect.anchorMin=new Vector2(.30f,.045f);rect.anchorMax=new Vector2(.70f,.12f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        foreach(var text in done.GetComponentsInChildren<Text>(true))text.text="DONE";
        restPosition=((RectTransform)Root.transform).anchoredPosition3D;
    }
    public void Place(int id,int slot){
        if(disposed || !recall || !ready() || shake>0 || slot<0 || slot>=sequence.Length)return;
        int source=Array.IndexOf(current,id);if(source<0 || source==slot)return;
        int displaced=current[slot];current[slot]=id;current[source]=displaced;RefreshCard(source);RefreshCard(slot);
    }
    void RefreshCard(int slot){slots[slot].name=current[slot].ToString();slots[slot].image.overrideSprite=CardFaces.SmallFace(current[slot]);slots[slot].image.type=Image.Type.Simple;slots[slot].image.fillAmount=1;slots[slot].image.raycastTarget=true;}
    public void Submit(){if(disposed || !recall || !ready() || shake>0)return;bool correct=submit((int[])current.Clone());if(!disposed && !correct)shake=.4f;}
    public void Tick(float elapsed=-1){
        if(disposed)return;
        if(elapsed<0)elapsed=Time.unscaledDeltaTime;
        if(shake>0){shake=Mathf.Max(0,shake-elapsed);((RectTransform)Root.transform).anchoredPosition3D=restPosition+new Vector3(Mathf.Sin(shake*65)*8*(shake/.4f),0,0);}
        else if(recall)((RectTransform)Root.transform).anchoredPosition3D=restPosition;
        if(DragAndDropItem.draggedItem==null)foreach(var slot in slots)slot.image.raycastTarget=true;
    }
    public void ShowClock(int seconds){view.ShowRecallClock(seconds,false);}
    public void Hearts(int hearts){view.Hearts(hearts);}
    public void Destroy(){disposed=true;view.Destroy();}
}
