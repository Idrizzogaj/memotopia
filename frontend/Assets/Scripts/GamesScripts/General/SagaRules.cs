using System;
using UnityEngine;

public enum SagaGame { Chain, Flash, Pairs, Boxes, Says, Sort, Boss, FocusPairs, FocusBoxes, FocusFlash, FocusChain, FocusBoss }

public static class SagaRoute
{
    public static readonly SagaGame[] Games = {SagaGame.Chain,SagaGame.Flash,SagaGame.Pairs,SagaGame.Boxes,SagaGame.Says,SagaGame.Sort};
    public static readonly SagaGame[] FocusGames={SagaGame.FocusPairs,SagaGame.FocusBoxes,SagaGame.FocusFlash,SagaGame.FocusChain};
    public const float BossSeconds=90;
    public static int Count { get { return 12*20; } }
    public static readonly Color FlashColor = new Color32(254,191,75,255);
    public static SagaLevel At(int stop)
    {
        if(stop<1 || stop>Count)throw new ArgumentOutOfRangeException("stop");
        int index=(stop-1)%12;
        return new SagaLevel(index<6?Games[index]:index==6?SagaGame.Boss:index==11?SagaGame.FocusBoss:FocusGames[index-7],(stop-1)/12+1);
    }
    public static int Stop(SagaGame game,int level)
    {
        new SagaLevel(game,level);
        int index=Array.IndexOf(Games,game);if(index<0)index=game==SagaGame.Boss?6:game==SagaGame.FocusBoss?11:7+Array.IndexOf(FocusGames,game);
        return (level-1)*12+index+1;
    }
    public static Color ColorFor(SagaGame game) {
        switch(game){
            case SagaGame.Flash:return FlashColor;
            case SagaGame.Pairs:return new Color32(36,189,224,255);
            case SagaGame.Boxes:return new Color32(234,24,100,255);
            case SagaGame.Says:return ChainGame.Purple;
            case SagaGame.Sort:return new Color32(242,139,55,255);
            case SagaGame.Boss:return new Color32(95,42,161,255);
            case SagaGame.FocusPairs:case SagaGame.FocusBoxes:case SagaGame.FocusFlash:case SagaGame.FocusChain:return FocusGame.Teal;
            case SagaGame.FocusBoss:return new Color32(15,94,117,255);
            default:return ChainGame.Theme;
        }
    }
    public static bool Unlocked(int stop)
    {
        if(stop<1 || stop>Count)return false;
        if(SagaProgress.DevelopmentAllLevelsOpen || stop==1)return true;
        var previous=At(stop-1);var current=At(stop);
        return SagaProgress.Stars(current.Game,current.Number)>0 || SagaProgress.Stars(previous.Game,previous.Number)>0;
    }
}

public sealed class SagaLevel
{
    public readonly SagaGame Game;
    public readonly int Number, ImageCount, Seconds, StudySeconds, TimedShuffles;
    public bool SharedStudyRecallClock {get{return Game==SagaGame.Pairs;}}
    public bool ShuffleLastTen { get { return Number >= 16; } }
    public const int Hearts = 3;

    public SagaLevel(int number) : this(SagaGame.Chain, number) { }

