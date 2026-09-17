# Current integration status — September 17, 2026

The owner requested continuing here on the latest Game P4 version. Branch `integration/p4-night-city-20260917` combines P4 `b787b5f` and NightCityKit `250d221`, recorded by merge `bc91b58`. Verified 327 P4 scene, billboard, runtime-script and project-setting files are byte-identical to P4. The latest verified playable app remains **Billboards06.app** below.

`NightCityIntegrationSetup.cs` prepares a separate `NightCityStage7.unity` candidate with five facade-connected service terraces, kit prefabs and local doorway lights. It includes facade-end support checks, sampled road clearance, billboard overlap checks, and unchanged collider/display signatures. New integration tests are prepared. **These authoring changes are uncompiled and unrun; Stage 7 has not been generated.** Do not describe the merge or source preparation as playable integration completion.

Unity's sandboxed startup failed to connect to licensing IPC. Automatic approval review rejected the normal installed Editor launch and a read-only check of the task's process IDs; both rejections reported a review-stream `content_filter` failure. No policy/security setting was changed. The user explicitly approved the launch in the next turn. Both the read-only process check and the exact Editor launch were retried with that authorization, but automatic review again failed with the same content_filter stream error before execution. Authorization is established; do not ask for it again. The execution-review service remains the blocker. Startup session 95510 reported Unity PID 4569 and licensing PID 4646; inspect their current identity before terminating any stale process. Sandbox termination was not permitted.

Next, after the execution-review failure is resolved: run `NightCityIntegrationSetup.Inspect` to write `sites.json` into `/Users/anping.wang/output/vector-rush-p4-city-integration-2026-09-17`; inspect actual facade sites and adjust the explicit layout as needed, then run `Prepare` with a fresh evidence directory. Build the new scene using `ProductionSceneSetup.BuildExperienceCandidate` and `-experienceScene Assets/Scenes/NightCityStage7.unity`. Native driving inspection and correction, current tests, a separate race/performance run and moving review remain required. Keep the portable wrapper selecting the verified Stage 6 scene until Stage 7 works. Speed blur remains a separate subsequent integration decision. No publication or delegation.

---

# Current Vector Rush game — new-agent handoff

## Billboard architectural finish — September 17, 2026 (current local candidate)

Owner approved mounts/framing, brightness balance, animation pacing and hitch investigation. Current scene `Assets/Scenes/AnimatedBillboardsStage6.unity`, revision `animated-billboards-stage6-06`, build `e2e0c129118247348e77af4c98d57deb`. Candidate 05 placement/size/orientations are retained. Added cabinet lips, hoods and ledges, with braced supports reaching actual facades and two shared structural connections. Original buildings are not carved. Campaign-specific gains and periods, softer facade wash and slower tickers refine presentation. [Finish milestone and native comparison](current-game/billboard-finish-2026-09-17/README.md).

286 tests passed. Native driving and detail captures complete; the 46.101-second driving recording has uneven intervals and is not a performance benchmark. Two separate uncaptured 1080p three-lap runs passed six-finisher/zero-recovery and result/pause/restart checks. Run A had one 41.47 ms hitch with no recorded GC delta or focus change; run B had none above 33.3 ms. The old 190.926 ms stalls did not recur; no repeatable cause was found. Per-hitch diagnostics are opt-in evidence instrumentation.

Local review: `http://127.0.0.1:8773/review/`; app: `/Users/anping.wang/output/vector-rush-billboards-2026-09-17/Billboards06.app`. Course/collisions, handling, HUD, ship, fog/rain and dry tunnel remain preserved. Game P5 blur is separate. Owner visual acceptance remains open. Source remains local because automatic approval review blocked earlier GitHub publication; direct publication approval is pending. Next is owner review of the actual driving replay; no automatic wider rollout or agent dispatch.

## Billboard driving readability — September 17, 2026 (previous local candidate)

Owner rejected candidate 04's poor driving visibility and small signs. Current scene remains `Assets/Scenes/AnimatedBillboardsStage6.unity`, now revision `animated-billboards-stage6-05`, build `6b98ead89ba04809a6da20de39b5e546`. Main screens are larger/lower; the corner and wide landscape use approach-facing architectural planes; the triptych moved onto the visible outer bend. Five sparse clusters and actual animation remain. [Readability milestone and native comparison](current-game/billboard-readability-2026-09-17/README.md).

