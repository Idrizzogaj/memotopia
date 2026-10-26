# Says recall and stars, 26 October 2026

Update: included in [Android2.1.0(23)](ANDROID-2.1.0-23.md), built and verified after the user authorized the Android build. No new iOS build or device installation.

Status: source and editor validation only, no new build or installation. Applies to the 2.1 Focus/Says branch.

- After playback, all ten pictures are covered for0.9seconds with the existing Memotopia logo and RECALL. Input is blocked during this cue. The pictures then return and the180second recall countdown starts. The existing keypad, playback animation and wrong-answer feedback are unchanged.
- Practice remains untimed, with its elapsed clock continuing through playback and recall. Challenge timing is not extended by the cue.
- Removed the Challenge prototype button from the Says level menu.
- Level stars were already saved, but the menu did not assign the gold star sprites used by Chain/Flash. It now reuses those exact sprites. If an older local completion has no saved star rating, it displays the minimum one star; it never invents a perfect performance. Existing saved ratings remain intact.

`SaysNativeValidation.Review` passed: playback/input gating, hidden pictures, cue timing, countdown starting after cue, wrong-answer shake, level1 two-star and level2 three-star results, persistence after reopening, legacy completion fallback, correct star artwork, absence of Challenge button and new pictures on the next level. Native screenshots of the cue and earned stars were inspected.

`PracticeValidation.Review` also passed, including all four untimed Focus games, the three original Memory timers, Chain/Says, no practice achievements and normal timed Focus.

Re-run the combined editor review with `SaysNativeValidation.ReviewWithSession`. First run log: `MemotopiaAndroidBuild/ChallengeSprint/session-says-review.log`, exit0; the entry point was subsequently moved out of the shared startup test so the 2.0 branch does not depend on Says or Focus.
