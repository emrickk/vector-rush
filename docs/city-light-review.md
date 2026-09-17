# Existing city lighting review — 2026-09-16

The reviewed opening passage uses the existing buildings and signs. Occupancy now spans a 24-floor surface pattern with varied lit, dim and dark suites. Existing facade advertisements have broader graphic fields and sit in the driving view. Existing facade piers and floor bands are slimmer. Sign spill is stronger and the three material-scoped static glass captures are refreshed. There are still 164 authored building footprints, 107 scene renderers and 70 lights, matching Architecture09. No pedestrians, trains or traffic systems were added.

- Playable app: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/builds/Vector Rush City Light 03.app`
- Build GUID: `5629a036f23d4c729ce15415679d210c`. Revision: `stage1-city-light-03`.
- Review: `/Users/anping.wang/output/vector-rush-city-light-2026-09-16/City lighting review.html`
- Main clip: 12 seconds through opening progress .32; full 18-second capture also available. Both native 1920×1080, 24 Hz simulation-time, silent, ordinary HUD, automated normal physics.

## Verification

All 248 existing EditMode tests passed with no failures/skips. Strict 30-model import and native shader/build validation passed. All 432 motion PNGs decoded and were verified. Approach, bend and exit anchor camera position, rotation, FOV and racer positions exactly match Architecture09. Across every frame position/FOV/path/speed/racer position match; maximum non-anchor camera rotation difference is 0.3370 degrees.

Fresh isolated first-lap opening checks on Apple M4 Max at 1920×1080: baseline mean 8.3411 ms, P95 9.0762, P99 9.3112; candidate mean 8.3336 ms, P95 9.0206, P99 9.2440. These delivered intervals are constrained by the existing 120 fps cap and do not measure isolated GPU cost. The legacy openingEnabled flag describes the older procedural opening toggle, not visibility of this persistent Stage1 city.

Three native visual iterations were inspected. The first was too uniformly bright; the delivered version balances occupied suites and quiet glass while keeping the distant city legible. The reference video remains the benchmark. The report media loaded and video playback was checked in-browser.

## Limits

Architecture and signs remain more regular than the reference. Reflections are static local captures and omit moving racers. The preserved road is smoother and more uniformly illuminated than the reference. Full-route art, motion/HUD redesign, a new full-race stability result and human handling are outside this pass. Owner artistic acceptance remains open. No game simulation, route, camera, ship or interface code changed.

## Reproduction

Run `tools/stage1/make_architecture_assets.py`, then the shared lease runner with `VectorRush.Editor.Stage1CitySetup.Prepare`. Build `Assets/Scenes/Stage1City.unity` with `VectorRush.Editor.ProductionSceneSetup.BuildExperienceCandidate`, fresh evidence and a separately named app. Use `-openingEvidence` and optional `-openingStills -openingHideHud` for captures; validate with `tools/stage1/validate_delivery.py` against `artifacts/architecture/native-09`, then package with `tools/stage1/package_city_light_review.py`. Logs/checks are in `artifacts/city-light`. Incidental Unity global-settings writes were restored; no new global rendering settings are part of the milestone.

A duplicate generated `Mesh-4 2.asset` appeared without a meta file during packaging and was preserved outside the Assets tree in `artifacts/city-light/sync-recovery`. An orphaned lease containing `owner 2.txt` was preserved after verifying its process had exited. Neither duplicate is part of the delivered build.
