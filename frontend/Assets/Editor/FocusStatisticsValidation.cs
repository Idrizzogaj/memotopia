using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class FocusStatisticsValidation {
 static void Check(bool okay,string reason){if(!okay)throw new Exception(reason);}
 public static void Review(){
  const int owner=1999999927,other=1999999926;string key="focus.stats.device.v1."+owner;var prior=UserConstants.s_user;PlayerPrefs.DeleteKey(key);PlayerPrefs.DeleteKey("focus.stats.device.v1."+other);
  try{
   Check(FocusStats.Read(owner).games.Sum(g=>g.sessions)==0,"No invented historical sessions");
   string first=FocusStats.Begin(owner,1,4);FocusStats.End(owner,first,5,-3,2,5,90,true);FocusStats.End(owner,first,9,999,9,0,90,true);
   var record=FocusStats.Read(owner).games[1];Check(record.finished==1&&record.bestScore==-3&&record.bestStart==4,"Negative record, start level and idempotent save");
   string partial=FocusStats.Begin(owner,1,1);FocusStats.End(owner,partial,2,80,80,0,8,false);record=FocusStats.Read(owner).games[1];Check(record.sessions==2&&record.finished==1&&record.bestScore==-3,"Early quit must not replace full90second record");
   Check(FocusStats.Read(other).games.Sum(g=>g.sessions)==0,"Account isolation");
   FocusStats.Begin(owner,0,1);Check(!FocusStats.Read(owner).recent[0].ended,"Interrupted session remains unknown");
   UserConstants.s_user=new User{id=owner,username="Focus statistics review"};
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var root=new GameObject("Focus stats review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
   var camera=new GameObject("Review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;root.GetComponent<Canvas>().worldCamera=camera;
   var stack=ChainGame.Panel(root.transform,"Statistics content",new Vector2(.04f,0),new Vector2(.96f,1),Color.white);var layout=stack.AddComponent<VerticalLayoutGroup>();layout.childControlHeight=layout.childControlWidth=true;layout.childForceExpandHeight=false;
   var existing=ChainGame.Panel(stack.transform,"Existing challenge statistics",Vector2.zero,Vector2.one,Color.clear);existing.AddComponent<LayoutElement>().preferredHeight=0;
   var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);
   foreach(bool tablet in new[]{false,true}){
    rect.sizeDelta=tablet?new Vector2(1200,1600):new Vector2(800,1734);FocusStatisticsPanel.Apply(existing.transform);Canvas.ForceUpdateCanvases();
    var section=stack.transform.Find("Focus statistics");Check(section!=null,"Focus section exists");Check(section.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("On this device")),"Scope visible");
    Check(section.Find("Game records").childCount==4,"Four game records");capture.Invoke(null,new object[]{root,camera,tablet?"focus-statistics-ipad":"focus-statistics-phone",tablet?1200:945,tablet?1600:2048});
   }
   UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(camera.gameObject);
   Debug.Log("FOCUS_STATISTICS_VALIDATION_PASSED: real-only sessions, negative records, interrupted runs, duplicate save, account scope and phone/tablet section captures");
  }finally{PlayerPrefs.DeleteKey(key);UserConstants.s_user=prior;}
 }
}
