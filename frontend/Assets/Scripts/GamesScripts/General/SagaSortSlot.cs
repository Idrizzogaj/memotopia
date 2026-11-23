using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SagaSortSlot : MonoBehaviour, IDropHandler
{
    public SagaSortBoard Owner;
    public int Slot;
    public void OnDrop(PointerEventData data){var item=DragAndDropItem.draggedItem;if(item==null || Owner==null)return;int id;if(int.TryParse(item.name,out id))Owner.Place(id,Slot);}
}
