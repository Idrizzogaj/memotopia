# Memotopia Android 2.1.0 (23)

Built and verified 26 October 2026 at the user's request, for internal Android testing. Source: `d4854e8` on `codex/focus-2.1.0`; 2,985 tracked script, editor, scene and resource files checked against the separate Android staging project before the build.

User file: `C:/Users/IdrZog_MEM/Downloads/Memotopia-2.1.0-23.aab`.
Original: `C:/Users/IdrZog_MEM/MemotopiaAndroidBuild/Output/Memotopia-23.aab`.

Includes the changes after build22:
- Saved-session startup remains behind the existing Memotopia loading view until the authenticated menu opens; network retry retains the session.
- Says RECALL cue before the countdown/input, earned stars on level tiles and removal of its Challenge prototype button.
- Arena online-first ordering, green/red dividers and Wi-Fi placement between status and challenge sword.
- The Deck tip icon and placement after Major System.
- Client rematch-start guard correction. **The accompanying server rematch fix has not been deployed** because the Mac SSH route is unavailable. This bundle alone cannot fully resolve or verify that server behaviour.

Verification passed:
- Bundletool validation, package `com.zogajretention.memotopia`, version2.1.0/code23, minSDK24, targetSDK36, ARM64.
- JAR signature and matching existing upload certificate; all native libraries pass16KB ELF load alignment checks; no Facebook paths.
- Packaged feature markers, including SavedSessionView, Says recall cue, Deck tip icon, Focus/Practice, Challenge and all52 Deck mnemonic assets.
- Download copy SHA256 matches original: `6ab2e0b8e1ac46ded48f0a685b7a9ba004f88902a506e3fbae5abbb9be584305`; 70,122,199 bytes.

Unity2022.3.62f2 reported Succeeded in9m45s and exited0. Its summary lists the same three existing editor resolver diagnostics as previous successful Android builds; no C# build failure. Local evidence: `Output/unity-build-23.log`, `Memotopia-23.aab.build-summary.txt`, `Memotopia-23.source-manifest.json`, `Memotopia-23.native-verification.json`, `bundletool-validation-23.txt`, `version23-signature.log`, `version23-manifest.xml`, `version23-verification.json`.

No Play upload/publication, Android device installation, new iOS build or server deployment performed. The user will upload the signed bundle to **Memotopia's internal test track**.
