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
- [x] Verify the complete race and current performance, preserve critique and record a new preview; package the night build for review.

Ownership: parent road, lighting, materials, rendering settings, integration, camera/HUD and runtime evidence; game_brief NightDistrict and NightWindows; gameplay IonPropulsion and VehicleVFX; independent critic reads actual captures after integration. Existing daytime source/history stays recoverable in Git.

## Ship-first visual iteration

The user explicitly assigned Astra as planner and harsh independent visual reviewer, authorized Blender tooling, and requested repeated visual correction. The user then clarified that exact reference replication is not expected: once the ship's shape is close and coherent, effort should shift to the materials, propulsion, lighting, road and motion that improve the whole game. Scene work is paused only for the current bounded shape pass.

- [x] Astra baseline critique and ranked ship art plan against reference C.
- [x] Preserve V3 form pass 01 and Astra rejection, including nine inspection views, staged export and topology audit.
- [x] Preserve pass 02 source/export, clean reimport audit, completed rear clay and provisional review at the user's wrap-up request.
- [x] Correct pass 01 compound hull shape, integrated intakes, canopy curvature and supported joints; repeat the same independent form review.
- [x] Calibrate a native neutral inspection rig against recorded origin, orientation and bounds before judging runtime materials.
- [x] Establish a coherent primary form in staged Blender source; independent rear/top/side/chase review clears readiness for finish work.
- [x] Develop surface/material depth and readable livery after form acceptance; review and loop.
- [x] Integrate the reviewed asset and calibrated engine anchors; review native neutral views, ordinary chase views and propulsion.
- [x] Preserve each reviewed pass, its critique and source/export identity; validate the integrated race and package for review.

A successful compile or a flattering studio frame does not establish visual quality. The reference is directional; minor contour differences must not prevent progress on larger whole-game improvements. Reviewer remains independent from the asset implementation.

Current status: pass04 ship contact repairs and refreshed maps are integrated. Native reviews accept the targeted geometry and gallery/plume corrections. The combined candidate passed 29 tests and completed three player laps with zero observed recoveries across all racers. A final road-gloss reduction softens the visible sheen; residual diagonal patterning remains. The current 15-second native preview and verified local macOS archive are ready for a playable milestone. Continuous-motion/manual-feel review, city depth and competitive rival pacing remain open.


## Bounded native candidate corrections

- [x] Native matched gallery/plume comparison: motion03 shows readable recessed charcoal construction and attached soft exhaust; preserve independent review 008.
- [x] Integrate pass04 contact correction and refreshed maps, then compare native canopy and nozzle views.
- [x] Validate periodic dampness texture against the matched amber interior.
- [x] Add a conservative rival-only outer-corridor steering guard; compare a full native race with the previously repeated wall stalls. Preserve player controls and collision shape.
- [x] Run current gameplay checks and a real-time performance sample; save a current night preview and versioned local package.


Final bounded material comparison: periodic dampness did not remove diagonal sheen in the native amber interior. Compare a restrained smoothness-mask range of 0.42–0.78 against 0.48–0.93, retaining normals, direct specular, geometry and lighting. Keep the tested combined candidate as evidence; use the reduced-gloss version only if native comparison improves the road without losing the warm lighting rhythm.


Milestone outcome: keep the 0.42–0.78 range after the native comparison. Warm/cool pools remain readable and the bright road pattern is modestly quieter; its underlying cause is not claimed fixed. Archive integrity and the 360-frame, 1080p, 15-second preview are verified. This closes the current playable iteration; it does not award AAA or independent continuous-motion acceptance.


## Throttle-driven exhaust correction

User rejected the always-on flame appearance. Replace speed-driven plume intensity and its idle floor with actual analog throttle demand, strengthened by active boost. Verify trigger press, partial throttle and lift-off through the input path, including extinguishing the plume while the craft is still coasting at speed. Preserve a faint powered nozzle core. Build, capture the native input sequence, and package the corrected app.


Throttle correction complete: 32/32 tests pass. Native virtual-gamepad sequence verifies off, quarter, full, coast release, boost and off-after-boost through normal physics. Response fell below 0.02 in 0.150 seconds at 242 km/h and subsequently reached zero; parent inspected quarter/full/release/boost PNGs. Updated native app is packaged separately from the previous lighting milestone.

## Premium racing HUD pass

- [x] Replace the boxed telemetry with a compact race header, unified speed/boost instrument, live circuit map and contextual lap/boost feedback. Keep the road and craft center clear. Bundle an open-license condensed font for native typography.
- [x] Bring countdown, pause and results into the same visual language while preserving existing input handling.
- [x] Inspect native menu/countdown/racing/boost/pause/results at 16:9, 16:10 and ultrawide; retain independent critique. Run relevant checks, package and push the completed HUD milestone.

