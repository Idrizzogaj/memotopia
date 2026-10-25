# Focus 2.1 mockup interpretation

Source: user's five-page handwritten `Focus games - idea.pdf` (24 October 2026), plus the explicit Boxes clarification in chat. This is a review prototype, not a native app build. The approved existing Memory game symbols are exported directly through TrainingGameSymbol; no substitute artwork. Focus uses the existing teal identity. App language stays English, explanatory review notes are Swedish.

Source PDF SHA256: `8d5e4aa911acc88d687eb57036a65a4448d9806c8403acbf510b518376917876`.

Earlier session specification: each Focus run lasts 90 seconds. The mockup lets the user select a starting level and try all four activities locally. No account, XP, live play, achievements or production server state is changed. The 2.0 branch keeps Focus hidden.

Boxes scoring follows the explicit +1/−1 specification. For the other three games, the preview score counts correct answers; wrong answers add an error without a points deduction. Their final scoring was not defined in the notes.

## Pairs: mathematics (PDF page 5)

| Level | Operation / range | Questions |
| --- | --- | --- |
| 1 | Addition up to 20 | 5 |
| 2 | Subtraction up to 20 | 5 |
| 3 | Addition up to 50 | 5 |
| 4 | Subtraction up to 50 | 5 |
| 5 | Multiplication up to 100 | 5 (inherits previous question count) |
| 6 | Division up to 100 | 5 (inherits previous question count) |
| 7 | Mixed addition, subtraction, multiplication, division up to 100 | 10 |
| 8+ | Same mixed operations up to 200 | 10 |

The user writes the answer. Prototype assumption: integer answers, exact division, no negative subtraction results; addition/multiplication result stays inside the range, subtraction/division operands stay inside the range. The PDF does not define operands versus result limits; this needs approval before final game implementation.

## Boxes: anticipate the next wall (PDF page 4 + chat clarification)

Click the wall a ball is heading toward **before** it arrives. A correct pre-emptive click flashes that wall green and earns +1. A bounce without a timely correct click flashes red and loses 1 point. Correctly covered bounces do not score a second time. Balls continue moving and reflecting.

| Level | Boxes | Balls per box | Correct anticipations per ball |
| --- | --- | --- | --- |
| 1 | 1 | 1 | 7 |
| 2 | 1 | 2 | 7 |
| 3 | 1 | 3 | 7 |
| 4 | 2 | 1 | 5 |
| 5 | 2 | 1 | 7 |
| 6 | 2 | 2 | 7 |
| 7 | 2 | 3 | 7 |
| 8 | 3 | 1 | 5 |
| 9 | 3 | 1 | 7 (provisional continuation) |

Level9 is only written as “3 boxes ... etc.”, so its continuation is provisional. Ball speed and later progression are not specified. For the prototype: a wrong wall gives no reward and does not add an extra penalty; if multiple balls head for the same wall, the click covers the earliest uncovered arrival, one ball per click. These are temporary interaction choices to make the sketch testable, not approved scoring rules. Completed balls remain in motion, visually muted, and no longer score; the next level starts once each ball reaches its target.

## Flash: scattered numbers (PDF pages 2–3)

Green ascending; red descending. All numbers are visible before clicking; a correct click leaves an empty outline in place, matching the sketch. Mixed stages alternate one green, then one red, keeping green ascending and red descending. Green circles and red squares distinguish mixed streams in addition to colour, following the mixed-stage sketch.

| Levels | Content |
| --- | --- |
| 1–2 | 3 green ascending / 3 red descending |
| 3–4 | 5 green / 5 red |
| 5–6 | 7 green / 7 red |
| 7–8 | 9 green / 9 red |
| 9 | Alternate green1→3 and red3→1 |
| 10 | Alternate green1→5 and red5→1 |
| 11 | Alternate green1→7 and red7→1 |
| 12 | Alternate green1→9 and red9→1 |
| 13 | Alternate green1→12 and red12→1 |
| 14+ | Alternate green1→15 and red15→1 |

Five consecutive errors return the user to the preceding level (minimum1). The PDF explicitly places this rule under Flash. A correct answer clears the consecutive-error streak.

## Chain: numbers in rows (PDF page1)

| Level | Content |
| --- | --- |
| 1 | Green1→6 |
| 2 | Red6→1 |
| 3 | Green1→12 |
| 4 | Red12→1 |
| 5 | Green1→15 |
| 6 | Red15→1 |
| 7 | Green1→6, then red6→1 |
| 8 | Green1→12, then red12→1 |
| 9 | Green1→15, then red15→1 |
| 10 | Alternate green1→6 and red6→1 |
| 11 | Alternate green1→12 and red12→1 |
| 12+ | Alternate green1→15 and red15→1 |

The handwritten level7 red range reads1→6, but the page heading explicitly says red/down and subsequent mixed levels descend. Prototype follows the heading: red6→1. Repeated rounds at the final described level retain the same rules. No invented extra levels or speed ramp. No clinical or training-efficacy claims are made.

## Provisional details for native test review

- Confirm the arithmetic range interpretation and integer-only division.
- Confirm Chain7's red direction and Boxes9+ continuation.
- Confirm behaviour on a wrong wall and one click per incoming ball.
- Confirm whether the Flash five-error fallback should apply to other Focus games as well.

These are interpretations of the user's approved concept, not new game-design recommendations. If new balance/mechanic alternatives are proposed, apply AGENTS.md's three-source rule and record them in GAME-DESIGN-SOURCES.md.

## Native test implementation,25October2026

Latest user correction: Focus Flash is a working-memory task, not an ordering task with persistent labels. All numbers appear in circles of randomly varied size and non-overlapping random positions. Display time is1second for3items,2seconds for4–6and3seconds for7+. Input is blocked during display, then numbers disappear while the blank circles remain for ordered recall. Both red and green use circles. Chain keeps visible numbers and receives thicker outlines. Boxes walls increase from2%to5%of box width, with broad120unit touch strips and nearest-wall routing at corners. These corrections supersede the earlier prototype presentation; Pairs is unchanged.

All four games are implemented in FocusGame.cs and FocusRules.cs, enabled only on codex/focus-2.1.0. They reuse the original Flash header, original button/back hierarchy and quit/result dialog. Each runs90seconds with local score only. The native test follows the provisional interpretations above; no new XP/achievement/account writes. Windows Unity validation passed2,000 arithmetic cases, all unique number sequences, wall prediction/reflection/early tap handling, native callbacks,90second termination, score popup and four rendered native scenes. The actual Unity2019 iOS export also compiled its managed sources successfully. Native binary/signing/install verification is tracked in the build record.
