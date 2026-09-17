# Coding and integration assignment — GPT-5.6 Sol

**Model:** `gpt-5.6-sol`. Follow README, contract-v1 and the owner-first stop. Do not spawn a reviewer or switch models. Your endpoint is a complete implementation handoff, not a quality-approved game.

## Owned files

You may edit `UnityProject/Assets/Scripts/`, create editor code in `UnityProject/Assets/Editor/Production/`, create `UnityProject/Assets/Scenes/NocturneProduction.unity`, and create/import production assets under `Assets/Art/NocturneProduction/`, `Assets/World/NocturneProduction/`, `Assets/Settings/NocturneProduction/`, and `Assets/Audio/NocturneProduction/`. Own code tests, `tools/production/`, `evidence/nocturne-production/context/`, `evidence/nocturne-production/coding/`, `evidence/nocturne-production/final/`, and `docs/production-handoffs/CODING_HANDOFF.md`.

Do not alter `SourceAssets/nocturne-production/`, legacy `Solstice.unity`, prior art/evidence, existing native apps, AGENTS, or shared coordination plans. Read legacy code/resources but preserve their reproduction path. Parent alone stages/pushes named worker payloads after each implementation checkpoint; never use `git add -A` or clean/reset/stash.

## Existing behavior you must account for

Read these files before C1: `VectorBootstrap.cs`, `World/WorldBuilder.cs`, `World/TrackPath.cs`, `Presentation/ShipSurfaceMaps.cs`, `Gameplay/ChaseCamera.cs`, `Editor/VectorRushSetup.cs`. Before C4 also read `Gameplay/HoverVehicle.cs`, `Gameplay/RaceDirector.cs`, `Gameplay/RaceProgress.cs`, `Presentation/RaceAudio.cs`, `Presentation/RaceHUD.cs`, and all existing Editor tests.

- Prepare changes shared pipeline/material state and recreates Solstice. Production builds must not call it.
- TrackPath stores fixed knots in code; its transform does not move Evaluate results. Preserve identity and route/collision behavior in this run.
- WorldBuilder and its children create and destroy transient assets. Do not run them once in edit mode, save, and assume their output is persistent or bake-ready.
- ShipSurfaceMaps currently consumes WorldBuilder.MakeMaterial; craft metal fallback is initialized by world construction. Extract the dependency explicitly.
- Bootstrap creates a camera, volume and global lighting. The production branch must not overwrite saved scene presentation or create duplicate world/camera objects.
- RaceDirector currently freezes every racer at player finish, and HoverVehicle only drives during Racing. Changing only RaceDirector would leave rivals unable to finish.

## C1 — production runtime boundary and exact geometry context

Create `World/ProductionWorld.cs`, `Presentation/CraftMaterialFactory.cs` and editor `Production/ProductionSceneSetup.cs`. Modify Bootstrap/ShipSurfaceMaps narrowly, preserving the legacy path when the production reference is null.

Use this runtime boundary (types listed here are implemented by you):

```csharp
public sealed class ProductionWorld : MonoBehaviour
{
    public TrackPath track;
    public ProductionMaterialLibrary materials;
    public string courseHash;
    public string artRevision;
    public void ValidateReady(); // throw/report missing references; never regenerate world
}
public sealed class ProductionMaterialLibrary : ScriptableObject
{
    public Material craftSurfaceTemplate;
    public Material craftMetalFallback;
    public Material craftGlass;
    public Material craftEngineAccent;
    public Material craftEngineCore;
}
```

Place `ProductionMaterialLibrary.cs` in World. The craft factory owns only runtime instances it creates, supports the existing mapped craft materials/team tints, and disposes them once. It never destroys persistent templates. Preserve the existing ShipSurfaceMaps legacy overload or adapt all legacy callers without changing their behavior. Construct exactly one gameplay camera with current ChaseCamera behavior; use production renderer settings and retain scene-authored lighting/volume. Skip legacy SSR/opening preview setup for this scene.

Implement editor entry points, in namespace `VectorRush.Editor`:

```csharp
ProductionSceneSetup.ExportContext();
ProductionSceneSetup.ImportArt();
ProductionSceneSetup.BuildScene();
ProductionSceneSetup.BakeScene();
ProductionSceneSetup.BuildCandidate();
```

