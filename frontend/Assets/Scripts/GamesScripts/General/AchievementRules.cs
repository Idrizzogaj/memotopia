using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;

public static class AchievementRules
{
    // Every achievement the app has. Game scenes only carry a subset in their own list, so progress
    // counters ("x / total") must use this full list.
    public static readonly string[] AllKeys =
    {
        "unlock-challenge", "first-challenge", "win-ten-challenge", "first-global-score", "kickstarter",
        "in-the-box", "looking-around", "pairing-up", "with-the-flash",
        "boxes-all-levels", "pairs-all-levels", "flash-all-levels",
    };

    public static HashSet<int> CompletedLevels(GameLevel[] levels, int justCompleted)
    {
        var result = new HashSet<int>();
        if (levels != null)
            foreach (var level in levels)
                if (level != null && level.level >= 1 && level.level <= 20 && level.stars > 0) result.Add(level.level);
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
        // clearing level 20 (open from the start in every game), or all 20 levels of one game
        if (level == 20 || b.Count == 20 || p.Count == 20 || f.Count == 20) keys.Add("memory-athlete");
        // stars: 20 levels x 3 stars = 60 per game
        int starsB = StarRules.TotalStars(boxes), starsP = StarRules.TotalStars(pairs), starsF = StarRules.TotalStars(flash);
        if (starsB >= StarRules.MaxStarsPerGame) keys.Add("boxes-star");
        if (starsP >= StarRules.MaxStarsPerGame) keys.Add("pairs-star");
        if (starsF >= StarRules.MaxStarsPerGame) keys.Add("flash-star");
        int allStars = starsB + starsP + starsF;
        if (allStars >= StarRules.MaxStarsPerGame) keys.Add("stars-60");
        if (allStars >= 2 * StarRules.MaxStarsPerGame) keys.Add("stars-120");
        if (allStars >= 3 * StarRules.MaxStarsPerGame) keys.Add("all-star");
        if (xp >= 400) keys.Add("unlock-challenge");
        if (challengeWins >= 1) keys.Add("first-challenge");
        if (challengeWins >= 10) keys.Add("win-ten-challenge");
        return keys;
    }
}
