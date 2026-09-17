# Vector Rush Implementation Plan

## Selected Apex application icon — 2026-09-17

Package the owner-selected option 2, Apex: preserve the approved 1024px artwork, include PNG/ICO/ICNS exports and generation provenance, configure Unity’s default application icon, validate asset linkage and the portable source manifest, and publish the scoped source milestone. No gameplay or scene changes.

## Underground gallery — bounded playable candidate, 2026-09-17

**Paused by owner for public source publication.** Two native candidates were built; the second completed a 43.136-second preview lap. The first iteration passed 271 tests. Final regression rerun, captured-camera validation and final multi-lap performance remain pending; the performance run was stopped on request. Continue from [the current handoff](CURRENT-GAME-HANDOFF.md), not an older scene/plan. No visual acceptance or automatic promotion.

Owner approved the next environment stage after HUD completion. Build a separate `UndergroundGalleryStage2` scene from `EnvironmentStructureStage1`, preserving its source assets and the finished HUD app. The explicit request for a genuinely lower gallery supersedes the historical fixed-elevation constraint. Introduce an opt-in, smooth vertical course profile; update road visuals, collision and course identity together. Enclose the second gallery with substantial city foundations, continuous retaining walls on both approach sides, a thick portal, an enclosed interior and a connected exit. Use the approved `underground-descent.png` as architectural direction, not a promise of raster identity. First audit the existing mesh ownership and course, then implement and test this single section. Validate grade/frame continuity, road/camera clearance, native descent/portal/interior/exit images and a full race with recovery/performance evidence. Do not promote the candidate or claim full-environment completion before owner review. Other districts, billboards and route-wide transition changes remain subsequent work.

## HUD completion — Saira and boost response, 2026-09-17

Finish the approved HUD with real Saira Black/Medium Italic fonts, replacing synthetic font weight/slant/stretch. Add speed-dependent boost vibration and a subtle directional cyan trail to the speed/energy panel only, with smooth onset/settling and immediate Reduced Interface Motion suppression. Preserve environment, handling, other instruments and menus. Test envelope bounds, phase/reset/accessibility behavior and font loading; inspect a fresh native build, actual boost frames/motion and phase/aspect captures. Deliver one finished HUD candidate before returning to environment work.

Completed: Saira and isolated speed-panel boost response; 267 passing tests, 21 phase/aspect screenshots, 120 native motion frames with Reduced Motion suppression verified during actual boost, and three completed laps with zero recoveries. Final build `a42a55aa30f74b808167201dc3136303`. [HUD completion review](hud-a-review.md). Next environment work remains the deep underground gallery/descent and connected approaches; no environment change is included here.

## Owner-approved HUD A implementation — 2026-09-17

Implement selected `environment-hud-concepts-v2/hud-a-arcade.png` at its original visual scale. Reconstruct angled graphite/cyan panels with live position, lap, timer, speed, five energy cells and unlabeled minimap. Preserve menu/input behavior and environment. Keep legacy overlay available for diagnostics. Verify actual native reference comparison, boost fill boundaries, three aspect ratios and normal race states before delivery. Exact raster identity is not promised; owner visual acceptance remains separate.

Implemented and natively iterated. Final build `11d4bbd805824b568b40da78952fc63c`: 263 passing tests, 21 native phase/aspect captures, three completed laps and zero recoveries. [Evidence and remaining visual differences](hud-a-review.md). Environment stages remain unchanged and unfinished; this milestone is the approved HUD only.

## Integrated racing experience execution, 2026-09-15

- [x] Preserve the original `opening-city` checkout and copy its current source state into a new integration repository without Unity caches, local logs, user settings, or native builds.
- [x] Put the shared heavy-work lease at the project level so all four isolated workspaces serialize Unity builds, captures, Blender renders, and performance runs.
- [x] Commit the recoverable integration baseline and create the art, motion/VFX, and UI/audio workspaces from that exact commit.
- [x] Export the authoritative TrackPath course context, publish coordination/contracts, and verify every workspace can read the plan, references, and source.
- [x] Integrate contributor deliveries incrementally into the production scene, with native validation after each batch. Batch A, motion/VFX, UI/audio, and Astra B/C/D source deliveries are retained. The verified scene remains `vrx-a01` because Batch B FBX orientation fails the strict import gate.
- [ ] Deliver the approximately 30-second exemplar, collision supplement, playable app, comparisons, performance evidence, and acceptance matrix; stop for user review before full-circuit expansion.

Current integration checkpoint: Unity 6000.6.0f1 passes 246/246 EditMode tests. Native build `c332adf9551842c698d1afaa4c56d5cb` completes a 42.816-second real-time full-lap preview with 402/402 recorded frames, 12 route anchors, and non-silent stereo audio. The separate real-time performance race completes in 129.17 seconds with all six racers, zero recoveries, and 8.49/9.29/16.68 ms mean/P95/P99 frame delivery. Visual review rejects promotion because most of the circuit remains sparse and the road is too dark despite correcting the white-emission defect. The three-race lifecycle run passes two races, then fails when the third player race does not finish within 220 seconds. Batch B remains blocked by inconsistent FBX orientation, Batch C lacks its 03-owned boost shader binding, and UI presentation is not visually accepted. `NocturneExperience` is therefore not the ordinary-launch default.

## 当前交接入口：完整赛车体验四任务协作

[English packet](plans/2026-09-15-integrated-racing-experience/START_HERE.en.md): complete translations of the four handoffs, startup guide, overall plan, and acceptance criteria. Scope and model assignments are unchanged.

用户要求从统一美术、资产基础和运动体验重新规划，当前只授权计划与交接。[总计划](plans/2026-09-15-integrated-racing-experience/PLAN.md) · [逐项验收](plans/2026-09-15-integrated-racing-experience/ACCEPTANCE.md) · [四任务启动入口](plans/2026-09-15-integrated-racing-experience/START_HERE.md)。最新分工：01 / Sol 负责场景与最终集成，02 / Astra 负责全部美术资产与 Blender，03 / Sol 负责速度和飞船/碰撞特效代码，04 / Sol 负责 UI/声音实现。用户先启动 01，待 WORKSPACES_READY 后启动 02–04；不自动派发。以道路候选 `7358f77` 为待核对运行时起点，保留蓝色尾焰与道路证据及预先改动。先共同完成约 30 秒整体示范段，用户认可后才扩展全赛道与全部既有界面。以下旧条目保留为历史，不是当前执行顺序或模型分工。

**Current owner-approved plan, 2026-09-14:** [One polished, competitive race](plans/2026-09-14-playable-race/README.md). Integration preserved the original checkout and combined the Environment Gallery02 identity correction, Racing's competitive grid and authoritative hooks, Replay's ghost/sector/presentation work, and the shared runtime bridge. Exact production-editor sources missing from the baseline worktree were recovered from the existing GitHub branch only after their blob hashes matched the preserved original index. Focused verification and both complete EditMode runs pass, including 164/164 after the Gallery02 import. A two-setting FBX probe corrected Unity's extra axis bake, and the strict retry imported all 12 FBXs before atomically publishing the Gallery resource. The first candidate exposed a missing runtime attach call, now corrected; a replacement identified candidate and matched `off`/`gallery` review are next. Gallery appearance, full-circuit art rollout, native racing/replay behavior, audiovisual review, performance, and human play remain open. Older status/model/dispatch entries below are historical where they conflict.

**Current concrete execution packet — 2026-09-09:** Owner selected GPT-5.6 Sol for coding/Unity integration and Astra for Blender/modeling. [The detailed worker packet](superpowers/plans/2026-09-09-sol-astra/README.md) defines separate briefs, a versioned asset/data contract, dependencies, full-course delivery and the mandatory owner-first review stop. Planning is complete; the owner started Sol coding separately. Current work is being checkpointed for transfer; Astra has not started. Read [the transfer guide](production-handoffs/START_HERE.md) and its coding checkpoint before resuming. Independent review and corrections do not start automatically after implementation.

