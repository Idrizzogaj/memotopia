# Saved-session startup, 26 October 2026

Status: source changes and Unity editor validation only. No app package, device installation or server deployment. The existing Android 2.1.0 (22) bundle does not include these changes.

## Cause and change

`SplashViewController` opens LoginScene. Previously the credential forms stayed visible while `AuthView.getUser()` fetched the saved account; LoadingManager intentionally hides its global indicator until needed. This made a returning user appear signed out before navigation to GameMenu.

With a saved token, AuthView now hides all three credential forms in Awake, before the first frame, and reuses the existing opaque LoadingScreen prefab and animated Memotopia logo. That view remains until GameMenu replaces LoginScene. This is common code for iOS and Android. It does not bypass server authentication or claim to remove network latency.

The profile request has a 12-second timeout. A transient network, server or malformed-response failure preserves the token, offers Retry and automatically retries after 10 seconds or on app focus. Sign in explicitly clears the saved session and opens the credential form. Only an actual HTTP 401 returns automatically to sign-in. Responses from before logout/account switching cannot restore or invalidate the new account. Existing Apple/Google login and session renewal remain in place.

## Validation

`StartupSessionValidation.Review` passed in the Windows Unity2022 staging editor: real LoginScene, forms hidden before first frame, original logo reuse, retained token/retry on temporary failure, valid profile, malformed response, HTTP401, late-response guard, explicit account switch and no-token login. Phone and tablet renders were inspected. No test logged into a real account or sent a real profile request.

Log: `MemotopiaAndroidBuild/ChallengeSprint/session-says-review.log`, exit0. Known pre-existing dependency resolver and cyclic User/UserStatistics serialization diagnostics remain. A device startup check with good/slow/offline connectivity is still needed after the user authorizes the next build.

Shared fix for the 2.0 release and 2.1 Focus branches; no server change required.
