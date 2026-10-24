using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LiveRecoveryValidation {
 static void Set(object o,string name,object value){o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);}
 public static void Review(){
  const int user=1999999944;var prior=UserConstants.s_user;string pending="sprint.pending."+user,cancel="sprint.cancel."+user;int posts=0;
  try{
   UserConstants.s_user=new User{id=user,username="Recovery review"};EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
   var nav=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().First(n=>n.gameObject.scene.IsValid());var lobby=SprintLobby.Attach(nav);var live=LiveChallenges.Current;
   var battle=new SprintBattleView{id=21,ownerId=user,recipientId=2,done=true,invitation="accepted",endReason="gave_up",forfeitedId=2,winner=user,legs=new[]{new SprintLegView{game="Boxes",pacer=user}}};
   SprintAPI.EditorTransport=(path,body,success,fail)=>{if(path=="/21/cancel"){posts++;success(JsonUtility.ToJson(battle));}else if(path=="/21" && body==null)success(JsonUtility.ToJson(battle));else fail("Unexpected recovery request "+path);};
   PlayerPrefs.SetString(pending,JsonUtility.ToJson(battle)+"\n"+JsonUtility.ToJson(new SprintSubmit{legIndex=0,result=new SprintResult()}));
   var retry=typeof(SprintLobby).GetMethod("RetryPending",BindingFlags.Instance|BindingFlags.NonPublic);retry.Invoke(lobby,null);
   if(PlayerPrefs.HasKey(pending)||posts!=0)throw new Exception("Authoritative ended battle must clear obsolete receipt without re-posting it");
   PlayerPrefs.SetString(cancel,JsonUtility.ToJson(battle));retry.Invoke(lobby,null);if(PlayerPrefs.HasKey(cancel)||posts!=1)throw new Exception("Queued give-up must acknowledge exactly once");
   PlayerPrefs.SetString("sprint.active."+user,JsonUtility.ToJson(battle));retry.Invoke(lobby,null);if(PlayerPrefs.HasKey("sprint.active."+user)||PlayerPrefs.HasKey(cancel)||posts!=2)throw new Exception("Force-close ticket must recover to one acknowledged forfeit");
   var reactions=SprintReactions.Attach(nav.challengeUserProfileScreen.transform,battle.id);foreach(string key in SprintReactions.Keys)if(SprintReactions.Icon(key)==null)throw new Exception("Missing standard emoji sprite "+key);
   var receive=typeof(SprintReactions).GetMethod("Receive",BindingFlags.Instance|BindingFlags.NonPublic);var feed=new SprintReactionFeed{playing=true,friends=false,events=new[]{new SprintReaction{sender=2,key="brain",at=12345}}};PlayerPrefs.DeleteKey("sprint.reaction.seen."+user+"."+battle.id);
   receive.Invoke(reactions,new object[]{feed});var toast=reactions.transform.Find("Opponent reaction").gameObject;if(toast.activeSelf)throw new Exception("Stranger emoji must wait during play");
   feed.friends=true;receive.Invoke(reactions,new object[]{feed});if(!toast.activeSelf)throw new Exception("Friend emoji must display during play");PlayerPrefs.DeleteKey("sprint.reaction.seen."+user+"."+battle.id);
   Set(live,"account",user);Set(live,"loaded",true);Set(live,"settings",new SocialSettings{friendsVisible=false,visible=false,friends=new[]{new SocialPlayer{id=2,requesterId=user}}});Set(live,"online",new OnlinePlayers{players=new[]{new SocialPlayer{id=2}}});
   live.Profile(nav.challengeUserProfileScreen.transform,2);var friend=nav.challengeUserProfileScreen.transform.Find("Friend").GetComponent<Button>();if(friend.interactable||friend.GetComponentInChildren<Text>().text!="Already sent")throw new Exception("Pending outgoing request must show Already sent and be disabled");
   var label=nav.challengeUserProfileScreen.transform.Find("Player presence").GetComponent<Text>();Canvas.ForceUpdateCanvases();PresenceIndicator.Set(label,true);var icon=label.GetComponentInChildren<PresenceIndicator>(true);
   if(label.text!="Online" || icon.rectTransform.anchoredPosition.x<label.rectTransform.rect.width*.5f)throw new Exception("Single-player profile must put presence icon after Online");
   Debug.Log("LIVE_RECOVERY_VALIDATION_PASSED: obsolete outbox, queued give-up, forced-close recovery, six emoji sprites and friendship gate, Already sent, presence after label");
  }finally{PlayerPrefs.DeleteKey(pending);PlayerPrefs.DeleteKey(cancel);UserConstants.s_user=prior;SprintAPI.EditorTransport=null;}
 }
}
