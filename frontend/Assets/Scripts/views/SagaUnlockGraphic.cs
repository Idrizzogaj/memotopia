using UnityEngine;
using UnityEngine.UI;

// A short electrical pulse runs along the existing axon, inside the scroll mask.
public sealed class SagaUnlockGraphic : MaskableGraphic
{
    public Vector2 From, To, Bend;
    public float Progress;
    public bool Cubic;
    public void SetProgress(float value){Progress=value;SetVerticesDirty();}
    Vector2 Point(float t){return Cubic?SagaAxonGraphic.RoutePoint(From,To,t,true):(1-t)*(1-t)*From+2*(1-t)*t*Bend+t*t*To;}
    protected override void OnPopulateMesh(VertexHelper mesh){
        mesh.Clear();float start=Mathf.Max(0,Progress-.28f);
        for(int i=0;i<24;i++){
            float a=Mathf.Lerp(start,Progress,i/24f),b=Mathf.Lerp(start,Progress,(i+1)/24f);
            Vector2 p=Point(a),q=Point(b);
            Vector2 normal=new Vector2(-(q-p).y,(q-p).x).normalized;
            float wave=Mathf.Sin(a*150+Progress*20)*3;
            p+=normal*wave;q+=normal*Mathf.Sin(b*150+Progress*20)*3;
            Line(mesh,p,q,10,new Color(.1f,.8f,1,.25f));
            Line(mesh,p,q,4,new Color(.4f,.95f,1,.9f));
            Line(mesh,p,q,1.5f,Color.white);
        }
    }
    static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color tint){
        Vector2 normal=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
        int first=mesh.currentVertCount;
        mesh.AddVert(a-normal,tint,Vector2.zero);mesh.AddVert(a+normal,tint,Vector2.zero);
        mesh.AddVert(b+normal,tint,Vector2.zero);mesh.AddVert(b-normal,tint,Vector2.zero);
        mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
    }
}
