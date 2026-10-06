using System.Linq;
using System.Reflection;
using Hex.QuestScript;
using QuestData = Hex.Session.Quests2.Data.Data;

namespace SpicyWeeks;

/// <summary>
/// Game class and method names are obfuscated and change between patches, so targets are found by shape.
/// Only patch methods with substantial unique bodies: IL2CPP folds identical trivial methods together,
/// and hooking a folded method hooks every unrelated method sharing its code (see .docs).
/// </summary>
internal static class Targets
{
    private static Assembly HexAssembly => typeof(Quests).Assembly;

    private static MethodInfo[] PublicInstanceMethods() =>
        HexAssembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .ToArray();

    /// <summary>Quest manager setup: bool Init(Quests, Data, campaignCounters).</summary>
    public static MethodBase QuestManagerInit() =>
        PublicInstanceMethods().Single(m => m.Name == "Init"
                                            && m.ReturnType == typeof(bool)
                                            && m.GetParameters() is { Length: 3 } p
                                            && p[0].ParameterType == typeof(Quests)
                                            && p[1].ParameterType == typeof(QuestData));

    /// <summary>
    /// Quest manager factory: static Manager X(Quests, int[] sides, int[] sides, campaignCounters).
    /// The obfuscator emits several identical copies (new Data → Data fill → new Manager → Init); any works.
    /// Random maps never call it: their session start path skips the quest system entirely.
    /// </summary>
    public static MethodInfo QuestManagerFactory()
    {
        var manager = QuestManagerInit().DeclaringType!;
        return manager.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .First(m => m.ReturnType == manager
                        && m.GetParameters() is { Length: 4 } p
                        && p[0].ParameterType == typeof(Quests));
    }
}
