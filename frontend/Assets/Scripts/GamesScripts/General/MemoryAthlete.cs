using System.Linq;

/// <summary>
/// Memory Athlete: the hardest Challenge tier. It uses its own level number (21), is unlocked by clearing
/// all 20 levels of at least one game, and has fixed rules per game.
/// </summary>
public static class MemoryAthlete
{
    public const int Level = 21;
    public const string Key = "memory-athlete";

    private static readonly string[] Qualifying = { "pairs-all-levels", "flash-all-levels", "boxes-all-levels" };

    /// <summary>
    /// True once the badge is earned, as soon as one of the three "all levels" badges exists, or as soon as level 20 has
    /// been cleared in any game (read straight from the saved level results, so it never depends on a badge round trip).
    /// </summary>
    public static bool Unlocked
    {
        get
        {
            var done = GameManager.completedAchievements;
            if (done.Contains(Key) || Qualifying.Any(done.Contains)) return true;
            if (!ClearedLevel20()) return false;
            EnsureBadge();
            return true;
        }
    }

    private static bool ClearedLevel20()
    {
        return StarRules.StarsOf(LevelProgress.Get("Boxes"), 20) > 0
            || StarRules.StarsOf(LevelProgress.Get("Pairs"), 20) > 0
            || StarRules.StarsOf(LevelProgress.Get("Flash"), 20) > 0;
    }

    /// <summary>Gives the badge itself when the level evidence is there but the badge was never recorded.</summary>
    private static void EnsureBadge()
    {
        try
        {
            if (GameManager.UnlockAchievement(Key))
                ProgressSync.Instance.GetApi<Assets.Script.Controllers.AchievementsAPIController>().FlushPending();
        }
        catch (System.Exception) { /* saved locally, synced later */ }
    }

    public static bool IsRound { get { return LevelScript._selectedLevel == Level; } }

    /// <summary>Set when a Challenge round on the Memory Athlete level was completed (not failed).</summary>
    public static bool LastChallengeCompleted;

    // Pairs: 24 pairs (48 cards), 60 s to memorize, 300 s to find them.
    public const int PairsCards = 48;
    public const float PairsMemorizeSeconds = 60f;
    public const float PairsFindSeconds = 300f;

    // Flash: 42 images, 120 s to recall, 3 hearts.
    public const int FlashImages = 42;
    public const int FlashMistakes = 3;
    public const float FlashRecallSeconds = 120f;

    // Boxes: 24 boxes, 60 s to place everything, 3 hearts.
    public const int BoxesBoxes = 24;
    public const int BoxesMistakes = 3;
    public const float BoxesRecallSeconds = 60f;

    public const string LockedHint = "Unlock Memory Athlete by clearing level 20 in Pairs, Flash or Boxes. Level 20 is open from the start.";
}
