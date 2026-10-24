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
