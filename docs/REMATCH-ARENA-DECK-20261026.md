# Rematches, Arena presence and The Deck

Source changes only. The user explicitly requested no new build. No app installation or server deployment has been performed for these changes. AAB 2.1.0 (22) remains the earlier verified artifact and does not contain these changes.

## Rematches

The API applied a blanket one-minute pair cooldown even when the prior invitation was declined or the game had ended. Replace that restriction with idempotent reuse of a genuinely pending or accepted unfinished battle, while retaining the existing sender limit of five new invitations per minute, membership checks and blocking rules. Retry reuse happens before charging that rate limit.

The client lobby also kept a `starting` flag after its first round start without clearing it. Use `SprintRuntime.Active`, which reflects the actual current round and is cleared on close, as the double-start guard instead.

Local service regression `node tools/check-sprint-rematch.cjs` passed: immediate rematches after decline/completion, reverse invitations, pending and active reuse, and retained sender rate limit. Backend TypeScript typecheck passed. `check-live-social.cjs` now also checks immediate rematches against real PostgreSQL without artificially aging old invitations. That PostgreSQL integration test and deployment are pending: the previous Mac SSH route is unavailable, and the user confirmed the Mac is not available now.

## Arena

Keep the existing player cards, avatar and challenge sword. Reserve a non-overlapping Wi-Fi position between the full name/status label and the sword. Presence remains based only on the server-authorised audience. Reorder current rows on presence refresh, with online players above a thin green divider and offline players below a red divider; preserve order within each group. Dividers are three canvas units high and do not intercept clicks. Retired rows are deactivated immediately when rebuilding search/favourites.

## Tips

The Deck immediately follows The Major System and precedes Memory Palace. Its icon is a fan of three existing CC0 card-face sprites, with the heart ace at the front. No new licensed artwork, network requests or generated raster assets were added. Reopening Tips keeps one Deck card in the correct position.

## Client validation

`RematchArenaValidation.Review` and `LiveRecoveryValidation.Review` passed locally, covering repeat Tips creation/order, three card faces, online-first rows, non-overlapping presence badges, dynamic reordering, obsolete result outbox, give-up/force-close recovery, and prior friend/emoji rules. Phone and iPad render checks are saved locally under `MemotopiaAndroidBuild/ChainReuseFix` as `arena-rematch-phone.png`, `arena-rematch-ipad.png`, and `tips-deck-icon-phone.png`.
