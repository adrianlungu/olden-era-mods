# Spicy Weeks changelog

## 0.10.0

For the game's November 10 update (regular branch). Includes everything from the two betas below.

- **Special weeks.** Some invasions are now *Week of the Swarm* (countless small stacks), *Week of Champions* (a few
  huge stacks), *Week of Mercenaries* (stacks that join the first hero to reach them, for free) or *Week of the Rift*
  (hostile top-tier creatures). New setting: `SpecialChancePercent` (`0` turns them off).
- **Spicy Week.** Now and then, from the second month on, an invasion week turns spicy: something new happens
  every day. A swarm of small stacks, a few huge champions, free mercenaries that join whoever reaches them first,
  a rift of top-tier creatures, or a mood swing that turns the week's earlier stacks hostile (or docile), each day
  harder than the last. New setting: `SpicyChancePercent` (`0` turns it off).
- **Works with the latest game update.** 0.9.0 and the betas don't work on it: the mod loaded but did nothing.
- **Month of Plague is banned by default**, like Month of Clarity: it killed every creature waiting to be bought in
  towns and dwellings. Already have a settings file? Add `astrologist_month_1` to `BannedEffects` yourself, or
  delete the file to get the new defaults.
- **Sturdier after game updates.** If a patch breaks one of the mod's hooks, only that feature switches off (with a
  warning in the BepInEx log) instead of the whole mod failing to load.

Multiplayer: everyone must update to 0.10.0 and use the same settings file.

## 0.10.1-beta

- **Month of Plague is banned by default.** It killed every creature waiting to be bought in towns and dwellings;
  now that month is Calm instead. Already have a settings file? Add `astrologist_month_1` to `BannedEffects`
  yourself (or delete the file to get the new defaults).

Multiplayer: everyone must update and use the same settings file.

## 0.10.0-beta

For the game's **public test branch** (beta). On the regular game, keep using 0.9.0.

- Updated for the beta's changes to the game's internals. Gameplay and settings are the same as in 0.9.0.
- If a future game patch breaks one of the mod's hooks, only that feature switches off (with a warning in the
  BepInEx log) instead of the whole mod failing to load.

Multiplayer: everyone must be on the beta branch with 0.10.0-beta and the same settings file.

## 0.9.0

- **Week of the Horde.** Invasions now bring creatures from every faction, not just neutrals. Most invasions are a
  horde with a different creature in every stack, so you can't recruit a whole army of one kind by diplomacy. About
  a quarter are still a Heroes 3-style week of a single creature, like *Week of Pestilence*.
  New settings: `Factions` (which factions invade) and `MixedChancePercent` (how often a week is a horde;
  `0` for single-creature weeks only).
- **Invaders have moods.** Each invasion rolls a mood that changes how willing its stacks are to join you, from
  *Week of the Raging …* (never joins) to *Week of the Travelling …* (joins most easily). The announcement tells you
  which one it is. New setting: `Moods`.
- **Stacks spawn where you can reach them.** Stacks no longer appear in pockets walled in by mountains or cliffs.
- **Invasions follow the neutral difficulty** you pick in the lobby, like the map's own monsters: half as strong on
  *easy*, twice as strong on *hell*. New setting: `MatchNeutralDifficulty`.

Multiplayer: everyone must update to 0.9.0 and use the same settings file.

## 0.8.0

- First release: Heroes 3-style neutral invasion weeks on random maps and hand-made maps without their own script,
  announced by the astrologer.
- `BannedEffects`: week and month effects that never happen; Month of Clarity is banned by default.
