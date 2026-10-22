using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Constants;

// Visual proposal only. This does not add an executable Athlete round or change game rules.
public static class AthleteLevelPreview
{
 public static void Render() {
  var old=UserConstants.s_user;UserConstants.s_user=new Assets.Script.Models.User{id=1999999988};
  var root=new GameObject("Athlete preview",typeof(RectTransform),typeof(Canvas));
  var cameraObject=new GameObject("Preview camera",typeof(Camera));
  try {
   var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(945,2048);
   var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
   var camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=1024;camera.transform.position=new Vector3(0,0,-20);camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;camera.cullingMask=1<<30;canvas.worldCamera=camera;
   var screen=(RectTransform)ChainGame.Panel(rect,"Training",Vector2.zero,Vector2.one,Color.white).transform;
   ChainGame.Attach(screen);screen.GetComponentInChildren<ChainGame>().Open();Canvas.ForceUpdateCanvases();
   var cells=root.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Chain Level ")).OrderBy(i=>int.Parse(i.name.Substring(12))).ToArray();
   if(cells.Length!=20)throw new Exception("Missing original20 level cells");
   var originalPositions=cells.Select(i=>i.rectTransform.localPosition).ToArray();
   var originalSizes=cells.Select(i=>i.rectTransform.sizeDelta).ToArray();
   var last=cells[19].rectTransform;
   var athlete=UnityEngine.Object.Instantiate(last.gameObject,last.parent,false);athlete.name="Proposed Memory Athlete";
   var athleteRect=(RectTransform)athlete.transform;athleteRect.anchoredPosition=last.anchoredPosition+new Vector2(0,last.localPosition.y-cells[15].rectTransform.localPosition.y);
   athlete.transform.GetChild(0).gameObject.SetActive(false);
   foreach(var label in athlete.GetComponentsInChildren<Text>(true))label.gameObject.SetActive(false);
   foreach(var button in athlete.GetComponentsInChildren<Button>(true)){button.onClick=new Button.ButtonClickedEvent();button.interactable=false;button.transition=Selectable.Transition.None;}
   var art=ChainGame.Panel(athlete.transform,"Existing elephant logo",new Vector2(.17f,.32f),new Vector2(.83f,.82f),Color.white).GetComponent<Image>();art.sprite=Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01");art.preserveAspect=true;art.raycastTarget=false;
   ChainGame.Label(athlete.transform,"1 ♥",new Vector2(.22f,.10f),new Vector2(.78f,.32f),29,ChainGame.Purple);
   Canvas.ForceUpdateCanvases();
   for(int i=0;i<20;i++)if(cells[i].rectTransform.localPosition!=originalPositions[i] || cells[i].rectTransform.sizeDelta!=originalSizes[i])throw new Exception("Original level moved or resized: "+(i+1));
   typeof(ChainUiValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{root,camera,945,2048,"chain-athlete-below20"});
   Debug.Log("ATHLETE_VISUAL_PROPOSAL_READY: original hexagon, original elephant logo,1heart; awaiting user approval before implementation/build.");
  } finally {UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UserConstants.s_user=old;}
 }
}
