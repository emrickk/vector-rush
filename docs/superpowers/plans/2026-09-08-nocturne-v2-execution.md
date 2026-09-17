# Nocturne V2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Use the project's explicitly assigned asset/review agents only with separate file ownership; one integrator owns shared runtime files and native execution.

**Goal:** Improve the existing Nocturne game toward the four consistent V2 appearance targets, first proving a limited native passage and then extending accepted work across the circuit.

**Architecture:** Retain the current Unity game, authored craft and route. Test the installed reflection feature in a separately named app, then improve road lighting, connected architecture and surface finish in separate reviewed milestones. Reuse existing native evidence components and preserve control/candidate identities throughout.

**Tech Stack:** Unity 6000.6.0f1, URP 17.6.0 Forward+, C#, Blender 5.2.0, Apple M2 Max macOS, existing Python validation and FFmpeg.

**Spec:** [Nocturne V2 direction, reference review and gap criteria](../../../references/nocturne-v2/README.md), [illustrated comparison](../../visual-targets-v2.html), and the retained [production design and acceptance contract](../specs/2026-09-08-night-production-finish-design.md). Use [reflection feasibility 005](../../road-reviews/005-reflection-feasibility.md) for source findings and limitations. The [earlier detailed production plan](2026-09-08-night-production-finish.md) retains the implementation recipes for later asset, racing and delivery work; the order and baseline below supersede its stale road-experiment steps.

## Global constraints

- Unity 6000.6.0f1, URP 17.6.0, Apple Silicon macOS; retain the existing Forward+ pipeline.
- Keep the current night direction, six racers, three laps, useful HUD and original asset identity.
- Freeze route, player physics, AI, camera and craft geometry through Tasks 1–5. A later gameplay correction must follow an observed defect in Task 6.
- Preserve the three road-fixture renderer groups with shadow casting disabled.
- Preserve corrected fog, the readable control, editable source, Unity metadata, previous deliveries and rejected candidates. Never rewrite published corrective history.
- No phase/shield mechanics, full HUD replacement, engine migration, package upgrade or third-party rendering dependency belongs to the initial reflection experiment.
- Review all five opening crossings at .15/.22/.30/.37/.43, two thermal views, the warm-gallery middle, and the station reveal. Generated pixels are appearance references; native source/camera geometry remains authoritative.
- Native visual evidence uses ordinary physics and the racing camera. Label simulation-time capture, automated steering, continuous viewing and manual play separately.
- Target 1920×1080 on Apple M2 Max; verify 1280×800 and 1920×810 presentation before delivery.
- The inherited **provisional** performance budget is P95 ≤ 12 ms and P99 ≤ 16.7 ms, with a regression over 25% against a fresh comparable control requiring review. The last control P99 was **16.74 ms**, already above that provisional threshold. Do not round it into a pass, silently relax the budget, or attribute it to the rejected smoother-road candidate.
- No Editor build, baking or video encoding overlaps a performance sample. Never stop unrelated user processes as a shortcut to isolation.
- At most two substantial correction rounds per visual milestone; then preserve the failure and revisit its diagnosis. A critical failed benchmark dimension blocks circuit rollout.
- Commit and push each completed candidate, correction and verdict with its evidence. Native release publication remains separately authorized.

## Starting point and scope

Execution began at the owner's request. SSR01 and SSR02 are preserved native experiments; the current reflection approach has not passed production acceptance. Reference milestone `3c29f48` follows restored-control milestone `23362cc`. Current native app GUID is `40bed5f53c2449418e7fb56bf59739f6`. The restored app's existing 42 tests, 128.32 s automated race, restart checks, captures and performance scope are in [HANDOFF](../../HANDOFF.md). Reuse those records; do not repeat baseline work simply to make a new folder.

The immediate deliverable is **one SSR availability/appearance result**, followed by a retain/reject decision. Later tasks are independently reviewable follow-on work. This plan does not promise that one rendering switch will reproduce the generated targets.

### Work sequence

