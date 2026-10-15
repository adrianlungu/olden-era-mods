using System;
using System.Collections.Generic;
using System.Linq;

namespace SpicyWeeks;

/// <summary>
/// A single-creature squad an invasion can spawn: game squad id, its faction, the value range the game can
/// scale it to (dynamicSquad.requiredValue .. randomSquad.maxValue), the announcement name (plural, title case),
/// and an optional themed week name.
/// </summary>
internal record Creature(string SquadId, string Faction, int Min, int Max, string Name, string Week = null);

internal static class Creatures
{
    public static readonly string[] Factions = { "neutral", "human", "demon", "dungeon", "nature", "undead", "unfrozen" };

    // DB/squads/squads_neutral.
    private static readonly Creature[] Neutral =
    {
        new("squad_neutral_one_unit_t1_1", "neutral", 1, 10001, "Pixies", "Week of the Fair Folk"),
        new("squad_neutral_one_unit_t2_1", "neutral", 2503, 15002, "Halflings", "Week of the Halflings"),
        new("squad_neutral_one_unit_t2_2", "neutral", 2503, 15002, "Peasants", "Week of the Peasant Revolt"),
        new("squad_neutral_one_unit_t3_1", "neutral", 5003, 30003, "Gorillas", "Week of the Apes"),
        new("squad_neutral_one_unit_t3_2", "neutral", 5003, 30003, "Animated Armors", "Week of Haunted Steel"),
        new("squad_neutral_one_unit_t4_1", "neutral", 10004, 60004, "Horned Kittens", "Week of the Horned Kittens"),
        new("squad_neutral_one_unit_t4_2", "neutral", 10004, 60004, "Primal Remnants", "Week of the Primal Echo"),
        new("squad_neutral_one_unit_t4_3", "neutral", 10004, 60004, "Star Children", "Week of Falling Stars"),
        new("squad_neutral_one_unit_t4_4", "neutral", 10004, 60004, "Unicorns", "Week of the Unicorn"),
        new("squad_neutral_one_unit_t5_1", "neutral", 15005, 120005, "Fairy Dragons", "Week of the Fairy Dragon"),
        new("squad_neutral_one_unit_t5_2", "neutral", 15005, 120005, "Giant Frogs", "Week of the Frog Plague"),
        new("squad_neutral_one_unit_t6_1", "neutral", 20006, 1800060, "Coatls", "Week of the Feathered Serpent"),
        new("squad_neutral_one_unit_t6_2", "neutral", 20006, 1800060, "Mech Guards", "Week of the Iron Guard"),
        new("squad_neutral_one_unit_t6_3", "neutral", 20006, 1800060, "Dragon Hunters", "Week of the Hunt"),
        new("squad_neutral_one_unit_t7_1", "neutral", 25007, 6000000, "Sentinels", "Week of the Sentinels"),
        new("squad_neutral_one_unit_t8_1", "neutral", 25007, 24000000, "Lich Dragons", "Week of Pestilence"),
    };

    // DB/squads/squads_<faction>/one_tier_units_squads: squad_<faction>_one_unit_t<tier>_<n>, n = base, upgrade,
    // alternate upgrade. All factions share the same per-tier value ranges. Names from Lang/english (<unit>_name).
    private static readonly (int Min, int Max)[] FactionTiers =
        { (1, 10001), (2502, 15002), (5003, 30003), (10004, 60004), (15005, 120005), (20006, 180006), (25007, 240007) };

