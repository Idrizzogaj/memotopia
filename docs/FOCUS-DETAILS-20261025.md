# Focus refinement status — 25 October 2026

No app binary was built or installed for this refinement pass. The user is continuing to review changes.

## Focus 2.1

- Flash now has 12 levels. Every level has three parts: green ascending, red descending, then alternating green ascending/red descending. Level N has N+2 targets per colour (3 at level1, 14 at level12). All three parts must be completed before advancing. Existing random circles, hide/recall interaction and exposure durations are retained. Vision targets level12.
- Boxes uses larger red balls, green armed balls with a halo, full-board wall touch zones and constant-speed random bounces within ±45 degrees of the inward wall normal. The three-box layout gives each box one third of the available touch area.
- Nine achievement conditions are implemented; Professor requires starting Pairs at level1. The user approved the second nine-motif design with one change: remove the central star from Vision. The nine approved512px sprites are wired to their achievement keys; editable vector sources are in `docs/design/focus-achievements/`.
- Focus Statistics provides four game records, actual session counts, full-duration records, accuracy and recent sessions. Records are device-local and account-scoped, explicitly labelled “On this device”. Quits cannot replace full90-second best scores; unknown interrupted runs receive no invented result. There is no cross-device Focus statistics sync.
- The actual Statistics screen now scrolls its existing panels and the new Focus section, with its original back button and a fixed Statistics heading above the viewport.
- Says uses the phone-keypad arrangement, clean images in images-only mode, new motifs each level, a rectangular playback frame with easing, and red shake/haptic error feedback. Physical vibration still needs device verification.
- Each Focus game's first START opens a short illustrated page-by-page guide. The final Play starts the90-second session; reading earns no play badge or recorded session. Help reopens it from the selector. Seen state is account-scoped on this device. The guide distinguishes Chain's green-first variant from alternating colours and demonstrates Flash numbers disappearing.
- The Says cue has its own row below the original header, avoiding the five-heart area on iPad.

## Shared 2.0 / 2.1 changes

- The Deck is first in Tips, with the existing list correctly configured for preferred content height. Its Major System explanation and four suit accordions expose all52 associations. The existing Deck Decoder read award is retained.
- Reactions use a smaller bottom strip, a rising/fading received emoji and the existing5-second/friend delivery rules. The Boxes tray is raised above the strip; its horizontal scrollbar is hidden while dragging remains available.
- Boxes reuses its existing board-fitting helper after the lower controls move, keeping all targets above Recall/the answer tray. On shorter tablet viewports, the Challenge pace indicator sits vertically at the right edge, outside the timer header.
- The server whitelist accepts all nine Focus achievement keys. Candidate database checks verified idempotency, invalid-key rejection, profile count and no invented XP reward. Isolated HTTP lifecycle/social tests passed. Deployed image: `sha256:a232f6f0bdb1…`; rollback retained on server.

## Verification

- Editor checks passed for all12 Flash sequences and three-part level advancement; native Focus/Says callbacks; 1,000 approved-angle bounce cases; the reachable Deck entry, four tabs/52rows and one-time read award.
- Focus statistics checks passed for negative records, account isolation, duplicate saves and interrupted runs. Phone/tablet section renders were inspected.
- Highest-density Flash (14green +14red) rendered without overlap on phone/tablet.
- Live recovery checks passed for outbox handling, friend-only in-game reactions, fade animation, Already sent and presence placement.
- Illustrated first-play guides passed all-page phone/tablet captures, first-start gating, no recorded play while reading, final Play once and reopening Help. All nine approved icons load as sprites.
- Four-game reaction play-mode checks passed, including the raised Boxes tray and existing deadline/retry flow. No claim of on-device verification is made.
