using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Original Flash hierarchy, original animation controller, original image presenter.
// Uses instance callbacks instead of Flash's scene-wide static events.
public sealed class SagaFlashStudy
{
    public GameObject Root { get; private set; }
    public SagaFlashCell CurrentCell { get; private set; }
    public int ShownCount { get; private set; }
    public readonly List<int> ShownImages = new List<int>();
    private readonly int[] sequence;
    private readonly List<Transform> unused = new List<Transform>();
    private readonly List<Animator> animators = new List<Animator>();
    private readonly Action finished;
    private int position;
    private bool disposed, paused;
    private readonly float playbackSpeed;

    public SagaFlashStudy(Transform parent,int[] images,UnityEngine.Events.UnityAction quit,Material theme,Action finished,float playbackSpeed=1,int hearts=SagaLevel.Hearts)
    {
        sequence=(int[])images.Clone();this.finished=finished;this.playbackSpeed=playbackSpeed;
        Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall"),parent,false);
        var rect=(RectTransform)Root.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
        rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.anchoredPosition3D=Vector3.zero;
        var all=Root.GetComponentsInChildren<Transform>(true);
        foreach(var item in all){
            if(item.name=="Original countdown" || item.name=="HowToPlay" || item.name=="Exact recall grid")item.gameObject.SetActive(false);
            if(item.name=="Unused active cells" || item.name=="Unused inactive cells")item.gameObject.SetActive(true);
        }
        var header=(RectTransform)all.First(t=>t.name=="Exact recall header");
        header.anchoredPosition=((RectTransform)all.First(t=>t.name=="Exact recall header target")).anchoredPosition;
        foreach(var text in header.GetComponentsInChildren<Text>(true))if(text.name=="Exact recall timer" || text.text=="Timer")text.gameObject.SetActive(false);
        foreach(var button in Root.GetComponentsInChildren<Button>(true)){
            button.onClick=new Button.ButtonClickedEvent();
            if(button.name=="Exact Quit"){
                button.onClick.AddListener(quit);
                foreach(var image in button.GetComponentsInChildren<Image>(true))image.material=theme;
            }
        }
        for(int i=0;i<5;i++){
            var heart=all.FirstOrDefault(t=>t.name=="Exact heart "+i);
            if(heart==null)continue;heart.gameObject.SetActive(i<hearts);
            var image=heart.GetComponent<Image>();if(image!=null)image.material=theme;
        }
        var cells=all.First(t=>t.name=="Unused active cells");
        foreach(Transform cell in cells){CellController.HideFace(cell);unused.Add(cell);}
        if(unused.Count==0)throw new InvalidOperationException("Original Flash study grid is empty.");
        // The exported native board has 24 cells. Higher native Flash levels
        // reuse a location with a clean copy, never omit a picture.
        while(unused.Count<sequence.Length){
            var extra=UnityEngine.Object.Instantiate(unused[0].gameObject,cells,false);
            extra.name="Extra native Flash cell";CellController.HideFace(extra.transform);unused.Add(extra.transform);
        }
        Advance();
    }

    private void Advance()
    {
        if(disposed)return;
        if(position==sequence.Length){CurrentCell=null;finished();return;}
        int choice=UnityEngine.Random.Range(0,unused.Count);var cell=unused[choice];unused.RemoveAt(choice);
        cell.SetAsLastSibling();
        CurrentCell=cell.gameObject.AddComponent<SagaFlashCell>();
        CurrentCell.Configure(this,sequence[position++]);
        var animator=cell.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController=Resources.Load<RuntimeAnimatorController>("Prefabs/Flash/FlashCellScale");
        if(animator.runtimeAnimatorController==null)throw new InvalidOperationException("Original Flash animation controller is missing.");
        animator.speed=paused?0:playbackSpeed;animators.Add(animator);
        if(Application.isPlaying)animator.SetTrigger("Play");
        else CurrentCell.ShowImage();
    }

    public bool Accepts(SagaFlashCell cell) { return !disposed && !paused && CurrentCell==cell; }
    public void DidShow(int image) { ShownCount++;ShownImages.Add(image); }
    public void FinishedCell(SagaFlashCell cell) { if(Accepts(cell))Advance(); }
    public void SetPaused(bool value) { paused=value;foreach(var animator in animators)if(animator!=null)animator.speed=value?0:playbackSpeed; }
    public void Destroy()
    {
        disposed=true;CurrentCell=null;
        if(Root!=null){Root.SetActive(false);CardFaces.DestroyGenerated(Root);}
    }
}
