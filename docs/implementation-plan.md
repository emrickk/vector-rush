# Vector Rush Implementation Plan

> For agentic workers: use subagent-driven-development with explicit ownership and independent review. User has requested parallel implementation. Do not change another owner's files without coordinating.

**Goal:** A complete original anti-gravity racing slice with Blender sources and verified native gameplay.

**Architecture:** Track frame sampling drives generated course geometry and AI. Fixed-step hover controller drives player physics; race director owns transitions and ordered progress. Presentation reads published state. Parent owns integration, track and Unity setup.

**Tech Stack:** Unity 6000.6.0f1/URP 17.6.0, C#, Blender 5.2, Apple Silicon.

**Spec:** docs/design.md

## Global constraints
- 1 unit = 1 metre; +Y up, +Z forward; namespace VectorRush.
- Exact contracts in design.md; announce required contract changes before edits.
- Actual runtime capture required to claim playability; asset renders are separately labeled.
- Do not install Editor: user is doing that.

## Task 1 — gameplay
- [x] Create Assets/Scripts/Gameplay/{HoverVehicle,RaceDirector,RaceProgress,ChaseCamera}.cs.
- [x] Implement specified public interfaces, suspension via downward raycasts, tunable force/steering, boost resource, AI lookahead, track recovery.
- [x] Implement ordered forward race progress, transitions and restarts. Test reverse crossing, skipped sectors, wrap, duplicate crossing, reset in Assets/Tests/Editor/RaceProgressTests.cs.
- [x] Hand over initialization contract and limitations; parent compiles in Unity and runs a complete race.

## Task 2 — authored assets
- [x] Create SourceAssets/build_assets.py, hero .blend source and FBX export in Assets/Resources/Art. Craft hierarchy faces Unity +Z after import, approximately 6x2x8m; named material slots.
- [x] Render review images in evidence/asset-renders, inspect silhouette/materials and fix concrete issues.
- [x] Report names, orientation, scale and intended material assignments. No Unity scene edits.

## Task 3 — track and integration (parent)
- [x] Create TrackPath and TrackFrame, smooth closed sample path and collision mesh.
- [x] Generate ocean, coastal architecture, track markings, barriers, grandstands, lighting and boost strips.
- [x] Create editor setup that persists scene and URP settings; runtime bootstrap instantiates race entities and presentation.
- [x] Provide shell wrappers for asset export, editor setup and standalone macOS build.

## Task 4 — presentation
- [x] Create Assets/Scripts/Presentation/RaceHUD.cs and RaceAudio.cs reading published gameplay state.
- [x] Draw coherent HUD, title, pause, results; pointer and keyboard controls; scalable typography and settings.
- [x] Engine, boost, collision and countdown feedback; original procedural audio allowed and documented.

## Task 5 — independent review and delivery
- [x] A separate agent reviews actual images plus source for critical race/integration defects.
- [x] Fix highest-impact findings; recapture and re-review up to three meaningful cycles. Initial three review cycles complete, followed by the user-directed modeling pass. Final deck artifact correction and remaining AAA gap are documented.
- [x] Record exact tools, tests, performance and remaining gaps; deliver prompt, sources, native build, archive and controls.

## GitHub milestones
- [x] Preserve integrated source checkpoint and push to private `emrickk/vector-rush`.
- [x] Complete second native race and collect current evidence (112.58 seconds, 3 laps, zero player recoveries).
- [x] Final polish: team paint and grandstand roof improved, rebuilt and recaptured; road artifact diagnostics were inconclusive and are documented.
- [x] Verify native menu/keyboard interaction and record the current 15-second gameplay video.
- [x] Commit final evidence, critique, limitations and delivery documentation; push all completed steps.

## Delivery input check
- [x] Native keyboard Start, Resume, Pause and throttle verified at 1280×800; throttle taps reached21km/h.
- [x] Diagnose unregistered pointer clicks and verify native Start, Resume, Restart and Shake activation at 1280×800.

