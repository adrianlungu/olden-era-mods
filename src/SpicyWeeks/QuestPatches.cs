using System.Reflection;
using HarmonyLib;
using Hex.QuestScript;

namespace SpicyWeeks;

[HarmonyPatch]
internal static class QuestManagerInitPatch
{
    static MethodBase TargetMethod()
    {
        var m = Targets.QuestManagerInit();
        Plugin.L.LogInfo($"Quest manager init found: {m.DeclaringType?.FullName}.{m.Name}");
        return m;
    }

    static void Prefix(Quests __0)
    {
        var quests = __0?.quests?.Length ?? -1;
        var counters = __0?.counters?.Length ?? -1;
        Plugin.L.LogInfo($"Quest system init: script {(__0 == null ? "null" : "present")}, quests={quests}, counters={counters}");
    }
}
