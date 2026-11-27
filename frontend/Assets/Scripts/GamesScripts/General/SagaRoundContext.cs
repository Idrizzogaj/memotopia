using System;
using UnityEngine;

// A boss owns one deadline. Child games never reset it or save individual wins.
public sealed class SagaRoundContext
{
    readonly Action<bool,int> finished;
    readonly Func<float> clock;
    readonly Func<float> remainingClock;
    readonly float deadline;
    bool reported;
    public readonly int StartingHearts;
    public SagaRoundContext(float deadline,Action<bool,int> finished,Func<float> clock=null,int hearts=SagaLevel.Hearts){this.deadline=deadline;this.finished=finished;this.clock=clock??(()=>Time.realtimeSinceStartup);StartingHearts=hearts;}
    public SagaRoundContext(Func<float> remainingClock,Action<bool,int> finished,int hearts=SagaLevel.Hearts){
        if(remainingClock==null)throw new ArgumentNullException("remainingClock");
        this.remainingClock=remainingClock;this.finished=finished;StartingHearts=hearts;
    }
    public float Remaining {get{return Mathf.Max(0,remainingClock!=null?remainingClock():deadline-clock());}}
    public const float FlashPlaybackSpeed=4f;
    public const float SaysPlaybackSpeed=4.5f;
    public float StudyLimit(SagaLevel level){return level.Game==SagaGame.Sort?Mathf.Min(5,level.StudySeconds):level.StudySeconds;}
    public void Report(bool won,int hearts){if(reported)return;reported=true;finished(won,hearts);}
}
