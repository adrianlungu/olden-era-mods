using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;

namespace SpicyWeeks;

/// <summary>
/// Which tiles an invasion stack may spawn on: passable land that belongs to a large connected region, so
/// stacks never land in pockets walled in by mountains or cliffs. The game's SpawnSquad only avoids water and
/// occupied tiles, not unreachable ones.
///
/// Read from the map the game itself uses (session +104 → world, +80 → Map):
///   Map +24 → MapData: levelsMap (+80, sbyte), climbsMap (+88), waterMap (+104), one entry per tile;
///   Map +104 → occupancy table: +24 byte per tile, non-zero = blocked by an object (mountains, trees,
///   buildings, ...); the game's SpawnSquad neighbour search rejects these tiles (byk.pqm).
/// Captured once on the first frame the map exists, before anyone can move, so every multiplayer client
/// sees the same terrain. Offsets come from the IL2CPP disassembly; array sizes are sanity-checked and any
/// mismatch falls back to unfiltered random tiles.
/// </summary>
internal static class Terrain
{
    private const int SessionToWorld = 104;
    private const int WorldToMap = 80;
    private const int MapToData = 24;
    private const int MapToOccupancy = 104;
    private const int DataLevels = 80;
    private const int DataClimbs = 88;
    private const int DataWater = 104;
    private const int OccupancyBlocked = 24;
    private const int OccupancyOther = 32;

    // Il2Cpp array layout: max_length at +24, elements from +32.
    private const int ArrayLength = 24;
    private const int ArrayData = 32;

    private const int Margin = 2;

    private static byte[] _water, _blocked, _other, _climbs;
    private static sbyte[] _levels;
    private static int _width, _height;
    private static bool[] _spawnable;
    private static bool _levelsIgnored;

    /// <summary>Spawnable tile indices (x + z * width), or null if the terrain could not be read.</summary>
    public static int[] SpawnTiles { get; private set; }

    public static void Clear()
    {
        _water = _blocked = _other = _climbs = null;
        _levels = null;
        _spawnable = null;
        SpawnTiles = null;
    }

    /// <summary>Snapshot the terrain and compute spawnable tiles. Call once, as soon as the map exists.</summary>
    public static unsafe void Capture(int width, int height)
    {
        Clear();
        try
        {
            var session = dbx.me;
            if (session == null) return;

            var world = *(IntPtr*)(session.Pointer + SessionToWorld);
            var map = world == IntPtr.Zero ? IntPtr.Zero : *(IntPtr*)(world + WorldToMap);
            if (map == IntPtr.Zero) return;
            var data = *(IntPtr*)(map + MapToData);
            var occupancy = *(IntPtr*)(map + MapToOccupancy);
            if (data == IntPtr.Zero || occupancy == IntPtr.Zero) return;

            var area = width * height;
            _water = ReadBytes(*(IntPtr*)(data + DataWater), area);
            _levels = ReadBytes(*(IntPtr*)(data + DataLevels), area)?.Select(b => (sbyte)b).ToArray();
            _climbs = ReadBytes(*(IntPtr*)(data + DataClimbs), area);
            _blocked = ReadBytes(*(IntPtr*)(occupancy + OccupancyBlocked), area);
            _other = ReadBytes(*(IntPtr*)(occupancy + OccupancyOther), area);
            if (_water == null || _blocked == null)
            {
                Plugin.L.LogWarning("Map terrain layout not recognised; invasion stacks use unfiltered random tiles");
                return;
            }

            (_width, _height) = (width, height);
            _spawnable = Regions(useLevels: _levels != null && _climbs != null);
            SpawnTiles = Enumerable.Range(0, area).Where(t => _spawnable[t] && !NearEdge(t)).ToArray();

            var passable = Enumerable.Range(0, area).Count(Passable);
            Plugin.L.LogInfo($"Terrain: {passable} passable tiles, {SpawnTiles.Length} spawnable" +
                             $"{(_levelsIgnored ? " (height levels ignored)" : "")}, checksum {Checksum(SpawnTiles):x8}");
        }
        catch (Exception e)
        {
            Plugin.L.LogWarning($"Reading map terrain failed ({e.Message}); invasion stacks use unfiltered random tiles");
            Clear();
        }
    }

    private static unsafe byte[] ReadBytes(IntPtr array, int expected)
    {
        if (array == IntPtr.Zero) return null;
        var length = *(long*)(array + ArrayLength);
        if (length != expected) return null;
        var result = new byte[expected];
        new ReadOnlySpan<byte>((void*)(array + ArrayData), expected).CopyTo(result);
        return result;
    }

