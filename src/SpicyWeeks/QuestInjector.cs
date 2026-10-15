using System;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace SpicyWeeks;

/// <summary>
/// Gives script-less maps (every random map) a quest manager running our generated script.
/// The session's per-frame update ticks whatever manager sits in its quest slot, so once the slot
/// is filled the game drives StartWeek triggers and actions itself. Maps with their own script
/// already have a manager in the slot and are left alone.
/// Runs as a component rather than a Harmony patch to stay clear of obfuscated hook targets.
/// </summary>
public class QuestInjector : MonoBehaviour
{
    private const int SessionQuestManager = 160;
    private const int SettleFrames = 120;

    private static MethodInfo _factory;

    private IntPtr _session;
    private int _readyFrames;
    private bool _handled;

    public QuestInjector(IntPtr ptr) : base(ptr) { }

    private unsafe void Update()
    {
        var session = dbx.me;
        if (session == null)
        {
            _session = IntPtr.Zero;
            return;
        }

        if (session.Pointer != _session)
        {
            _session = session.Pointer;
            InvasionSchedule.Clear();
            Terrain.Clear();
            WeekOverride.RestoreText();
            _readyFrames = 0;
            _handled = false;
        }

        if (_handled || !Plugin.SpawnEnabled.Value) return;

        var slot = (IntPtr*)(_session + SessionQuestManager);
        if (*slot != IntPtr.Zero)
        {
            _handled = true;
            Plugin.L.LogInfo("Session already has a quest manager (map script); not injecting");
            return;
        }

        if (!MapInfo.TryGetSize(out var width, out var height)) return;
        // Snapshot the terrain on the first frame the map exists, before anyone can move, so every
        // multiplayer client picks the same spawn tiles.
        if (_readyFrames == 0) Terrain.Capture(width, height);
        if (++_readyFrames < SettleFrames) return;
        _handled = true;

        try
        {
            var power = 1.0;
            var difficulty = "ignored";
            if (Plugin.MatchNeutralDifficulty.Value) power = Difficulty.NeutralPower(out difficulty);
            var stackValue = (int)Math.Round(Plugin.StackValue.Value * power);
            var settings = new SpawnSettings(Plugin.TilesPerStack.Value, stackValue,
                Plugin.WeeklyGrowthPercent.Value / 100.0, Plugin.SpawnChancePercent.Value, Plugin.FirstWeek.Value,
                Plugin.MinWeeksBetween.Value, Plugin.RevealSpawns.Value, Weeks: 200, Moods.Parse(Plugin.Moods.Value),
                Creatures.Pool(Plugin.Factions.Value), Plugin.MixedChancePercent.Value);
            // Every client shares the lobby seed → same script, keeping multiplayer in lockstep.
            if (!GameSeed.TryGet(out var seed))
            {
                seed = width * 7919 + height;
                Plugin.L.LogWarning("Game seed unavailable; falling back to a map-size seed");
            }

            var script = SpawnQuest.Build(width, height, seed, settings);
            var (humans, firstBot) = Sides.FromStartInfo();
            _factory ??= Targets.QuestManagerFactory();
            var manager = (Il2CppObjectBase)_factory.Invoke(null, new object[]
            {
                script, new Il2CppStructArray<int>(humans), new Il2CppStructArray<int>(firstBot), null,
            });

            if (manager == null)
            {
                Plugin.L.LogWarning("Quest manager factory returned null; weekly spawns disabled for this game");
                return;
            }

            IL2CPP.il2cpp_gc_wbarrier_set_field(_session, (IntPtr)slot, manager.Pointer);
            Plugin.L.LogInfo($"Injected invasion quest manager into script-less map {width}x{height}, seed {seed}, neutral difficulty {difficulty} (stack value {stackValue}), humans [{string.Join(",", humans)}], bot [{string.Join(",", firstBot)}], " +
                                $"invasion weeks [{string.Join(",", InvasionSchedule.AbsoluteWeeks.Select(w => w + 1))}]");
        }
        catch (Exception e)
        {
            Plugin.L.LogError($"Quest manager injection failed: {e}");
        }
    }
}
