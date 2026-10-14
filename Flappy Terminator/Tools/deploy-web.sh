#!/usr/bin/env bash
# Uploads a web build to the test server and switches q-c.space/ft/ to it.
#
#   Tools/deploy-web.sh            newest folder in Builds/Web
#   Tools/deploy-web.sh v0.1.3     a specific version (also how to roll back)
#   Tools/deploy-web.sh --list     versions on the server, the live one marked

set -euo pipefail

SERVER="deploy@147.45.246.87"
RELEASES="/srv/ft/releases"
LIVE_LINK="/var/www/q-c.space/ft"
URL="https://q-c.space/ft/"

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
BUILDS="$PROJECT/Builds/Web"

if [[ "${1:-}" == "--list" ]]; then
    ssh "$SERVER" "live=\$(readlink $LIVE_LINK); for d in \$(ls -1 $RELEASES | sort -V); do
        [[ $RELEASES/\$d == \$live ]] && echo \"\$d  <- live\" || echo \"\$d\"; done"
    exit 0
fi

VERSION="${1:-$(ls -1 "$BUILDS" | sort -V | tail -1)}"
SOURCE="$BUILDS/$VERSION"

if [[ ! -f "$SOURCE/index.html" ]]; then
    echo "No build at $SOURCE" >&2
    exit 1
fi

echo "Uploading $VERSION..."
rsync -az --delete "$SOURCE/" "$SERVER:$RELEASES/$VERSION/"

echo "Switching /ft/ to $VERSION..."
ssh "$SERVER" "ln -sfn $RELEASES/$VERSION $LIVE_LINK.new && mv -T $LIVE_LINK.new $LIVE_LINK"

echo "Live: $URL ($VERSION)"
