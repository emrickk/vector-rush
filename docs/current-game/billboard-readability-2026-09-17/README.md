# Billboard driving readability — September 17, 2026

Owner found candidate 04's signs mostly outside the driving view and too small. Candidate 05 corrects that layout while retaining actual animation, sparse architectural clusters and screen/housing separation. Artistic acceptance remains open.

Current scene `Assets/Scenes/AnimatedBillboardsStage6.unity`, revision `animated-billboards-stage6-05`, native build `6b98ead89ba04809a6da20de39b5e546`. Local app `/Users/anping.wang/output/vector-rush-billboards-2026-09-17/Billboards05.app`. Updated local driving review: `http://127.0.0.1:8773/review/`. Previous review is preserved at `review-04/`.

## Changes

- Opening AURORA: 16×26 m, moved to the next facade and lowered into the approach view.
- ECHO stack: 18×28 m hero plus 23×4 m ticker; hero lowered 20 m.
- Corner pair: 16×24 m faces; south face now addresses the approach and east face reads while passing the corner.
- Triptych: three 7.8×30 m panels on the visible outer bend, with 1.2 m screen gaps.
- ORBIT landscape: 30×13 m, facing the south approach; projecting blade 12×3.5 m.

Five groups still leave intervening buildings clear. No additional campaigns or runtime video-generation dependencies. Animation, fog/rain, dry tunnel, driving geometry, collisions, handling, HUD and ship are preserved.

## Native verification

286/286 Unity tests passed. Completed 43.029-second native lap with 1,009 frames at 23.4375 Hz and captured game audio. [Matched driving-view comparison](comparison.jpg) shows the opening portrait, lower/larger ECHO and visible front of the landscape display. The native opening also shows the corner pair and triptych as the road bends. Close-up cameras were not used to establish readability in this correction.

The supplemental projection comparison uses the recorded candidate 04 camera/FOV: front-facing sign center inside an inset viewport and projected height over 12 percent. It predicts longer useful exposure for hero screens. It excludes building occlusion, is not a native visibility test, and the height threshold intentionally excludes small ticker/blade accents. The actual native frames remain the visual evidence.

Isolated 1920×1080 three-lap run: six finishers, zero recoveries; results, pause and restart checks passed. Frame delivery: 8.391 ms mean, 8.995 ms P95, 9.301 ms P99, 190.926 ms maximum, 2 frames above 33.3 ms. These are warmed frame-delivery measurements, not GPU timings. No human driving feel or artistic acceptance claim. Prior 01/02 density/flicker and 04 readability objections remain documented; do not treat those layouts as accepted.

## Source and relay state

Reproduce through `VectorRush.Editor.BillboardSceneSetup.PrepareAndBuild` using fresh output/evidence paths, or build the serialized scene using `tools/current-game.sh`. The projection report is a diagnostic, not a build dependency. Source and `.meta` identities remain in this checkout. Existing billboard commit `7d5307e` is local: publication was rejected by automatic approval review and still awaits direct user approval in this task. Do not infer publication approval from the visibility correction. Game P5 blur remains separate and unintegrated.
