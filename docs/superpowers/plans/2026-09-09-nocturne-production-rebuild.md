# Nocturne Production Rebuild Implementation Plan

> **For agentic workers:** Use superpowers:subagent-driven-development or superpowers:executing-plans during implementation. Parent owns integrated art direction, shared scene/settings and native execution; agents own explicitly assigned assets or systems. Steps use checkboxes for execution tracking. This is a production plan with substantial playable deliverables, not a request to substitute small source tasks for those deliverables.

**Goal:** Rebuild the complete existing Nocturne circuit into a consistently finished, visually distinct night race, with coherent audiovisual feedback and a complete three-lap player experience.

**Architecture:** Preserve the working race framework while replacing the production environment with a serialized Unity scene containing authored geometry, persistent materials and lighting data. Procedural tools remain construction tools; the final nearby world is explicitly composed and inspectable before runtime. Environment assets, lighting and presentation are integrated under one scene owner and accepted together.

**Tech Stack:** Existing Unity 6000.6.0f1, URP 17.6 Forward+, Blender, C#, Apple Silicon macOS. No engine migration or purchased dependencies are assumed.

**Spec:** [Global assessment](../../visual-target-reviews/004-global-aaa-assessment.md), [independent visual critique](../../visual-target-reviews/004-global-aaa-independent-critique.md), [gameplay/production critique](../../visual-target-reviews/004a-global-gameplay-production-critique.md), and the original single-race quality ambition in [the build brief](../../GPT6-BUILD-PROMPT.md).

**Status:** Proposed in response to the owner's request for a solution plan. No rebuild has started. The opening04 rollout remains paused; its local changes do not satisfy this plan.

## What must change in execution

The earlier [environment plan](../../visual-environment-plan.md) already promised layered architecture, better lighting, a continuous passage and a full-circuit rollout. The problem was not missing aspirational wording. Its broad outcome was repeatedly narrowed into bounded corrections and never earned whole-game acceptance.

This plan changes the production commitments:

- **Replace, not indefinitely decorate.** Inventory every dominant nearby assembly as retain, substantially rework, replace or demote to background. Existing assets do not retain foreground status merely because they took effort to build.
- **Compose the whole lap before finishing one portion.** All four spatial beats and transitions must exist in a playable rough scene first. The first finished section establishes the production method; full-lap completion is a required milestone in this plan.
- **Promote the environment to editable production data.** Current `VectorRushSetup.Prepare` creates a bootstrap-only scene, and `VectorBootstrap` constructs the world and grade at runtime. The new scene stores placed geometry, persistent materials, scene lighting and bake data; runtime cannot silently reconstruct or overwrite them.
- **Approve exemplars by their native appearance.** Correct imports, UVs and shader variants qualify an asset for review. They do not establish sufficient quality to copy it around the course.
- **One integrated art verdict.** Asset, lighting and rendering owners supply work to the scene owner. Their local technical passes cannot independently close a visual milestone.
- **A failure changes the next action.** After at most two coherent scene treatments fail the same whole-image target, stop parameter refinement and identify whether asset quality, lighting technique, composition or available production capability is the limiting factor. Make a concrete replacement decision and report any continuing capability limit. Do not automatically switch engines or lower the target.

## Scope and invariants

- Complete one circuit, one recognizable Kestrel craft, five rivals and three laps. Content breadth is not the remedy for unfinished quality.
- Preserve original `Builds/Vector Rush.app` and historical preview apps/source. Use `Assets/Scenes/NocturneProduction.unity` and versioned `Builds/Vector Rush-production-NN.app` outputs. Never overwrite an existing output.
- Keep race progress rules, world scale (one Unity unit = one metre) and baseline route/collision alignment intact during art production. Track width, individual corners and camera behavior may change later when a specific driving/composition finding justifies the change; document and revalidate them rather than treating them as permanently untouchable.
- Reopen sky, fog, ambient/fill, practical lighting, material families, nearby architecture and track-edge design as a coordinated composition. Preserve night identity and navigation, not every old visual constant.
- Native gameplay is the output. Generated images and Blender renders are references or asset checks only. Frozen-camera stills, automated driving, simulation-time footage and real-time recordings have distinct labels.
- Profile at 1920×1080 on the existing M2 Max setup. Retain provisional P95 <= 12 ms and P99 <= 16.7 ms as project gates, not current measured achievements. Record all >33.3 ms spikes with stage/context. Measure capture overhead separately; no encoder, bake or build may run during performance sampling.
- Preserve metadata, editable sources, licenses and separate critique history. Save each completed production milestone and its independent verdict separately, and push to the existing authorized remote. An incomplete or rejected milestone stays marked incomplete/rejected.

