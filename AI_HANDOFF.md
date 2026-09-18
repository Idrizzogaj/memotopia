# AI handoff: Memotopia

Read this file first when continuing on another computer or with another AI. It records the verified working state as of 2026-09-18. Detailed history remains in `CODEX_HANDOFF.md`; Windows setup is in `docs/WINDOWS_HANDOFF.md`.

## Repository and architecture

- GitHub: `git@github.com:Idrizzogaj/memotopia.git`.
- Working branch: `work/ios`.
- Unity client: `frontend`, Unity **2019.3.12f1**.
- NestJS backend: `backend`, Yarn 1.22.x.
- iOS bundle ID: `com.retention.memotopia`; Apple app ID `1442530846`; team `E838WX6KF8`.
- Current client version/build in source: **1.0.39 (2026091707)**.
- TestFlight 1.0.38 (2026091706, minimum iOS 15) was uploaded successfully on 2026-09-17. Build 1.0.39 has been built and installed locally but was not uploaded before this handoff.

## Current product state

- Pairs, Flash and Boxes use the same verified Major System set of 100 images in `frontend/Assets/Resources/MajorSystem/00.png` through `99.png`. Preserve every `.meta` file and GUID.
- Boxes renders each upright image on a separate rounded, slightly skewed plate. Flash and Pairs use the clean image without that plate.
- Achievements were repaired and extended with local persistence/retry, account isolation and updated UI. Focused backend tests and Unity validations passed.
- iPhone safe areas, menu flow, loading transitions, disabled side-menu actions and result navigation were polished. Rate stays disabled until the public store listing works. Facebook login is disabled. Go Premium is enabled only when required products are available, and the unsupported iOS monthly-to-yearly upgrade stays blocked.
- Statistics no longer invents first place when the current account is absent, handles short lists safely and refreshes on enable.
- The iOS startup crash observed in 1.0.32 was fixed by keeping `GCC_STRICT_ALIASING=NO`; the build postprocessor also removes `-mno-thumb` and tolerates missing dSYM output. Preserve these until a Unity migration proves they are no longer needed.

## Verified builds and devices

- Unity validation, ReviewAndBuild and Xcode device builds passed for 1.0.39.
- 1.0.39 was installed on ZogajSon iPhone. Hanushe iPad has the previously verified 1.0.38 build; update it from TestFlight or a fresh device build when it is available.
- Reviewed renderings and release notes live under `docs/game-flow/`, `docs/achievements/` and `docs/major-system/`.
- Build with at most two jobs on this Mac to limit memory and heat.

## Important source and commands

- `frontend/Assets/Editor/MemotopiaBuild.cs`: repeatable Unity iOS export and Xcode compatibility fixes.
- `frontend/Assets/Editor/MajorSystemValidation.cs`: verifies all 100 Major System sprites.
- `frontend/Assets/Editor/FlowValidation.cs` and `AchievementValidation.cs`: focused regressions.
- `frontend/Assets/Scripts/GamesScripts/`: Pairs, Flash, Boxes and progression.
- `frontend/Assets/Scripts/views/`: safe areas, navigation, achievements, statistics and account UI.
- `backend/src/modules/achievements/achievements.unit.ts`: focused backend achievement tests.

On macOS, use the exact Unity editor version:

```sh
/Applications/Unity/Hub/Editor/2019.3.12f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath "$PWD/frontend" -executeMethod MajorSystemValidation.Validate -logFile /private/tmp/memotopia-major-validation.log
```

Run the additional validators documented in `CODEX_HANDOFF.md` before a release. Generated Xcode projects, archives and IPA files belong under ignored `ios-build/`, not in Git.

## Continue Android work on Windows

1. Clone branch `work/ios` into a short path such as `C:\Projects\memotopia`.
2. Install Unity Hub and Unity 2019.3.12f1 with Android Build Support, SDK/NDK/OpenJDK, plus iOS Build Support because editor scripts reference Unity's iOS APIs.
3. Open the `frontend` directory. Let Unity rebuild Library/Temp locally; do not copy Mac caches.
4. Run the Major System and flow validators, then manually exercise login, Pairs, Flash, Boxes, achievements and progression.
5. Locate the existing Android signing keystore through a secure private transfer. It is not stored in Git. A new key cannot update an already published Android app.
6. Build and test Android on a real device before changing the store release.

Shared Unity/client and backend work can be done on Windows. Final iOS compilation, signing, archive export and TestFlight upload require this Mac with Xcode, or a configured macOS CI service. Commit and push Windows changes first, pull them on the Mac, rerun Unity/Xcode validation, increment the iOS build number, then upload.

## Release and security guardrails

- Never commit `.env` files, tokens, App Store Connect `.p8` keys, certificates, provisioning profiles, Android keystores, passwords, archives, IPA/APK files, generated Xcode projects, Unity Library/Temp or backup files.
- Do not upgrade Unity as part of ordinary feature work. Treat it as a separate migration with regressions for login, purchases, all games and iOS/Android builds.
- Keep the backend deployment separate from client/TestFlight work. Review production configuration and the documented empty challenge-list SQL issue in `docs/SERVER_CHECK_20260917.md` before deploying server changes.
- Bump the Apple build number for each upload. Public App Store release is a separate action from TestFlight.
