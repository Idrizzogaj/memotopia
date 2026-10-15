> **Newest handoff: `docs/SESSION_HANDOFF_2026-10-10.md` (video/tips update); then read `docs/SESSION_HANDOFF_2026-10-09.md` for the full release context.**

# AI handoff: Memotopia

Read this file first when continuing on another computer or with another AI. It records the verified working state as of 2026-09-18. Detailed history remains in `CODEX_HANDOFF.md`; Windows setup is in `docs/WINDOWS_HANDOFF.md`.

## Mandatory handoff routine

At the start of every work session, read this entire file, then verify `git status`, the checked-out branch and the latest commit before making changes. Treat the repository as the source of truth when it differs from an old note.

Before ending any session—whether for a day, week, year or permanent transfer—update this file in place. Record the current date, latest commit/branch, version and build number, completed work, tests and device/store verification, unresolved blockers, and the next concrete actions. Remove stale “next action” text instead of endlessly appending history. Never include passwords, tokens, private keys or signing material. Commit and push the product changes and this updated handoff together. A future AI should only need the instruction: **Read `AI_HANDOFF.md` and continue from there.**

## Where we stand — handover of 2026-10-04 (read this block first)

Written by the Claude Code session that works on Memo Gym (a different project, repo `ZogajMemoGym`, do not mix the two). Idriz will give the next small Memotopia fixes to another AI; this block says what is known and where to look. Facts below are marked **verified** (checked in files/tools on 2026-10-04) or **last known** (from earlier notes, not re-checked today).

**iOS (the focus):**
- Version in source: 1.0.39 (build 2026091707). **Last known:** submitted for App Review on 2026-09-29 and still "Waiting for Review" on 2026-10-02. The outcome after that is **not known** — ask Idriz or look in App Store Connect (app ID 1442530846, team E838WX6KF8). Nothing is wrong with the submission as far as we know.
- After Apple approves: sale must be **re-enabled manually** under Pricing and Availability (the app was "Removed from sale"; France is deliberately excluded; age rating 13+ because of the challenge feature). The subscriptions (`monthly_subscription_memotopia`, `yearly_subscription_memotopia`) are already Approved, ~19 kr/month and ~149 kr/year; a lifetime unlock was considered and skipped for now. Idriz also applied for the App Store Small Business Program on 2026-10-03 (approval pending).
- **verified:** the Mac copy `~/Projects/memotopia` (ssh `idrizzogaj@192.168.1.209`) is clean on `work/ios` at `7102cae` ("Record validated TestFlight build in AI handoff"), same as `origin/work/ios`. iOS builds, signing and uploads (Transporter) are done on that Mac only.

