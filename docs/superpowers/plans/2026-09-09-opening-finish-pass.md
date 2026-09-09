# Opening finish pass — execution

Owner authorized the next pass at High effort after [the parallel critique and recommended approach](../../visual-target-reviews/002-improvement-approach.md). Scope is one visibly improved opening passage plus a small warm-gallery surface calibration, not circuit rollout or a craft redesign. This revises the order of Tasks 2–3 in the Nocturne V2 plan. The [Nocturne V2 reference contract](../../../references/nocturne-v2/README.md) remains the appearance authority.

## Global constraints

- Keep Unity 6000.6.0f1 / URP 17.6 Forward+, route, camera, six racers, three laps, physics, AI, craft geometry/maps, fog and postprocessing fixed.
- Park SSR. Preserve original app GUID40bed5f53c2449418e7fb56bf59739f6 and SSR02 app/source. New output is `Builds/Vector Rush-opening-01.app`; never call ordinary BuildMac or overwrite the original.
- Preserve the three outdoor fixture groups' shadow-casting exclusions. No package-cache edits, upgrades or third-party dependencies.
- Focus construction on visible opening .15/.22/.30; .37/.43, thermal views, warm gallery and station remain regression controls except the explicitly scoped gallery surface study.
- Audit/reuse existing assets first. Add at most three connection/module types, with real support relationships, full-track clearance and landmark reservations. No new skyline ring or global exposure lift.
- Road material work must concern broad spatial response and controlled source coverage; no global smoothness-endpoint increase, arbitrary probe tuning or camera-dependent painted highlights.
- Native normal racing camera/physics is authoritative. Diagnostic/frozen/simulation-time evidence must be labeled. Static frames do not prove motion, audio or performance.
- Use native mode `-vrOpeningFinish off|construction|surface|combined`, default off. Compare same-binary states. Construction and surface sources have separate owners; one integrator runs Unity and commits shared settings/evidence.
- Target 1920x1080. Existing provisional P95<=12ms/P99<=16.7ms and >25% regression review remain; original control P99 16.74ms is not a pass. No build/bake/encoder overlaps performance.
- At most two substantial visual correction rounds, preserving each failed candidate and diagnosis. Commit/push candidate and independent verdict separately. No release publication.

## Task 1 — preview controls and separate build

**Owner:** setup implementer. Files: new `Assets/Scripts/Presentation/OpeningFinishPreview.cs` + meta; new `Assets/Editor/OpeningFinishPreviewSetup.cs` + meta; `Assets/Scripts/VectorBootstrap.cs` for configuration log only; `Assets/Scripts/EnvironmentEvidence.cs` for an opt-in nine-anchor draft capture. Work at `/Users/anping/Documents/Stuff/AI Space/Vector Rush`.

- [x] Implement static `OpeningFinishPreview.ConstructionEnabled` and `.SurfaceEnabled` (public bool properties) parsed from `-vrOpeningFinish off|construction|surface|combined`. Missing/invalid value means off. Use ordinary managed initialization only; no Unity native allocations in static/field constructors. Log actual chosen state once via `OpeningFinishPreview.LogConfiguration()` called at startup. Diagnostics must not alter camera/physics.
- [x] Add `OpeningFinishPreviewSetup.BuildPreview()` calling existing Prepare once, preserving existing SSAO/default renderer while disabling the project SSR preview feature. Do not edit/remove prior SSR scripts or dependencies. Use supported ManagedCodeVariant.Instrumented and BuildOptions.None consistently for all art modes; state configuration honestly.
- [x] Output fixed default `../Builds/Vector Rush-opening-01.app`; support optional `-vrOpeningPreviewOutput` for later separate revisions, rejecting the original app path and existing output directories rather than overwriting evidence. Build Solstice for StandaloneOSX; throw unless BuildReport succeeds. Keep Unity execution and all generated settings to integrator.
- [x] Add `-environmentOpeningStills` opt-in evidence mode: fullLap/stillsOnly, nine natural thresholds .15/.22/.30/.37/.43/.58051056/.61970216/.87306988/.94621426; all1440 poseframes retained, only9originalPNGs. Ordinary capture modes unchanged; dynamic anchor-count reporting/validation. No camera/physics/race edits.
- [x] Report source self-checks and Unity compile pending; do not run Unity or commit others' files. Own task report `.superpowers/sdd/2026-09-09-opening-finish-pass/task-1-report.md`. No subagents.

## Task 2 — connected opening construction

**Owner:** environment implementer. Files: `Assets/Scripts/World/NightDistrict.cs`, optional new `OpeningDistrictFinish.cs` + meta and source recipe/audit under `SourceAssets/environment-v6-context/`. No WorldBuilder, lighting owner files, Bootstrap or Editor settings. Work at `/Users/anping/Documents/Stuff/AI Space/Vector Rush`.

