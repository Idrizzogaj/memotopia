# Saga code and visual review — 2026-11-28

Scope: Saga route/configuration/progress, map graphics and materials, result lifecycle, boss/context, Flash/Pairs/Boxes/SORT adapters, drop targets, shared Chain/Says/Focus integration, native presentation, exported prefab references and editor validation. This is a focused code/visual review, not a claim that every performance characteristic or balance value is optimal.

## Changes

- Victory remains open until MAP or REPLAY. MAP queues the scroll/unlock animation before closing the child, with an owner/stop guard; replay consumes and discards the old MAP hook. There is no automatic result timeout. All Saga wins use `You won!` and global `SAGA LEVEL <stop>`; native Training results keep their own identity.
- Boss artwork scale is1.2, previously1.4. Normal cells remain.78; the larger boss touch area is retained. The extra map star was only a boss marker, not a reward, and is removed. BOSS text and native result stars remain.
- Transparent new synapses previously exposed the old axon baked into MapUnnumbered. Local shader cutouts remove that underlying connection, protect the cell cores/nearby branches and preserve the rest of the approved illustration. The curved7-8 connection needs a wider, more protective mask than12-13/19-20.
- Eleven bounded myelin meshes connect20-21,40-41,...220-221. Their texture is an existing sleeve cropped by UV from the original map, not a new raster or drawing. The shared curved route also positions electrical pulses and the boundary boss gates. The connector leaves the synaptic gap clear. A Focus heading was moved sideways to avoid crossing the cable.
- Map geometry now responds to width and height changes. Progress is refreshed on entry/return/start; pulse frames update only the revealing cell. Immutable route configurations, per-section cell buffers and Boxes tray images are reused instead of repeated route allocation, PlayerPrefs reads and GetComponents arrays during animation.
- Saga Pairs no longer loses hearts for mismatches and hides its unused per-game hearts. It starts with5pairs on tiers1-5,6/7/8/9/10pairs on6-10,11...15pairs on11-15 and16...20pairs on16-20. Study+recall share30/30/45/60seconds, respectively. Boss Pairs uses only the boss clock and preserves the boss's remaining hearts. Chain/Boxes/Pairs/SORT also display that same clock from their initial frame, avoiding a brief flash of a per-game timer.
- Review found three terminal Focus callbacks that could keep touching a board after losing the last heart closed its boss child: wrong math input, wrong wall tap and missed wall impact. They now return before updating answer text or starting feedback on the closed board. Dedicated native-callback regressions reproduce all three paths.

## Validation and practical limits

- `SagaAllValidation.Validate`: passed 240route configurations, six memory adapters, all 80 Focus goals, local/account-isolated saves, test-install reset guards and ordinary native Focus checks.
- `SagaValidation.Validate` after final graphics changes: passed. Rendered all 12 phone map sections, tablet map,20-21 / 40-41 / 60-61 seams, gameplay/results and boss intermission screens. Original neuron/myelin/synapse assets are retained.
- `SagaPlayDriver.BeginMap`: passed actual Focus Pairs/Boxes wins; result still present after 6 seconds; MAP/replay;20-21 scroll/reveal; boss synaptic crossing; ambient full-connection pulse and Motion Off. Cache freshness is checked when starting a stage after progress changes.
- `SagaPlayDriver.BeginBoss` and `BeginFocusBoss`: passed actual six / four native games, shared clock, heart carry, paused GO, double-click guard, result identity and isolated boss progress. Automated correct answers establish integration, not human difficulty balance.
- C# 7.3/legacy-Unity-defines compilation: passed against the installed 2022 reference assemblies. This is an API/language compatibility check, not an actual Unity 2019 iOS/Metal build.
- Terminal Focus callbacks: passed fifth wrong math answer, fifth wrong wall tap and missed-wall collision with synchronous child destruction; native Focus regressions also passed.

No new phone or store build was started. Actual iPhone/Metal appearance, touch response and frame-time profiling remain to be verified in the next approved test build. App marketing version stays 2.2.0. Installed test build remains 2026112701; next build should use a minimal separate counter increment, never a date as the marketing version.

Open balance item: memory boss Flash still uses the previously approved 4x playback override. The earlier speed complaint has no final replacement speed/boss-budget specification; this review does not silently change either. Native ordinary Flash stays 1x.

The unused procedural SagaMapGraphic is not instantiated by the current painted map. It is retained as source rather than deleted under generated-build cleanup permission. Saga progress still has local account-scoped keys; no new server game IDs, XP or badges are introduced.

## Flashing Pairs — separate pending Training experiment

User approved a new Training-only game, explicitly not Saga: 20 levels plus a Memory Athlete view, no statistics/XP/badge registration. Level 1 has 2 pairs and 3 seconds to memorize, then +1 pair / +1 second each level, always 60 seconds recall. Common native full-grid board support and a controller template are prepared. FlashingPairsBoardValidation passed 4 / 42 cards fitting phone/tablet, ignored study/modal input, hidden recall, mismatch heart loss and correct-pair completion. The prepared controller template and explicit-rule installation helper are saved under `tools/flashing-pairs/`; no unchosen rule has been added to the app. One user clarification is pending: two different associated images like ordinary Pairs, or two identical images whose positions are memorized. The choice determines card faces, study grouping and whether recall positions may shuffle; it must not be guessed silently. The full game is not yet exposed in Training.
