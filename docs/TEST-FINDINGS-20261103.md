# Test findings from Idriz, 2026-11-03 (not yet implemented)

Status: listed only. Nothing below is coded. Creator page ("Meet the creator") is coded but uncompiled (see CreatorPage.cs).

## Feedback on wrong answers (all Memory games, Training)
- Wrong tap: iPhone vibration AND/OR shake of the picture or red frame. Must be unmistakable.
- Focus: clear but must not disturb play; vibration only.

## Boxes (big box)
- Tapping the correct wall reverses the ball direction (faster progress).
- +1 point for a correct tap, -1 for a wrong one; at -10 the player loses ("Lost focus" or similar).
- Applies to the big box with 1, 2 and 3 balls, NOT to 2 boxes and beyond.

## Pairs / Flash
- Pairs OK as is (needs real testers). Flash very hard, OK for now.
- Flash: move value and number closer together (halve the distance).

## Chain
- Larger squares for 1-6 and 1-12 (up to roughly the 1-15 look).
- From level 7: larger squares AND mixed; green and red mixed and evenly spread (like the 1-15 view).
- With the deck: cards look stretched/too wide. Remove the inner thin black line OR shape the card to fit the square; redo the square so it fits.
- Level select + READY/STEADY/GO: wrong background, screen split in two colours.

## Achievements / badges
- Deck Decoder: Ace of Spades instead of 5 of Diamonds.
- Pair of Stars: parts fall outside the badge.
- New icons: Memory Athlete Boxes (runner stepping out of a box, box around her whole body); Flash (runner with lightning, strikes at the feet); Chain (muscular athlete tearing a chain around the hands).

## Tips: The Deck
- Main button: some cards (e.g. four aces) as the picture. Position between Major System and Memory Palace (already placed).
- Examples of how images are coded; tapping a rank shows one card (Ace of Hearts), its picture and how it connects with the letters.

## BUG: Deck + "?" in Training menu
- Many identical pictures: Says shows identical cards (text differs); Pairs identical pairs; Boxes only 2 different pictures out of 10-12. Only for those menu choices.
- With "?" (cards only) some cards show a 3-digit explanation; wrong, show only how the card decodes.

## Says
- Move rank and number closer; thin light-grey frame around the card; during play a thicker black frame around the card to memorise.
- Level select: stars not filled (verify whether he simply has none).
- Wrong "Challenge" label under the SAYS heading above the Celeste level.
- Level titles: Pairs/Boxes/Flash use bold text in the background colour; Chain and Says do not (match them).

## Coded 2026-11-03 (NOT compiled, NOT tested in Unity, not committed)
- Deck + "?" bug: DeckImage copied the whole (possibly shared) texture instead of the sprite's own rectangle (CardFaces.cs). Decode text no longer shows the extra code (DeckCatalog.Decode).
- Wrong-answer feedback: new WrongFeedback.cs (vibration + shake + red flash) in Pairs, Flash, Boxes; Focus uses vibration only (FocusGame.Award).
- Focus Boxes: single big box reverses the ball on a correct tap; score <= -10 ends the run with "LOST FOCUS".
- Focus Chain: one evenly filled, larger grid; green and red shuffled together only on levels that use both colours (7+).
- Still open from the list: Chain deck cards/background colour, Says/Flash spacing and frames, level-select titles and stars, Pair of Stars/Deck Decoder icons, Deck tips guide, three Athlete icons. Memory Chain (deck card shape) is a different screen from Focus Chain.
- Also coded (uncompiled): Flash/Says value-number spacing halved (CardFaces.WideFace, shared by both), Deck Decoder badge = ace of spades (ExtraAchievements), Memory Chain study card keeps card proportions (ChainGame.NextCard).
- Pair of Stars badge: star and cards scaled to 80 % and centred inside the hexagon (original kept as %TEMP%\pairs-star.orig.png; also in git history). Check the faint hexagon halo at the lower edge on device.
- Still open: Chain/Says level-select colours and READY/STEADY/GO background, Says frames and stars, Deck guide in Tips, three Athlete icons, Memory Chain inner card line.
- Also coded (uncompiled): Chain and Says level-select titles now bold white word on the purple banner (as Pairs/Boxes/Flash); Says hides a stray "Challenge" template label (guess: could not see the screen); Says keys get a thin light-grey rest frame and a thicker black frame while shown (SaysGame.cs). Says stars are only set after a win (data-driven, not a bug).
- Not done: Chain "READY STEADY GO"/level-select split background colour (needs a look at the screen), Deck guide in Tips, three Athlete icons, Memory Chain inner card line.
- Also coded (uncompiled): Tips The Deck main button shows all four aces (TipsEnhancer.DeckTipArt; two validators now expect four faces); Chain READY/STEADY/GO uses the countdown panel colour behind it (ChainGame.Countdown, a guess without seeing the screen).
- Not done: three Memory Athlete icons (athlete-one-heart-boxes/-flash/-chain, new art needs his OK on the style), Memory Chain inner card line, Chain level-select background if still wrong.
- DRAFT icons added (need his OK on style): athlete-one-heart-boxes/-flash/-chain.png in Resources/Achievements/Extra (runner leaving a box, runner with lightning striking at the feet, strongman tearing a chain). Simple flat figures in the badge palette, drawn with a script; to be replaced if he wants a finer illustration.

## Compile check 2026-11-03 (evening)
The 27 changed files above were copied to the Windows staging project (MemotopiaAndroidBuild\Project) and Unity 2022.3.62f2 ran in batchmode (-quit, no build): 0 "error CS", "Exiting batchmode successfully". Not tested on a device; Unity 2019.3 (Mac/iOS) compile still to be confirmed. Log: MemotopiaAndroidBuild\compile-check-20261103.log.
