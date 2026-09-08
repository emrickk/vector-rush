# Night Racing Production Finish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring the existing night circuit toward the supplied video's integrated level of finish, proving one representative racing passage before expanding the treatment around the lap.

**Architecture:** Keep the current Unity game and improve its existing world, material and presentation components. One integrator owns composition and shared assembly; specialists own separate asset or presentation files, and an independent critic owns acceptance reports. Runtime checks observe the normal race and keep visual, watched-motion, manual-play and performance evidence distinct.

**Tech Stack:** Unity 6000.6.0f1, URP 17.6.0 Forward+, C#, Blender 5.2, Apple Silicon macOS, existing native evidence components and FFmpeg.

**Spec:** [Night racing production finish: design and acceptance](../specs/2026-09-08-night-production-finish-design.md). Read it with [the reference brief](../../../references/night-production-video.md) and [independent acceptance critique 019](../../environment-reviews/019-production-finish-plan-critique.md).

## Global Constraints

- Unity 6000.6.0f1, URP 17.6.0, Apple Silicon macOS; retain the existing Forward+ pipeline.
- Keep the current night direction, six racers, three laps, useful HUD and original asset identity.
- Freeze route, player physics, AI, camera and craft geometry through the environment/material stages; consider bounded motion changes only in Task 5 of the implementation plan.
- Preserve the three road-fixture renderer groups with shadow casting disabled.
- Keep editable source, Unity metadata, prior deliveries, rejected candidates and published history.
- No phase/shield system, eight-racer expansion, full HUD replacement or engine migration.
- Review five natural route crossings at progress .15, .22, .30, .37 and .43, plus thermal and final-station controls.
- Native visual evidence uses ordinary physics and the racing camera; injected poses and recording-only pace changes cannot establish acceptance.
- Observed motion, manual play with audio and technical checks are separate gates; unobserved coverage remains pending.
- Target 1920×1080 on Apple M2 Max; verify 1280×800 and 1920×810 presentation.
- Proposed working performance budget: P95 ≤ 12 ms and P99 ≤ 16.7 ms; review regression over 25% against a fresh comparable baseline.
- No Editor build, baking or video encoding overlaps a performance sample.
- Allow at most two substantial correction rounds before revisiting the diagnosis or technique; a critical below-target judgment blocks circuit expansion.
- Commit and push completed source/evidence milestones; native release publication remains a separately authorized action.

---

## Delivery order and ownership

This is a visual production plan, with a later bounded racing/presentation stage. Numerical light and surface choices are native experiments, not inferred settings from a reference video. Each task ends in a reviewable candidate and recorded verdict. A local task pass does not establish overall finish.

| Task | Reviewable result | Owner and dependency |
| --- | --- | --- |
| 1 | Honest baseline, five views and a 15-second reference passage | Integrator; first |
| 2 | Readable large lighting shapes, atmosphere and credible road response | Integrator; after 1 |
| 3 | Connected nearby city and substantial track context | Environment author; after 2 |
| 4 | Craft and rival material separation under the accepted lighting | Craft finish author; after 2, reviewed with 3 |
| 5 | Visible ordinary racing, restrained motion/effects and a coherent sound mix | Gameplay/presentation author; after 3–4 |
| 6 | Integrated benchmark passes visual, motion/play and technical gates | Integrator and independent critic; after 5 |
| 7 | Accepted rules applied across three circuit districts | Environment author and integrator; after 6 |
| 8 | Full-lap acceptance, verified native package and documented delivery | Integrator and independent critic; after 7 |

Tasks 3 and 4 may run in parallel on separate files. The integrator alone edits `WorldBuilder.cs`, `VectorBootstrap.cs`, rendering settings and the bootstrap scene. Task 5 transfers ownership of nominated gameplay/presentation files explicitly; it does not overlap another editor of those files. The critic changes review documents only. Coordinate any shared-file patch before applying it.

Paths below are repository-relative. `U` in this plan means `UnityProject/Assets`, and `E` means `evidence/night-production`; expand them before running a command. Preserve `.meta` files beside changed assets. Keep raw frame sequences local, consistent with `.gitignore`; commit compact selections, metadata, reviews and manifests, not caches or native binaries.