## The circuit to build

Normalized route ranges are initial planning zones; transition extents are adjusted from actual driving views before final placement. They partition the existing lap rather than introducing four disconnected showcase scenes.

| Zone | Spatial treatment | Main construction and light | Driving/transition intent |
| --- | --- | --- | --- |
| 0.00–0.30: service viaduct | Elevated track visibly supported within an inhabited service district; readable levels below and beside the course; open distant skyline | Substantial deck edges, pier-to-podium connections, occupied service blocks, framed signal mast. Cool route light with selective amber interior depth | Start acceleration into a legible banked bend; nearby structure supplies parallax without covering the exit |
| 0.30–0.52: transit canyon | Buildings and overhead transit structure frame the existing cool gallery approach; enclosure contracts then releases | Deep facade bays, structural cross-frames and a clearly constructed gallery portal. Directional cool light reveals planes rather than only tracing edges | Visible braking/turn commitment and readable exit; preserve lateral room for traffic |
| 0.52–0.72: thermal works | The facility becomes a connected industrial space the route passes through, with a quieter open side | Cylinders tied into platforms, pipe supports, service access and large structural framing. Controlled warm process accents within cool exterior illumination | Crest/reveal and passing space; avoid a wall of arbitrary pipes or repeating silhouettes |
| 0.72–1.00: gallery and station | Warm enclosure, visible exit destination and a civic transit frontage connected to the start area | Gallery entry/interior/exit assemblies, station concourse/frontage, clear roof terminations. Warm surface illumination transitions into cool city depth | Final turn opens into a clear boost opportunity and start/finish straight; environmental rhythm supports the next lap |

Visual density is selective. Open sky and quiet road stretches remain valid where they strengthen scale or release. District names, prop counts and brighter lighting alone do not establish these differences.

## Milestone 1 — whole-lap redesign and a working production scene

**Files:** create `docs/nocturne-production-design.md`, `SourceAssets/nocturne-production/asset-ledger.csv`, `UnityProject/Assets/Scenes/NocturneProduction.unity`, `UnityProject/Assets/Editor/ProductionSceneSetup.cs`, `UnityProject/Assets/Scripts/World/ProductionWorld.cs`, `UnityProject/Assets/Scripts/Presentation/CraftMaterialFactory.cs`, and rendering assets under `UnityProject/Assets/Settings/NocturneProduction/`; modify `UnityProject/Assets/Scripts/VectorBootstrap.cs` and `Presentation/ShipSurfaceMaps.cs` to separate craft-material creation from world construction while preserving the legacy path. Scene owner controls all changes.

- [ ] Write a whole-lap spatial/light storyboard using normal racing views. Record an image and intended transformation for every zone entry, middle and exit. Use real released-game references for finish and existing generated A/B for local intent; neither becomes proof of native capability.
- [ ] Complete a replacement ledger for road, rails, gallery, signal mast, thermal works, station, nearby facades, middle blocks and skyline. Each row records zone, retain/rework/replace/demote decision, source, intended view size, light/material needs, owner and the native shot that must prove it. Reuse only assets that can meet the declared appearance.
- [ ] Build the entire lap's rough spatial layout in the new scene. Replace the gross arrangement of weak nearby assemblies now; do not hide untouched zones behind a “later” placeholder. Keep the existing TrackPath at identity transform: its current fixed knots and Evaluate results are world-space. Later route edits require explicit serialized route data, rebuilding its cached samples and regenerating matching road/colliders; moving the spline root is not a route edit.
- [ ] Add an optional serialized production-world reference to Bootstrap. With it assigned, consume the scene's one TrackPath, spawn vehicles/race/HUD/audio and exactly one runtime gameplay camera with production camera settings, and preserve scene lighting/grade. With it absent, the retained legacy path works as before. Extract a narrow craft-material factory and runtime disposal owner; update ShipSurfaceMaps to consume it without constructing a duplicate city or relying on uninitialized WorldBuilder material fields. Skip legacy global grade, lighting and preview configuration in the production branch.
- [ ] Use an editor construction/export path with editor-appropriate temporary-object disposal. Persist meshes, materials and generated texture dependencies as assets and leave a plain authored hierarchy without legacy generator cleanup owners or temporary primitive colliders. Regeneration replaces only its declared generated output and preserves separately authored scene work. Do not retrofit every old city builder into a general editor tool. Select production rendering assets directly, including isolated renderer-feature/material references; do not call `Prepare` or mutate shared legacy rendering assets while building the production scene.
- [ ] Establish baked indirect lighting on a representative gallery/frontage area, local reflection capture where useful, and probe lighting for moving craft. Supply suitable lightmap UVs and texel allocation, explicit GI/static participation and light modes, and persistent lighting/reflection data; saving the old generated meshes alone does not satisfy these requirements. Use the supported local lightmapper and measure its actual bake/memory behavior before scaling. Baking is a selected technique to prove, not a promised automatic improvement. Confirm scene lighting survives a standalone build.
- [ ] Supply a playable rough full lap plus one native lit area proving the scene workflow. Review all zone transitions, route visibility and crowded clearance before producing final meshes. Obtain an early short human driving check of braking sightlines and camera/space comfort before freezing any changed racing envelope. If that input is unavailable, keep those changes provisional and continue reusable asset/lighting work; do not mislabel automated clearance as human approval.

