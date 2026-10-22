# Challenge Sprint90 —23October2026

Four games reuse the existing Pairs, Boxes, Flash and Chain study/recall scenes, controls and approved icons. The combined game/tier selector replaces the separate level step. Single game and Match! share the same90second engine.

## Rules
- Intro outside the clock: Neurons on the ready → GO! Countdown cues45s,10s,3/2/1,BRAINS OUT! Study, recall, overlays and round transitions all consume the same90seconds.
- Easy pictures; Medium pictures/words; Hard pictures/words/numbers; Memory Athlete adds cards. Content/layout is deterministic per game seed and round. Training preferences remain unchanged.
- Pairs3,5,7,8,9,… pairs. Others5,7,9,12,15,18,20,20,… items. Compare completed rounds, then correct answers in the current round. Pairs tie: fewer flips. Others: fewer errors, then latest completed round duration, proceeding backwards. Complete equality is a draw.
- Match!: at most3distinct games; owner chooses/sets pace first, recipient chooses/sets pace second, third randomly uses one of the2remaining games with owner setting pace. Win1point, draw0.5each; finish at2points or after3games. No rematch or automatic fourth game. Match points are separate from account XP.
- Opponent pace is a recorded asynchronous timeline, not live presence. Live matchmaking remains deferred by request.

## Reliability
The server stores versioned Sprint battles separately from legacy challenges. Membership, pace order and server start deadlines are enforced. Row locks and idempotent result receipts settle statistics once. Client results are structurally/time validated; this is not a full server replay of every card action. The client keeps an unacknowledged result on disk, retries after reconnection and blocks starting another game until synced. Explicit played flags avoid Unity JsonUtility's null-object behaviour.

## Other fixes included
Chain uses original Pairs NEXT and Flash recall/back/quit controls. Training Chain progression follows original locks, with Athlete requiring cleared20. Athlete gives500XP once per game and no stars. The card selector uses a large A♥. Profile achievements use the stored distinct count. Saved valid sessions renew for365days, preserving tokens during temporary connection failure. Working Apple login is preserved.

## Verification
Unity Play Mode: all4real game scenes complete a round and reset; quit blocks input; deadline stops input; offline result is persisted/retried/acknowledged; menu bell opens the new inbox. Deterministic content and wire-null handling verified. Full Chain, deck/settings/tips, Athlete, profile/session and combined-selector regressions pass.

Backend typecheck and actual pure/service/wire checks pass. The review Docker image passes real PostgreSQL migration, concurrent receipt/row-lock,0.5point draw, chooser, counter,500XP and profile tests in an isolated database. Localhost-only HTTP tests verify authenticated routes,365day renewal and a complete Chain draw. Production was switched only after these checks and a database/source/image backup; HTTPS health and auth guards verified.

Reproduce backend checks from backend/: node tools/check-sprint.cjs; node tools/check-sprint-service.cjs; node tools/check-sprint-wire.cjs; node tools/check-profile-session.cjs; node tools/run-athlete-check.cjs. PostgreSQL/HTTP scripts deliberately refuse any database except the named disposable review database.

## Release
Production backend deployed23October2026; rollback image/source/database on server under sprint-20261023. iOS1.0.55/build2026102301 uses the Mac's Unity2019 exporter and existing signing/privacy fixes. See final release note for signing/install status. No Android/store upload in this change.

## Final release status
Backend deployment and HTTPS/auth checks passed. Unity2019 exports the corrected iOS player successfully, and the full repository's Editor scripts compile after Android-only build-tool guards. Windows Play Mode passes again with a real wrong-drop prefix-collision test for Boxes (1 must not match10), followed by all4stage resets, modal, deadline, outbox retry and menu checks.

The iOS1.0.55/2026102301 Xcode build compiles, but remote codesigning is blocked by macOS errSecInternalComponent. No new app is installed yet. The original iPhone, now named iPhone, is absent from CoreDevice discovery (both its known identifier and current name were tested). A local Mac installer is prepared at ~/Projects/memotopia/ios-build/build-install-sprint-2026102301.sh; it verifies version/signature/privacy files and targets only the original phone. Continue after the user runs it locally or resolves keychain/device access. Do not report installation as completed before verifying the device.
