using System.Collections.Generic;
using StartSide = Hex.Session.StartSide;

namespace SpicyWeeks;

/// <summary>
/// Side index lists the quest data needs, built the way the game does for scripted maps:
/// every human side, plus the first bot side. Clone-shared counters and quests get one copy per listed side.
/// </summary>
internal static class Sides
{
    public static (int[] Humans, int[] FirstBot) FromStartInfo()
    {
        var humans = new List<int>();
        var bots = new List<int>();
        var sides = GameSeed.StartInfo()?.sides;
        if (sides == null) return (humans.ToArray(), bots.ToArray());

        for (var i = 0; i < sides.Length; i++)
        {
            switch (sides[i]?.eType)
            {
                case StartSide.EType.Player:
                    humans.Add(i);
                    break;
                case StartSide.EType.Bot or StartSide.EType.BotAkaPlayer when bots.Count == 0:
                    bots.Add(i);
                    break;
            }
        }

        return (humans.ToArray(), bots.ToArray());
    }
}