**Gate:** All four experiences are visibly distinguishable in the rough full lap, and the saved production scene loads its intended world and lighting without runtime replacement. This closes layout/workflow only; it is explicitly not a finished visual milestone.

## Milestone 2 — replace the visible construction with a finished asset family

**Files:** create editable assets and regeneration recipes under `SourceAssets/nocturne-production/`; export into `UnityProject/Assets/Art/NocturneProduction/`; create prefabs/materials under `UnityProject/Assets/World/NocturneProduction/`. Asset owners do not edit the integration scene.

- [ ] Author the road/edge assembly as one design: running deck, convincing barrier thickness/base, drainage/service channel, joints and underside/support connections. Keep apparent barriers aligned with the actual collision envelope.
- [ ] Author gallery portal, interior bay and exit assemblies with distinct structures, wall/ceiling depth, believable material boundaries and light housings. Finish one exemplar of each before duplication.
- [ ] Author near facade/base/roof assemblies and station frontage with recessed openings and meaningful floor/storey dimensions; use quiet lower-detail forms for distant towers. Do not scale a crude tower to become a hero building.
- [ ] Substantially rework the signal mast and thermal facility as connected structures; preserve recognizable identities where they remain useful. Their construction must survive ordinary chase-camera distance.
- [ ] Produce one shared material library for graphite road, structural metal, concrete/ceramic, painted enclosure and glazing. Establish physical map scale, UVs/tangents, edge response, roughness and appropriate mip behavior. Avoid fine noise as the main signal of finish.
- [ ] Review each family inside the lit native production scene, including an oblique driving view and another light condition. Reject exemplars whose large surfaces still read as flat blocks. Before replication, profile frame delivery and memory in a composed representative worst view with the finished kit, intended lights and six craft in a standalone run without capture overhead. Resolve costly geometry/material/light choices here; this early check does not replace the full-race measurement. Only accepted exemplars may be repeated across zones.

**Gate:** Road/edge, gallery, facade/station and industrial families meet the common finish reference in native views. Each family's acceptance names what is visibly achieved and what is not. Asset count and successful import are not the criterion.

## Milestone 3 — integrated finished racing section, with sound

**Files:** production scene and materials; create `UnityProject/Assets/Settings/NocturneProduction/` lighting/volume assets; modify `Presentation/RaceAudio.cs`, `Presentation/IonPropulsion.cs`, `Presentation/VehicleVFX.cs`, `Gameplay/ChaseCamera.cs` only as integration findings require; add authored audio assets under `UnityProject/Assets/Audio/NocturneProduction/` with provenance.

- [ ] Finish the continuous gallery→station→start→viaduct segment, initially progress .72 through the wrap to .18. Measure actual traversal duration; the 15–20-second target is approximate and not permission to speed up footage. The segment remains part of the same playable three-lap circuit.
- [ ] Integrate geometry, world depth, sky/fog, indirect/fill light, practical light and material response across every dominant surface in those views. Road, rail bodies, walls and near structures all receive finished treatment. Cyan remains readable navigation without carrying the whole composition.
- [ ] Build the road around stable physical surface response. Evaluate local reflection contribution only with an appropriate captured environment and in motion; leave the failed SSR preview parked. If the desired sheen fails, make an explicit material/lighting decision rather than darkening the artifact or repeating arbitrary sweeps.
- [ ] Reconcile craft materials, contact/hover cues and exhaust under the scene's lighting. Preserve craft identity; refine its finish as part of the scene rather than as another separate hero-object milestone.
- [ ] Join throttle/load propulsion response, speed-driven airflow, spatial rival approach/pass-by, boost, impacts and gallery/exterior acoustics. Add an original or properly licensed music direction mixed beneath essential racing cues; do not claim soundtrack completion from a missing or silent placeholder.
- [ ] Review normal driving, boosted traversal, a nearby rival, a legal pass and contact/recovery. Automation supplies reproducible technical cases; independently heard/watched native play establishes presentation quality. Make any needed camera adjustment through a declared comparison and recapture environment views rather than using FOV/shake to hide weak space.

