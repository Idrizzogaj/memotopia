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

The user completed local signing with the prepared Mac installer. After replacing the USB cable, the original iPhone (CoreDevice 2CB1A144-09A8-5249-908B-1CDDB7054B5B, now named iPhone) was discovered and paired successfully. The existing signed app was installed without rebuilding. Verified before installation: bundle com.retention.memotopia, version1.0.55/build2026102301, strict/deep code signature and both privacy manifests. CoreDevice confirmed the installed version and successfully launched the app. On-device gameplay still requires user acceptance testing; no Android/store upload was performed.

## Follow-up: Challenge selector touch fix
The installed build exposed a full-card TouchTarget that intercepted the new tier buttons. Disable the retired parent Button and any previously created parent hit area so the global touch installer cannot cover the tier choices. A real GraphicRaycaster/pointer-click regression reproduced the failure before the fix (Pairs/Easy routed to the parent) and now passes all16tier buttons with repeated touch-target scans. Both mode buttons are also clicked through the raycaster and verified to swap orange/light-grey backgrounds with black labels. Only Flash's Athlete elephant is tinted black. The updated selector screenshot was visually reviewed. These source changes are not yet built or installed on the phone; its installed version remains1.0.55/2026102301.
