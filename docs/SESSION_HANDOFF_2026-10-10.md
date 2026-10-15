# Memotopia video and tips update â€” 2026-10-10

Read this update, then `SESSION_HANDOFF_2026-10-09.md` for the complete project/build/deployment context and user rules.

## Scope and status
- Idriz supplied four MP4s and requested the same streaming/in-app playback as the existing lessons, ordered after Introduction and before Linking Method.
- Uploaded all four to the existing one.com `/video/` directory. Public HTTPS requests support byte ranges (206, `video/mp4`); full downloaded SHA-256 hashes match the supplied files. Original four files and website pages were not replaced.
- Shared code is updated in the local Android copy and Windows repo frontend (iOS source). No Git commands, commit, push, APK, backend deploy, or store upload performed in this session. Idriz subsequently explicitly authorized builds for Play and iOS, and installation on ZogajSon. Both platform builds are complete; iOS installation is verified. All work outside OneDrive.
- **Latest Android: `C:\Users\IdrZog_MEM\MemotopiaAndroidBuild\Output\Memotopia-17.aab`, version 1.0.51 / code 17**, 63,067,266 bytes. Unity Succeeded, 9m29s, existing 3 graphics/log errors and 104 warnings; bundletool validation, manifest, jarsigner, expected upload certificate, ARM64 ELF 16 KB alignment and no Facebook paths all pass. Package `com.zogajretention.memotopia`, min API 24, target API 36. Evidence: `Output/version17-verification.json`, `Memotopia-17.native-verification.json`, `Memotopia-17.aab.build-summary.txt`, `unity-build-17.log`. Not uploaded to Play in this session. Prior build 16 retained.
- **iOS 1.0.51 / 2026101001 is installed on ZogajSon**, independently confirmed with devicectl on 2026-10-10. Unity 2019.3 export succeeded. The first SSH Xcode attempt failed at codesign (`errSecInternalComponent`, locked keychain); user was asked to unlock locally. Subsequently devicectl confirmed the new version installed, and Idriz explicitly reported the new videos and tips scrolling working. Do not treat signing/install as still blocked.
- Mac source and .meta files synced after comparison. Build method version updated on Mac (backup `ios-build/MemotopiaBuild.before-2026101001.cs`). Export: `/Users/idrizzogaj/Projects/memotopia/ios-build/IzFreshBuild`; built app: `ios-build/DeviceBuild/Build/Products/Release-iphoneos/memotopia.app`. Added optional local `~/Desktop/Memotopia/Install-Memotopia-1.0.51.command` to unlock/sign/verify/install on ZogajSon only. The new optional script was not used. Idriz confirmed he ran the EXISTING command `bash ~/Desktop/Memotopia/build_and_install.sh`, which built/signed/installed the new Unity export. That script still has old 1.0.50 labels and output names; the actual device version was independently verified as 1.0.51 / 2026101001. Prefer the established command, and do not confuse its labels with the actual app version.
- Next concrete action: upload AAB 17 to Memotopia Play internal testing, then test Android. No new build needed for the completed video/tips fixes. Future new builds still require Idriz's approval.

## Media
Originals: `C:\Users\IdrZog_MEM\Downloads\Mp4`.
Upload copies and verification evidence: `C:\Users\IdrZog_MEM\MemotopiaAndroidBuild\VideoUpdate` (`upload/`, `video-hosting-verification.json`, `uploaded-videos.png`). MP4s have the moov atom before media data for progressive playback. Do not add these binaries to Unity Assets or commit them by default.

Public URL base: `https://www.memotopia.com/video/`

| New filename | Original file |
| --- | --- |
| `5_HowToPlayPairs.mp4` | How to play the Game of Pairs.mp4 |
| `6_HowToPlayBoxes.mp4` | How to Play the Game of Boxes.mp4 |
| `7_HowToPlayFlash.mp4` | How to Play the Game of Flash.mp4 |
| `8_ChallengeFriends.mp4` | Challenge friends and foe in epic memory battles!.mp4 |

## Code changes
Relative to both frontend/Assets and Android Project/Assets:
- `Scripts/views/VideosEnhancer.cs`: Introduction, Pairs, Boxes, Flash, Challenge, Linking Method, Associations, Memory Palace. New rows use the existing in-app player. Original premium restrictions remain; new guides are free like the previous YouTube rows. Bottom padding belongs to VerticalLayoutGroup because ContentSizeFitter overrides manually assigned heights.
- `Scripts/views/MenuView/MenuNavigation.cs`: game guide playback clears the previous lesson's achievement key; returning to the video list stops playback and clears the key.
- `Resources/_VideoPlayer/Scripts/MyVideoPlayer.cs`: resets playback controls on every enable, not only the first Start.
- `Scripts/views/ScrollAboveFooter.cs` + meta: keeps text viewport above fixed read bar with 24 canvas-unit gap, recalculating after size/orientation changes. Legacy pages get a separate masked viewport.
- `Scripts/views/TipsEnhancer.cs`: applies footer clearance to fixed-height as well as layout-driven tips. SensesPage and DreamPage were static with unbound ScrollRects; groups their artwork/text into scrollable Content while keeping Back and ReadBar fixed.
- `Editor/VideoTipsValidation.cs` + meta: scene-based checks of all 8 rows and all 12 tips at three portrait heights; repeated initialization, last-row bounds, actual text bounds, and read-to-end gating. It does not save the scene or contact the account backend.

## Validation / next action
Unity 2022.3.62f2 compilation and scene validation PASSED: `VideoUpdate\validation-4.log` contains `VIDEO_TIPS_VALIDATION_PASSED`. Checks cover all 8 rows and 12 tips at canvas sizes 800x1200, 800x1422 and 800x1778, repeated setup, bottom clearance including text bounds, and read-to-end gating. Earlier validation logs intentionally record the issues fixed during this session. Existing dependency-resolver and missing-prefab scene warnings remain; they were not introduced or resolved here.
Pairs direct MP4 additionally decoded and played in the desktop in-app browser: 1280x720, duration 227.3 seconds, readyState 4, advancing playback, no media error. Idriz subsequently tested successfully on iPhone as recorded above.
Idriz initially reported the four new links failed while existing videos worked. After the new iOS app was installed, he confirmed: "Videona funkar nu och även skrollningen i Tips delen." Android device validation via Play is still pending.
Eligible for a later user-authorized commit: the shared source changes and their Unity .meta files, validator and handoff documents. Do not include signing material, caches, output builds or the staged MP4s.