    private static bool Passable(int t) => _water[t] == 0 && _blocked[t] == 0;

    private static bool NearEdge(int t)
    {
        int x = t % _width, z = t / _width;
        return x < Margin || z < Margin || x >= _width - Margin || z >= _height - Margin;
    }

    // Moving between two height levels needs a climb (ramp) on one of the tiles.
    private static bool Connected(int a, int b, bool useLevels) =>
        !useLevels || _levels[a] == _levels[b] || _climbs[a] != 0 || _climbs[b] != 0;

    /// <summary>
    /// Flood-fills passable tiles into 8-connected regions and keeps the large ones (the main land and real
    /// islands), dropping small enclosed pockets. If height levels split the map implausibly (the largest region
    /// holds under a third of the passable land), the level rule is assumed wrong and regions ignore levels.
    /// </summary>
    private static bool[] Regions(bool useLevels)
    {
        var area = _width * _height;
        var region = new int[area];
        Array.Fill(region, -1);
        var sizes = new List<int>();
        var queue = new Queue<int>();

        for (var start = 0; start < area; start++)
        {
            if (region[start] >= 0 || !Passable(start)) continue;
            var id = sizes.Count;
            var size = 0;
            region[start] = id;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                size++;
                int x = t % _width, z = t / _width;
                for (var dz = -1; dz <= 1; dz++)
                for (var dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, nz = z + dz;
                    if ((dx == 0 && dz == 0) || nx < 0 || nz < 0 || nx >= _width || nz >= _height) continue;
                    var n = nx + nz * _width;
                    if (region[n] >= 0 || !Passable(n) || !Connected(t, n, useLevels)) continue;
                    region[n] = id;
                    queue.Enqueue(n);
                }
            }

            sizes.Add(size);
        }

        if (sizes.Count == 0) return new bool[area];

        var passable = sizes.Sum();
        if (useLevels && sizes.Max() * 3 < passable)
        {
            _levelsIgnored = true;
            return Regions(useLevels: false);
        }

        // Pockets are a few dozen tiles; real regions (main land, islands reachable by boat) are much larger.
        var minSize = Math.Max(64, area / 100);
        return region.Select(r => r >= 0 && sizes[r] >= minSize).ToArray();
    }

    private static uint Checksum(int[] tiles)
    {
        var hash = 2166136261u;
        foreach (var t in tiles) hash = (hash ^ (uint)t) * 16777619u;
        return hash;
    }

    /// <summary>
    /// Debug: writes BepInEx/spicy-weeks-map.txt, one character per tile, row z = 0 first.
    /// ~ water, # blocked by an object, . spawnable, , passable but not spawnable (pocket or edge),
    /// o passable with the occupancy table's second counter set, S chosen spawn tile; then the height levels.
    /// </summary>
    public static void Dump(IEnumerable<int> spawns)
    {
        if (_spawnable == null) return;
        try
        {
            var chosen = new HashSet<int>(spawns);
            var sb = new StringBuilder();
            sb.AppendLine($"# {_width}x{_height}  ~ water  # blocked  . spawnable  , unreachable/edge  o other  S spawn");
            for (var z = 0; z < _height; z++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var t = x + z * _width;
                    sb.Append(chosen.Contains(t) ? 'S'
                        : _water[t] != 0 ? '~'
                        : _blocked[t] != 0 ? '#'
                        : _other != null && _other[t] != 0 ? 'o'
                        : _spawnable[t] && !NearEdge(t) ? '.' : ',');
                }

                sb.AppendLine();
            }

            if (_levels != null)
            {
                sb.AppendLine("# height levels (0-9, - below 0), climbs shown as /");
                for (var z = 0; z < _height; z++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var t = x + z * _width;
                        sb.Append(_climbs != null && _climbs[t] != 0 ? '/' : _levels[t] < 0 ? '-' : (char)('0' + Math.Min(9, (int)_levels[t])));
                    }

                    sb.AppendLine();
                }
            }

            var path = Path.Combine(Paths.BepInExRootPath, "spicy-weeks-map.txt");
            File.WriteAllText(path, sb.ToString());
            Plugin.L.LogInfo($"Terrain map written to {path}");
        }
        catch (Exception e)
        {
            Plugin.L.LogWarning($"Writing terrain map failed: {e.Message}");
        }
    }
}
