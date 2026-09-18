using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;

public static class AchievementRules
{
    public static HashSet<int> CompletedLevels(GameLevel[] levels, int justCompleted)
    {
        var result = new HashSet<int>();
        if (levels != null)
            foreach (var level in levels)
                if (level != null && level.level >= 1 && level.level <= 20) result.Add(level.level);
        if (justCompleted >= 1 && justCompleted <= 20) result.Add(justCompleted);
        return result;
    }

    public static List<string> Eligible(string game, int level, GameLevel[] boxes, GameLevel[] pairs,
        GameLevel[] flash, int xp, int challengeWins)
    {
        var b = CompletedLevels(boxes, game == StaticVar.s_gameBoxes ? level : 0);
        var p = CompletedLevels(pairs, game == StaticVar.s_gamePairs ? level : 0);
        var f = CompletedLevels(flash, game == StaticVar.s_gameFlash ? level : 0);
        var keys = new List<string>();
        if (b.Count > 0) keys.Add("in-the-box");
        if (p.Count > 0) keys.Add("pairing-up");
        if (f.Count > 0) keys.Add("with-the-flash");
        if (b.Contains(1) && p.Contains(1) && f.Contains(1)) keys.Add("looking-around");
        if (b.Count == 20) keys.Add("boxes-all-levels");
        if (p.Count == 20) keys.Add("pairs-all-levels");
        if (f.Count == 20) keys.Add("flash-all-levels");
        if (xp >= 400) keys.Add("unlock-challenge");
        if (challengeWins >= 1) keys.Add("first-challenge");
        if (challengeWins >= 10) keys.Add("win-ten-challenge");
        return keys;
    }
}
