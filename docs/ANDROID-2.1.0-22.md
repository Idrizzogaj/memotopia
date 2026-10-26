# Memotopia Android 2.1.0 (22)

Built 2026-10-26 for internal testing, with the same game code, scenes and resources as the installed iOS 2.1.0 / 2026102504 release. Android keeps its existing package identity, signing certificate and Android-specific dependencies/manifest. Source baseline `cac0104`; Android build-helper update `2f3ac98` sets version 2.1.0 and checks Says/emoji assets before building.

Output: `C:/Users/IdrZog_MEM/MemotopiaAndroidBuild/Output/Memotopia-22.aab`. User copy: `C:/Users/IdrZog_MEM/Downloads/Memotopia-2.1.0-22.aab`.

- Package: `com.zogajretention.memotopia`; version name 2.1.0; version code 22.
- ARM64, min SDK 24, target SDK 36; Unity 2022.3.62f2.
- Bundletool validation and manifest checks passed. JAR signature verified and upload certificate matches the existing certificate.
- Every packaged native library passes 16KB ELF load alignment checks. No Facebook paths packaged.
- Packaged feature markers verified for Challenge, Focus, Says, Practice session/records/statistics, Major catalog, Deck and all 52 mnemonic resources.
- 70,116,004 bytes. SHA256: `399b1eddcb4e9543031d6a8ac5d7796398944a5dfc476a71b176c2130b970e84`.
- Build result Succeeded; duration 14m11s. Summary includes the same three pre-existing Editor dependency resolver diagnostics (iOS Xcode reference / versioned resolver assembly names) seen in prior Android builds. No C# build failures. No Android device testing performed in this turn.

Evidence is local in `MemotopiaAndroidBuild/Output`: `Memotopia-22.source-manifest.json`, `Memotopia-22.native-verification.json`, `version22-verification.json`, `bundletool-validation-22.txt`, `version22-signature.log` and `Memotopia-22.aab.build-summary.txt`. The AAB is signed and ready for the user to upload to Memotopia's internal test track; it has not been uploaded or published by the agent.