| Task | Concrete result | Reference / exit gate |
| --- | --- | --- |
| 1. Reflection feasibility | Separately built native preview with verified SSR off/on comparison | Existing geometry/materials/lights; actual feature/pass, useful visible response, no destructive artifacts |
| 2. Road and light finish | A readable dark road with broad broken highlights and coherent light pools | A plus all opening views; B/C and station stay readable; motion and cost checked |
| 3. Connected city | A limited near/middle service layer that joins track supports and existing buildings | A; preserve the signal mast, driving line and sky gaps |
| 4. Architectural finish | Convincing gallery returns and thermal service construction | B/C; strengthen existing assets without replacing their identity |
| 5. Craft finish | More distinct pearl, graphite, glazing and metal on the current ship | D in a fresh native rig, then A/B/C at chase size |
| 6. Integrated experience | The benchmark holds together in continuous motion, real racing and manual play with sound | All required visual/experience dimensions and technical gates |
| 7. Circuit rollout and delivery | Accepted rules applied across the lap, then one verified local build and current previews | Full circuit and required aspect ratios; original and failed evidence retained |

### Ownership and file map

Paths are repository-relative. New file names here are proposed implementation locations, not files created by this planning step. Preserve each Unity asset's `.meta` when it is introduced.

| Owner / task | Files | Responsibility |
| --- | --- | --- |
| Integrator / 1 | Create `UnityProject/Assets/Editor/RoadReflectionPreviewSetup.cs`; create `UnityProject/Assets/Scripts/Presentation/RoadReflectionPreview.cs`; modify `VectorBootstrap.cs`, `Assets/Settings/VectorRenderer.asset`, `Assets/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/ProjectSettings.asset` only as required | Separate preview build, retained feature/resources, runtime off/on volume and diagnostics |
| Integrator / 2 | `UnityProject/Assets/Scripts/World/WorldBuilder.cs`, `NightTrackLighting.cs`, `UnityProject/Assets/Editor/VectorRushSetup.cs`, road material assets | Road maps, material retention and direct-light/fixture changes in separately evaluated steps |
| Environment author / 3–4 | New `SourceAssets/environment-v6-context/` recipes, `.blend`, FBX, export audit and integration record; new `UnityProject/Assets/Resources/Art/NocturneContext/` assets | Small reusable service kit with measured dimensions, material names and clear regeneration instructions |
| Integrator / 3–4 | `UnityProject/Assets/Scripts/World/NightDistrict.cs`, `NightLandmarks.cs`, `NightArchitectureFinishes.cs`, `NightTrackLighting.cs` | Placement, reservations, lighting and material assignment; shared world assembly has one editor |
| Craft author / 5 | New `SourceAssets/hero-v3/pass-05-finish/` and `pass-05-maps-01/`; `tools/ship/bake_materials.py` only if the recipe requires a change | Geometry-preserving source finish and baked maps with unchanged UV/anchor contract |
| Integrator / 5 | `UnityProject/Assets/Scripts/Presentation/ShipSurfaceMaps.cs`, `ShipInspection.cs`, `VectorBootstrap.cs`, ship material/map assets | Native import, fresh neutral inspection and gameplay integration |
| Gameplay/presentation owner / 6, if a defect is observed | `HoverVehicle.cs`, `ChaseCamera.cs`, `RaceAudio.cs`, `IonPropulsion.cs`, `VehicleVFX.cs`, `RaceHUD.cs`, existing behavior tests | Only the bounded correction justified by the observed issue |
| Critic | New reports under `docs/visual-target-reviews/` | Native-image and actually observed motion critique; no runtime edits |
| Integrator | `evidence/nocturne-v2/`, handoff, implementation plan, history and README | Identities, selected originals, capture validity, milestone records and delivery |

Existing world interfaces remain `WorldBuilder.Build(TrackPath)`, `NightDistrict.Build(WorldBuilder, TrackPath)`, `NightLandmarks.Build(WorldBuilder, TrackPath)` and `NightArchitectureFinishes.Apply(Material, string)`. Do not invent an `AIDriver` subsystem; the existing AI is in `HoverVehicle`.