## Evidence-driven corrections after final review
- [x] Identify large within-quad normal differences and discontinuous banking numerically; implement continuous projected-curvature frames and12-column wide ribbons.
- [x] Rebuild and inspect crest/descent, rerun full race and completed restart launches (run04).
- [x] Gate paused vehicle input and run the paused-recovery regression: 19/19 tests passed.
- [x] Verify corrected pointer mapping at 1280×800 and 1080p, including outside-click non-activation.

## Visual-first modeling and rendering milestones
- [x] Restore URP postprocess resources and emissive shader retention; capture matched native road diagnostics.
- [x] Replace opaque propulsion with additive, fading engine trails and recessed cores.
- [x] Integrate reviewed Kestrel V2 and tower A/B models; capture native title, start, crest and descent (run06).
- [x] Add restrained ambient occlusion and verify contact/material depth; retain matte deck after matched reflection diagnostics.
- [x] Integrate the reviewed textured cliff into connected harbor districts, replacing the old radial geology; inspect native coastline and texture response (run09).
- [x] Preserve final authoring sources, independent critique, current gameplay recording and delivery evidence.

## Night urban circuit revision

The user rejected the daylight prototype's visual quality and asked for a more achievable night road setting, stronger craft effects, and additional iterations. The target is a visibly composed racing scene with readable road and craft, purposeful lighting, material depth and credible architecture. A functioning race or darkened background alone does not close this request.

- [x] First native night pass: replace coastal vista with a built urban district; add patterned window lights, pools of road illumination, matte/damp deck response and revised propulsion.
- [x] Inspect matched title/start/crest/descent captures; fix the three largest visible failures.
- [x] Generate coordinated night-scene reference images before further implementation; preserve prompts and select a practical primary target.
- [ ] Inspect a second native pass and moving footage; refine materials, craft separation and speed presentation. Third pass verifies native atmosphere, varies facades, and develops the two overhead light corridors.
- [ ] Verify the complete race and current performance, preserve critique and record a new preview; package the night build for review.

Ownership: parent road, lighting, materials, rendering settings, integration, camera/HUD and runtime evidence; game_brief NightDistrict and NightWindows; gameplay IonPropulsion and VehicleVFX; independent critic reads actual captures after integration. Existing daytime source/history stays recoverable in Git.

## Ship-first visual iteration

The user explicitly assigned Astra as planner and harsh independent visual reviewer, authorized Blender tooling, and requested repeated visual correction. The user then clarified that exact reference replication is not expected: once the ship's shape is close and coherent, effort should shift to the materials, propulsion, lighting, road and motion that improve the whole game. Scene work is paused only for the current bounded shape pass.

- [x] Astra baseline critique and ranked ship art plan against reference C.
- [x] Preserve V3 form pass 01 and Astra rejection, including nine inspection views, staged export and topology audit.
- [x] Preserve pass 02 source/export, clean reimport audit, completed rear clay and provisional review at the user's wrap-up request.
- [ ] Correct pass 01 compound hull shape, integrated intakes, canopy curvature and supported joints; repeat the same independent form review.
- [x] Calibrate a native neutral inspection rig against recorded origin, orientation and bounds before judging runtime materials.
- [ ] Rebuild primary form and construction in staged Blender source; review neutral renders and loop on visible failures.
- [ ] Develop surface/material depth and readable livery after form acceptance; review and loop.
- [ ] Integrate the reviewed asset and calibrated engine anchors; review native neutral views, ordinary chase views and propulsion.
- [ ] Preserve each reviewed pass, its critique and source/export identity; validate the integrated race and package for review.

A successful compile or a flattering studio frame does not establish visual quality. The reference is directional; minor contour differences must not prevent progress on larger whole-game improvements. Reviewer remains independent from the asset implementation.

Status at wrap-up: work paused by the user. V3 is staged, with only the first rear clay render complete; native app still uses V2. Finish the remaining readiness views before selecting a payload, then focus on surface finish and the whole night scene. The optional anchor/surface preparation added after the calibrated build remains uncompiled and untested in runtime.
