using System.Linq;
using System.Reflection;
using Hex.QuestScript;
using StartInfo = Hex.Session.StartInfo;

namespace SpicyWeeks;

/// <summary>
/// The lobby's RandomSeed: shared by every client in a game and different between games.
/// Read from the session StartInfo, which the game keeps in a static field (found by type, name is obfuscated).
/// </summary>
internal static class GameSeed
{
    private static PropertyInfo _startInfo;

    public static StartInfo StartInfo()
    {
        _startInfo ??= typeof(Quests).Assembly.GetTypes()
            .SelectMany(t => t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .FirstOrDefault(p => p.PropertyType == typeof(StartInfo));
        return _startInfo?.GetValue(null) as StartInfo;
    }

    public static bool TryGet(out int seed)
    {
        seed = 0;
        if (StartInfo() is not { settings: not null } info) return false;
        seed = info.settings.RandomSeed;
        return true;
    }
}