**Proposed replacement plan — 2026-09-09:** The owner requested a substantial solution after the global critique. [Nocturne production rebuild](superpowers/plans/2026-09-09-nocturne-production-rebuild.md) covers whole-lap spatial redesign, explicit asset replacement, a saved production scene/lighting workflow, integrated finished racing, mandatory full-circuit rollout and complete player-experience validation. This is a planning deliverable; implementation has not started. The opening04 promotion remains paused. Quality lessons are recorded in AGENTS.md so local technical passes cannot again substitute for the requested outcome.

**Current status — 2026-09-09, after owner rejection:** The owner found opening04 insufficiently different and requested a global critique of the AAA gap. The opening promotion/rollout path is paused; no major visual-quality pass is claimed. The [global assessment](visual-target-reviews/004-global-aaa-assessment.md) records scene-wide art/lighting deficits, experience-validation gaps and the failure of narrowly scoped iteration. An integrated racing-section reset is a recommendation, not an approved new implementation step.

**Prior execution status — 2026-09-09, superseded by owner rejection above:** Owner authorized the [opening finish pass at High effort](superpowers/plans/2026-09-09-opening-finish-pass.md). Separate preview and scoped construction/surface sources are implemented; 42 existing tests pass. Native draft review and bounded corrections are underway. SSR02 is visually rejected and stays off. Original control app remains preserved. This opening pass precedes broader circuit rollout.

**Active next plan — 2026-09-08:** [Night Racing Production Finish](superpowers/plans/2026-09-08-night-production-finish.md), with [design/acceptance spec](superpowers/specs/2026-09-08-night-production-finish-design.md). The owner requested planning toward the supplied night video's finish. Keep the night game and its existing systems; prove the opening-city benchmark before wider rollout. This documentation milestone is complete; implementation has not started. Earlier sections below preserve the completed work and historical directions, rather than overriding the new plan.

> Historical worker instruction, superseded by the current four-task packet: earlier work used subagents and independent review. The current owner starts tasks personally; do not revive automatic dispatch or review. Preserve explicit file ownership.

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
- [x] Author the benchmark city layers and exit transit structure; landmark identity remains a documented weakness.
- [x] Develop track construction and both galleries with visible ceilings; corrective still views pass.
- [x] Run isolated road diagnostics and deliberately redesign the satin finish; residual bands remain.
- [ ] Integrate surface lighting, depth and speed cues; independently inspect stills and continuously view native motion.
- [ ] Expand the accepted design around the current circuit, profile the exact build and package the visual milestone.

AI, handling, ship-form and HUD redesign work are deferred during this stage. No environment code or assets changed in the planning checkpoint.


## Visual environment implementation — active

User authorized implementation with “go.” First bounded step: lock five existing native chase-camera baseline selections and define the passage composition/asset contract. Parent owns the baseline, WorldBuilder and integration; game_brief inspects and then authors NightDistrict/Blender environment assets; gameplay owns an opt-in matched road-diagnostic harness plus Bootstrap registration; Astra owns independent benchmark direction/review. Environment production follows the baseline contract. Road diagnosis is isolated from art changes. No AI, handling or HUD redesign.

Baseline step complete: unchanged-art native build, 264 frames / five anchors with full pose records, concept provenance, passage contract and first 12-condition road experiment. Native capture completion is not visual acceptance. Next bounded step: integrate the audited transit/service kit, rebuild the gallery and visible deck construction, and run the next isolated road-normal diagnostic. Parent owns WorldBuilder/RoadSurfaceEvidence; gallery author owns NightTrackLighting; city author owns NightDistrict and nominated Blender/live exports; critic owns review files. Validate the native benchmark before expansion.

Candidate01 integrated and captured; city composition fails (station passed before reveal, blank workshop face, dark nearby masses). Preserve that candidate and correct placement/facing with at most four architectural fills, then test the deliberate satin road finish. Do not extend the district kit until the corrected benchmark is reviewed. Native camera drift is reported as a failed exact-match check, not hidden.

Candidate02: corrected composition accepted for further verification in review 005; no expansion acceptance. Preserve its 1080p/16:10 evidence and sampled-sequence defects in review 006. Final bounded correction addresses bearing banking, hatch placement and nearby glazing, then refresh native passage/full-lap/aspect and real-time performance evidence on the exact build. Continuous normal-speed motion acceptance remains explicit and separate from sampled images.

Final bounded correction complete: glazing finish, bank-aligned bearing and single-panel hatch passed inspected native stills (008). Capture instrumentation was corrected after preserving its first full-lap anchor failure. Final build GUID 153b77ea7d53463b8fc11a0ddae232be supplies the 42-second full-circuit preview, 11-second benchmark extract and five valid anchors at each of 1080p, 16:10 and ultrawide.

- [x] Author near/middle/far benchmark composition and transit/service architecture; preserve two rejected/corrective stages.
- [x] Rebuild both galleries and continuous deck construction; verify the bearing/hatch corrective views.
- [x] Perform isolated road experiments and adopt deliberate satin finish. Broad residual shading bands remain; root cause and motion stability are not accepted.
- [x] Capture a complete current circuit and package the native review build with source/build manifests.
- [ ] Obtain normal-speed subjective playback acceptance; verify crowded gallery/opponent readability.
- [ ] Expand the accepted city design across the remaining circuit after that gate.

Next bounded work depends on playback findings: address a demonstrated visual defect or extend the accepted city families. Do not multiply assets while treating sampled-frame review as motion acceptance.

Final real-time native race/performance gate complete: 3 player laps, zero player recoveries, restart/countdown checks pass; P95/P99 15.14/15.99 ms versus baseline16.67/17.01 ms. The macOS archive passed integrity checks. Playback question remains pending; absence of a reply is not acceptance for city expansion.

## Final close-racing and visual pass — user request 2026-09-08

User requests slowing the player or increasing rival pace so they race together, then one further visual critique/improvement round, a final wrap-up and all work pushed to GitHub. This supersedes the pending playback-only handoff. First measure pack separation and driver losses, then make a bounded pace adjustment through normal vehicle physics, preserving collision/edge safety. Native acceptance: recurring nearby rivals beyond the start, closer completed-race progress, and no new recovery/stall regression. Parent owns driver tuning/integration; pace author owns PaceEvidence; diagnostic reviewer initially reads only. After closer traffic is demonstrated, independent visual critique selects one bounded polish pass on actual native imagery. Finish with current preview, tests, race/performance checks, archive and published corrective history.

Pace diagnosis from the previous full-lap poses: no rival within 60 m after 5 s; fastest rival averages 164.7 km/h versus player 179.1 km/h, with other rivals 130.5–155.1 km/h. Rival-only edge guard is a demonstrated source-level 42 m/s cap; pose-derived guard exposure is an estimate until native instrumentation confirms it. First candidate will use actual player CruiseSpeed 60 m/s and BoostSpeed 82 m/s (previously 78/108), preserving acceleration, AI trajectories, collisions and safety limits. Baseline/candidate telemetry will decide acceptance; no pose synchronization or hidden teleporting.

Candidate01 (player60/82) completed with no recoveries but failed the intended proximity/finish gate. Next bounded candidate: actual player55/75, keeping all other driver/physics settings fixed. Preserve candidate01 source and native evidence as a separate corrective checkpoint.

Candidate02 (55/75) fails sustained closeness despite a close first lap. Next bounded controller experiment: restore player60/82 for comparison with candidate01, replace fixed angle-only steering gain with speed/chord-aware pursuit yaw, add known-radius regression tests and resolved-lane telemetry. Retain all collision, following, guard and yaw limits. Native acceptance must include actual guard-duty reduction, sustained proximity and recovery/stall checks before visual polish.

Candidate03 validates pursuit math but does not resolve wall-contact lock. Before another pace adjustment, address the demonstrated contact mechanism with bounded guard-only lateral acceleration toward the already-resolved safe target. Add inward-direction/player-exclusion/blocked-target regression coverage and verify native guard duty, race proximity and recoveries. No more cap-only tuning.

