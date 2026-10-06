using System;
using System.Collections.Generic;

namespace SpicyWeeks;

internal record Invasion(int AbsoluteWeek, string SquadId, string WeekName, string Creatures, int Stacks, int Value);

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
