# Motion and camera candidate — 2026-09-16

Motion03 implements the owner-authorized next stage on the existing Stage1 scene. Neon09 lighting, track, handling, ship, exhaust and HUD remain unchanged. The original 10.112-second racing clip was sampled at 2 Hz: it supports rear-chase banking and scenery blur; it does not establish a front/rear cut. The look-back interaction is this implementation's design proposal, not a claimed reference reproduction.

## Behavior

- Lower chase position (3.25 m nominal height, 9.2 m plus speed/boost distance), faster positional follow, filtered turn anticipation and partially stabilized road banking.
- Speed-dependent CameraAndObjects motion blur: ordinary 175 km/h cruise now produces about 0.193 intensity rather than remaining near its previous activation threshold. Ship motion vectors retain relative clarity; no custom exclusion mask.
- Hold Tab or controller right-stick press to look back; release to return. A 0.38-second side arc keeps the ship visible. The rejected first overhead path pointed too sharply at the road and was replaced.
- Blur updates after the camera and is suppressed throughout viewpoint transitions plus a short settling interval.
- Existing Reduced Interface Motion preference yields instant viewpoint changes, zero blur, no new camera shake and a stable 70-degree FOV target. Evidence changes this only in memory and never saves preferences.
- Enabled for Stage1 scenes; `-motionBaseline` restores the prior camera/blur behavior for same-build comparison. `-motionViewEvidence` triggers look-back at captured seconds 4–5.5; normal gameplay only responds to input.

## Native delivery and validation

App: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/builds/Vector Rush Motion 03.app`.
Build GUID: `b3ff6a21b92d4d95b1a61a446278d66e`. Scene art revision remains `stage1-neon-09` because the lighting/assets did not change.
Review: `/Users/anping.wang/output/vector-rush-city-light-2026-09-16/motion-03/Motion review.html`.

251/251 EditMode tests passed. A new geometric test originally rejected a -0.0000005 m floating-point endpoint; its tolerance was corrected to 0.001 m and the full suite rerun. No runtime change was needed for that test correction.

All 432 native frames in each baseline/candidate sequence decoded. Racer positions, rotations, visual poses, speed and progress match exactly for every frame; camera poses intentionally differ. Both variants are the same build. Reduced-motion capture records only view states 0/1 and zero blur; all 18 animated transition frames in the regular capture record zero blur. All native capture runs completed successfully. Three native iterations were inspected. The 12-second HUD-visible review footage is 1080p, silent, 24 Hz simulation-time with automated steering, not real-time performance or human driving. Browser playback reached the end for both comparison clips.

Isolated real-time 18-second opening on M4 Max at 1080p under the existing 120 fps cap:

| Variant | Mean ms | P95 ms | P99 ms |
|---|---:|---:|---:|
| Previous camera/blur | 8.333 | 9.089 | 9.300 |
| Motion03 | 8.419 | 9.165 | 15.873 |

Average/P95 remain close, but occasional slower frames remain. This is not a locked 120 fps or isolated-GPU-cost claim. No encoder or screenshots ran during measurement.

## Assessment and limits

The near façades and rail now streak with motion while the ship/HUD remain readable. The lower camera presents the ship more strongly, and the side arc avoids the first version's downward plunge. Reference art, velocity and route differ, so matching its full sensation is not established. Manual handling, physical controller input and whole-circuit camera clearance have not been signed off. The side arc is six metres lateral from the craft and is not an obstacle-aware camera system. Owner artistic acceptance remains open.

## Reproduce

Build the persistent Stage1City scene with ProductionSceneSetup.BuildExperienceCandidate under tools/opening-city-run.py. Use -openingEvidence for candidate and same-build -motionBaseline capture; -motionViewEvidence demonstrates the transition; -motionReducedEvidence -openingStills checks comfort behavior. Measure -openingPerformance separately. The packaging/validation entry is tools/stage1/package_motion_review.py; its evidence paths target Motion03. All source changes are scoped; pre-existing global GraphicsSettings/QualitySettings edits remain outside the milestone.
