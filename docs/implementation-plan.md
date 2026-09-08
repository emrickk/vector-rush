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
- [ ] Fix highest-impact findings; recapture and re-review up to three meaningful cycles.
- [ ] Record exact tools, compile/test logs, performance and remaining gaps. Deliver prompt, sources, build and controls, or precise external blocker if build cannot run.

## GitHub milestones
- [x] Preserve integrated source checkpoint and push to private `emrickk/vector-rush`.
- [x] Complete second native race and collect current evidence (112.58 seconds, 3 laps, zero player recoveries).
- [ ] Final polish: road shadow artifacts, team paint distinction, grandstand roof structure; rebuild and recapture.
- [ ] Verify menu/keyboard interaction and record a short native gameplay video.
- [ ] Commit final evidence, critique, limitations and delivery documentation; push all completed steps.
