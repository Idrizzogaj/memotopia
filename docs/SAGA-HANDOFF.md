# Memotopia Saga � handoff 2026-11-23

Branch: `codex/saga`, originally based on the fetched `origin/codex/focus-2.1.0`. No other branch is pushed.

## Current approved scope

- One Saga entry above the existing home controls. Statistics, Training, Challenge, Videos, Tips and side-menu controls remain.
- Five games in this exact order at each difficulty level: **Chain, Flash, Pairs, Boxes, Says**. Complete all five at level 1, then all five at level 2, through level 20: **100 stops**.
- `SagaRoute.Games` defines ordering; game and local level identify saved results. Changing route order does not rewrite game-level progress.
- All stops/sections remain open during development. Ordered unlocking is implemented as the previous route stop having a completion result; release policy is still undecided.
- Use the approved original 20-neuron image, recoloured in a repeating turquoise/yellow/blue/pink/violet pattern. The user rejected the sharper isolated-cell map and requested no game labels, only numbers 1-20.
- `Resources/Saga/Map20.png` is the approved colour revision, stored in the repository. It includes the route, gold myelin, numbers, title and painted Back label. Native transparent hit targets overlay its nuclei and Back area.
- The 100-stop route is presented in five sections of 20. Section controls show 1/5 through 5/5; each section retains the image's numbers 1-20. Internal game/level identity is independent of section numbering.

## Games and initial balance

| Local levels | Chain/Flash pictures | Pairs: learned pairs | Boxes placements | Says signals | Seconds per recall |
| --- | --- | --- | --- | --- | --- |
| 1-5 | 10 | 4 | Existing levels 1-5: 2,3,4,4,5 | 3,3,4,4,5 | 90,88,86,84,80 |
| 6-10 | 15 | 6 | Existing levels 6-10: 6,6,8,8,9 | 5,6,6,7,7 | 80,78,76,74,72 |
| 11-15 | 20 | 8 | Existing levels 11-15: 10,12,12,15,15 | 8,8,9,9,10 | 70,68,66,64,62 |
| 16-20 | 25 | 10 | Existing levels 16-20: 15,16,20,20,22 | 10,11,11,12,12 | 60,58,56,54,52 |

The Pairs/Boxes/Says values are an intentionally gentle first tuning, not empirically validated equivalent difficulty. Sources and limits are recorded in `GAME-DESIGN-SOURCES.md`. User explicitly reintroduced Pairs, superseding its original exclusion.

- All Saga rounds start with three hearts; wrong answers consume one. Full completion wins; zero hearts or recall expiry loses.
- Chain keeps its original NEXT presentation, with a study budget equal to the recall budget.
- Flash uses its original scene hierarchy, `FlashCellScale` controller/clip, random study locations and the original `CellController.PresentImage` helper. It shows the entire automatic sequence at native speed; **only recall counts down**. Instance event receivers avoid changing legacy global Flash events. The native template has 24 study cells: a clean duplicate at an existing location handles the 25th picture without omitting it or inventing a new board.
- Pairs reuses the original two-image study layout, original card prefab, back faces, flip animation and the actual native ScrollRect content (not the inactive legacy CardContent object). Recall matches the learned associations, not identical images. An incorrect pair turns back after 0.5 seconds and costs one Saga heart.
- Boxes reuses exported phone/tablet scene hierarchies, native per-level board geometry, perspective pictures and native drag/tray gestures. Study locations remain fixed; recall places pictures into their original boxes. The exported shells contain no live legacy managers. Recall can start early through the original control, or automatically when the study budget ends.
- Says uses the existing SaysGame keypad, highlighting, playback, RECALL transition, wrong/right feedback and result prefabs. The whole signal sequence plays before recall; Training Practice mode does not disable the Saga recall deadline.
- Chain/Flash recall placement shuffles: none for local levels 1-5; once halfway for 6-10; twice at thirds for 11-15; up to three at quarters for 16-20. Once ten pictures remain on levels 16-20, skip pending timed shuffles and shuffle after each correct answer. Incorrect answers do not shuffle. Shuffles never change the answer sequence.
- Boxes targets and Says keys do not shuffle. Pairs retains its native matching layout; the Chain/Flash shuffle layer is not applied to these different mechanics.
- Replay, NEXT LEVEL and return-to-map use original result prefabs. NEXT follows the five-game route. Stop 100 returns to the map, never level 21.

## Progress and pending product decisions

- Best Saga stars are remaining hearts (1-3), persisted locally per account under `saga.<game>.stars.v1.<owner>.<level>`. Original Chain/Flash key strings are preserved. No historical times are fabricated.
- Existing Training progress, Practice records, XP, Memory Athlete badges and server keys are not modified by Saga. Account changes abort active rounds; writes check ownership.
- **Device-local only:** no server sync or new achievement IDs. Access, XP/badges, final unlocking and release version remain undecided.
- Idriz is considering a separate **Memotopia Zaga** app, first 20 stops free and the next 80 unlocked for roughly SEK 39. This is discussion only: **no new app identity, repository, paywall, purchase product, store submission or entitlement change has been authorised or implemented**. Maintain a shared implementation until a decision is made.

## Validation and environment