**Gate:** A continuous audible native section meets the declared finish criteria against the chosen reference across road, architecture, light, craft and feedback. A reviewer must identify achieved construction and light/material relationships; “approaching the target” alone does not close this gate. All evaluated views improve as a whole at ordinary display size; no annotated inset is needed to find the main change. It remains readable with rivals and stable through transitions. Passing this section does not finish this plan.

**Failure action:** At most two coherent integrated scene treatments. If neither meets the target, diagnose production capability with side-by-side native exemplars and record a concrete art/lighting/asset-pipeline replacement decision. Do not relabel “better than baseline” as success, expand the rejected kit, or conceal the shortfall with more documentation.

## Milestone 4 — finish the entire circuit to the same standard

**Files:** production scene, asset ledger and all production asset families; expand `evidence/nocturne-production/` with full-lap native evidence.

- [ ] Complete the transit canyon/cool gallery and thermal works from the accepted exemplars, while retaining their distinct spatial and light identities. Their gross layout and required asset families were already proven in Milestones 1–2.
- [ ] Finish the connecting road, barriers, middle-distance fabric, skyline composition and all zone boundaries. Remove or demote the replaced legacy assets; do not leave a polished foreground next to an obviously unfinished adjacent segment.
- [ ] Place deliberate quiet intervals and distinct reveals so repetition does not expose the kit. Check both ordinary and boosted speeds, including camera paths during wall contact/recovery.
- [ ] Produce one uninterrupted full-lap review with sound, plus entry/middle/exit stills for all four zones. Review every zone against the same reference and scorecard used for the first section.
- [ ] Profile the actual expanded candidate. Reduce unseen complexity, adjust LOD/culling, bake or rebalance lighting and optimize costly materials before sacrificing the accepted visible construction. Record any quality change needed to meet performance.

**Gate:** The whole circuit meets the accepted visual standard; no zone is excused as future work. Passing the showcase section cannot compensate for a weak thermal, canyon or transition section.

## Milestone 5 — complete the race's control, competition and replay loop

**Files:** `Gameplay/HoverVehicle.cs`, `Gameplay/RaceDirector.cs`, `Gameplay/RaceProgress.cs`, `Gameplay/ChaseCamera.cs`, `Presentation/RaceHUD.cs`, `Presentation/RaceAudio.cs`, `VectorBootstrap.cs`; new `Gameplay/RaceRecords.cs` and `Presentation/PlayerPreferences.cs`; focused tests in `Assets/Tests/Editor/`.

Start diagnosis alongside Milestone 3, then integrate and finish after whole-course art is stable. Gameplay owners do not retune the reference driving behavior underneath art captures without coordination.

- [ ] Evaluate where human driving earns time through braking, airbraking, racing line and boost. Identify specific weak decisions before changing corners or handling; preserve race-progress validity and repeatable recovery.
- [ ] Audit AI pace by sector and traffic condition, independent completion and assistance duty. First replace the current freeze-everyone-on-player-finish contract: freeze the player's final result while rivals may finish for up to 60 seconds; record actual rival crossings, mark any remaining racers DNF at the limit, and allow immediate restart/quit. Keep the player's final time separate from the continuing simulation clock, and implement records against that contract. Tune contest quality rather than merely increasing speed or artificially keeping every rival beside the player. Demonstrate actual overtaking and recovery, not just proximity.
- [ ] Persist personal best lap/race with track/version identity, show useful previous/best comparison at results, and preserve records across restart and relaunch. Separate invalid/incompatible records after route changes.
- [ ] Add persistent audio levels, sensitivity/control configuration and camera comfort options. Provide complete controller menu focus/activation and keyboard/pointer support. Retain the readable HUD design; change what is needed for these functions.
- [ ] Test new behavioral contracts: saved records survive restart/relaunch; corrupt records fall back safely; paused input cannot affect the vehicle; controls remain valid after remapping; reordered finish crossings remain correct. Run the existing race/input tests after relevant changes. Art constants do not receive mirror tests.
- [ ] Obtain sustained human keyboard and physical-controller sessions on the candidate, covering crowded starts, braking, boost decisions, overtaking, wall contact, recovery, finish and repeat play. Tool-driven steering is never substituted for this evidence. If direct human/controller coverage is unavailable, retain that explicit unfinished gate while completing independent production work.