## Task 1 — prove the reflection technique on the current game

**Deliverable:** `Builds/Vector Rush-SSR-preview-01.app`, native off/on evidence and `docs/visual-target-reviews/001-reflection-feasibility.md`. This separate output avoids replacing the current control app while the technique is unproven.

**New interfaces:** `RoadReflectionPreviewSetup.EnableDefine()` adds the compile symbol without replacing existing symbols; `RoadReflectionPreviewSetup.BuildPreview()` prepares, retains the feature and builds the separate app; `RoadReflectionPreview.Configure(VolumeProfile profile)` adds the diagnostic volume configuration. The runtime option is `-vrRoadSSR on|off`; missing or invalid values keep SSR disabled. These interfaces are for this prototype, not new player-facing menu settings.

- [x] Record the current branch/commit and control app hashes in `evidence/nocturne-v2/ssr-01/control-identity.json`. Retain the existing app and source restore point. Confirm no other editor owns the shared settings. Copy the existing reference/control metadata; do not rebuild the control.
- [x] Add only `URP_SCREEN_SPACE_REFLECTION` to Standalone scripting symbols, in an Editor method compiled without SSR type references. End that Editor invocation; compilation/domain reload occurs before the separate preview-build invocation. Do not edit the package cache or add an engine dependency.

The define update uses the installed API and preserves other symbols:

```csharp
var target = UnityEditor.Build.NamedBuildTarget.Standalone;
var symbols = new System.Collections.Generic.HashSet<string>(
    UnityEditor.PlayerSettings.GetScriptingDefineSymbols(target)
        .Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
symbols.Add("URP_SCREEN_SPACE_REFLECTION");
UnityEditor.PlayerSettings.SetScriptingDefineSymbols(target,
    string.Join(";", symbols));
UnityEditor.AssetDatabase.SaveAssets();
```

- [x] In the next invocation, verify that `ScreenSpaceReflectionRendererFeature` and `ScreenSpaceReflectionVolumeSettings` compile. Put references behind `#if URP_SCREEN_SPACE_REFLECTION`; the build method's `#else` must throw an explicit unavailable-feature exception. Create/find one feature subasset in `VectorRenderer.asset`, set `afterOpaque=false`, and activate it. Preserve SSAO and all existing renderer settings. Save the renderer, resources and feature before building so native stripping sees the active feature.

The installed feature supports the following concrete registration pattern, after `VectorRushSetup.Prepare()`:

```csharp
var renderer = UnityEditor.AssetDatabase.LoadAssetAtPath<
    UnityEngine.Rendering.Universal.UniversalRendererData>(
    "Assets/Settings/VectorRenderer.asset");
if (!renderer) throw new System.InvalidOperationException("VectorRenderer missing");
UnityEngine.Rendering.Universal.ScreenSpaceReflectionRendererFeature ssr = null;
foreach (var feature in renderer.rendererFeatures)
    if (feature is UnityEngine.Rendering.Universal.ScreenSpaceReflectionRendererFeature found)
        ssr = found;
if (!ssr)
{
    ssr = UnityEngine.ScriptableObject.CreateInstance<
        UnityEngine.Rendering.Universal.ScreenSpaceReflectionRendererFeature>();
    ssr.name = "Nocturne SSR preview";
    UnityEditor.AssetDatabase.AddObjectToAsset(ssr, renderer);
    renderer.rendererFeatures.Add(ssr);
}
ssr.afterOpaque = false;
ssr.SetActive(true);
UnityEditor.EditorUtility.SetDirty(ssr);
UnityEditor.EditorUtility.SetDirty(renderer);
UnityEditor.AssetDatabase.SaveAssets();
```

- [x] Call `RoadReflectionPreview.Configure(profile)` beside the existing runtime grade-profile creation, before assigning that profile to the global Volume. Read the explicit on/off flag and apply this one starting configuration. Keep the road's cubemap sampling off, original `.35–.50` mask endpoints and `.90` multiplier, normals, fog, lighting, craft and race logic unchanged. The effect's own material opt-out `_SCREENSPACEREFLECTIONS_OFF` must not be enabled on the road.