## Task 1: Establish the ordinary-race benchmark

**Files:** Read `evidence/lighting-depth-candidate-01/full-lap/environment-evidence.json`, its `frames/` originals, `evidence/lighting-depth-candidate-01/source-manifest.json` and `evidence/lighting-depth-final/build-manifest.json`. Create `E/baseline/selection.json`, `E/baseline/selected/`, `E/baseline/benchmark.mp4` and `docs/environment-reviews/020-production-baseline.md`.

**Interfaces:** Consume the existing `EnvironmentEvidence.Report`: `buildGuid`, `complete`, and `frames[]` with `index`, `file`, `lap`, `progress`, `raceTime`, `speedKph`, camera/FOV and racer state. Produce a selection manifest containing unchanged selected frame records, thresholds, source report path and source build GUID. All later visual comparisons use these crossings, not EnvironmentEvidence's built-in final-station anchors.

- [x] Preserve the native baseline GUID `5c49b92643bd4d41820ba81857fdf8c7` and source delivery `a948825`; verify existing source hashes before capturing a fresh performance baseline. Do not rebuild merely to create baseline selections.
- [x] Copy five unmodified first-crossing frames using the recipe below. Include the existing thermal views `frame-0606.png` / `frame-0641.png` and station view `frame-1004.png` as global-change controls, labeled separately from the opening benchmark.
- [x] Encode original frames 121–480 at their recorded 24 simulation fps, producing the baseline's 360-frame / 15-second continuous passage. Record its approximately .119–.439 route interval and 5.17–20.13 s race interval, including the slowdown near .33–.39. This is silent automated native rendering, not a performance sample.
- [x] In review 020, describe the largest visible failure at each view and draw a simple layout marking foreground route, nearby structure, middle district, skyline and intended bright surfaces. Use actual-frame annotations only as design studies; keep originals alongside them.
- [x] Record each acceptance dimension as below / approaching / meets, or pending where motion has not been watched. Record a fresh standalone real-time baseline using the real-time switches in Task 6 before any runtime changes, with output `E/baseline/performance` and log `E/baseline/native-performance.log` so later candidate measurements cannot overwrite it.
- [x] Commit and push `docs: establish the night production benchmark`, including the updated implementation plan and development history.

Run this selection recipe from the repository root. It rejects the initial progress near .99 as a false crossing and never injects a pose:

```python
import json, shutil
from pathlib import Path
source = Path('evidence/lighting-depth-candidate-01/full-lap')
out = Path('evidence/night-production/baseline')
report = json.loads((source / 'environment-evidence.json').read_text())
assert report['complete']
frames = report['frames']
thresholds = [.15, .22, .30, .37, .43]
selected = []
(out / 'selected').mkdir(parents=True, exist_ok=False)
for threshold in thresholds:
    crossing = next(b for a, b in zip(frames, frames[1:])
                    if a['lap'] == b['lap']
                    and a['progress'] < threshold <= b['progress']
                    and 0 < b['progress'] - a['progress'] < .05)
    original = source / crossing['file']
    assert original.is_file(), original
    shutil.copy2(original, out / 'selected' / original.name)
    selected.append({'threshold': threshold, 'frame': crossing})
(out / 'selection.json').write_text(json.dumps({
    'source': str(source / 'environment-evidence.json'),
    'buildGuid': report['buildGuid'], 'selections': selected
}, indent=2))
```

```bash
ffmpeg -n -framerate 24 -start_number 121 \
  -i evidence/lighting-depth-candidate-01/full-lap/frames/frame-%04d.png \
  -frames:v 360 -c:v libx264 -crf 17 -pix_fmt yuv420p \
  evidence/night-production/baseline/benchmark.mp4
```

For later candidates, use the same crossing logic with their own report and a fresh output directory. Record camera, speed and timing differences; do not label route-aligned frames as exact-pose matches. If Task 5 changes traversal time, show the complete same route interval at its true recorded speed and label its new duration rather than retiming it to 15 seconds.

## Task 2: Establish the light, atmosphere and road response

