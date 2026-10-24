using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Rasterise the existing approved symbols for the browser mockup, without redrawing them.
public static class FocusMockupAssets {
 public static void Export(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
  string output=Path.GetFullPath("../Focus21Review");Directory.CreateDirectory(output);
  for(int kind=0;kind<4;kind++){
   var root=new GameObject("Original symbol export",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(240,200);
   var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
   var cameraObject=new GameObject("Export camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.orthographicSize=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
   TrainingGameSymbol.Draw(rect,kind,Color.white);
   Canvas.ForceUpdateCanvases();var target=new RenderTexture(240,200,24);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
   var image=new Texture2D(240,200,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,240,200),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,new[]{"pairs","boxes","flash","chain"}[kind]+"-original.png"),image.EncodeToPNG());
   RenderTexture.active=old;camera.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(target);Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);
  }
  Debug.Log("FOCUS_ORIGINAL_SYMBOLS_EXPORTED");
 }
}
