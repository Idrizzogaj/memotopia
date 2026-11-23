using UnityEngine;

// Receives the unchanged ShowImage / HideImage / AnimationFinished clip events.
public sealed class SagaFlashCell : MonoBehaviour
{
    private SagaFlashStudy owner;
    private int imageNumber;
    private bool shown,completed;
    public void Configure(SagaFlashStudy value,int image) { owner=value;imageNumber=image; }
    public void ShowImage()
    {
        if(owner==null || !owner.Accepts(this) || completed || shown)return;
        shown=true;CellController.PresentImage(transform,imageNumber);owner.DidShow(imageNumber);
    }
    public void HideImage()
    {
        if(owner==null || !owner.Accepts(this) || completed)return;
        CellController.HideFace(transform);
    }
    public void AnimationFinished()
    {
        if(owner==null || !owner.Accepts(this) || completed)return;
        completed=true;owner.FinishedCell(this);
    }
}
