namespace SpicyWeeks;

/// <summary>
/// The lobby's neutral difficulty (GameStartInfo.NeutralsDifficulty), as the power multiplier the game applies
/// to its own neutral stacks: DB/difficulties_lobby.json "squad" (same in the solo file), indexed by the setting.
/// </summary>
internal static class Difficulty
{
    private static readonly (string Name, double Power)[] Squad =
    {
        ("easy", 0.50), ("normal", 0.75), ("hard", 1.00), ("impossible", 1.25), ("deadly", 1.50), ("hell", 2.00),
    };

    /// <summary>Multiplier for invasion stack strength; 1 (hard) when the setting can't be read.</summary>
    public static double NeutralPower(out string name)
    {
        var index = GameSeed.StartInfo()?.settings?.NeutralsDifficulty ?? -1;
        if (index < 0 || index >= Squad.Length)
        {
            name = $"unknown ({index})";
            return 1.0;
        }

        name = Squad[index].Name;
        return Squad[index].Power;
    }
}
