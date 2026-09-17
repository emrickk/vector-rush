# Animated billboard layout correction — September 17, 2026

Native review candidate; owner artistic acceptance remains open. Scene `Assets/Scenes/AnimatedBillboardsStage6.unity`, revision `animated-billboards-stage6-04`, build `668888fd7612468c8a867cc8e3cb65ee`.

## Owner correction and resulting layout

Candidates 01 and 02 were rejected: uniformly placed signs, too much opening density, similar heights, player-facing orientation, harsh flicker and one display per building. The corrected scene has 10 screens in five selected architectural clusters; intervening buildings remain clear. Compositions: high portrait; portrait over a separate scrolling ticker; two orthogonal corner faces; one synchronized video divided over three high panels; low landscape screen with projecting blade and rotating orbital object. Screens use fixed facade orientations and different heights. The native comparison is evidence for review, not artistic sign-off.

Four original Scenario campaigns play actual generated video frames in Unity: portrait movement/blinks, moving illustrated DJ, rotating drink can and orbital product. Video frames are shared GPU atlases with adjacent-frame interpolation. Three campaigns loop over five seconds; ECHO runs forward/back over ten seconds. Titles and tickers form additional layers. Source videos, prompts and provider/job provenance are in `SourceAssets/BillboardsStage6`. References are linked there and are not bundled as game artwork.

Screen/housing depth overlap caused z-fighting in the rejected candidates. Screens now stand 18 cm in front of housings. Fine scanlines were removed; mip-filtered atlas playback limits distant aliasing and tile bleed. Direct road reflections use the corresponding animated frames and timing. Existing city reflection cubemaps remain static. Rain and the dry tunnel behavior are preserved.

## Verification

286/286 Unity EditMode tests passed. Tests compare every collider identity/transform/mesh path, course hash, shelter profile and fog density to Stage 5, and check animation textures and screen/housing separation. Final build captured a complete 43.029-second native lap: 1,009 frames at 23.4375 Hz, with game audio. A separate 35.008-second fixed-camera recording contains 401 native frames across the five compositions. The local browser replay uses JPG frames and recorded timestamps; playback was visually verified. Isolated 1920×1080 three-lap race completed with six finishers and zero recoveries; frozen results, pause and restart passed. Frame delivery: 8.334 ms mean, 9.073 ms P95, 9.292 ms P99, 17.360 ms maximum; 0 frames above 33.3 ms. Stage 5 baseline was 8.335 ms mean / 8.333 ms P99. These are warmed frame-delivery measurements, not GPU timings or human driving validation.

Course: `a43cff0540e9b86cc0d60810b1f19e6cd49ae52d69e41ea43f6fb5b590790abc`. No driving geometry, collision, handling, ship or HUD changes. Game P5 speed blur remains a separate unintegrated branch; this milestone does not claim that work.

[Native opening comparison](opening-comparison.jpg) · [Native compositions](combinations.jpg)

## Reproduce and review

Use `tools/current-game.sh` for the selected current scene after milestone publication. Authoring from Stage 5 is explicit: `VectorRush.Editor.BillboardSceneSetup.PrepareAndBuild`, with fresh absolute build/evidence outputs. It regenerates only this candidate and its generated assets; do not run it over hand-edited candidate work without preserving it.

Local playable build: `/Users/anping.wang/output/vector-rush-billboards-2026-09-17/Billboards04.app`. Review: `/Users/anping.wang/output/vector-rush-billboards-2026-09-17/review/index.html`. Native source captures/builds/logs remain outside Git. Automated steering and fixed-camera evidence do not establish manual driving feel or artistic acceptance.
