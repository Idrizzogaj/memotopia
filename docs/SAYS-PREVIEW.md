# Says — native design preview

This is a playable2.1prototype, not a fifth network Challenge game. The existing multiplayer protocol and Match! selection remain four games so older2.0 clients cannot receive an unsupported game. The local Challenge button previews90seconds with5,7,9,11,13,15… and reports fully completed rounds plus correct inputs in the unfinished round and errors. It makes no claim to have compared two players.

The ten fixed keys are available from the first level. They use CardFaces and the normal Training content selection; Number faces are specifically single digits0–9, independent of the two/three-digit preference. Images, words, mixed content and selected deck presentation reuse the normal faces. Keys retain their positions and face within a round; repeated sequence entries are valid. Adjacent duplicate entries are avoided, as in the user's Zogaj says implementation. Playback uses that implementation's2second highlight with0.25second neutral gap and a short initial wait. Input is disabled until playback ends. Correct keys remain available for later repeated entries.

Exact existing assets/helpers: ExactFlashChainLevels, its stars/locked hexagons, TrainingAthleteTile, the elephant logo, ChainOriginalPresentation's Flash grid/back/header, MidGameQuitPanel, WinPanel/LosePanel and GamesScript.ConfigureWinButtons. No new artwork. Memory progress/stars are account-scoped prototype preferences, separate from Chain. Level1 and20 can be entered directly; other levels unlock consecutively or after completing20. The elephant level requires completed20 and has one heart. Prototype progress does not award server XP/achievements.

## Level interpretation requiring review

Starting with one entry, the written increments would produce39at20, in conflict with the stated37. This prototype prioritises37/40and keeps the regular increments until19, then adds3rather than5. Also, the Challenge example's12conflicts with +2; the prototype follows +2and uses13.

| Level | Sequence length |
| --- | --- |
|1–8|1,2,3,4,5,6,7,8|
|9–15|10,12,14,16,18,20,22|
|16–19|25,28,31,34|
|20|37|
|Elephant21|40|

Source of requested reuse: the user's ZogajMemoGym/core/src/se/smiths/zogaj/says/SaysRound.java and screens/ZogajSaysScreen.java. This ports the specified sequence/playback concept; it does not introduce Zogaj's unrelated score penalties/recovering hearts into Memotopia. The new game and its timing are a prototype for user review, not an efficacy claim or a new research-backed design recommendation.