**Gate:** A human can understand and improve a lap, make meaningful racing decisions, receive coherent audiovisual feedback and repeat the race with useful comparison. Competition and fairness require observed play; correctness tests alone cannot close this milestone.

## Milestone 6 — accept and deliver the complete candidate

**Files:** `UnityProject/Assets/Editor/ProductionSceneSetup.cs`, `tools/play-production.command`, `docs/HANDOFF.md`, `docs/development-history.md`, `docs/implementation-plan.md`; final evidence under `evidence/nocturne-production/final/`.

- [ ] Build the saved production scene into a fresh versioned standalone app without rebuilding the legacy environment or overwriting existing apps.
- [ ] Run three complete native races with restart, pause/resume, results and saved-settings/records checks. Retain crowded/worst-view evidence and errors rather than selecting only a clean empty-track lap.
- [ ] Collect separate warmed, real-time frame-delivery samples on the exact candidate. Report the declared resolution/settings, P95/P99, spikes and test conditions. A simulation-time video cannot satisfy the performance gate.
- [ ] Deliver the complete native app, functioning launcher, full-lap audible preview, short original-versus-production comparison, editable source assets and clear controls. The selected build should launch the new production scene directly, without an undisclosed preview flag.
- [ ] Independent review separately states visual, audiovisual/motion, human gameplay, controls and performance verdicts. Any unmet category remains visible. Update the handoff and push completed source/evidence milestones.

**Completion rule:** A local asset pass, attractive screenshot, working build or majority of completed tasks cannot stand in for the complete accepted race. Do not claim AAA as a guaranteed label or give an unsupported calendar estimate. Report the actual finished quality and any remaining shortfall.

## Review scorecard and ownership

Every visual review uses: (1) dominant surface/construction finish; (2) coherent light/material depth; (3) distinct spatial sequence; (4) craft/opponent/route readability; (5) stable native motion. Record pass/fail/untested with evidence against the target, not only against the previous build. Evaluate all five together in the integrated scene. Sound/control/product/performance get separate explicit verdicts.

Parent owns the whole scene, the replacement ledger, final kit acceptance and the decision to reject an inadequate treatment. Environment artists own nominated Blender/texture/prefab families; a lighting/technical artist owns nominated lighting/material assets but coordinates daily in the integrated scene; gameplay/audio owners work on nominated scripts/assets; critics only edit reports. Unity/baking/native performance execution is serialized through the parent. Parallelism accelerates production; it does not split the quality standard.

## Technical feasibility and realistic limits

Unity's [pipeline comparison](https://docs.unity3d.com/6000.0/Documentation/Manual/render-pipelines-feature-comparison.html) documents baked lighting, probes and reflection capabilities in URP. This supports evaluating the selected production approach, not assuming it will meet the art target. Unity also documents [bake-memory and macOS constraints](https://docs.unity3d.com/6000.0/Documentation/Manual/GPUProgressiveLightmapper.html). Milestone 1 must prove the installed editor/backend and persistent native result before large bakes.

The substantial work is authored asset production, integrated scene iteration and whole-lap finish, followed by real experiential validation. Saved scenes and baking are tools for that work, not substitutes for design. If required asset quality cannot be produced with the available workflow, report it and propose a concrete art-production alternative rather than silently reusing a weak kit. No paid purchase, engine migration or external service expenditure is implied by this plan.

## Planning self-review

- Global critique coverage: environment quality → M1/M2/M4; lighting/material coherence → M1/M3/M4; spatial sequence → M1/M4; craft integration → M3; racing/audio → M3/M5; replay/controls → M5; truthful delivery/performance → M6.
- Pipeline work is attached to a working native scene, not an independent architecture project. No new function signatures are assumed by later milestones; the existing runtime interfaces and concrete file ownership are the starting contracts.
- Asset production is reviewed in native scenes before replication. Whole-lap rollout is mandatory and remains incomplete until all four zones pass.
- This document defines production deliverables and decisions. Source-level patches and exact behavioral tests are written against the inspected code at each execution boundary, rather than inventing an entire implementation before its visual and play findings exist.

## Independent planning review

[Visual/production review 005](../../visual-target-reviews/005-production-rebuild-plan-review.md) checked recurrence of the old methodology. Its target-gate wording, early human layout check and early composed performance check are incorporated. [Source feasibility review 005a](../../visual-target-reviews/005a-production-scene-plan-review.md) identified TrackPath world coordinates, transient resource ownership, bake preparation, craft/camera/render asset boundaries and race-finish lifecycle; these are incorporated into M1 and M5. Neither review claims a native quality pass.
