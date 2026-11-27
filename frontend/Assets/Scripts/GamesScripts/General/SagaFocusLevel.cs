using System;

public sealed class SagaFocusLevel
{
    public readonly int Kind,Number,Target,Seconds;
    public SagaGame Game {get{return new[]{SagaGame.FocusPairs,SagaGame.FocusBoxes,SagaGame.FocusFlash,SagaGame.FocusChain}[Kind];}}
    static readonly int[] Times={20,19,18,17,15,30,29,28,27,25,40,39,38,37,35,60,58,56,54,50};
    // Targets are Idriz's specifications, not scores invented from existing records.
    static readonly int[][] Targets={new[]{10,15,20,30},new[]{20,30,40,60},new[]{10,20,30,50},new[]{30,50,70,100}};
    public SagaFocusLevel(int kind,int number){
        if(kind<0 || kind>=Targets.Length)throw new ArgumentOutOfRangeException("kind");
        if(number<1 || number>20)throw new ArgumentOutOfRangeException("number");
        Kind=kind;Number=number;Target=Targets[kind][(number-1)/5];Seconds=(kind==0 && number<=5?20:Times[number-1])+(kind==0?10:0);
    }
}