- [x] Read [environment critique](../../visual-target-reviews/002c-improvement-environment-critique.md), original native frames and target A. Audit current code/recorded camera poses for opening .15/.22/.30. Distinguish existing but hidden/unlit assets from missing construction. Record placement intent and measured world bounds before authoring.
- [x] Gate all new construction/local reveal lights under `OpeningFinishPreview.ConstructionEnabled` (interface Task1). Integrate in NightDistrict after needed reservations exist. Reuse existing authored source pieces where meaningful; rough in missing interfaces using editable procedural recipes first. Preserve existing control content when off.
- [x] Compose two limited connected groups around crossing viaduct/inner frontage: support-to-podium interface, recessed service frontage, short platform with supported ends (at most three types). Connect to actual support feet and real frontage. Reserve/check all course branches and existing landmarks; no racing colliders, new obstruction, duplicated hidden foundations, or opaque wall filling every under-track gap.
- [x] Local practical lights must reveal broad top/front/recess differences and have visible source housings. Reallocate/reuse existing lighting where reasonable; explicitly report added light count. Keep fog/global lighting unchanged; preserve shadow exclusions. Make ordinary .15/.22 visibly more connected, .30 at least preserve depth, and .37/.43 avoid clutter.
- [x] Include opt-in preview telemetry of chosen group world bounds, clearance result and count. No cameras/poses moved. Own materials/meshes properly; no native allocations in constructors.
- [x] Write task report with file ownership, source/placement audit, counts, regeneration instructions and checks. Do not run Unity or commit other files. Report `.superpowers/sdd/2026-09-09-opening-finish-pass/task-2-report.md`. No subagents. Root will review/build/native-capture before detailed asset expansion.

## Task 3 — opening lighting and surface response

**Owner:** surface implementer. Files: `Assets/Scripts/World/WorldBuilder.cs`, `NightTrackLighting.cs`, optional new surface helper + meta. No NightDistrict, Bootstrap, Editor or project settings. Work at `/Users/anping/Documents/Stuff/AI Space/Vector Rush`.

- [x] Read visual/rendering critiques002a/002b and inspect native opening149/283 and warm943 plus generatedA/B. Check actual road/material map construction and gallery UVs before proposing treatment; gallery pieces must not blindly use the landmark UV contract.
- [x] Gate changes under `OpeningFinishPreview.SurfaceEnabled` (Task1 public property). Off must preserve original road/maps/light construction. Restrict new road treatment to opening passage and warm-gallery study where feasible; if shared-material limitations require a distinct region material, preserve batching/clearance and report scope explicitly. Do not silently alter full circuit.
- [x] Author a coherent small opening light group: illuminated approach, darker transition and lit bend with sources corresponding to actual fixtures. Aim, coverage and intensity may work together; mere pool translation is insufficient. Preserve overall night hierarchy, fog and the three caster-off groups. No new lamp count unless a documented bounded source model warrants it.
- [x] Create road-scale broad broken response using spatial organization, restrained normals and material separation. Retain nominal smoothness endpoints .35–.50 and .90 multiplier; no blanket rise, baked camera-dependent bright streaks or tiny noise as headline. Direct specular is not claimed reflected structure. Preserve seams/driving readability.
- [x] Use warm gallery as bounded calibration: wall faces vs dark ribs/metal returns and recesses must read through coherent light/material contrast. Existing geometry may receive distinct materials, but do not redesign tunnel or craft; no normal-map assumptions without valid UV scale. Keep cool gallery and thermal/station inputs stable.
- [x] Record actual material/lighting changes and native hypotheses; root captures surface-only and combined modes. Own task report `.superpowers/sdd/2026-09-09-opening-finish-pass/task-3-report.md`. Do not run Unity or commit other files. No subagents.

## Task 4 — native iteration, review and delivery

**Owner:** integrator; independent source and visual critics get bounded diff/image packages.

- [x] Review all source tasks before building. Run appropriate existing tests once after integrated source compiles; no tests mirroring art constants. Preserve clear source/base/app identities and original control hashes.
- [x] Build separate preview, run off and combined native representative captures. Verify same-binary off broadly reproduces control before judging candidate. Capture construction/surface component views as needed; defer complete full-lap reruns until draft is visibly useful.
- [ ] Independent whole-frame critique: clear improvement ordinary approach and bend (.15/.22), depth retained.30; regression all fiveopening + two thermal +warm +station. Preserve original pixels/pose differences. Reject negligible or cluttered candidates; at most two substantive corrections with diagnosis.
- [ ] Watch continuous opening where tooling genuinely supports observation, label coverage honestly; inspect transitions/occlusion/flicker. Obtain native realtime same-binary off/combined/off race/performance after a useful visual result, with no concurrent heavy tooling. Preserve failures and budget limitations.
- [ ] Preserve separate candidate and verdict commits/push, update comparison, handoff/history and bounded remaining work. Original app remains retained; selected candidate separate. Tell owner when this pass is complete and whether remaining tasks suit lighter reasoning. Do not call broader rollout complete or manufacture missing manual/audio/motion gates.

### Native round record

- Opening01 candidate `ce50b9e`: four sparse native modes validated; independent003c rejects useful finish. Main failures are hidden frontage at.22, pale/slab-dominant disconnected construction and flattened road response. Original app preserved.
- Correction1 / opening02: same attachments, narrower dark walks, compact raised occupied fronts with lit support contacts; metric architectural finish maps. Restore road detail at original frequency beneath broad response atlas, retain detail shader variant explicitly, shape existing-source coverage. Recheck all changed clearance volumes and native nine views before later gates. Implementers own disjoint source; integrator owns Unity/settings.

- Correction1 native opening03: valid baseline, useful construction, rejected regional road/cloudy response (003f). Two technical shader-retention failures are preserved underopening02.
- Correction2/final: restore original road mesh/material/maps in every mode by disconnecting OpeningRoadFinish from WorldBuilder; retain construction and gallery/fixture study. Evaluate combined once, then use construction-only if this retained study still regresses. No third substantive art round. Road reflected-source finish remains unresolved.