- Isolated editor project: `C:/Users/IdrZog_MEM/SagaReview-20261123`, product `MemotopiaSagaValidation`, Unity 2022.3.62f2. It uses copied assets/packages, synthetic accounts, dummy receipt-key classes and legacy Input settings. These stubs/settings never go into the production project or Git.
- `SagaValidation.Validate` checks all 100 route identities, every game's representative level groups, real UI callbacks, victories, next-game transitions, result handling, independent saves and original Training behavior. It also renders phone/tablet previews. Passed after the approved bitmap map and live Pairs ScrollRect content correction; phone/tablet screenshots reviewed.
- `SagaPlayDriver.Begin` is an editor-only runtime driver, excluded from player builds. It validates the real, full-speed 25-picture Flash animation and native Boxes drag/drop. Passed: all 25 pictures display in original order before recall, static Flash handlers remain unchanged, and native Boxes drag/drop completes the round. The isolated runtime test locks assembly reloads because Google dependency updates otherwise interrupt its coroutines.
- A full C# 7.3 source check with legacy API preprocessor paths passes against the available Unity 2022 references. This is not a substitute for compilation against Unity 2019.3.12f1 on the Mac.
- No APK/AAB/IPA, installation, store build, server operation, Unity upgrade or signing change has been performed. Android staging is unchanged. Read-only SSH confirmed the Mac and paired iPhone are reachable. Idriz authorised cleanup and an iPhone test build on 2026-11-23. Native Unity 2019 compilation and iOS export now pass. Xcode compilation completes, but automatic signing stopped with errSecInternalComponent; the owner must unlock the login keychain in the Mac Terminal.
- Idriz explicitly authorised an iPhone test build/install on 2026-11-23; do not ask for build permission again for this test. Store submissions remain unauthorised. Never handle signing passwords. Use included allowance only and stop at 90% usage.

## Pending iPhone build

- Mac: idrizzogaj@192.168.1.209, Unity 2019.3.12f1. Existing Mac repo branch work/ios is heavily dirty/divergent: do not pull, reset or overwrite it. Prepare an isolated copy preserving native integration/settings.
- Cleanup authorised by Idriz: removed ios-build/CompileCheck, DeviceBuild and Sprint-2026110307, plus regenerable Xcode device symbols for iPhone17,3 26.6.2. Preserved source, release xcarchive and signing files. Free space reached 9.5 GiB; monitor after native build and remove this isolated test copy's expendable build intermediates after installation.
- Latest runtime log: C:/Users/IdrZog_MEM/SagaReview-20261123/play-five-6.log; final integration log: validate-final-map.log. Native iOS compilation/export passed; signing/install pending owner keychain unlock.

- Isolated Mac copy: /Users/idrizzogaj/Projects/memotopia-saga-test-20261123; source commit f65a926. Only this copy uses test version 2.2.0 build 2026112301. Original work/ios repo untouched. Export: ios-build/Saga-2026112301. Xcode logs: ios-build/xcode-2026112301.log; build-result.log.
- Reusable signing/build script: ios-build/build-saga-ios.sh (does not unlock or request passwords). A Mac Terminal was requested over osascript to run security unlock-keychain interactively, then this script. Never capture/read the Terminal session or its password.
- Intended installation target only: iPhone 16, device 2CB1A144-09A8-5249-908B-1CDDB7054B5B. Never install on Hana's iPad. After SAGA_IOS_BUILD_PASSED, use xcrun devicectl device install app, then process launch com.retention.memotopia.

## Map update and expanded scope (2026-11-23)

- Idriz confirmed the initial iPhone signing/install script worked. Initial phone test version remains 2.2.0 (2026112301). This map update has not yet been installed.
- Implemented: continuous upward scrolling, 20 stops per viewport, global numbers 1-100, Statistics below Videos/Tips, original glossy artwork restored after the user rejected a flat variant, static turquoise/lavender neural background, grey future cells, post-victory automatic return and electrical next-cell reveal. Existing developmental all-level access remains enabled; grey is a progress state rather than a restriction in this development version.
- Ambient effects: sporadic small local UV vibration/discharge on colored visible cells, private cosmetic random generator, 3-9 second intervals, no gameplay effects. Device-local Motion On/Off cancels ambient effects and skips scroll/unlock motion.
- Checks: validate-scroll-3.log passed full five-game integration; play-map-3.log passed actual Says/Chain completion, cross-section20->21 reveal, spontaneous effects and motion cancellation; C#7.3 legacy-define compile-map-legacy.log passed. Actual native iOS build of these later changes still pending.
- Built-in image tool asset: frontend/Assets/Resources/Saga/MapUnnumbered.png. Final accepted prompt: remove white numerals1-20 and top text only; retain exact glossy cells, positions, colors, myelin and white background. Runtime UI shader composites it over the background. Flat and failed transparency variants are NOT used. No external image API/paid credits used.
- New approved pending scope: six games (add Order, Saga-only) plus a boss every seventh stop =>140 stops. Pairs levels1-20 use1-20 pairs and one90-second study+recall budget. Order uses5/10/15/20 images; study times[10,9,8,7,5], [20,19,18,17,15], [25,24,23,22,20], [30,28,26,24,20]. Shared3hearts unless revised.
- Boss: random permutation of the previous six games at the same local level; six completion icons, one90-second deadline across all study, recall and transitions. Ordinary Chain keeps separate study/recall clocks. Owner approved faster automatic Flash/Says and shorter Order study ONLY in bosses. Keep counts/level identity. Boss total may be increased after testing; expose one configuration constant. Do not silently change ordinary game timing.
