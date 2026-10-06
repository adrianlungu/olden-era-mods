#!/usr/bin/env bash
# Builds the distributable zip: BepInEx (IL2CPP) + Unity base libraries + the SpicyWeeks plugin,
# its shipped settings and install notes, laid out to be extracted into the game folder.
# Usage: scripts/package.sh <version> [plugin.dll]   → dist/SpicyWeeks-<version>.zip
set -euo pipefail

VERSION="${1:?usage: package.sh <version> [plugin.dll]}"
REPO_DIR="$(cd "$(dirname "$0")/.." && pwd)"
PLUGIN_DLL="${2:-$REPO_DIR/src/SpicyWeeks/bin/Release/SpicyWeeks.dll}"

BEPINEX_BUILD="${BEPINEX_BUILD:-788}"
BEPINEX_URL="${BEPINEX_URL:-https://builds.bepinex.dev/projects/bepinex_be/$BEPINEX_BUILD/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.$BEPINEX_BUILD%2B5b766a3.zip}"
UNITY_VERSION="${UNITY_VERSION:-6000.0.66}"
UNITY_LIBS_URL="https://unity.bepinex.dev/libraries/$UNITY_VERSION.zip"

[ -f "$PLUGIN_DLL" ] || { echo "Plugin not built: $PLUGIN_DLL" >&2; exit 1; }

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
STAGE="$WORK/stage"
mkdir -p "$STAGE"

curl -sSfL -o "$WORK/bepinex.zip" "$BEPINEX_URL"
unzip -q "$WORK/bepinex.zip" -d "$STAGE"

mkdir -p "$STAGE/BepInEx/unity-libs" "$STAGE/BepInEx/plugins/SpicyWeeks" "$STAGE/BepInEx/config"
curl -sSfL -o "$STAGE/BepInEx/unity-libs/$UNITY_VERSION.zip" "$UNITY_LIBS_URL"
cp "$PLUGIN_DLL" "$STAGE/BepInEx/plugins/SpicyWeeks/"
cp "$REPO_DIR/packaging/olden-era-mods.spicy-weeks.cfg" "$STAGE/BepInEx/config/"
cp "$REPO_DIR/packaging/INSTALL.txt" "$STAGE/SpicyWeeks-INSTALL.txt"

mkdir -p "$REPO_DIR/dist"
OUT="$REPO_DIR/dist/SpicyWeeks-$VERSION.zip"
rm -f "$OUT"
(cd "$STAGE" && zip -qr "$OUT" .)
echo "$OUT"
