# Memotopia Saga

Work branch: `codex/saga`, based on the fetched `origin/codex/focus-2.1.0`.

## Approved scope (2026-11-23)

- Add Memotopia Saga above Training and Challenge on the home screen.
- White map background, turquoise neuron cell bodies with purple numbered nuclei, axons with segmented gold myelin. Follow Idriz's attached route and approved image concept.
- All stops are selectable during development. Ordered unlocking comes later.
- Start with Chain; Flash and Says follow later.
- Use the existing Chain presentation, input, sequence rules, countdown, quit confirmation, and result prefabs. Saga changes image count, phase times, hearts, and recall placement only.

| Levels | Pictures | Seconds for each phase | Timed recall shuffles |
| --- | --- | --- | --- |
| 1-5 | 10 | 90, 88, 86, 84, 80 | 0 |
| 6-10 | 15 | 80, 78, 76, 74, 72 | 1 |
| 11-15 | 20 | 70, 68, 66, 64, 62 | 2 |
| 16-20 | 25 | 60, 58, 56, 54, 52 | Up to 3, then the last-ten rule |

- All rounds start with three hearts. Each wrong answer costs one heart. Zero hearts or recall timeout loses; full sequence completion wins.
- Memorization starts after the existing READY/STEADY/GO sequence. NEXT works as before. Expiry starts recall with its own complete time budget.
- Regular recall shuffles are evenly spaced through the time budget: halfway, thirds, or quarters.
- On levels 16-20, once ten pictures remain, pending timed shuffles are skipped. Placements then shuffle after every correct answer (including the answer that leaves ten). Wrong answers do not shuffle. A fast player need not encounter all three timed shuffles.
- Shuffles move the actual existing cells to different slots without changing the answer sequence, image identity, callbacks, hearts, timer, or which answers are already cleared.

## Implementation

- `SagaPage.Attach` runs after the existing menu startup and adds the tile once.
- Statistics, Training, Challenge, Videos, Tips, title and side-menu controls are all retained. The three original tall rows get smaller bounds to fit the extra Saga row above Statistics. Existing artwork and callbacks are reused unchanged.
- `SagaMapGraphic` draws axons and myelin; the new `Resources/Saga/Neuron` sprite is reused for all numbered stops. The generated asset is in the repository, not referenced from a temporary path.
- The page uses the existing safe-area container, supports scrolling, and closes with Back or Escape.
- `ChainGame.OpenSaga` configures the same Chain engine and original views. Saga always uses pictures; Training content settings and Practice preference are left intact.
- `SagaLevel` defines all configurations and shuffle timing. `SagaProgress` stores best stars (remaining hearts, 1-3) under `saga.chain.stars.v1.<owner>.<level>`. Wins, replay, NEXT LEVEL, and return-to-map are wired; level 20 returns to the map instead of offering 21.
- Saga does not write Training progress, Practice records, XP or Memory Athlete badges. Account changes abort active rounds. No server calls or new achievement keys are introduced.
- Progress is **device-local**, not server-synced. Access/payment and XP/badge policy still need a decision before release. Development deliberately keeps all levels open through `SagaProgress.DevelopmentAllLevelsOpen`.

## Validation and next work

- Source compilation passed with Unity 2022 references and with legacy preprocessor paths/C# 7.3. This does not replace compilation against the actual Unity 2019 iOS project.
- `SagaValidation.Validate` is an editor-only integration suite guarded to run in a separate project named `MemotopiaSagaValidation`. It exercises real menu, map, Chain and result callbacks with synthetic accounts; captures phone/tablet layouts. Validation state stays in the isolated test product's preferences.
- Validation project uses Unity 2022.3.62f2, copied assets and packages, and empty receipt-key test stubs. The original repo Unity version, Android staging project, signing configuration and Mac are unchanged.
- Final editor integration run passed (`SAGA_VALIDATION_PASSED`, process exit 0). Phone and tablet captures were inspected, including the visible tablet study timer and the actual axon/myelin render. A render assertion also checks that myelin is present.
- Isolated test project: `C:/Users/IdrZog_MEM/SagaReview-20261123`. Review PNGs are outside Git under the current Codex visualization directory's `saga-validation/review/`. No APK/AAB/IPA or store build was produced.
- Ask Idriz "Ska jag bygga?" before each build. No store builds or publication.
- Verify layout and navigation on phone and tablet before treating the UI as approved.
- Confirm final release unlock requirements, access, rewards and server sync separately. Do not treat the development unlock override as release-ready policy.
- Keep Unity 2019.3.12f1 and Unity 2022.3.62f2 compatibility. Do not change the separate Android staging project without coordinating its files.

## Flash preparation

- Idriz requested Flash with the same neuron artwork, another game colour, and the same Saga difficulty logic.
- Map order is not decided: Chain 1-20 followed by a sliding transition to Flash 1-20; separate game maps; or alternating Chain 1, Flash 1, Chain 2, Flash 2, etc.
- `SagaGame` and `SagaLevel(game, number)` share the existing count/time/heart/shuffle configuration without conflating each game's identity. Existing `SagaLevel(number)` calls still mean Chain.
- `SagaProgress` now has game-scoped overloads. Existing `saga.chain.stars.v1` keys are preserved exactly; Flash uses `saga.flash.stars.v1`. This is preparation only: no Flash map navigation or playable Saga Flash has been wired yet.
- Two questions have been presented to Idriz: which initial map order to use, and whether Flash should spread its automatic presentation across the entire memorization budget or retain its current animation speed. Do not silently replace Flash with Chain's manual NEXT study screen.