The configuration body, inside the SSR compilation guard, uses the pinned source's actual public fields:

```csharp
var args = System.Environment.GetCommandLineArgs();
int index = System.Array.IndexOf(args, "-vrRoadSSR");
bool enabled = index >= 0 && index + 1 < args.Length && args[index + 1] == "on";
var ssr = profile.Add<UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings>();
ssr.mode.Override(enabled
    ? UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings.ReflectionMode.OpaquesOnly
    : UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings.ReflectionMode.Disabled);
ssr.resolution.Override(UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings.Resolution.Half);
ssr.upscalingMethod.Override(UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings.UpscalingMethod.Bilinear);
ssr.marchingMethod.Override(UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings.MarchingMethod.Linear);
ssr.maxRaySteps.Override(32);
ssr.maxRayLength.Override(30f);
ssr.hitRefinementSteps.Override(5);
ssr.objectThickness.Override(.325f);
ssr.finalThicknessMultiplier.Override(.05f);
ssr.roughnessFilter.Override(UnityEngine.Rendering.Universal.ScreenSpaceReflectionVolumeSettings.RoughReflectionsQuality.GaussianBlur);
ssr.roughnessScale.Override(0f);
ssr.reflectionStrength.Override(1f);
ssr.minimumSmoothness.Override(.05f);
ssr.smoothnessFadeStart.Override(.1f);
ssr.reflectSky.Override(false);
ssr.temporalFiltering.Override(false);
UnityEngine.Debug.Log("VR_SSR_REQUESTED " + enabled);
```

- [x] Build to the separate app path. `BuildPreview()` calls `Prepare()` once, registers/saves the feature, then invokes `BuildPipeline.BuildPlayer` for `Assets/Scenes/Solstice.unity`, `BuildTarget.StandaloneOSX`, and the separate output above; throw if `report.summary.result != BuildResult.Succeeded`. Do not call the ordinary `BuildMac()` afterward: it prepares again and overwrites `Builds/Vector Rush.app`.
- [ ] Verify **native** feature/volume settings, retained resources, depth/normal/smoothness data and the actual SSR pass. Use an attached native profiler/GPU capture to identify the SSR marker or its output; the installed source also names `SSR - Upscaling`. A type name, requested flag or enabled checkbox alone does not prove execution. If native pass inspection is unavailable, record that technical gate as unverified rather than inventing a pass.
- [x] Capture SSR-off and SSR-on in the same preview binary using the native commands below. First confirm that preview-off visually reproduces the current control; unexplained off-state differences invalidate the comparison. Select natural crossings and retain pose differences. Check all opening views, thermal controls, warm gallery and station, not only a favorable road crop.
- [ ] Watch the whole opening interval continuously. Inspect screen-edge disappearance, craft/rail occlusion, bright-source clipping, black propagation, history trails, bank/turn response and the ordinary slowdown. Opaque-only SSR is not expected to reflect transparent exhaust. A 30 m ray limit does not test all distant city reflections.
- [ ] If the effect has useful visible gain, collect separate off/on/off real-time performance samples with captureFramerate disabled and stable display conditions. Preserve each sample, including bad ones. The inherited budget stays provisional; compare both cost and absolute thresholds.
- [x] Commit the candidate/evidence, then the verdict separately. A compilation/resource/pass failure, unusable artifacts, no useful whole-frame gain, or unacceptable measured cost rejects this prototype. Preserve it and retain the exact control. Write one evidence-based alternative technique proposal before new rendering work; do not repeat arbitrary smoothness/probe-strength changes or silently upgrade URP.

The preview build can carry every setting identically while the volume mode supplies the off/on difference. Selecting a useful renderer is the exit from this task, not completion of the production appearance target.

