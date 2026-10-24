# Online visual polish — awaiting the next device build

User requested a Memotopia graphics pass and explicitly asked to wait afterwards while they connect the iPhone. No iOS/Android player build, installation, Mac source transfer or API deployment is part of this change. Installed iPad release remains1.0.58/2026102402.

- Reuses the existing back arrow, button base, corner treatment, Memory purple and original profile Challenge icon; no new generated artwork.
- Visibility controls sit side by side with integrated ON/OFF labels; selected uses white text on purple.
- Thin purple section bars group Requests, Friends, Online and Blocked, with counts.
- Consistent pale player cards separate names/status from one-word actions. Primary actions are purple; destructive confirmation uses the existing Boxes pink.
- The private declined total moves into a small footer. Consent explanations remain before friend acceptance and in the original confirmation flow.
- Confirmation dialogs use the same heading/action treatment. Network endpoints, invitation decisions, friendship/privacy rules and game rules are unchanged.
- Retains scroll position when the existing polling refreshes the view.

The existing `LiveSocialValidation.Review` exercises actual raycasts for both visibility toggles, acceptance, block confirmation/back and the Online entry point. Preview captures use the actual Unity hierarchy at phone945x2048 and iPad1024x1366 proportions, not a separate mockup. Final preview/test status is recorded with the change commit.

Validation passed with exit0 (`online-polished-preview-review.log`): actual visibility, accept/no, block/back and entry-point raycasts. Phone/iPad previews inspected. Source and editor preview changes only; no device build/install performed.
