using System;
using System.Collections.Generic;
using System.Linq;

namespace SpicyWeeks;

/// <summary>
/// How an invasion's stacks treat heroes. Each mood maps to a game squad reaction type
/// (Hex.Configs.ESquadReactionType, set with the ChangeSquadReactionType quest action) and gives the week
/// its name and announcement. The point is variety: joining odds differ from week to week instead of every
/// invasion being equally willing to join.
/// </summary>
internal record Mood(string Reaction, Func<string, string> Title, Func<string, int, string> Announcement);

internal static class Moods
{
    private static readonly Mood[] All =
    {
        new("Aggressive", c => $"Week of the Raging {c}",
            (c, n) => $"{c} rampage across the land! {n} hostile stacks have appeared, in no mood to bargain."),
        new("Negative", c => $"Week of the Restless {c}",
            (c, n) => $"Restless {c} roam the land. {n} wary stacks have appeared; few will hear a hero out."),
        // Common keeps the creature's themed week name (null title); see Creatures.
        new("Common", null,
            (c, n) => $"{c} invade the land! {n} roaming stacks have appeared across the map."),
        new("Friendly", c => $"Week of the Wandering {c}",
            (c, n) => $"Wandering {c} travel the land. {n} stacks have appeared; some may join a strong hero."),
        new("Peaceful", c => $"Week of the Travelling {c}",
            (c, n) => $"Travelling {c} pass through the land. {n} peaceful stacks have appeared, looking for a leader."),
    };

    /// <summary>Moods named in the setting; a mood listed twice is twice as likely. Unknown names are skipped.</summary>
    public static Mood[] Parse(string setting)
    {
        var moods = new List<Mood>();
        foreach (var name in setting.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var mood = All.FirstOrDefault(m => m.Reaction.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (mood != null) moods.Add(mood);
            else Plugin.L.LogWarning($"Unknown mood '{name}' in Moods setting; valid: {string.Join(", ", All.Select(m => m.Reaction))}");
        }

        return moods.Count > 0 ? moods.ToArray() : new[] { All[0] };
    }
}