**Execution result (2026-09-08):** SSR01 rejected for reflection-camera RenderGraph failures. Corrected SSR02 source/evidence is preserved at `dd7a417`; its two native runs exit 0 with 2,880 intact PNGs. CPU SSR pass samples are positive on and zero off, but GPU input/output contents remain unverified. [Independent review 001g](../../visual-target-reviews/001g-ssr02-visual-review.md) rejects useful whole-frame improvement after all nine pairs. The original app remains intact. The unchecked motion and GPU-data items are not passed; conditional performance sampling was not triggered. The prototype experiment has a reject result, while selection of a useful reflection technique remains unresolved. See [updated verdict](../../visual-target-reviews/001-reflection-feasibility.md) and [alternative proposal](../../visual-target-reviews/001h-next-technique-proposal.md). Tasks 2–7 remain pending.

## Task 2 — road and light finish

**Files:** existing `WorldBuilder.cs`, `NightTrackLighting.cs`, `VectorRushSetup.cs`, road material retention assets. **Output:** `evidence/nocturne-v2/road-light-01/` and `docs/visual-target-reviews/002-road-light.md`.

- [ ] Use the retained Task 1 result as control. If SSR was rejected, first resolve the technique decision; do not describe direct lighting as completed reflections. Record whether the selected approach supplies reflected visible structure or only direct highlights.
- [ ] Make one direct-light/visible-fixture adjustment in the existing opening light group, so illumination and visible source agree. Use fewer, broader shaped pools with dark intervals; keep warm and cool controls. Judge the whole frame before changing surface maps.
- [ ] If surface response still needs shaping, make one separate material candidate based on the observed defect: alter the spatial dampness distribution to produce broad broken regions, retaining panel/edge readability and the control's normal field. Do not raise the already rejected smoothness endpoints by habit. Record actual native mask range and material keywords, not only nominal endpoints.
- [ ] Inspect the five opening views and B/C/station. Watch the continuous interval for shimmer, hard fan boundaries, popping, streaks across the line and overbright cyan. Require a clear improvement in at least the ordinary approach and bend, with no critical loss at any other primary view.
- [ ] Measure relevant rendering cost separately, and preserve the accepted values, source identity, comparison and critic's verdict. Reject a result whose benefit is confined to a crop or a still that fails in motion. Commit/push each meaningful candidate and correction without squashing.

**Gate:** dark readable road, broad source-related response, preserved craft grounding and meaningful whole-frame improvement toward A. The generated road is a sheen ceiling; reproducing all its fine grain or its apparent transparent-plume reflections is not required.

## Task 3 — connect the opening city's near and middle structure

**Files:** `SourceAssets/environment-v6-context/`, its imported `Art/NocturneContext/` kit and `NightDistrict.cs`. **Output:** `evidence/nocturne-v2/context-01/` and `docs/visual-target-reviews/003-connected-city.md`.

- [ ] Project the current recorded A cameras against existing supports, landmark reservation and building footprints. Identify the largest visible black/disconnected lower-city area. Author **three reusable modules** for that area: a service podium/retaining face, a recessed maintenance entrance, and a narrow platform/catwalk with supported ends. Establish metric dimensions, origin, axes, material names and placement bounds in `INTEGRATION.md` before export.
- [ ] Use the existing Blender Python/CLI pipeline to save a versioned `.blend`, recipe, FBX and export audit. Audit finite geometry, triangle degeneracy, UV/tangent validity and dimensions after FBX round-trip. Do not regenerate or replace the existing mast, thermal plant or ship for this kit.
- [ ] Place a bounded set beside the opening passage using the existing track frames and `NightLandmarks.Overlaps` reservation contract. Check all course branches for road/collider clearance. Connect visible support feet and podium faces; suppress a competing repeated tower only where it blocks the established mast silhouette. Keep the primary road and its collision geometry unchanged.
- [ ] Compare all opening views and the continuous approach/exit. Check that the same modules read consistently across adjacent frames, remain below/outside the racing corridor, and create a clear foreground/middle/background relationship. A beautiful isolated studio module cannot pass the native composition gate.
- [ ] Commit the source kit and integration with its evidence, then preserve the independent view-based verdict. Limit correction to the demonstrated placement/light issue before creating more modules.

