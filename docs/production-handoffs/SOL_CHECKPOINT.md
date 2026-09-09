# Sol Coding Lane Checkpoint

Status: **PAUSED FOR OWNERSHIP TRANSFER**  
Recorded: 2026-09-09  
Repository: `/Users/anping/Documents/Stuff/AI Space/Vector Rush`  
Source branch at worker checkpoint: `build/first-playable`  
Source base: `b0dd53da80faf4d96ae22af775f877d34bb08fab`

Packaging note: this work is now committed on `codex/nocturne-production-handoff` in PR #1. The paths listed as modified/untracked below describe the pre-commit worker state. Continue from the PR branch; do not reset to the source base. Editor logs referenced below remain local and are intentionally excluded.

No Unity, Blender, or shell process remains active. Nothing was staged, committed,
pushed, cleaned, reset, or stashed for this handoff.

## Ownership boundary

These paths were already dirty or user-owned and were preserved. Do not attribute
them to the Sol lane:

- `UnityProject/Assets/Scenes/Solstice.unity`
- `docs/opening-native-comparison.html`
- `docs/visual-target-reviews/003g-opening04-preservation-source-review.md`
- `evidence/nocturne-v2/opening-04/**`

`Solstice.unity` remained dirty but was not intentionally edited by this lane; an
intermediate `TimeManager` change was restored.

## C1 — Complete

- Added the production runtime boundary through `ProductionWorld`,
  `ProductionMaterialLibrary`, `ProductionRenderConfiguration`, and
  `CraftMaterialFactory`, plus narrow integration changes in `ShipSurfaceMaps` and
  `VectorBootstrap`.
- Production uses a saved `TrackPath`, material library, and exactly one active
  scene `MainCamera`; it does not call `WorldBuilder`. The legacy path remains when
  `productionWorld == null`.
- Runtime craft materials are cloned, owned, and disposed without destroying
  templates.
- Batch entry points exist in
  `UnityProject/Assets/Editor/Production/ProductionSceneSetup.cs`.
- Context evidence is in `evidence/nocturne-production/context/`: 1,201 frames,
  width 22, length 1844.517578125, course hash
  `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`, and
  geometry SHA-256
  `5a22d7779b229a7f7d38342a393414d99c5804f13badab5e6d8040ded782c764`.

## C2 — Implemented and diagnostically tested; native proof pending

- Added `ProductionArtManifest`, `ProductionArtImporter`,
  `ProductionSceneBuilder`, `ProductionSceneSetup`, and the production Editor
  assembly definition under `UnityProject/Assets/Editor/Production/`.
- Validation occurs before scene mutation and covers READY/version/revision/course
  hash/checksum coverage, path traversal, missing dependencies, duplicate IDs,
  non-finite transforms, normalized rotations, positive unit scale, and
  material/lighting/profile rules.
- Import goes through staging, keeps an immutable byte-exact payload below
  `ImmutablePayload~`, copies only declared FBX/textures into `Imported`, applies
  FBX/texture policy, validates imported LOD dimensions, and creates persistent URP
  materials, maps, and LOD prefabs.
- Scene assembly includes full-course OBB corridor clearance, authoritative saved
  collision/profile meshes, transforms, LOD/GI flags, lights, reflection and light
  probes, isolated URP renderer/pipeline/volume/lighting assets, camera/world/
  bootstrap setup, and save/reopen/reference checks.
- `ImportArt`, `BuildScene`, `BakeScene`, and `BuildCandidate` are implemented.
  Candidate builds reject overwrite of the original app, incomplete layouts,
  missing zones, and `DIAGNOSTIC_ONLY` packages.
- No immutable Astra production package has been supplied or imported. There is no
  production scene, bake, app build, or native image/video/performance proof yet.

## C3 — Not started

No Astra revision was available, so there has been no native exemplar, issue, or
performance exchange.

## C4 — Partially implemented; exact pickup point documented

- `RaceFinishLifecycle` is integrated into `RaceDirector` and `HoverVehicle`.
  Player results freeze at finish, unresolved rivals may continue simulating, the
  timeout/DNF clock is pause-aware, and Ion/VFX consult `CanSimulate`.
- `RaceRecords` implements versioned course/rules keys, manual best lap/race,
  signed comparison, automated/evidence exclusion, atomic replacement, and
  corrupt-safe loading.
- `PlayerPreferences` implements versioned atomic JSON, clamped levels and
  sensitivity, shake, all existing keyboard actions, contextual conflict checks,
  defaults, and reset. Input, director, and camera consume it.
- `RaceHUD` contains results comparison, menu/pause/results/settings/key-binding
  screens, pointer and keyboard/D-pad focus/activation, key capture, conflict
  display, and reset. It compiled in the last successful audio-focused run but has
  no native interaction proof.
