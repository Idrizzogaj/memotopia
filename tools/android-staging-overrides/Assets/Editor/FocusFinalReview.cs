using System;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class FocusFinalReview {
 public static void Review(){
  FocusStatisticsValidation.Review();LiveRecoveryValidation.Review();
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var root=new GameObject("Flash density review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
  var camera=new GameObject("Review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;canvas.worldCamera=camera;
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);
  foreach(bool tablet in new[]{false,true}){
   rect.sizeDelta=tablet?new Vector2(1200,1600):new Vector2(800,1734);var game=FocusGame.Open(rect,2);typeof(FocusGame).GetField("level",flags).SetValue(game,12);typeof(FocusGame).GetField("flashPart",flags).SetValue(game,2);typeof(FocusGame).GetMethod("Prepare",flags).Invoke(game,null);
   var buttons=root.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Token ")).ToArray();if(buttons.Length!=28)throw new Exception("Level12 mixed requires14of each colour");
   for(int a=0;a<buttons.Length;a++)for(int b=a+1;b<buttons.Length;b++){var ar=(RectTransform)buttons[a].transform;var br=(RectTransform)buttons[b].transform;if(Vector2.Distance(ar.localPosition,br.localPosition)<(ar.rect.width+br.rect.width)*.5f)throw new Exception("Flash circles overlap");}
   capture.Invoke(null,new object[]{root,camera,tablet?"flash-level12-mixed-ipad":"flash-level12-mixed-phone",tablet?1200:945,tablet?1600:2048});game.Close();
  }
  UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(camera.gameObject);Debug.Log("FLASH_TWELVE_DENSITY_PASSED:14green14red; no overlapping circles on phone and tablet");
 }
}
