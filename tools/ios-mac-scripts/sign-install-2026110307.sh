#!/bin/bash
set -euo pipefail
cd /Users/idrizzogaj/Projects/memotopia/ios-build
test -f focus-2026110307-ready.txt || { echo "The build is not finished yet."; exit 1; }
KEYCHAIN="$HOME/Library/Keychains/login.keychain-db"
KEY_LABEL=$(cat signing-key-label.txt)
[ "$KEY_LABEL" = 'Apple Development: Idriz Zogaj (Idriz Zogaj)' ] || { echo 'Unexpected signing key. Aborting.'; exit 1; }
security find-key -t private -l "$KEY_LABEL" "$KEYCHAIN" >/dev/null
trap 'unset signing_password' EXIT
read -r -s -p "The Mac's password (hidden): " signing_password
printf '\n'
security unlock-keychain -p "$signing_password" "$KEYCHAIN"
security set-key-partition-list -l "$KEY_LABEL" -S apple-tool:,apple:,codesign: -s -t private -k "$signing_password" "$KEYCHAIN" >/dev/null
unset signing_password
MEMOTOPIA_UNLOCK=0 bash build-install-2026110307.sh