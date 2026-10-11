using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace SpicyWeeks;

[BepInPlugin(Guid, Name, Version)]
public class Plugin : BasePlugin
{
    public const string Guid = "olden-era-mods.spicy-weeks";
    public const string Name = "Spicy Weeks";
    public const string Version = "0.9.0";

    internal static ManualLogSource L;

    internal static ConfigEntry<bool> SpawnEnabled;
    internal static ConfigEntry<int> TilesPerStack;
    internal static ConfigEntry<int> StackValue;
    internal static ConfigEntry<int> WeeklyGrowthPercent;
    internal static ConfigEntry<int> SpawnChancePercent;
    internal static ConfigEntry<int> FirstWeek;
    internal static ConfigEntry<int> MinWeeksBetween;
    internal static ConfigEntry<bool> OverrideWeekEffect;
    internal static ConfigEntry<string> Moods;
    internal static ConfigEntry<string> BannedEffects;
    internal static ConfigEntry<bool> RevealSpawns;

    internal static HashSet<string> BannedEffectSids() =>
        new(BannedEffects.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    public override void Load()
    {
        L = Log;

        SpawnEnabled = Config.Bind("Invasions", "Enabled", true,
            "Neutral invasion weeks on maps without their own script (all random maps).");
        TilesPerStack = Config.Bind("Invasions", "TilesPerStack", 400,
            "Invasion size: one stack per this many map tiles (160x160 -> 64 stacks, 72x72 -> 12).");
        StackValue = Config.Bind("Invasions", "StackValue", 4000,
            "Strength of each stack in week 2; the game scales the stack size to this.");
        WeeklyGrowthPercent = Config.Bind("Invasions", "WeeklyGrowthPercent", 12,
            "Compound growth of stack strength per week (12 -> ~10k by week 10, ~30k by week 20).");
        SpawnChancePercent = Config.Bind("Invasions", "ChancePercent", 25,
            "Chance (0-100) that an eligible week is an invasion week.");
        FirstWeek = Config.Bind("Invasions", "FirstWeek", 3,
            "Earliest game week an invasion can happen (week 2 starts on day 8).");
        MinWeeksBetween = Config.Bind("Invasions", "MinWeeksBetween", 2,
            "Quiet weeks required between two invasions.");
        OverrideWeekEffect = Config.Bind("Invasions", "OverrideWeekEffect", true,
            "On invasion weeks, replace the astrologer's week effect and announcement with the invasion.");
        Moods = Config.Bind("Invasions", "Moods", "Aggressive,Negative,Common,Friendly,Peaceful",
            "Moods an invasion can roll, comma-separated; list one twice to make it more likely. " +
            "They set how likely stacks are to offer to join a hero, from " +
            "Aggressive (Raging: least likely) through Negative (Restless), Common (the themed week) and " +
            "Friendly (Wandering) to Peaceful (Travelling: most likely).");
        BannedEffects = Config.Bind("Astrology", "BannedEffects", "astrologist_month_5",
            "Comma-separated week/month effects that never happen; a banned roll becomes Week/Month of Calm. " +
            "astrologist_month_5 = Month of Clarity (reveals the whole map), astrologist_week_5 = Week of Dusk, " +
            "astrologist_week_6 = 3-day week. Sids are in DB/weeks_info.json inside Core.zip.");
        RevealSpawns = Config.Bind("Debug", "RevealSpawns", false,
            "Clear the fog around each spawned stack (for testing).");

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(Plugin).Assembly);
        AddComponent<QuestInjector>();
        L.LogInfo($"{Name} {Version} loaded, {harmony.GetPatchedMethods().Count()} methods patched");
    }
}
