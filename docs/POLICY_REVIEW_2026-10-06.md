# Policy review – memotopia.com/privacy-policy.html (original dated 2020-05-28)

Reviewed 2026-10-06 against what the app and backend actually do now, and against current App Store / Google Play / GDPR expectations. This is a gap analysis, not legal advice; have the final text checked by someone qualified before relying on it. Nothing was published or changed on the website.

**Verdict: not ready for a public release on either store. The text must be rewritten, not patched.**

## A. Statements that are wrong or out of date
| Where | Problem |
|---|---|
| "register through Facebook or Google … first and last name" | Facebook login was removed. Google is not active. The app collects **no first/last name**. Apple sign-in (iOS) is not mentioned at all. |
| "Device identifier, IP-address" collected at registration | The app does not collect a device ID. IP addresses appear in server/proxy logs only. Say what is true. |
| Profiling used for "look-alike" marketing on Facebook/Instagram; marketing e-mails, notifications, "business partners" | The app does no such marketing. Delete. Bundling marketing consent into the Terms is also not valid GDPR consent. |
| Transfers to "United States or other countries" | Backend is hosted in the EU (memotopia-eu-1). Describe the real processors (see C). |
| Ages: 13–15 need parental consent | Sweden's age of digital consent is 13. Contradicts the 13+ age rating set in App Store Connect / Play. Align to 13+. |
| Rectification "must be made in writing and signed" | GDPR does not allow such a formality. Remove. |
| "Premium Lifetime subscription … 100 years" | No lifetime product exists. Remove or keep out until it does. |
| Controller "Memotopia, 41276 Göteborg" | Use the legal entity (Zogaj of Sweden AB, org. no., address) and a mailbox that exists (memotopia@memotopia.com was given; the text says support@). |
| Withdrawal/refund wording (14 days, refunded within 30 days) | Purchases are made through Apple/Google, who handle refunds. Say that only. |

## B. Missing – required by stores or GDPR
1. **Account deletion** (Apple 5.1.1(v), Google Play account-deletion policy): how to delete in the app, **and a public web URL** for requests; what is erased, what is kept and for how long. The app now has Account → "Delete account" (new in 1.0.40); a web page/mail path for people without the app is still needed (draft below).
2. **Legal basis per purpose** (contract, legitimate interest, legal obligation) and **retention periods**.
3. **Full GDPR rights**: access, rectification, erasure, restriction, objection, portability, withdraw consent, and the right to complain to **IMY (Integritetsskyddsmyndigheten)**.
4. **E-mail verification** and what happens to unconfirmed registrations (deleted after 24 h).
5. What other players can see: username, avatar, level/XP, achievements, challenge results (search, leaderboard, challenges).
6. **Subscriptions**: Apple 3.1.2 needs the paywall itself to show price, period, auto-renewal terms and working links to Terms and Privacy Policy. Check the in-app paywall.
7. A statement that deleting the account does **not** cancel the store subscription.
8. Security measures described accurately (HTTPS, hashed passwords, hashed one-time tokens).

## C. Facts to confirm before writing the new text (not guessable)
- Processors and where they are: hosting (Hetzner, EU?), e-mail delivery (one.com SMTP), **Sentry** (the backend has Sentry; error reports can contain user data), Apple/Google (payments, Sign in with Apple), any analytics/crash SDK in the Unity app (Unity Analytics? check). Each must also be reflected in Apple "App Privacy" and Google "Data safety".
- Retention: how long are logs kept? backups (deleted accounts persist in backups until they rotate)?
- Does the app use any tracking or advertising identifier? (Play: no ads was saved. Apple: needs matching answers.)
- Legal entity details and the mailbox that will receive deletion/privacy requests.

## D. Consistency checks (must match the policy)
- Apple App Privacy answers ↔ policy ↔ Google Data safety. Data types actually handled: email, username, user ID, gameplay/usage data (scores, XP, achievements), purchase status; not linked to tracking.
- Age rating 13+ ↔ policy age section.
- Google reviewer needs a working login with Premium (Play "app access"); with e-mail verification this account must be created and confirmed in advance.

## E. Suggested next step
Confirm section C, then a new policy + terms in English (and Swedish if wanted) is drafted and replaces the 2020 text; keep the old PDF as an archive. Publishing requires an explicit go from the owner.