**Gate:** A's lower city reads as connected construction at ordinary chase size, while the road line, paired signal mast and sky gaps remain clear. More building count is not the criterion.

## Task 4 — finish the gallery and thermal context

**Files:** `NightDistrict.cs`, `NightLandmarks.cs`, `NightArchitectureFinishes.cs`, `NightTrackLighting.cs`, the V6 service kit. **Output:** `evidence/nocturne-v2/architecture-01/` and `docs/visual-target-reviews/004-architecture.md`.

- [ ] In B, retain the existing shell, chamfered ribs, hatch and exit. Improve the selected large panel returns and hatch recess with supported joints and differentiated existing finish materials. Use `NightArchitectureFinishes.Apply` where the current material contract permits; custom `NightWindows` does not automatically gain Lit's local-light/reflection behavior.
- [ ] Re-aim or redistribute the existing warm wash to reveal ceiling/wall returns before adding light count. Keep reflected road light and the craft's warm/cool transition consistent with the accepted Task 2 treatment. Treat additional lamps as a separate measured change.
- [ ] Reuse the service kit around C's existing pipe supports/platform and add a limited readable entrance/access level. Preserve the three-vessel silhouette and reserved footprint. Fix the known mast tangent issue before introducing any tangent-space normal map to that affected mesh; the old non-normal-mapped pass does not establish readiness for new normal maps.
- [ ] Capture B, both C controls, opening regression views and station. Review approach, closest passage and exit in motion, including geometry contacts, wall hatch backing and light transitions. Keep the latest opening context as the regression control.
- [ ] Commit gallery and thermal changes as separate reviewable steps. Advance only if each improves construction/light response without creating a competing bright focal point or losing the racing line.

**Gate:** B's structure is materially convincing and C reads as a supported service facility. A small useful kit is preferred to copying every generated railing, drain and lamp.

## Task 5 — craft materials, with geometry locked

**Files:** `SourceAssets/hero-v3/pass-05-finish/`, `pass-05-maps-01/`, `ShipSurfaceMaps.cs`, the existing native inspection and material import paths. **Output:** `evidence/nocturne-v2/craft-01/` and `docs/visual-target-reviews/005-craft.md`.

- [ ] First capture the **current** ship under the calibrated native neutral rig, plus warm/cool ordinary gameplay. D's historical comparator uses older maps and cannot substitute for this baseline.
- [ ] Preserve vertex positions, topology, UVs, livery positions and engine anchors. Adjust source-driven pearl/graphite/metal roughness and restrained contact detail in a new finish pass. Keep glazing borders and the previous contact correction intact. Do not add weathering or fasteners as a substitute for material separation.
- [ ] Bake with the existing UV/channel contract: sRGB base color, linear metallic RGB/smoothness alpha, green occlusion, tangent normal orientation. Verify map identities and import settings. Keep the old maps available for a geometry-invariant native control.
- [ ] Inspect emission-off neutral geometry/materials, emission-on engines, and A/B/C at ordinary chase scale. Ensure rival tint variations preserve material response. Retain compact engine cores and throttle-driven short exhaust; do not enlarge the rims to imitate an incidental generated highlight.
- [ ] Commit the finish recipe/maps and native verdict. Reject a studio-only improvement that makes the ship flat, clipped, glittery or unreadable during racing.

**Gate:** visibly distinct pearl, graphite, glass and metal on the existing ship under neutral and gameplay lighting. D's invented seam details do not mandate new geometry.

## Task 6 — integrated benchmark, motion and play

**Files:** existing evidence components and only the behavior/presentation files justified by an observed issue. **Output:** `evidence/nocturne-v2/benchmark/` and `docs/visual-target-reviews/006-integrated-benchmark.md`.

