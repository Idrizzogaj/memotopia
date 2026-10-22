using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Constants;

// Training-only variant. The existing Challenge Athlete rules remain unchanged.
public static class TrainingAthleteTile
{
 public static bool IsRound {get{return !ChallengeConstants.s_isChallenge && LevelScript._selectedLevel==MemoryAthlete.Level;}}
 private static string Key(string game){return "training.athlete.completed.v1."+GameManager.AchievementUserId+"."+game;}
 public static bool Completed(string game){return PlayerPrefs.GetInt(Key(game),0)==1;}
 public static string Badge(string game){return "athlete-one-heart-"+game.ToLowerInvariant();}
 private static string pendingPopup;private static int popupOwner;
 public static bool TakePopup(string game){if(popupOwner!=GameManager.AchievementUserId || pendingPopup!=game)return false;pendingPopup=null;return true;}
 public static bool Eligible(string game){if(game=="Chain")return ChainProgress.Completed(20);var levels=LevelProgress.Get(game);return levels!=null && Array.Exists(levels,l=>l.level==20 && l.stars>0);}
 public static bool Complete(string game){
  if(!Eligible(game) || GameManager.AchievementUserId<=0)return false;
  PlayerPrefs.SetInt(Key(game),1);PlayerPrefs.Save();
  bool fresh=GameManager.UnlockAchievement(Badge(game));pendingPopup=fresh?game:null;popupOwner=GameManager.AchievementUserId;return fresh;
 }
 public static string RewardText(string game){return GameManager.PendingAchievements.Contains(Badge(game))?"500 XP • awaiting sync":"500 XP • earned once";}
 public static GameObject Attach(Image last,Image previous,Color ink,UnityEngine.Events.UnityAction action,bool unlocked=true) {
  if(last==null || previous==null)return null;
  var parent=last.transform.parent;var existing=parent.Find("Training Memory Athlete");if(existing!=null)return existing.gameObject;
  var tile=UnityEngine.Object.Instantiate(last.gameObject,parent,false);tile.name="Training Memory Athlete";
  var rect=(RectTransform)tile.transform;
  rect.anchoredPosition=last.rectTransform.anchoredPosition+new Vector2(0,last.rectTransform.localPosition.y-previous.rectTransform.localPosition.y);
  tile.transform.GetChild(0).gameObject.SetActive(false);
  foreach(var label in tile.GetComponentsInChildren<Text>(true))label.gameObject.SetActive(false);
  foreach(var button in tile.GetComponentsInChildren<Button>(true)) {
   button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(action);button.interactable=unlocked;
  }
  var logo=ChainGame.Panel(tile.transform,"Existing elephant logo",new Vector2(.17f,.32f),new Vector2(.83f,.82f),Color.white).GetComponent<Image>();
  logo.sprite=Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01");logo.preserveAspect=true;logo.raycastTarget=false;
  ChainGame.Label(tile.transform,unlocked?"1 ♥":"Clear 20",new Vector2(.10f,.10f),new Vector2(.90f,.32f),unlocked?29:22,ink);
  return tile;
 }
}
