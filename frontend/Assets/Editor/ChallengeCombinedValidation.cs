using System;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
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
  ValidateTouches(navigation,canvas);
  canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;
  var rect=(RectTransform)canvas.transform;rect.sizeDelta=new Vector2(800,2048f*800/945);
  var camera=new GameObject("Review camera",typeof(Camera));var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.transform.position=new Vector3(0,0,-20);cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.white;
  var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;
  foreach(var t in canvas.GetComponentsInChildren<RectTransform>(true)){var p=t.localPosition;p.z=0;t.localPosition=p;}
  typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas.gameObject,cam,"challenge-game-and-level",945,2048});
  Debug.Log("CHALLENGE_COMBINED_VALIDATION_PASSED");
 }
 private static void ValidateTouches(ChallengeNavigation navigation,Canvas canvas){
  var cameraObject=new GameObject("Selector touch camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();
  var texture=new RenderTexture(945,2048,24);camera.targetTexture=texture;camera.transform.position=new Vector3(0,0,-20);camera.farClipPlane=1000;
  canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
  var events=UnityEngine.Object.FindObjectOfType<EventSystem>();if(events==null)events=new GameObject("Selector events",typeof(EventSystem)).GetComponent<EventSystem>();
  // Reproduce the production installer's expanded hit areas, including its later rescans.
  for(int scan=0;scan<2;scan++)foreach(var button in navigation.challengeGameScreen.GetComponentsInChildren<Button>())if(button.isActiveAndEnabled)TouchTarget.Ensure(button);
  Canvas.ForceUpdateCanvases();foreach(var target in canvas.GetComponentsInChildren<TouchTarget>())target.Refresh();Canvas.ForceUpdateCanvases();camera.Render();
  var raycaster=canvas.GetComponent<GraphicRaycaster>();
  var images=new[]{navigation.pairsImage,navigation.boxesImage,navigation.flashImage,navigation.challengeGameScreen.transform.Find("Chain challenge").GetComponent<Image>()};
  foreach(var image in images)foreach(var caption in new[]{"Easy","Medium","Hard","Memory Athlete"}){
   var button=image.transform.Find(caption).GetComponent<Button>();
   var point=RectTransformUtility.WorldToScreenPoint(camera,button.transform.TransformPoint(((RectTransform)button.transform).rect.center));
   var hits=new List<RaycastResult>();raycaster.Raycast(new PointerEventData(events){position=point},hits);
   var handler=hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
   if(handler!=button.gameObject)throw new Exception("Selector touch blocked: "+image.name+"/"+caption+" hit="+(hits.Count==0?"nothing":hits[0].gameObject.name)+" handler="+(handler==null?"none":handler.name));
   int clicks=0;var previous=button.onClick;button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(()=>clicks++);
   ExecuteEvents.Execute(handler,new PointerEventData(events){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);button.onClick=previous;
   if(clicks!=1)throw new Exception("Selector click did not reach "+image.name+"/"+caption);
  }
  var row=navigation.challengeGameScreen.transform.Find("Single or Match");
  var single=row.Find("Single game").GetComponent<Button>();var match=row.Find("Match!").GetComponent<Button>();
  var orange=(Color)new Color32(254,191,75,255);var grey=(Color)new Color32(245,245,245,255);
  if(single.image.color!=orange || match.image.color!=grey)throw new Exception("Single selection colours incorrect");
  foreach(var button in new[]{match,single}){
   var point=RectTransformUtility.WorldToScreenPoint(camera,button.transform.TransformPoint(((RectTransform)button.transform).rect.center));
   var hits=new List<RaycastResult>();raycaster.Raycast(new PointerEventData(events){position=point},hits);
   var handler=hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
   if(handler!=button.gameObject)throw new Exception("Mode toggle blocked");
   ExecuteEvents.Execute(handler,new PointerEventData(events){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
   if(button.image.color!=orange || (button==match?single:match).image.color!=grey)throw new Exception("Mode selection did not swap");
   if(button.GetComponentInChildren<Text>().color!=Color.black)throw new Exception("Mode text must be black");
  }
  for(int i=0;i<images.Length;i++){
   var logo=images[i].transform.Find("Memory Athlete/Existing elephant logo").GetComponent<Image>();
   if(logo.color!=(i==2?Color.black:Color.white))throw new Exception("Elephant tint incorrect on "+images[i].name);
  }
  camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(texture);canvas.worldCamera=null;
  Debug.Log("CHALLENGE_SELECTOR_TOUCHES_PASSED");
 }

}