**Files:** Modify `U/Scripts/World/WorldBuilder.cs` (`Build`, `BuildLighting`), `U/Scripts/World/NightTrackLighting.cs` (`Build`), and, only for demonstrated sky/fog or retention needs, `U/Shaders/NightSky.shader`, `U/Editor/VectorRushSetup.cs`, `U/Scenes/Solstice.unity`, `U/Settings/VectorPipeline.asset`, `U/Settings/VectorRenderer.asset`. Create `U/Resources/RoadSurfaceReflections.mat` with its Unity `.meta`; retain `U/Resources/RoadSurface.mat` as the no-environment-reflection control. Create `E/lighting-road/decision.md` and separate baseline/A/B evidence directories.

**Interfaces:** Keep `WorldBuilder.Build(TrackPath path)`, `NightTrackLighting.Build(WorldBuilder world, TrackPath track)` and `WorldBuilder.MakeMaterial(string name, Color color, float smooth=.5f, float metallic=0, Color? emission=null, string templateName="SurfaceLit")`. The material resource name passed through `templateName` selects a serialized retained variant. No gameplay or public component contract changes are needed.

- [ ] Mark the five views' intended broad illumination: readable neutral road/craft surfaces, a cool atmosphere, and limited warm service/magenta accents. Adjust ambient/key/fog together as one documented lighting candidate; compare thermal/station controls before accepting a global change. Do not compensate by raising every window or adding a row of tiny lamps.
- [ ] First keep road environment reflections disabled and use visible fixture/architectural sources to establish direct specular response. Preserve road geometry, normal field and the three proven non-casting fixture groups. Save this native candidate A before testing a different reflection state.
- [ ] Duplicate the current RoadSurface material in the Editor as `RoadSurfaceReflections.mat`. In the duplicate set `_EnvironmentReflections` to `1`, remove `_ENVIRONMENTREFLECTIONS_OFF`, retain direct specular and the existing normal/metallic-map keywords, and save the asset. Native inclusion is verified by the visible response and the retained asset, not `shader.isSupported` alone.
- [ ] Switch only the running deck template to `RoadSurfaceReflections` for candidate B; initially keep its mask, normal map and source lighting identical to A. Evaluate the existing 256 px probe while moving through the bends. If reflections cannot describe the nearby sources consistently, test a bounded probe volume centered on this passage with box projection and one initialization capture; do not introduce per-frame probe rendering.
- [ ] Review broad highlight shape, reflection stability under banking, road joints, edge readability, diagonal facets, abrupt probe boundaries and craft spill. A brighter patch without a plausible source is a failure. Stop after two substantial correction rounds to reassess technique. Consider costlier reflection methods only through a separately documented feasibility comparison after the simpler approach demonstrably fails.
- [ ] With the reflection state selected, adjust roughness variation once against both moving highlights and ordinary dark road. Remember the packed mask alpha is multiplied by the material smoothness: the current effective range is about .315–.45. Preserve the reference material and record accepted mask/keyword/probe values in `decision.md`.
- [ ] Build and capture using the commands in Task 6; inspect all five views plus both global controls, and take an isolated performance sample if probe/rendering cost changed. Commit and push `feat: establish production night lighting and road response` only with the actual verdict and remaining issues recorded.

The one-line integration change is the existing deck creation call's `templateName`; it is not permission to replace its geometry:

```csharp
road = MakeMaterial("Satin graphite running deck",
    new Color(.13f, .145f, .165f), .9f, 0f,
    templateName: "RoadSurfaceReflections");
```

## Task 3: Give the city and route continuous construction

**Files:** Environment author modifies `U/Scripts/World/NightDistrict.cs` (`Build`, `BuildUrbanFabric`, nearby placement/clearance helpers), `U/Scripts/World/NightLandmarks.cs` and, if facade hierarchy requires it, `U/Shaders/NightWindows.shader`. Reuse the current resources `Nocturne_PlatformButtress_A.fbx`, `Nocturne_ServiceWorkshop_A.fbx`, terrace/split tower assets and landmark kit under `U/Resources/Art/Environment/`. If the actual-camera layout reveals a missing connection, create only that bounded source/export in `SourceAssets/environment-v6-connections/`, recording its recipe, dimensions, slots and importer metadata. Create `E/city/placement-review.md`.

