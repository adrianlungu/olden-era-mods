# Agent guide

Olden Era Mods: BepInEx 6 (IL2CPP) plugins for Heroes of Might and Magic: Olden Era (Unity 6000.0.66, IL2CPP,
GUPS-obfuscated), one per folder under `src/`. Current mods: Spicy Weeks (`src/SpicyWeeks`: monster invasion weeks
on random maps, banned astrologer effects). Shared tooling (`lib/`, `scripts/`, `packaging/`, CI) serves all mods.
When adding a mod, add it to the README mod table and give it its own section there.
Player docs: `README.md`. Detailed reverse-engineering notes: `.docs/plan-1.md`, environment and release
notes: `.docs/status-1.md` (git-crypt encrypted; readable once the repo is unlocked).

## Layout

- `src/SpicyWeeks/`: the Spicy Weeks plugin.
  - `QuestInjector.cs`: MonoBehaviour that gives script-less sessions a quest manager running our script.
  - `SpawnQuest.cs`: rolls the invasion schedule and builds the quest script (themes, values, tiles).
  - `WeekOverride.cs`: postfix on the astrologer week picker: invasion weeks and `BannedEffects` → Calm, text swap.
  - `Targets.cs`, `GameSeed.cs`, `Sides.cs`, `MapInfo.cs`: locating game objects without obfuscated names.
- `lib/`: reference-only assemblies to compile against. Regenerate with `scripts/update-refs.sh` after a game or BepInEx update.
- `packaging/`: shipped settings file and `INSTALL.txt` (also used as release notes).
- `scripts/package.sh <version>`: builds the install zip. `.github/workflows/release.yml`: same, on `v*` tags.

## Build and test loop

- `dotnet build src/SpicyWeeks -c Release` builds and copies the DLL into the local game's
  `BepInEx/plugins/SpicyWeeks/` (path in the csproj `GameDir`; override with `-p:GameDir=...`).
- The game must be **fully restarted** to load a new build; returning to the main menu is not enough.
- Results come from logs, not the screen: `BepInEx/LogOutput.log` (plugin) and the game's `Player.log`
  (`AppData/LocalLow/Unfrozen/HeroesOldenEra/`). Quest `Print` actions show up in `Player.log` as `[Quests] [Action] ...`.
- Use a `[Debug]` config option (e.g. `RevealSpawns`) or temporary `Print` triggers to make behaviour observable.
  Revert test values in the user's config afterwards.

## Rules for touching the game

- **Find targets by shape, not by obfuscated name.** Names like `dly`, `dir`, `ect`, `ecr` change with game
  patches. Match on signatures and readable types (`Hex.QuestScript.Quests`, `Hex.Session.Data.Data`, `SerializableRandom`).
  Where code still uses an obfuscated name or raw offset (`dbx.me`, `MapInfo` offsets, `it.btdt.cafu[sid].cafz`),
  keep it in one place and sanity-check it.
- **Never Harmony-patch small methods.** IL2CPP folds identical method bodies; patching a trivial method
  (e.g. any `OnStartWeek`) hooks every unrelated method sharing its code and crashes the game at startup.
  Prefer large, unique methods, or a MonoBehaviour that polls state.
- **Multiplayer runs in lockstep.** Anything that changes game state must be identical on every client:
  derive randomness from the lobby seed (`GameSeed`) and act only inside game logic (quest triggers, the week picker),
  never on frame timing. Settings that affect gameplay must match between players.
- **Fail soft.** A broken hook after a game patch must log a warning and leave the game playable.

## Verified quest-script behaviour (injected scripts)

- `Counter` conditions never match on our injected script. Don't use counters; schedule with
  `AnyStartTurn [month, week-of-month, day]` (verified: `1,2,3` fires on day 10). Non-repeating triggers work.
- `SpawnSquad [squadSid, tile, value, "true", name]`: tile = `x + z * mapWidth`; the game moves the stack to a free
  neighbour. Neutral squads `squad_neutral_one_unit_t1..t8` scale to `value` within their min/max range.
- The quest manager factory needs real side lists: human sides, plus the first bot side (`Sides.cs`).
- `StartWeek` fires on day 8, 15, ... (not day 1). The astrologer's roll happens before quest triggers on the same day.

## Reverse-engineering tools

- `ilspycmd -p -o <out> -r BepInEx/interop -r BepInEx/core -r dotnet BepInEx/interop/Hex.dll`: wrapper signatures.
- Cpp2IL (2022.1 pre-release, macOS needs `codesign --force -s - Cpp2IL`) with `--output-as isil`: method bodies with
  resolved call names. Use it to read what a method actually does before hooking it.

## Releases

1. Bump the version in **both** `src/SpicyWeeks/SpicyWeeks.csproj` and `Plugin.Version`, and in the header of
   `packaging/olden-era-mods.spicy-weeks.cfg` if settings changed (add new settings there too; it ships to players).
2. Commit, then `git tag vX.Y.Z && git push origin vX.Y.Z`. The workflow builds and publishes the zip.
3. The pipeline currently assumes a single mod (`v*` tags build only Spicy Weeks). **When a second mod is added**,
   switch to per-mod tags (e.g. `spicy-weeks-v0.9.0`, `other-mod-v0.1.0`): the workflow derives the mod and version
   from the tag, builds and packages only that mod, and titles the release after it; `package.sh` takes the mod name.

## Docs and repo hygiene

- The repo is public. Internal notes, plans and status go in `.docs/` (git-crypt encrypted) with non-descriptive
  file names (`plan-N.md`, `status-N.md`); keep `.docs/plan-1.md` current with new findings.
- `lib/` contains game-derived reference assemblies and is public by the owner's decision; if the publisher objects,
  encrypt it with git-crypt and have CI unlock it from a secret.
