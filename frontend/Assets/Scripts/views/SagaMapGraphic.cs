using UnityEngine;
using UnityEngine.UI;

// Positions trace Idriz's sketch from the bottom upwards, including its alternating bends.
[RequireComponent(typeof(CanvasRenderer))]
public class SagaMapGraphic : MaskableGraphic
{
    public int SelectedLevel;
    public static readonly Vector2[] Points = {
        new Vector2(.52f,.055f), new Vector2(.76f,.18f), new Vector2(.51f,.20f), new Vector2(.20f,.23f),
        new Vector2(.17f,.35f), new Vector2(.42f,.33f), new Vector2(.72f,.38f), new Vector2(.70f,.50f),
        new Vector2(.40f,.54f), new Vector2(.17f,.51f), new Vector2(.12f,.63f), new Vector2(.42f,.68f),
        new Vector2(.73f,.65f), new Vector2(.77f,.79f), new Vector2(.53f,.81f), new Vector2(.29f,.81f),
        new Vector2(.13f,.86f), new Vector2(.28f,.93f), new Vector2(.51f,.94f), new Vector2(.78f,.96f)
    };

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        var rect = rectTransform.rect;
        // Smooth horizontal tangents reproduce the hand-drawn winding route.
        for (int i = 1; i < Points.Length; i++)
        {
            Vector2 a = Position(Points[i - 1]), b = Position(Points[i]);
            float direction = b.x >= a.x ? 1 : -1;
            float reach = Mathf.Max(Mathf.Abs(b.x - a.x) * .45f, rect.width * .08f);
            Vector2 c = a + Vector2.right * direction * reach;
            Vector2 d = b - Vector2.right * direction * reach;
            Vector2 previous = a;
            var route=new Vector2[49];route[0]=a;
            for (int step = 1; step <= 48; step++)
            {
                float t = step / 48f, u = 1 - t;
                Vector2 next = u*u*u*a + 3*u*u*t*c + 3*u*t*t*d + t*t*t*b;
                route[step]=next;
                Stroke(mesh, previous, next, rect.width * .009f, ChainGame.Theme);
                previous = next;
            }
            // Rounded myelin segments wrap the thin axon, leaving gaps between sheaths.
            for(int first=4;first+5<48;first+=8){
                Vector2 start=route[first],end=route[first+5];
                if(Vector2.Distance(start,a)<rect.width*.045f || Vector2.Distance(end,b)<rect.width*.045f)continue;
                Capsule(mesh,start,end,rect.width*.014f,new Color32(209,158,66,255));
                Capsule(mesh,start,end,rect.width*.011f,new Color32(255,222,139,255));
                Vector2 normal=new Vector2(-(end-start).y,(end-start).x).normalized*rect.width*.004f;
                Capsule(mesh,start+normal,end+normal,rect.width*.003f,new Color32(255,247,210,255));
            }
        }
    }

    private Vector2 Position(Vector2 point)
    {
        var rect = rectTransform.rect;
        return new Vector2(rect.xMin + point.x * rect.width, rect.yMin + point.y * rect.height);
    }
    private static void Stroke(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tone)
    {
        Vector2 delta = b - a;
        Vector2 normal = new Vector2(-delta.y, delta.x).normalized * width * .5f;
        int start = mesh.currentVertCount;
        mesh.AddVert(a - normal, tone, Vector2.zero); mesh.AddVert(a + normal, tone, Vector2.zero);
        mesh.AddVert(b + normal, tone, Vector2.zero); mesh.AddVert(b - normal, tone, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
    }
    private static void Capsule(VertexHelper mesh,Vector2 a,Vector2 b,float radius,Color tone)
    {
        Stroke(mesh,a,b,radius*2,tone);
        foreach(Vector2 center in new[]{a,b}){
            for(int k=0;k<12;k++){
                float angle=k*Mathf.PI/6,next=(k+1)*Mathf.PI/6;
                int start=mesh.currentVertCount;
                mesh.AddVert(center,tone,Vector2.zero);
                mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tone,Vector2.zero);
                mesh.AddVert(center+new Vector2(Mathf.Cos(next),Mathf.Sin(next))*radius,tone,Vector2.zero);
                mesh.AddTriangle(start,start+1,start+2);
            }
        }
    }
}
