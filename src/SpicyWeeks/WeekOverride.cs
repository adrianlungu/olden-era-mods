using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hex;
using Hex.QuestScript;
using SessionData = Hex.Session.Data.Data;
using LocTable = Il2CppSystem.Collections.Generic.Dictionary<string, cae>;

namespace SpicyWeeks;

/// <summary>
/// On invasion weeks, replaces the astrologer's roll with Week (or Month) of Calm — which has no effect —
/// and rewrites Calm's name and description so the game's own announcement presents the invasion.
/// The picker runs inside the lockstep game logic and every client holds the same schedule, so all
/// clients make the same replacement. Calm's original text is restored on every other week.
/// </summary>
[HarmonyPatch]
internal static class WeekOverride
{
    private static readonly (string Sid, string Name, string Desc)[] Calm =
    {
        ("astrologist_week_10", "astrologist_week_10_name", "astrologist_week_10_desc"),
        ("astrologist_month_10", "astrologist_month_10_name", "astrologist_month_10_desc"),
    };

    private static LocTable _loc;
    private static readonly Dictionary<string, string> Originals = new();
    private static PropertyInfo _sessionData;

    // The picker: ecr X(SerializableRandom) on the week-picker class (two identical obfuscator copies).
    static IEnumerable<MethodBase> TargetMethods() =>
        typeof(ect).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(ecr)
                        && m.GetParameters() is { Length: 1 } p
                        && p[0].ParameterType == typeof(SerializableRandom));

    static void Postfix(ect __instance, ref ecr __result)
    {
        try
        {
            RestoreText();

            var data = SessionDataNow();
            var invasion = Plugin.OverrideWeekEffect.Value && data != null
                ? InvasionSchedule.For(InvasionSchedule.AbsoluteWeek(data.month, data.week))
                : null;

            if (invasion != null)
            {
                if (!ReplaceWithCalm(__instance, ref __result, out var calm)) return;
                SetText(calm.Name, invasion.WeekName);
                SetText(calm.Desc, invasion.Announcement);
                Plugin.L.LogInfo($"Week {invasion.AbsoluteWeek + 1}: replaced astrologer roll with {calm.Sid} for {invasion.WeekName}");
                return;
            }

            // Banned effects apply on every map, scripted or random.
            var rolled = __result?.chyb?.sid;
            if (rolled == null || !Plugin.BannedEffectSids().Contains(rolled)) return;
            if (ReplaceWithCalm(__instance, ref __result, out var quiet))
                Plugin.L.LogInfo($"Banned effect {rolled} rolled; replaced with {quiet.Sid}");
        }
        catch (Exception e)
        {
            Plugin.L.LogError($"Week override failed: {e}");
        }
    }

    // Week of Calm from the week picker, Month of Calm from the month picker.
    private static bool ReplaceWithCalm(ect picker, ref ecr result, out (string Sid, string Name, string Desc) calm)
    {
        foreach (var candidate in Calm)
        {
            var replacement = picker.bbsn(candidate.Sid);
            if (replacement == null) continue;
            result = replacement;
            calm = candidate;
            return true;
        }

        calm = default;
        Plugin.L.LogWarning("No Calm effect available to replace the astrologer roll");
        return false;
    }

    public static void RestoreText()
    {
        if (_loc == null) return;
        foreach (var (sid, text) in Originals)
            if (_loc.TryGetValue(sid, out var entry)) entry.cafz = text;
        Originals.Clear();
    }

    // Localized text lives in the text provider's sid → entry table (it.btdt.cafu); the shown string is
    // the entry's second field (the one the provider's lookup returns).
    private static void SetText(string sid, string text)
    {
        _loc ??= it.btdt?.cafu;
        if (_loc == null || !_loc.TryGetValue(sid, out var entry))
        {
            Plugin.L.LogWarning($"Localization entry {sid} not found; invasion week keeps Calm's text");
            return;
        }

        if (!Originals.ContainsKey(sid)) Originals[sid] = entry.cafz;
        entry.cafz = text;
    }

    private static SessionData SessionDataNow()
    {
        var session = dbx.me;
        if (session == null) return null;
        _sessionData ??= typeof(dbx).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.PropertyType == typeof(SessionData));
        return _sessionData?.GetValue(session) as SessionData;
    }
}
