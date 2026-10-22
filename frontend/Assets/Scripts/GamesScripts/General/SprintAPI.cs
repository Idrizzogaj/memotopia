using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
[Serializable] public class SprintLegView {public string game;public int seed,pacer;public long ownerStart,recipientStart;public bool ownerPlayed,recipientPlayed;public SprintResult ownerResult,recipientResult;public int winner;}
[Serializable] public class SprintBattleView {public int id,ownerId,recipientId,version,tier,chooser,winner;public long serverTime;public string mode,ownerName,recipientName;public bool done;public SprintLegView[] legs;}
[Serializable] public class SprintBattleList {public SprintBattleView[] battles;}
[Serializable] public class SprintCreate {public int recipientId,tier;public string game,mode;}
[Serializable] public class SprintSubmit {public int legIndex;public SprintResult result;}
[Serializable] public class SprintChoose {public string game;}
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
 public void Request<T>(string path,string body,Action<T> success,Action<string> failure){StartCoroutine(Send(path,body,success,failure));}
 private IEnumerator Send<T>(string path,string body,Action<T> success,Action<string> failure){
#if UNITY_EDITOR
  if(EditorTransport!=null){EditorTransport(path,body,text=>success(Decode<T>(text)),failure);yield break;}
#endif
  string token=PlayerPrefs.GetString("token");int account=GameManager.AchievementUserId;
  string root=new Uri(new Assets.Script.Constants.APIConstants().GetUserRoute).GetLeftPart(UriPartial.Authority);
  using(var request=new UnityWebRequest(root+"/sprints"+path,body==null?"GET":"POST")){
   request.downloadHandler=new DownloadHandlerBuffer();if(body!=null)request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
   request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Authorization",token);request.timeout=20;
   yield return request.SendWebRequest();
   if(GameManager.AchievementUserId!=account || string.IsNullOrEmpty(PlayerPrefs.GetString("token")))yield break;
   if(request.isNetworkError || request.isHttpError){failure("Could not sync Challenge. Please try again. ("+request.responseCode+")");yield break;}
   T result;try{result=Decode<T>(request.downloadHandler.text);}catch(Exception){failure("Could not read Challenge response.");yield break;}
   success(result);
  }
 }
}
