# Olden Era mods

Mods for Heroes of Might and Magic: Olden Era.

- `src/SpicyWeeks`: BepInEx 6 (IL2CPP) plugin adding neutral invasion weeks to random maps.
- `lib/`: reference-only assemblies to compile against (regenerate with `scripts/update-refs.sh` after game updates).
- `scripts/package.sh <version>`: builds the install zip into `dist/`.
- Releases: push a `v*` tag; the Release workflow builds and publishes the zip.

Player instructions: `packaging/INSTALL.txt`.
