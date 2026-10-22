using System;
using System.Linq;
using UnityEngine;
public static class ChallengeSprintValidation {
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
 public static void Run(){
  Check(DragAndDropCell.Matches("1 Number","1") && !DragAndDropCell.Matches("10 Number","1") && !DragAndDropCell.Matches("100 Number","10"),"Boxes matches exact identity, not a numeric prefix");
  Check(Enumerable.Range(0,5).Select(i=>ChallengeSprintRules.StageSize("Pairs",i)).SequenceEqual(new[]{3,5,7,8,9}),"Pairs growth");
  foreach(var game in new[]{"Boxes","Flash","Chain"})Check(Enumerable.Range(0,10).Select(i=>ChallengeSprintRules.StageSize(game,i)).SequenceEqual(new[]{5,7,9,12,15,18,20,20,20,20}),"Stage cap");
  var state=new ChallengeSprintState("Pairs",6,1000);
  state.Turn(2000);state.Turn(2000);state.Solve(2000);state.Solve(3000);Check(state.Solve(4000),"Stage completion");
  Check(state.Stage==1 && state.Correct==0 && state.Result().roundMs.Single()==3000,"Round timing includes study");
  state.Solve(5000);Check(state.Result().correct==1,"Partial progress retained");
  Check(!state.Solve(91000) && state.Finished && state.Correct==1,"Deadline blocks late answers");
  Check(state.Remaining(2000)==0,"Clock cannot rewind");
  Check(ChallengeSprintRules.Compare("Pairs",new SprintResult{completed=1,correct=2,turns=100},new SprintResult{completed=1,correct=1,turns=2})>0,"Progress before turns");
  Check(ChallengeSprintRules.Compare("Pairs",new SprintResult{turns=3},new SprintResult{turns=4})>0,"Fewer turns");
  SprintResult a=new SprintResult{completed=2,roundMs=new[]{100,300}},b=new SprintResult{completed=2,roundMs=new[]{200,200}};
  Check(ChallengeSprintRules.Compare("Boxes",a,b)<0,"Most recent complete round first");
  a.roundMs=new[]{100,200};Check(ChallengeSprintRules.Compare("Flash",a,b)>0,"Earlier round tie break");
  a.errors=1;Check(ChallengeSprintRules.Compare("Chain",a,b)<0,"Errors precede speed");
  Check(ChallengeSprintRules.Compare("Chain",b,b)==0,"Draw");
  Check(ChallengeSprintRules.OpponentAt(state.Result().trace,3500)==3,"Replay uses elapsed time, not opponent final score");
  Check(ChallengeSprintRules.Cue(45001,45000)=="Half time!" && ChallengeSprintRules.Cue(1,0)=="BRAINS OUT!","Time cues");
  Check(new[]{6,12,18,21}.Select(ChallengeSprintRules.KindCount).SequenceEqual(new[]{1,2,3,4}),"Content tiers");
  Debug.Log("CHALLENGE_SPRINT_ENGINE_VALIDATION_PASSED");
 }
}

