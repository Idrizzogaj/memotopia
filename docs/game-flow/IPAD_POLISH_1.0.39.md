# iPad polish 1.0.39 (2026091707)

Boxes: the white platform follows the slight skew of boxes-ring, with rounded quadratic corners and a continuous shaded front rim. The motif stays upright. Empty recall cells keep the platform but no picture contact shadow. Unity rendered phone and 4:3 canvas geometry; this does not replace a physical iPad layout check.

Ranking: StatisticsView previously defaulted an absent user's index to zero, duplicated the winner in the last row, assumed ten returned entries, and only loaded once. It now refreshes when enabled, hides placeholders while loading/on failure, handles short/empty responses, and only adds the actual user's row when their ID exists. Old account/request responses are ignored and highlighting resets per row. Regression checks cover absent account, own rank beyond capacity and empty/unavailable responses.

The user's exact report of being first remains unconfirmed: an asynchronous question asks whether it was an Achievement or the ranking table, and which account was signed in. No leaderboard scores or historical achievements have been changed. The Achievement grant already compares the first server user ID against the current account. Do not claim that all causes of the report are solved until clarified.

Unity 2019.3.12f1 Personal is still in use (HasPro=false). Its Unity startup logo remains; no license bypass or engine migration is part of this update.

Validation: FlowValidation.Review and ReviewAndBuild passed; Xcode build succeeded. Installed on ZogajSon. Hanushe was disconnected, so the iPad update remains pending. Build/install status recorded in CODEX_HANDOFF.md.
