using System;

namespace SpicyWeeks;

/// <summary>
/// Reads the current map size by following the same pointer chain the game's SpawnSquad action uses:
/// session singleton (dbx.me) +104 → world, +80 → Map, +24 → map data, +64/+68 → width/height.
/// Offsets come from the IL2CPP disassembly and may change with game patches; results are sanity-checked.
/// </summary>
internal static class MapInfo
{
    private const int SessionToWorld = 104;
    private const int WorldToMap = 80;
    private const int MapToData = 24;
    private const int DataWidth = 64;
    private const int DataHeight = 68;

    public static unsafe bool TryGetSize(out int width, out int height)
    {
        width = height = 0;
        var session = dbx.me;
        if (session == null) return false;

        var p = session.Pointer;
        foreach (var offset in new[] { SessionToWorld, WorldToMap, MapToData })
        {
            p = *(IntPtr*)(p + offset);
            if (p == IntPtr.Zero) return false;
        }

        width = *(int*)(p + DataWidth);
        height = *(int*)(p + DataHeight);
        return width is >= 8 and <= 1024 && height is >= 8 and <= 1024;
    }
}
