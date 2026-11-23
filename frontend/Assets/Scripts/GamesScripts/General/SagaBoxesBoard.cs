using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class SagaBoxesBoard
{
    public GameObject Root {get;private set;}
    private readonly SagaBoxesLayout layout;
    private readonly List<Image> pictures=new List<Image>();
    private readonly Dictionary<int,GameObject> tray=new Dictionary<int,GameObject>();
    private readonly Func<int,int,bool> place;
    private readonly Func<bool> ready;
    private readonly int[] sequence;
    private readonly GameObject board;
    private bool disposed;
    public SagaBoxesBoard(Transform parent,SagaLevel level,int[] sequence,UnityEngine.Events.UnityAction quit,UnityEngine.Events.UnityAction recall,Func<int,int,bool> place,Func<bool> ready)
    {
        this.place=place;this.ready=ready;this.sequence=(int[])sequence.Clone();
        var rectParent=(RectTransform)parent;
        bool tablet=Mathf.RoundToInt(rectParent.rect.width/rectParent.rect.height*4)==3;
        Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Saga/"+(tablet?"BoxesSceneIpad":"BoxesScene")),parent,false);
        var rect=(RectTransform)Root.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.anchoredPosition3D=Vector3.zero;
        layout=Root.GetComponent<SagaBoxesLayout>();
        foreach(var button in Root.GetComponentsInChildren<Button>(true))if(button.name=="Saga Quit")button.onClick.AddListener(quit);
        foreach(var item in Root.GetComponentsInChildren<Transform>(true))if(item.name=="HowToPlay" || item.name=="ReadySteadyGo")item.gameObject.SetActive(false);
        board=layout.Boards[level.Number-1];board.SetActive(true);
        var images=board.GetComponentsInChildren<Image>(true).Where(i=>i.name=="ImgPref").ToArray();
        if(images.Length<sequence.Length)throw new InvalidOperationException("Original Boxes layout has too few cells.");
        for(int i=0;i<images.Length;i++){
            if(i>=sequence.Length){images[i].transform.parent.gameObject.SetActive(false);continue;}
            var picture=images[i];picture.sprite=CardFaces.SmallFace(sequence[i]);picture.enabled=true;BoxesPicturePerspective.Attach(picture);pictures.Add(picture);
            var marker=picture.transform.parent.gameObject.AddComponent<DragAndDropCell>();marker.enabled=false;
            var drop=picture.transform.parent.gameObject.AddComponent<SagaBoxesDrop>();drop.Owner=this;drop.Slot=i;
        }
        var holder=new GameObject("Inactive tray staging");holder.transform.SetParent(Root.transform,false);holder.SetActive(false);
        foreach(int id in sequence.OrderBy(x=>UnityEngine.Random.value)){
            var item=UnityEngine.Object.Instantiate(layout.TrayItem,holder.transform,false);
            foreach(var cell in item.GetComponentsInChildren<DragAndDropCell>(true))cell.enabled=false;
            var picture=item.GetComponentsInChildren<Image>(true).Last();picture.name=id.ToString();picture.sprite=CardFaces.SmallFace(id);BoxesPicturePerspective.Attach(picture);
            if(picture.GetComponent<DragAndDropItem>()==null)picture.gameObject.AddComponent<DragAndDropItem>();
            item.transform.SetParent(layout.Tray,false);item.SetActive(false);tray.Add(id,item);
        }
        var recallButton=layout.RecallButton.GetComponent<Button>();recallButton.onClick=new Button.ButtonClickedEvent();recallButton.onClick.AddListener(recall);
        layout.RecallButton.SetActive(true);layout.Bottom.SetActive(false);Hearts(SagaLevel.Hearts);
        BoxesBoardFit.Fit(board,layout.RecallButton,layout.Header);
    }
    public void BeginRecall(){
        foreach(var picture in pictures)picture.enabled=false;
        layout.RecallButton.SetActive(false);layout.Bottom.SetActive(true);layout.Bottom.transform.position=layout.BottomTarget.transform.position;
        foreach(var item in tray.Values)item.SetActive(true);
        BoxesBoardFit.Fit(board,layout.Bottom,layout.Header);
    }
    public bool Place(int id,int slot){
        if(disposed || !ready() || !tray.ContainsKey(id) || slot<0 || slot>=pictures.Count)return false;
        bool correct=place(id,slot);if(disposed)return correct;
        if(correct){pictures[slot].enabled=true;tray[id].SetActive(false);}
        else if(Application.isPlaying)WrongFeedback.Mistake(pictures[slot].transform.parent.gameObject);
        return correct;
    }
    public void Tick(){
        if(disposed || DragAndDropItem.draggedItem!=null)return;
        foreach(var item in tray.Values)foreach(var image in item.GetComponentsInChildren<Image>(true))image.raycastTarget=true;
    }
    public void ShowClock(int seconds){layout.Clock.text=seconds.ToString();}
    public void Hearts(int remaining){for(int i=0;i<layout.Hearts.Length;i++)layout.Hearts[i].gameObject.SetActive(i<remaining);}
    public void Destroy(){disposed=true;if(Root!=null){Root.SetActive(false);CardFaces.DestroyGenerated(Root);}}
}
