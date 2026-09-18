using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;

// Small per-account cache and durable outbox. Navigation never has to wait for a score upload.
public static class LevelProgress
{
    [Serializable]
    public class Entry
    {
        public string game;
        public int level, stars, score, id, revision;
        public bool pending;
        public Entry Copy() { return (Entry)MemberwiseClone(); }
    }
    [Serializable]
    private class Save
    {
        public List<Entry> entries = new List<Entry>();
        public List<string> loaded = new List<string>();
    }
    private static int owner = -1;
    private static Save data = new Save();
    public static int Owner { get { return UserConstants.s_user == null ? 0 : UserConstants.s_user.ID; } }

    public static void EnsureUser()
    {
        if (owner == Owner) return;
        owner = Owner; data = new Save();
        if (owner > 0)
            try
            {
                var saved = JsonUtility.FromJson<Save>(PlayerPrefs.GetString("levels.v1." + owner, "{}"));
                if (saved != null)
                {
                    data.entries = saved.entries ?? new List<Entry>();
                    data.loaded = saved.loaded ?? new List<string>();
                }
            }
            catch (ArgumentException) { }
        Publish();
    }

    private static GameLevel[] Snapshot(string game)
    {
        if (!data.loaded.Contains(game)) return null;
        return data.entries.Where(e => e.game == game).OrderBy(e => e.level)
            .Select(e => new GameLevel { id = e.id, level = e.level, score = e.score, stars = e.stars }).ToArray();
    }
    public static GameLevel[] Get(string game) { EnsureUser(); return Snapshot(game); }
    public static List<Entry> Pending { get { EnsureUser(); return data.entries.Where(e => e.pending).Select(e => e.Copy()).ToList(); } }

    public static void Record(string game, int level, int stars, int score)
    {
        EnsureUser();
        if (owner <= 0 || level < 1 || level > 20) return;
        var entry = data.entries.Find(e => e.game == game && e.level == level);
        if (entry == null) { entry = new Entry { game = game, level = level }; data.entries.Add(entry); }
        entry.stars = Math.Max(entry.stars, stars); entry.score = Math.Max(entry.score, score);
        entry.pending = true; entry.revision++;
        if (!data.loaded.Contains(game)) data.loaded.Add(game);
        Persist();
    }

    public static void Merge(string game, GameLevel[] levels)
    {
        EnsureUser();
        if (levels == null) return;
        foreach (var level in levels)
        {
            if (level == null || level.level < 1 || level.level > 20) continue;
            var entry = data.entries.Find(e => e.game == game && e.level == level.level);
            if (entry == null) { entry = new Entry { game = game, level = level.level }; data.entries.Add(entry); }
            entry.id = level.id;
            entry.stars = Math.Max(entry.stars, level.stars); entry.score = Math.Max(entry.score, level.score);
        }
        if (!data.loaded.Contains(game)) data.loaded.Add(game);
        Persist();
    }

    public static void Confirm(Entry sent)
    {
        EnsureUser();
        var entry = data.entries.Find(e => e.game == sent.game && e.level == sent.level);
        if (entry != null && entry.revision == sent.revision) entry.pending = false;
        Persist();
    }

    private static void Publish()
    {
        GameLevelConstants.s_boxesLevels = Snapshot("Boxes");
        GameLevelConstants.s_pairsLevels = Snapshot("Pairs");
        GameLevelConstants.s_flashLevels = Snapshot("Flash");
    }
    private static void Persist()
    {
        if (owner > 0)
        {
            PlayerPrefs.SetString("levels.v1." + owner, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        Publish();
    }
}
