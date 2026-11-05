#!/bin/bash
set -euo pipefail
cd /Users/idrizzogaj/Projects/memotopia
rm -f ios-build/focus-2026110307-ready.txt
if xcodebuild -project ios-build/Sprint-2026110307/Unity-iPhone.xcodeproj -scheme Unity-iPhone -configuration Release -destination "generic/platform=iOS" -derivedDataPath ios-build/CompileCheck -jobs 2 CODE_SIGNING_ALLOWED=NO CODE_SIGNING_REQUIRED=NO ARCHS=arm64 ONLY_ACTIVE_ARCH=YES ENABLE_BITCODE=NO GCC_STRICT_ALIASING=NO GCC_GENERATE_DEBUGGING_SYMBOLS=NO DEBUG_INFORMATION_FORMAT=dwarf IPHONEOS_DEPLOYMENT_TARGET=15.0 > ios-build/xcode-compile-2026110307.log 2>&1; then
  /usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' ios-build/Sprint-2026110307/Info.plist
  touch ios-build/focus-2026110307-ready.txt
  echo COMPILE_OK
else
  echo COMPILE_FAILED; grep -E "error:" ios-build/xcode-compile-2026110307.log | head -8
fi