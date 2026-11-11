#!/usr/bin/env bash
# Release notes for a version: its CHANGELOG.md section, then the install guide.
# Usage: scripts/release-notes.sh 0.9.0 > notes.md
set -euo pipefail
version="$1"
root="$(cd "$(dirname "$0")/.." && pwd)"
changes="$(awk -v v="$version" '
  /^## / { if (found) exit; if ($2 == v) { found = 1; next } }
  found' "$root/src/SpicyWeeks/CHANGELOG.md" |
  # Join wrapped bullet lines: GitHub release notes keep single line breaks.
  awk 'NR > 1 && /^  [^ ]/ { sub(/^ +/, " "); printf "%s", $0; next } { if (NR > 1) print ""; printf "%s", $0 } END { print "" }')"
if [ -z "${changes//[[:space:]]/}" ]; then
  echo "No CHANGELOG.md section for $version" >&2
  exit 1
fi
# The game build lib/ was generated from (scripts/update-refs.sh): the only game version this release is known to work with.
game="$(sed -n 's/^Steam build: //p' "$root/lib/README.txt")"
[ -n "$game" ] || { echo "lib/README.txt has no Steam build line; rerun scripts/update-refs.sh" >&2; exit 1; }
printf '**Built for:** Heroes of Might and Magic: Olden Era, Steam build %s. Other game versions may not work.\n\n' "$game"
printf '## What'\''s new\n%s\n\n## Installing\n\n```\n%s\n```\n' "$changes" "$(cat "$root/packaging/INSTALL.txt")"
