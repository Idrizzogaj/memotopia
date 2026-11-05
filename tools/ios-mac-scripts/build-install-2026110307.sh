#!/bin/bash
set -euo pipefail
cd /Users/idrizzogaj/Projects/memotopia
EXPORT=ios-build/Sprint-2026110307
APP=ios-build/DeviceBuild/Build/Products/Release-iphoneos/memotopia.app
/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$EXPORT/Info.plist" | grep -qx 2026110307
if [ "${MEMOTOPIA_UNLOCK:-1}" = 1 ]; then
  security unlock-keychain "$HOME/Library/Keychains/login.keychain-db"
fi
echo "Building and signing Memotopia 2.1.0 for iPhone and iPad..."
if ! xcodebuild -project "$EXPORT/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration Release -destination "generic/platform=iOS" -derivedDataPath ios-build/DeviceBuild -jobs 2 -allowProvisioningUpdates -allowProvisioningDeviceRegistration DEVELOPMENT_TEAM=E838WX6KF8 CODE_SIGN_STYLE=Automatic ARCHS=arm64 ONLY_ACTIVE_ARCH=YES ENABLE_BITCODE=NO GCC_STRICT_ALIASING=NO GCC_GENERATE_DEBUGGING_SYMBOLS=NO DEBUG_INFORMATION_FORMAT=dwarf IPHONEOS_DEPLOYMENT_TARGET=15.0 >ios-build/xcode-focus-2026110307.log 2>&1; then
  echo "Build or signing stopped. Last log lines:"; tail -n 25 ios-build/xcode-focus-2026110307.log; exit 1
fi
/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$APP/Info.plist" | grep -qx 2026110307
/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$APP/Info.plist" | grep -qx 2.1.0
codesign --verify --deep --strict "$APP"
test -f "$APP/PrivacyInfo.xcprivacy"
test -f "$APP/Frameworks/UnityFramework.framework/PrivacyInfo.xcprivacy"
for TARGET in iphone:2CB1A144-09A8-5249-908B-1CDDB7054B5B ipad:2B57BBEE-0997-5428-9799-44BECA4FAFDD; do
  KIND=${TARGET%%:*}; DEVICE=${TARGET#*:}
  echo "Installing on $KIND..."
  xcrun devicectl device install app --device "$DEVICE" "$APP" --json-output "ios-build/install-$KIND-2026110307.json"
  printf '%s\n' "2.1.0/2026110307 installed on $KIND" >"ios-build/installed-$KIND-2026110307.txt"
  xcrun devicectl device process launch --device "$DEVICE" com.retention.memotopia || echo "Installed. Open Memotopia manually on $KIND."
done
echo IOS_BOTH_INSTALLED_2026110307