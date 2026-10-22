using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Constants;

public static class AthleteReleaseValidation
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static object Field(object o,string n){return o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(o);}
 static void Hearts(object o){o.GetType().GetMethod("EnableHeartsPerLevel",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,null);}
 public static void Review(){ChainExactFlashTemplate.Export();UnityEditor.AssetDatabase.Refresh();ChainReleaseValidation.Validate();Validate();}
 public static void Validate() {
  int oldLevel=LevelScript._selectedLevel;bool oldChallenge=ChallengeConstants.s_isChallenge;var oldUser=UserConstants.s_user;
  var root=new GameObject("Athlete tests");
  try {
   ChallengeConstants.s_isChallenge=false;LevelScript._selectedLevel=20;
   var flash=root.AddComponent<FlashManager>();flash.hearts=Enumerable.Range(0,5).Select(i=>{var o=new GameObject("Heart",typeof(RectTransform),typeof(Image));o.transform.SetParent(root.transform);return o.GetComponent<Image>();}).ToArray();
   flash.LevelSelector();int normalImages=(int)Field(flash,"imagesToShowNumber");float normalTime=(float)Field(flash,"gameTimeAllocation");
   LevelScript._selectedLevel=MemoryAthlete.Level;flash.LevelSelector();Hearts(flash);
   Check((int)Field(flash,"imagesToShowNumber")==normalImages && normalImages==32,"Training Athlete Flash changed level20 content");
   Check((int)Field(flash,"maxMistakes")==1 && flash.hearts.Count(h=>h.gameObject.activeSelf)==1,"Flash needs exactly1 visible heart");
   Check((float)Field(flash,"gameTimeAllocation")==normalTime,"Flash timing changed");
   ChallengeConstants.s_isChallenge=true;flash.LevelSelector();Check((int)Field(flash,"maxMistakes")==MemoryAthlete.FlashMistakes && (int)Field(flash,"imagesToShowNumber")==MemoryAthlete.FlashImages,"Challenge Athlete changed");
   ChallengeConstants.s_isChallenge=false;LevelScript._selectedLevel=20;
   var boxes=root.AddComponent<BoxesManager>();boxes.hearts=flash.hearts;boxes.levelPanels=Enumerable.Range(0,20).Select(i=>{var o=new GameObject("Level "+(i+1));o.transform.SetParent(root.transform);return o;}).ToArray();
   boxes.LevelSelector();int normalBoxes=boxes.spawnNumber;normalTime=(float)Field(boxes,"gameTimeAllocation");var panel=Field(boxes,"panelLvl");
   LevelScript._selectedLevel=MemoryAthlete.Level;boxes.LevelSelector();Hearts(boxes);
   Check(boxes.spawnNumber==normalBoxes && normalBoxes==22 && boxes.maxMistakes==1,"Boxes must be level20 with1heart");
   Check(boxes.hearts.Count(h=>h.gameObject.activeSelf)==1 && Field(boxes,"panelLvl")==panel && (float)Field(boxes,"gameTimeAllocation")==normalTime,"Boxes layout/timing/hearts drift");
   var pairs=root.AddComponent<LevelScript>();pairs.game=LevelScript.Games.Pairs;LevelScript._selectedLevel=7;pairs.StartAthlete();Check(LevelScript._selectedLevel==7,"Pairs must not get Athlete training");
   UserConstants.s_user=new Assets.Script.Models.User{id=1999999977};LevelScript._selectedLevel=MemoryAthlete.Level;
   string key="training.athlete.completed.v1.1999999977.Flash";bool had=PlayerPrefs.HasKey(key);int prior=PlayerPrefs.GetInt(key);
   string levelsKey="levels.v1.1999999977",badgeKey="achievements.verified.v2.1999999977";
   PlayerPrefs.DeleteKey(levelsKey);PlayerPrefs.DeleteKey(badgeKey);LevelProgress.EnsureUser();
   try {
    Check(!TrainingAthleteTile.Eligible("Flash") && !TrainingAthleteTile.Complete("Flash"),"Athlete must stay locked before clear20");
    LevelProgress.Record("Flash",19,3,100);Check(!TrainingAthleteTile.Eligible("Flash"),"19 must not unlock Athlete");
    LevelProgress.Record("Flash",20,1,50);Check(TrainingAthleteTile.Eligible("Flash"),"Direct20clear must unlock Athlete");
    flash.LevelCompleted("180",new Assets.Script.Models.GameLevel[0],"Flash");Check(TrainingAthleteTile.Completed("Flash"),"Athlete win not saved");
    Check(GameManager.PendingAchievements.Contains(TrainingAthleteTile.Badge("Flash")) && !TrainingAthleteTile.Complete("Flash"),"One reward receipt only");
    Check(flash.setStars(180)==0,"Athlete must award no stars");
    var canvas=new GameObject("Canvas",typeof(Canvas));
    try {
     flash.ActivateWinPanel("180",0,Resources.Load<GameObject>("Prefabs/GamePopUps/WinPanel"),"FlashScene","Flash");
     var shown=GameObject.Find("WinPanel(Clone)");Check(shown!=null,"Athlete win panel missing");
     Check(shown.GetComponentsInChildren<StarFillScript>(true).All(s=>!s.enabled),"Athlete star animation still running");
     Check(shown.GetComponentsInChildren<Image>(true).Where(i=>i.name.IndexOf("Star",StringComparison.OrdinalIgnoreCase)>=0).All(i=>!i.gameObject.activeSelf),"Athlete stars still visible");
     var reward=shown.GetComponentsInChildren<Text>(true).First(t=>t.text.Contains("500 XP"));
     Check(reward.resizeTextForBestFit && reward.resizeTextMaxSize<=28,"XP reward text would clip in old90point score field");
     UnityEngine.Object.DestroyImmediate(shown);
    } finally {UnityEngine.Object.DestroyImmediate(canvas);}
   }
   finally {if(had)PlayerPrefs.SetInt(key,prior);else PlayerPrefs.DeleteKey(key);PlayerPrefs.DeleteKey(levelsKey);PlayerPrefs.DeleteKey(badgeKey);}
   var parent=(RectTransform)ChainGame.Panel(root.transform,"Unmoved board",Vector2.zero,Vector2.one,Color.clear).transform;
   var last=ChainGame.Panel(parent,"20",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();var prev=ChainGame.Panel(parent,"16",Vector2.zero,Vector2.one,Color.white).GetComponent<Image>();
   last.rectTransform.anchoredPosition=new Vector2(100,-100);prev.rectTransform.anchoredPosition=new Vector2(100,100);last.rectTransform.sizeDelta=new Vector2(100,100);
   ChainGame.Panel(last.transform,"Stars",Vector2.zero,Vector2.one,Color.clear);var button=ChainGame.Button(last.transform,"20",Vector2.zero,Vector2.one,()=>{});
   var original=last.rectTransform.anchoredPosition;var size=last.rectTransform.sizeDelta;
   var tile=TrainingAthleteTile.Attach(last,prev,ChainGame.Purple,()=>{});Check(last.rectTransform.anchoredPosition==original && last.rectTransform.sizeDelta==size,"Original20 moved/resized");
   Check(((RectTransform)tile.transform).anchoredPosition==new Vector2(100,-300),"Athlete must be directly below20 using original spacing");
   Check(TrainingAthleteTile.Attach(last,prev,ChainGame.Purple,()=>{})==tile,"Duplicate Athlete tile");
   Debug.Log("ATHLETE_RELEASE_VALIDATION_PASSED: level20 content/timing,1 visible heart, first-error loss, no Pairs, original geometry, separate completion, Challenge preserved.");
  } finally {UnityEngine.Object.DestroyImmediate(root);LevelScript._selectedLevel=oldLevel;ChallengeConstants.s_isChallenge=oldChallenge;UserConstants.s_user=oldUser;PlayerPrefs.Save();}
 }
}
