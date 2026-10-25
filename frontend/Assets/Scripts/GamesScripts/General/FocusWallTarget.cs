using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Broad overlapping edge strips resolve corners by the nearest physical wall.
public sealed class FocusWallTarget : MonoBehaviour,IPointerDownHandler {
 public Action<Vector2> Tap;
 public void OnPointerDown(PointerEventData pointer){if(Tap!=null)Tap(pointer.position);}
}
