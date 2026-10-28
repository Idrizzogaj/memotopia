using UnityEngine;
using UnityEngine.UI;

public static class ChallengeStatisticsPanel
{
    public static void Apply(Transform host, UserStatistics statistics)
    {
        // Keep the serialized panel reference; retire only its old fixed-width presentation.
        foreach (Transform child in host) child.gameObject.SetActive(false);
        var old = host.Find("Challenges statistics");
        if (old != null) { old.name += " retired"; CardFaces.DestroyGenerated(old.gameObject); }
        var background = host.GetComponent<Image>();
        if (background != null) { background.sprite = null; background.color = Color.clear; background.raycastTarget = false; }
        foreach (var layout in host.GetComponents<LayoutGroup>()) layout.enabled = false;
        var stack = host.GetComponent<VerticalLayoutGroup>() ?? host.gameObject.AddComponent<VerticalLayoutGroup>();
        stack.enabled = true; stack.padding = new RectOffset(); stack.spacing = 0;
        stack.childControlWidth = stack.childControlHeight = true; stack.childForceExpandHeight = false;
        var size = host.GetComponent<LayoutElement>(); if (size != null) size.minHeight = size.preferredHeight = -1;
        var root = StatisticsCards.Section(host, "Challenges statistics", "CHALLENGES", TrainingHubLayout.MemoryTone);
        var scope = StatisticsCards.Block(root.transform, "Scope", 48, Color.clear);
        StatisticsCards.Label(scope.transform, "Completed challenges · Synced with your account", Vector2.zero, Vector2.one, 22, ChainGame.Ink);
        string[] names = { "Games", "Matches", "Single", "Losses", "Draw" };
        string[] descriptions = { "All completed challenges", "Match! duels", "Single-game challenges", "Challenges lost", "Challenges drawn" };
        string[] values = { statistics == null ? "—" : statistics.numberOfChallenges.ToString(), statistics == null || !statistics.challengeFormatsAvailable ? "—" : statistics.numberOfMatchChallenges.ToString(), statistics == null || !statistics.challengeFormatsAvailable ? "—" : statistics.numberOfSingleChallenges.ToString(), statistics == null ? "—" : statistics.numberOfLossChallenges.ToString(), statistics == null ? "—" : statistics.numberOfDrawChallenges.ToString() };
        var grid = StatisticsCards.Grid(root.transform, "Challenge format stats", 210);
        for (int i = 0; i < names.Length; i++)
        {
            var card = StatisticsCards.Card(grid, names[i], TrainingHubLayout.MemoryTone).transform;
            var value = StatisticsCards.Label(card, values[i], new Vector2(.06f, .26f), new Vector2(.94f, .78f), 65, Color.white); value.name = "Value";
            StatisticsCards.Label(card, descriptions[i], new Vector2(.04f, .04f), new Vector2(.96f, .25f), 20, Color.white);
        }
        StatisticsCards.Reflow(host);
    }
}
