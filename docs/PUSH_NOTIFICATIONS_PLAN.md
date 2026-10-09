# Push notifications for challenges – plan (2026-10-07, not started)

**Goal:** when someone challenges you (or answers your challenge) you get a notification on your phone even if the app is closed. Today the only signal is a number on the Challenge icon, so most challenges are noticed late.

## What it takes

### Apple (iOS)
1. In the Apple Developer account: enable the **Push Notifications** capability on the app ID `com.retention.memotopia`.
2. Create an **APNs Auth Key (.p8)** (one key works for all apps). Note Key ID and Team ID (`E838WX6KF8`).
3. Xcode/Unity: add the `aps-environment` entitlement (the export script `MemotopiaBuild.ConfigureXcodeExport` must add it, the same way it fixes other settings).
4. App asks permission on a good moment (after the first sent challenge, not on first start).
5. App receives the **device token** and sends it to our server.

### Google (Android)
1. Create a **Firebase project**, add the Android app `com.zogajretention.memotopia`, download `google-services.json`.
2. Firebase Cloud Messaging (FCM) HTTP v1 needs a **service account key** on the server.
3. Android 13+: the app must request the `POST_NOTIFICATIONS` permission (again at a good moment).
4. Unity: Firebase Messaging plugin (check compatibility with Unity 2022.3 for Android; iOS stays on Unity 2019.3, so on iOS use plain native APNs registration instead of the Firebase SDK).

### Server (NestJS)
1. Table `device_token` (userId, platform, token, updatedAt). Endpoints: register token on login, remove on logout and on account deletion (**the delete flow must also remove tokens**).
2. When a challenge is created or answered: send a short notification to the opponent ("Hanushe challenged you!"). iOS via APNs HTTP/2 with the .p8 key, Android via FCM v1.
3. Remove tokens that APNs/FCM report as invalid.
4. Respect rate limits: at most one push per challenge event, and none to a user who is currently online in the app.

### Privacy and store forms
- The privacy policy and the Apple **App Privacy** / Google **Data safety** forms must list the device token (identifier used for notifications, not for advertising).
- Notification text must not include private data.

## Order of work (one build per platform)
1. Server tables/endpoints + sender (testable without the app, using a test token).
2. iOS registration + entitlement → TestFlight build.
3. Android Firebase → AAB.
4. End-to-end test: challenge from phone A → notification on phone B.

## What I need from the owner
- Access to the Apple Developer account for the capability + the **.p8 key** (never in chat – place it on the server or give it to me through a secure file path you control).
- A Firebase project and its two files (`google-services.json`, service account JSON).
- Decide the wording and language of the notifications (English first?).

## Risks
- Unity 2019.3 + current Firebase/APNs plugins: use native APNs registration on iOS to avoid SDK incompatibility.
- Notifications on TestFlight/development builds use the APNs **sandbox**; production builds use the production endpoint. The server must know which.
- A user who denies permission still sees the in-app number; nothing breaks.

## Cost
Apple and Google push services are free. Server load is negligible at 1 000 users.