Candidate04 closes sustained outer-wall contact, but player60/82 remains slightly faster than the improved rivals. Final bounded pace fit:55/75 with pursuit and guard assistance retained. Compare all three laps, gallery proximity and actual finish observations; then perform the single requested visual critique/improvement pass and wrap up.

Candidate05 establishes frequent close racing, but nearest finish gap remains160m. Final small pace calibration: player53/72 with corrected controller unchanged. Run the same race, then proceed with the single requested visual critique/polish and final delivery; preserve measured limits rather than invent an overtaking/finish pass.

Candidate06 is the final pace preset:53/72. Validated-progress proximity within60m is80.9% overall and86.2/77.7/79.2% across laps; both galleries100%; all six racers have zero recoveries. Nearest finish gap100.85m narrowly misses the earlier100m target; no overtaking is proved. Accept sustained proximity for the user request and stop tuning. Next bounded step is one independent native visual critique and its selected targeted improvements, followed by final native validation and distribution.

Visual critique009 selects two final corrections: a compact truthful nearest-rival HUD cue beneath the position panel, and softer cool-gallery surface wash. Parent owns NightTrackLighting; HUD author owns RaceHUD. Preserve warm lighting and all race tuning. Verify a new native full lap, both gallery stills, HUD aspect views, all tests and a separate real-time race before release.

### Requested wrap completed

Final close-racing preset and both review 009 visual corrections are implemented. Final build 7da2883ade4f4223b7a611db8cf60eb6 reproduces candidate 06 racer states exactly; all 42 tests pass. Actual native full-lap and three aspect sets are captured and checked; the separate real-time three-lap race finishes in 128.32 s with zero recoveries for all racers. Both restart launches and three countdown-pause checks pass. P95/P99 frame intervals 9.22/9.32 ms; the comparison has changed race pace and is not an identical-trajectory benchmark. Archive CRC passes. All source/selected evidence is pushed. The native archive is ready locally; versioned GitHub release publication requires separate authorization after automatic approval review rejected that action. The user-requested bounded pass is finished; broad city expansion and subjective driving/motion assessment remain future work, not blockers to this wrap.

## Landmark composition and road finish — user direction 2026-09-08

The user explicitly selects distinctive landmarks and better composition immediately beside the track, followed by road finish. Implement in that order. First retain the current native full-lap as baseline; author two original landmark families for the first and middle sectors, with lower service architecture establishing parallax and framing the road. Reserve their footprints so procedural blocks cannot bury them; subordinate the far skyline through restrained windows and a more deliberate height rhythm. Preserve final-sector station/gallery composition and race tuning. Asset author owns new environment-v4 Blender/FBX resources and NightLandmarks; parent owns NightDistrict integration and later WorldBuilder/road materials; critic owns independent direction/native review; road investigator reads prior experiments before proposing one bounded experiment.

Complete the landmark stage with actual native full-lap views and independent critique before adopting a road-finish correction. Then verify combined native visuals, race/restart behavior and performance, refresh local build/previews, and commit/push each completed step. Release publication remains outside this request.

Landmark candidate 01 is preserved with independent review 012. Next bounded correction: remove only the BenchmarkSkyline local placement (−226,0,467), whose world position crowds the opening mast. Verify the corrected native mast approach and retained station before the targeted road-fixture shadow experiment. No second asset cycle or race tuning.

The landmark correction is visible in native candidate02: the competing slab is gone and the two authored families remain. Next bounded road step: identical-pose A/B/A changes only the three road-fixture renderer shadow modes at .94585. Adopt only a demonstrated caster contribution; preserve road material, geometry, lighting and racer tuning. Finish with combined native views and standalone race/performance verification.

Fixture A/B/A is positive: the L-shaped exterior band disappears when only the three fixture renderer casters are off and returns after restoration. Native metadata holds all101 lights, camera, road material and mesh/collider identity fixed; restoration passes. Adopt shadowCastingMode.Off only for road steelwork and cool/amber diffuser groups. Preserve the satin finish, road geometry, other shadows and race physics. Final combined build is now under native full-circuit, regression and real-time race/performance checks.

### Landmark and road update delivered

Two authored landmarks, reserved roadside composition, receding skyline and the mast's slab correction are complete; independent reviews012/013 preserve the candidate and acceptance. Native fixture A/B/A establishes the selected road-shadow cause, and the limited caster correction passes the documented final sampled sequence. Final build b709ef36660443b7bdec08170d00b8df has42 passing tests, a complete native circuit and two alternate-aspect station sets. Two real-time races each finish three laps in128.32s with all six racers at zero recoveries and all restart/countdown checks passing. The repeat P95/P99 is9.23/9.33ms; first-run long intervals are preserved with undetermined cause. Source/app/archive hashes and ZIP integrity pass. Current24-second landmark preview,42.75-second full lap and local macOS archive are packaged. Source/assets/selected evidence and corrective history are pushed at the final delivery commit. No release publication or continuous subjective-motion acceptance is claimed. This requested implementation step is closed; broad finish limitations remain documented.

## Lighting and material depth — user approved

The user selects the proposed lighting/material pass. Improve the visible landmark foundations and service elevations, differentiate concrete/ceramic/metal through restrained surface response, and reveal local structural depth against the skyline. Parent owns NightLandmarks integration and local lighting, plus any targeted NightDistrict/WorldBuilder adjustment. Finish author owns an isolated environment-v5 material kit and helper; independent critic owns direction and native critique; technical reviewer reads light/material/fog behavior. Preserve the current native landmark views as baseline. Implement one candidate, review actual full-circuit images, make bounded corrections if demonstrated, then verify the exact final native build, race/performance and package/push source,assets,evidence. No race tuning is part of this pass.

Owner clarification after the account-switch coordination message: continue working until this lighting/material-depth pass is complete. This explicitly supersedes the temporary pause. Resume implementation, native critique and final delivery under the existing approved scope.

Native candidate01 is accepted in independent review016 with no required corrections. Preserve the exact build GUID5c49b92643bd4d41820ba81857fdf8c7 for final delivery. Thermal recesses and supported pipework improve clearly; mast underside separation is modest and its amber pane remains flat. Nearby camera comparisons are explicitly not exact matches. Next bounded step: final existing tests, alternate-aspect context views, separate real-time race/performance, preview encoding, archive integrity and published source/evidence. No additional runtime edits are planned.

Final existing tests, full-lap/preview decoder validation, source/app hashes and archive integrity pass. Completion is blocked only on the locked Mac display: fresh alternate-aspect native captures and separate real-time race/performance remain. Preserve the incomplete no-frame attempt. Resume those checks after manual unlock; do not rebuild or reopen accepted art work without a demonstrated defect.

### Lighting and material depth delivered

The display blocker is resolved. The unchanged accepted build completes fresh16:10/ultrawide station captures and the separate real-time race: three laps in128.32s, zero recoveries for all six racers, both restart launches and all three countdown-pause checks passed. Frame intervals average8.35ms/P959.21/P999.33, consistent with the prior accepted repeat. All42 tests,1,450 original PNG checks, video decoding and source/app/archive hashes pass. The local build,24-second lighting preview and42.75-second lap are current. Source, original finish assets and selected native evidence are delivered through published milestone history. This requested lighting/material step is complete; no release publication or subjective continuous-motion acceptance is implied.

## Comparative visual review — coastal versus current night build

User reports that the earlier bright coastal version feels more AAA-like and requests a review. Review only: compare representative actual native coastal/current views, preserve an independent visual critique, inspect source evolution for supported causes, and distinguish local correctness from overall visual quality. Do not alter the runtime, art assets or current build. Deliver a candid prioritized diagnosis and one proposed direction experiment; implementation awaits a subsequent user instruction.

