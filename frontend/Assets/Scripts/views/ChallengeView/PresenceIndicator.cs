using UnityEngine;
using UnityEngine.UI;

// Game-presence badge, not the phone's Wi-Fi connection. Hidden status is never disclosed.
public sealed class PresenceIndicator : MaskableGraphic {
 public static void Set(Text label,bool online){
  var child=label.transform.Find("Presence wifi");PresenceIndicator icon;
  if(child==null){var go=new GameObject("Presence wifi",typeof(RectTransform),typeof(CanvasRenderer),typeof(PresenceIndicator));go.transform.SetParent(label.transform,false);icon=go.GetComponent<PresenceIndicator>();}
  else icon=child.GetComponent<PresenceIndicator>();
  float size=Mathf.Clamp(label.fontSize,22,30);var rect=icon.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.sizeDelta=new Vector2(size,size);rect.anchoredPosition=new Vector2(size*.5f,0);
  icon.raycastTarget=false;icon.color=online?(Color)new Color32(28,145,89,255):(Color)new Color32(202,48,75,255);icon.SetVerticesDirty();
 }
 protected override void OnPopulateMesh(VertexHelper mesh){
  mesh.Clear();var rect=GetPixelAdjustedRect();float size=Mathf.Min(rect.width,rect.height);Vector2 centre=rect.center+Vector2.down*size*.28f;
  foreach(float radius in new[]{.24f,.44f,.64f})for(int i=0;i<16;i++){
   float a=(45+i*90f/16)*Mathf.Deg2Rad,b=(45+(i+1)*90f/16)*Mathf.Deg2Rad;
   Vector2 x=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),y=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
   Quad(mesh,centre+x*size*(radius-.035f),centre+x*size*(radius+.035f),centre+y*size*(radius+.035f),centre+y*size*(radius-.035f));
  }
  for(int i=0;i<16;i++){
   float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;int n=mesh.currentVertCount;
   Vertex(mesh,centre);Vertex(mesh,centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size*.07f);Vertex(mesh,centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size*.07f);mesh.AddTriangle(n,n+1,n+2);
  }
 }
 private void Vertex(VertexHelper mesh,Vector2 point){var vertex=UIVertex.simpleVert;vertex.position=point;vertex.color=color;mesh.AddVert(vertex);}
 private void Quad(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d){int n=mesh.currentVertCount;Vertex(mesh,a);Vertex(mesh,b);Vertex(mesh,c);Vertex(mesh,d);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);}
}