Every entry parses explicit command-line flags and fails with an explanatory error for missing input. Shared flags: `-productionArtPackage <absolute package dir>`, `-productionEvidence <absolute fresh evidence dir>`, `-productionBuildOutput <absolute fresh .app path>`. ExportContext requires only evidence output. Never infer a package by modification time; the selected revision is explicit.

Export actual geometry-context JSON, preserving the contract arrays/course hash and camera formulas/settings. Send its absolute path and hash to Astra. Write `C1_STATUS.md` with source changes and exact boundaries. This checkpoint does not wait for art or a parent review.

## C2 — importer, saved geometry and rendering proof

Create DTOs in `Production/ProductionArtManifest.cs` and validation/import code in `Production/ProductionArtImporter.cs`. JsonUtility-compatible serializable classes and arrays are sufficient; no package dependency is needed. Validate the entire package before touching the active production scene. Reject bad version/hash/path, missing asset/material, duplicate IDs, NaN/Infinity, non-unit quaternion, negative/non-unit placement scale and absent READY.

Importer behavior:

1. Copy the immutable payload into its revision folder, preserving original hashes; set FBX scale/camera/light import and texture color-space/channel settings per contract.
2. Reimport and check Unity mesh bounds against declared bounds (absolute dimension error <=0.02 m or relative <=0.5%, whichever is larger). Check orientation with the asymmetric specimen. A mismatch blocks import instead of scaling everything until it looks plausible.
3. Create persistent URP Lit materials from the named slots/maps. Serialize actual normal/mask/emission references and keywords, avoiding the old runtime-only shader-retention failure. Clone production pipeline/renderer/features rather than mutating shared legacy assets. Keep fog variants consistent with the saved production scene.
4. Generate the collision road from the existing authoritative method, and visual track surfaces from Astra's profile and road material. Keep the current collision envelope and racing alignment. Save meshes/textures as assets and attach no legacy cleanup generators to static scenery.
5. Instantiate exact layout transforms into zone roots. Add LODGroups, set bake participation and lightmap UV generation only when required. Unrelated decorative meshes receive no collider.
6. Run full-course oriented clearance checks and return offending instance IDs/positions to Astra. Do not silently move a visible structure farther away to make the check pass.
7. Create lights/fixtures, environment grade and local baked reflection volumes from data. Add moving-craft probe coverage. Persist lightmap/GI settings, with deliberate UV/atlas allocation. Test one representative area before a full bake.
8. Save only NocturneProduction. Reopen it and verify meshes/materials/lighting references, then build a fresh standalone to prove persistence.

A tiny asymmetric test object and diagnostic light room may be generated inside your coding evidence area to test the importer before Astra is ready. Tag them `DIAGNOSTIC_ONLY`; final packaging asserts none remain. They do not replace any required art family.

Useful mechanical checks: package traversal rejected before any destination mutation; checksum mismatch preserves last usable scene; duplicate IDs rejected; persistent materials survive scene reopen; startup has one track/world/camera; no original app or legacy scene changed. Write real regression tests for those failure-prone boundaries, not tests asserting artistic constants.

## C3 — native integration with the art worker

Import the first READY revision. Render unedited native exemplar views and a normal-speed section, with explicit scene/build/package identity. Send paths, logs and concrete issues to Astra. You may inspect your own native output for implementation mistakes; do not call that independent acceptance.

Use four issue categories: `IMPORT` (scale/axis/maps), `PLACEMENT` (clearance or hidden intended structure), `LIGHTING` (fixture/coverage/material intent mismatch), `PERFORMANCE` (measured cost). Supply instance/material IDs and views. Astra publishes a new revision for art fixes; you fix importer/code faults. Never modify the artist's FBX or paint over screenshots.

Measure a composed dense view with six craft and the real kit before full replication. Record frame times, memory, object/light counts and conditions; pass technical findings to Astra. Limit lightmap resolution/LOD/culling based on measurements, retaining visible structure. Report an unresolved cost/quality conflict; do not silently collapse geometry into primitives.

## C4 — complete specified racing functions and audiovisual plumbing

Preserve baseline physics, camera and AI speed constants for this first implementation candidate unless fixing a reproducible correctness defect. Add sector/traffic/assistance telemetry for the later human quality review. No automation result is evidence that handling became fun.

**Finish lifecycle.** Preserve the player's final time/position when it crosses the valid finish. Continue only unfinished rivals for up to 60 seconds; give them valid simulation inputs while the player/result UI is finished. Mark remaining rivals DNF on timeout. Permit immediate restart/quit. Handle pause without advancing simulation/finish timeout. Update HoverVehicle phase gating as well as Director. Keep finish timestamps independent of a presentation clock and expose immutable per-racer finish records.

