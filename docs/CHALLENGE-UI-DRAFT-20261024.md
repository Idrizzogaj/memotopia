# Challenge UI draft — 24 October 2026

Implements the approved visual changes; no player build, store upload or server deployment is part of this draft.

- One Challenges inbox with separate, independently scrollable Challenges and History sections. Older pending challenges and results remain available through the same sections. Pending historical results are not labelled as draws.
- Rectangular result cards with small rounded outer corners. Closed cards show both contestants and Waiting, Draw or the winner. The dropdown expands facts inside that card: per-leg results, rounds, correct answers including completed rounds, errors, Pairs flips and recorded round times. Green wins reuse the profile trophy; red losses reuse the existing sword.
- Match game choices and replay entry points reuse Memory tile content, icons, font and per-game colours. Generic orange NEXT-shaped lobby buttons are removed. Static visible button labels are short (Challenges, Single, Match!, Retry); no redundant footer navigation.
- Reused back arrow is always active; detail returns to inbox, inbox to the Challenge home. Menu parent activation is restored when returning after a game. Standard UI materials keep dropdown arrows clipped within the appropriate scroll viewport.
- Statistics retains its original panel and uses Games / Matches / Single / Losses / Draw with white numbers and labels. Games counts settled duels, Matches counts settled Match! duels, Single includes settled individual and historical challenges. Existing forfeit rules are unchanged.

## API contract and release dependency

GET /user-statistics adds challengeFormatsAvailable, numberOfMatchChallenges and numberOfSingleChallenges. Match count uses all settled Sprint rows rather than the inbox's latest100; total/loss/draw counters and format count are read in the same SQL statement for a consistent snapshot. No schema migration or counter mutation is needed. Existing clients remain compatible. New clients show an em dash for format counts while connected to an older API, rather than inventing zeroes.

The server change was subsequently deployed for the user-authorised iOS1.0.57 test build: memotopia-backend:ui-draft-20261024 (image sha256:5fbfcefe3be4…). Exact SQL passed against an isolated disposable PostgreSQL database with fake records (both player roles, empty accounts and205matches beyond the inbox limit); the database was removed afterward. Docker production build, service format checks and HTTPS health/JWT guards passed. Source/image rollback is retained under /opt/memotopia/backups/ui-draft-20261024. Android internal-test AAB21 is unchanged.

## Checks

- SprintLobbyValidation.Review: actual GraphicRaycaster taps for expanding/collapsing cards and back navigation; independent scrolling and retained positions; no retired view layers; correct shared game colours/icons; reused trophy; five white Statistics fields; safe old-API placeholders. Actual Canvas preview captures were visually reviewed.
- ChallengeCombinedValidation.Review: original game/tier selector and Single/Match controls.
- Backend production typecheck: node node_modules/typescript/bin/tsc -p tsconfig.build.json --noEmit.
- Backend format-count checks: node tools/check-challenge-format-statistics.cjs (empty accounts, historical singles, all-time counts beyond100, both participant roles, unchanged loss/draw counters). This is a mocked repository check; no live database was altered.
- The broader root tsconfig still includes the pre-existing implicit-any error in athlete-reward.unit.ts:19; the production build config already excludes unit files. That unrelated test was left unchanged.

Preview fixtures are illustrative test data, not production account statistics. Preview PNGs are under C:/Users/IdrZog_MEM/MemotopiaAndroidBuild/ChainReuseFix/: challenge-lobby-collapsed.png, challenge-lobby-expanded.png, challenge-match-games.png and challenge-statistics.png.
