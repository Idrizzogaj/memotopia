using UnityEngine;
using UnityEngine.UI;

// One compact, filled lightning bolt shared by Memory and Challenge. Focus keeps its old placeholder.
public sealed class TrainingFlashSymbol : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        var bounds=rectTransform.rect;
        var points=new[]{new Vector2(.60f,.82f),new Vector2(.29f,.46f),new Vector2(.48f,.46f),new Vector2(.40f,.18f),new Vector2(.71f,.56f),new Vector2(.52f,.56f)};
        foreach(var point in points)mesh.AddVert(new Vector3(bounds.xMin+point.x*bounds.width,bounds.yMin+point.y*bounds.height),color,Vector2.zero);
        mesh.AddTriangle(0,1,5);mesh.AddTriangle(1,2,5);mesh.AddTriangle(2,4,5);mesh.AddTriangle(2,3,4);
    }
}
