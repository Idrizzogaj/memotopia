using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Models;

public static class MemoryStatisticsPanel
{
    public sealed class Record { public string game; public bool known; public int highest, levels, stars, perfect; }
    public static string[] Games(bool says) { return says ? new[] { "Pairs", "Boxes", "Flash", "Chain", "Says" } : new[] { "Pairs", "Boxes", "Flash", "Chain" }; }

    // These are saved level awards, not an invented count of historical attempts or wins.
    public static Record Read(string game)
    {
        var result = new Record { game = game };
        if (GameManager.AchievementUserId <= 0) return result;
        GameLevel[] levels = null;
        if (game == "Pairs" || game == "Boxes" || game == "Flash")
        {
            levels = LevelProgress.Get(game);
            if (levels == null) return result;
        }
        result.known = true;
        string saysKey = "says.preview.v1." + GameManager.AchievementUserId;
        for (int n = 1; n <= 20; n++)
        {
            bool completed = game == "Chain" ? ChainProgress.Completed(n) : game == "Says" && (PlayerPrefs.GetInt(saysKey, 0) & (1 << (n - 1))) != 0;
            int stars = game == "Chain" ? ChainProgress.Stars(n) : game == "Says" ? PlayerPrefs.GetInt(saysKey + ".stars." + n, completed ? 1 : 0) : StarRules.StarsOf(levels, n);
            stars = Mathf.Clamp(stars, 0, 3);
            if (completed || stars > 0) { result.levels++; result.highest = n; }
            result.stars += stars;
            if (stars == 3) result.perfect++;
        }
        return result;
    }

    public static void Apply(Transform before, bool includeSays)
    {
        if (before == null) return;
        var root = StatisticsCards.Section(before.parent, "Memory statistics", "MEMORY", TrainingHubLayout.MemoryTone);
        root.transform.SetSiblingIndex(before.GetSiblingIndex());
        var scope = StatisticsCards.Block(root.transform, "Scope", 60, Color.clear);
        StatisticsCards.Label(scope.transform, "Saved level progress · Practice records below\nPairs / Boxes / Flash sync · " + (includeSays ? "Chain / Says" : "Chain") + " on this device", Vector2.zero, Vector2.one, 20, ChainGame.Ink);
        var data = Games(includeSays).Select(Read).ToArray();
        var summary = StatisticsCards.Block(root.transform, "Totals", 86, Color.white);
        bool allKnown = data.All(r => r.known);
        string[] values = { (allKnown ? data.Sum(r => r.levels).ToString() : "—") + "\nLevels awarded", (allKnown ? data.Sum(r => r.stars).ToString() : "—") + "\nStars", (allKnown ? data.Sum(r => r.perfect).ToString() : "—") + "\nThree-star levels" };
        for (int i = 0; i < 3; i++) StatisticsCards.Label(summary.transform, values[i], new Vector2(i / 3f, 0), new Vector2((i + 1) / 3f, 1), 25, TrainingHubLayout.MemoryTone);
        var grid = StatisticsCards.Grid(root.transform, "Game records");
        for (int i = 0; i < data.Length; i++)
        {
            var r = data[i]; var card = StatisticsCards.Card(grid, r.game, TrainingHubLayout.MemoryTone, i).transform;
            StatisticsCards.Label(card, r.known && r.highest > 0 ? "L" + r.highest : "—", new Vector2(.05f, .39f), new Vector2(.53f, .77f), 65, Color.white);
            StatisticsCards.Label(card, "HIGHEST", new Vector2(.05f, .29f), new Vector2(.53f, .41f), 19, Color.white);
            StatisticsCards.Label(card, r.known ? r.stars + "/60" : "—", new Vector2(.58f, .42f), new Vector2(.95f, .71f), 42, Color.white);
            StatisticsCards.Label(card, "STARS", new Vector2(.54f, .29f), new Vector2(.98f, .41f), 19, Color.white);
            StatisticsCards.Label(card, r.known ? r.levels + "/20 levels · " + r.perfect + " with three stars" : "Progress not available yet", new Vector2(.04f, .15f), new Vector2(.96f, .28f), 19, Color.white);
            bool athlete = r.game == "Says" ? PlayerPrefs.GetInt("says.preview.v1." + GameManager.AchievementUserId + ".athlete", 0) == 1 : TrainingAthleteTile.Completed(r.game);
            string note = r.game == "Pairs" ? "Saved level awards" : "Memory Athlete · " + (athlete ? "Completed" : "Not yet");
            StatisticsCards.Label(card, note, new Vector2(.04f, .02f), new Vector2(.96f, .15f), 18, Color.white);
        }
        var practice = StatisticsCards.Block(root.transform, "Practice records", 0, StatisticsCards.Paper);
        practice.GetComponent<LayoutElement>().preferredHeight = PracticeStatisticsPanel.Draw(practice.transform, "Memory", includeSays);
        StatisticsCards.Reflow(before.parent);
    }
}