- `RaceAudio` implements throttle/boost motor load, speed-only wind, procedural
  score, nearest-three spatial rival/pass-by budgeting, spatial impacts,
  gallery/exterior reverb response, and category volumes.
- **Pickup point:** `RaceTelemetry.cs` and `RaceTelemetryTests.cs` were the final
  in-flight write. The expected red test failed before the class existed. The class
  now exists but has not been compiled or integrated into `RaceDirector`.
  Sector/traffic/assistance sampling still needs wiring.
- Remaining C4 work: make telemetry green, add direct `RaceDirector` post-finish
  integration coverage, resolve compile/runtime defects, run native settings/
  controller/audio checks, and run the full suite.

## C5 — Not started

This is blocked on Astra's final `layoutComplete` package and `ART_HANDOFF`. There
are no full-course races, candidate build, native captures, performance sample,
`tools/production/play-production.command`, final evidence bundle, or
`docs/production-handoffs/CODING_HANDOFF.md`.

## Latest test evidence

- Baseline: `evidence/nocturne-production/coding/baseline-2026-09-09/editmode-results.xml`
  — 42/42 passed.
- C2 focused: `c2-green-entrypoints-02/results.xml` — 10/10 passed. Separate
  focused runs passed import 8/8, nested track 1/1, and scene reopen 1/1.
- C4 finish lifecycle: 3/3 passed.
- C4 finish/input integration: 8/8 passed.
- C4 records: 4/4 passed.
- C4 preferences: 5/5 passed in `c4-green-preferences-02`; the preceding run failed
  only because Unity Package Manager IPC did not connect.
- C4 audio: 3/3 passed in `c4-green-audio-01`. This is the last successful compile
  before the telemetry class was written.
- `c4-red-telemetry/unity.log` is expected red from before `RaceTelemetry` existed.
- The current tree has not been compiled or tested after adding `RaceTelemetry`.
  There is no post-C4 full-suite result.
- `git diff --check` was clean for the tracked diff at transfer time.

## Sol-owned modified tracked paths

- `UnityProject/Assets/Scripts/Gameplay/ChaseCamera.cs`
- `UnityProject/Assets/Scripts/Gameplay/HoverVehicle.cs`
- `UnityProject/Assets/Scripts/Gameplay/RaceDirector.cs`
- `UnityProject/Assets/Scripts/Presentation/IonPropulsion.cs`
- `UnityProject/Assets/Scripts/Presentation/RaceAudio.cs`
- `UnityProject/Assets/Scripts/Presentation/RaceHUD.cs`
- `UnityProject/Assets/Scripts/Presentation/ShipSurfaceMaps.cs`
- `UnityProject/Assets/Scripts/Presentation/VehicleVFX.cs`
- `UnityProject/Assets/Scripts/VectorBootstrap.cs`
- `UnityProject/Assets/Tests/Editor/VectorRush.EditorTests.asmdef`

## Sol-owned untracked paths

- `UnityProject/Assets/Editor/Production/**`
- `UnityProject/Assets/Scripts/Gameplay/{RaceFinishLifecycle,RaceRecords,RaceTelemetry}.cs[.meta]`
- `UnityProject/Assets/Scripts/Presentation/{CraftMaterialFactory,PlayerPreferences}.cs[.meta]`
- `UnityProject/Assets/Scripts/World/{ProductionMaterialLibrary,ProductionRenderConfiguration,ProductionWorld}.cs[.meta]`
- `UnityProject/Assets/Tests/Editor/{PlayerPreferencesTests,ProductionArtImporterTests,ProductionBoundaryTests,RaceAudioTests,RaceFinishLifecycleTests,RaceRecordsTests,RaceTelemetryTests}.cs[.meta]`
- `evidence/nocturne-production/**`

The handoff document itself, `docs/production-handoffs/SOL_CHECKPOINT.md`, was
added only to record the requested ownership transfer.

## Empty test residue

The following are empty test-created asset directories/metas, not a real art
import. Evaluate before removing:

- `UnityProject/Assets/Art.meta`
- `UnityProject/Assets/Art/NocturneProduction.meta` and its empty directory
- `UnityProject/Assets/World.meta`
- `UnityProject/Assets/World/NocturneProduction.meta` and its empty directory
- `UnityProject/Assets/Settings/NocturneProduction.meta` and its empty directory

## Pickup risks and decisions

- The current checksum JSON shape is
  `{contractVersion,revision,files:[{path,sha256}]}`; READY is
  `{contractVersion,revision,courseHash,checksumsSha256}`. Align Astra output to
  this shape or expand the parser before integration.
- Scene, pipeline, bake, and build code has diagnostic Editor-test coverage only;
  it has not run against final production data.
- Preserve the owner-first review stop. This checkpoint transfers implementation
  state; it is not visual-quality approval.
