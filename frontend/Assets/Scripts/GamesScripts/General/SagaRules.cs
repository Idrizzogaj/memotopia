using System;
using UnityEngine;

public sealed class SagaLevel
{
    public readonly int Number, ImageCount, Seconds, TimedShuffles;
    public bool ShuffleLastTen { get { return Number >= 16; } }
    public const int Hearts = 3;

    public SagaLevel(int number)
    {
        if (number < 1 || number > 20) throw new ArgumentOutOfRangeException("number");
        Number = number;
        int group = (number - 1) / 5;
        int offset = (number - 1) % 5;
        ImageCount = 10 + group * 5;
        Seconds = 90 - group * 10 - offset * 2;
        if (number == 5) Seconds = 80;
        TimedShuffles = group;
    }

    public bool TimedShuffleDue(float elapsed, int performed, int remaining)
    {
        if (remaining <= 0 || performed >= TimedShuffles || TimedShuffles == 0) return false;
        if (ShuffleLastTen && remaining <= 10) return false;
        return elapsed >= Seconds * (performed + 1f) / (TimedShuffles + 1f);
    }

    public bool ShuffleAfterAnswer(int remaining)
    {
        return ShuffleLastTen && remaining > 0 && remaining <= 10;
    }
}

// Separate, account-scoped local progress. Never shares Training keys or server game IDs.
public static class SagaProgress
{
    public static readonly bool DevelopmentAllLevelsOpen = true;
    private static string Key(int owner, int level) { return "saga.chain.stars.v1." + owner + "." + level; }
    public static int Stars(int level) { return Stars(GameManager.AchievementUserId, level); }
    public static int Stars(int owner, int level)
    {
        if (level < 1 || level > 20) return 0;
        return Mathf.Clamp(PlayerPrefs.GetInt(Key(owner, level), 0), 0, SagaLevel.Hearts);
    }
    public static bool Unlocked(int level)
    {
        return level >= 1 && level <= 20 && (DevelopmentAllLevelsOpen || level == 1 || Stars(level) > 0 || Stars(level - 1) > 0);
    }
    public static void Complete(int owner, int level, int hearts)
    {
        if (owner != GameManager.AchievementUserId || level < 1 || level > 20 || hearts < 1 || hearts > SagaLevel.Hearts) return;
        PlayerPrefs.SetInt(Key(owner, level), Mathf.Max(Stars(owner, level), hearts));
        PlayerPrefs.Save();
    }
}
