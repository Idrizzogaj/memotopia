# Statistics and The Deck — 28 October 2026

Source/UI update only. No new AAB, iOS export, installation or server deployment. Android 2.1.0 (23) remains the latest packaged build and does not contain this change.

## Statistics

Memory, Challenge and Focus reuse the same `StatisticsCards` section, card, typography, spacing, original game symbols and responsive grid. The original Statistics back/header stays fixed above one vertical scroll viewport. Cards resize with the actual available viewport, not a guessed device size.

Memory shows highest saved level, total stars, levels with awards and three-star levels per game. Pairs/Boxes/Flash come from the existing account cache refreshed through `ProgressSync`; Chain/Says use their existing account-scoped device progress. Level 20 can award lower levels, and Practice can earn ordinary level stars under existing rules: these values are **level awards**, not a count of historical games or timed wins. Missing server progress remains unknown, distinct from a verified empty result. No historical times or attempt counts have been invented.

Practice level/time records remain separate, under Memory and Focus respectively. Focus's existing 90-second records and recent runs retain their meaning. Challenge keeps exactly the approved five fields: Games, Matches, Single, Losses and Draw. These are all-time server counters, not totals guessed from the latest inbox page; old servers without format counters display an em dash for Matches/Single.

The 2.0 release has four Memory games and no Focus section. The 2.1 development track includes Says and Focus.

## Tips

The Deck uses a full-size white illustrated Tips card immediately after The Major System and before Memory Palace. Card height uses the original card's **effective** preferred height: its LayoutElement contains -1 and the original sprite supplies the real height, so copying the raw -1 would collapse a sprite-less card. It reuses the licensed CC0 card faces (including the ace of hearts) rather than introducing downloaded/new art.

The guide has the original back arrow, an illustrated Diamond 5 → D + L → DOLL example, existing Major-code explanation, four expandable suit groups containing all 52 card/word/image associations, and the existing read checkbox/Deck Decoder achievement. Scrollable text/cards reserve the footer area on phone and tablet.

## Verification

`StatisticsDeckValidation.Review` passed in the Unity2022 staging editor: actual level awards, duplicate-level handling, unknown versus empty progress, account isolation, repeated-screen idempotence, separate Practice records, five Challenge counters, legacy API fallback, card widths, Deck effective height/order, real button callbacks, all four suits/all52 cards and read badge presence. `FocusStatisticsValidation.Review` passed through that entry point, covering negative/partial/duplicate/interrupted records.

Phone and iPad native renders were inspected; visual inspection caught and corrected a collapsed Deck list card. Final 2.1 log: `MemotopiaAndroidBuild/ChallengeSprint/statistics-deck-20261028d.log` (exit0). Renders: `MemotopiaAndroidBuild/ChainReuseFix/statistics-{memory,challenges,focus}-{phone,ipad}.png`, `tips-deck-{list,guide,diamonds}-{phone,ipad}.png`. Fixtures are synthetic test accounts, not production records.

Device verification awaits the next user-authorized build. No backend change is required for this UI. The earlier rematch server fix remains a separate, undeployed task while the Mac route is unavailable.
