# Achievements repair — 2026-09-17

The menu and achievement popup referred to LiberationSans SDF GUID 8f586378b4e144a9851e7b34d9b748ee, but its assets were excluded from Git and missing. Restored the essential resources from the installed TMP 2.0.1 package and removed that ignore rule.

The repair also covers async menu refresh, readable locked/unlocked descriptions, real catalog progress, fallback rows for legacy keys, unique unlocks, per-account persisted pending awards, retry on menu refresh and harmless handling of server 409. Eligibility uses distinct completed levels and inclusive XP/win thresholds. Cached progression is reconciled with the menu. Gameplay waits at most three seconds for remote statistics/leaderboard before continuing to the result screen. Late callbacks cannot modify a later popup sequence. Both OK and X advance the queue.

Backend duplicate achievement inserts no longer abort a challenge or the remainder of a batch. That server change is local only; the iOS client also accepts the existing server's already-owned response.

Validation commands:
- Unity batch executeMethod AchievementValidation.Review (rules, per-account persistence/retry, real menu/popup rendering; output review/).
- backend: node node_modules/jest/bin/jest.js --config jest.config.js --runInBand --watchman=false achievements.unit.ts (3 passed).
- backend: node node_modules/typescript/bin/tsc --noEmit -p tsconfig.build.json (passed).

Pending phone checks: open Achievements, inspect locked/unlocked descriptions, finish a previously unplayed mode and close its award with OK/X, reopen menu, and repeat a completed level without duplicate awards. Live network/offline retry and challenge completion require device testing. No production records were modified by automated tests.

## Installed build

iOS 1.0.34 (2026091702) built successfully in Unity/Xcode and installed wirelessly on ZogajSon. Device inventory confirms the version. Automated launch was denied because the phone was locked; on-device startup/gameplay verification remains pending. No TestFlight or GitHub upload.

After the user unlocked ZogajSon, devicectl launched the app successfully and confirmed its process was still running approximately 30 seconds later (PID 12872). Manual on-device achievement/gameplay testing remains pending.