Comparative review complete in independent017 and parent018, with an unaltered coastal/current image comparison. Both reviews find stronger overall image hierarchy, visible grounding and identity in the coastal build; current HUD, gallery and service structure improvements remain useful. Source inspection confirms unchanged chase-camera code and a changed broad lighting environment. Recommended next experiment is one current-system coastal lighting/composition passage, or a deliberately restructured night alternative if the owner retains that direction. This is review output only; no implementation step has been started.

## Production night finish — planning milestone

The owner supplied a polished night-racing video, confirmed it represents the same concept at a higher level of finish, and requested a plan toward that standard. This supersedes the proposed coastal-restoration experiment. No game, shader, asset or build change is part of this planning step.

- [x] Preserve a private-source-safe reference brief and define the intended night experience.
- [x] Write the design/spec and an eight-task implementation plan with exact existing component ownership and native validation commands.
- [x] Preserve independent planning critique 019 and incorporate its whole-image, connected-world, material, racing, watched/manual and effects-off gates.
- [x] Set one ordinary opening passage and five natural progress crossings as the benchmark, including the current slowdown and thermal/station control views.
- [x] Update project entry points and retain previous plans as history.
- [ ] Execute Tasks 1–8 in the linked production finish plan. First deliverable: baseline/acceptance record and the bounded lighting/road candidate; full-circuit expansion waits for the integrated benchmark gate.

The new plan permits documented global lighting/material experiments in its environment stages and later bounded motion/racing work. Prior step-specific locks applied to their completed milestones. At the planning milestone, the accepted native build was GUID `5c49b92643bd4d41820ba81857fdf8c7`; the current paused checkpoint is identified in [HANDOFF.md](HANDOFF.md).

## Stage 1 execution — lighting and road response

The owner authorizes Stage 1 from the six-stage summary: lighting and road response, including prerequisite benchmark capture/acceptance definition (Tasks 1–2 of the detailed production plan). Work continues in the existing clean `build/first-playable` checkout so current native evidence, Unity import state and authoring paths remain usable. Parent owns the integrated world/light/material edits; independent critic owns baseline/native visual reports, and technical reviewer audits rendering behavior read-only. Geometry, race/AI, camera and craft surface payloads remain fixed for this stage. Preserve the baseline, one direct-light candidate and the controlled reflection variant before selecting the final road response. Later city, craft and race work is outside this bounded stage.

Task 1 baseline is complete: eight original-image selections, natural-crossing metadata, schematic lighting layout, independently inspected review020 and a fully decoded 15-second native clip. Fresh real-time baseline completes three laps in128.32s; mean8.35/P959.18/P999.31ms. Its conditions and evidence limits are retained. Proceed to the controlled lighting/road candidate (Task2).

Task2 preserves A (modest broad-light improvement) and B (rejected darker road). Native diagnostics confirm a fog color-space mismatch; candidate C corrects the upload while preserving B lighting/materials. Next bounded experiment: two one-shot local256px reflection probes on the opening bends, with native verification of clustered blending and box projection. Roughness stays fixed for the probe comparison; source-linked response and global controls decide whether to retain it.

## Owner wrap-up — 2026-09-08

Work is paused. Task 1 is complete; Task 2 is partially delivered and its production road-response acceptance remains open. Preserve A/B/C/D and independent review021. The checkpoint combines A's direct-light/readable reflection-off road with C's accepted fog correction. Remove D's local probes and pipeline toggles. Do not initiate the roughness experiment, SSR, later city/craft/racing stages, releases or merges during wrap-up. Build and inspect the local checkpoint, record its exact limited validation in [HANDOFF.md](HANDOFF.md), push all completed work, then stop. Further work requires the owner to resume it.

## Detailed retrospective documentation — 2026-09-08

The owner requested a comprehensive record of the whole project, including inspected images, findings, concrete Python/Blender/code operations, verification and rejected attempts. Compile the source-linked record through checkpoint `3097bc8` from the 60 recorded commits, authoring recipes, native reports and independent critiques. This is documentation work; it does not restart game builds or asset experiments. Deliver one navigable complete document with the original evidence linked and the latest checkpoint limits explicit. The owner has also asked for the next development plan: a bounded road-response investigation with the accepted atmosphere fixed, following the record delivery.

## Resumed road-response comparison — user approved

The owner says “continue” after the proposed next step. Finish and publish the complete retrospective, then execute [next-road-pass.md](next-road-pass.md): capture the combined checkpoint as the actual control, inspect the continuous opening passage, make one effective-smoothness comparison with accepted atmosphere and reflection state fixed, obtain native comparison/independent verdict and verify any retained result. If it fails, preserve the negative result and reassess technique. No later city/craft/racing stage is included.

### Bounded comparison completed

The fresh control and candidate each completed 1,440 native 1080p frames, eight original selections and a fully decoded 15-second silent simulation-time clip. Candidate 01 failed the useful whole-frame gain gate in parent and independent review004. Preserve its source at `a5debfb`, restore the exact control runtime/app and keep the validator correction that detects unexpected resolution changes. Continuous watched-motion acceptance remains open. [Decision](../evidence/night-production/road-response/decision.md).

The [feasibility report](road-reviews/005-reflection-feasibility.md) proposes one isolated opt-in preview-SSR prototype on the existing URP 17.6 package, beginning with compile/type/resource/pass checks. No new reflection implementation or later city/craft/racing stage is included in this completed comparison. Stage 1 remains incomplete. Current technical checks belong to the restored app and are recorded in [HANDOFF.md](HANDOFF.md).

## Consistent visual references and gap evaluation — user requested

The owner requests a new consistent set of generated reference images, followed by evaluation against the current game. Define one shared night art direction and generate four views anchored to existing native evidence: opening city bend, amber gallery, thermal passage, and a separate close craft-material study. Preserve the approved craft's primary design and the chase views' route/framing relationships. Reuse the first generated view as a visual reference for the other views, with their native images as composition authorities.

Inspect the generated images for internal consistency and for deviations from the intended invariants. Deliver the original generated files, exact prompts/input provenance, native-versus-target comparisons, and a prioritized gap assessment with observable acceptance criteria. Label generated imagery as concept targets and distinguish appearance goals from unverified rendering techniques, performance, motion and playability. This step concerns reference assets and documentation; runtime implementation, including the proposed SSR prototype, remains a separate step.

Completed: four original 1672 × 941 PNGs in [Nocturne V2](../references/nocturne-v2/README.md), using A as the common appearance input to B/C/D; original native comparisons and [an interactive reading page](visual-targets-v2.html); exact prompts, hashes, source poses, consistency critique, prioritized gaps and observable native criteria. All eight comparison PNGs pass integrity checks and local links resolve. The opening comparison and enlarged-target control were inspected in the browser. D's older map/control identity is explicit. No runtime, native build, imported art, or app changed; no new game test or independent art verdict is claimed.

## Nocturne V2 implementation plan

The reference-aligned [implementation plan](superpowers/plans/2026-09-08-nocturne-v2-execution.md) is prepared: isolated reflection feasibility, road/light finish, connected city, architectural finish, craft materials, integrated play validation, then circuit rollout/delivery. It preserves the restored control and starts with a separately named preview app. The isolated SSR experiment is implemented; production acceptance and later milestones remain open.

### Active SSR02 correction after SSR01 availability rejection

SSR01 source and full failed native evidence are preserved at6c0e1a6. [Verdict and bounded proposal](visual-target-reviews/001-reflection-feasibility.md): restrict the installed preview feature to Game cameras and enable supported managed instrumentation in a separately named SSR02 build, preserving probe/world/material/physics inputs. Task1 remains in progress; later visual milestones remain pending.

### SSR02 result — visual feasibility rejected

Camera-scoped SSR02 source and native evidence are preserved at `dd7a417`. Both captures complete with intact images; positive CPU pass samples establish execution but do not prove GPU output. Independent review of all nine pairs finds no convincing whole-frame reflection gain. [Comparison](ssr02-native-comparison.html) · [verdict](visual-target-reviews/001-reflection-feasibility.md) · [bounded alternative proposal](visual-target-reviews/001h-next-technique-proposal.md). The exact original app remains retained. Motion, GPU-buffer and performance gates are not passed; Tasks 2–7 have not begun.

