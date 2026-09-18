# Game flow review — 2026-09-17

Target: iOS 1.0.35 (2026091703), before TestFlight. Keep the approved Major System images and achievement UI.

## Findings and changes

- Startup used a separate SplashScene that held login activation for three seconds. The device build now starts at LoginScene; the old scene remains in source for reference. The engine's own splash is a separate setting/license issue, confirmed by Unity: HasPro=False. Unity 2019 Personal requires its engine splash (https://docs.unity3d.com/2019.3/Documentation/Manual/class-PlayerSettingsSplashScreen.html). Removing that requires a supported editor/license change; it is not bypassed by this patch.
- LoadingManager inserted two-second sleeps before some scene loads and before hiding the overlay. These artificial waits are removed. A single persistent, guarded async transition paints feedback first and ignores repeated taps while loading.
- Training selection waited on all three mode requests. Cached arrays could let each response load LevelsScene again. Selection now transitions once when the chosen mode is available; other modes fetch independently. Cached levels can be shown immediately and refresh in the background. Failed first fetch gives a retry message.
- Win/replay buttons each fetched progress before navigating, and replay explicitly unloaded and reloaded the same scene. Next Level now goes directly to the next round, skipping the level-selection scene and its tap. Replay and a separate Levels button remain. Level 20 returns to Levels. Single-mode async loading handles unloading.
- Scores are saved locally before navigation and uploaded by a persistent service. Per-account pending records survive restart, stale responses cannot erase newer wins, and replay preserves best scores. The upload reads the current server record before writing, both to avoid duplicating an uncertain create and to preserve a better score from another device. Network operations have 12-second timeouts and malformed-response handling. Other gameplay API components also survive scene changes.
- Boxes and Flash panel slides used endless coroutines with a speed depending on total app uptime. They now finish in a consistent 0.2 seconds.
- Boxes contact shadows now follow visible pictures: empty recall cells retain the 3D platform without a motif shadow. The shadow returns on placement and disappears while the picture is dragged away. This also handles disabled placeholder images.
- Level cells now look up actual level numbers instead of treating array positions/counts as level IDs. The Pairs/Paris typo is corrected.
- Training Ready/Steady/Go is shortened from three seconds to one, including its animation speed. Challenge countdown remains three seconds. Memorization time, recall time, image exposure and difficulty are unchanged. First-use tutorials remain; a completed/skipped tutorial is remembered per account and mode.
- Achievement network checks no longer hold the result screen for up to three seconds. Locally known new awards still show; late remote awards update the collection without reopening the result flow.
- Runtime canvas content is inset to Screen.safeArea, including dynamically added result/achievement panels. Root backgrounds and loading overlays may remain full screen. This protects the camera/Dynamic Island and home indicator while preserving references and game layout.

The removed fixed waits are facts from source, not claimed device benchmarks. Scene transition durations are now logged in milliseconds for subsequent phone testing. We retain separate game scenes; a full in-place reset rewrite is unnecessary to remove these particular bottlenecks.

## Validation

FlowValidation validates: unfetched vs empty progress, sorting, stale GET protection, concurrent result revision acknowledgements, per-account isolation, pending persistence after reload, best-score retention, safe-area bounds, and renders the actual menu/levels/game/result UI with iPhone 16 insets. Existing achievement and 100-image checks remain in the build pipeline.

The scene review also checks Boxes geometry with a visible picture, a disabled recall placeholder, an invisible dragged picture and a restored picture: only visible pictures generate contact-shadow vertices.

Phone checks after install: launch; select each game; finish a level and use Next Level; replay; use Levels; check level 20; verify top controls and home indicator clearance; check Boxes shadows before/after placement; verify sync after reconnect/restart. Timing comparisons and live challenge behavior still require device testing.
