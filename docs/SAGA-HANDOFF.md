# Memotopia Saga handoff

Branch: codex/saga. Start point: fetched origin/codex/focus-2.1.0, including unpublished Focus bugfixes. Push only this branch. Do not touch Captain Zogaj or Memo Gym.

## Current scope

- 20 difficulty tiers. Each tier has 12 stops: Chain, Flash, Pairs, Boxes, Says, SORT, memory boss, Focus Pairs, Focus Boxes, Focus Flash, Focus Chain, Focus boss. Total240 stops. Mode/level progress is independent of visual stop numbers, so existing Saga wins survive route expansion.
- Four groups of FIVE levels,1-5/6-10/11-15/16-20, explicitly approved via annotation51453439-3ae4-480d-8b03-7592c4c77b13. Earlier suggestions of five groups of four were superseded.
- Memory rounds start with3hearts. Focus rounds start with5hearts. Every ordinary round starts full. Inside a boss the same hearts AND deadline carry through its sub-games: memory3 total across6games; Focus5 total across4games. This is the final clarification, superseding a brief suggestion to reset per boss sub-game.
- DevelopmentAllLevelsOpen remains true, as requested while developing. Future cells are visually grey but still accessible during development. Only completed/current cells have color. Disable the flag before a future production release when authorised.

## Memory configuration

- Chain uses10/15/20/25 images by five-level group. Flash now follows the native Flash count curve:[3,4,5,6,7,8,9,10,11,12,14,16,18,20,21,22,24,26,28,32]. The earlier inherited Chain counts made early Flash incorrectly difficult.
- Chain study and recall have SEPARATE budgets:[90,88,86,84,80,80,78,76,74,72,70,68,66,64,62,60,58,56,54,52]. Level5 intentionally jumps84->80. NEXT advances study without waiting out the budget. Ordinary Chain does NOT use the boss shared clock.
- Flash uses the exact native automatic animation at speed1, showing every picture including all32 at level20. Only recall counts down. No forced study deadline or NEXT.
- Chain/Flash recall: levels1-5 no shuffles;6-10 once halfway;11-15 twice at thirds;16-20 up to3timed shuffles, but once10remain cancel pending timed shuffles and reshuffle after each correct answer. Incorrect answers never reshuffle.
- Pairs now has1-20 learned pairs at levels1-20. One90second budget covers study+recall. Starting recall MUST retain the remaining study budget; no reset. Native associated pairs, not identical-image matching.
- Boxes uses5/10/15/20 images by five-level group, explicitly confirmed after phone feedback. Select the smallest existing native board with enough picture slots for the group. Native perspective pictures and drag/drop. Locations stay fixed.
- Says retains its native playback/keypad. Sequence length3+floor((level-1)/2), recall-only countdown. No new Training achievements or server IDs.

## SORT (Saga-only)

-5/10/15/20 images by group. Study seconds:[10,9,8,7,5],[20,19,18,17,15],[25,24,23,22,20],[30,28,26,24,20], confirmed by Idriz.
- Show all images in order. On study expiry, shuffle identities in EXACTLY the same grid/positions. No separate tray or blank destination board; the earlier Boxes-like version was rejected.
- Drag an occupied picture to another occupied position: swap the two pictures. All swaps are free; do not grade individual drops or freeze correct pictures.
- DONE grades the whole arrangement. Correct ends the round. Incorrect costs exactly ONE heart, shakes the entire board briefly, and retains the arrangement for corrections. Three unsuccessful submissions lose the memory round. Recall currently has an initial90second budget; configurable in SagaLevel.
- Uses existing native card/Next assets and DragAndDropItem, with SagaSortSlot as drop receiver. No SORT entry in normal Training.

## Focus configuration

- Reuse native FocusGame, starting at native level1 and retaining its automatic within-round difficulty progression. Saga tier controls goal/time, not the native starting level. This was explicitly approved.
- All four games use time schedule:[20,19,18,17,15,30,29,28,27,25,40,39,38,37,35,60,58,56,54,50]. Same five-step endpoint pattern as SORT.
- Targets by five-level group: Pairs10/15/20/30; Boxes20/30/40/60; Flash10/20/30/50 (50 explicitly corrected from40); Chain30/50/70/100 (supersedes earlier five-value draft).
- Correct+1, wrong-1 as native Focus, plus5hearts in Saga only. Five errors fail even if score remains positive. Reaching the target ends the Saga round immediately. Timeout also fails.
- Saga Focus ignores Practice for timing, does not write FocusStats sessions/bests, does not earn/flush Training achievements or XP, and does not mutate existing Training settings. Normal Focus retains its native90second score-attack and existing lost-focus behavior.
- Focus stars use the existing native5-life conversion:5hearts->3stars,3-4->2stars,1-2->1star.

## Bosses

