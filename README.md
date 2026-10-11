# Olden Era Mods

Mods for **Heroes of Might and Magic: Olden Era**, built on the [BepInEx](https://github.com/BepInEx/BepInEx) mod loader.

| Mod | What it does |
|---|---|
| [Spicy Weeks](#spicy-weeks) | Heroes 3-style monster invasion weeks, and bans week effects you don't enjoy. |

## Installing a mod

1. Download the mod's zip from the [releases page](../../releases).
2. In Steam, right-click **Olden Era** → **Manage** → **Browse local files**.
3. Extract the zip **into that folder**. `winhttp.dll` and the `BepInEx` folder must end up
   right next to `HeroesOldenEra.exe`, not inside a subfolder.
4. Start the game as usual. The **first** start takes a few extra minutes on the splash screen;
   after that it's normal.

Everything needed is in the zip, BepInEx included.

**Is it working?** After starting the game, `BepInEx/LogOutput.log` should exist in the game folder
and list the mod, e.g. `Loading [Spicy Weeks ...]`.

**Playing with friends:** everyone needs **the same mod versions** and **the same settings files**.
If one player changes a setting, the games will drift apart and desync.
Simplest: everyone installs the same zips and nobody edits the settings.

**Uninstall:** delete `winhttp.dll` from the game folder to turn all mods off. To remove everything,
also delete the `BepInEx` and `dotnet` folders.

**After a game update:** a game patch can break a mod. If it does, the game still runs normally,
just without that mod's features, until a new version of the mod is released.

**Mac (Wine, CrossOver, Whisky):** the mods work under Wine with two extra settings in the Wine bottle.
See *MAC* in [`packaging/INSTALL.txt`](packaging/INSTALL.txt).

---

## Spicy Weeks

Brings back the Heroes 3 feeling of *"Month of the Peasant"*, and lets you get rid of week effects you don't enjoy.

- **Monster invasions.** Now and then the astrologer announces something like
  *"Week of Pestilence: lich dragons invade the land!"*, and stacks of that creature appear all over the map.
  Early invasions are peasants and pixies; late ones are coatls, sentinels and lich dragons.
- **Banned week effects.** Month of Clarity, the one that reveals the whole map, never happens.
  You can ban others too.
- **Works in multiplayer.** Every player sees the same invasions.

Invasions happen on random maps (any template). Banned effects apply on every map.

### Settings

Open `BepInEx/config/olden-era-mods.spicy-weeks.cfg` in any text editor.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Turns invasions on or off. |
| `ChancePercent` | `25` | Chance that a week is an invasion week. |
| `FirstWeek` | `3` | Earliest week an invasion can happen (week 3 starts on day 15). |
| `MinWeeksBetween` | `2` | Quiet weeks between two invasions. |
| `TilesPerStack` | `400` | Invasion size: one stack per this many map tiles. Lower means more monsters. |
| `StackValue` | `4000` | How strong each stack is at the start of the game. |
| `WeeklyGrowthPercent` | `12` | How much stronger stacks get each week. |
| `OverrideWeekEffect` | `true` | Invasion weeks replace the normal week effect and announcement. |
| `BannedEffects` | `astrologist_month_5` | Week/month effects that never happen, comma-separated. |

Some effects you might want to ban:

| Effect | Name |
|---|---|
| `astrologist_month_5` | Month of Clarity: reveals the whole map |
| `astrologist_week_5` | Week of Dusk: covers the map in fog |
| `astrologist_week_6` | Wednesday Week: the week lasts 3 days |
| `astrologist_month_1` | Month of Plague: kills unowned creatures in towns and dwellings |
| `astrologist_month_8` | Month of Destruction: no building |

To remove only Spicy Weeks, delete the `BepInEx/plugins/SpicyWeeks` folder.

---

## For developers

Each mod is a BepInEx 6 (IL2CPP) plugin under `src/` (Spicy Weeks: `src/SpicyWeeks`).

- **Build:** `dotnet build src/SpicyWeeks -c Release`. It compiles against the reference assemblies
  in `lib/`, and copies the plugin into your local game if one is found.
- **After a game update:** run `scripts/update-refs.sh` to regenerate `lib/`, then rebuild.
- **Release:** push a tag like `v0.9.0`. GitHub Actions builds the install zip and publishes the release.
- **Local zip:** `scripts/package.sh 0.9.0` writes `dist/SpicyWeeks-0.9.0.zip`.