**Interfaces:** Preserve `NightDistrict.Build(WorldBuilder world, TrackPath track)`, `NightLandmarks.Build(WorldBuilder world, TrackPath track)` and `NightLandmarks.Overlaps(Vector3 center, Quaternion rotation, Vector2 halfSize)`. Placement consumes `TrackPath.Evaluate(float)` and existing clearance checks. Integrator alone applies any change to WorldBuilder's track support/deck assembly.

- [ ] At each primary view, identify one exposed gap between track, support and surrounding service level. Place existing buttress/workshop/podium forms to close that specific visible gap, with clear bearing/connection and road clearance. Preserve the mast's sky gap and landmark reservations.
- [ ] Compose a nearby sequence from mast approach to first gallery: supported foreground structures, a few middle blocks with lower service faces, and a quieter skyline with deliberate gaps. Use large silhouette changes and occlusion/parallax, not more window seeds. Preserve breathing room at the bend and portal.
- [ ] Tune facade occupancy/emission by depth and building family. NightWindows already mixes fog but lacks Lit-style additional-light/probe response; use its actual properties deliberately, without assuming a new spotlight will illuminate those panes.
- [ ] If an existing asset cannot form a credible connection, author only the missing connector in the new staging folder. Preserve original Blender/export sources, audit full bounds/road clearance, then copy reviewed exports with metadata. The mast's existing tangent limitation precludes casually adding normal maps.
- [ ] Inspect the five native views and the whole ordinary passage for floating bases, blank walls, repeated skyline rhythm, lost turn visibility and gallery entry occlusion. Check the thermal and station controls for unwanted changes from shared facade/material edits. Record original frame/time evidence in `placement-review.md`.
- [ ] Commit and push `feat: connect the opening night district to the circuit`. This accepts the construction candidate only; Task 6 still owns integrated acceptance.

## Task 4: Make the craft read as an engineered object

**Files:** Craft finish author owns `U/Scripts/Presentation/ShipSurfaceMaps.cs`, `U/Resources/Art/ShipSurfaces/` and a fresh immutable finish pass under `SourceAssets/hero-v3/`. Integrator applies only agreed material-assignment changes in `U/Scripts/VectorBootstrap.cs`. Read current `U/Resources/CraftSurfaceLit.mat`, original hero FBX and engine anchors. Create `E/craft/material-review.md`.

**Interfaces:** Preserve `ShipSurfaceMaps.Create(WorldBuilder world, string name, string key, Color tint, float smoothness, float metallic)` and the existing `Ivory`, `Ceramic`, `Graphite`, `Metal` map keys. Preserve craft bounds, orientation, collider, hierarchy and `ShipEngineAnchors` names. Existing complete mapped surfaces set `_Smoothness=1`; changing a nominal creation argument alone does not change their packed roughness response.

- [ ] Review current hull, canopy, recesses and hardware under Task 2's lighting before touching geometry. Identify where casing reads flat, metal blends into ceramic, or a lit edge obliterates the silhouette.
- [ ] Adjust the actual tint/mask/occlusion payloads for the identified surfaces: broad controlled casing highlights, readable darker structure, restrained metal and a canopy separated from both. Keep player and rival color identities readable at race distance. Do not enlarge bevels or add microdetail to compensate for poor illumination.
- [ ] Preserve each generated map pass, channel/color-space contract and source. Validate completeness before copying to the live ShipSurfaces directory. Keep world materials separate from ship materials; prevent helper changes from silently affecting architecture.
- [ ] Capture the five primary race views plus a genuine close-rival view when available. Use neutral inspections only to diagnose persistent geometry issues. If lighting/material corrections leave a specific form defect, document that exact native view and a bounded geometry proposal before expanding this task; a whole ship rebuild is outside this plan.
- [ ] Record material readability with intended effects and with decorative effects suppressed during Task 6. Commit and push `art: refine craft surface separation for the night circuit` with its native scope stated.

## Task 5: Establish visible racing and coherent motion, effects and sound

