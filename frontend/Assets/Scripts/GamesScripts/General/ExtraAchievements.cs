using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Achievements added after the original twelve. They are built at runtime from this table, so no
/// scene or asset lists need editing. Badge images live in Resources/Achievements/Extra/&lt;key&gt;.png.
/// </summary>
public static class ExtraAchievements
{
    public struct Def
    {
        public string Key, Title, Description;
        public Def(string key, string title, string description) { Key = key; Title = title; Description = description; }
    }

    public static readonly Def[] All =
    {
        new Def("focus-first", "First time focus!", "Start your first Focus game."),
        new Def("focus-pairs-first", "Beginner class", "Start Focus Pairs for the first time."),
        new Def("focus-pairs-professor", "Professor", "Reach the highest Focus Pairs level in one run starting at level 1."),
        new Def("focus-boxes-first", "First catch", "Start Focus Boxes for the first time."),
        new Def("focus-boxes-max", "Ball breaker", "Reach the highest Focus Boxes level, or complete it."),
        new Def("focus-flash-first", "Fast eyes", "Start Focus Flash for the first time."),
        new Def("focus-flash-vision", "Vision", "Reach level 12 in Focus Flash, or complete it."),
        new Def("focus-chain-first", "In line", "Start Focus Chain for the first time."),
        new Def("focus-chain-max", "First in line", "Reach the highest Focus Chain level, or complete it."),
        new Def("chain-first", "First chain!", "Start your first Chain round."),
        new Def("athlete-one-heart-boxes", "Memory Athlete: Boxes", "Clear the extra Boxes level with one heart. 500 XP, once."),
        new Def("athlete-one-heart-flash", "Memory Athlete: Flash", "Clear the extra Flash level with one heart. 500 XP, once."),
        new Def("athlete-one-heart-chain", "Memory Athlete: Chain", "Clear the extra Chain level with one heart. 500 XP, once."),
        new Def("chain-all", "Break the Chains!", "Complete all 20 Chain levels."),
        new Def("memory-athlete", "Memory Athlete", "Clear level 20 in Pairs, Flash or Boxes."),
        new Def("all-star", "All Star", "Earn 3 stars on all 20 levels in Pairs, Flash and Boxes."),
        new Def("stars-60", "60 Stars", "Collect 60 stars in total: one game's worth."),
        new Def("stars-120", "Universe of Stars", "Collect 120 stars in total: two games' worth."),
        new Def("boxes-star", "Box Star", "Earn 3 stars on all 20 levels in Boxes."),
        new Def("flash-star", "Flashing Star", "Earn 3 stars on all 20 levels in Flash."),
        new Def("pairs-star", "Pair of Stars", "Earn 3 stars on all 20 levels in Pairs."),
        new Def("tip-the-deck", "Deck Decoder", "Read The Deck tip and its card associations."),
        new Def("tip-major-system", "Major Master", "Read the Major System tip. It unlocks Help in the games."),
        new Def("tip-link-method", "Master of Links", "Read the Link Method tip."),
        new Def("tip-fun", "Fun Factor", "Read the tip about making it fun."),
        new Def("tip-memory-palace", "Palace Builder", "Read the Memory Palace tip."),
        new Def("tip-senses", "Sense Maker", "Read the tip about activating your senses."),
        new Def("tip-emotions", "Booster of Emotions", "Read the tip about emotions."),
        new Def("tip-retrieval", "Retrieval Pro", "Read the tip about practice and recall."),
        new Def("tip-myth", "Visualizer", "Read the tip about visualisation."),
        new Def("tip-mindset", "Master of Mindset", "Read the Mindset tip."),
        new Def("tip-focus", "Focus Finder", "Read the Focus tip."),
        new Def("tip-born", "Born to Learn", "Read the tip about being born with a great memory."),
        new Def("tip-dream", "Daydreamer", "Read the Daydream tip."),
        new Def("video-intro", "Introduced", "Watch the Introduction video to the end."),
        new Def("video-link", "LinKing", "Watch the Link Method video to the end."),
        new Def("video-associations", "Associator", "Watch the Associations video to the end."),
        new Def("video-palace", "Architect", "Watch the Memory Palace video to the end."),
    };

    private static List<Achievement> built;

    public static IEnumerable<string> Keys { get { return All.Select(d => d.Key); } }

    public static bool IsExtra(string key) { return All.Any(d => d.Key == key); }

    public static List<Achievement> Built()
    {
        if (built != null) return built;
        built = new List<Achievement>();
        foreach (var def in All)
        {
            var achievement = ScriptableObject.CreateInstance<Achievement>();
            achievement.name = def.Key;
            achievement.constantString = def.Key;
            achievement.title = def.Title;
            achievement.description = def.Description;
            if(def.Key.StartsWith("athlete-one-heart-")){achievement.icon=Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01");built.Add(achievement);continue;}
            achievement.icon = def.Key.StartsWith("chain-") ? Resources.Load<Sprite>("Achievements/Extra/tip-link-method") : def.Key == "tip-the-deck" ? DeckCatalog.Face(17) : Resources.Load<Sprite>("Achievements/Extra/" + def.Key);
            built.Add(achievement);
        }
        return built;
    }

    /// <summary>Adds the extra achievements to a list (typically the serialized scene list). Safe to call repeatedly.</summary>
    public static void AppendTo(List<Achievement> list)
    {
        if (list == null) return;
        foreach (var extra in Built())
            if (!list.Any(a => a != null && a.constantString == extra.constantString)) list.Add(extra);
    }
}