### Owner-requested parallel critique — proposed next milestone

The owner finds the SSR difference almost negligible and requested independent critics plus an improvement approach. Three separate [visual](visual-target-reviews/002a-improvement-visual-critique.md), [rendering](visual-target-reviews/002b-improvement-rendering-critique.md), and [environment](visual-target-reviews/002c-improvement-environment-critique.md) reports support parking SSR and reject a lamp-origin/aim tweak as the next headline finish result. [Integrator synthesis](visual-target-reviews/002-improvement-approach.md) recommends combining the bounded opening portions of Tasks 2–3 into one finished-passage milestone, retaining component snapshots and using a small warm-gallery surface study. Audit existing asset visibility first, reuse/reposition, then author only missing support/service connections and their local light. This is a proposed plan revision; no new runtime implementation occurred in the critique task.

### Active opening finish pass

Owner authorized the next pass at High effort. Execute [the bounded opening plan](superpowers/plans/2026-09-09-opening-finish-pass.md): separate default-off preview controls, connected opening construction, coordinated road/gallery surface-light study, then independent native review and isolated performance. SSR is parked. Original app remains retained. No full-circuit rollout or craft redesign belongs to this pass.

## Opening-city assignment — 2026-09-14

Isolated from e243303. Next bounded step: implement large opening-city architecture and connected supports, then inspect an early native view. Scope and fixed 18-second passage: [opening-city.md](opening-city.md).

## Owner-requested Stage 1 — 2026-09-15

The owner requested parallel subagents and a stop/report after Stage 1. Stage 1 delivers a native opening-city candidate from the committed landmark, support and reveal corrections at `88d6399`, with any necessary bounded correction before delivery. Capture the current source before changing art again. One scene owner handles the complete composition and native integration; independent workers inspect visual quality and source/evidence correctness.

Keep track, handling, camera, craft and HUD fixed. Verify the landmark remains legible through the bend, support connections convey height, and the lower city strengthens the reveal without competing with the racing route. Produce a separately named playable app, three natural before/after views and continuous 18-second simulation-time captures with actual pose comparison. Record technical checks separately from visual judgment and human play. Stop for the owner after the Stage 1 report; gallery rollout, race tuning and later stages remain outside this step.

Stage 1 is now owner-review ready: [result, evidence and remaining limits](stage1-opening-review.md). The committed art corrections are visible in the fresh named app. Complete native A/B capture passes independent validation; the visual verdict is progress in composition/construction, with material and lighting finish still open. Stage 2 has not started.

## Stage 2 planning: propulsion and all player interfaces

The owner requested a plan that adds the HUD and all interfaces to the proposed propulsion work. [Stage 2 plan](stage2-propulsion-and-interface-plan.md) covers shared visual direction, a playable propulsion/HUD milestone, the complete menu/settings/results flow and integrated verification. This entry records planning only; no runtime implementation or later stage has started.

The owner then authorized stronger HUD references based on the existing game. The [2A interface reference set](../references/interface-stage2a/README.md) now contains six editable screen designs over clean native backgrounds, an interactive comparison and full-resolution PNGs. The only Unity change is an opt-in diagnostic HUD-hide flag; the production HUD and propulsion remain unchanged. Propulsion-shape reference and native 2B implementation remain open.

## Active Stage 2 implementation

The owner now authorizes completion of 2A–2D without intermediate stops. Implement the approved interface references and stronger state-driven propulsion in parallel, with one owner for all UI, one for propulsion and one for diagnostic evidence. Integration owns narrow gameplay lifecycle/record-read contracts, shared builds and independent native inspection. Preserve all Stage 1 geometry, driving rules, camera and original apps. Deliver a fresh named app, complete interface and propulsion recordings, resolution/input/persistence checks and isolated performance comparison, then stop before Stage 3.


## Screenshot-based interface references — Scenario

The owner rejects the current HUD/menu finish and freezes feature additions. The next bounded step is four generated visual concepts from the current native race, boost, title and pause screenshots, using the explicitly requested Scenario API. Keep the existing content and actions as the source of truth; emphasize typography, surface depth, selection states and boost energy. Use the race/title concepts as style references for boost/pause. Inspect and deliver the images with source comparisons, exact prompts and provenance. These are art-direction references only; do not modify or rebuild the game in this step.

Completed: four inspected 2048 × 1152 Scenario concepts, exact native inputs, full prompts and job/hash provenance, plus a browser-checked current/concept gallery in [the reference pack](../references/interface-polish-scenario-v1/README.md). The critique records generation deviations and distinguishes visual direction from implementation. No game code or build changed. Owner review of this direction remains open.


## Item 1: exhaust jet and curved trail polish

Owner authorizes item 1 only. Preserve current Stage 2 exhaust as a same-binary comparison. Replace crossed-sheet main plumes with camera-facing soft jets and bounded world-space curved trail history. Retain existing throttle/boost envelopes, nozzle anchors, pause/recovery/menu lifecycle and physics. Native normal/boost/release footage and focused lifecycle checks must precede completion. Road, camera, HUD and city are outside this step.

Owner rejects the ribbon prototype: it reads as a flat glowing texture. Item 1 remains open. Replace the ribbon with independent world-space plume particles, volumetric dispersion and short-lived irregular hot fragments; preserve the existing boost state authority and no-feature scope.


## Blue exhaust revision — 2026-09-15

Owner resumes item 1. Preserve the paused blue source in artifacts/exhaust-blue-v2/baseline-source. Inspect the supplied video sequence, then replace the main crossed sheets with layered animated particles, keep a compact core and a subordinate world-space wake. Add near-camera and depth fades. Iterate in the live Editor before producing matched acceleration/boost/turn/release footage, then verify lifecycle and performance. Keep other presentation and gameplay fixed. The reference holographic phase is a separate effect, not evidence of boost timing.

Completed first reviewable blue-particle candidate: [implementation, matched video, tests and performance](blue-exhaust-review.md). Owner visual acceptance remains open. Later items have not started.


## Planned next task: opening-road light response

Owner requests a plan and a handoff for a new GPT-5.6 Sol conversation. [Bounded technical plan](plans/2026-09-15-road-light-response/PLAN.md) and [self-contained handoff](plans/2026-09-15-road-light-response/HANDOFF.md) cover one opening bend, verified material/light inputs, evidence-gated technique selection, matched native footage and separate performance checks. Preserve the blue exhaust and other presentation/gameplay. Planning only; road implementation has not started in this conversation.

### Opening-road review candidate complete

The bounded passage now uses the existing overhead fixtures to cast alternating cyan and amber pools across the banked road. Ordinary launch enables the candidate; `-vrRoadLightBaseline` restores only the prior fixture setup and retains the layered blue exhaust. The 18-second same-build comparison has exact recorded vehicle/camera/input/FOV parity across 432 frames per variant. The native app, three still pairs, video, 233/233 EditMode result and separate real-time A/B measurements are recorded in [the review note](road-light-response-review.md). Stop for owner review; full-circuit rollout and artistic acceptance remain open.

## Active owner-authorized Stage 1 reference match — 2026-09-15

Latest owner instruction assigns all game work to GPT-6 Astra and authorizes execution. This supersedes the historical multi-model lanes above. Build one native playable banked city passage approaching the two liked Scenario references; preserve the original app, track and handling. Deliver fixed repeatable chase-camera stills, honest reference/native comparison, and a short native moving clip. Stage 2 motion redesign and whole-track expansion remain outside scope.

Bounded steps: (1) correct Batch B source FBX orientation and verify unchanged declared bounds/material slots, (2) author a separate persistent Stage 1 scene with varied overlapping city layers, connected rail/infrastructure and ads, localized cyan/magenta road response, depth and ship/exhaust finish, (3) visually inspect and iterate in native game, (4) run relevant tests and isolated performance/playability checks, capture and document remaining visual gaps. Technical verification does not confer artistic acceptance.

