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
Privacy/terms sources describe actual new flows, audience defaults, retention and decline/block effects. Both pages were published via one.com on 24 October 2026 after backing up the previous public files. Independent HTTPS responses matched the local content (privacy SHA256 `1f6e051ac5a5cd0ed09b5068b16c29776456464a8b61df9b0ccf1ef3c09d9de5`; terms `c012fbfd4e945fa1714e00bca372b625196b93f49f11be28b4639bdb8227038e`). Console App Store privacy and Google Data Safety answers have not been independently audited this turn. Age/consent assessment and report-handling operations remain public-release prerequisites. In-app consent explanations are included in this test version. No claim that policy publication alone certifies compliance.

## Operational status
- Existing statistics API deployed earlier this turn: `memotopia-backend:ui-draft-20261024`, image starts5fbfcefe3be4.
- New live candidate database/HTTP tests: `/opt/memotopia/backups/live-20261024/` (`candidate-verified.txt`, `http-verified.txt`). New live API deployed and HTTPS/auth verified: image starts `754e4a664b24`; exact ID in `deployed-verified.txt`.
- Mac export path `~/Projects/memotopia/ios-build/Sprint-2026102402`; preserves native Apple sign-in, signing team and privacy manifest. Initial2019 compile caught ambiguous Color/Color32 ternary; explicit cast fixed.
- Local final installer prepared at `~/Projects/memotopia/ios-build/build-install-live-ipad-2026102402.sh`. It verifies version, signature, privacy manifests and that the signing profile includes the confirmed iPad before installing. Final Xcode build and deep/strict signature verification passed. Unlocking alone was insufficient; user ran the prepared interactive helper granting Apple signing-tool access only to the matched app signing-key label (no credentials stored). User supplied the successful CoreDevice App installed receipt for com.retention.memotopia on the confirmed iPad. The subsequent apps-list verification failed with CoreDevice StreamingAction error3; a separate launch then succeeded with exit0. Installed and launched on Hana`s Ipad; iPhone untouched.
- Old six Mac archives are preserved in the verified Windows backup documented in the previous task. Generated1.0.57 export directory was reused for1.0.58 to avoid duplicating2.2GB; old1.0.56 export retained.

Earlier code pushes: c6109ce plus entry-point UI test c6aa595 on codex/challenge-sprint-90. Additional Challenge home Online entry raycast passed. Public website publication is now verified; store declarations remain the pre-release work listed above.

## Current test revision (1.0.60 / 2026102404)
- Live intro waits for the opponent before the ready cue, preserving the common server deadline.
- Challenge scene managers are frozen before clearing the session, preventing a training 180-second timer flash while the menu loads.
- Online/offline labels retain their text and gain green/red Wi-Fi-shaped game-presence badges. Hidden presence remains hidden; no new network or personal data is collected.
- Boxes fits the original board between the original header and Recall button when the viewport is shorter. Tablet/phone bounds checks passed; both tablet scene renders inspected.
- FOCUS is entirely hidden for the 2.0 release track. See RELEASE-TRACKS.md for the separate 2.1 branch plan.
- Validation: `BoxesTabletValidation.Review` passed tablet/phone geometry checks without moving Recall. `LiveSocialValidation.Review` passed actual control raycasts with green/red presence badges. `ChainUiValidation.Validate` passed with no Focus section or placeholders, and phone/tablet renders inspected. `SprintPlayReview.Run` passed all four original game stages, frozen exit timers, deadline/result retry and menu flow. The exit check additionally caught a Flash animation callback after session disposal; deactivating the original stage before clearing session data fixed it.
- `SprintPlayReview.RunLive` also passed: Waiting for opponent precedes the ready cue, originals remain gated until the common start, the live session runs90seconds, lead feedback updates and the deadline receipt returns to the menu.
- Fresh Mac discovery identified the authorized iPhone and Hana`s Ipad as available/paired over the network. Build/install verification is recorded separately; discovery is not installation.
- Native Unity2019 export and unsigned Xcode compilation of the corrected1.0.60/2026102404 build passed. Mac runtime sources were hashed against verified commit672df33. Both authorized device IDs are included in the provisioning profile. Final signing failed with `errSecInternalComponent` while the keychain was unavailable; user-run helper `~/Projects/memotopia/ios-build/sign-install-current-2026102404.sh` performs the scoped keychain unlock, signing, signature/privacy checks and installation on both devices. Do not describe this build as installed until the two installation receipts are confirmed.