**Persistent records.** Implement `Gameplay/RaceRecords.cs`: save best valid lap and race keyed by course hash plus versioned driving-rules ID; ignore automated evidence/autopilot runs for player personal bests. Results show current time, previous best and signed delta. Persist across restart/relaunch. Use atomic replacement under Application.persistentDataPath; malformed files fall back safely without crashing and do not erase unrelated files. A changed course/rules ID starts separate records.

**Preferences/input.** Implement `Presentation/PlayerPreferences.cs` with versioned defaults: master/music/effects volumes (0..1), steering sensitivity (0.5..1.5, default1), shake toggle, keyboard bindings for existing actions. Preserve existing default keys and controller bindings. Reject conflicting required bindings or supply a clear reassignment flow; offer reset defaults. Persist changes. Give every menu/results/settings action keyboard, pointer and controller focus/activation; pause UI must not retain drive input. Preserve the HUD's visual direction.

**Audio/feedback.** Extend RaceAudio so motor load follows throttle and boosts while airflow remains speed driven. Add spatial rival voices with approach/pass-by behavior and a bounded simultaneous-voice budget, impacts and gallery/exterior response. Keep audible race signals distinct from propulsion/music. Sol owns any audio synthesis scripts and original procedural score production, or may use clearly licensed local material with recorded provenance; no purchased assets or unlicensed songs. Put the actual playable audio payload in the candidate, not a future-music stub. Synchronize existing IonPropulsion/VehicleVFX demand with these states. Make levels adjustable through preferences.

Required behavioral test cases (implement with the existing project test framework):

- Player finishes while two rivals remain: player result/time never changes; rivals keep simulating; actual later crossings are recorded; timeout marks only nonfinishers DNF.
- Restart during post-finish simulation clears old state/events and creates a valid new race. Pause freezes timeout and neutralizes drive/recovery inputs.
- First manual result creates a record; slower result preserves best; faster result replaces it; other course/rules hash is isolated; automated evidence result is excluded; corrupted file loads defaults; relaunch retains the last valid record.
- Saved volume/sensitivity clamp to valid ranges; duplicate binding is rejected/resolved; reset restores defaults; controller focus can reach/activate all actions without moving the paused craft.
- Existing ordered-gate, recovery, AI and input tests remain passing. Add tests to behavior you change, not a blanket count target.

Human fairness, subjective mixing and controller feel remain `NOT_REVIEWED` for the owner gate. If you cannot produce the specified audio payload or a function, list the exact incomplete item; do not set implementation-complete.

## C5 — full-course integration, validation and handoff

Wait for Astra's complete package with all four zones and ART_HANDOFF. Import that exact revision, finish production lighting/probes, build the whole circuit and run three complete automated native races with restart. Verify pause/results/settings/records using the actual app where tools allow; distinguish hardware-controller tests not performed. Inspect the selected native outputs yourself only to catch implementation faults and assemble the review package.

Run Editor tests into a new evidence path rather than overwriting historical XML. The existing command wrapper is a reference; use the installed Editor directly with a fresh result directory. Do not overlap Unity Editor/native runs with Blender rendering.

Build a fresh `Builds/Vector Rush-production-NN.app` using `BuildCandidate`. The build entry opens the saved production scene and fails if the original app path or an existing output is supplied. Use the existing supported standalone backend/options, production rendering assets and referenced shader variants. Do not call legacy Prepare. Include build report/GUID/source/art hashes in evidence.

Capture:

- 12 original 1920×1080 native views: entry/middle/exit for each zone, with actual poses and capture labels;
- an uninterrupted full-lap preview with actual game sound, ordinary playback speed and stated capture method;
- the gallery→station→viaduct section with a nearby rival where reproducibly possible;
- crowded start, contact/recovery, finish/results and settings views;
- separate warmed real-time frame-delivery/memory sample, without capture/encoder/bake overlap. Report provisional P95<=12ms/P99<=16.7ms results and >33.3ms spikes honestly, including failures.

Create `tools/production/play-production.command` that launches the exact handed-off app in its new scene directly, with no hidden legacy-preview flag. Launch it once to verify the path. Follow handoff-and-stop.md, report all remaining technical limitations, then STOP. No review agents, automatic aesthetic correction cycle, release publication or next phase.