### Stage 1 reference candidate delivered for owner review

Source repair is recorded at `c0e552c`; native scene/ship presentation milestone at `f1fb7fd`. The separate playable app is `Vector Rush Stage1 City 05.app`, build GUID `7cba6941ae2a469b83a9c807ba5ac957`. All 248 tests pass. The three selected native before/after camera poses match exactly; complete capture metadata, an 8.54-second HUD-visible clip, full 18-second capture, isolated opening performance and a complete real-time six-finisher race are verified. Full-race player time is 129.166 seconds with zero recoveries and a valid post-finish pause/result. [Review, reproduction details and remaining gaps](stage1-reference-match-review.md).

Stage 1 implementation and its review package are complete. Owner artistic acceptance remains open. Near facade/material richness, full dynamic city reflections, later motion/UI treatment and complete circuit art remain outside this delivered candidate. The historical third-consecutive-race timeout was not revalidated; the new native check covers one full race. Stop here before Stage 2.


## Building architecture pass — 2026-09-16

Owner authorized execution after comparing both selected Scenario concepts with Stage1 City05. Preserve track geometry, road treatment, handling, ship and camera. Rebuild the complete opening approach/bend/reveal city composition, replace repeated small-window textures with three distinct architectural families, add recessed facades, controlled glass response and selective integrated signage. Fill the empty reveal with bounded lower-city and skyline backdrop. Do not expand to the full circuit or Stage2.

Deliver a separate playable native app, exact-pose before/after stills for all three anchors, a HUD-visible native motion sequence, technical checks and an explicit visual assessment against the concepts. Iterate native appearance before marking the pass ready for owner review.

### Building-pass result

Architecture09 is ready for owner review after native iterations, 248 passing tests, exact-anchor captures, a verified 8.96-second native clip, isolated opening performance and one complete six-finisher race. See [city architecture review](city-architecture-review.md). The full concept finish and owner artistic acceptance are not claimed. No Stage2 or full-circuit expansion was performed.


## Existing city illumination pass — 2026-09-16

Owner authorizes improving the existing buildings, signage, lighting and reflection composition against the original racing video. No pedestrians, trains, traffic systems, new building types or route expansion. Preserve Architecture09 and track, handling, camera, ship and interfaces. Break repeated occupancy bands with a larger irregular interior atlas, reposition/resize existing facade advertisements into the driving view, and rebake the existing material-scoped glass captures with localized sign spill. Inspect native approach/bend/exit, then deliver HUD-visible native racing footage and a separately named app. Technical validation is not owner art acceptance.

### Existing city lighting result

City Light03 is ready for owner review after three native iterations, 248 passing tests, exact-anchor comparison, verified native motion and separate capped performance checks. [Review and limits](city-light-review.md). Original apps remain intact; no later stage has started.


## Active neon lighting and grading pass — 2026-09-16

Owner authorizes completing the agreed current-stage correction before camera/motion work: replace broad blue-gray illumination with darker selective fill, source-localized cyan/magenta/warm spill, textured road/glass reflection, and color-preserving bright highlights. Test a scene-local HDR grade against unchanged chase-camera poses. Use the original racing video plus the supplied neon street image for lighting/material behavior, not new street objects. Preserve City Light03, layout, handling, ship, HUD and camera. Inspect and iterate the bend natively, carry coherent treatment through the opening and deliver the app, comparisons and native clip. No camera/blur or full-circuit expansion.

### Roadside glow correction — continued in current task

Restore a readable cyan/magenta near-rail wash after the neon pass reduced it too far. Retain a nonzero textured response, dark road center, localized advertisement spill and the approved camera/layout. Rebuild a separate Neon09 candidate, inspect matching native anchors and motion, rerun current-build performance and regression checks, then package the current-stage result for owner review.

### Neon09 delivered for owner review

Roadside cyan/magenta wash restored with texture retained. Current native build, 249 passing tests, three exact-pose anchors, 432-frame HUD-visible capture, same-build grading comparison and isolated performance are recorded in [the neon review](neon-lighting-review.md). Artistic acceptance remains open; stop before the camera/motion stage.

## Owner-authorized motion and camera pass — 2026-09-16

Owner approved the next stage with “lets go”. Preserve Neon09 lighting, road/handling/ship/HUD and earlier apps. Inspect the supplied 10.112-second racing clip: sampled views retain a rear chase camera; front/rear timing is not established by that clip. Implement a lower responsive chase camera with bounded banking/turn anticipation, speed-dependent camera-and-object blur at ordinary cruise, and hold-to-look-back (Tab / right-stick press), with blur suppressed during viewpoint transitions and reduced-motion support. Enable only for the Stage1 scene. Provide same-build baseline comparison, actual native footage including view changes, current-build regression/performance evidence and a separately named native app. No full-circuit art work.

### Motion03 ready for owner review

Implemented the lower chase camera, cruise-speed motion blur and hold-to-look-back side arc. Same-build native comparisons preserve all racer physics; reduced-motion behavior is verified and 251 tests pass. P99 frame time increased to 15.873 ms, recorded as a limitation. [Native review, controls and checks](motion-camera-review.md). Owner visual/handling acceptance remains open.

## Owner-authorized complete-circuit lighting rollout — 2026-09-16

The owner explicitly authorizes expanding the accepted first 13-second test-field treatment across the complete circuit and requires the test field itself to remain unchanged. Preserve its first 310 infrastructure spans, normalized start `-0.012`, spacing `0.00108`, geometry, materials, light values and ordering. Continue the same road-reflection overlay, cyan/magenta rails, structural supports, embedded markers and local rail wash from the existing boundary at normalized progress `0.3228` through the wrap at `0.988`. Do not change track geometry, handling, camera, ship, HUD, architecture or the protected test-field settings.

Build a separately named native app. Verify the boundary contract in tests, compare the original three protected-field anchors, inspect captures after progress `0.3228`, complete a native race, and measure current-build performance. Technical completion and a brighter complete circuit do not establish owner artistic acceptance.

## Owner-authorized lighting-field and original-map hybrid — 2026-09-16

The owner clarified that `Vector Rush.app` and `Vector Rush Circuit Lighting 01.app` contain visibly different world maps. Preserve `Stage1City.unity` and its first 13-second lighting field without editing either existing app. Create a separate hybrid scene from the lighting scene, remove only the later placeholder environment from that copy, and add the original Vector Rush map districts after the protected field. Reuse the existing identical course spline instead of stacking a second road or collision surface.

Build a separately named native app. Verify the protected scene file remains byte-identical, the hybrid retains one gameplay world/camera/track, the original route districts are present, a full native lap completes, and the result passes current tests and performance validation. Any overlap, route discontinuity or protected-field change rejects the hybrid.

### Continuation validation

The saved Hybrid Route 01 three-race run failed after race 1. Diagnose the next-race timeout with opt-in native telemetry before delivery, preserving all existing apps and both source scenes. Recheck the final binary's protected anchors and map handoff, record an isolated performance race, and save a scoped milestone with explicit limitations.

### Hybrid result

Hybrid Route 02 is ready for owner review. The protected scene remains byte-identical; three matched opening views retain the lighting-field composition. All 253 editor tests and three consecutive six-finisher native races pass with zero recoveries. A separate full-race performance run reports 8.35 ms mean and 9.30 ms P99, with two hitches up to 183.89 ms. The earlier repeat-race timeout did not reproduce and is not claimed fixed. The map switch uses a brief blackout after the protected boundary. [Build, evidence and limitations](hybrid-route-review.md).

## Screenshot-based environment reference standard — 2026-09-16

The owner clarified that sparse suburban surroundings are appropriate, but the expansion currently feels unfinished; the covered sections must feel underground. The owner requests several native screenshots and generated completed reference images before environment changes. Capture the exact Hybrid Route 02 binary, select repeatable city-edge, suburban and tunnel views, generate Scenario image edits anchored to those frames and the unchanged opening quality benchmark, inspect composition and scene identity, and deliver a clearly labeled comparison set with source metadata and prompts. This step creates art references; it does not alter the game environment.

