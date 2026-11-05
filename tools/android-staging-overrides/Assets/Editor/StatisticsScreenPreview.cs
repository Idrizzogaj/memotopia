using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEditor.SceneManagement;
public static class StatisticsScreenPreview {
 public static void Render(){
  EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var menu=Resources.FindObjectsOfTypeAll<MenuNavigation>().First(n=>n.gameObject.scene.IsValid());var screen=menu.statisticsScreen;var canvas=screen.GetComponentsInParent<Canvas>(true).First().rootCanvas;
  for(var t=screen.transform;t!=canvas.transform;t=t.parent){foreach(Transform sibling in t.parent)if(sibling!=t)sibling.gameObject.SetActive(false);t.gameObject.SetActive(true);}
  canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;canvas.GetComponent<CanvasScaler>().enabled=false;var rect=(RectTransform)canvas.transform;
  var camera=new GameObject("Statistics page camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.backgroundColor=Color.white;camera.clearFlags=CameraClearFlags.SolidColor;canvas.worldCamera=camera;
  var view=menu.GetComponent<StatisticsView>();view.SendMessage("Start");view.ApplyChallengeCounters(null);var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);
  foreach(bool tablet in new[]{false,true}){rect.sizeDelta=tablet?new Vector2(800,1067):new Vector2(800,1734);view.ApplyFocusStatistics();Canvas.ForceUpdateCanvases();var focus=screen.GetComponentsInChildren<Transform>().First(t=>t.name=="Focus statistics");var scroll=focus.GetComponentInParent<ScrollRect>();if(scroll==null)throw new System.Exception("Statistics needs a scrolling viewport");scroll.verticalNormalizedPosition=.35f;Canvas.ForceUpdateCanvases();capture.Invoke(null,new object[]{canvas.gameObject,camera,tablet?"statistics-native-ipad":"statistics-native-phone",tablet?1024:945,tablet?1366:2048});}Debug.Log("NATIVE_STATISTICS_PREVIEW_RENDERED");
 }
}
