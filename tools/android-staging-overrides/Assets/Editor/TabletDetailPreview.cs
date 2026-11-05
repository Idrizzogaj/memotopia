using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEditor.SceneManagement;using Assets.Script.Constants;using Assets.Script.Models;
public static class TabletDetailPreview {
 public static void Render(){
  var prior=UserConstants.s_user;UserConstants.s_user=new User{id=1999999925,username="Layout review"};
  try{
   ArenaHomePreview.Render();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var root=new GameObject("Tablet details",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1067);var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
   var camera=new GameObject("Tablet review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.backgroundColor=Color.white;camera.clearFlags=CameraClearFlags.SolidColor;canvas.worldCamera=camera;
   var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);var flags=BindingFlags.NonPublic|BindingFlags.Instance;
   foreach(int level in new[]{1,4,8}){var game=FocusGame.Open(rect,1);typeof(FocusGame).GetField("level",flags).SetValue(game,level);typeof(FocusGame).GetField("startLevel",flags).SetValue(game,level);typeof(FocusGame).GetMethod("Prepare",flags).Invoke(game,null);capture.Invoke(null,new object[]{root,camera,"focus-boxes-ipad-"+level,1024,1366});game.Close();}
   CardFaces.Selected=CardFaces.Choice.Images;CardFaces.BeginSession();var says=SaysGame.Open(rect);says.StartLevel(1);says.BeginPlayback();says.Tick(2.1f);capture.Invoke(null,new object[]{root,camera,"says-keypad-ipad",1024,1366});says.Close();CardFaces.EndSession();UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(camera.gameObject);
   FocusStatisticsValidation.Review();Debug.Log("TABLET_DETAIL_PREVIEW_RENDERED: Arena, Says keypad, one/two/three Boxes and Focus statistics");
  }finally{UserConstants.s_user=prior;}
 }
}