    public SagaLevel(SagaGame game, int number)
    {
        ValidateGame(game);
        if (number < 1 || number > 20) throw new ArgumentOutOfRangeException("number");
        Game = game;
        Number = number;
        int group = (number - 1) / 5;
        int offset = (number - 1) % 5;
        ImageCount = 10 + group * 5;
        Seconds = 90 - group * 10 - offset * 2;
        if (number == 5) Seconds = 80;
        StudySeconds=Seconds;
        TimedShuffles = group;
        if(game==SagaGame.Flash)ImageCount=new[]{3,4,5,6,7,8,9,10,11,12,14,16,18,20,21,22,24,26,28,32}[number-1];
        if(game==SagaGame.Pairs){ImageCount=number*2;Seconds=StudySeconds=90;TimedShuffles=0;}
        if(game==SagaGame.Boxes){ImageCount=5+group*5;TimedShuffles=0;}
        if(game==SagaGame.Says){ImageCount=3+(number-1)/2;TimedShuffles=0;}
        if(game==SagaGame.Sort){ImageCount=5+group*5;Seconds=90;StudySeconds=new[]{10,9,8,7,5,20,19,18,17,15,25,24,23,22,20,30,28,26,24,20}[number-1];TimedShuffles=0;}
        if(game==SagaGame.Boss){ImageCount=0;Seconds=StudySeconds=(int)SagaRoute.BossSeconds;TimedShuffles=0;}
        if(Array.IndexOf(SagaRoute.FocusGames,game)>=0){var focus=new SagaFocusLevel(Array.IndexOf(SagaRoute.FocusGames,game),number);ImageCount=0;Seconds=focus.Seconds;StudySeconds=0;TimedShuffles=0;}
        if(game==SagaGame.FocusBoss){ImageCount=0;Seconds=(int)SagaRoute.BossSeconds;StudySeconds=0;TimedShuffles=0;}
    }

    public static void ValidateGame(SagaGame game)
    {
        if(!Enum.IsDefined(typeof(SagaGame),game))throw new ArgumentOutOfRangeException("game");
    }

    public bool TimedShuffleDue(float elapsed, int performed, int remaining)
    {
        if (remaining <= 0 || performed >= TimedShuffles || TimedShuffles == 0) return false;
        if (ShuffleLastTen && remaining <= 10) return false;
        return elapsed >= Seconds * (performed + 1f) / (TimedShuffles + 1f);
    }

    public bool ShuffleAfterAnswer(int remaining)
    {
        return (Game==SagaGame.Chain || Game==SagaGame.Flash) && ShuffleLastTen && remaining > 0 && remaining <= 10;
    }
}

// Separate, account-scoped local progress. Never shares Training keys or server game IDs.
public static class SagaProgress
{
    public static int StarsFromFocusHearts(int hearts){return hearts>=5?3:hearts>=3?2:1;}
    public static readonly bool DevelopmentAllLevelsOpen = true;
    private static string Key(SagaGame game, int owner, int level)
    {
        SagaLevel.ValidateGame(game);
        return "saga." + game.ToString().ToLowerInvariant() + ".stars.v1." + owner + "." + level;
    }
    public static int Stars(int level) { return Stars(GameManager.AchievementUserId, level); }
    public static int Stars(int owner, int level) { return Stars(SagaGame.Chain,owner,level); }
    public static int Stars(SagaGame game, int level) { return Stars(game,GameManager.AchievementUserId,level); }
    public static int Stars(SagaGame game, int owner, int level)
    {
        if (level < 1 || level > 20) return 0;
        return Mathf.Clamp(PlayerPrefs.GetInt(Key(game, owner, level), 0), 0, SagaLevel.Hearts);
    }
    public static bool Unlocked(int level) { return Unlocked(SagaGame.Chain,level); }
    public static bool Unlocked(SagaGame game, int level)
    {
        SagaLevel.ValidateGame(game);
        return level >= 1 && level <= 20 && (DevelopmentAllLevelsOpen || level == 1 || Stars(game,level) > 0 || Stars(game,level - 1) > 0);
    }
    public static void Complete(int owner, int level, int hearts) { Complete(SagaGame.Chain,owner,level,hearts); }
    public static void Complete(SagaGame game, int owner, int level, int hearts)
    {
        if (owner != GameManager.AchievementUserId || level < 1 || level > 20 || hearts < 1 || hearts > SagaLevel.Hearts) return;
        PlayerPrefs.SetInt(Key(game, owner, level), Mathf.Max(Stars(game, owner, level), hearts));
        PlayerPrefs.Save();
    }
}
