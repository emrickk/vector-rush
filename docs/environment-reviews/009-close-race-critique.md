# Environment review 009: one bounded close-race polish pass

Independent visual critique, 2026-09-08. **Make two changes, then verify their native result and wrap: expose the nearest rival's real distance in the HUD, and soften the cool gallery's upper light pools.** These address the clearest problems in the current race views without another city rebuild or pace-calibration cycle.

## Evidence and scope

Directly inspected all six original 1920 × 1080 [pace candidate 06 screenshots](../../evidence/pace-candidate-06/pace-evidence.json), at approximately 2, 10, 20, 30, 45 and 60 race seconds. Also directly inspected eight original frames from [the completed visual-before capture](../../evidence/close-race-visual-before/environment-evidence.json): cold-gallery frames **412, 429 and 445**, plus all five anchors **747, 921, 943, 965 and 1004**. Those anchors show the approach, warm-gallery entry, middle, exit and exterior reveal.

Both captures record build GUID `2e193f7e0b434c90889e503ce4668749`, Unity 6000.6.0f1 and Apple M2 Max. The visual-before capture completed at 09:55:11 UTC and contains 1,440 frames covering 60 simulation seconds. Only the eight frames identified above were visually inspected from that sequence. Automated steering, normal physics and a 24 fps simulation capture rate were used; the picture sequence has no audio. This is neither continuous normal-speed viewing nor a performance measurement.

[Pace review 002](../pace-reviews/002-native-pace-comparison.md) records a rival within 60 m for 80.9% of post-launch race time, with zero recoveries, while all close-rival observations are behind the player. This visual review does not independently recalculate those race-wide metrics or certify overtaking.

## 1. Make nearby opposition perceptible

The [2-second view](../../evidence/pace-candidate-06/pack-01-002s.png) shows a rival alongside on the right. The [10-second](../../evidence/pace-candidate-06/pack-02-010s.png), [20-second](../../evidence/pace-candidate-06/pack-03-020s.png), [30-second](../../evidence/pace-candidate-06/pack-04-030s.png), [45-second](../../evidence/pace-candidate-06/pack-05-045s.png) and [60-second](../../evidence/pace-candidate-06/pack-06-060s.png) views chiefly show an empty road ahead. The minimap contains nearby markers, but their small scale gives weak awareness of a pursuer. The pace improvement is consequently easy to miss in the forward camera.

**Recommended change:** add one restrained row beneath the existing position/lap cluster with the actual nearest rival's name, distance and `BEHIND` or `AHEAD`. Use validated along-track race distance, including lap progress, so another course branch or a lapped racer cannot appear falsely close. Exclude finished racers and hide the row when no eligible rival is within the chosen proximity range. Keep it subordinate to position and lap, readable over local scenery, and stable when two rivals have similar gaps.

**Verify:** compare the displayed identity, direction and rounded distance with the native race state in a trailing-rival view and the opening side-by-side view. Check a warm and a cool background and the smallest supported frame shape. This communicates real trailing pressure; actual passes remain unproven.

## 2. Reduce the cool gallery's repeated hotspots

The [second-lap gallery approach](../../evidence/pace-candidate-06/pack-06-060s.png) and first-lap frames [412](../../evidence/close-race-visual-before/selected/frame-0412.png), [429](../../evidence/close-race-visual-before/selected/frame-0429.png) and [445](../../evidence/close-race-visual-before/selected/frame-0445.png) show strong blue-white pools repeated across the upper wall and sloped ceiling. Their bright centers dominate the small fixtures and flatten broad portions of the panels. The ceiling is visible and the route remains readable, so a local adjustment is sufficient.

**Recommended change:** reduce peak intensity and spread the cool upper illumination more gently across the construction. Keep enough light on ribs, ceiling and wall joints, retain the cool exterior identity, and preserve the current [warm-gallery treatment](../../evidence/close-race-visual-before/03-middle.png). No extra lights, global exposure change or new geometry is needed for this pass.

**Verify:** recapture the same cold-gallery progress ranges and check that hotspot contrast decreases while the player, next turn, ribs and ceiling remain readable. Keep the warm-gallery anchor as a regression reference. Stills can establish the local light balance; lamp rhythm in continuous playback remains unreviewed.

## Wrap boundary

The five warm-passage anchors retain the earlier improvements in construction, glazing and route clarity. Repetitive skyline patterns, weak district identity, broad diagonal road bands and the modest station silhouette remain the larger environment limitations documented in [review 008](008-final-environment-art.md). They do not become another expansion project in this final polish round.

After the two targeted changes, perform the focused native checks and deliver the current build and full-lap preview for user playback. Preserve the distinction between a nearby rival and a visible fighting pack, and between sampled frames and continuous motion acceptance. This report records recommendations against the before capture; it does not claim the changes are already verified.

Only this critique document was authored. No runtime source or assets were edited.