286 tests passed and the final native 43.029-second / 1,009-frame lap completed. Isolated three-lap run passed with six finishers, zero recoveries, and results/pause/restart checks; exact timing is in the milestone. Owner visual acceptance remains open. Stage 5 weather/dry tunnel, course/collisions, handling, ship and HUD remain preserved. The current local source includes unpublished billboard work: automatic approval review blocked the GitHub push; await direct publication approval in this task. Game P5 blur remains separate. Next is owner review from the actual driving camera, not acceptance based on close-ups.

## Animated city displays — September 17, 2026 (previous candidate)

Owner requested actual animated ads, then rejected the first layout for opening density, repeated heights/headings, harsh flicker and one sign per building. Current review candidate: `Assets/Scenes/AnimatedBillboardsStage6.unity`, revision `animated-billboards-stage6-04`, build `668888fd7612468c8a867cc8e3cb65ee`. The wrapper builds this scene. [Billboard milestone, native comparisons and measured evidence](current-game/billboards-2026-09-17/README.md).

10 screens form five architectural clusters: high portrait, stacked portrait/ticker, corner pair, synchronized triptych and low landscape/projecting blade. Four original Scenario video campaigns animate in Unity; one kinetic orbital object remains. Surface separation fixes the screen/housing overlap; filtering and removal of fine scanlines reduce distant shimmer. Stage 5 weather, dry tunnel, course, collisions, handling, ship and HUD are preserved.

286 tests passed. Final native evidence: 43.029-second lap / 1,009 frames with audio, and 35.008-second fixed-camera animation / 401 frames. Isolated 1920×1080 three-lap race completed with six finishers, zero recoveries, frozen results, pause and restart checks. Exact timing is in the milestone. Owner artistic acceptance and manual/controller feel remain open. Do not treat the rejected candidates 01/02 as accepted work. Game P5 speed blur is a separate unintegrated branch; do not silently overwrite or claim it here.

Next is owner review of opening density, facade composition and animation. No automatic wider-city rollout or agent dispatch. Preserve prior scenes and source identities.


## Atmosphere correction, September 17, 2026 (current)

Owner approved restoring outdoor haze, dry tunnel road, no sheltered rain/spray, and rain audio fading deeper inside. Current scene `Assets/Scenes/RainAtmosphereStage5.unity`, revision `rain-atmosphere-stage5-01`, build `0aea73895a504a62b9380a8ec3e39c83`. The portable wrapper builds this scene. [Atmosphere milestone and measured evidence](current-game/atmosphere-2026-09-17/README.md).

282 tests passed. A complete 43.029-second native lap has 1,009 timestamped frames at 23.4375 Hz and captured audio. Matched comparisons show the wet-to-dry road correction. The isolated 1920×1080 three-lap race passed with six finishers, zero recoveries, frozen results, pause and restart checks. Frame delivery: 8.33479 ms mean, 8.33334 ms P95/P99, 17.20417 ms maximum, zero frames above 33.3 ms.

Course hash remains `a43cff0540e9b86cc0d60810b1f19e6cd49ae52d69e41ea43f6fb5b590790abc`; driving geometry, collisions, handling, ship and HUD are preserved. Prior dry and rain scenes remain. Apex icon relay is preserved and this build succeeds with it. Next is owner review of haze, tunnel transitions and sound; no wider art rollout is authorized. Do not interpret technical checks as artistic acceptance.


## Rain candidate, September 17, 2026 (previous milestone)

The owner approved the rain presentation pass with handling unchanged. Current scene `Assets/Scenes/RainRoadStage4.unity`, revision `rain-road-stage4-02`, native build `79586df76e61472b8a858042da9574b5`. [Rain milestone and measured evidence](current-game/rain-2026-09-17/README.md). The portable wrapper builds this scene. 279 tests passed and a 43.029-second native lap recorded outdoor rain and tunnel shelter. Read the milestone for uncaptured performance results.

The corrected course hash remains `a43cff0540e9b86cc0d60810b1f19e6cd49ae52d69e41ea43f6fb5b590790abc`; all road collision meshes, dry scene, handling, ship and HUD are preserved. Next is owner review of rain intensity, wet reflection visibility and sound. No wider art rollout or traction change is authorized.


## Packaged application icon — September 17, 2026

Owner selected Apex (option 2). Its master and PNG/ICO/ICNS exports are in `SourceAssets/Branding/Apex`; Unity’s default Player Settings icon references `Assets/Branding/ApexIcon.png`. No current scene or gameplay change. Existing native bundles need a rebuild; Unity import/build verification remains pending after an isolated compiler-service startup failure. See the icon entry in `development-history.md`.

## Smooth road candidate, September 17, 2026 (previous milestone)

