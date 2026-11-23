using UnityEngine;
using UnityEngine.UI;

// Quiet, static neural background. Decorative motion lives only on the map cells.
public sealed class SagaBackdropGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh){
        mesh.Clear();Rect r=rectTransform.rect;
        for(int row=0;row<12;row++){
            float y=row/12f,next=(row+1)/12f;
            Color a=Color.Lerp(new Color32(233,228,249,255),new Color32(222,245,243,255),y);
            Color b=Color.Lerp(new Color32(233,228,249,255),new Color32(222,245,243,255),next);
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector2(r.xMin,r.yMin+r.height*y),a,Vector2.zero);mesh.AddVert(new Vector2(r.xMax,r.yMin+r.height*y),a,Vector2.zero);
            mesh.AddVert(new Vector2(r.xMax,r.yMin+r.height*next),b,Vector2.zero);mesh.AddVert(new Vector2(r.xMin,r.yMin+r.height*next),b,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
        var random=new System.Random(517);var points=new Vector2[34];
        for(int i=0;i<points.Length;i++)points[i]=new Vector2(r.xMin+(float)random.NextDouble()*r.width,r.yMin+(float)random.NextDouble()*r.height);
        for(int i=0;i<points.Length;i++)for(int j=i+1;j<points.Length;j++)if(Vector2.Distance(points[i],points[j])<r.width*.25f){
            Vector2 a=points[i],b=points[j],normal=new Vector2(-(b-a).y,(b-a).x).normalized*.6f;int n=mesh.currentVertCount;Color tint=new Color(.15f,.55f,.61f,.07f);
            mesh.AddVert(a-normal,tint,Vector2.zero);mesh.AddVert(a+normal,tint,Vector2.zero);mesh.AddVert(b+normal,tint,Vector2.zero);mesh.AddVert(b-normal,tint,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
    }
}