- Both currently start with one90second deadline, centrally configured in SagaRoute.BossSeconds. Timer includes all study, recall, transitions and quit dialogs. Random permutation without replacement; completion icons fill by game identity. Child successes never save individual ordinary-level progress.
- Memory boss uses the previous six games at the same local tier,3shared hearts. Boss-only automatic playback: Flash speed4, Says speed4.5; SORT study capped5seconds. Counts and ordinary-round timing remain unchanged. Owner approved shortening these automatic phases to make the shared clock feasible.
- Focus boss uses the four preceding Focus goals at the same tier,5shared hearts. Native Focus gameplay/playback unchanged. Boss expires/fails at deadline or exhausted shared hearts.
- Late boss suggestion:90seconds for tiers1-15 and120for16-20. This is a proposed test setting, NOT yet applied/approved; user said120might be needed after testing. Do not claim the prototype is balanced for human play. Exact timings/goals need phone testing.

## Map and home UI

- Continuous upward scroll,20stops per viewport, global numbers1-240. Starts at bottom. No game names on map. Fixed original heading/Back. Original glossy neurons, gold myelin; flat variant was explicitly rejected.
- Accepted built-in image tool asset: Assets/Resources/Saga/MapUnnumbered.png. Prompt: remove only white numerals1-20 and top text, preserving original glossy neurons, exact positions/colors/myelin and white background. Failed transparency and flat variants are NOT used. Map20.png retained for original heading.
- Runtime UI shader composites original artwork over a static pale turquoise/lavender neural background. Only completed/current cells colored. Boss cells have a star marker.
- Win: native victory/stars first, automatic map return, scroll to the next section, electrical pulse reveals next cell. No extra tap required.
- Ambient: sporadic local vibration/discharge on colored visible cells, private cosmetic RNG,3-9second gaps, tiny travelling cyan pulse, no full-screen strobe. Device-local Motion On/Off cancels ambient effects and skips transition motion. Does not affect gameplay.
- Home keeps all existing controls. Saga above Training/Challenge. Videos and Tips remain. Statistics now below Videos/Tips.

## Persistence and source safety

- Saga keys remain saga.<game>.stars.v1.<owner>.<local-level>, best stars only. All new keys local; no server operations or new server achievement IDs. Normal Training progress untouched. No historical stats invented.
- Unity2019.3.12f1 remains the iOS SDK; Android uses separate2022.3.62f2 staging. Never upgrade either. Use UNITY_2022_2_OR_NEWER guards where required. Android staging has not been changed in this task.
- Design sources/rationale in GAME-DESIGN-SOURCES.md. Any new mechanics/balance recommendations require three independent relevant sources including a new one versus prior relevant search. Distinguish empirical evidence, developer examples and own judgment.

## Validation and build status

- Isolated Windows project C:/Users/IdrZog_MEM/SagaReview-20261123, product MemotopiaSagaValidation, Unity2022.3.62f2, fake owners19999998xx, dummy receipt-key stubs. Stubs, caches, preferences and test binaries never enter Git.
- Passed validate-all240.log sections:240route/memory integration and80Focus targets/native callbacks/5hearts/goal/timeout/save isolation. Final normal-Focus section originally failed only because output directory ../ChainReuseFix was absent. Directory created; validate-native-focus-after-saga.log then passed original native Focus checks. Wrapper now creates the test directory.
- Earlier memory runtime play-boss-final.log passed all6games at local20, actual25-picture Flash, SORT swaps/DONE/wrong-heart/shake, six icons, shared90clock, inherited hearts, isolated boss progress and timeout. Automated correct answers are not evidence of human balance.
- Passed play-focus-boss-3.log: actual four-game Focus boss at tier20, shared90second deadline and shared5hearts, target completion, four filled icons, carried errors and no Training sessions. An initial test-harness reflection bug was fixed by including public fields; no gameplay fix was required. Passed play-map240-final.log:240stops, actual Focus Pairs/Boxes wins,20->21scroll, electrical reveal, ambient effects and motion-off behavior.
- C#7.3 legacy-define compile-focus-final.log passed latest Focus/player source against available2022references. Actual2019iOS compilation/export will occur when Idriz runs the prepared command. No store upload authorised.
- Map commit0f9c732 precedes this verified expansion. Commit/push the final source only to codex/saga and sync tracked Assets into the isolated Mac copy.

## iPhone/Mac

