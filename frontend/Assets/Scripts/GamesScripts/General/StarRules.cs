using UnityEngine;

/// <summary>
/// Star rules for Pairs, Flash and Boxes. Every round has 180 seconds and the score is the seconds left.
/// The time you may use for 3 and 2 stars grows with the level (more cards to find), so level 1 is not
/// trivial and level 20 is not impossible:
///   level 1  : 3 stars within 40 s, 2 stars within 81 s
///   level 10 : 3 stars within 85 s, 2 stars within 119 s
///   level 20 : 3 stars within 135 s, 2 stars within 162 s
/// Finishing at all always gives 1 star.
/// </summary>
public static class StarRules
{
    public const float TimeLimit = 180f;
    public const int MaxStarsPerGame = 60; // 20 levels x 3 stars

    /// <summary>Stars recorded for one level (0 when the level has no record).</summary>
    public static int StarsOf(Assets.Script.Models.GameLevel[] levels, int level)
    {
        int best = 0;
        if (levels != null)
            foreach (var entry in levels)
                if (entry != null && entry.level == level && entry.stars > best) best = entry.stars;
        return Mathf.Min(best, 3);
    }

    /// <summary>All stars of one game, levels 1 to 20.</summary>
    public static int TotalStars(Assets.Script.Models.GameLevel[] levels)
    {
        int total = 0;
        for (int level = 1; level <= 20; level++) total += StarsOf(levels, level);
        return total;
    }

    private static float Progress(int level)
    {
        return Mathf.Clamp01((level - 1) / 19f);
    }

    public static float ThreeStarSecondsLeft(int level)
    {
        return TimeLimit * (1f - (0.22f + 0.53f * Progress(level)));
    }

    public static float TwoStarSecondsLeft(int level)
    {
        return TimeLimit * (1f - (0.45f + 0.45f * Progress(level)));
    }

    public static int Stars(int secondsLeft, int level)
    {
        if (secondsLeft >= ThreeStarSecondsLeft(level)) return 3;
        if (secondsLeft >= TwoStarSecondsLeft(level)) return 2;
        return 1;
    }
}
