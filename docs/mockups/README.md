# Focus 2.1 review prototype

`focus-2.1.0.html` is the self-contained interactive conversation fragment. It has four local activities, a90second clock, selectable starting levels, scoring, quit confirmation and results. It does not call APIs, save accounts, change the native app or enable Focus in the2.0 release track. Gameplay interpretation and provisional choices are recorded in `../FOCUS-2.1-SPEC.md`.

The existing Montserrat fonts and four approved game symbols are embedded. `FocusMockupAssets.Export` renders the symbols directly using the existing `TrainingGameSymbol.Draw` and `TrainingFlashSymbol` in the Unity editor. The back arrow is the actual sprite referenced by `ExactFlashChainRecall` (`GamesResources/Flash/Group 11072@3x.png`). The Montserrat license is included alongside this file.

Browser review on24October2026:
- Pairs: wrong answer retains the question; correct answer increments score and advances to the next question.
- Boxes: a correct pre-emptive wall tap earns1before impact; subsequent uncovered bounces subtract1each. The same covered impact does not score twice.
- Flash: green1then red3 at mixed level9 earns2; five consecutive wrong clicks drop from level9to8.
- Chain: green1then red6 at mixed level10 earns2.
- Quit confirmation and return to the Focus menu work; host state echoes do not reset an active result screen or reopen a closed game.
- A real90second run ended at0with BRAINS OUT!, the correct1point/1error result and Home/Replay controls; the result remained visible after host-state acknowledgement.
- At320pixels wide, the30Flash targets at level14 are42×44pixels with zero overlapping targets. Full-page screenshots of all four games were inspected. No browser console errors were observed in the final revision.

This is a browser mockup only. No iOS or Android build, signing, installation or store publication was performed for it.