    private static readonly Dictionary<string, string[]> FactionNames = new()
    {
        ["human"] = new[]
        {
            "Swordsmen", "Guard Captains", "Sun’s Aegises", "Crossbowmen", "Austringers", "Marksmen",
            "Griffins", "Temple Griffins", "Guardian Griffins", "Lightweavers", "Hierophants", "Sun Heralds",
            "Cavalry", "Noble Cavalry", "Sunspear Cavalry", "Inquisitors", "Mothers Superior", "Excommunicators",
            "Angels", "Archangels", "Apotheoses",
        },
        ["demon"] = new[]
        {
            "Parasites", "Warden Parasites", "Ravager Parasites", "Locusts", "Overgrown Locusts", "Harvester Locusts",
            "Hornets", "Chanters", "Stingers", "Scorpions", "Volcanic Scorpions", "Spelaean Scorpions",
            "Reavers", "Menacing Reavers", "Maniacal Reavers", "Waurms", "Devourers", "Pyroboroi",
            "Hive Queens", "Hive Mothers", "Hive Huntresses",
        },
        ["dungeon"] = new[]
        {
            "Troglodytes", "Infernal Troglodytes", "Toxic Troglodytes", "Infiltrators", "Guile Infiltrators", "Bleak Infiltrators",
            "Onyx Dancers", "Jasper Dancers", "Aureate Dancers", "Minotaurs", "Minotaur Lords", "Minotaur Vanguards",
            "Medusas", "Medusa Sculptors", "Medusa Queens", "Hydras", "Chthonic Hydras", "Infernal Hydras",
            "Cave Dragons", "Black Dragons", "Ashen Dragons",
        },
        ["nature"] = new[]
        {
            "Fauns", "Faun Archers", "Faun Warriors", "Hoplets", "Dawn Hoplets", "Dusk Hoplets",
            "Vine Iriyads", "Fungal Iriyads", "Crystal Iriyads", "Naiads", "Vernal Naiads", "Brumal Naiads",
            "Herbomancers", "Sporemancers", "Murmurmancers", "Qilin", "Thunder Qilin", "Mist Qilin",
            "Phoenixes", "Flaming Phoenixes", "Energy Phoenixes",
        },
        ["undead"] = new[]
        {
            "Skeletons", "Skeleton Warriors", "Skeleton Archers", "Wights", "Wraiths", "Phantasms",
            "Undead Pets", "Barghests", "Armored Hounds", "Graverobbers", "Merchants of Death", "Kennelmasters",
            "Liches", "Pestilent Liches", "Sanguine Liches", "Dread Knights", "Avatars of War", "Hollow Reapers",
            "Vampires", "Vampire Lords", "Vampire Scholars",
        },
        ["unfrozen"] = new[]
        {
            "Ra’Shoth", "Stinging Ra’Shoth", "Ferocious Ra’Shoth", "Cultists", "Binders", "Votaries",
            "Aga’Shoth Riders", "Aga’Shoth Tamers", "Aga’Shoth Mat’has", "Grand Shoth", "Unspeakable Shoth", "Unthinkable Shoth",
            "Concubi", "Mistresses of Chains", "Bewitchers", "Arbitrators", "Rift Arbitrators", "Bloated Arbitrators",
            "Abyssal Envoys", "Abyssal Overseers", "Abyssal Executors",
        },
    };

    private static readonly Creature[] All = Neutral.Concat(
        from f in FactionNames
        from i in Enumerable.Range(0, f.Value.Length)
        let tier = i / 3
        select new Creature($"squad_{f.Key}_one_unit_t{tier + 1}_{i % 3 + 1}", f.Key,
            FactionTiers[tier].Min, FactionTiers[tier].Max, f.Value[i])).ToArray();

    /// <summary>Creatures of the factions named in the setting ("all" for every faction). Unknown names are skipped.</summary>
    public static Creature[] Pool(string setting)
    {
        var wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in setting.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (name.Equals("all", StringComparison.OrdinalIgnoreCase)) wanted.UnionWith(Factions);
            else if (Factions.Contains(name, StringComparer.OrdinalIgnoreCase)) wanted.Add(name);
            else Plugin.L.LogWarning($"Unknown faction '{name}' in Factions setting; valid: all, {string.Join(", ", Factions)}");
        }

        var pool = All.Where(c => wanted.Contains(c.Faction)).ToArray();
        return pool.Length > 0 ? pool : Neutral;
    }

    /// <summary>
    /// Creatures that can be scaled to the value. Past every creature's range, the strongest ones are used
    /// (the game caps their size), so late-game invasions keep coming with any faction selection.
    /// </summary>
    public static Creature[] Fitting(Creature[] pool, int value)
    {
        var fitting = pool.Where(c => c.Min <= value && value <= c.Max).ToArray();
        if (fitting.Length > 0) return fitting;
        var top = pool.Max(c => c.Max);
        return value > top ? pool.Where(c => c.Max == top).ToArray() : Array.Empty<Creature>();
    }
}
