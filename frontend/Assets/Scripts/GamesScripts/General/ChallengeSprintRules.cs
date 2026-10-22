using System;
using System.Collections.Generic;
using System.Linq;

// Versioned independently of legacy timed challenges and training levels.
public static class ChallengeSprintRules {
 public const int Version=1,DurationMs=90000;
 public static readonly string[] Games={"Pairs","Boxes","Flash","Chain"};
 public static int StageSize(string game,int stage){
  if(stage<0 || !Games.Contains(game))throw new ArgumentException("Invalid sprint stage");
  if(game=="Pairs")return stage==0?3:stage==1?5:stage+5;
  return new[]{5,7,9,12,15,18,20}[Math.Min(stage,6)];
 }
 public static int KindCount(int tier){
  if(tier==6)return 1;if(tier==12)return 2;if(tier==18)return 3;if(tier==21)return 4;
  throw new ArgumentException("Invalid sprint tier");
 }
 // Positive means first player wins. Only integer, monotonic timestamps cross the wire.
 public static int Compare(string game,SprintResult a,SprintResult b){
  int c=a.completed.CompareTo(b.completed);if(c!=0)return c;
  c=a.correct.CompareTo(b.correct);if(c!=0)return c;
  if(game=="Pairs")return b.turns.CompareTo(a.turns);
  c=b.errors.CompareTo(a.errors);if(c!=0)return c;
  for(int i=Math.Min(a.roundMs.Length,b.roundMs.Length)-1;i>=0;i--){c=b.roundMs[i].CompareTo(a.roundMs[i]);if(c!=0)return c;}
  return 0;
 }
 public static int Units(string game,int completed,int correct){int n=correct;for(int i=0;i<completed;i++)n+=StageSize(game,i);return n;}
 public static int OpponentAt(SprintTrace[] trace,int elapsed){int units=0;foreach(var point in trace){if(point.ms>elapsed)break;units=point.units;}return units;}
 public static string Cue(int previousRemainingMs,int remainingMs){
  int[] times={45000,10000,3000,2000,1000,0};string[] text={"Half time!","10!","3","2","1","BRAINS OUT!"};
  string cue=null;for(int i=0;i<times.Length;i++)if(previousRemainingMs>times[i] && remainingMs<=times[i])cue=text[i];return cue;
 }
}
// Explicit algorithm rather than platform Random/GetHashCode: same recipe on Mac, Android and server.
public sealed class SprintRandom {
 private uint state;
 public SprintRandom(int seed){state=(uint)seed;if(state==0)state=0x6d2b79f5;}
 public uint Next(){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
 public void Shuffle(int[] values){for(int i=values.Length-1;i>0;i--){int j=(int)(Next()%(uint)(i+1));int n=values[i];values[i]=values[j];values[j]=n;}}
}
[Serializable] public sealed class SprintTrace {public int ms,units;}
[Serializable] public sealed class SprintResult {
 public int completed,correct,turns,errors;
 public int[] roundMs=new int[0];public SprintTrace[] trace=new SprintTrace[0];
}
// Input-independent state, used by UI bindings and deterministic replays.
public sealed class ChallengeSprintState {
 public readonly string Game;public readonly int Tier;public readonly long StartedMs;
 public int Stage {get;private set;}public int Correct {get;private set;}
 public int Turns {get;private set;}public int Errors {get;private set;}
 public bool Finished {get;private set;}
 private int lastElapsed,roundStarted;
 private readonly List<int> rounds=new List<int>();private readonly List<SprintTrace> trace=new List<SprintTrace>();
 public ChallengeSprintState(string game,int tier,long startedMs){ChallengeSprintRules.StageSize(game,0);ChallengeSprintRules.KindCount(tier);Game=game;Tier=tier;StartedMs=startedMs;}
 public int Elapsed(long now){return (int)Math.Max(lastElapsed,Math.Min(ChallengeSprintRules.DurationMs,Math.Max(0,now-StartedMs)));}
 public int Remaining(long now){return ChallengeSprintRules.DurationMs-Elapsed(now);}
 public bool Tick(long now){lastElapsed=Elapsed(now);if(lastElapsed>=ChallengeSprintRules.DurationMs)Finished=true;return !Finished;}
 public bool Turn(long now){if(!Tick(now))return false;Turns++;return true;}
 public bool Error(long now){if(!Tick(now))return false;Errors++;return true;}
 public bool Solve(long now){
  if(!Tick(now))return false;Correct++;
  trace.Add(new SprintTrace{ms=lastElapsed,units=ChallengeSprintRules.Units(Game,Stage,Correct)});
  if(Correct==ChallengeSprintRules.StageSize(Game,Stage)){rounds.Add(lastElapsed-roundStarted);roundStarted=lastElapsed;Stage++;Correct=0;return true;}
  return false;
 }
 public SprintResult Result(){return new SprintResult{completed=Stage,correct=Correct,turns=Turns,errors=Errors,roundMs=rounds.ToArray(),trace=trace.Select(p=>new SprintTrace{ms=p.ms,units=p.units}).ToArray()};}
}