**Android (paused by Idriz on 2026-09-29; resume only if he asks):**
- The real app is `com.zogajretention.memotopia` (not `com.retention.memotopia`, which is a dead end). Idriz has uploaded **1.0.40, versionCode 2** to **Internal testing** (his confirmation). **Not verified:** that it was ever installed and run on a phone. 16 KB runtime is untested. Subscription products were missing in the new Play app. Do not call it production ready.
- **Where to look:** the working Android build is NOT in this repo but in the isolated copy `C:\Users\IdrZog_MEM\MemotopiaAndroidBuild` (Unity 2022.3.62f2, Billing 8.0.0, API 36). Its own `AI_HANDOFF.md` there is the Android source of truth: build script `Build-AndroidAab-local.ps1` (use versionCode 3 or higher for the next upload), output in `Output\`, reports next to the `.aab`. The upload keystore is under `C:\Users\IdrZog_MEM\.android-signing\memotopia\` (never print or share its contents; it is **still not backed up off this machine**, which Idriz has not confirmed).
- Rules in that folder's handoff still apply: all work locally on Windows and outside OneDrive; no Git commands there; the original project is not modified.
- Nothing from the Android work has been merged back into this repo. Candidate changes are listed in that handoff under "Vad kan/bör senare föras tillbaka".

**This working copy on Windows (`C:\Users\IdrZog_MEM\GitProjects\memotopia`, branch `work/ios`, at `7102cae`) has uncommitted changes** from the first Android attempt on 2026-09-28: `MemotopiaBuild.cs` (BuildAndroid/BuildAndroidRelease), `AndroidManifest.xml` (Facebook leftovers removed), many deleted `androidx.*` files in `Assets/Plugins/Android`, edits to `.gitignore` and two editor validators, and this file. They are described under "Android release attempt" below. Nothing was committed or pushed; Idriz decides that, deliberately, when a milestone is finished.

**Rules that apply to every AI here:**
- Never commit or push without Idriz's explicit go-ahead; never work inside OneDrive; never put secrets in files or chat.
- Idriz wants the Memotopia Android and iOS stories kept apart; check with him which one a task is about.
- Read `CODEX_HANDOFF.md` and `docs/WINDOWS_HANDOFF.md` for older history.

## Repository and architecture

- GitHub: `git@github.com:Idrizzogaj/memotopia.git`.
- Working branch: `work/ios`.
- Unity client: `frontend`, Unity **2019.3.12f1**.
- NestJS backend: `backend`, Yarn 1.22.x.
- iOS bundle ID: `com.retention.memotopia`; Apple app ID `1442530846`; team `E838WX6KF8`.
- Current client version/build in source: **1.0.39 (2026091707)**.
- TestFlight 1.0.39 (2026091707, minimum iOS 15) was uploaded on 2026-09-18. Apple finished processing it with state `VALID` and audience `APP_STORE_ELIGIBLE`; build resource/delivery ID is `95ff7b84-567d-499c-af50-0bdea436fbcf`.

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
- The archived and exported 1.0.39 IPA passed Apple upload and processing. Local generated artifacts remain ignored under `ios-build/`.
- Reviewed renderings and release notes live under `docs/game-flow/`, `docs/achievements/` and `docs/major-system/`.
- Build with at most two jobs on this Mac to limit memory and heat.

## Next concrete action

- (Superseded by the 2026-10-04 block at the top: the build was assigned and submitted for App Review on 2026-09-29.) iOS: In App Store Connect, assign build 1.0.39 (2026091707) to the intended TestFlight tester group if it is not already assigned. Install it on Hanushe iPad and smoke-test login, Pairs, Flash, Boxes, achievements, progression and purchases. Public App Store submission and backend deployment are separate decisions.
- Android: **deliberately paused by Idriz on 2026-09-29** — do not resume Android work unless he brings it up again. A separate session did get much further than the "Android release attempt" note below describes (upgraded Unity to 2022.3.62f2 in an isolated copy at `C:\Users\IdrZog_MEM\MemotopiaAndroidBuild`, fixed Billing/API level, uploaded 1.0.40/build 2 to Internal Testing) — see that folder's own `AI_HANDOFF.md` for the real current Android state if this is ever resumed. Nothing from that effort has been merged back into this repo.
- iOS: **1.0.39 (build 2026091707) submitted for App Review on 2026-09-29** — up to 48h turnaround, Idriz will get an email. The app had been fully "Removed from sale from the App Store" (last live version was the old 0.0.19); France was excluded from distribution (French export-compliance paperwork skipped deliberately) via both the encryption-documentation question and Pricing and Availability. Age rating: 13+ (Contests = Frequent, honestly reflecting the real challenge-friends feature — see below). **Once Apple approves: still need to manually re-enable sale in Pricing and Availability** — an approved version does not automatically resume sale on its own.
- Store listing (English) was rewritten to lead with Idriz's real, verified credentials (Swedish national memory team coach — WMC silver 2012, silver 2013, gold 2014; TEDx talk "How to become a memory master", TEDxGöteborg, one of the most-watched TEDx talks specifically on memory — view count varies 8-10M+ by source, don't repeat the debunked "top 30 of all TEDx talks" claim from a speaker-bureau bio, it doesn't hold up against actual most-viewed-TEDx rankings). App name: "Memotopia: Brain Training" (chosen over "Mental Training" — the latter reads as sports-psychology/mindset coaching in both English and especially Swedish, not cognitive games).
- Confirmed via code: the in-app "Challenge" feature (`ChallengeAPIController`, `Participant`/`Challenge` models) is real, not just a leaderboard — free users get 3 challenge games **per calendar day** (resets on day rollover, not a rolling 12h window — `backend/src/modules/restrictions/restrictions.service.ts`), Premium is unlimited (bypasses the restriction entirely via a `paymentStatus == none` check in `MenuNavigation.cs`). No Game Center integration anywhere in this project (unlike Memo Gym, which has a harmless false-positive entitlement warning — Memotopia has neither the feature nor the warning).

## Android release attempt (2026-09-28, paused)

First real attempt to get Memotopia onto Android, done entirely on the Windows machine (`C:\Users\IdrZog_MEM\GitProjects\memotopia`, branch `work/ios`, all changes below still **uncommitted**).

- **Play Console has two app records now.** `com.retention.memotopia` (created earlier, matches the iOS bundle ID) is a dead end — it has a phantom app-signing certificate pinned from some earlier, now-untraceable attempt (likely tied to the lost `memotopiaMyStore.keystore` referenced elsewhere in this file), and none of Play's three "Appsignering" enrollment paths (existing key from keystore, existing key from another store, new key with a two-part rollout) can satisfy it since we have no matching old key. **Leave that app record alone, do not try to fix it.**
- The real one going forward is **`com.zogajretention.memotopia`** (the original `com.zogaj.memotopia` was already taken by the same lost old attempt). It is a brand-new, never-published app entry; Play App Signing was successfully enrolled on it using a freshly generated upload key.
- **New upload keystore**: `C:\Users\IdrZog_MEM\.android-signing\memotopia\memotopia-upload.jks` (alias `memotopia`), password in the git-ignored, sibling `memotopia-upload-credentials.local.txt` in that same folder. **This has not been backed up anywhere off this machine yet** — do that before anything else touches this app again, or we repeat the Memo Gym lost-upload-key saga permanently this time (there is no fallback old key here).
- `frontend/Assets/Editor/MemotopiaBuild.cs` gained `BuildAndroid()` (debug-signed local APK, sideload testing only) and `BuildAndroidRelease()` (signed `.aab` for Play, reads the keystore path/passwords from `MEMOTOPIA_ANDROID_KEYSTORE_PATH`/`MEMOTOPIA_ANDROID_KEY_ALIAS`/`MEMOTOPIA_ANDROID_STORE_PASS`/`MEMOTOPIA_ANDROID_KEY_PASS` process environment variables — never hardcoded, never persisted to the registry). Both target `com.zogajretention.memotopia`, `minSdkVersion` bumped to 21 (Play's automatic Store protection minimum). Android build support needs the standalone JDK 8 at `C:\Users\IdrZog_MEM\.jdk8\jdk8u504-b01` (Unity 2019.3's bundled OpenJDK install failed here on a Windows elevation prompt); Android Studio's bundled JBR (`C:\Program Files\Android\Android Studio\jbr`, Java 21) is needed separately to run Google's `pepk.jar` for the Play App Signing enrollment, since that tool needs Java 11+.
- Removed dead Facebook SDK leftovers from `frontend/Assets/Plugins/Android/AndroidManifest.xml` (activities, meta-data, and a `FacebookContentProvider` whose authority ID collided with another developer's app on Play) — the actual Facebook SDK plugin files are already gone from the project (Facebook login is fully disabled in `AuthView.cs`), these were just orphaned manifest entries blocking upload.
- Also uncommitted: a batch of `androidx.*`/Kotlin-stdlib `.aar`/`.jar` files deleted from `Assets/Plugins/Android/` (pre-dates this session; looks like a move from manually-vendored jars to EDM4U-resolved dependencies — EDM4U is already configured in `frontend/ProjectSettings/GvhProjectSettings.xml` with Jetifier on).
- **Blocked on, in order hit:** (1) `androidUseCustomKeystore`/package-name mismatches — resolved. (2) Play App Signing enrollment against the wrong app — resolved by creating the new app record. (3) `minSdkVersion` 19 too low for Play's automatic protection — fixed, now 21. (4) Facebook manifest conflict — fixed, removed. (5) **Currently stuck**: Play Console rejects the build because (a) the bundled Play Billing Library is 3.0.1, Play now requires >=8.0.0, and (b) the app targets API 28, Play now requires >=36. Both stem from **Unity 2019.3.12f1 itself being too old** — its bundled Android Gradle Plugin/Billing integration predate what current Play policy requires. Android SDK platform 36 and build-tools 36.1.0 are already installed locally, so that part isn't missing; the open question is whether Unity 2019.3's Gradle export can even be forced to compileSdk/targetSdk 36 and a newer Billing Library, or whether this requires the Unity engine upgrade that's been deliberately deferred elsewhere in this file. **Do not attempt small patches here again without first deciding whether to do that Unity upgrade** — this needs a real scoping/decision, not another quick fix attempt.
- Play Store listing copy (English, "Memotopia: Brain Training") and placeholder graphics (icon flattened to 512×512, feature graphic, phone screenshots pulled from `docs/game-flow/review/`) were prepared and handed to Idriz directly (not saved in this repo) — ask him if this needs to be re-fetched.

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
