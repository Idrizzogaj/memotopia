# Challenge recovery and give-up

Read-only inspection limited to the reported players confirmed ended accepted games missing receipts. The old cancel path recorded no reason. A subsequent valid result received409, stayed in that phone's outbox and blocked new games. Tests reproduce this direction-dependent symptom; it is not proof that every reported failure has the same cause.

## Behaviour

- Repeat/crossed invitations open the existing pending invitation without auto-acceptance. Pending refusal/withdrawal does not count as a game or loss.
- Confirmed Give up forfeits the entire Single/Match!, records `gave_up` and its player, awards the opponent the result and settles statistics once. Offline give-up intentions are persisted and retried; the user can leave the game view.
- Temporary interruption preserves the common90second deadline and existing60second receipt grace. One missing receipt loses by connection forfeit; both missing draw. A ready player awaiting the other player's start now has the same60second bound rather than an endless wait. This is a practical beta rule, not an empirically optimal threshold.
- Ended games acknowledge late start/progress/result calls without changing the outcome or counting twice. Invalid/unauthorized calls remain rejected; conflicting normally completed receipts remain conflicts.
- The client reads authoritative state before retrying an outbox receipt, clears only a confirmed end/receipt, shows actual server explanations and retains access to Give up during sync failure.
- Live creation uses the observed opponent's presence instead of depending on the sender's locally loaded visibility preferences. The server still enforces mutual visibility and consent.
- Presence badges follow Online/Offline text, including the single-player profile. Already sent disables repeat outgoing requests; backend friend requests were already idempotent. Profile callbacks no longer accumulate stale player closures.

## Verification

TypeScript typecheck, all four original Unity stage/deadline/retry checks and `LiveRecoveryValidation.Review` passed. Disposable PostgreSQL and authenticated HTTP tests passed against `memotopia-backend:flow-candidate-20261025`, including both invitation directions, cancellation, late receipts, pre-start timeout and exactly-once statistics. Tests never modify production users. Deployment/install status must be recorded separately.

## Primary sources

- Unity: https://docs.unity.com/en-us/mps-sdk/session-states — explicit connected/disconnected/deleted lifecycle states.
- Photon: https://doc.photonengine.com/realtime/v5/troubleshooting/analyzing-disconnects — distinguish recoverable interruption, rejoin and terminal failure.
- AWS: https://aws.amazon.com/builders-library/making-retries-safe-with-idempotent-APIs/ — atomic idempotency and stable acknowledgements for retries/late arrivals.

Three independent organisations, all new for this review. They support the recovery structure, not a universal game timeout or win rule. Existing timing was retained; the missing pre-start boundary uses the same grace.

## Native reconnect and reactions

The user specified that gameplay freezes during a connection failure while the real90second deadline continues. The client blocks input and scaled game animation, shows Connecting with the existing large translucent red Wi-Fi graphic, and retries progress with a3second request timeout. It resumes the same scene/state after acknowledgement. Returning at elapsed60seconds leaves30seconds. Being disconnected at the deadline forfeits the entire accepted Single/Match!, with a connection reason. A forced-close ticket persists before gameplay and is reconciled as a forfeit on reopening; a completed result outbox takes precedence. There is no reliable immediate iOS force-kill notification; an absent client is also bounded by the server's existing deadline/receipt grace. Neither temporary backgrounding nor a transient network failure immediately forfeits a game. Explicit Give up ends the whole match. The whole-match consequence is an implementation judgement, not a research finding.

Six fixed reactions: happy, angry, love, brain, clap, thumbs-up. Unmodified Twemoji14.0.2 sprites with CC BY4.0 attribution in the tray and bundled source record. Cosmetic only, no free text. Accepted friends receive during play; other participants receive before/after their timed round. The server enforces membership, blocking, friendship gating and a5second per-sender throttle across matches. The client locks all six buttons for5seconds. One incoming reaction appears at a time, without intercepting taps. Reactions are capped at32per battle, expire for delivery after10minutes and are pruned on the next challenge refresh; no reaction history is shown. The privacy source gains this disclosure; publication is a separate release task.

Verification: `SprintPlayReview.RunLive` passed the30→60second interruption, preserved progress, input freeze, visible Wi-Fi,30seconds remaining and deadline forfeit/time-scale restoration. `LiveRecoveryValidation.Review` passed force-close recovery, all six loaded sprites and friend/stranger display gates. Authenticated disposable HTTP tests passed whitelist, membership, throttling, friendship changes, end delivery and blocking. Physical two-device network-loss testing remains a device-test task.

Deployment: the initial flow image475d841c2eba was verified, then superseded by `memotopia-backend:reactions-candidate-20261025`, image prefix `e9eb32417369`. Both PostgreSQL and authenticated HTTP tests passed against disposable databases. Production HTTPS health/auth checks passed. Rollback is `memotopia-backend:before-reactions-20261025`; verification records are under `/opt/memotopia/backups/reactions-20261025/`. No production player test data was inserted.
