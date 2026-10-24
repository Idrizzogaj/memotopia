using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class BoxesTabletValidation {
 public static void Review(){
  var prior=UserConstants.s_user;bool challenge=ChallengeConstants.s_isChallenge;int level=LevelScript._selectedLevel;
  try{
   UserConstants.s_user=new User{id=1999999951,username="Layout review"};ChallengeConstants.s_isChallenge=false;
   foreach(var scene in new[]{"BoxesScene","BoxesSceneIpad"})foreach(int height in scene=="BoxesScene"?new[]{1067,1734}:new[]{1067}){
    EditorSceneManager.OpenScene("Assets/_Scenes/"+scene+".unity");var manager=UnityEngine.Object.FindObjectOfType<BoxesManager>();var canvas=manager.GetComponentInParent<Canvas>();if(canvas==null)canvas=UnityEngine.Object.FindObjectOfType<Canvas>().rootCanvas;
    canvas.renderMode=RenderMode.WorldSpace;var space=(RectTransform)canvas.transform;space.position=Vector3.zero;space.rotation=Quaternion.identity;space.localScale=Vector3.one;space.sizeDelta=new Vector2(800,height);canvas.GetComponent<CanvasScaler>().enabled=false;
    foreach(var item in manager.levelPanels)item.SetActive(false);LevelScript._selectedLevel=20;manager.LevelSelector();manager.rsg.SetActive(false);manager.tutorialController.DisableTutorialObjects(true);
    var panel=(GameObject)typeof(BoxesManager).GetField("panelLvl",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(manager);
    CardFaces.BeginSession();typeof(BoxesManager).GetMethod("InitArrays",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(manager,null);typeof(BoxesManager).GetMethod("SpawnImages",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(manager,null);Canvas.ForceUpdateCanvases();
    var recallBefore=BoxesBoardFit.RectBounds((RectTransform)manager.recallBtn.transform,space);var before=BoxesBoardFit.BoardBounds(panel,space);BoxesBoardFit.Fit(panel,manager.recallBtn,manager.topPanel);var after=BoxesBoardFit.BoardBounds(panel,space);var top=BoxesBoardFit.RectBounds((RectTransform)manager.topPanel.transform,space);
    Debug.Log("BOXES_BOUNDS "+scene+" "+height+" board="+before+" fitted="+after+" recall="+recallBefore+" header="+top);
    if(after.min.y<recallBefore.max.y+8)throw new Exception("Boxes overlap Recall at "+height);
    if(after.max.y>top.min.y-8)throw new Exception("Boxes overlap header at "+height);
    if(BoxesBoardFit.RectBounds((RectTransform)manager.recallBtn.transform,space)!=recallBefore)throw new Exception("Original Recall button moved");
    if(height==1067){var go=new GameObject("Boxes review camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.backgroundColor=Color.white;camera.clearFlags=CameraClearFlags.SolidColor;camera.cullingMask=1<<30;typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas.gameObject,camera,"boxes-tablet-"+scene,1024,1366});UnityEngine.Object.DestroyImmediate(go);}
   }
   Debug.Log("BOXES_TABLET_LAYOUT_PASSED: original board fits above Recall on tablet and phone; original button unchanged");
  }finally{UserConstants.s_user=prior;ChallengeConstants.s_isChallenge=challenge;LevelScript._selectedLevel=level;CardFaces.EndSession();}
 }
}