**Files:** Gameplay owner may modify `U/Scripts/Gameplay/HoverVehicle.cs` (`ResetForRace`, only evidenced driver defects), `U/Scripts/Gameplay/ChaseCamera.cs`, `U/Scripts/Presentation/IonPropulsion.cs`, `U/Scripts/Presentation/VehicleVFX.cs`, `U/Scripts/Presentation/RaceAudio.cs`. Integrator owns any needed `VectorBootstrap.cs` assignment. Read `U/Scripts/PaceEvidence.cs` and existing `U/Tests/Editor/{RaceProgressTests,HoverVehicleInputTests,AIPursuitTests,RivalCorridorGuardTests}.cs`. Create `E/motion/decision.md` and `docs/environment-reviews/021-production-motion.md`.

**Interfaces:** Preserve `HoverVehicle.Initialize(TrackPath circuit, bool isPlayer, int index)`, public observed throttle/boost/speed/impact properties, `ChaseCamera.Initialize(HoverVehicle vehicle)`, `IonPropulsion.Initialize(HoverVehicle craft)`, `VehicleVFX.Initialize(HoverVehicle craft)`, and `RaceAudio.Initialize(RaceDirector raceDirector)`. Presentation consumes the real vehicle state; it does not force it. AI lives in HoverVehicle; there is no separate AIDriver file.

- [ ] Watch the integrated baseline passage at normal speed, then manually drive it with audio. Record the slowdown near progress .33–.39, visible-rival intervals, road-edge clarity and any camera discomfort before changing the driver. Determine whether the slowdown comes from contact/guard, turn demand, or the test driver; do not infer the cause from a slow screenshot.
- [ ] If rivals remain almost entirely behind, test a single ordinary grid change first: exchange physical slots 0 and 2 while preserving identity indices, racer names and tuning. Apply it inside `ResetForRace`, so normal start and restart both use it; keep 53/72 player pace and pursuit/guard settings fixed for this comparison. No capture-only arrangement is allowed.
- [ ] Use real pace telemetry and visible footage to judge the grid candidate. Require at least two actual pursuit/alongside moments after launch during lap one, with a rival meaningfully visible through an approach/bend in the benchmark. If the grid does not sustain that experience, diagnose the observed loss/contact before a separate bounded driver correction; preserve the failed candidate and current corridor safeguards.
- [ ] Assess camera distance, focus, FOV response and bank smoothing one contribution at a time. Keep road boundaries and nearby rivals visible in a crowded boost moment. Prefer scenery parallax and readable acceleration to extra roll or blur. Retain a before/after movie at its true capture cadence.
- [ ] Refine existing propulsion/trails/contact effects against actual throttle, coast, boost and impact. Keep the engine silhouette and HUD legible; no giant exhaust, added phase ability or center-screen spectacle. Assess the effects-off control in Task 6 before increasing additive brightness.
- [ ] Listen to acceleration, wind, boost entry/sustain/release, passing rivals, contact, pause and finish. Rebalance existing original sound layers for distinct events without clipping or a constant wall of noise. Record whether directional rival sound exists and reads clearly; any missing rival voice is a focused audio addition with actual listening validation, not an assumed benefit from a silent capture.
- [ ] Run existing tests after changed behavior. For a newly diagnosed controller defect, first preserve its failing physical case and add a regression that exercises that case, such as mirrored wall contact and safe release; do not add tests that merely repeat art constants. Native three-lap/restart/zero-recovery verification is mandatory after a grid/driver change.
- [ ] Record watched interval, manual inputs and sound observations in review 021, including pending coverage. Commit and push the grid/driver correction separately from camera/effects/audio changes, with each outcome described honestly.

The grid experiment changes only the physical-slot calculation in the existing reset method; retain its boost, progress and recovery reset operations:

```csharp
int slot = gridIndex == 0 ? 2 : gridIndex == 2 ? 0 : gridIndex;
float progress = Mathf.Repeat(-(14f + (slot / 2) * 12f) / track.Length, 1f);
// Keep existing reset operations between progress calculation and lane setup.
aiLane = (slot % 2 == 0 ? -1f : 1f) * Mathf.Min(5.8f, track.Width * .265f);
nextLaneDecision = 0f;
Reposition(progress, slot % 2 == 0 ? -4f : 4f);
```

