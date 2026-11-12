using System;
using System.Collections.Generic;

namespace SpicyWeeks;

internal record Invasion(int AbsoluteWeek, Wave[] Waves, string WeekName, string Announcement, bool Spicy = false);

/// <summary>
/// One day's spawns in an invasion week: a stack per squad id, all at the same value and reaction.
/// Retarget waves spawn nothing and instead change the reaction of every stack spawned earlier that week.
/// </summary>
internal record Wave(int Day, string[] SquadIds, string Reaction, int Value, string Label, bool Retarget = false);

/// <summary>
/// The current game's invasion weeks, keyed by 0-based absolute week (week 1 of month 1 = 0, day 8 = 1).
/// Rolled once per game from the lobby seed, so every client holds the same schedule.
/// </summary>
internal static class InvasionSchedule
{
    private static readonly Dictionary<int, Invasion> Weeks = new();

    public static void Set(IEnumerable<Invasion> invasions)
    {
        Weeks.Clear();
        foreach (var invasion in invasions) Weeks[invasion.AbsoluteWeek] = invasion;
    }

    public static void Clear() => Weeks.Clear();

    public static Invasion For(int absoluteWeek) => Weeks.TryGetValue(absoluteWeek, out var i) ? i : null;

    public static int AbsoluteWeek(int month, int weekOfMonth) => (month - 1) * 4 + (weekOfMonth - 1);

    public static IReadOnlyCollection<int> AbsoluteWeeks => Weeks.Keys;
}
