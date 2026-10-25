using System.Collections.Generic;using System.Linq;using UnityEngine;
// One real practice attempt. Finished results retain the award guard until leaving their result screen.
public sealed class PracticeSession:MonoBehaviour {
 static readonly HashSet<PracticeSession> scopes=new HashSet<PracticeSession>();
 MonoBehaviour source;int owner,start,highest;string game;bool running,manual;public float Elapsed {get;private set;}
 public static bool BlocksAchievements {get{return scopes.Any(s=>s!=null&&s.source!=null&&s.owner==GameManager.AchievementUserId);}}
 public static PracticeSession Get(MonoBehaviour source){var s=source.GetComponent<PracticeSession>();return s!=null&&s.source==source?s:null;}
 public static PracticeSession Begin(MonoBehaviour source,string game,int level,bool manual=false){if(!PracticeMode.Untimed)return null;var s=source.GetComponent<PracticeSession>()??source.gameObject.AddComponent<PracticeSession>();s.Stop();s.source=source;s.owner=GameManager.AchievementUserId;s.game=game;s.start=level;s.highest=0;s.Elapsed=0;s.manual=manual;s.running=true;scopes.Add(s);return s;}
 public void Tick(float seconds){if(running)Elapsed+=Mathf.Max(0,seconds);}
 public void Progress(int level){if(running)highest=Mathf.Max(highest,level);}
 public void Stop(){if(!running)return;running=false;PracticeStats.Save(owner,game,start,highest,Elapsed);}
 public static void Release(MonoBehaviour source){var s=Get(source);if(s==null)return;s.Stop();scopes.Remove(s);s.source=null;}
 public static string Clock(MonoBehaviour source){var s=Get(source);return s==null?"0:00":PracticeStats.Time(s.Elapsed);}
 void Update(){if(source==null)return;if(owner!=GameManager.AchievementUserId){Release(source);return;}if(!manual)Tick(UnityEngine.Time.unscaledDeltaTime);}
 void OnDestroy(){Stop();scopes.Remove(this);}
}