- [ ] Combine the accepted environment and craft milestones. Compare all five opening views, B/C and station to both the original control and the latest accepted intermediate. Require separate verdicts on composition, depth, surface response, craft read, visible racing and motion clarity. Do not average a failed category into an overall pass.
- [ ] Watch the uninterrupted 15-second opening interval and a complete lap at the true playback cadence. Record actual observation and defect timestamps. Perform manual acceleration/coast/brake/boost, both galleries, contact/recovery, pause, restart and finish with sound. If human-quality manual/audio observation is unavailable, record the missing gate and obtain that evidence; silent decoding does not satisfy it.
- [ ] Diagnose the existing slowdown and visibility of rivals from actual controller/contact telemetry and observed footage before changing gameplay. If a bounded grid/driver/camera/audio correction is justified, execute the corresponding explicit procedure in Task 5 of the [earlier production plan](2026-09-08-night-production-finish.md#task-5-refine-visible-racing-motion-and-audio), preserving player pace and controller safeguards until a demonstrated defect supports changing them. Use actual visible pursuit/alongside moments; behind-camera proximity alone does not pass racing presentation.
- [ ] Run existing behavior tests after changed logic and preserve a failing physical regression case for any new controller fix. Do not add tests that mirror art constants. Validate race/reset behavior in the native game after gameplay changes.
- [ ] Check the effects-suppressed diagnostic specified by Task 6 of the earlier plan, preserving environmental light, fog, geometry, navigation and HUD. Require the scene to remain spatially readable without relying on exaggerated additive effects.
- [ ] Collect separate real-time performance/race evidence on the selected app. Require the actual three-player-lap result, zero final recoveries for all six, both restart launches and three countdown-pause checks. The director freezes unfinished rivals when the player finishes; never relabel this as all six independently finishing.
- [ ] Commit/push the integrated benchmark verdict. Critical visual/motion failures block rollout. Keep unknown coverage explicit and diagnose after two substantive correction rounds instead of accumulating unreviewed changes.

## Task 7 — roll out accepted rules and deliver

**Files:** the already-owned environment/material files; final evidence, versioned local app/archive, README, handoff, regeneration record and development history. **Output:** `evidence/nocturne-v2/final/` and `docs/visual-target-reviews/007-final.md`.

- [ ] Extend only accepted construction/light/material rules through opening/cool-gallery, thermal/service and final-station/warm-gallery districts. Work one district at a time, preserving previously accepted contexts and quiet connecting stretches. Follow the full-lap procedures in Tasks 7–8 of the earlier production plan with this plan's current baseline and new evidence directory.
- [ ] After each district, inspect approach/busiest view/exit and watch affected transitions for seams, reflection dropout, lighting changes and facade shimmer. Commit each accepted district separately.
- [ ] Freeze the final source and build identity. Run the relevant existing tests once; capture the final circuit and opening/thermal/gallery/station at 1920×1080, 1280×800 and 1920×810. Check HUD and actual visible opponents, not only architecture.
- [ ] Complete the final applicable native race/restart checks, continuous-view/manual-audio evidence and separate performance measurements. Reuse still-valid checks on the exact same app; repeat only when later changes invalidate them.
- [ ] Validate original PNG streams, complete video decoding, ZIP integrity and source/app/archive SHA-256 identities. Package a dated local app archive plus current continuous preview and lap, labeling cadence, automation and audio coverage. Update handoff/history and push source/assets/compact evidence. Do not overwrite older deliveries or publish a release without its separate authorization.

**Completion:** consistent native visual quality across ordinary and busy race views, satisfactory observed motion/manual play with sound, and an explicitly resolved performance verdict. A local material pass or clean compile cannot close the whole plan.

## Capture and evaluation procedure

Run from the repository root. Use a fresh evidence directory for each attempt. The preview app name below is produced by Task 1; it does not exist at this planning milestone. The same run pattern applies to later named candidates.

```bash
mkdir -p evidence/nocturne-v2/ssr-01/off evidence/nocturne-v2/ssr-01/on
./Builds/'Vector Rush-SSR-preview-01.app'/Contents/MacOS/'Vector Rush' \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -vrRoadSSR off \
  -environmentEvidence "$PWD/evidence/nocturne-v2/ssr-01/off/full-lap" \
  -environmentFullLap \
  -logFile "$PWD/evidence/nocturne-v2/ssr-01/off/native-capture.log"
```

After the off process exits, repeat using `-vrRoadSSR on` and the `ssr-01/on` directory. These are 1,440-frame, 24-simulation-fps native traversals, not real-time performance samples. Preserve each source/app manifest with the actual separately named app paths in `appFiles`.

The existing capture validator accepts an explicit control and enforces resolution:

```bash
python3 evidence/night-production/lighting-road/verify_capture.py \
  evidence/nocturne-v2/ssr-01/off \
  --reference evidence/night-production/road-response/control/capture-validation.json
python3 evidence/night-production/lighting-road/verify_capture.py \
  evidence/nocturne-v2/ssr-01/on \
  --reference evidence/nocturne-v2/ssr-01/off/capture-validation.json
```

Write `build-identity.json` before invoking the validator. It expects `files` and `appFiles` dictionaries mapping **repository-relative paths** to SHA-256 hashes, and creates `selected/` once. Its existing control contains eight selections; also retain the warm-gallery middle as a ninth comparison, selecting the natural crossing of progress **0.8730698824** from the report and copying the original PNG plus its full frame metadata. Do not compare solely by frame index. All prior exact-pose tolerances failed; retain the new measured deltas rather than assuming a match.

For separate real-time profiling, use the same app with `-vrRoadSSR off`, then on, then off, one process at a time:

```bash
./Builds/'Vector Rush-SSR-preview-01.app'/Contents/MacOS/'Vector Rush' \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -vrRoadSSR on \
  -evidencePath "$PWD/evidence/nocturne-v2/ssr-01/performance-on" \
  -autopilot -quitAfterEvidence \
  -logFile "$PWD/evidence/nocturne-v2/ssr-01/performance-on.log"
```

Record warm-up, actual resolution/display/VSync, native process exit, frame-interval distribution, memory and any hitch. No simulation capture or encoding may overlap. Observed intervals include display/CPU scheduling and are not isolated GPU cost. Preserve the exact app through all comparisons.

The ordinary wrapper remains `./tools/unity.sh test` and `./tools/unity.sh build` for non-preview milestones. `build` already calls `Prepare`; do not habitually run a separate `prepare` first. Save the wrapper's common test/build outputs under the current candidate before a later run replaces them.

## Why SSR is an experiment

The installed package contains compile-guarded feature/volume/pass implementations, and its public fields were re-read for this plan. Unity's [official preview announcement](https://discussions.unity.com/t/preview-of-screen-space-reflections-for-urp/1721494) describes screen-visible color/depth sampling and its offscreen limitations. The [official issue reproduction](https://issuetracker.unity.com/issues/24156/ssr-all-gameobjects-keep-rendering-black-after-restoring-a-valid-transform-scale-when-screen-space-reflections-is-enabled) documents the early-version scripting-symbol opt-in and a black-output fault. Neither source establishes that this project's native Metal player passes the feature's availability, appearance or performance gate.

The generated targets show appearance, including proposed geometry and richer illumination. They do not identify the renderer that could reproduce it. Task 1 therefore tests the most bounded existing technique before treating it as the production route.

## Planning self-review

- The four reference views have owners, implementation locations and native acceptance checks; the primary five-view opening benchmark is retained.
- Native app identity, the old studio comparator, generated-image drift, sheen limits and the provisional performance-budget discrepancy remain explicit.
- Preview off/on uses the same binary and frozen scene/material/light inputs. Type/resource availability and actual native pass evidence remain separate from visual judgment.
- Existing later-task recipes are retained with corrected current baseline, evidence directory and wrapper preparation order. No new runtime API in this plan is presented as already implemented.
- No game, Editor build, asset bake or native performance test was launched to write this plan. Execution begins with Task 1 when implementation is requested.
