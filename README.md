# Spicy Weeks

A mod for **Heroes of Might and Magic: Olden Era** that brings back the Heroes 3 feeling of
*"Month of the Peasant"*, and lets you get rid of week effects you don't enjoy.

- **Monster invasions.** Now and then the astrologer announces something like
  *"Week of Pestilence: lich dragons invade the land!"*, and stacks of that creature appear all over the map.
  Early invasions are peasants and pixies; late ones are coatls, sentinels and lich dragons.
- **Banned week effects.** Month of Clarity, the one that reveals the whole map, never happens.
  You can ban others too.
- **Works in multiplayer.** Every player sees the same invasions.

Invasions happen on random maps (any template). Banned effects apply on every map.

## Install

1. Download `SpicyWeeks-x.y.z.zip` from the [latest release](../../releases/latest).
2. In Steam, right-click **Olden Era** → **Manage** → **Browse local files**.
3. Extract the zip **into that folder**. `winhttp.dll` and the `BepInEx` folder must end up
   right next to `HeroesOldenEra.exe`, not inside a subfolder.
4. Start the game as usual. The **first** start takes a few extra minutes on the splash screen;
   after that it's normal.

Everything needed is in the zip, BepInEx (the mod loader) included.

**Is it working?** After starting the game, `BepInEx/LogOutput.log` should exist in the game folder
and contain `Loading [Spicy Weeks ...]`.

## Playing with friends

Everyone needs **the same version** and **the same settings file**. If one player changes a setting,
the games will drift apart and desync. Simplest: everyone installs the same zip and nobody edits the settings.

## Settings

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

## Uninstall

Delete `winhttp.dll` from the game folder. To remove everything, also delete the `BepInEx` and `dotnet` folders.

## After a game update

A game patch can break the mod. If it does, the game still runs normally, just without invasions,
until a new version of the mod is released.

## Playing on a Mac (Wine, CrossOver, Whisky)

The mod works under Wine with two extra settings in the Wine bottle. See *MAC* in
[`packaging/INSTALL.txt`](packaging/INSTALL.txt).

---

## For developers

The mod is a [BepInEx 6](https://github.com/BepInEx/BepInEx) (IL2CPP) plugin in `src/SpicyWeeks`.

- **Build:** `dotnet build src/SpicyWeeks -c Release`. It compiles against the reference assemblies
  in `lib/`, and copies the plugin into your local game if one is found.
- **After a game update:** run `scripts/update-refs.sh` to regenerate `lib/`, then rebuild.
- **Release:** push a tag like `v0.9.0`. GitHub Actions builds the install zip and publishes the release.
- **Local zip:** `scripts/package.sh 0.9.0` writes `dist/SpicyWeeks-0.9.0.zip`.