The owner approved correcting rough curves and abrupt banking. Current scene: `Assets/Scenes/SmoothRoadStage3.unity`, revision `smooth-road-stage3-01`, build `1dd288cb14fb4fb998f4f165a89a864a`. Use the portable wrapper for this candidate. [Road correction milestone and evidence](current-game/smooth-road-2026-09-17/README.md).

276 tests passed, three complete automated races passed with six finishers and zero recoveries each, and the 1,544-sample recorded underground camera check passed. The previous underground scene, HUD, ship and architecture remain preserved. Course identity changed to `a43cff0540e9b86cc0d60810b1f19e6cd49ae52d69e41ea43f6fb5b590790abc`; do not reuse prior records as equivalent-course data.

Next: owner review of corrected road motion and manual/controller driving feel. Wider environment polish remains subsequent work. No artistic acceptance or automatic expansion is claimed. Sections below retain previous milestone history.

## Underground finish candidate, September 17, 2026 (previous milestone)

The owner approved the underground finish pass and moved continuation from Game P2 to its follow-up task. The saved candidate now includes inhabited terraces, planting, podium fronts, wall recesses and continuous tunnel services. Build GUID `b0c358a8850b446da15227ead1951e33`, revision `underground-gallery-finish-03`. **273 tests passed; 405-frame lap and recorded camera check passed; three consecutive races passed with six finishers and zero recoveries each.** See [the finish milestone](current-game/underground-finish-2026-09-17/README.md) for source, measured performance, actual comparisons, local playable build and limitations. Course, five driving meshes and finished HUD baseline remain preserved.

This is ready for owner review, not artistic acceptance. Foliage silhouettes, facade repetition and concrete detail still trail the supplied reference. Manual/controller validation and wider-city work remain open. Do not redo the HUD or expand the full circuit automatically. Historical sections below retain the earlier publication/verification states; use this milestone for current evidence.

## Latest relay — 2026-09-17 (supersedes the publication pause below)

The owner supplied this GitHub branch after requesting alternating-agent continuation. Game work is resumed. Relay 01 verified a fresh checkout and rebuilt the existing underground candidate without game changes: **271/271 current tests**, complete **43.2 s / 405-frame native lap**, recorded camera-clearance check, and one separate **three-lap six-finisher race with zero recoveries**. Build GUID `98d13df67244414c923b8e1b64f3c643`; baseline scene and course identities are preserved. See [the relay results and next baton](current-game/relay-2026-09-17/README.md) and its machine-readable evidence.

Next: improve the sparse upper foundations and terrace/city connection in the existing underground candidate, then inspect actual continuous native motion and present a reference comparison. Completed HUD stays intact. Three consecutive races, human/controller validation and artistic acceptance remain open. The older unrun-check statements below describe the publication moment; use this relay's precise results for current validation coverage.

## Publication state, retained as history

Owner paused implementation on 2026-09-17 to publish this complete working-source checkpoint to the explicitly approved **public** repository `emrickk/vector-rush`. Publication branch: **`game/current`**. Existing GitHub branches remain untouched and are older. Clone this branch explicitly:

```sh
git clone --branch game/current --single-branch https://github.com/emrickk/vector-rush.git
cd vector-rush
python3 tools/verify-current-game.py
```

This is the current WIP game, **not a final accepted/released product**. Do not automatically resume game implementation just because the checkpoint exists. When the owner resumes, continue the underground candidate below; do not restart the design, regenerate the old coastal scene, or redo the finished HUD. Read root `AGENTS.md`, then this document. Older README/HANDOFF files and dated plans are historical when they conflict.

## Exact state at the pause

1. HUD is finished: real **Saira Black Italic / Medium Italic**, angled arcade instruments, live speed/energy, five cells, minimap and reduced-text display. Boost vibration and cyan afterglow affect the speed/energy panel only; Reduced Interface Motion suppresses them. Preserve this work.
2. Earlier environment structure introduced substantial deck/podium/viaduct support instead of thin roller-coaster-like posts.
3. The current **underground gallery** is implemented as a separate scene and has had two native visual iterations. The road actually descends another **22 m**, with matching road/shoulder/barrier geometry and a new course hash. Retaining walls, foundations, enclosed roof, portals and connected exit replace the exposed second-gallery canopy. Texture, service bays, coping, terrace rails and warm practical lights are included.
4. It remains a **review candidate**. Large upper foundation faces still feel sparse; the skyline/terrace integration and finish need judgment. Neither the whole environment nor this candidate has owner visual acceptance.
5. Owner explicitly stopped further work. The latest three-lap performance run was **terminated at the pause**, before a report was written. Do not call it passed. No game work continued after the pause; only publication/handoff preparation.

