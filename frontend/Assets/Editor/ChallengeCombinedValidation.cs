using System;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class ChallengeCombinedValidation {
 public static void Review(){
  EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
  var navigation=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().First(n=>n.gameObject.scene.IsValid());
  var screen=navigation.challengeGameScreen.transform;
  var canvas=screen.GetComponentsInParent<Canvas>(true).First().rootCanvas;
  for(var current=screen;current!=canvas.transform;current=current.parent){foreach(Transform sibling in current.parent)if(sibling!=current)sibling.gameObject.SetActive(false);current.gameObject.SetActive(true);}
  TrainingHubLayout.ApplyChallenge(navigation);navigation.GoToChallengeLevel();
  if(navigation.challengeLevelScreen.activeSelf || !navigation.challengeGameScreen.activeSelf)throw new Exception("Legacy level page must not open");
  var images=new[]{navigation.pairsImage,navigation.boxesImage,navigation.flashImage,screen.Find("Chain challenge").GetComponent<Image>()};
  foreach(var image in images){
   Debug.Log("COMBINED_TILE "+image.name+" active="+image.gameObject.activeInHierarchy+" parent="+image.transform.parent.name+" position="+image.transform.position+" rect="+image.rectTransform.rect);
   foreach(var caption in new[]{"Easy","Medium","Hard","Memory Athlete"})if(image.transform.Find(caption)==null)throw new Exception("Missing corner choice "+caption);
   if(image.GetComponent<Button>().onClick.GetPersistentEventCount()!=0)throw new Exception("Old game-only action retained");
  }
  if(images.Select(i=>i.color).Distinct().Count()!=4)throw new Exception("Games need different original colours");
  TrainingHubLayout.ApplyChallenge(navigation);
  if(images.Any(i=>i.transform.Cast<Transform>().Count(t=>t.name=="Easy")!=1))throw new Exception("Duplicate corner buttons");
  canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;
  var rect=(RectTransform)canvas.transform;rect.sizeDelta=new Vector2(800,2048f*800/945);
  var camera=new GameObject("Review camera",typeof(Camera));var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.transform.position=new Vector3(0,0,-20);cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.white;
  var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;
  foreach(var t in canvas.GetComponentsInChildren<RectTransform>(true)){var p=t.localPosition;p.z=0;t.localPosition=p;}
  typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas.gameObject,cam,"challenge-game-and-level",945,2048});
  Debug.Log("CHALLENGE_COMBINED_VALIDATION_PASSED");
 }
}
