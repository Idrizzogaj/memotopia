using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Account-scoped device records. No historical runs or cross-device sync are inferred.
public static class FocusStats {
 [Serializable] public sealed class Game {public int sessions,finished,highest,correct,errors,bestScore,bestStart,bestHighest,bestCorrect,bestErrors;public float seconds;public bool hasBest;}
 [Serializable] public sealed class Run {public string id,utc;public int kind,start,highest,score,correct,errors;public float seconds;public bool ended,completed;}
 [Serializable] public sealed class Data {public Game[] games={new Game(),new Game(),new Game(),new Game()};public List<Run> recent=new List<Run>();}
 static string Key(int owner){return "focus.stats.device.v1."+owner;}
 public static Data Read(int owner){try{var data=JsonUtility.FromJson<Data>(PlayerPrefs.GetString(Key(owner),"{}"));if(data!=null&&data.games!=null&&data.games.Length==4&&data.games.All(g=>g!=null)){if(data.recent==null)data.recent=new List<Run>();return data;}}catch(Exception){}return new Data();}
 static void Save(int owner,Data data){PlayerPrefs.SetString(Key(owner),JsonUtility.ToJson(data));PlayerPrefs.Save();}
 public static string Begin(int owner,int kind,int start){if(owner<=0)return null;var data=Read(owner);var run=new Run{id=Guid.NewGuid().ToString("N"),utc=DateTime.UtcNow.ToString("o"),kind=kind,start=start,highest=start};data.games[kind].sessions++;data.recent.Insert(0,run);if(data.recent.Count>20)data.recent.RemoveRange(20,data.recent.Count-20);Save(owner,data);return run.id;}
 public static void End(int owner,string id,int highest,int score,int correct,int errors,float seconds,bool completed){
  if(owner<=0||string.IsNullOrEmpty(id))return;var data=Read(owner);var run=data.recent.FirstOrDefault(r=>r.id==id);if(run==null||run.ended)return;
  run.ended=true;run.completed=completed;run.highest=highest;run.score=score;run.correct=correct;run.errors=errors;run.seconds=Mathf.Clamp(seconds,0,90);var game=data.games[run.kind];game.finished+=completed?1:0;game.correct+=correct;game.errors+=errors;game.seconds+=run.seconds;if(correct>0)game.highest=Mathf.Max(game.highest,highest);
  if(completed&&(!game.hasBest||score>game.bestScore||score==game.bestScore&&errors<game.bestErrors)){game.hasBest=true;game.bestScore=score;game.bestStart=run.start;game.bestHighest=highest;game.bestCorrect=correct;game.bestErrors=errors;}
  Save(owner,data);
 }
 public static string Accuracy(int correct,int errors){return correct+errors==0?"—":Mathf.RoundToInt(100f*correct/(correct+errors))+"%";}
}
