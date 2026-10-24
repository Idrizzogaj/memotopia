## 2026-10-22 — 90-second Challenge discussion (proposal, not approved implementation)
- Mattel/Radica Tetris Challenge manual: https://service.mattel.com/instruction_sheets/I1045.pdf — timed two-player points/lines competition; supports fixed-duration scoring precedent, not proof that90seconds is optimal.
- Nintendo Big Brain Academy manual: https://www.nintendo.com/eu/media/downloads/games_8/emanuals/nintendo_ds_21/Manual_NintendoDS_BigBrainAcademy_EN.pdf — timed cognitive activities/multiplayer and separate categories; game precedent, not clinical efficacy evidence.
- osu! developer wiki Performance points: https://osu.ppy.sh/wiki/en/Performance_points — speed, accuracy and difficulty contribute to performance; supports considering accuracy/difficulty alongside raw progress, not copying its formula.
Assessment:90-second rounds are promising for quick asynchronous duels, but memorisation and recall must both consume the same90seconds; equal content/order/settings within a match, game-specific correct progress, avoid rewarding random tapping. Consider reusing planned90-second Focus rules for Challenge to avoid conflicting variants. Specific scoring penalties/tie-breaks and game loops need user agreement/playtests. No claim that90seconds is empirically optimal. Nintendo Switch marketing page fetch502; use official DS manual instead. All three sources new for this discussion.

## 2026-10-24 — Live presence/invitations assessment (subsequently approved)
- IMY privacy by design/default: https://www.imy.se/verksamhet/dataskydd/det-har-galler-enligt-gdpr/inbyggt-dataskydd-och-dataskydd-som-standard/ — supports privacy-friendly defaults and minimizing disclosed activity data.
- Apple App Review1.2/5.1: https://developer.apple.com/app-store/review/guidelines/ — supports blocking/reporting/moderation and privacy safeguards for relevant social features.
- Google Play User Data: https://support.google.com/googleplay/android-developer/answer/10144311?hl=en — supports matching disclosures/retention/deletion to new actual data flows.
- Heroic Labs authoritative multiplayer: https://heroiclabs.com/docs/nakama/concepts/multiplayer/authoritative/ — first-party implementation precedent for server-owned state, join/leave and disconnect handling; no claim of universally optimal timing.
All four sources new relative to the preceding entry. See docs/LIVE-CHALLENGE-ASSESSMENT-20261024.md. User subsequently authorised implementation, then specified accepted friendships with separate friend visibility on by default and public visibility off. This is an approved product specification, not empirical evidence of a best default.

## 2026-10-24 — Friend consent and separate presence audiences
- [IMY: children's personal data](https://www.imy.se/verksamhet/dataskydd/det-har-galler-enligt-gdpr/introduktion-till-gdpr/personuppgifter/personuppgifter-om-barn/) (new): clear explanations, capacity to understand, and country/age consent requirements matter. A13+ label alone is not blanket consent approval.
- [Epic: Fortnite social privacy controls](https://www.epicgames.com/help/c-34254770/a11220579) (new): first-party precedent for user-facing presence and social privacy controls; no evidence that a particular default is best.
- Apple and Google sources above support blocking, disclosures and privacy controls. Together these are four independent organisations; IMY/Epic are new pages for this follow-up. No new game balance or scoring rule is proposed here.

## 2026-10-25 — Disconnects, give-up and late receipts
- Unity session lifecycle (new): https://docs.unity.com/en-us/mps-sdk/session-states — explicit connected/disconnected/deleted states.
- Photon recovery (new): https://doc.photonengine.com/realtime/v5/troubleshooting/analyzing-disconnects — distinguish transient disruption from terminal session loss; bounded rejoin.
- AWS idempotent APIs (new): https://aws.amazon.com/builders-library/making-retries-safe-with-idempotent-APIs/ — safe retries, late arrivals and atomic exactly-once effects.
These three independent primary sources support lifecycle recovery and receipt safety. Existing90second rounds and60second receipt grace remain; a previously indefinite one-ready-player wait now uses that same grace. No claim of empirically optimal timings. See docs/LIVE-RECOVERY-20261025.md.
# 2026-10-25 — app closure versus temporary interruption

- Unity lifecycle reference: https://docs.unity.cn/430/Documentation/ScriptReference/MonoBehaviour.OnApplicationQuit.html — iOS suspension does not reliably invoke quit; distinguishes suspension from explicit exit.
- Photon Fusion recovery documentation: https://doc.photonengine.com/fusion/v2/troubleshooting/analyzing-disconnects — bounded recoverable reconnect/rejoin, no assumption that every disconnection is final.
- AWS GameLift session states: https://docs.aws.amazon.com/gameliftservers/latest/apireference/API_PlayerSession.html — server-owned active/completed/timed-out states and bounded connection waits.

Three independent primary organisations. Unity lifecycle and AWS session reference are new compared with the earlier live-recovery search. These support explicit lifecycle/recovery states, not a proven optimal grace period or whole-match forfeiture. Whole-match Give up is an implementation judgement; real90second time during interruption, resume position, terminal disconnection loss and reaction rules were explicitly requested by the user.

