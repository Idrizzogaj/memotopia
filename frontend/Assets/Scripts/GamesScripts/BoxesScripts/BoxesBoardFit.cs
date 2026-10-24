using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Preserve the original board and Recall control; reserve non-overlapping space
// when the shorter tablet viewport cannot contain the fixed-size phone board.
public static class BoxesBoardFit {
 public static Bounds RectBounds(RectTransform item,RectTransform space){
  var corners=new Vector3[4];item.GetWorldCorners(corners);var bounds=new Bounds(space.InverseTransformPoint(corners[0]),Vector3.zero);
  for(int i=1;i<4;i++)bounds.Encapsulate(space.InverseTransformPoint(corners[i]));return bounds;
 }
 public static Bounds BoardBounds(GameObject panel,RectTransform space){
  var cells=panel.GetComponentsInChildren<DragAndDropCell>(true).Where(c=>c.gameObject.activeInHierarchy).ToArray();
  var bounds=RectBounds((RectTransform)cells[0].transform,space);foreach(var cell in cells.Skip(1))bounds.Encapsulate(RectBounds((RectTransform)cell.transform,space));return bounds;
 }
 public static void Fit(GameObject panel,GameObject recall,GameObject header){
  if(panel==null || recall==null || header==null || !panel.GetComponentsInChildren<DragAndDropCell>(true).Any(c=>c.gameObject.activeInHierarchy))return;
  Canvas.ForceUpdateCanvases();var canvas=panel.GetComponentInParent<Canvas>().rootCanvas;var space=(RectTransform)canvas.transform;
  var board=BoardBounds(panel,space);var button=RectBounds((RectTransform)recall.transform,space);var top=RectBounds((RectTransform)header.transform,space);float gap=space.rect.width*.02f;
  float lower=Mathf.Max(space.rect.yMin+gap,button.max.y+gap),upper=Mathf.Min(space.rect.yMax-gap,top.min.y-gap);
  if(upper-lower<100 || board.size.y<=0 || board.size.x<=0)return;
  float scale=Mathf.Min(1,(upper-lower)/board.size.y,(space.rect.width-2*gap)/board.size.x);var rect=(RectTransform)panel.transform;
  if(scale<.999f){rect.localScale*=scale;Canvas.ForceUpdateCanvases();board=BoardBounds(panel,space);}
  float y=Mathf.Clamp(0,lower-board.min.y,upper-board.max.y);float x=Mathf.Clamp(0,space.rect.xMin+gap-board.min.x,space.rect.xMax-gap-board.max.x);
  rect.position+=space.TransformVector(new Vector3(x,y,0));
 }
}
