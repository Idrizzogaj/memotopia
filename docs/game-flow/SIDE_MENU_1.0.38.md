# Side-menu availability — 1.0.38 (2026091706)

User requested greying unavailable actions, checking/fixing Rate, then phone installation before TestFlight.

- Account and Log Out remain enabled.
- Facebook Share is disabled and faded: Facebook SDK was removed and the old method only opened the website. The fallback website redirect is removed.
- Go Premium waits until Unity IAP has initialized and both subscriptions are available to purchase. The unfinished iOS monthly-to-yearly upgrade is disabled in the side menu. The live store previously returned both products successfully, but no purchase or upgrade was executed in this check.
- Rate is disabled because the public listing currently cannot be opened. Public Apple lookup returned no results in Sweden/USA; the Swedish App Store page returned HTTP 404. App Store Connect still records legacy version 0.0.19 as READY_FOR_SALE, while territory content status reports CANNOT_SELL. No availability, agreement, or public publishing settings were changed.
- Prepared the documented direct review URL, https://apps.apple.com/app/id1442530846?action=write-review; `PublicStoreListingAvailable` remains false pending a working public listing. The former exception handler that quit the app is removed. Apple documentation: https://developer.apple.com/documentation/storekit/requesting-app-store-reviews.

SideMenuAvailability applies disabled interaction and faded appearance, refreshing while the menu is open as store initialization completes. Existing menu actions and gameplay remain unchanged.

Unity validation covers enabled Account/Logout, disabled Facebook/Rate, store readiness and unsupported upgrade. Actual open-menu render reviewed. Unity export, Xcode device build, archive and App Store export succeeded. 1.0.38/2026091706 installed and verified on ZogajSon. Launch attempt was denied because the phone was locked, not an observed crash. User unlock question is pending. See CODEX_HANDOFF.md for TestFlight upload/processing result.

Distribution: the first upload was rejected for minimum iOS 10. The permanent deployment target is now iOS 15, verified in the new archive. That archive's developer-signed app was reinstalled on ZogajSon and version verified. App Store export and upload succeeded without errors (delivery UUID 34dccf6e-0709-4c7c-8a0e-6f6a5d774cbf). Apple buildUploads API confirms PROCESSING with no errors or warnings; TestFlight install availability is not yet verified. No public App Store release or external tester group changes.
