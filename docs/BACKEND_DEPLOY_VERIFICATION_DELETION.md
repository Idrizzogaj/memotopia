# Backend: e-mail verification, rate limiting, account deletion (DEPLOYED to production 2026-10-06)

Code is in `backend/` (uncommitted). Unit tests (17) and database tests against a throwaway Postgres (6) pass locally. **Deployed 2026-10-06 and verified end to end on the live API** (register, mail, confirm, login, delete). Server mail is Brevo SMTP; its credentials were moved from the server code into `/opt/memotopia/backend/.env.app` (MAIL_*). Backups: `/opt/memotopia/backups`.

## What changed
- Registration (`POST /users`): creates the account as *pending*, mails a one-time link, returns `{verificationRequired:true,email,mailSent}` and **no token**. Existing users are unaffected (`emailVerificationPending` defaults to `false`, so the automatic `synchronize: true` schema update cannot lock anyone out).
- Login (`POST /auth/signin`): correct password but unconfirmed -> 403 `EMAIL_NOT_VERIFIED`. Pending accounts have no valid session.
- Link flow: `GET /users/verify-email/:token` shows a button (so mail scanners cannot burn the token), `POST` consumes it atomically (only the SHA-256 hash is stored, 24 h expiry, single use).
- `POST /users/resend-verification {identifier}` (email or username): always 204, 60 s cooldown per account.
- An unconfirmed registration older than 24 h no longer blocks that e-mail/username (it is deleted when someone registers again).
- Rate limits (in-memory, single process): register 5/h per IP and 3/h per address, signin 30/min per IP and 10/15 min per username, resend, forgot-password, verify, delete. `trust proxy` set to 1 (Caddy) so the real client IP is used.
- `POST /users/delete-account {password?, confirm:"DELETE"}` (auth required): checks the password for e-mail accounts, then in one transaction removes favorites (both directions, no cascade exists) and the user; everything else cascades from `user`. Other players' data is untouched.
- Fixed: SQL injection in `GET /users/search/:search` (now a bound parameter, LIKE wildcards escaped); password-reset link no longer built from the request `Host` header (uses `PUBLIC_BASE_URL`); forgot-password no longer reveals whether an address exists.
- **SMTP credentials were hard-coded in `mail.module.ts` (and are in git history).** They now come from environment variables. **Change that mailbox password** and set it only in the server environment.

## Environment variables to set on the server before starting the new image
`MAIL_HOST` (default send.one.com), `MAIL_PORT` (default 587), `MAIL_USER`, `MAIL_PASS`, optional `MAIL_FROM`, `PUBLIC_BASE_URL=https://memotopia-eu-1.duckdns.org`.
Without `MAIL_USER/MAIL_PASS` nobody can confirm an address, i.e. **no new registration can complete**. Check that sending works (e.g. password reset to a test address) *before* switching clients over.

## Order of rollout (recommended)
1. Back up the database (`pg_dump`).
2. Set the env vars above, then deploy the new image. Schema: the app auto-syncs the five new columns; `1791200000000-AddEmailVerification` is the same change as a migration if you prefer to run it explicitly (do not run both).
3. Smoke test with a test address: register -> mail arrives -> link -> confirm -> log in -> delete account.
4. Only then ship client versions that expect `verificationRequired` (iOS 1.0.40+ handle both old and new server responses).
Rollback: previous image; the new nullable/default columns are harmless to the old code.

## Known gaps (not done)
- Apple "Sign in with Apple" token revocation on deletion (needed only if Apple login is active for users).
- Payment status is still set by the client (`/users/update-payment-status`), no server-side receipt validation. Not fixed here.
- Rate limits reset on restart and are per process.
- Challenges the deleted user took part in lose that participant; the opponent keeps their own record.
- `forgotPassword` token is stored in plain text (pre-existing).

