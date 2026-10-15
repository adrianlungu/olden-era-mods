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
printf '## What'\''s new\n%s\n\n## Installing\n\n```\n%s\n```\n' "$changes" "$(cat "$root/packaging/INSTALL.txt")"