## Open and build the right scene

- Editor: **Unity 6000.6.0f1** (`f7f8ed4d1e24`). URP **17.6.0**. Exact packages are committed under `UnityProject/Packages`; there are no local file-package dependencies.
- Open `UnityProject` in Unity Hub. Allow a clean Library import/package restore. A normal Unity license and network access for registry packages are required.
- **Current candidate:** `Assets/Scenes/UndergroundGalleryStage2.unity`.
- **Preserved HUD-finished baseline:** `Assets/Scenes/EnvironmentStructureStage1.unity`.
- Other scenes are retained for source compatibility, tests and history. `Solstice.unity`, `NocturneProduction.unity` and old default build settings are **not** the current entry point.
- Open the chosen scene explicitly and press Play. Enter starts/confirms; WASD/arrows drive; Space boosts; Q/E airbrake; R recovers; Escape/P pauses. Physical-controller and manual-driving validation remain open.
- Existing serialized assets are ready to import. **Do not run `tools/unity.sh prepare`, old scene generators or Blender exporters to get the game running.** These can replace newer content with historical content.

Close this project's Editor before batch commands. The Unity CLI plugin is optional; the wrapper below invokes the pinned Editor directly. It assumes macOS for `.app` builds, matching current verified delivery. Set `VECTOR_UNITY_EDITOR` if installed elsewhere.

```sh
bash tools/current-game.sh open
bash tools/current-game.sh test /absolute/fresh/vector-rush-tests
bash tools/current-game.sh build /absolute/fresh/VectorRush.app /absolute/fresh/vector-rush-build
# Optional baseline comparison:
bash tools/current-game.sh baseline-build /absolute/fresh/VectorRushBaseline.app /absolute/fresh/vector-rush-baseline
```

Choose real absolute paths that do not exist yet. Native apps and build logs are intentionally not in Git. No Scenario, OpenAI or other image-generation API key is needed to open/build/play this snapshot. New image generation is a separate workflow, not a build dependency. Editable Blender source/export recipes and font licenses/provenance are included; regeneration tools may require their documented external tools.

## Verification: separate checked facts from pending work

Latest native build: **`2ef7de7505b34a03804ebf27bc4df955`**, revision `underground-gallery-stage2-02`.

- Second candidate built successfully and recorded a complete native lap: **43.136 s**, **377** timestamped frames at approximately 10 Hz, ordinary physics + automated steering. Not a performance benchmark or human playtest.
- Geometry validation: **7,200** lane-clearance ray checks, no new enclosure intrusion in the tested driving corridor. Maximum road/curve alignment error **0.1188 m**. Maximum course grade **0.3316** (33.16%, about 18.3°). Smooth arcade-racing grade, not civil-engineering realism.
- **271/271 EditMode tests passed on the first underground iteration.** This includes four new underground tests. The second build compiled after lighting/material corrections and the addition of a persisted-emission assertion. The complete suite **has not been rerun after those last changes**. The original test XML is included and clearly named.
- `UndergroundGallerySetup.ValidateCapture` was added and compiled to test recorded native camera segments and 0.35 m proximity against the new enclosure. **It was not run before the pause.** No completed camera-clearance report is claimed.
- Final three-lap performance/race run: **interrupted by owner pause**, no result. A full lap does not prove multi-lap AI/race reliability.
- Current screenshots were visually inspected; no generated overlay is being passed off as gameplay. Selected before/after images and the approved generated direction reference are in `docs/current-game/evidence/`.
- The preserved HUD baseline had 267 passing tests, 21 native phase/aspect shots, actual boost/reduced-motion captures and a three-lap race with zero recoveries in the previous milestone. Those are baseline results, not proof for the new course.

Current course hash: `e5b995eaedbda504165e40147acfbd697321b38a249fd1f08f770642b0a2dc4f`.
Baseline course hash: `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`.
Baseline scene SHA-256 at authoring: `83195acf00ea5140cb4600a1d686ca1cd1641683c7b16729aa8b7a57e4c2f7b5`.

Course identity intentionally changed; don't reuse old ghosts/records as equivalent-course data. The default `TrackPath` still produces the exact historical course hash; only the new scene opts in.

## Resume sequence when asked

