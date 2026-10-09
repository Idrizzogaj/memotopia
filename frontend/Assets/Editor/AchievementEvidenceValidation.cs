using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;

public static class AchievementEvidenceValidation
{
    public static void ReviewAndTest()
    {
        AchievementValidation.Review();
    }
    public static void Validate()
    {
        const int owner = 2147483520;
        var savedUser = UserConstants.s_user;
        string oldKey = "achievements.v1." + owner;
        string newKey = "achievements.verified.v2." + owner;
        string otherKey = "achievements.verified.v2." + (owner - 1);
        var wrong = new List<string> { "unlock-challenge", "first-global-score", "in-the-box", "looking-around", "pairing-up", "with-the-flash" };
        PlayerPrefs.DeleteKey(newKey); PlayerPrefs.DeleteKey(otherKey);
        string legacy = "{\"completed\":[\"unlock-challenge\",\"first-global-score\",\"in-the-box\",\"looking-around\",\"pairing-up\",\"with-the-flash\"],\"pending\":[]}";
        PlayerPrefs.SetString(oldKey, legacy);
        try
        {
            UserConstants.s_user = new User { id = owner };
            Require(GameManager.completedAchievements.Count == 0, "old false cache must not unlock a new account");
            GameManager.completedAchievements = wrong;
            Require(GameManager.completedAchievements.Count == 0, "remote badge flags alone must not unlock gameplay awards");
            Require(PlayerPrefs.GetString(oldKey) == legacy, "migration must preserve old history");
            var blank = new[] { new GameLevel { level = 1, stars = 0, score = 0 } };
            Require(!AchievementRules.Eligible(null, 0, blank, blank, blank, 0, 0).Any(), "unplayed placeholder levels must not count");
            var flash = new[] { new GameLevel { level = 1, stars = 3, score = 50 } };
            foreach (var key in AchievementRules.Eligible(null, 0, null, null, flash, 30, 0)) GameManager.UnlockAchievement(key);
            Require(GameManager.completedAchievements.SequenceEqual(new[] { "with-the-flash" }), "one Flash win must restore only Flash");
            GameManager.completedAchievements = wrong;
            Require(GameManager.completedAchievements.SequenceEqual(new[] { "with-the-flash" }), "stale remote flags must not reintroduce false awards");
            UserConstants.s_user = new User { id = owner - 1 };
            Require(GameManager.completedAchievements.Count == 0, "another new account must start locked");
            UserConstants.s_user = new User { id = owner };
            Require(GameManager.completedAchievements.SequenceEqual(new[] { "with-the-flash" }), "verified award survives account switch");
            var all = Enumerable.Range(1, 20).Select(n => new GameLevel { level = n, stars = 1 }).ToArray();
            var earned = AchievementRules.Eligible(null, 0, all, all, all, 400, 10);
            Require(earned.Count == 12 && earned.Contains("memory-athlete") && earned.Contains("stars-60"), "all level/XP/challenge thresholds restore earned badges, including Memory Athlete and 60 Stars");
            var onlyOneGame = AchievementRules.Eligible(null, 0, null, null, all, 0, 0);
            Require(onlyOneGame.Contains("memory-athlete") && onlyOneGame.Contains("flash-all-levels") && !onlyOneGame.Contains("boxes-all-levels"), "clearing all 20 levels of ONE game earns Memory Athlete");
            GameManager.completedAchievements = new List<string> { "kickstarter" };
            Require(GameManager.completedAchievements.Contains("kickstarter"), "administrative backer award is retained");
            Debug.Log("ACHIEVEMENT_EVIDENCE_VALIDATION_PASSED: bad old cache and server flags, empty/new accounts, zero-star placeholders, one Flash win, account isolation, restored thresholds, preserved history.");
        }
        finally
        {
            UserConstants.s_user = savedUser;
            // Switch ownership before clearing the dedicated test fixtures.
            var ignored = GameManager.completedAchievements;
            PlayerPrefs.DeleteKey(oldKey); PlayerPrefs.DeleteKey(newKey); PlayerPrefs.DeleteKey(otherKey); PlayerPrefs.Save();
        }
    }
    private static void Require(bool valid, string message)
    { if (!valid) throw new Exception("Achievement evidence: " + message); }
}
