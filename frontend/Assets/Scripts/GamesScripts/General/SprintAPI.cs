using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
[Serializable] public class SprintLegView {public string game;public int seed,pacer;public long ownerStart,recipientStart,liveStart;public int ownerUnits,recipientUnits;public bool ownerPlayed,recipientPlayed;public SprintResult ownerResult,recipientResult;public int winner;}
[Serializable] public class SprintBattleView {public int id,ownerId,recipientId,version,tier,chooser,winner,forfeitedId;public long serverTime;public string mode,ownerName,recipientName,invitation,endReason;[NonSerialized]public long roundTripMs;public bool done,live;public SprintLegView[] legs;}
[Serializable] public class SprintFailure {public string message;}
[Serializable] public class SprintBattleList {public SprintBattleView[] battles;}
[Serializable] public class SprintCreate {public int recipientId,tier,version=2;public bool live;public string game,mode;}
[Serializable] public class SprintSubmit {public int legIndex;public SprintResult result;}
[Serializable] public class SprintChoose {public string game;}
[Serializable] public class SprintCancel {public string reason;}
public class SprintAPI : MonoBehaviour {
 // Unity's serializer materialises null custom classes. Explicit receipt flags
 // distinguish a missing result from a valid played round with zero progress.
 public static T Decode<T>(string json){
  T result=JsonUtility.FromJson<T>(json);
  var battle=result as SprintBattleView;if(battle!=null)Normalize(battle);
  var list=result as SprintBattleList;if(list!=null && list.battles!=null)foreach(var item in list.battles)Normalize(item);
  return result;
 }
 public static void Normalize(SprintBattleView battle){if(battle.legs==null)return;foreach(var leg in battle.legs){if(!leg.ownerPlayed)leg.ownerResult=null;if(!leg.recipientPlayed)leg.recipientResult=null;}}
#if UNITY_EDITOR
 public static Action<string,string,Action<string>,Action<string>> EditorTransport;
#endif
 public void Request<T>(string path,string body,Action<T> success,Action<string> failure,int timeout=20){
#if UNITY_EDITOR
  if(EditorTransport!=null){EditorTransport(path,body,text=>success(Decode<T>(text)),failure);return;}
#endif
  StartCoroutine(Send(path,body,success,failure,timeout));
 }
 private IEnumerator Send<T>(string path,string body,Action<T> success,Action<string> failure,int timeout){
#if UNITY_EDITOR
  if(EditorTransport!=null){EditorTransport(path,body,text=>success(Decode<T>(text)),failure);yield break;}
#endif
  string token=PlayerPrefs.GetString("token");int account=GameManager.AchievementUserId;
  string root=new Uri(new Assets.Script.Constants.APIConstants().GetUserRoute).GetLeftPart(UriPartial.Authority);
  using(var request=new UnityWebRequest(root+"/sprints"+path,body==null?"GET":"POST")){
   request.downloadHandler=new DownloadHandlerBuffer();if(body!=null)request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
   request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Authorization",token);request.timeout=timeout;
   float sent=Time.realtimeSinceStartup;yield return request.SendWebRequest();
   if(GameManager.AchievementUserId!=account || string.IsNullOrEmpty(PlayerPrefs.GetString("token")))yield break;
   if(PlayerPrefs.GetString("token")!=token){failure("Session refreshed. Please try again.");yield break;}
   if(request.isNetworkError || request.isHttpError){
    string message=null;try{message=JsonUtility.FromJson<SprintFailure>(request.downloadHandler.text).message;}catch(Exception){}
    failure(string.IsNullOrEmpty(message)?"Could not sync Challenge. Please try again. ("+request.responseCode+")":message);yield break;
   }
   T result;try{result=Decode<T>(request.downloadHandler.text);}catch(Exception){failure("Could not read Challenge response.");yield break;}
   var timed=result as SprintBattleView;if(timed!=null)timed.roundTripMs=(long)((Time.realtimeSinceStartup-sent)*1000);
   success(result);
  }
 }
}
