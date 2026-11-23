using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SagaBoxesDrop : MonoBehaviour, IDropHandler
{
    public SagaBoxesBoard Owner;
    public int Slot;
    public void OnDrop(PointerEventData data)
    {
        var item=DragAndDropItem.draggedItem;int id;
        if(Owner!=null && item!=null && int.TryParse(item.name,out id))Owner.Place(id,Slot);
    }
}
