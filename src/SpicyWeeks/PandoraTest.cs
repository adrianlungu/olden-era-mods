using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hex;
using Hex.QuestScript;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Action = Hex.QuestScript.Action;
using SessionData = Hex.Session.Data.Data;
using StartSide = Hex.Session.StartSide;

namespace SpicyWeeks;

/// <summary>
/// Experiment for trapped Pandora's boxes (Debug.PandoraTest). On day 2 it places two boxes next to each human
/// hero's day-1 tile: a plain one and a trapped one. Every interaction with either box is printed to Player.log,
/// and opening the trapped one spawns a strong aggressive stack beside it. What to learn:
///   - does SpawnObject ["pandora_box", tile, "false", sid] place a working box;
///   - do ObjectInteractionBefore/After [sid] fire for a box placed this way, and in which order with the reward;
///   - does a stack spawned on ObjectInteractionBefore start a fight, or just stand there.
/// </summary>
internal static class PandoraTest
{
    private const string Box = "pandora_box";
    private const int TrapValueMultiplier = 3;

    public static IEnumerable<Trigger> Triggers(int width, int height, SpawnSettings s)
    {
        var heroes = HumanHeroNodes();
        Plugin.L.LogInfo($"PandoraTest: human hero tiles [{string.Join(",", heroes)}]");
        if (heroes.Count == 0) yield break;

        var spawnActions = new List<Action>();
        var used = new HashSet<int>(heroes);
        var i = 0;
        foreach (var hero in heroes)
        {
            var plain = FreeNear(hero, width, height, used);
            var trap = FreeNear(hero, width, height, used);
            var ambush = FreeNear(trap, width, height, used);
            if (plain < 0 || trap < 0 || ambush < 0)
            {
                Plugin.L.LogWarning($"PandoraTest: no free tiles around hero tile {hero}");
                continue;
            }

            var plainSid = $"spicy_weeks_box_plain_{i}";
            var trapSid = $"spicy_weeks_box_trap_{i}";
            spawnActions.Add(SpawnQuest.MakeAction(EAction.SpawnObject, "SpawnObject", Box, plain.ToString(), "false", plainSid));
            spawnActions.Add(SpawnQuest.MakeAction(EAction.SpawnObject, "SpawnObject", Box, trap.ToString(), "false", trapSid));
            spawnActions.Add(Print($"placed {plainSid} at {plain} and {trapSid} at {trap} (ambush tile {ambush}) next to hero tile {hero}"));

            foreach (var sid in new[] { plainSid, trapSid })
            {
                yield return On(ECondition.ObjectInteractionBefore, "ObjectInteractionBefore", sid, Print($"{sid}: interaction BEFORE"));
                yield return On(ECondition.ObjectInteractionAfter, "ObjectInteractionAfter", sid, Print($"{sid}: interaction AFTER"));
            }

            var creature = Ambusher(s);
            var squad = $"spicy_weeks_box_ambush_{i}";
            yield return On(ECondition.ObjectInteractionBefore, "ObjectInteractionBefore", trapSid,
                SpawnQuest.MakeAction(EAction.SpawnSquad, "SpawnSquad", creature, ambush.ToString(),
                    (s.BaseValue * TrapValueMultiplier).ToString(), "true", squad),
                SpawnQuest.MakeAction(EAction.ChangeSquadReactionType, "ChangeSquadReactionType", squad, "Aggressive"),
                Print($"{trapSid}: TRAP sprung, {creature} at {ambush}"));
            i++;
        }

        // AnyStartTurn [month, week of month, day]: day 2, so the boxes appear after the map has settled.
        yield return new Trigger
        {
            comment = "SpicyWeeks PandoraTest spawn",
            repeat = false,
            conditionsLogic = "And",
            eConditionsLogic = EConditionsLogic.And,
            conditions = new Il2CppReferenceArray<Condition>(new[]
            {
                new Condition { c = "AnyStartTurn", condition = ECondition.AnyStartTurn, p = new Il2CppStringArray(new[] { "1", "1", "2" }) },
            }),
            actions = new Il2CppReferenceArray<Action>(spawnActions.ToArray()),
        };
    }

    private static Trigger On(ECondition condition, string name, string sid, params Action[] actions) => new()
    {
        comment = $"SpicyWeeks PandoraTest {name} {sid}",
        repeat = false,
        conditionsLogic = "And",
        eConditionsLogic = EConditionsLogic.And,
        conditions = new Il2CppReferenceArray<Condition>(new[]
        {
            new Condition { c = name, condition = condition, p = new Il2CppStringArray(new[] { sid }) },
        }),
        actions = new Il2CppReferenceArray<Action>(actions),
    };

    private static Action Print(string text) =>
        SpawnQuest.MakeAction(EAction.Print, "Print", $"[SpicyWeeks] PandoraTest {text}");

    private static string Ambusher(SpawnSettings s)
    {
        var fitting = Creatures.Fitting(s.Pool, s.BaseValue * TrapValueMultiplier);
        return (fitting.Length > 0 ? fitting : s.Pool).Last().SquadId;
    }

    // Closest spawnable tile within a few steps of the centre that nothing else took.
    private static int FreeNear(int centre, int width, int height, HashSet<int> used)
    {
        if (centre < 0) return -1;
        int cx = centre % width, cz = centre / width;
        for (var r = 1; r <= 4; r++)
        for (var dz = -r; dz <= r; dz++)
        for (var dx = -r; dx <= r; dx++)
        {
            if (Math.Max(Math.Abs(dx), Math.Abs(dz)) != r) continue;
            int x = cx + dx, z = cz + dz;
            if (x < 0 || z < 0 || x >= width || z >= height) continue;
            var t = x + z * width;
            if (used.Contains(t) || !Terrain.IsSpawnable(t)) continue;
            used.Add(t);
            return t;
        }

        return -1;
    }

    private static PropertyInfo _data;

    private static List<int> HumanHeroNodes()
    {
        var nodes = new List<int>();
        try
        {
            _data ??= typeof(dcy).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.PropertyType == typeof(SessionData));
            var data = _data?.GetValue(dcy.me) as SessionData;
            var sides = GameSeed.StartInfo()?.sides;
            if (data?.heroes?.list == null || sides == null) return nodes;

            foreach (var hero in data.heroes.list)
            {
                if (hero == null || hero.sideId < 0 || hero.sideId >= sides.Length) continue;
                if (sides[hero.sideId]?.eType == StartSide.EType.Player) nodes.Add(hero.node);
            }
        }
        catch (Exception e)
        {
            Plugin.L.LogWarning($"PandoraTest: reading heroes failed ({e.Message})");
        }

        return nodes;
    }
}