This is a candidate to test, not a prediction that a starting-grid swap solves sustained racing. A different driving correction must name the observed defect and its test before implementation; do not use repeated speed reductions as a substitute for diagnosis.

## Task 6: Pass the integrated passage gates

**Files:** Create `E/benchmark-final/` with source/build identity, selected originals, clips and measured reports; create `docs/environment-reviews/022-production-benchmark-native.md`. Runtime changes are limited to demonstrated corrections in the owning task. For the effects-off diagnostic, use a preserved temporary build configuration; no new player-facing setting or capture harness is required.

**Interfaces:** Use existing `EnvironmentEvidence.TryStart(VectorBootstrap bootstrap)`, `PaceEvidence.TryStart(VectorBootstrap owner)` and RaceEvidence command-line switches. `-environmentFullLap` records 1,440 frames at 24 simulation fps; its five built-in anchors are still final-station controls. The independent critic consumes actual selected originals, the source reference and watched-motion records.

- [ ] Capture the current candidate's complete circuit, then select the five opening crossings. Show baseline/current pairs plus thermal/station controls. Record build GUID, source hashes, capture mode, resolution and pose/speed differences.
- [ ] Review all six dimensions—composition, depth, surface response, craft read, visible racing, motion clarity—with individual below / approaching / meets judgments and actual frame/time evidence. Do not average a failed outdoor view against a strong gallery shot. All six dimensions are required; approaching is progress, not acceptance. An unobserved motion dimension is pending.
- [ ] Preserve a separate diagnostic build with bloom, motion blur, extra speed trails, sparks and exaggerated additive exhaust suppressed. Retain environmental lights, fog, reflections, geometry, navigation and HUD. Repeat the same natural route crossings. Then restore the intended effects and recapture the busy-race case. Do not turn off all postprocessing or all emission and claim it is an equivalent control.
- [ ] Actually watch the uninterrupted passage at normal playback speed and a continuous full lap. Record observer, file/build, speed and defect timestamps. Separately record manual acceleration, coast, brake, boost, both galleries, contact/recovery, pause, restart and finish with audio. Missing playback/control access leaves this gate pending; decoder checks and automated steering do not replace observation.
- [ ] Run a separate real-time performance/race sample with this project's Editor closed and no build, bake or encoding concurrent. Compare to Task 1's fresh baseline and the historical mean 8.35 ms / P95 9.21 ms / P99 9.33 ms. Apply the stated working budget and 25% regression review; retain any bad first run and its unresolved cause.
- [ ] Close the benchmark only when all six visual/experience dimensions meet the stated target and the motion/play and technical gates pass. If a critical dimension is below target, correct its cause before rollout; after two substantial rounds revise the technique/diagnosis. Keep review 022 independent and preserve earlier failures.
- [ ] Commit and push `review: verify the integrated production night benchmark`, with open issues explicit if it is not yet accepted.

Native commands below are run sequentially from the repository root, using a fresh evidence directory for each run. Archive each build/test result under its candidate before a later run replaces the wrapper's common output files. `prepare` is required when changing serialized scene/setup or retained variants, not for every material-value edit.

```bash
./tools/unity.sh prepare
./tools/unity.sh test
./tools/unity.sh build
```

```bash
./Builds/'Vector Rush.app'/Contents/MacOS/'Vector Rush' \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -environmentEvidence "$PWD/evidence/night-production/benchmark-final/full-lap" \
  -environmentFullLap \
  -logFile "$PWD/evidence/night-production/benchmark-final/native-capture.log"
```

The separate real-time race/performance command has no simulation-time recording flag:

```bash
./Builds/'Vector Rush.app'/Contents/MacOS/'Vector Rush' \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -evidencePath "$PWD/evidence/night-production/benchmark-final/performance" \
  -autopilot -quitAfterEvidence \
  -logFile "$PWD/evidence/night-production/benchmark-final/native-performance.log"
```

Optional full-race pack telemetry is a different run and is not a performance sample:

```bash
./Builds/'Vector Rush.app'/Contents/MacOS/'Vector Rush' \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -paceEvidence "$PWD/evidence/night-production/motion/pack-01" \
  -paceSimulationRate 24 -paceCapture \
  -logFile "$PWD/evidence/night-production/motion/native-pack-01.log"
```

## Task 7: Roll the accepted treatment around the circuit

**Files:** Modify the same nominated environment/material/light files from Tasks 2–3, with the integrator retaining shared-file ownership. Create `E/rollout/opening/`, `E/rollout/thermal/`, `E/rollout/station/` and `docs/environment-reviews/023-production-circuit-native.md`.

**Interfaces:** Consume the accepted material/light values and construction rules recorded in Tasks 2–6. Retain the original component contracts, road clearance and six-racer race. District variations change composition and palette rhythm, not the quality bar.

- [ ] Extend the opening treatment through the start/crest/cool gallery transitions, preserving the benchmark as the control.
- [ ] Apply the same depth and surface principles around the middle thermal service district; emphasize its existing industrial identity and support connections instead of duplicating the mast architecture.
- [ ] Integrate the final station, warm gallery and skyline reveal, including quiet ordinary stretches between authored locations. Preserve compression/release and varied warm/cool rhythm.
- [ ] After each district, capture the full circuit and inspect its ordinary approach, busiest view and exit against the opening benchmark. Watch the affected interval for light/probe transitions, facade shimmer and reflection instability. A shared change also requires checking the already accepted controls.
- [ ] Commit and push each accepted district separately. Review 023 must explicitly cover ordinary stretches across the whole lap; three flattering landmark stills cannot close rollout.

## Task 8: Verify and deliver the finished circuit pass

**Files:** Create `E/final/`, `docs/environment-reviews/024-production-final-acceptance.md` and a dated local archive under `Builds/`. Update `README.md`, `docs/implementation-plan.md`, `docs/visual-environment-plan.md`, `docs/development-history.md` and `docs/asset-regeneration.md` for new source assets.

**Interfaces:** Final manifest maps the source revision and asset hashes to one native build GUID, app files, archive SHA-256, capture modes and review documents. README distinguishes current evidence from historical coastal/night deliveries.

- [ ] Freeze the accepted source candidate, run all existing and relevant new behavior tests, build once, and retain exact source/resource/settings identity. Rebuild only for a demonstrated correction and invalidate only the checks it affects.
- [ ] Capture the final 1080p full circuit and representative opening/thermal/station views at 1280×800 and 1920×810. Verify road, craft, opponents and HUD in each aspect. The default environment stills alone cover the station; use full-lap captures plus the selection recipe for opening and middle views.
- [ ] Complete the real-time three-lap race, all-six-racer zero-recovery, both restart launches and three countdown-pause checks on the final binary. Repeat performance only if source/rendering changes since Task 6 justify it. Preserve the comparable display conditions and all observed outliers.
- [ ] Have the independent reviewer watch the final continuous lap and confirm the manual-with-audio coverage on the accepted build. If a later district correction changed the observed experience, watch its affected interval again. Keep unresolved critical visual/motion failures open instead of declaring production finish from test results.
- [ ] Validate PNG chunk integrity/decompression, full video decoding, ZIP integrity and source/app/archive hashes. Preserve a current continuous preview and full lap labeled with actual cadence, resolution, automation and audio coverage. No stitched racing incident should be presented as continuous gameplay.
- [ ] Write the final acceptance report and honest remaining limitations. Commit and push source, original authoring assets, compact evidence and corrective history; package the native archive locally. Do not create a GitHub release without the separate release authorization.

## Completion criteria

The plan is complete when the night circuit consistently earns its visual target across ordinary and crowded race views, holds together in watched motion and manual play with sound, and stays within an explicitly accepted performance budget on the target Mac. A better local asset, a test pass or a claim of “AAA” is not a substitute for those observations.

**Execution status:** The owner has authorized Stage 1 of the six-stage summary: the prerequisite baseline (Task 1) and lighting/road response (Task 2). Later tasks remain planned. See the implementation log for current candidate and native review status.