### Owner correction: compare GPT and Gemini through Scenario

The owner rejected the FLUX 2 Max references as insufficient and explicitly requested both GPT and Gemini through Scenario. The FLUX set is marked rejected and must not become an implementation standard. Generate the same three screenshot-anchored section briefs with Scenario's GPT Image 2 (high quality) and Gemini 3 Pro (2K), compare the full outputs, and present model-labeled alternatives. No game changes are authorized by this reference-generation step.

## Whole-environment polish — planning request, 2026-09-16

The owner now asks for an environment-first plan: retain the successful opening, improve the weak left-side city beyond it, make covered sections read as partly underground tunnels, allow deliberately sparse areas with natural polished transitions, and vary opening billboard shape, artwork and height. The [whole-environment polish plan](plans/2026-09-16-whole-environment-polish.md) defines route composition, bounded assets, a continuous structural pass, signage and finish stages, and native acceptance checks. It also corrects the earlier gallery reference: surface finish is useful, but the structural goal is an embedded tunnel. Current authorization is documentation only; implementation has not begun.

### Authorized execution: structural Stage 1

The owner subsequently authorized implementation in stages. First deliver an isolated `EnvironmentStructureStage1` candidate: replace the bank-tilted thin-post support batch with vertical piers and deep decks, add bounded service podiums around the exposed district while retaining a bridge opening, and leave the baseline and racing geometry untouched. Native views 04/06/10 are the principal comparisons. Run course/collider preservation and clearance checks, editor tests, a fresh native build, a full-lap capture and a race. Tunnel enclosure, billboard variety, district-blackout removal and surface finish remain later stages, not completion claims for this structural milestone.

Stage 1 candidate built: 48 pier assemblies, continuous deep deck and three bounded podium stretches in a separate scene, using three mesh batches and no added lights/colliders. Native review prompted one correction to flat face normals to eliminate a ribbed-looking deck edge. The 255-test suite, unchanged course/collider checks and 12,000 sampled clearance rays pass. The baseline scene remains byte-identical. Final candidate GUID `11b17a9bc497459a9844f05b03e7b6c7` has a completed 400-frame native lap; see the [structural review](../../../../../Vector%20Rush%20Review%202/environment-stage1/REVIEW.md) and [before/after views](../../../../../Vector%20Rush%20Review%202/environment-stage1/comparison.html). Owner visual acceptance and human driving review remain open. Next milestone: embedded gallery/tunnel enclosure and connected approaches, not more surface decoration on the existing open frames.


## GitHub relay continuation — 2026-09-17

The owner supplied `emrickk/vector-rush` branch `game/current` after requesting alternating agent continuation through GitHub. Start from publication commit `a7b8d70013703f17b65125e8a8b1ad90c9a53d4f` in an isolated checkout. First bounded milestone: verify the publication manifest, run the complete current EditMode suite on a clean import, build the underground candidate, and complete native preview/camera-clearance and isolated three-lap checks. Diagnose concrete failures before visual expansion. Preserve the completed HUD, baseline scene and horizontal course; do not interpret technical validation as artistic acceptance. Record actual results and remaining work in the current handoff before returning the baton.

Relay 01 verification milestone completed: clean 271-test suite, native build, 405-frame lap/camera check and isolated six-finisher three-lap race all passed. No game changes were needed for these checks. The next bounded step is underground upper-foundation and terrace/city visual finish, with actual continuous native comparison; see the current handoff.

## Authorized underground finish pass — 2026-09-17

Owner approved the next pass with “lets do it”. Starting from `e3d3ff6`, apply a deterministic additive finish layer to the serialized underground candidate, preserving all existing route meshes/colliders, HUD and baseline scene. Hypothesis: inhabited terraces, dimensional wall panel/recess rhythm, planted pockets, podium fronts and localized warm light will connect the large retaining masses to the city in the native descent/portal sequence. Add only candidate-owned assets and an idempotent editor authoring entry point, without rerunning the earlier route generator. Review real native captures against the included reference, correct inadequate results, then run current tests, recorded camera/lane checks, three consecutive races and isolated frame-time comparison. Deliver native app, matched views, continuous footage and updated GitHub handoff. Whole-circuit rollout remains later work.

The underground finish candidate is implemented and ready for owner review. 273 tests, native build, recorded camera path, three consecutive races and separate performance run passed. Matched comparisons and continuous footage are delivered; see `docs/current-game/underground-finish-2026-09-17/README.md`. Reference-fidelity gaps and manual/controller testing remain explicit; no wider-city rollout was performed.


## Authorized road smoothness correction, September 17, 2026

Owner approved correcting the curve and banking after identifying rough road/curve motion. Starting from `854be8a`, preserve the finish candidate for comparison and author a separate smooth-road scene. Use a periodic curvature-continuous centerline with bounded displacement, smooth distance-based banking and denser shared road/barrier geometry. Adapt the existing trackside assets to the corrected path, retain the HUD/ship/material direction, and validate lane/camera clearance, curvature/bank transitions, repeated races and actual higher-rate native footage. This authorization supersedes the previous exact-course preservation constraint for this correction only; the old scene stays unchanged.

## Road correction verification, September 17, 2026

Implemented the owner-approved curve/banking correction in an isolated candidate. 276 tests, three races and recorded camera clearance passed. See [the road milestone](current-game/smooth-road-2026-09-17/README.md) for performance evidence and review limitations. Next is owner driving review; wider visual rollout remains pending.

## Owner-approved rain pass, September 17, 2026

Add restrained rain streaks with actual shelter checks, wet road response, subtle ship spray and rain audio that softens under cover. Preserve handling, road geometry, HUD and ship. Author a separate RainRoadStage4 candidate; verify native outdoor/tunnel footage, dry-scene preservation, effects mute/pause and uncaptured performance before publishing.

Rain pass completed for owner review: 279 tests, native outdoor/tunnel capture and isolated three-lap/performance run passed. See [rain milestone](current-game/rain-2026-09-17/README.md). No handling or broader environment changes.

## Owner-approved atmosphere correction, September 17, 2026

Dry covered road, no tunnel rain/spray, rain sound fading with distance from portals; stronger outdoor city haze while nearby road stays readable. Bake coverage from actual roof colliders, use an 8 m wet-to-dry transition outside roof edges, preserve original vertex positions and collision meshes. Verify native outdoor/portal/interior/exit views, shelter/material regression checks and isolated performance.

Atmosphere correction implemented in the separate Stage 5 scene. 282 tests and a complete 43.029-second native capture passed; tunnel road is visibly dry in the matched comparison. See [atmosphere milestone](current-game/atmosphere-2026-09-17/README.md) for isolated performance results and review limits. Owner visual/audio judgment and manual/controller feel remain open.

## Owner-authorized night-city art kit — September 17, 2026

The owner supplied a rainy teal/red city reference, clarified that city detail should resolve at low speed and blur progressively at high speed, then explicitly prioritized completing polished art materials/assets before racing presentation. Build a cohesive reusable environment kit in an isolated checkout. Preserve all racing scenes, ship, HUD, handling and current weather. Scope: physical surface materials with dry/wet variants, distinct facade/signage artwork, modular rooftop/service/infrastructure/sign assets and city silhouettes. Deliver editable sources, engine exports/materials/prefabs, an asset review scene, inventory/provenance and inspected close/assembly renders. This is asset production; track placement and speed blur follow later. Do not equate generated concept art with exported assets or artistic acceptance.

Steps: (1) inventory retained/replaced assets and author material/sign sources; (2) model/export modular kit with LODs and inspect assembled lighting; (3) verify Unity imports/material bindings and produce a review package; (4) publish scoped source milestone and precise handoff after review of the diff. Scenario remains the image-generation provider. The recent icon task supplied the current working connection; three high-quality billboard artworks were generated with GPT Image 2 through Scenario.


