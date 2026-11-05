using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class TrainingColourPreview {
 public static void Run(){
  Assets.Script.Constants.UserConstants.s_user=new Assets.Script.Models.User{id=1999999966};
  EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var menu=Object.FindObjectOfType<MenuNavigation>();
  var root=new GameObject("Training preview",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(945,2048);root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
  var camera=new GameObject("Preview camera",typeof(Camera));var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=1024;cam.transform.position=new Vector3(0,0,-20);cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.white;
  var training=(RectTransform)menu.trainingScreen.transform;training.SetParent(rect,false);training.anchorMin=Vector2.zero;training.anchorMax=Vector2.one;training.offsetMin=training.offsetMax=Vector2.zero;training.gameObject.SetActive(true);
  typeof(ChainUiValidation).GetMethod("BuildTrainingBar",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{training});
  typeof(ChainUiValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,cam,945,2048,"training-game-colours"});
  Debug.Log("TRAINING_COLOUR_PREVIEW_SAVED");
 }
}