Ownership: parent RaceHUD and presentation assets; gameplay HUD evidence harness and its Bootstrap hook; Astra independent native visual critique. Gameplay and vehicle rendering remain outside this step.

HUD milestone complete: 32 existing tests passed; native02 covers 21 actual state/aspect views and a three-lap finish with zero player recoveries. Independent review rejects the first line-scaling implementation and accepts its responsive correction. Pointer Start/Resume/Restart/Quit and outside-click rejection verified at 1280×800. Final timer spacing and bright-gallery contrast are captured in a separate 360-frame native preview. Low-energy and physical-controller coverage remain open; no AAA certification is claimed.


## Previous next-stage path — superseded priority, 2026-09-08

The HUD milestone is complete; the whole game remains below the requested quality target. See [next-stage roadmap](next-stage-roadmap.md) for the evidence, sequence, scope and proposed acceptance gates.

- [x] Conduct a harsh whole-game critique and define the next bounded path.
- [ ] Diagnose opponent pace: compare solo versus traffic, log guard/brake/boost duty and checkpoint times, and observe every driver finish in an evidence mode.
- [ ] Correct demonstrated pacing losses; align engine-load audio with throttle, add readable rival feedback and remove decorative boost-pad ambiguity. Obtain a continuous manual/audible race baseline.
- [ ] Author and independently review one 10–15-second section: final-sector turn, warm gallery and exit skyline reveal. Keep course geometry and player/HUD form stable during comparison.
- [ ] Expand the accepted standard around the existing circuit, verify the full audiovisual race and measure final-build performance.

This checkpoint changes the plan and critique only. Implementation of these new milestones has not started. Prior HUD/ship acceptances remain narrowly scoped; they do not establish AAA quality.


## Active visual-first environment plan — 2026-09-08

User explicitly prioritizes city/track visuals and feeling. This order supersedes the AI-first sequence above. Detailed implementation and acceptance criteria: [visual environment plan](visual-environment-plan.md).

- [x] Write the revised visual-first plan and preserve the prior critique/history.
- [x] Capture five fixed benchmark views and compose one 10–15-second final-sector/gallery/exit passage.
- [ ] Author visible near/middle/far city layers with distinct architecture and an exit landmark.
- [ ] Develop track construction and entry/interior/exit gallery variants with a visible ceiling.
- [ ] Isolate and resolve or deliberately redesign the unstable road-sheen treatment.
- [ ] Integrate surface lighting, depth and speed cues; independently inspect stills and continuously view native motion.
- [ ] Expand the accepted design around the current circuit, profile the exact build and package the visual milestone.

AI, handling, ship-form and HUD redesign work are deferred during this stage. No environment code or assets changed in the planning checkpoint.


## Visual environment implementation — active

User authorized implementation with “go.” First bounded step: lock five existing native chase-camera baseline selections and define the passage composition/asset contract. Parent owns the baseline, WorldBuilder and integration; game_brief inspects and then authors NightDistrict/Blender environment assets; gameplay owns an opt-in matched road-diagnostic harness plus Bootstrap registration; Astra owns independent benchmark direction/review. Environment production follows the baseline contract. Road diagnosis is isolated from art changes. No AI, handling or HUD redesign.

Baseline step complete: unchanged-art native build, 264 frames / five anchors with full pose records, concept provenance, passage contract and first 12-condition road experiment. Native capture completion is not visual acceptance. Next bounded step: integrate the audited transit/service kit, rebuild the gallery and visible deck construction, and run the next isolated road-normal diagnostic. Parent owns WorldBuilder/RoadSurfaceEvidence; gallery author owns NightTrackLighting; city author owns NightDistrict and nominated Blender/live exports; critic owns review files. Validate the native benchmark before expansion.

Candidate01 integrated and captured; city composition fails (station passed before reveal, blank workshop face, dark nearby masses). Preserve that candidate and correct placement/facing with at most four architectural fills, then test the deliberate satin road finish. Do not extend the district kit until the corrected benchmark is reviewed. Native camera drift is reported as a failed exact-match check, not hidden.

Candidate02: corrected composition accepted for further verification in review 005; no expansion acceptance. Preserve its 1080p/16:10 evidence and sampled-sequence defects in review 006. Final bounded correction addresses bearing banking, hatch placement and nearby glazing, then refresh native passage/full-lap/aspect and real-time performance evidence on the exact build. Continuous normal-speed motion acceptance remains explicit and separate from sampled images.
