using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public static event Action AchievementsChanged;

    [Serializable]
    private class AchievementSave
    {
        public List<string> completed = new List<string>();
        public List<string> pending = new List<string>();
    }

    private static int achievementOwner = -1;
    private static AchievementSave achievementSave = new AchievementSave();
    public static int AchievementUserId { get { return UserConstants.s_user == null ? 0 : UserConstants.s_user.ID; } }
    // v1 trusted remote badge flags. Start a separate, evidence-earned cache without deleting history.
    private static string SaveKey { get { return "achievements.verified.v2." + achievementOwner; } }

    private static void LoadAchievements()
    {
        int owner = AchievementUserId;
        if (achievementOwner == owner) return;
        achievementOwner = owner;
        achievementSave = new AchievementSave();
        if (owner <= 0) return;
        try
        {
            var saved = JsonUtility.FromJson<AchievementSave>(PlayerPrefs.GetString(SaveKey, "{}"));
            if (saved != null)
            {
                achievementSave.completed = Normalize(saved.completed);
                achievementSave.pending = Normalize(saved.pending);
            }
        }
        catch (ArgumentException) { /* Ignore a damaged local cache; the server can restore it. */ }
    }

    private static List<string> Normalize(IEnumerable<string> keys)
    {
        return (keys ?? Enumerable.Empty<string>()).Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
    }

    private static void SaveAchievements()
    {
        if (achievementOwner > 0)
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(achievementSave));
            PlayerPrefs.Save();
        }
        if (AchievementsChanged != null) AchievementsChanged();
    }

    public static List<string> completedAchievements
    {
        get { LoadAchievements(); return Normalize(achievementSave.completed.Concat(achievementSave.pending)); }
        set
        {
            LoadAchievements();
            // Only Kickstarter is an administrative award; gameplay badges need performance evidence.
            // The old server flags cannot reintroduce the erroneous unlocks this migration repairs.
            var remote = Normalize(value);
            // "I have read this tip" badges cannot be re-derived from play, so the server's record is trusted.
            foreach (var key in remote.Where(k => k.StartsWith("tip-")))
                if (!achievementSave.completed.Contains(key)) achievementSave.completed.Add(key);
            if (remote.Contains("kickstarter") && !achievementSave.completed.Contains("kickstarter"))
                achievementSave.completed.Add("kickstarter");
            foreach (var key in achievementSave.pending.Where(remote.Contains).ToArray())
            {
                if (!achievementSave.completed.Contains(key)) achievementSave.completed.Add(key);
                achievementSave.pending.Remove(key);
            }
            SaveAchievements();
        }
    }

    public static List<string> PendingAchievements
    {
        get { LoadAchievements(); return new List<string>(achievementSave.pending); }
    }

    public static bool UnlockAchievement(string key)
    {
        if(PracticeMode.BlockAchievements)return false;
        if (string.IsNullOrEmpty(key) || AchievementUserId <= 0 || completedAchievements.Contains(key)) return false;
        achievementSave.pending.Add(key);
        SaveAchievements();
        return true;
    }

    public static void ConfirmAchievement(string key)
    {
        LoadAchievements();
        if (!achievementSave.pending.Contains(key)) return;
        if (!achievementSave.completed.Contains(key)) achievementSave.completed.Add(key);
        achievementSave.pending.Remove(key);
        SaveAchievements();
    }

    void Awake()
    {
        if (gameManager != null && gameManager != this) { Destroy(gameObject); return; }
        gameManager = this;
        DontDestroyOnLoad(gameObject);
    }
}