- Initial test2.2.0 build2026112301 installed successfully; Idriz confirmed script worked. Later map/6game/Focus work NOT built/installed yet. Ask Ska jag bygga? before the next phone app build; previous installation is already complete.
- Mac idrizzogaj@192.168.1.209, home LAN. Original /Users/idrizzogaj/Projects/memotopia is heavily dirty on work/ios; never pull/reset/overwrite it.
- Isolated copy /Users/idrizzogaj/Projects/memotopia-saga-test-20261123 holds initial export/build. Owner approved cleanup as needed: removed original ios-build/CompileCheck,DeviceBuild,Sprint-2026110307 and regenerable Xcode iPhone17,3 device symbols. Source/release xcarchive/signing preserved. Old installed test app was compressed to Installed-2026112301.zip and CRC-verified; old isolated export/DerivedData, PlayerDataCache for original/isolated Memotopia, old1.0.xexports and source tar were removed within the authorised regenerable-cache scope.7.6GiB is free before new build. Any further cleanup must stay within approved expendable build/cache scope, not other projects/user documents.
- Initial scripts live in isolated ios-build: build-saga-ios.sh and sign-install-saga.sh. The latter prompts securely in Mac Terminal, unlocks login keychain, builds/signs/verifies privacy, installs only on iPhone16 selected from the paired-device inventory and launches com.retention.memotopia. Never install on Hana's iPad. User types passwords; never capture Terminal/password or put credentials/signing/user data/builds in Git.
- Use only subscription allowance; pause at90percent. Usage read late session showed27percent weekly used, ordinary allowance still available. Never consume credits or redeem resets.

- New manual command for test2.2.0 build2026112401: bash ~/Projects/memotopia-saga-test-20261123/ios-build/sign-install-saga-2026112401.sh. Prepare/sync source before handing it off. This script performs Unity export, interactive keychain unlock, Xcode build/sign/verify and owner-iPhone installation; it never publishes to stores. Script/logs/builds remain outside Git. Original Mac work/ios source remains untouched; only its expendable PlayerDataCache was cleared with cleanup approval.

## Build command ready (2026-11-24)

Source719e75c was pushed to codex/saga and synced as tracked Assets into the isolated Mac copy. FocusGame.cs, SagaRules.cs and SagaPage.cs match their Git blobs after CRLF normalization. Local-only MemotopiaBuild.cs overrides testversion2.2.0/build2026112401/exportSaga-2026112401. New sign-install script syntax passed bash-n; Unity2019 executable/privacy source exist. Free space7.5GiB after sync. The new app build has NOT been started; owner requested the command to run himself. Initial installed build2026112301 is saved as CRC-verified Installed-2026112301.zip. Latest player source compile-focus-final.log passed C#7.3 legacy paths. Source code commit remains719e75c; this later documentation update changes no app code.

## iPhone installation verified (2026-11-24)

Idriz ran sign-install-saga-2026112401.sh in Mac Terminal and entered the keychain password himself. Unity2019.3.12f1 export and Xcode Release build succeeded. A scoped devicectl apps query for com.retention.memotopia verified version2.2.0/bundle2026112401 installed on the owner iPhone. Source commit719e75c, with local-only build metadata overrides. No store upload or server change. User now needs human playtesting of flow, scoring and boss difficulty; automated tests do not establish balance. Never capture the Terminal password.

## Phone feedback fixes (2026-11-24, after build2026112401)

- All ordinary memory/Focus games and both bosses use SagaResultView. Win AND loss titles use SAGA LEVEL plus the global map stop, never the native game title/local tier. Keep the original result artwork, native replay icon and star reveal; MAP is always enabled alongside replay. Training results are unchanged.
- Actual GameMenu stores MenuNavigation/SagaPage on a scene root outside the Canvas. The previous auto-return searched Canvas from that controller and threw NullReferenceException, leaving disabled result buttons. ReturnAndUnlock now gets Canvas from its map view and observes the specific Saga result. A manual MAP/replay consumes that result and cancels its old auto-return, preventing it from closing the new round.
- Flash recall timer explicitly activates/enables its native Text, with fitting/overflow allowed so glyphs are not dropped by native text clipping. Only recall counts down, as agreed. The isolated render shows89 after a one-second tick from90. Phone rendering of this fix still needs a new build.
- Final validation: validate-boxes-final.log passed240route integration, approved Boxes5/10/15/20 on phone and iPad boards, every memory type at tier boundaries, visible/decreasing Flash clock, global result labels, pointer-click MAP/replay, all80Focus goals and unchanged native Focus. play-results-Begin.log passed actual full-speed32picture Flash and five-picture Boxes drag/drop/MAP at stop4. play-results-BeginMap.log passed the real scene-root controller arrangement, auto return/electric unlock and manual replay/MAP cancellation. play-results-BeginFocusBoss.log passed the four Focus games. play-boxes-boss-final.log passed all6memory games after the final Boxes update, shared deadline/hearts, native animation/drag-drop, isolated progress and timeout. compile-boxes-final.log passed C#7.3/legacy Unity defines against available references. Phone build2026112401 remains installed; these fixes still need a new phone build. Native replay artwork is an image-only icon and must not assume a child Text.
