using System;using System.Linq;using System.Reflection;using Assets.Script.Constants;using Assets.Script.Models;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.UI;
public static class FocusHowToValidation {
 static void Check(bool okay,string message){if(!okay)throw new Exception(message);}
 public static void Review(){
  const int owner=1999999924;var prior=UserConstants.s_user;UserConstants.s_user=new User{id=owner,username="Guide review"};EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var root=new GameObject("Guide review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
  var camera=new GameObject("Guide camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;canvas.worldCamera=camera;var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);
  try{
   for(int kind=0;kind<4;kind++){
    PlayerPrefs.DeleteKey("focus.howto.v1."+owner+"."+kind);rect.sizeDelta=new Vector2(800,1734);var game=FocusGame.Open(rect,kind);game.StartWithGuide();var guide=root.GetComponentInChildren<FocusHowTo>();Check(guide!=null,"First START opens tutorial");Check(!FocusStats.Read(owner).games[kind].hasBest&&FocusStats.Read(owner).games[kind].sessions==0,"Guide must not count as play");
    var pages=FocusHowTo.Pages(kind);for(int page=0;page<pages.Length;page++){
     foreach(bool tablet in new[]{false,true}){rect.sizeDelta=tablet?new Vector2(800,1067):new Vector2(800,1734);Canvas.ForceUpdateCanvases();capture.Invoke(null,new object[]{root,camera,"focus-guide-"+FocusRules.Names[kind]+"-"+(page+1)+(tablet?"-ipad":"-phone"),tablet?1024:945,tablet?1366:2048});}
     guide.Next();
    }
    Check(!FocusHowTo.Needed(owner,kind)&&FocusStats.Read(owner).games[kind].sessions==1,"Last Play marks guide seen and starts actual play once");game.Close();
    var reopened=FocusHowTo.Open(root.transform,owner,kind,null);Check(reopened!=null,"Help can reopen completed guide");reopened.Close();
   }
   Check(FocusHowTo.Pages(3).Any(p=>p.Title=="Green first, then red")&&FocusHowTo.Pages(3).Any(p=>p.Title=="Alternate colours"),"Chain variants must not be conflated");FocusAchievementValidation.Review();Debug.Log("FOCUS_HOW_TO_VALIDATION_PASSED: four first-play guides, every page phone/tablet, no timer/stats/awards while reading, final Play once, replayable Help and distinct Chain rules");
  }finally{for(int kind=0;kind<4;kind++)PlayerPrefs.DeleteKey("focus.howto.v1."+owner+"."+kind);foreach(string prefix in new[]{"focus.stats.device.v1.","achievements.verified.v2.","achievements.pending.v2."})PlayerPrefs.DeleteKey(prefix+owner);UserConstants.s_user=prior;if(root!=null)UnityEngine.Object.DestroyImmediate(root);if(camera!=null)UnityEngine.Object.DestroyImmediate(camera.gameObject);}
 }
}
