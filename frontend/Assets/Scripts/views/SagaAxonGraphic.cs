using UnityEngine;
using UnityEngine.UI;

// Reuse the painted myelin from the approved map across section boundaries.
public sealed class SagaAxonGraphic : MaskableGraphic
{
    public Texture Source;
    public Vector2 From,To,GateCenter;
    public float UnitWidth,EndPadding,GateWidth;
    public bool HasGate;
    public override Texture mainTexture {get{return Source!=null?Source:s_WhiteTexture;}}
    const int Samples=64;
    readonly Vector2[] points=new Vector2[Samples+1];
    readonly float[] lengths=new float[Samples+1];
    public static Vector2 RoutePoint(Vector2 from,Vector2 to,float t,bool curved){
        if(!curved)return Vector2.Lerp(from,to,t);
        float height=(to.y-from.y)*.4f,u=1-t;
        Vector2 a=from+Vector2.up*height,b=to-Vector2.up*height;
        return u*u*u*from+3*u*u*t*a+3*u*t*t*b+t*t*t*to;
    }
    protected override void OnPopulateMesh(VertexHelper mesh){
        mesh.Clear();if(Source==null || UnitWidth<=0)return;
        points[0]=From;lengths[0]=0;
        for(int i=1;i<=Samples;i++){points[i]=RoutePoint(From,To,i/(float)Samples,true);lengths[i]=lengths[i-1]+Vector2.Distance(points[i-1],points[i]);}
        float total=lengths[Samples];if(total<=EndPadding*2)return;
        int count=Mathf.Max(1,Mathf.CeilToInt((total-EndPadding*2)/UnitWidth));
        float step=(total-EndPadding*2)/count;
        Vector2 gateAxis=(RoutePoint(From,To,.58f,true)-RoutePoint(From,To,.56f,true)).normalized;
        // One existing gold sleeve, in the source map's pixel coordinates.
        var uv=new Rect(322f/1024f,475f/1536f,43f/1024f,33f/1536f);
        for(int n=0;n<count;n++){
            Vector2 a=At(EndPadding+n*step),b=At(EndPadding+(n+1)*step);
            Vector2 tangent=(b-a).normalized,center=(a+b)*.5f;
            float half=Vector2.Distance(a,b)*.53f;
            if(HasGate && Mathf.Abs(Vector2.Dot(center-GateCenter,gateAxis))<GateWidth*.28f+half)continue;
            Vector2 across=new Vector2(-tangent.y,tangent.x)*UnitWidth*(33f/43f)*.5f;
            Vector2 along=tangent*half;int first=mesh.currentVertCount;
            mesh.AddVert(center-along-across,color,new Vector2(uv.xMin,uv.yMin));
            mesh.AddVert(center-along+across,color,new Vector2(uv.xMin,uv.yMax));
            mesh.AddVert(center+along+across,color,new Vector2(uv.xMax,uv.yMax));
            mesh.AddVert(center+along-across,color,new Vector2(uv.xMax,uv.yMin));
            mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
        }
    }
    Vector2 At(float distance){
        int i=1;while(i<Samples && lengths[i]<distance)i++;
        return Vector2.Lerp(points[i-1],points[i],Mathf.InverseLerp(lengths[i-1],lengths[i],distance));
    }
}
