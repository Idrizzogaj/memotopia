# Live Challenge test release — 1.0.58 / 2026102402

User approved the live feature after the assessment, then added mutual friendship. Latest installation scope: **Hana`s Ipad only** (confirmed by user). iPhone pairing work cancelled by user. No store publication authorised.

## Behaviour
- Favourites remain unilateral shortcuts. Friend requests require explicit acceptance; cross-requests do not auto-accept. Both sending and accepting explain friend visibility first.
- Two independent audiences: public visibility off by default; accepted-friend visibility on by default. Switching off friends also hides from friends when public visibility is on. No friendship grants location, background tracking or last-seen history.
- Foreground authenticated heartbeats every15seconds,45second lease, server-side audience/block filtering. Menu/invitation polling2seconds; live progress polling1second. This is HTTP live synchronisation, not WebSockets or OS push. No production-scale load claim.
- New invitations require yes/no. Decline/expiry/block/withdrawal never settle a game or add a loss/XP; declined total is private and idempotent.
- Blocks prevent contact and presence both directions, remove friendship and pending invitations. Unblock does not restore friendship. Sender5invitations/minute, pair60second cooldown/one pending invitation, friend10requests/hour and pair cooldown. Lists capped500.
- Accepted live games wait for both original game screens before scheduling one server start4seconds ahead. Both players have the same90second deadline. The original games, cues, lead bar and final-result outbox are reused. Asynchronous accepted games keep chooser-first recorded pace.
- Existing result/tie-break rules and0.5draw match points remain. A missing live receipt has60seconds after the deadline to reconnect. If one receipt is missing, the other player wins the challenge by connection forfeit; both missing draw. This is distinct from invitation refusal. Shared start is immutable on retry.
- New clients use invitation protocol2. Old existing results/history remain readable; clients without acceptance support cannot create new invitations through the old endpoints. Both devices need the new app to test the new invitation/live flow.

## Verification
- Backend production TypeScript compilation passed.
- Disposable PostgreSQL test `tools/check-live-social.cjs` passed with fake accounts only: friendships/privacy/favourites/blocks, concurrent decline and result retries, server start/progress, disconnect settlement and cascading deletion.
- Separate disposable authenticated Nest HTTP API test `tools/check-live-http.cjs` passed: real JWT routing, membership, consent, private counts/statistics, block bypass prevention and shared deadline/results. Neither test reads or modifies production users.
- `LiveSocialValidation.Review` passed real UI raycasts for audience controls, friend acceptance and block confirmation; screenshot inspected. Original game stage regression checks for all four games passed; `SprintPlayReview.RunLive` also passed: original Chain presentation waits for the common future start, runs90seconds, displays live opponent lead and submits at the deadline. Full four-game stage/outbox checks passed before this separate live case.
- Backend candidate image `memotopia-backend:live-candidate-20261024` built and tested; production deployment and iPad installation must be confirmed separately below.

## Policy / release boundaries
Local privacy/terms sources now describe actual new flows, audience defaults, retention and decline/block effects. Published pages on one.com still need upload before public release; local file edits do not publish them. Console App Store privacy and Google Data Safety answers have not been independently audited this turn. Age/consent assessment and report-handling operations remain public-release prerequisites. In-app consent explanations are included in this test version. No claim that these source edits alone certify compliance.

## Operational status
- Existing statistics API deployed earlier this turn: `memotopia-backend:ui-draft-20261024`, image starts5fbfcefe3be4.
- New live candidate database/HTTP tests: `/opt/memotopia/backups/live-20261024/` (`candidate-verified.txt`, `http-verified.txt`). New live API deployed and HTTPS/auth verified: image starts `754e4a664b24`; exact ID in `deployed-verified.txt`.
- Mac export path `~/Projects/memotopia/ios-build/Sprint-2026102402`; preserves native Apple sign-in, signing team and privacy manifest. Initial2019 compile caught ambiguous Color/Color32 ternary; explicit cast fixed.
- Local final installer prepared at `~/Projects/memotopia/ios-build/build-install-live-ipad-2026102402.sh`. It verifies version, signature, privacy manifests and that the signing profile includes the confirmed iPad before installing. Xcode completed compilation but framework signing stopped with errSecInternalComponent. User was given a Windows SSH command to unlock the Mac login keychain. Awaiting unlock before retrying final signing/install; installation is not yet confirmed.
- Old six Mac archives are preserved in the verified Windows backup documented in the previous task. Generated1.0.57 export directory was reused for1.0.58 to avoid duplicating2.2GB; old1.0.56 export retained.
