#!/bin/bash
# Bumps the package version and prepends a release-notes entry in SoundFingerprinting.csproj.
#
# Usage:
#   ./bump.sh <major|minor|patch|X.Y.Z> "note" ["note" ...]   # bump, update notes, commit
#   ./bump.sh <major|minor|patch|X.Y.Z> --no-commit "note"    # leave changes uncommitted
#   ./bump.sh <major|minor|patch|X.Y.Z> --dry-run "note"      # print the outcome, change nothing
#   git log --format='%s' v15.8.0..HEAD | ./bump.sh minor -   # read notes from stdin, one per line
set -euo pipefail

CSPROJ="$(dirname "$0")/src/SoundFingerprinting/SoundFingerprinting.csproj"
PROPS="$(dirname "$0")/src/Directory.Build.props"
COMMIT=1
DRY_RUN=0

if [ $# -lt 1 ]; then
    sed -n '2,8p' "$0" | sed 's/^# \{0,1\}//'
    exit 1
fi

BUMP="$1"
shift

NOTES=()
for arg in "$@"; do
    case "$arg" in
        --no-commit) COMMIT=0 ;;
        --dry-run)   DRY_RUN=1; COMMIT=0 ;;
        -)           while IFS= read -r line; do [ -n "$line" ] && NOTES+=("$line"); done ;;
        *)           NOTES+=("$arg") ;;
    esac
done

if [ ${#NOTES[@]} -eq 0 ]; then
    echo "error: at least one release note is required" >&2
    exit 1
fi

CURRENT=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$PROPS")
if [ -z "$CURRENT" ]; then
    echo "error: could not read <Version> from $PROPS" >&2
    exit 1
fi

IFS='.' read -r MAJOR MINOR PATCH <<< "${CURRENT%%-*}"
case "$BUMP" in
    major) NEXT="$((MAJOR + 1)).0.0" ;;
    minor) NEXT="$MAJOR.$((MINOR + 1)).0" ;;
    patch) NEXT="$MAJOR.$MINOR.$((PATCH + 1))" ;;
    *)
        if ! printf '%s' "$BUMP" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+([-.][0-9A-Za-z.-]+)?$'; then
            echo "error: '$BUMP' is not major|minor|patch or a valid version" >&2
            exit 1
        fi
        NEXT="$BUMP"
        ;;
esac

if [ "$NEXT" = "$CURRENT" ]; then
    echo "error: version $NEXT equals the current version" >&2
    exit 1
fi

if git -C "$(dirname "$0")" rev-parse -q --verify "refs/tags/v$NEXT" >/dev/null 2>&1; then
    echo "error: tag v$NEXT already exists" >&2
    exit 1
fi

xml_escape() {
    printf '%s' "$1" | sed -e 's/&/\&amp;/g' -e 's/</\&lt;/g' -e 's/>/\&gt;/g'
}

echo "Bumping $CURRENT -> $NEXT"
echo "Release notes:"
for note in "${NOTES[@]}"; do
    echo "  - $note"
done

if [ $DRY_RUN -eq 1 ]; then
    exit 0
fi

# entry lines mirror the existing PackageReleaseNotes formatting: two tabs of indentation, newest entry first
ENTRY=$(printf '\t\tVersion %s' "$NEXT")
for note in "${NOTES[@]}"; do
    ENTRY="$ENTRY$(printf '\n\t\t- %s' "$(xml_escape "$note")")"
done

TMP=$(mktemp)
sed "s|<Version>[^<]*</Version>|<Version>$NEXT</Version>|" "$PROPS" > "$TMP"
mv "$TMP" "$PROPS"

TMP=$(mktemp)
ENTRY="$ENTRY" awk '
    { print }
    /<PackageReleaseNotes>/ { print ENVIRON["ENTRY"] }
' "$CSPROJ" > "$TMP"
mv "$TMP" "$CSPROJ"

if [ $COMMIT -eq 1 ]; then
    git -C "$(dirname "$0")" add "src/Directory.Build.props" "src/SoundFingerprinting/SoundFingerprinting.csproj"
    git -C "$(dirname "$0")" commit -m "Version bump to v$NEXT" -- "src/Directory.Build.props" "src/SoundFingerprinting/SoundFingerprinting.csproj"
    echo "Committed 'Version bump to v$NEXT'"
else
    echo "Updated $PROPS and $CSPROJ (not committed)"
fi
