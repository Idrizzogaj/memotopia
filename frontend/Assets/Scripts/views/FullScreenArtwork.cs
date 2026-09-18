using UnityEngine;
using UnityEngine.UI;

// Stretch background geometry to the canvas edges without moving its child controls.
[RequireComponent(typeof(Image))]
public class FullScreenArtwork : BaseMeshEffect
{
    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || mesh.currentVertCount == 0 || graphic.canvas == null) return;
        var canvas = (RectTransform)graphic.canvas.rootCanvas.transform;
        var rect = graphic.rectTransform;
        var source = rect.rect;
        if (source.width <= 0 || source.height <= 0) return;
        Vector3 minimum = rect.InverseTransformPoint(canvas.TransformPoint(canvas.rect.min));
        Vector3 maximum = rect.InverseTransformPoint(canvas.TransformPoint(canvas.rect.max));
        UIVertex vertex = new UIVertex();
        for (int i = 0; i < mesh.currentVertCount; i++)
        {
            mesh.PopulateUIVertex(ref vertex, i);
            vertex.position = new Vector3(
                vertex.position.x, // Preserve horizontal movement when Home slides to reveal the side menu.
                Mathf.LerpUnclamped(minimum.y, maximum.y, (vertex.position.y - source.yMin) / source.height), vertex.position.z);
            mesh.SetUIVertex(vertex, i);
        }
    }
}
