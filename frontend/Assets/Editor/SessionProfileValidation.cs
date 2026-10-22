using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
public static class SessionProfileValidation {
 public static void Review(){
  ChainReuseValidation.ReviewAll();
  var root=new GameObject("Selector preview",typeof(RectTransform),typeof(Canvas));
  var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,170);
  var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
  var camera=new GameObject("Preview camera",typeof(Camera));var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.transform.position=new Vector3(0,0,-20);cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.white;
  var prior=CardFaces.Selected;
  try{
   var bar=root.AddComponent<ContentSelectorBar>();var flags=BindingFlags.NonPublic|BindingFlags.Instance;
   typeof(ContentSelectorBar).GetField("font",flags).SetValue(bar,Resources.GetBuiltinResource<Font>(Application.unityVersion.StartsWith("2019")?"Arial.ttf":"LegacyRuntime.ttf"));
   typeof(ContentSelectorBar).GetMethod("BuildChips",flags).Invoke(bar,new object[]{rect,800f,170f});
   CardFaces.Selected=CardFaces.Choice.Deck;
   typeof(ContentSelectorBar).GetMethod("Refresh",flags).Invoke(bar,null);
   typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,cam,"hearts-ace-selector",945,201});
   var uri=new Uri(new Assets.Script.Constants.APIConstants().GetUserRoute);
   if(!(uri.GetLeftPart(UriPartial.Authority)+"/auth/session").EndsWith("duckdns.org/auth/session"))throw new Exception("Wrong session endpoint");
   Debug.Log("SESSION_SELECTOR_VALIDATION_PASSED");
  }finally{CardFaces.Selected=prior;UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(camera);}
 }
}
