using UnityEngine;
using UnityEngine.EventSystems;

// Only the detail overlay handles these gestures; the collection keeps vertical scrolling.
public class AchievementDetailGestures : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public AchievementsView view;
    public RectTransform card;
    private Vector2 start;
    private bool dragged;

    public void OnPointerDown(PointerEventData data) { dragged = false; }
    public void OnBeginDrag(PointerEventData data) { start = data.pressPosition; dragged = true; }
    public void OnDrag(PointerEventData data) { }
    public void OnEndDrag(PointerEventData data)
    {
        Vector2 delta = data.position - start;
        float threshold = Mathf.Max(40, Screen.width * .06f);
        if (Mathf.Abs(delta.x) >= threshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y) * 1.25f)
            view.MoveDetails(delta.x < 0 ? 1 : -1);
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (!dragged && card != null && !RectTransformUtility.RectangleContainsScreenPoint(card, data.position, data.pressEventCamera))
            view.OnClosePopupClick();
        dragged = false;
    }
    private void OnDisable() { dragged = false; }
}