### Night-city kit continuation result

The material/model/import/review-package steps are complete for asset review. Current evidence and limitations are in `docs/night-city-art-2026-09-17/README.md`. Keep publication local and await owner art judgment before any full-track placement or blur rollout.

## Owner-approved animated billboard pass, September 17, 2026

Owner approved the shown Cyberpunk-inspired reference direction and explicitly requires actual animation in Unity. Add original varied advertisement artwork through Scenario, different screen proportions/mounting treatments, independently timed animated ad layers and scrolling typography, plus a few rotating/bobbing 3D holographic display objects. Preserve atmosphere Stage 5, fog/rain/shelter, all driving geometry and collisions, ship, HUD and handling. Author a separate Stage 6 scene. Completion requires native motion evidence showing the displays changing over time, current regression checks and an isolated uncaptured performance/race run. Static images alone do not satisfy this step.

Owner correction during native review: the first candidate is too dense in its opening eight seconds, signs sit at similar heights, face the racer uniformly, flicker harshly and use one display per building. Replace that repeated placement with sparse architectural clusters, height hierarchy, fixed facade/corner orientations and composed stacks/strips/segmented displays. Leave intervening buildings clear. Correct screen/housing depth overlap and distant texture aliasing, then inspect the opening and animation before publication. Previous Stage 6 captures are rejected layout evidence, not an accepted milestone.

Corrected Stage 6 candidate delivered for owner review: 10 screens/five clusters, four actual video campaigns, final native driving and fixed-camera playback, 286 tests, and isolated six-finisher/zero-recovery three-lap lifecycle verification. See [the billboard milestone](current-game/billboards-2026-09-17/README.md). Artistic acceptance remains open.


## Billboard driving-readability correction — September 17, 2026

Owner finds most corrected displays invisible from the driving camera and too small. Lower and enlarge hero displays, move the triptych onto the visible outer bend, and correct the corner/landscape approach faces. Retain five sparse groups, independent facade orientations, animation and the z-fighting fix. Verify projected visibility using the recorded chase camera, then inspect a new complete native driving capture. Close-up shots are supplementary and cannot establish driving readability. Preserve all gameplay/weather/collision behavior. GitHub publication remains blocked pending the owner's explicit approval in this task; continue local implementation and native delivery.

Candidate 05 completed with native chase-camera comparison, 286 passing tests, full lap capture, and separate six-finisher/zero-recovery three-lap lifecycle/performance verification. The new build is ready for owner review; see the driving-readability milestone.


## Billboard architectural finish and pacing — September 17, 2026

Owner approved convincing mounts, frames and facade recesses; balanced screen brightness against fog; gentler portrait motion, stronger product motion and slower tickers; investigation of candidate 05's two timing hitches. Preserve candidate 05's display positions/dimensions and gameplay/weather. Add facade-connected visual supports and recessed cabinet framing, then compare native driving footage. Remove avoidable performance-recorder allocations/logging, record bounded hitch diagnostics, and run isolated repeat measurements. Do not attribute the old hitches without evidence. Keep source local while publication approval remains pending.

Candidate 06 complete locally: facade-connected mounts/cabinets, balanced emission and campaign pacing, 286 passing tests, native driving/detail captures, and two uncaptured three-lap measurements. The old long stalls did not recur; one 41.47 ms hitch in run A and none above 33.3 ms in run B do not establish a cause. See [finish milestone](current-game/billboard-finish-2026-09-17/README.md). Capture intervals were uneven; artistic acceptance remains open.

## Owner-directed P4 and night-city integration — September 17, 2026

Resumed on `integration/p4-night-city-20260917`: the approved Unity launch now succeeds and the facade survey is generated. Next: resolve survey-driven placement, generate the isolated Stage 7 scene, validate P4 preservation and current tests, build a native app, inspect a complete driving replay, then measure an isolated race. Keep all outputs local; owner artistic acceptance and human/controller handling review remain separate.

Completed locally as Stage 7 revision 02: five terraces/30 kit instances, corrected structural support and right-side height, 290 passing tests, a 43.029-second native driving capture, and a passing foreground six-finisher/zero-recovery three-lap race. All 2,169 original P4 Unity files remain unchanged. The earlier unfocused rival timeout and foreground window-size change are documented rather than hidden. [Milestone, evidence and remaining owner-review boundary](current-game/night-city-2026-09-17/README.md). The owner approved committing and pushing this milestone on September 17, 2026.

Use latest local Game P4 b787b5f as the playable baseline. Merge its actual animated billboards, mounting, pacing and diagnostic work intact. Build a separate Stage 7 scene that composes the reviewed NightCityKit into the opening city district: grounded service podiums, inhabited frontage, rooftop equipment and differentiated background architecture around existing sparse animated displays. Preserve road/collision, gameplay, ship, HUD and weather/shelter. Verify geometry clearance and baseline component parity, inspect native driving motion against P4, correct visible issues, then run current tests and isolated race/performance validation. Save source and review locally. The previous speed-responsive blur candidate is considered only after the city integration is visually coherent; do not substitute blur for resolved art.

## Full-lap billboard correction — owner approved September 17, 2026

The gallery's After Hours, Night Market and Last Train assets are missing from the playable scene. The current five animated compositions cluster in the opening district. Create a separate Stage 8 from committed Stage 7, retain campaign animations, distribute existing groups across early/middle/late race sections and place the three missing artworks visibly beside the course. Rebuild physical supports and animated-reflection positions when relocating signs. Verify scene/source preservation, all ad references, per-quarter distribution, corridor clearance and correct reflection mapping. Native lap and close-up evidence, current tests and an isolated race/performance check are required before promoting the wrapper or calling the work complete.


Full-lap placement completed as Stage 8 revision 02: all three artworks visible in native gameplay, five animated groups across the course, interior display fitting corrected after first capture, 292 passing tests and separate three-lap race pass. Replay and evidence: [full-lap milestone](current-game/full-lap-ads-2026-09-17/README.md). Wrapper promotes Stage 8 while preserving Stage 7/P4 comparisons. Await owner artistic review; keep this milestone local.

## Koi Lantern Tower focal-point trial — September 17, 2026

Owner approved the first of three proposed focal landmarks. Produce Scenario concept and surface artwork, author an editable 3D koi/amber ring/market tower, survey a bend using the recorded driving camera, integrate a separate Stage 9 scene and inspect native approach footage. Preserve existing driving/gameplay/weather. Adjust only landmark composition as needed for clear sightlines. Deliver one verified turn and a local milestone; no automatic repetition across the course.

Koi trial completed locally as Stage 9 revision 02: new authored 3D landmark, Scenario concept/enamel assets, corrected curved fins and larger sculpture, 294 passing tests, native approach replay and separate three-lap race pass. [Evidence and remaining owner review](current-game/koi-landmark-2026-09-17/README.md). Stage 8 remains default; no additional landmarks implemented.

## Owner-directed luminous koi replacement

Stage 9 is visually rejected. Rebuild the anatomy, flowing fins/tail and luminous material around the two supplied references. Replace the decorative hoop with a projection installation, add local illumination and a wet-road reflection, and reduce competing nearby signage in the candidate only. Inspect actual native approach and sustained motion, adjust if necessary, then record validation and a local milestone. Keep earlier scenes preserved.

### Stage 10 owner correction: overhead swimmer
Rework the new koi into a giant translucent animated fish above the bend. Add whole-body roaming and visibly sweeping tail/fin deformation; validate the swept overhead clearance, inspect actual chase-camera approach and underpass, then deliver native motion. The previous fixed-scale beside-road placement is rejected.

Stage 10 revision 04 complete for owner review: native approach/underpass and tail sweep inspected, browser replay playback verified, 296 tests pass, isolated three-lap race completes with six finishers and zero recoveries. Saved as a local candidate; Stage 8 default and older content preserved.
