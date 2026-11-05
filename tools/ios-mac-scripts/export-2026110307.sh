#!/bin/bash
set -euo pipefail
cd /Users/idrizzogaj/Projects/memotopia
sed -i '' -e 's/buildNumber = "2026102504"/buildNumber = "2026110307"/' -e 's#ios-build/IzFreshBuild#ios-build/Sprint-2026110307#' frontend/Assets/Editor/MemotopiaBuild.cs
grep -n 'buildNumber\|Sprint-' frontend/Assets/Editor/MemotopiaBuild.cs
rm -rf ios-build/Sprint-2026110307
/Applications/Unity/Hub/Editor/2019.3.12f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/idrizzogaj/Projects/memotopia/frontend -executeMethod MemotopiaBuild.BuildIOS -logFile ios-build/unity-build-2026110307.log || true
grep -c 'error CS' ios-build/unity-build-2026110307.log || true
tail -n 3 ios-build/unity-build-2026110307.log