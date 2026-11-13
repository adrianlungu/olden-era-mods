using System;
using System.Collections.Generic;
using System.Linq;

namespace SpicyWeeks;

/// <summary>
/// Spicy Week: a rare invasion week where something new happens every day. Each day rolls a "dish" from a menu
/// (a swarm of small stacks, a few champions, free mercenaries, a rift of top-tier creatures, a mood swing that
/// turns the week's earlier stacks hostile or docile, or a calm day), and each day's spawns are stronger than the
/// day before. Only the astrologer announces the week; the days themselves are a surprise.
/// Rolled from the same seeded Random as the rest of the schedule, so every multiplayer client gets the same menu.
/// </summary>
internal static class SpicyWeek
{
    public const string WeekName = "Spicy Week";
    public const string Announcement =
        "The stars are on fire! Something new will happen every day this week: hordes, champions, mercenaries, or worse.";

    private const double DailyEscalation = 0.15;

    private enum Dish { Swarm, Champions, Mercenaries, Rift, MoodSwing, Calm, Beasts }

    // Weighted menu; the first day always serves something that spawns.
    private static readonly (Dish Dish, int Weight)[] Menu =
    {
        (Dish.Swarm, 3), (Dish.Champions, 2), (Dish.Mercenaries, 2), (Dish.Rift, 2), (Dish.MoodSwing, 2), (Dish.Calm, 1),
    };

    public static Wave[] Roll(int stacks, int value, Random rng, SpawnSettings s)
    {
        var waves = new List<Wave>();
        var spawned = false;
        for (var day = 1; day <= 7; day++)
        {
            var dish = RollDish(rng, canSwing: spawned);
            var dayValue = (int)(value * (1 + DailyEscalation * (day - 1)));
            var wave = Cook(dish, day, stacks, dayValue, rng, s);
            if (wave == null) continue;
            waves.Add(wave);
            spawned |= wave.SquadIds.Length > 0;
        }

        return waves.ToArray();
    }

    private static Dish RollDish(Random rng, bool canSwing)
    {
        var menu = Menu.Where(m => canSwing || (m.Dish != Dish.MoodSwing && m.Dish != Dish.Calm)).ToArray();
        var roll = rng.Next(menu.Sum(m => m.Weight));
        foreach (var (dish, weight) in menu)
            if ((roll -= weight) < 0) return dish;
        return menu[0].Dish;
    }

    private static Wave Cook(Dish dish, int day, int stacks, int value, Random rng, SpawnSettings s)
    {
        switch (dish)
        {
            case Dish.Swarm:
                return Spawn(day, "a swarm", stacks, value / 4, RandomMood(rng, s), rng, s);
            case Dish.Champions:
                return Spawn(day, "champions", Math.Max(1, stacks / 16), value * 6, RandomMood(rng, s), rng, s);
            case Dish.Mercenaries:
                return Spawn(day, "mercenaries", Math.Max(2, stacks / 6), value, "Docile", rng, s);
            case Dish.Rift:
                return Spawn(day, "a rift", Math.Max(2, stacks / 8), value * 3, rng.Next(2) == 0 ? "Aggressive" : "Negative", rng, s);
            case Dish.Beasts:
                // Weak at first, scattered, and they grow every day for the rest of the game.
                return Spawn(day, "beasts", Math.Max(3, stacks / 10), value, RandomMood(rng, s), rng, s) with { Growing = true };
            case Dish.MoodSwing:
                return new Wave(day, Array.Empty<string>(), rng.Next(2) == 0 ? "Aggressive" : "Docile", 0, "a mood swing",
                    Retarget: true);
            default:
                return new Wave(day, Array.Empty<string>(), null, 0, "a calm day");
        }
    }

    // The menu's spawning dishes also run as ordinary one-day invasion weeks (SpecialChancePercent).
    private static readonly (Dish Dish, string Week, string Announcement)[] Specials =
    {
        (Dish.Swarm, "Week of the Swarm",
            "A swarm descends on the land! Countless small stacks block the roads; none of them alone is a threat."),
        (Dish.Champions, "Week of Champions",
            "Champions walk the land. A few stacks of fearsome strength have appeared; approach with care."),
        (Dish.Mercenaries, "Week of Mercenaries",
            "Mercenaries seek a paymaster. A few stacks will join the first hero to reach them, free of charge."),
        (Dish.Rift, "Week of the Rift",
            "A rift tears open! Stacks of the mightiest creatures pour out, hostile to all."),
        (Dish.Beasts, "Week of the Beasts",
            "Beasts awaken across the land. They grow stronger every single day, forever; hunt them while you can."),
    };

    /// <summary>A one-day special invasion week (swarm, champions, mercenaries or rift).</summary>
    public static Invasion Special(int absoluteWeek, int stacks, int value, Random rng, SpawnSettings s)
    {
        var (dish, week, announcement) = Specials[rng.Next(Specials.Length)];
        var forced = Plugin.ForceSpecialWeek.Value?.Trim();
        if (!string.IsNullOrEmpty(forced) && Specials.FirstOrDefault(x => x.Week.Equals(forced, StringComparison.OrdinalIgnoreCase)) is { Week: not null } f)
            (dish, week, announcement) = f;
        return new Invasion(absoluteWeek, new[] { Cook(dish, 1, stacks, value, rng, s) }, week, announcement);
    }

    private static string RandomMood(Random rng, SpawnSettings s) => s.Moods[rng.Next(s.Moods.Length)].Reaction;

    // Every stack a different creature that fits the value; below every creature's range, the weakest ones.
    private static Wave Spawn(int day, string label, int count, int value, string reaction, Random rng, SpawnSettings s)
    {
        var fitting = Creatures.Fitting(s.Pool, value);
        if (fitting.Length == 0)
        {
            var low = s.Pool.Min(c => c.Min);
            fitting = s.Pool.Where(c => c.Min == low).ToArray();
            value = low;
        }

        var squads = Enumerable.Range(0, count).Select(_ => fitting[rng.Next(fitting.Length)].SquadId).ToArray();
        return new Wave(day, squads, reaction, value, label);
    }
}
