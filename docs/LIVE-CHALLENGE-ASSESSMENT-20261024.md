# Live presence, invitations and blocking — assessment, 24 October 2026

Status: assessment only. Do not implement these features or publish policy changes until the user answers the requested yes/no approval. iOS1.0.57 contains only the approved UI/statistics draft.

## Feasibility

Feasible, but not a small UI patch. Current SprintLobby polls every20seconds, and SprintService.start explicitly makes the second player wait for the pace setter's recorded result. There is no app presence gateway, invitation acceptance state or player block relation in the reviewed code. A genuinely simultaneous match needs authenticated real-time delivery, a shared server start/deadline and explicit reconnect/timeout handling. Showing a green dot would not make the existing asynchronous protocol live. Heroic Labs' authoritative multiplayer documentation is a primary implementation precedent, not a proposal to replace our NestJS backend with Nakama.

The existing cancel method awards the opponent a win and increments game counters. Declining an invitation must therefore be a separate transition, never a call to cancel. Pending, declined and expired invitations must not become games/losses/XP/ranking results. Only explicit decline increments the recipient's separate private Declined counter; ignored/time-expired invitations should not be reported as deliberate rejections. Acceptance makes a match eligible for game-result statistics; completed/forfeited accepted games follow the agreed result rules. An asynchronous creator's provisional played result cannot force a recipient into statistics.

## Privacy-preserving boundaries proposed for approval

- Visibility off by default; the player affirmatively enables Show online and can disable it. Favorites is currently a unilateral relation, not the other player's permission to disclose presence. A favorite badge and public online list show only players who enabled the relevant visibility and are not blocked.
- Presence represents an active foreground authenticated connection; expire it promptly after disconnect (e.g.30–60seconds), not a permanent last-seen/activity history. No location, contacts, advertising identifiers or background tracking are needed. No chat, contact upload or OS push notifications are part of this proposed scope.
- Direct in-app invitations with Yes/No. A receiver's decline, no response or invitation expiry is not a loss. Decline totals are visible only to that player, separate from Games/Losses/Draw/XP/ranking.
- Enforce blocking on the server as well as the UI, covering new invitations and presence visibility in either direction. Provide unblocking. Prevent repeated invitations with one pending request per pair, cooldowns and server rate limits. Do not expose a private block list or tell an aggressor who blocked them.
- Keep block relations only while needed for the block/account, with account deletion cascading appropriately. Minimize invitation records; specify real retention before release. Preserve necessary game results under the disclosed game-data policy. Address abuse reporting/moderation too; blocking alone is not a blanket App Store compliance guarantee.
- Shared start time and authenticated participant-only match updates; decide safe reconnect and accepted-match abandonment behaviour before implementation/testing. Do not claim that polling or a recorded opponent trace is live play.

These are engineering/privacy recommendations, not a certification of legal compliance or evidence of an optimal game mechanic. A public always-on online list and public rejection totals are not recommended.

## Documents checked

- Published https://memotopia.com/privacy.html and https://memotopia.com/terms.html were retrieved over HTTPS and compared with docs/web/site/. Normalized contents match exactly. Privacy dated10October2026; Terms dated8October2026. Hashes: privacy63d521cbda64873fc839ee667fa3de0d9d36174b73cfc240554b54966c3d7d43; termsb466e2d5d5a284d2e1b47b4f7d0d92c3ec40345a375d8ac6bbbaf47c1e0ca588.
- Privacy sections3/4 cover account identifiers, favorites and challenge results, but do not explain presence visibility, invitation decisions, decline aggregates or block lists. Sections6/7 need concrete ephemeral presence/invitation/block retention and deletion behaviour. Section11 already promises notice of material changes. Optional presence visibility needs a documented legal-basis/consent assessment, including the13+ audience and applicable country-specific requirements; existing contract wording is not automatic permission for public activity disclosure.
- Terms section3 already prohibits harassment, but should explain invitations, refusal without a game loss, blocking/reporting and the accepted-match disconnect/forfeit rules.
- docs/legal/PRIVACY_POLICY_DRAFT.md and docs/POLICY_REVIEW_2026-10-06.md are older review/draft material, not the active published policy.
- frontend/Assets/Editor/PrivacyInfo.xcprivacy already declares linked UserID and ProductInteraction for app functionality, but must be reassessed against the actual new data flow. It is not proof of the correctness of the separate App Store privacy labels or Google Play Data safety form. Those current console declarations were not independently inspected in this assessment and must be checked before release.

## Primary sources newly consulted

1. IMY, privacy by design/default: https://www.imy.se/verksamhet/dataskydd/det-har-galler-enligt-gdpr/inbyggt-dataskydd-och-dataskydd-som-standard/ — minimize necessary data, privacy-friendly defaults, user control, retention and documented risk assessment.
2. Apple App Review Guidelines1.2 and5.1: https://developer.apple.com/app-store/review/guidelines/ — abusive-user blocking, reporting/moderation and privacy obligations for relevant social/user-generated content features.
3. Google Play User Data: https://support.google.com/googleplay/android-developer/answer/10144311?hl=en — accurate privacy disclosures, data safety alignment, retention and account deletion.
4. Heroic Labs authoritative multiplayer: https://heroiclabs.com/docs/nakama/concepts/multiplayer/authoritative/ — server-owned match state, presence join/leave, update loops and disconnect handling. Implementation precedent, not empirical proof that one UI/timing choice is best.

All four are new relative to the prior game-design source entry. Public policy text has not been changed or republished.

Implementation was subsequently authorised by the user. Mutual friendships and independent audience defaults were then added to the specification. See LIVE-CHALLENGES-20261024.md for actual implementation and remaining public-release checks.