1. Verify the download manifest; open the candidate; run the complete EditMode suite without claiming earlier results cover the latest source. Unity may normalize material/settings serialization during import/tests—review resulting diffs, do not blindly commit them.
2. Build a fresh native candidate with the wrapper. Re-run preview and the independent three-lap performance race, then the three-race lifecycle if needed. AI/recovery and camera clearance matter because vertical geometry changed.
3. Inspect a continuous descent → portal → gallery → exit, not only stills. Check whether the upper mass/terraces feel connected to the city rather than blank prototype walls. Compare to the included approved reference; don't promise a pixel-identical photoreal render.
4. Fix concrete issues within this section and show the owner a real comparison before full-circuit rollout. Existing banking/horizontal layout, recognizable ship and completed HUD are to be preserved unless explicitly changed by the owner.
5. Later environment stages: strengthen the weak left-city/central-landmark composition, connect sparse districts and remove artificial blackout transitions, vary billboard shape/content/height, then broader material/performance/size cleanup. Do not claim these are already done.

Native verification commands (use the app built above and fresh output folders):

```sh
"/absolute/fresh/VectorRush.app/Contents/MacOS/Vector Rush" \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -productionValidation /absolute/fresh/preview preview -logFile /absolute/fresh/preview.log
python3 tools/production-preview.py /absolute/fresh/preview

"/absolute/fresh/VectorRush.app/Contents/MacOS/Vector Rush" \
  -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 \
  -productionValidation /absolute/fresh/performance performance -logFile /absolute/fresh/performance.log
```

`performance` runs one three-lap race with no screenshots/audio encoding; `races` runs three complete races and pause/restart checks (several minutes). Do not overlap performance measurement with Editor, rendering or encoding. `preview` captures PNG/audio and records camera poses; its timing is not performance evidence. `tools/production-preview.py` needs Python 3 + ffmpeg.

For actual captured camera checking, close the Editor and run its executable with `-batchmode -quit -projectPath <absolute UnityProject> -executeMethod VectorRush.Editor.UndergroundGallerySetup.ValidateCapture -productionEvidence <the preview folder> -logFile <absolute log>`. This only checks the recorded automated camera path, not every possible manual path.

## Key code and assets

- `Assets/Scripts/World/TrackPath.cs`: opt-in gallery profile. Offset uses original arc-length position, quintic easing, recomputed cumulative length and analytic tangent adjustment. Baseline false preserves historical geometry. `SetUndergroundGallery` invalidates caches; `EvaluateParameter` maps old/new physical locations during authoring.
- `Assets/Editor/Production/UndergroundGallerySetup.cs`: reproducible candidate authoring. Copies the structural baseline, clones affected meshes into the candidate asset folder, moves road/collision together, trims old gallery geometry, builds architectural batches, publishes the new course identity, validates lane clearance. `Prepare` requires `-productionEvidence <absolute folder>`. It **overwrites this candidate and its own generated assets**; do not rerun after hand edits without preserving them.
- `Assets/Art/UndergroundGalleryStage2/`: serialized cloned route meshes, new geometry, materials and procedural cast-panel texture. No runtime mesh generation needed.
- `Assets/Tests/Editor/UndergroundGalleryTests.cs`: baseline hash, depth/XZ layout, grade/frame derivative, arc-length projection, candidate mesh ownership and lamp emission checks.
- `Assets/Scripts/ProductionEvidence.cs`: opt-in preview/race/performance capture.
- `Assets/Scripts/Presentation/`: finished HUD and other presentation. `docs/hud-a-review.md` records the completed HUD milestone.
- `Assets/Editor/Production/ProductionSceneSetup.cs`: existing exact-scene build entry point; wrapper passes the candidate explicitly.
- `SourceAssets/`, `tools/`, `references/`: editable originals, recipes and provenance. Some historical scripts/docs contain the previous author's local paths; these are not required for importing/building the serialized candidate. Prefer the portable wrapper for current work.
- `docs/plans/2026-09-16-whole-environment-polish.md`: wider plan. Its old fixed-elevation constraint is superseded by the owner's explicit genuinely-underground requirement and the current implementation plan.

## Publication boundary

This is one current game source snapshot, not the entire multi-workspace parent folder. Historical scene/assets inside the project are retained because tests, authoring tools and serialized references can depend on them; do not delete them as assumed duplicates. Other checkouts, native builds, Unity Library/Temp/Logs/UserSettings, raw captures, credentials and private licensing logs are excluded. A fresh import/build still needs verification on the next machine; file/hash completeness is not a claim of cross-machine execution success.

`current-game-manifest.json` lists every published payload file and SHA-256. `tools/verify-current-game.py` verifies download integrity without Unity. Existing public GitHub branches/history are left intact; this branch starts a clean current-game snapshot so old build/capture history is not part of its download.
