# Follow-up to user test of 1.0.36

User approved pacing and achievement appearance. New request: restore blank profile/purchase side menu, swipe left/right between achievement details, and dismiss details by tapping outside the card as well as X; install a new version.

Cause of hidden menu: FullScreenArtwork pinned Home background horizontally to the canvas while Home slid left, covering the side-menu controls beneath. It now expands only vertically for portrait safe-area margins and retains horizontal movement. Reviewed rendered open side menu, Account and Payments screens; existing scene controls and payment logic are unchanged. No purchase was made.

AchievementDetailGestures handles horizontal drags on the detail overlay, cycles through the same ordered/deduplicated achievement keys as the list, and dismisses outside-card taps. Vertical drags do not change achievements. Pointer-down resets drag state so a tap after swiping can still close. Collection scrolling and X remain available.

Unity review tests passed: left swipe advances, right swipe returns, outside tap after a swipe dismisses. Rendered side menu/profile/purchase screens inspected. Existing image/progress/achievement validations also run before build. Version target: 1.0.37 (2026091705). See CODEX_HANDOFF.md for installation confirmation.
