# Neon lighting and roadside glow — 2026-09-16

Neon09 completes the current lighting candidate for owner review. It retains the darker road, localized cyan/red/warm sign spill, textured sign and city reflections, and scene-specific HDR grading from the interrupted pass. The roadside response had been reduced twice: lower source color plus a multiplicative patch mask that reached zero. The correction restores a nonzero textured cyan/magenta wash while retaining a dark road center. Rail geometry and emissive cores, track, handling, camera, ship and HUD are preserved.

## Delivered evidence

- App: `../../builds/Vector Rush Neon 09.app` (relative to integration workspace).
- Build GUID: `9f6b3726179b48f7b51eec19aa9d9b2e`; revision `stage1-neon-09`.
- Review: `/Users/anping.wang/output/vector-rush-city-light-2026-09-16/neon-09/Neon lighting review.html`.
- Native 1920×1080 footage: 12-second bounded passage plus full 18-second sequence, ordinary HUD, automated steering, 24 Hz simulation-time, silent. Not a real-time benchmark or human-driving result.
- All 249 EditMode tests pass, including saved reflection-capture positions. Native build completed; current native logs contain no shader/compiler exceptions.
- Three anchors match City Light03 camera/racer poses exactly. Across all 432 frames: camera position/FOV, racer positions, path and speed match; maximum non-anchor camera rotation difference is 0.2896 degrees. All motion frames decode successfully.
- Isolated real-time opening on Apple M4 Max, 1080p: mean 8.353 ms, P95 9.146 ms, P99 9.306 ms. Existing 120 fps cap; delivered frame intervals, not isolated GPU timings. Previous Neon07 measured 9.179 ms mean / 16.673 ms P95. This single current run does not establish sustained worst-case performance.
- Same-build HDR/LDR comparison has identical anchor poses and lighting. HDR provides modest shadow/color separation; source lighting and reflection changes make most of the visual difference.

## Visual assessment and limits

Native approach/bend/reveal and the opening frame sequence were inspected. Cyan/magenta road-edge illumination is readable again, and sign reflections retain localized color on the dark road. The supplied neon street reference still has richer surface detail, more irregular architecture and more atmospheric depth. Existing architecture stays more regular; static captures omit racers, and bloom is not volumetric fog. The rail treatment ends at the bounded opening; whole-circuit expansion and camera/blur remain outside this stage. Owner artistic acceptance remains open.

## Reproduction

Generate surfaces with `tools/stage1/make_neon_surfaces.py`; run `Stage1CitySetup.Prepare` and `ProductionSceneSetup.BuildExperienceCandidate` under `tools/opening-city-run.py`. Capture with `-openingEvidence`, optionally `-openingStills -openingHideHud`; compare with `-neonLdrGrade`; measure separately with `-openingPerformance`. Validate with `tools/stage1/validate_delivery.py`; package with `tools/stage1/package_neon_review.py native-09 <output>`.

Earlier global GraphicsSettings/QualitySettings working-tree changes were already present when this task resumed. They are preserved outside this scoped source milestone; the scene carries its own rendering pipeline.
