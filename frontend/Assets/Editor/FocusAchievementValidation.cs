using System;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;

public static class FocusAchievementValidation {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static void Review(){
  var prior=UserConstants.s_user;const int user=1999999928;GameObject root=null;PlayerPrefs.DeleteKey("achievements.verified.v2."+user);
  try{
   UserConstants.s_user=new User{id=user,username="Focus awards review"};root=new GameObject("Focus awards canvas",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1734);
   var game=FocusGame.Open(rect,0);Check(!GameManager.completedAchievements.Contains("focus-first"),"Opening selector is not playing");game.Begin();Check(GameManager.completedAchievements.Contains("focus-first")&&GameManager.completedAchievements.Contains("focus-pairs-first"),"First play earns global and game award");game.Begin();Check(GameManager.PendingAchievements.Count(k=>k.StartsWith("focus-"))==2,"Replays must not duplicate first awards");game.Close();
   Check(FocusAchievements.Reached(0,1,7,8)=="focus-pairs-professor","Professor from level1");Check(FocusAchievements.Reached(0,8,8,8)==null&&FocusAchievements.Reached(0,2,7,8)==null,"Professor cannot be skipped");
   for(int kind=0;kind<4;kind++){Check(FocusAchievements.Reached(kind,1,1,2)==null,"No premature highest award");Check(FocusAchievements.Reached(kind,1,FocusRules.Maximum(kind)-1,FocusRules.Maximum(kind))==FocusAchievements.Highest[kind],"Highest award mapping");}
   var awards=ExtraAchievements.Built().Where(a=>a.constantString.StartsWith("focus-")).ToArray();Check(awards.Length==9&&awards.All(a=>a.icon!=null),"Nine Focus achievements with approved artwork");Debug.Log("FOCUS_ACHIEVEMENT_VALIDATION_PASSED:first play, no selector/duplicate unlock, Professor full run, four maxima and nine approved icons");
  }finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);PlayerPrefs.DeleteKey("achievements.verified.v2."+user);PlayerPrefs.DeleteKey("achievements.pending.v2."+user);PlayerPrefs.DeleteKey("focus.stats.device.v1."+user);UserConstants.s_user=prior;}
 }
}
