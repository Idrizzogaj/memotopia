# Release tracks

Keep `codex/challenge-sprint-90` for the2.0.0 release. It contains revised Challenge, live play, social privacy, Give up, reconnect recovery and fixed emoji reactions. FOCUS and its placeholder tiles remain entirely hidden. Current device builds are test versions until release approval.

`codex/focus-2.1.0` also contains all shared release fixes, plus four native Unity Focus games from the handwritten PDF: arithmetic Pairs, wall-anticipation Boxes, scattered-number Flash and number-row Chain. Native Focus is enabled only here. The browser prototype is a historical design reference, not the app implementation. Native test build2.1.0/2026102501 targets the user's iPhone and Hana's iPad. Export and native compilation/install status are tracked separately in the build record. Focus has no account XP, achievements or server statistics yet; provisional interpretations are recorded in FOCUS-2.1-SPEC.md for user testing.

The common recovery/reaction change is `ec48cb1`; cherry-pick it into2.0 rather than merging native Focus into the release branch. Presence after Online/Offline, the individual-player presence badge and Already sent protection were implemented in `1c31037` and are shared.

Privacy and terms were published on memotopia.com on24October2026. The new fixed-reaction disclosure in docs/web/site/privacy.html has not yet been published; publish/reverify that update before the public release. App Store privacy / Google Data Safety declarations and age-consent assessment still require release review. No store publication is part of this device test.
