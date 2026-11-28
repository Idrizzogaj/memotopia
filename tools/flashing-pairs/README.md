# Flashing Pairs draft

This is a prepared Training-only experiment, not an installed game or a Saga stage.
Idriz approved20levels plus a Memory Athlete view,2pairs/3study seconds on level1,
one additional pair and study second per level, and60seconds recall. No statistics,
XP, achievements, server identifiers or purchases are added.

Before exposing this game, obtain the pending identity-rule answer: two different
associated pictures, or two identical pictures whose positions are memorized.
The installer requires an explicit `--pair-rule associated` or
`--pair-rule identical`; it has no default. Run it from the repository root only
after that answer. It generates the controller and adds one tile to Training.
It deliberately never changes SagaRoute or SagaGame.

The shared native board's study/recall/input/mismatch behavior and4/42card fit are
covered by FlashingPairsBoardValidation in the isolated Unity validation project.
The complete controller, timers, levels, Athlete eligibility, owner isolation and
Training tile must still be compiled, played and visually checked after choosing
the identity rule. Do not claim the full game is implemented or tested yet.
