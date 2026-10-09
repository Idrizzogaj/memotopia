# Memotopia – combined session handoff (2026-10-07)

Merged from two Claude chats ("Tillbaka" = main, with server/Mac access, and "Tillbaka (fork)" = client/Android). Nothing is committed. Read this first, then `AI_HANDOFF.md`.

## Ground rules (from Idriz)
- Work only locally, never in OneDrive, only in the Memotopia/Memo Gym git repos. **Never write anything in vnv-tools**, even if a session starts there.
- No commit/push without his explicit "ja". No APK, only AAB. Never print secrets.
- Economical: batch fixes, **one build per test round**, short logs. "Always do it right": check store requirements, re-read own edits, verify artifacts before handing them over.
- Max 3 steps per message to him, Swedish, terse. Android first, then iOS. Do not build while he is testing unless he asks.

## Where things run
- **Repo:** `C:\Users\IdrZog_MEM\GitProjects\memotopia` (branch `work/ios`, many uncommitted changes from both chats).
- **Android build copy:** `C:\Users\IdrZog_MEM\MemotopiaAndroidBuild` (Unity 2022.3.62f2). Build: `.\Build-AndroidAab-local.ps1 -VersionCode N` (script may print a false "Unity failed exit 1": check `Output\Memotopia-N.aab` + `.build-summary.txt`). **Next code: 11+.** Signing material stays in `C:\Users\IdrZog_MEM\.android-signing\memotopia` (never shown).
- **Mac (iOS builds, Unity 2019.3.12f1, Xcode):** `idrizzogaj@192.168.1.209`, repo `~/Projects/memotopia`. Keychain is locked over ssh, so **codesign/install must be run by Idriz in his own Terminal**: `bash ~/Desktop/Memotopia/build_and_install.sh`. Last export on the Mac: **iOS 1.0.47 (build 2026100702)**, 0 compile errors, validators passed (now older than the latest fixes). Finished builds go in `~/Desktop/Memotopia`.
- **Server:** Hetzner `root@46.225.130.128`, `/opt/memotopia` (docker compose: postgres, backend, caddy), reached only through the Mac with Idriz's explicit OK; permission rules are in his user `~/.claude/settings.json`. Mail via Brevo (MAIL_* in `/opt/memotopia/backend/.env.app`). Backups: `/opt/memotopia/backups` (db-20261006-230601, db-pre-challenge-20261007-160052, source copies; a 20-byte broken dump from 2026-10-06 21:01 is to be ignored). Deploy = tar changed files to `/opt/memotopia/backend`, `docker compose up -d --build backend`. 20 real users.

## Deployed to production (verified)
- E-mail verification (flag `emailVerificationPending` default false, SHA-256 one-time token, 24 h, resend by username/e-mail), in-memory rate limits (trust proxy 1), account deletion `/users/delete-account`, search SQL-injection fix, reset link from `PUBLIC_BASE_URL`, no account-existence leak on forgot-password, SMTP creds moved to env.
- Challenge result rule: most correct first, then least time (`participant.progress`, default -1), 13 new achievement keys (memory-athlete + tip-*).
- Tests: 25 unit + 6 DB-integration (throwaway Postgres in Docker, `account-security.db.int.ts`).

## Builds
- Android: 6 (minSdk 23, rejected by Play), 7 (1.0.44), 8 (1.0.45), 9 (1.0.46), **10 (1.0.47, current, verified)**. Idriz uploads himself; unknown which he uploaded.
- iOS: 1.0.40 installed on ZogajSon earlier; 1.0.47 exported only (not signed/installed; the phone must be on the Mac's network and unlocked).

## Client features added this round (all in repo and Android project)
Delete account panel; Challenge freeze fix (`GoToChallenge` guard + spinner, `FailureFrom`); bell hit area; `TouchTarget` on iOS; achievement counter (AllKeys 12 + ExtraAchievements 13 = 25) and scrollable list; Invite friends (`NativeShare`, `MemotopiaShare.mm`); Memory Athlete (level 21) + card faces (Images/Help/Words/Numbers/Random) + selector bar; tips with badges and the free Major System tip (last of the open tips); analytics OFF / crash reporting ON; Android minSdk 24.

## Open items (Idriz's latest list; fork is working on these, no build until he asks)
a) Training bar covers the back arrow: move below it, move Pairs/Boxes/Flash down and closer. b) Make the chips more Memotopia-style. c) Boxes Memory Athlete: round must end at time 0 and after 3 lost hearts. d) Boxes: tray items must not swap places when dragged. e) Remove the separate Memory Athlete button: only level 20 stays open per game; **clearing level 20 unlocks Memory Athlete + badge**. f) Major System page: title "The Major System", one number per line under "Easy ways to remember them", more pictures. g) Android: "Invite friends" not visible (the scene's FacebookShare button may be hidden on Android: check `SideMenuAvailability`). h) Major System tip last of the open ones (done in code, not on the Mac yet).
Risks: Boxes swap bug is in `DragAndDropItem/Cell`; Training layout was set from scene YAML, not seen on a screen; chips untested on device; text sprites cost ~100-300 ms per round.

## Not done / decisions pending
- Privacy policy: `docs/POLICY_REVIEW_2026-10-06.md`, draft `docs/legal/PRIVACY_POLICY_DRAFT.md` with `[[ ]]` placeholders (org no., address spelling, retention), `docs/web/delete-account.html` draft: **nothing published**. Play needs the policy and a working reviewer login.
- At go-live: wipe test accounts (explicit yes needed), 30-day log retention, own backups (nightly encrypted pg_dump + weekly copy to kDrive; decided, not set up), delete the 2026-10-06 manual dumps around 2026-11-05. Plans: `docs/SERVER_RESET_AND_LOG_RETENTION.md`, `docs/PUSH_NOTIFICATIONS_PLAN.md`.
- Invite text links to memotopia.com until public store listings exist.
- Live challenge/chat skipped for now; emojis at game start later. Ideas to port from Brain Fight (memory, concentration, math) later. Payment: client sets payment status, no server-side receipt validation; purchase on iOS was slow in sandbox (unresolved).
- Security leftovers: old one.com mail password was in git history (Idriz changed it); Apple token revocation on account deletion not implemented.

## Merge note
The second chat (session f4b79332) is still working through the open items. Keep **this document** as the shared truth and update it when the list above changes; close the fork chat only after those items are done or taken over.

## Update 2026-10-08 (late)
- Server updated and live (backup `db-pre-xp-20261008-171147`): XP without cap (level every 100 XP), +200 XP first clear of level 20, 10 XP per extra star, no double pay, replay never lowers stars, 6 new star badge keys. Package: tools in scratchpad only; changed files `level-rules.ts`, `level.service.ts`, `user-statistics.service.ts`, `achievements.constants.ts`.
- Client (repo + Android copy): purchases unlock from the store confirmation; level-based star rules (`StarRules.cs`); first clear of level 20 fills levels 1-19 with the same stars; 6 star badges; Memory Athlete unlocks from saved level 20; Tips scroll gate, badge popup, bigger touch areas; terms link `https://memotopia.com/terms.html`; Major System sound table.
- Builds: Android Memotopia-13.aab (1.0.49, code 13, before the tip/star/athlete changes); iOS 1.0.49 (2026100805) exported on the Mac, signed and installed by Idriz with `bash ~/Desktop/Memotopia/build_and_install.sh` (iPhone ZogajSon + Hana's iPad).
- Rule: ask "Ska jag bygga?" before any build; fix code for both platforms always.
