## 2026-09-17 — Game P5 art-first kit resumed and packaged

Recovered the later Game P5 instruction that prioritizes polished art assets before racing presentation. Continued its existing isolated worktree and source assets. Delivered 19 prefabs / 38 LOD meshes, 33 Unity materials, 69 maps, six drawn sign designs and three Scenario ads. Fixed volume persistence, material/source retention, rail supports, review sign placement and texture directionality. All 282 existing tests pass; the final review-lighting/layout adjustment compiled and rebuilt. Native review build `b154fc6beded40999e21c9d816c53aba` completed both warmed captures. Package checks verify source/engine parity and unchanged racing scenes. [Asset gallery and rebuild guide](night-city-art-2026-09-17/README.md). Owner artistic review and racing integration remain open; no external publication.

# Development history

## 2026-09-17 — Rain presentation on the corrected road

Added the owner-approved rain scene, sheltered streaks/audio, wet road materials and low ship spray. Preserved handling, prior scenes, all road/collision meshes, ship and HUD. Final candidate: 279 passing tests, a 43.029-second native lap with 1,009 frames, and a separate three-lap race with all six finishers and zero recoveries. Frame times: 8.335 ms mean, 9.324 ms P99; no frames above 33.3 ms. [Rain evidence and limitations](current-game/rain-2026-09-17/README.md). Embedded video playback crashed; a verified native-frame replay provides browser review without video decoding. Owner visual acceptance remains open.

## 2026-09-17 — Owner-selected Apex application icon

Packaged approved option 2, Apex, generated with GPT Image 2 through Scenario. Includes the exact master, PNG sizes 16–1024, Windows ICO, macOS ICNS and prompt/provenance under `SourceAssets/Branding/Apex`. Added the identical image at `UnityProject/Assets/Branding/ApexIcon.png` with a stable texture GUID and configured the default Player Settings icon. Scene and gameplay files are unchanged.

Validation: exported images decode, master and Unity image hashes match, serialized icon GUID resolves to the included texture, and the portable manifest passes. An isolated Unity validation was attempted but its IL post-processing service failed during compiler startup; no Unity import/build success is claimed. Existing native apps were not rebuilt. Verify the icon in the next native package build.

## 2026-09-17 — Road curvature and banking correction

Completed the approved road correction in SmoothRoadStage3 with periodic filtered curve geometry, gradual banking and matching denser surfaces. Previous underground and HUD baseline scenes remain byte-identical. 276 tests, 14,400 lane probes, 1,544 camera samples and three six-finisher races passed with zero recoveries. Uncaptured performance: 8.337 ms mean, 9.303 ms P99, no frames above 33.3 ms. Native before/after captures average about 36 fps with occasional recording gaps. [Source milestone, evidence and limitations](current-game/smooth-road-2026-09-17/README.md). Owner manual/controller feel and visual acceptance remain open.

## 2026-09-17 — Underground candidate paused; portable public checkpoint

Implemented opt-in 22 m additional descent in `TrackPath` and isolated `UndergroundGalleryStage2` scene/assets. Cloned affected road/shoulder/barrier/rail meshes, replaced the second gallery canopy with solid foundations/retaining walls/roof, added connected approach/exit and a second lighting/material/terrace iteration after inspecting native captures. Baseline scene remains unchanged. First iteration: 271 passing tests. Second native build `2ef7de7505b34a03804ebf27bc4df955`: complete 43.136 s preview lap, 377 timestamped frames; 7,200 driving-clearance samples, 0.1188 m max road alignment error, 33.16% maximum grade. No owner artistic approval.

Owner requested pause and upload of the current game only to the explicitly confirmed public `emrickk/vector-rush`. Stopped the in-progress performance race before completion. Final test rerun and native camera-path validator were not completed; no multi-lap/performance pass is claimed for this candidate. Added authoritative portable README/agent handoff, current-scene build/test wrapper, selected native/reference evidence and a verifiable source manifest. Publishing one clean `game/current` snapshot branch preserves the older public branches while avoiding old build/cache history. Current source originals and Unity metadata remain included; caches, native apps, other workspaces, raw captures and local/private logs do not.

## Production PR transfer — 2026-09-09

Owner requested all current work and plans in a PR for another agent. This checkpoint preserves pending opening04 source/evidence and partial Sol production code, tests, Unity metadata, route context, and a [portable handoff](production-handoffs/START_HERE.md). It is a work-in-progress transfer, not production completion or quality acceptance. Intermediate red/green test XML is preserved; see the Sol checkpoint for validation limits. Native apps, caches and local editor/licensing logs remain excluded. No independent review was performed during packaging.

The first design commit was recorded before implementation. GitHub was requested after the first native playtest and during the second visual iteration. Earlier uncommitted intermediate edits cannot be recovered as historical commits; the implementation checkpoint preserves their current combined result.

## Recorded milestones

1. **Design and ownership** — original anti-gravity racer, bounded playable scope, interface contracts and parallel plan. Commit `ca58eec`.
2. **Integrated implementation checkpoint** (`acc8252`) — Unity project, authored Blender craft, course, hover physics, five rivals, three-lap race, HUD/audio, reusable GPT prompt and tool wrappers. Includes first runtime evidence and independent critiques. First rule-test run: 17 passed. First native automated race: 3 laps in 155.43 seconds with 7 player recoveries. First visual review failed. Current checkpoint includes the next font, lighting, ocean, coastline and AI corrections; those corrections compile/build successfully but still await their own complete native playtest.

## Next steps, each with its own commit

- Validate the second native build, capture current evidence, and obtain an independent visual review.
- Address remaining blocking findings with separately recorded corrective commits and relevant rechecks.
- Record a short gameplay video and verify menu/keyboard interaction.
- Finalize delivery documentation and known limitations.

Every subsequent completed plan step should update this file and be committed and pushed to the authorized private repository. Build products, caches and temporary frames remain local; source, authoring assets, selected evidence and critiques belong in history.

### Second native validation

The private repository `https://github.com/emrickk/vector-rush` was created with explicit user approval and verified private. Both initial commits were pushed. `evidence/run-02` records a full automated race through ordinary player physics: 3 laps in 112.58 seconds, zero player recoveries, and three restart/countdown-pause checks passed. NOVA recovered once. At 1920×1080 on Apple M2 Max with VSync, 3,758 frame samples had P95 16.82 ms and P99 16.89 ms. These are frame intervals, not isolated GPU measurements. HUD fonts are visibly repaired; independent review still rejects AAA visual quality.

### Third review and final visual pass

Run 03 again completed the automated race in 112.58 seconds with zero player recoveries; restart checks passed. Rival hulls now carry team color, and the grandstand has separated roof bays, beams and supports. P95 frame interval was 16.62 ms at 1920×1080 (VSync enabled; observed refresh varied between approximately 60 and 120 Hz). The road still exhibits dark bands and distant zigzags. The critic found that per-light bias was ignored by URP, and a separate native caster-only experiment did not eliminate the artifact. The ineffective casting workaround was reverted. `docs/critique-runtime-03.md` preserves both negative findings. The third visual review remains an AAA failure; this is a playable prototype.

### Visual-first correction in progress

The companion visual task requested stronger modeling and visual focus. New isolated hero/environment packages are being authored there while this task retains Unity integration. Numerical analysis found bank discontinuities and wide twisting quads; continuous horizontal-curvature banking and lateral tessellation were implemented. Run04 completed3laps in 112.64 seconds with zero player recoveries, both results/paused restart launches entered Racing with movement and reset state, and countdown-pause checks passed. Road shading still failed visually, so matched material/shadow/normal diagnostics follow.

Two additional source-backed rendering defects were identified: the URP renderer lacked PostProcessData, disabling configured postprocessing; the saved Lit material lacked an emissive keyword/color, risking emissive variant stripping. Both are being corrected. Dedicated additive vertex-alpha propulsion replaces opaque Lit trails. Native pointer diagnostics show InputSystem window coordinates are valid while IMGUI carries a large desktop-origin offset; HUD normalization now uses InputSystem coordinates, pending runtime activation check.

### Renderer resources and propulsion correction

The native renderer now has its missing URP postprocess resource and the Lit template retains the emissive variant. ACES, bloom and grading are active, and additive vertex-alpha trails replace opaque ribbons. The controlled native baseline in `evidence/road-diagnostic-05/diag-01-baseline.png` shows a continuous road with craft shadows before any diagnostic toggles. The companion visual reviewer confirmed the improvement in this area; this does not establish full-course temporal quality or an AAA pass. Recessed engine cores preserve the revised ship's chamber depth. Native build succeeded. The next milestone separately records imported V2 models and their full race evidence.

### Reviewed ship and city models

Imported the companion task's refined Kestrel V2, preserving its existing Unity asset identity, and replaced blank city cubes with eight clusters of authored terrace/split towers. Source Blender files, export scripts, rejected first-pass images and revised inspection renders are retained. Run06 captures title/start, crest and city descent with the new assets. Independent visual feedback confirms that cockpit channels, layered engine chambers and tower terraces survive the runtime import; dark glazing and weak contact depth still need work.

Run06 completed 3 laps in 112.64 seconds with zero player recoveries. Both full restart launches and three countdown-pause checks passed. The 1080p sample reported P95 18.86 ms/P99 25.07 ms; authoring work ran concurrently, so this is not an isolated final performance benchmark. The next rendering pass addresses contact depth and glazing, followed by the reviewed cliff replacement.

### Paused recovery input correction

Vehicle controls now clear outside Racing, so a recovery button pressed during pause cannot remain queued and spend energy on resume. A virtual InputSystem gamepad verifies both paused rejection and ordinary racing recovery. All 19 EditMode tests passed in the pinned editor. The earlier 17-test report and failed test-fixture report remain separately labeled; the corrected fixture explicitly runs player input updates in EditMode and restores the package settings afterward.

### Native pointer activation correction

On this macOS setup the native IMGUI event carried desktop-offset coordinates. InputSystem supplied correct window pixels, but changing GUI.matrix reloaded the raw native event position. Applying normalized virtual-canvas coordinates after the matrix change repairs hit testing. Native pointer Start, camera-shake toggle, Resume and Restart all activated at 1280×800; an outside click did not activate. Evidence includes actual button activation logs. Keyboard checks from the earlier run are retained. A second resolution remains for the final build check.

### Safe authoring regeneration

The default Blender wrapper now shows versioned options instead of overwriting the live hero with the legacy generator. Explicit hero V2, environment V2, frozen inspection and legacy V1 operations write to fresh staging directories; publishing into Unity is a separate reviewed payload copy that preserves metadata. Current sources and the environment material helper are packaged with the selected CC0 texture manifests/provenance. Shell syntax and help were checked; these documentation changes did not trigger another geometry generation or studio render.

### Connected coast and stable deck shading

Replaced the old radial geology with the reviewed textured cliff, retained the original scanned Rock 3 maps and a dedicated URP normal/metallic-mask material, and grouped eight authored towers onto three connected waterfront districts. The quays include deep caissons, seawalls, common promenades, service buildings and piled jetties. Independent source review caught near-coincident cliff placement; a 40-metre center-separation check removes the duplicate.

Matched starting-grid diagnostics found that disabling SSAO did not remove the road artifact; reflection suppression removed much of it, and disabling sun shadows did not. A dedicated matte Lit deck now excludes environment reflections and specular highlights while retaining diffuse lighting, shadows and contact occlusion. Other materials retain reflections. This is a deliberate presentation/material correction, not a claim to have proven every renderer-level cause.

Run09 native title/start/crest/descent shows a clean road and integrated coast. Full automated race:3 laps, 112.64 seconds, zero player recoveries, both full restart launches and all 3 countdown-pause checks passed. A 60-second 1920×1080 standalone sample on Apple M2 Max with VSync recorded mean 16.69 ms, P95 16.76 ms, P99 16.96 ms, 203.6 MiB Unity allocated memory. No concurrent asset generation or Editor ran during profiling. Independent critic accepts prototype visual integration and explicitly rejects AAA quality. Empty plazas, cliff macro-shape/tiling, dark glazing and repetitive water remain documented. The dedicated close inspection camera was obstructed; correcting that separate evidence view does not alter race cameras or gameplay.

### Delivery evidence and package

The current native preview contains 360 frames at 24 fps, 1280×720, 15 seconds, silent H.264. It uses automated steering and fixed simulation-time capture; frame timing evidence comes from the separate run09 standalone profile. First, middle and final recording frames were visually inspected. Pointer activation also passed at 1920×1080, including outside-click rejection, Start, Resume and Quit.

The current macOS app is packaged in `Builds/VectorRush-macOS-2026-09-07.zip`; archive integrity passed and hashes/source milestone are recorded in `evidence/build-manifest.json`. Native build products remain local. README, revised experiment prompt, editable asset guidance, final independent critique and limitations now describe the current build. Earlier failed captures, rejected model images and prior video are retained with historical labels.

### Night urban pass 01 — native review rejects visual target

Replaced active coastline with midnight architecture, procedural occupied windows, a dark sky, 58 overhead lamps and two open light galleries. Added local-light road response, layered propulsion and small suspension lights. Unity 6000.6.0f1 standalone build succeeded after using its serialized additional-light setting. Captures in `evidence/night-01` show actual title/start/crest/descent. A separate real-time 60-second sample recorded mean 16.67 ms, P95 16.79 ms and P99 16.85 ms at 1920×1080 on Apple M2 Max.

Visual target fails: broad road areas remain underlit, isolated towers and black ground read as an empty test course, and exhaust has hard crystal-cone edges. Corrective pass is explicitly required: composed skyline and connected service corridors, broader neutral light pools/material response, softer compact propulsion. No AA/AAA acceptance is claimed.

### Night corrective pass 02 — stronger city, lighting and soft propulsion

Added three depth rings of stepped skyline buildings, mid-rise blocks and low urban fabric, service avenues and coherent occupied-window suites. Increased neutral road illumination, made road seams finer, reduced harsh hull/ring highlights, and replaced closed cone exhaust with soft fading plasma sheets. HUD panels are darker and less visually dominant. Build and native captures in `evidence/night-02` confirm substantial improvement in road/hull readability, city occupancy and propulsion softness.

The 60-second 1920×1080 standalone sample recorded mean16.67ms, P95 16.82ms, P99 17.31ms and241MiB Unity allocated on Apple M2 Max. Independent static review still fails the visual target: distant windows remain too sharp/bright, procedural facade families repeat, and road light is broad/even rather than composed pools. Next bounded correction: verify native fog retention, distinguish facade families and tighten road lighting/roughness.

### User-requested visual targets before proceeding

Paused further implementation/native review to generate three coordinated original night-racing references with the built-in image generator: open city straight, amber enclosed light corridor, and close craft/material view. Saved all three images plus exact prompts and usage notes in `references/nocturne`. The amber corridor is the recommended primary target because the scene can be composed from a limited modular kit with controlled lighting. These are concept targets, explicitly not game screenshots or implementation evidence. Third native build completed while references generated; its atmosphere and corridor inspection remain pending.

### Night candidate checkpoint before ship-first focus

Built the atmosphere/facade/corridor candidate with explicit Exp2 fog retention, more varied facade families, finer road relief and enclosed overhead light cassettes. A native starting-grid capture is preserved in `evidence/ship-baseline-03`. This is a checkpoint, not scene acceptance: the full night03 fog-pair, race, corridor and motion reviews remain pending while the user prioritizes the ship.

### Astra ship baseline and production gates

The user explicitly requested Astra planning and a separate harsh visual reviewer for repeated asset correction. Astra inspected reference C/B, the neutral V2 studio set, night02 gameplay and a fresh night03 native baseline. It rejected the ship's padded large forms, bubble canopy, slab fins, unresolved connections, clipped graphics and dominant glowing nozzle rings. `docs/ship-art-plan.md` defines separate form, construction, materials, propulsion and native/motion gates; `docs/ship-reviews/001-baseline-rejected.md` preserves the negative verdict. A fresh V3 form candidate is being authored separately; no hero-quality pass is claimed.

### V3 form pass 01 — independent rejection preserved

Rebuilt the craft in staged Blender source with a lower twin-nacelle layout, narrower canopy and integrated dark engine chambers. Preserved the editable blend, generation recipe snapshot, FBX, engine anchors, source hashes and nine rendered inspection views. Astra inspected the clay and simple neutral views and rejected Gate 1: the nacelles resemble chamfered beams, the canopy is a coarse wedge and the intakes are rectangular wells. Gate 2 also remains held for unsupported major joints and eight nonmanifold boundary edges per nacelle core recorded by the mesh audit. This candidate was not imported into the live game.

The next bounded pass changes the compound nacelle silhouette and section, swept recessed intakes, canopy curvature, and supported seams/collar returns. The same cameras plus a recorded 16:9 reference comparison will be reviewed again. Surface texture work remains gated by form/construction acceptance; source geometry and screenshot counts alone do not establish quality.

### Corrected native inspection calibration

The first neutral inspection attempt inherited an interpolated Rigidbody pose, leaving the ship away from its studio floor and lights. Its images and metadata remain explicitly rejected. The corrected opt-in rig detaches the actual imported visual hierarchy, disables physics interpolation during inspection and asserts its canonical pose and bounds before capturing. The current V2 asset and its runtime materials remain unchanged.

The standalone rebuilt successfully and exited normally after eight native 1920×1080 images in `evidence/ship-native-v2-neutral-02`. All eight calibration assertions passed; the reflection probe completed; rendered and canonical bounds agree at 5.2651 × 1.9973 × 7.2050 m. Parent viewed rear/front/side/top/engine/canopy images; Astra independently inspected all eight and found the setup suitable for native import/model/material diagnosis. V2 remains visually rejected. This validates the evidence setup, not the ship.

### User clarification: close shape, then whole-game quality

The user clarified that matching the reference completely is unrealistic and that once the shape is almost there, effort should move to other elements that make the whole game feel AAA. Astra and the asset worker were instructed to treat the reference as direction, advance when the primary shape is coherent and close, and prioritize material response, propulsion, lighting, road and motion over minor contour matching. Current pass 02 remains the bounded shape correction under review.

### User-requested wrap-up — V3 pass 02 staged checkpoint

Stopped further iteration and rendering at the user's request. Pass 02 retains editable source, the exact runtime UV meshes, FBX, anchor JSON, generation recipe and a completed rear clay image. Its audit reports all source meshes manifold, runtime meshes manifold and triangulated, and the reimported FBX manifold with valid tangents; runtime/export totals agree at 38,036 triangles. Astra found the visible hull changes coherent enough to support advancing under the revised practical target, conditional on the still-pending side/top/chase views. Its provisional review is preserved separately; no final shape, material, native or AAA acceptance was awarded.

Saved optional authored-engine placement and validation with the original V2 fallback, plus an opt-in ship texture helper, editor import preparation and a staged Blender coating-bake script. No V3 model, texture or anchor payload was copied into Unity. These latest preparatory changes have not been compiled, baked or checked in a new native run. The last built app remains the night candidate with V2 and the independently validated inspection rig. No render or audit job remains active. README distinguishes that app, the staged V3 work and the historical coastal archive/video.

### Resumed pass 02 — primary form ready for finish work

At the user's request, resumed the saved geometry with side, top and chase renders, using 32 CPU samples without the stalled denoiser. Astra directly inspected these angles and the preserved rear clay view and cleared the coherent primary form for advancement. Major contours are now frozen; remaining broad panel and mount simplifications are secondary finish items. Review 005 preserves the bounded verdict and missing front/neutral/native evidence. This is readiness to proceed, not AAA acceptance.

The previously saved engine-anchor and texture-import preparation compiled successfully in Unity's project preparation. New material mappings and replay telemetry are being prepared for the selected finish asset; native verification will follow payload selection. The finish revision will retain flush citron/07 markings in the baked color atlas and separate compact white static engine cores from dim cyan annuli.


### V3 pass 03 — integrated finish and native review

Baked flush livery and four coated material sets at 1024 pixels, preserving the exact selected source/export identity and metallic/smoothness packing. Integrated the FBX, authored engine anchors and calibrated material mapping. Native build succeeded; all eight inspection calibration assertions and the reflection probe passed. Source, baked preview and native images were independently reviewed in report 006. Material hierarchy and import are ready to advance, with no AAA or final visual acceptance.

Two 360-frame native gameplay recordings preserve per-frame speed, progress and boost metadata. The second includes amber gallery entry, interior, exit and 32 boost frames. These use automated steering at 24 simulation frames per second and do not measure real-time performance. Gallery images show useful warm ship lighting but overly black structural panels; propulsion remains too faint beyond the nozzle. Canopy and intake artifacts also appear in Blender and are undergoing a bounded surface-contact correction. These limitations remain open.


### Native gallery and propulsion correction

Added recessed charcoal faces, folded borders, vents and local warm/cool wall wash to the existing galleries. Increased the short attached plasma envelope and reduced nozzle point-light spill. Native build succeeded and motion03 completed all 360 1080p frames. Its selected gallery/boost frames exactly match motion02 race times and progress; the32 observed boost frames are recorded in metadata. Parent and Astra directly compared the interior and boost stills: panel depth and attached exhaust improve, while black ceiling masses, repeated bays/windows and road sheen remain follow-ups. The encoded 15-second video uses automated steering at 24 simulation fps; no real-time performance claim is attached.


### Pass04 — repair measured ship surface overlaps

Recessed only three Graphite parts: canopy sill, buried intake openings and rear returns. All seven runtime UV arrays remain byte-equivalent; armor, glazing and engine geometry stay fixed. Source audits reduce targeted positive-area coplanar overlaps to zero and canopy near-parallel contacts within 2 mm from 145 to zero. FBX reimport remains manifold with valid tangents.

The native control build kept exact pass03 maps to isolate geometry changes. All eight calibration checks and the reflection probe passed; parent and review 009 confirm removal of the jagged canopy patch, intake-mouth serration and white/Graphite nozzle-border conflict. This is a targeted repair verdict; fresh maps and final race remain separate checks. The 350-triangle native/export discrepancy is accounted for by 332 duplicate-position triangles plus 18 more collapsed at float32 centimeter precision, matching Ceramic 330/Engine 20 exactly. The precise importer operation is not instrumented.


### Combined night candidate — maps, road dampness and rival guard

Rebaked all four material sets against corrected pass04 geometry, validated 20 PNG hashes/dimensions and integrated 16 runtime maps. Only the expected Graphite normal/roughness and contact-AO payloads changed; the flush livery remains unchanged. Made the dampness field periodic while preserving its range and the original albedo/normal sampling. Its matched native visual comparison is recorded separately.

A rival-only outer-corridor guard anticipates lateral drift, temporarily reduces speed and steers inward through existing controls. It checks neighboring collision clearance and traffic on the corrected path, preserving player/testing-autopilot behavior, lane reservations and collider size. All 29 Unity tests pass. Native race01 completed three player laps in 112.60 seconds with zero player or rival recoveries. The old three deterministic early stalls disappeared. Both restart launches and all three countdown-pause checks passed. Rivals had not all completed three laps when the player finished; pacing remains provisional.

At 1920×1080 on Apple M2 Max, the separate real-time sample records 3529 frame intervals: mean 17.01 ms, P95 20.60 ms, P99 25.34 ms, with VSync enabled and 250.3 MiB Unity allocation. No build or bake ran during this sample; idle desktop applications remained. These are observed frame intervals, not isolated GPU timings or a locked 60fps claim.


### Final road finish and night package

Periodic dampness alone did not remove the diagonal road bands. A single final range adjustment from 0.48–0.93 to 0.42–0.78 makes the highlight modestly quieter while retaining warm/cool pools and the damp response. Parent and Astra compared the same 35.380-second gallery view and kept the change as a contrast improvement, not a root-cause fix. Gameplay source is byte-identical to the successful race build.

The final native run exited normally after 360 frames and 31 observed boost frames. Its H264 preview verifies at 1920×1080, 24 fps, 15 seconds, silent. The versioned macOS archive passed compressed-data integrity checking; artifact and build identities are preserved in `evidence/night-v4-build-manifest.json`. Current README links the night app/archive and final preview, distinguishing earlier coastal evidence. Remaining limits include road patterning, repetitive city/gallery finish, dark ceilings, manual handling, independent continuous-motion review and competitive rival pacing. This is an improved playable prototype milestone, not AAA delivery.


### Throttle-driven exhaust correction

User identified the flame as fake and requested throttle response. The previous effect used speed plus an always-on minimum, so it persisted after lift-off. Exposed the existing clamped throttle input and drive plume length, width, intensity and nozzle spill from that value; active boost multiplies demand by 1.35. Fast smoothing removes abrupt switching, with no idle plume floor. Zero response disables both plume renderers; static powered-nozzle appearance remains separate. Physics and driving controls are unchanged.

Added three actual input-path regressions and an opt-in native virtual-gamepad sequence. All 32 tests passed. Native six-stage verification exited 0 and confirmed analog demand/response of 0.25, 1.0 and 1.35 for quarter/full/boost, and zero on release. Response fell below 0.02 in 0.150 seconds while still at 242.0 km/h, then reached zero. Parent directly inspected quarter/full/coast/boost screenshots. The scripted steering encountered nearby traffic later in the sequence; this is an input/rendering test, not a full-race or handling verdict. The first compile caught a malformed new metadata GUID; it was corrected before the passing tests/build.

Source and native identity, per-frame samples and six screenshots are preserved under `evidence/throttle-native-01`. The previous night clip is explicitly labeled as predating this correction. The updated macOS archive is `Builds/VectorRush-macOS-throttle-fix-2026-09-07.zip`.


## 2026-09-07 — Racing HUD and native responsive correction

User requested a AAA-style HUD after the throttle correction. Replaced the distributed telemetry cards with four compact corner groups: position/lap, race timing with subordinate milliseconds, an actual course map with live racer markers, and unified speed/boost. Added real lap-change feedback, a short GO cue and bundled OFL Rajdhani typography. Pause/results remove stale underlying telemetry; menu/control labels use the same face. Gameplay, propulsion and asset geometry are unchanged.

The first native run completed eighteen phase/aspect views and three laps, but independent review found that RotateAroundPivot broke the map and speed arc at non-unit UI scales. Preserved that failure and changed line drawing to compose local transforms after the root scale. Added antialiased strokes, clear zero/unavailable timing, larger controls and final-lap capture coverage. Native02 completed twenty-one views across seven actual states and 1920×1080, 1280×800, 1920×810, finishing three laps in 112.603 seconds with zero player recoveries. The final-lap notice was triggered by actual progress and captured after 0.30 race-time seconds. Independent responsive-layout acceptance is in review 003, with the earlier failure in 002.

Existing 32 Unity tests passed and final native build exited successfully. A separate native pointer run at 1280×800 confirmed outside-Start rejection, Start, Escape pause, pointer Resume, Restart and Quit through visible controls and normal input. The last adjustment tightens only the seconds/fraction spacing; its current native evidence is the separate 15-second, 360-frame 1080p HUD preview with 32 boost frames. This is silent automated steering at 24 simulation frames per second, not real-time performance or human driving evidence. Current archive integrity verified. No new performance, low-energy or physical-gamepad acceptance is asserted.


## 2026-09-08 — Whole-game critique and next-stage path

Reviewed the current HUD milestone against the original polished-racing-slice target. Parent inspected native exterior, boost, gallery and menu evidence plus control/audio/world source; independent agents reviewed whole-game presentation and gameplay evidence. Primary gaps are a visibly procedural city, repetitive/unfinished track surfaces, unproven competitive racing and incomplete audiovisual feedback. The previous race finished with opponents only 2.27–2.74 laps through their three-lap distance; the rival-only safety guard has an unmeasured pace cost. Source also confirms speed-driven motor audio and decorative boost-like chevrons without a corresponding track mechanic. Human handling and sound quality were not newly tested or judged as if heard.

Saved the independent whole-game critique and a bounded roadmap. Immediate next work is opponent pace diagnosis and truthful race feedback, followed by an authored 10–15-second visual benchmark, then complete-circuit integration. Proposed numerical gates are tuning targets, not measured successes. No gameplay, art, settings or build changes in this planning checkpoint; preserve the current playable baseline.


## 2026-09-08 — User priority correction: visual environment first

User selected the city and track criticisms for action and requested a plan, explicitly prioritizing visuals and feeling. Replaced the active AI-first order with a visual environment plan: fixed race-camera benchmark, authored architectural depth, supported track/gallery construction, isolated road-sheen diagnosis, then integrated lighting/speed perception and full-circuit expansion after independent acceptance. The plan distinguishes existing source-level skyline layers from their insufficient native visual separation and retains the falsified periodic-noise explanation for the road artifact. Gameplay, craft and HUD remain stable references; performance and continuous-motion readability are included as visual acceptance checks.

This is a documentation-only planning checkpoint. No new art, runtime settings, code or build has been produced. Prior critique and published history remain intact.
# 2026-09-08 — Visual benchmark and isolated diagnostic

Built unchanged environment art from 0320b3c with opt-in EnvironmentEvidence and RoadSurfaceEvidence capture tools. The baseline reached five natural progress anchors across 264 native 1080p images and saved actual camera/racer poses and FOV. Encoded an 11-second silent, automated-steering, fixed-simulation preview. Saved original-image hashes, concept prompt/provenance, passage palette/asset contract and independent direction review. Twelve matched road variants completed and restored their original state; diagnosis/acceptance remain separate from successful capture. No runtime visual changes are part of this benchmark commit. City/gallery construction is the next bounded step.
# 2026-09-08 — First constructed environment candidate, review failed

Integrated three authored transit/service exports with editable Blender sources and placement audits, composed middle/near city assets, built chamfered gallery portals and continuous shells, and added continuous deck fascias/maintenance ledges. Replaced decorative luminous boost chevrons with shoulder direction arrows. Gallery geometry is combined into fourteen batches; its 36 lights are reallocated to reveal upper surfaces, and six conflicting exterior fixtures are suppressed. Native build and all 32 existing EditMode tests pass.

Captured candidate01's five views and 264 actual racing frames. The camera comparison correctly reports **not matched**: camera distances differ by roughly 0.22–0.60 m from the unchanged reference, so these are nearby actual-physics comparisons, not pixel-exact A/B. No poses were injected. The ceiling is now visible, but the station is behind the camera at the skyline reveal, the workshop shows a blank wall and nearby masses remain too dark. This candidate fails city composition and is preserved before correction; it is not benchmark acceptance or expansion approval.

Road02 contains 17 matched within-run material/light/normal/shadow conditions. It does not establish a safe causal normal/shadow repair, especially because the old strong facet is weaker in this changed scene. Retain original geometry attributes and shadow settings. Next bounded correction: move/orient/light the station and service frontage, reduce obvious gallery hotspots if supported by review, and deliberately change damp-road finish to satin rather than claim a root-cause fix. Fresh baseline real-time sample: 1080p M2 Max, 5,149 frames, mean 11.65 ms, P95 16.67 ms, P99 17.01 ms; normal-physics autopilot completes three laps with zero player recoveries. These are separate from fixed-simulation picture captures.

## 2026-09-08 — Corrected environment composition, finish verification pending

Candidate02 corrects station placement/facing, workshop frontage and near/middle architectural visibility with audited exports and four architectural fills. Softer gallery washes preserve ceiling construction. The road is deliberately satin; no root-cause repair is claimed. Five 1080p anchors and a 264-frame native sequence completed, together with five 16:10 views. Independent review 005 accepts composition for further verification, not finished benchmark acceptance or full-circuit expansion. Native camera comparison remains outside exact-match tolerance.

Sampled sequence review 006 exposed a banked-road bearing protrusion and a service hatch intersecting a wall joint between nominated views. This snapshot preserves those defects. A final bounded correction will bank-align the bearing, move the hatch onto a single wall panel and refine the closest flat amber glazing. Continuous playback review, final-source aspects and real-time performance remain pending.

## 2026-09-08 — Final bounded environment correction and native review build

Candidate03 replaces nearby flat amber fills with restrained glazing artwork, including soft room illumination and shaded glass. Existing window groups, architecture placement and lighting counts are unchanged. Bank-align the deck bearing so it stays beneath the road, and attach gallery service hatches to individual half-panels. Native candidate03 captures confirm both defects removed in their prior intermediate views; independent review 008 accepts the inspected still-art correction for delivery/review. Broad road bands, modest station identity and the brief BEST LAP/lamp crossing remain documented.

The first full-lap instrumentation run recorded a complete circuit but incorrectly selected all five anchor labels near the pre-start wrapped progress. Preserve that failure. Correct the capture tool to arm anchors only after entering the early circuit and require actual lap advancement. The final build (GUID 153b77ea7d53463b8fc11a0ddae232be) then records 1008 native 1080p frames over 42 simulation seconds, two start-line wraps, lap 1→2 and five valid anchors at frames 646/816/836/856/886. Final 16:10 and ultrawide still captures also complete. Both previews are silent automated-steering evidence, not real-time performance or continuous subjective acceptance. Editable assets, full source hashes and archive integrity are preserved.

The same gallery kit is installed in both enclosures and deck/material improvements apply around the circuit. Further city-family expansion is withheld: the plan requires actual normal-speed viewing, which the available frame inspection does not establish. A concrete 11-second passage and 42-second full-lap preview are ready for user playback, together with the native macOS review archive. This delivery is the benchmark milestone, not a claim that the full visual plan or AAA target is complete.

Final real-time verification completes three player laps in 112.60 seconds with zero player recoveries; both restart launches and all three countdown-pause checks pass. At 1080p on M2 Max with VSync1, 6,804 observed frame intervals give mean 8.82 ms / P95 15.14 ms / P99 15.99 ms and 240.1 MiB Unity allocation, versus fresh baseline P95/P99 16.67/17.01 ms. No build/bake/encoding ran concurrently. This run shows no observed frame-time regression; it is not an isolated GPU or guaranteed-framerate measurement. Archive CRC validation passes.

Final-source regression suite rerun: all 32 tests pass. Source hashes remain identical to the final build manifest after testing.

## 2026-09-08 — Native pace baseline before final balance pass

User requested closer racing, followed by one visual critique/improvement round and final publication. Added opt-in PaceEvidence recording actual all-racer state, guard duty, input requests, validated distance gaps and vehicle settings through an ordinary three-lap race. Read-only driver diagnostics do not change behavior. Native baseline completes in 112.60 simulation seconds, 1,127 samples, six original screenshots, zero recoveries. Capture runs at 24 simulation frames per second and makes no performance claim. Prior pose-based pace diagnosis and exact native fields are preserved before applying the authorized actual-player speed reduction. Expanded optional environment capture durations to accommodate the slower race.

## 2026-09-08 — First player pace candidate, still too separated

Actual player cruise60/boost82 completes three laps in 120.912 seconds with zero recoveries across the field. Duration-weighted nearest Euclidean rival under80m improves from1.28% to53.48% after5s, but under40m reaches only4.37%; nearest validated finish gap remains177.1m. This is an improvement but not close-racing acceptance. Preserve candidate01 and its actual settings before the next bounded 55/75 candidate; acceleration and rival safety remain unchanged.

## 2026-09-08 — Second cap candidate exposes lateral-control weakness

Actual player55/75 completes125.969s without recoveries, but closeness collapses across the race: within60m77.2% on lap1,26.1% on lap2,0% on lap3. Rival edge-guard duties rise sharply and nearest finish gap worsens to494.1m. Preserve this failed candidate. Further cap-only tuning is rejected: the next experiment corrects the AI pursuit steering gain to request curvature-appropriate yaw while retaining guard, collision avoidance and steering limits. Compare against the better60/82 player candidate; this is a controller hypothesis, not a proven sole cause of every edge interaction.

## 2026-09-08 — Pursuit command corrected; native rail contact remains

The speed-aware pursuit command passes37 tests after four known-radius regressions demonstrably failed the original response. Candidate03 (player60/82) completes120.708s with0 recoveries but still lacks sustained close racing. Native target telemetry shows an inward goal roughly4m away while bodies remain against the barrier. Collider projection puts82–94% of these outer-edge samples within0.15m of the wall. Changing yaw gain alone cannot free a long hull whose stern must sweep into that wall. Preserve the candidate as partial controller correctness, not pace acceptance.

The next bounded physical correction is a rival-only lateral thruster during an active guard, toward the resolved collision-clear inward target, limited to6m/s². It does not assign position or velocity and is disabled for the actual player. Existing guard, clearance, turning limits and hover physics remain. Native proximity and recovery checks must demonstrate the outcome before visual review.

## 2026-09-08 — Rail-lock mitigation verified, final pace fit pending

Candidate04 adds bounded6m/s² lateral assistance only for rivals with active guard and a resolved inward target; all 42 tests pass after two mirrored contact tests fail the previous zero-force response. Native outer-edge exposure falls from roughly20–40% to3–5%, with longest outer episodes≤2s and zero recoveries. This supports the contact-recovery correction. Player60/82 still averages164.2km/h versus the best rival160.4, and within60m proximity reaches38.3%, so sustained-pack acceptance remains pending. Retain the proven wall correction and test55/75 once more under the corrected controller rather than changing lane topology or weakening edge safety.

## 2026-09-08 — Sustained proximity established; final finish-gap calibration

Candidate05 uses player55/75 with pursuit and rail-recovery assistance retained. It completes125.567s with zero recoveries; nearest Euclidean proximity after5s is54.25% under40m and80.84% under80m. The nearest two rivals finish160.4/190.5m behind, so the intended finish-gap target remains unmet. Preserve this substantial proximity improvement and make a final small actual-player53/72 calibration, without additional AI or physics changes, before visual critique.

## Final player pace calibration — candidate 06

Actual manual/test player preset53m/s cruise and72m/s boost, with controller fixes retained. Native race128.32s,1285 samples, six pack stills; zero recoveries for every racer. Nearest validated-progress rival within60m80.9% of post-start race, within30m56.0%; each lap remains close. Final nearest100.85m and second112.95m; no observed pass/rank change. Evidence and independent comparison preserve the narrow finish-target miss. Proceed to requested single visual pass.

## Single requested visual critique and correction

Independent review 009 inspected six pacing stills and eight actual full-lap frames. It found that rivals behind the camera remained visually imperceptible and cool-gallery upper pools dominated the fixtures. Added a restrained named-rival distance/relation cue under POS/LAP using validated race gaps plus physical/track proximity filters, excluding finished/distant racers. Reduced cool surface-wash peak78→52, moved it inward0.72m, softened tint and extended range20→22; warm gallery unchanged. All42Unity regression tests passed on this final source. Final native visual/race evidence and release follow.

## Final close-race native verification and delivery

Build 7da2883ade4f4223b7a611db8cf60eb6: final pace samples exactly reproduce candidate 06 after visual changes. Five matched anchor poses; 1080p, 16:10 and ultrawide captures verified. Independent critique009 and focused parent verification 010 remain separate. 42/42 tests pass. Separate real-time race 128.32 s, 3 laps,all six racers zero recoveries; both restart launches and all three countdown-pause checks pass. Observed M2 Max 1080p frame intervals: 7,189 samples, mean 8.35 ms, P95 9.22 ms, P99 9.32 ms, 239.5 MiB allocation. No build/bake/encoding concurrent. Packaged archive 54,531,835 bytes, 441 entries, CRC pass; hashes recorded. Current 15-second preview and 42.75-second full lap are silent native automated captures at 24 simulation fps. Source/assets/evidence are committed; native app archive is ready locally. Automatic approval review rejected creating the proposed close-race-2026-09-08 GitHub release because release-asset publication requires separate authorization. No release was published. Larger environment and human-playback limitations remain explicit.

## Landmark composition baseline

User selected distinctive landmarks and stronger architecture beside the track, then road finish. Preserved twelve unmodified native views from final build 7da2883ade4f4223b7a611db8cf60eb6 across progress .10–.70, with original camera/racer metadata and source-capture identity. These are selections from the existing verified full lap, not a newly rendered baseline. The two proposed landmark sites are the opening bend's outer side and the middle sector's inner side; new geometry and placement remain under construction. Parent integration and the later road correction are separate steps.

## Landmark candidate 01 and independent native critique

Two original FBX landmarks and their editable Blender sources are integrated: the split signal mast on the opening right side and thermal exchange works on the middle left side. Reserved footprints, subdued skyline windows and lower background rhythm improve their separation. Native build 827452d9244a421b827bd5a4c90a85fa captures 1,440 frames / 60 simulation seconds and all five final-station anchors. Review 012 inspects fifteen candidate and fifteen baseline originals, accepts both landmarks and the retained station, and requires one correction: remove the transformed benchmark skyline slab crowding the mast. Camera differences are recorded; this is sampled composition review, not exact-pose or continuous-motion acceptance. Preserve candidate 01 before that correction. Opt-in road-fixture shadow diagnosis is compiled but has not yet established a road cause.

## Landmark composition correction

Removed the single benchmark skyline placement at local (−226,0,467), which transformed beside the opening mast. Native candidate02 build a1325c823e8b4541b20c143f42f12470 completes 1,440 frames / five anchors. The first-sector slab is absent, leaving sky around the mast; middle thermal works and final-station context remain. Parent inspected native frames175/606; independent correction review013 records its own inspected originals and limits. Selected originals and full pose metadata are retained. Proceed to the isolated fixture-shadow road experiment on this exact binary.

## Road fixture shadow cause and bounded finish correction

Corrected landmark binary a1325c823e8b4541b20c143f42f12470 reaches progress .9462143 through normal physics before freezing the actual rendered pose. Fixture-only A/B/A visibly removes the conspicuous pole-and-arm L-shaped road strip and restores it in A. All101 light states, camera, material and road mesh/collider identities remain fixed; three renderer modes read On/Off/On and restoration passes. The thin authored expansion seam remains. This establishes the current lamp-shadow contribution, not the cause of every earlier damp-road polygon. Final shipping adjustment disables casting only for Track lighting steelwork and the cool/amber linear diffuser mesh groups; gallery, landmark, vehicle and other architectural shadow casting remain. Native combined validation follows; no additional material or triangle change is made.

## Final landmark and road verification and local delivery

Build b709ef36660443b7bdec08170d00b8df completes1,440 native capture frames /60 simulation seconds. Every camera/racer/timestamp record matches the corrected-landmark candidate exactly after the shadow change; all five station anchors match. Preserved24-second continuous two-landmark preview and42.75-second full lap are1080p,24fps,silent automated captures. All1,450 PNGs across1080p/16:10/ultrawide passed chunk/stream integrity; sampled visual reviews retain their actual scope. All42 tests passed.

Two standalone real-time races each finish three laps in128.32s with zero recoveries for all six racers, both restart launches and all three countdown-pause checks passing. First performance sample:4,499 intervals,mean13.27ms,P95/P9916.66/188.14ms. Repeat:7,162 intervals,mean8.38ms,P95/P999.23/9.33ms,241.2MiB. Prior close-race P95/P99 was9.22/9.32ms. No Editor build,bake or encoding ran concurrently. Read-only sanitized process snapshots show sustained Chrome GPU-helper CPU activity during the good repeat too; initial hitch cause remains undetermined. Both runs are preserved rather than replacing the poor sample.

Local archive VectorRush-macOS-landmarks-2026-09-08.zip is55,318,267 bytes,441 entries,CRC pass. The172 runtime/resource/settings hashes and all app-file hashes remain unchanged after tests/captures; archive and video identity are recorded. Final source, editable assets, critiques and selected evidence are committed/pushed; no GitHub release is published.

## Lighting/material-depth baseline

User approved a focused presentation pass after the landmark/road milestone. Preserved seven landmark approach/pass originals and the final-station reveal from build b709ef36660443b7bdec08170d00b8df, with original camera/racer metadata and hashes. This reuses verified native evidence rather than generating a synthetic baseline. The next implementation targets material distinction and visible lower structural/service surfaces, followed by native critique.

## Lighting/material candidate01 implementation

After the owner explicitly overrode the brief account-switch pause, completed a four-family512-square albedo/metallic-smoothness kit and retained Lit shader template. Parent integrates it into five existing landmark material regions, with rougher concrete and warmer ceramic/insulation plus more legible smoked glass. One short mast bracing/soffit fill, one thermal end-portal wash and a retargeted thermal support/service wash address the surfaces identified in direction015. Original geometry, road shadows/materials, race physics and global moon/ambient/fog remain unchanged. Texture/metadata/channel validation passes; native candidate review is pending.

### Lighting/material native candidate accepted — 2026-09-08

Build5c49b92643bd4d41820ba81857fdf8c7 completed a1,440-frame native circuit. All PNG CRC/decompression checks passed;193 prebuild source/resource/settings hashes remain unchanged. Independent review016 directly compares eight original views against baseline and accepts the bounded pass with zero required corrections. Thermal service recesses and pipe/support connections improve; mast improvement is restrained. All five exact anchor comparisons fail small pose tolerances, preserved as a comparison limitation. Candidate evidence and critique are retained before final tests/performance/package; still review is not continuous subjective-motion acceptance.

### Lighting delivery artifacts verified; native checks blocked by display lock

All42 tests pass;193 source hashes and189 app hashes are unchanged. Both continuous simulation-time videos decode completely. The new macOS archive passes CRC with441 entries and56,664,092 bytes. Additional sampled circuit contexts remain readable. The Mac locked before the16:10 capture; the tool explicitly could not unlock it, and no frames were recorded. Only the stalled capture process was stopped. Preserve this incomplete attempt and mark alternate-aspect plus fresh real-time race/performance checks pending manual unlock. No completion or prior-performance substitution is claimed.

### Lighting/material delivery completed after unlock — 2026-09-08

Retained the second no-frame launch, then relaunched successfully after the display became available. Fresh16:10 and ultrawide station-context captures complete on GUID5c49b92643bd4d41820ba81857fdf8c7, with all10 PNGs validated and representative gallery/reveal views inspected. The separate real-time race finishes three laps in128.32s with all six racers at zero recoveries, both restart launches and all three countdown-pause checks passing. The7,184-frame sample yields mean8.35ms/P959.21/P999.33 and240.7MiB memory, consistent with the prior accepted repeat. No new correction or performance rerun is warranted. Final source/app/archive hashes remain unchanged; no runtime edits or rebuild followed acceptance. The user-requested lighting/material pass is complete with current native previews, local archive and published source/evidence.

### Coastal versus current visual review — user-requested comparison

The owner clarified that the earlier bright coastal version feels more AAA-like. Independent review017 inspects ten actual native images; parent018 compares representative crest/descent pairs, stronger current landmark/gallery views and source changes since coastal3351c38. Both identify loss of broad material/value separation, visible foundations and distinctive composition; the current craft also reads as broader/smoother under its lighting. Current HUD, gallery construction and thermal detailing are retained as gains. Camera code is unchanged, so camera-code regression is unsupported. The review exposes a process gap: prior bounded corrections did not establish whole-scene improvement over the coastal baseline. Saved an unaltered original-image comparison and a prioritized proposed experiment. No runtime, asset or native build was changed.

### Production night finish plan — 2026-09-08

The owner supplied a 10.112-second night-racing video, described it as the same concept with polished production-level execution, and requested a plan toward that finish. Retaining and improving the current night game is now the chosen direction; the coastal-restoration experiment is superseded. Preserved a reference identity/mechanism brief without publishing the private message-container path or video payload.

Created the production-finish design/spec and eight-task implementation plan: ordinary opening benchmark, broad light/road response, connected city, craft materials, visible racing/motion/audio, integrated acceptance, three-district rollout and verified delivery. Independent critique019 defines whole-image, grounded-world, material, genuine-pack, watched/manual and effects-off gates. The plan explicitly separates real-time performance from simulation-time imagery and requires actual watched motion plus manual play with audio before production acceptance. It retains the proven road-fixture shadow correction, original assets, six-racer race and published history. New whole-circuit work waits for the benchmark to satisfy visual, motion/play and technical gates.

Planning validation checks document links, existing component names/paths, native command switches, capture-schema crossings and source hash identity. This milestone changes documentation only; no Unity runtime, source asset, native build or gameplay result is changed, and no new native validation is claimed. Project entry points identify the new plan and preserve older scopes as historical records.

Final planning self-review and independent review of the written plan found no blocking scope, ordering or interface gap. The first-crossing recipe reproduces baseline frames 149/211/283/398/472; all referenced Markdown targets resolve, and all 193 existing source/resource/settings hashes remain unchanged. Acceptance is explicit: all six experience dimensions must meet the stated target; “approaching” and unobserved motion cannot close rollout. No runtime tests were rerun for this documentation-only change.

### Production lighting/road baseline completed

Preserved five natural opening crossings and three thermal/station controls from unchanged GUID `5c49b92643bd4d41820ba81857fdf8c7`; all 193 source and 189 app hashes match the accepted delivery. Review020 independently inspects all eight originals and identifies weak outdoor broad-surface/depth response, with motion explicitly pending. Saved the schematic light/depth layout and fully decoded 360-frame, 15.000-second 1080p/24fps silent native baseline excerpt.

The initial sandboxed native launch aborted before useful output. Automatic approval review rejected the first unrestricted performance attempt because existing Unity/Blender activity might overlap. Read-only investigation identified our stale licensing-failed batch job from the previous day, which was stopped; Blender's separate GUI was visually confirmed idle with no render/bake. A fresh check confirmed no Unity build or encoder, and the retry was approved. The separate native run completes three laps in128.32s with zero player recoveries, both restart launches and three countdown-pause checks passing. Its7,189 intervals yield mean8.35ms/P959.18/P999.31,241.9MiB allocation. Encoding began only after the native run exited. These are automated observations, not a human driving or continuous subjective-motion pass.

### Production lighting candidate A preserved

A neutral ambient/key lift, restrained teal fog/sky and modest cool-road-pool increase improve the candidate's broad light environment. Road geometry/maps/reflection state, race/AI, camera, craft payloads, postprocessing and the three fixture caster-off groups are unchanged. Technical review finds no blocker. Native build `9a03e9b1767f4b89abd9c3bf938f7898` completes1,440 frames with PNG CRC/decompression and195-source/189-app hash checks passing. Natural opening/control selections and camera differences are preserved; exact-pose tolerance fails rather than being misreported as a match. Parent notes modest surface improvement and still-weak city depth. Independent review is pending; A is a retained comparison candidate, not stage acceptance. Next comparison changes only road cubemap sampling through a separate serialized template.

### Controlled road-reflection candidate B preserved

Added an Editor-created, serialized Resources road template retaining mapped Lit keywords, with environment reflection sampling enabled while the original reflection-off control remains unchanged. B selects that template and preserves A's light/sky/fog, geometry, generated maps, smoothness, probe configuration and race state. Native GUID `de942ca116044e6eb2141c36b547d8a3` completes1,440 verified PNGs;197source/189app hashes are unchanged. The sampled road darkens without useful broad reflection shape; B is not accepted. Preserve this result and its comparison metadata before diagnosing the actual fog color upload and, if warranted, a bounded road-probe correction. No new performance or watched-motion claim is made.

### Production atmosphere correction C preserved

Independent review021 rejects B because broad road shading darkens without useful reflection benefit. A remains a better road-readability control. The diagnostic native build verifies the fog upload was gamma-converting intended linear values from `.041/.074/.086` to `.003174/.006451/.007986`. C assigns the intended color through `.gamma` in runtime and serialized scene setup; native readback now confirms `.041/.074/.086` with unchanged density. No road, sky, light, gameplay or camera contribution changes in this comparison.

C GUID `1287761a66a64ff9bb7a952eb5519b0a` completes1,440 validated PNGs with197 source and189 app hashes unchanged. Eight natural crossings and explicit failed exact-pose checks are preserved. Parent-inspected open views show layered city haze and retained thermal highlights; the road still needs correction. Independent C review is pending. Next test is the documented two-local-probe fallback with C lighting and roughness fixed; no performance or watched-motion acceptance is claimed here.

### Independent Stage1 A/B/C verdicts recorded

Review021 independently inspects all eight originals per candidate. A is a modest direct-light improvement; B is rejected because its road darkens without useful reflection benefit. C is accepted as an atmospheric-depth correction: existing near/middle/far city masses separate in the three open primary views, while thermal and workshop contrast survives. Its fog becomes the fixed control for the next road experiment. Full-production depth is approaching, and Stage1 remains open for credible broad road response. The report preserves every earlier verdict and exact-pose limitation.

### Local reflection candidate D preserved at owner wrap-up

D adds two bounded one-shot local 256px probes and retained Forward+ blending/box-projection/atlas settings with C atmosphere and unchanged road roughness. Native GUID `5edbc3cfa5be45358031fe8050f71d28` completes 1,440 validated PNGs; 197 source and 189 app hashes remain unchanged, and all three probes report completed render IDs. The parent-inspected five primary originals do not establish the required road improvement. D is preserved as an unaccepted experiment, with no independent motion/performance acceptance. The owner then requests wrap-up and push; no roughness, SSR or later-stage experiment is initiated. Final checkpoint will retain C atmosphere and restore A's clearer road.

### Owner-requested Stage 1 checkpoint and stop

Preserved unaccepted D as commit `69ba4af`, then restored the clearer A reflection-off road with C's independently accepted fog correction. Removed D's two local probes and pipeline blending/box-projection toggles; kept the serialized reflection material and all comparison evidence. No roughness, SSR, later city/craft/racing work, release or merge was started.

Final local GUID `40bed5f53c2449418e7fb56bf59739f6` builds successfully. Two short native runs exit zero: corrected fog readback and automated title-to-race smoke. All five 1920×1080 PNGs pass CRC/decompression, and all 197 source plus 189 app hashes remain unchanged. Parent inspected the title/fog and starting race: readable deck panels, player, nearby rival and HUD remain visible with layered city haze. Sixty document links resolve. A process audit confirms the project's Unity/game jobs are closed.

This is a resumable checkpoint, not completion of Stage 1 or production acceptance. No fresh full-circuit/alternate-aspect capture, current test-suite/performance/three-lap run, watched motion or manual-with-audio pass is claimed for the combined checkpoint. Updated README, implementation plan, production plan/spec and `docs/HANDOFF.md` distinguish this local app from the previous accepted archive and explicitly hold further work until the owner resumes it. Source/evidence and this handoff are pushed; all project work stops.

### Complete development retrospective and resumed road plan

Delivered [the illustrated work record](development-work-record.html) and its [Markdown source](development-work-record.md): approximately 16,400 words covering the whole project through `3097bc8`, with source/evidence chains, rejected assets and experiments, explicit tool provenance, six original images and all 60 historical commits. All 416 Markdown links resolve locally; the generated HTML has ten main navigation sections. The browser URL policy blocked the file preview, so no browser layout acceptance is claimed. The retained Node renderer reproduces the HTML from Markdown using marked. Documentation work did not run Unity, Blender, the game or video encoding.

The owner then says “continue” after the proposed next step. [The bounded road plan](next-road-pass.md) is approved: establish the actual combined-checkpoint opening control, test one road-smoothness variation with the accepted atmosphere fixed, evaluate it in native context and preserve the verdict. Historical pause text is explicitly superseded by this resumption; previous app/build acceptance remains unchanged.

### Resumed road control captured

The unchanged combined checkpoint GUID `40bed5f53c2449418e7fb56bf59739f6` completes a fresh 1,440-frame native circuit. All PNG CRC/decompression and 197-source/189-app identity checks pass. Parent inspects five opening originals and three thermal/station controls; exact-pose tolerance against the earlier baseline fails and is labeled. A continuous silent 15-second simulation-time clip includes the opening slowdown and passes full decoding. This does not establish watched-motion or real-time performance acceptance. [The decision record](../evidence/night-production/road-response/decision.md) defines one moderate smoothness shift with atmosphere/reflection state fixed, plus read-only material telemetry. The old app is retained locally for a precise rollback if rejected.

### One smoothness candidate rejected after native comparison

Raised only the road mask endpoints by.10, retaining the.90 multiplier, field shape and every lighting/reflection/geometry contribution. Added read-only native material/GPU-mask telemetry before the capture race. Candidate GUID `1441c9f070434c73a2c65dd244581929` reports an actual uploaded effective range.437647–.508235; generated noise never reaches the nominal endpoints. The first run resized atframe433 and is preserved as rejected capture evidence. Fixed the validator to enforce requested dimensions and support the new explicit control reference.

The unchanged rerun completes1,440 valid1080pframes, source/app/scene identity checks and a fully decoded15-second clip. Parent and independent [review004](road-reviews/004-resumed-smoothness-native.md) reject the material candidate for insufficient useful full-frame gain; readable panels and controls survive, but the road remains plain and lacks broad source-related response. All comparisons fail exact-pose tolerance and are labeled accordingly. Preserve the experiment separately, restore the exact control and reassess technique; no further roughness variant or costly reflection implementation is part of this pass.

### Exact control restored; bounded road experiment closed

Preserved rejected candidate source at `a5debfb`, then restored its three runtime/scene files from `8df99ad` and restored the retained exact control app. Kept the capture validator's resolution/reference correction. All197 source and189 app hashes plus the scene SHA match control GUID `40bed5f53c2449418e7fb56bf59739f6` after verification.

Fresh tests pass42/42. A separate native real-time race finishes three laps in128.32s with all-six-racer zero-recovery final telemetry, both restart launches and three countdown-pause checks passing, then exits0. Six1080p PNGs pass CRC/decompression; the parent inspects the real results image. The60-second M2Max/VSync1 sample has6,967 intervals, mean8.61/P959.25/P9916.74ms and243.3MiB allocated memory. Preserve its higher P99 than the older baseline; no candidate-performance or isolated GPU claim follows. [Verification](../evidence/night-production/road-response/final/verification.json).

[Feasibility005](road-reviews/005-reflection-feasibility.md) recommends one isolated native preview-SSR availability/visual prototype on pinned URP17.6, grounded in installed guarded source and official opt-in evidence. No new reflection implementation or dependency was added. README, handoff, plans and retrospective cross-links now identify the completed rejected experiment and restored control. Broader Stage1 and watched/manual/aspect acceptance remain open. The full retrospective retains its original60-commit cutoff and links this subsequent work separately.

### Consistent visual references and gap evaluation

The owner requested a coherent new reference set and evaluation against the current game. Generated four original 1672 × 941 PNGs with the built-in image tool, using actual native images as composition/geometry inputs and the new opening view as the shared appearance input for the gallery, thermal passage and neutral craft study. The [Nocturne V2 record](../references/nocturne-v2/README.md) preserves exact prompts, source camera/file identities, target hashes, consistency observations and prioritized acceptance criteria. The [interactive comparison](visual-targets-v2.html) displays original native images alongside generated targets and supports enlarged native/target views.

Parent evaluation finds the largest gaps in road/light interaction, connected city bases, and structural/material finish. The existing craft identity is worth keeping. Generated road sheen and micrograin are upper bounds; small projection/geometry drift and strong cyan outlines remain explicit limitations. The neutral native craft comparator is the historical contact-repair control with older maps, not a new current-app material inspection. This is not an independently delegated critique or a native production pass.

All four generated and four native PNGs pass CRC/decompression; all local comparison/record links resolve and all native comparison files are tracked. The opening side-by-side layout and enlarged-target control were inspected in the Codex browser; no broad responsive-device claim is made. Runtime, shaders, imported game assets and the local app are unchanged. No Unity build, game run, new tests or reflection implementation was part of this step. Old references and rejected experiments are retained.

### Nocturne V2 implementation planning

Prepared the [reference-aligned execution plan](superpowers/plans/2026-09-08-nocturne-v2-execution.md) after the owner asked for an implementation plan. Re-read pinned SSR APIs and existing setup/capture code; defined seven gated milestones, explicit file ownership, separate preview-app output, off/on comparison, native acceptance and rollback criteria. Corrected habitual duplicate preparation and preserved the provisional performance-budget discrepancy. Local plan links resolve; no runtime changes, builds or game tests were made.

## 2026-09-08 — Nocturne V2 SSR source prototype and retention finding

Owner authorized execution. Added separate SSR preview setup/runtime and optional native diagnostics; standalone opt-in invocation exits 0 and guarded Editor compilation/resource lookup succeed. Preserved current app hashes. Independent source review then identified SSR SelectOnly stripping the OFF keyword variant required by the planned runtime comparison. The first build was deliberately terminated (exit 143), with no native result or visual acceptance claimed. Preserve this implementation/finding before a separate retention correction. See `evidence/nocturne-v2/ssr-01/attempt-01.json` and `docs/visual-target-reviews/001a-reflection-source-review.md`.

## 2026-09-08 — SSR keyword-retention correction and first native run

Added matching unused SSR-free renderer as a build requirement. Generated prefilter is Select(1), retaining both states; original renderer remains default and SSAO is unchanged. Independent 001b review clears the correction. Separate Development preview builds successfully, but its first SSR-off run exposes a pre-existing IonPropulsion MaterialPropertyBlock constructor allocation rejected by Development checks. Integrator terminated invalid capture (exit143); archived binary and excerpt retained. No valid off/on comparison yet. The next bounded correction moves only that allocation into Awake and repeats the native run.

## 2026-09-08 — Propulsion startup correction verified; SSR01 native failure preserved

Moved only MaterialPropertyBlock allocation into IonPropulsion.Awake. Independent 001c review clears source; rebuilt Development preview GUID47944ad3754d45adb51c354c3251c518 completes clean SSR-off capture without the earlier initialization exceptions. Both off and on finish1440 original1080p PNGs, all integrity checks pass, nine natural crossings retained, exact-pose tolerances still fail. ON produces six RenderGraph exceptions on reflection-probe rendering and shows the error console, so image integrity is not a reflection or quality pass. No performance sample collected. Original control app unchanged. See `evidence/nocturne-v2/ssr-01/native-validation.json`.

## 2026-09-08 — SSR02 camera scope and native instrumentation

Replaced the preview feature with a project-owned subclass that delegates Game cameras and skips reflection cameras; kept the installed package, probe, materials, lights and scene behavior. Enabled the supported Instrumented managed variant in the separate Development app. Independent source review 001e found no blocker. BuildReport succeeds; the Editor later lingered after its exit message and was terminated with exit 143, recorded separately from the two successful native exits.

SSR02 GUID `f1a459e36c064afc930b841eede3d488` completes both off/on runs with 1,440 intact 1920 × 1080 original PNGs each. Source and app hashes remain unchanged across capture; all nine natural selections retain failed exact-pose tolerances. The prior rendering/initialization exceptions are absent. Instrumented SSR/upscaling/final-blit CPU markers record 1,690 positive frames on and zero off. This establishes instrumented pass work, not correct GPU buffers or useful reflections. Shutdown warnings remain recorded. Original control app hashes still match.

See [native validation](../evidence/nocturne-v2/ssr-02/native-validation.json). No GPU-output inspection, realtime performance, continuous-view or manual/audio acceptance is claimed. The candidate and source correction are preserved before the separate visual verdict.

## 2026-09-08 — SSR02 visual rejection and next proposal

Preserved candidate `dd7a417` before this separate verdict. Independent review 001g inspected all 18 selected original images and found no convincing whole-frame reflection improvement. Static scene appearance is broadly preserved; minor local differences are inconclusive under measured natural pose differences, especially the two thermal pairs. Positive CPU pass counters do not prove GPU input/output correctness. No motion, manual/audio or realtime-performance acceptance is claimed.

Retained the exact original app and rejected SSR02 production adoption. Added the [native comparison page](ssr02-native-comparison.html), current verdict, and source-grounded proposal 001h for one opening-only light origin/aim/footprint candidate, explicitly direct lighting rather than reflected structure. That proposal is not implemented; the reflected-structure technique and Tasks 2–7 remain pending. Source retains the isolated default-off prototype, as identified in handoff. No package upgrade, control-app replacement or release publication occurred.

## 2026-09-08 — Parallel critique of negligible SSR gain

At the owner's request, delegated independent visual, rendering and environment reviews with separate document ownership. Critics inspected original native images, targets and relevant source. They agree that SSR does not supply a demonstrated useful visual upgrade; the small fixture correction is inadequate as a headline milestone. The visual reviewer favors a controlled gallery finish first; the environment reviewer favors visible opening construction. The [synthesis](visual-target-reviews/002-improvement-approach.md) recommends an opening-passage deliverable with existing-asset visibility/placement audit, connected service construction, coherent local light and surface response; the gallery supplies a bounded surface study. Optional GPU diagnosis remains separate and capped.

Preserved all three reports and their differing priorities. No game code, assets, builds, captures or performance measurements changed. Proposed work still requires native visual/motion/technical acceptance; the critique itself is not implementation.

## 2026-09-09 — Opening finish source: preview controls and surface study

Owner authorized the next bounded pass at High effort. Added separate preview controls/build setup and a nine-anchor sparse native capture mode. Implemented scoped opening/warm-gallery road material regions with unchanged original collision, existing-source light rhythm and warm-gallery material separation. Source assertions/diff checks pass; Unity compile, native appearance and performance remain pending. Environment construction is a separate owned source step. Source reports are retained under `evidence/nocturne-v2/opening-01/`; original app has not been rebuilt or overwritten.

## 2026-09-09 — Opening construction source and measured placement

Added two default-off service connections from existing industrial/tower frontages to actual .200/.275 pier feet. Reuses existing materials and contacts; 49 boxes in up to eight batches, four housed local lights, no new colliders. The editable procedural recipe and source-based bounds/projection/whole-course clearance audit are retained under `SourceAssets/environment-v6-context/`. Projection rectangles are not proof of visible native coverage. Source is frozen for compilation and native feedback; visual acceptance remains pending.

## 2026-09-09 — Opening01 native draft candidate preserved

Separate Instrumented, non-Development preview builds successfully and exits 0; GUID `117ef07dd95f4ceab9efba1e41f5a6d7`. Fresh existing EditMode suite passes 42/42. Same-binary off, construction, surface and combined modes each capture nine original 1920 × 1080 natural-crossing PNGs with all 1,440 pose frames retained. Source/app identity, PNG CRC/decompression and selection validation are recorded per mode; exact-pose tolerances do not pass, so comparisons concern whole-frame appearance. Runtime confirms two accepted groups/four new construction lights. Native shutdown thread warnings remain; no exception-free-all-categories or performance claim is implied.

Generated settings keep SSR inactive and SSAO/default renderer intact. The original app remains unchanged. This commit preserves the first integrated candidate before its independent visual verdict and any correction; source cleanliness and file integrity are not visual acceptance. No continuous-motion, audio, full-lap image or realtime-performance gate has yet passed for this candidate.

## 2026-09-09 — Opening01 independent rejection and first correction

Candidate `ce50b9e` was preserved and pushed before this verdict. Independent review003c inspected all18 original off/combined stills plus targets A/B; current off also broadly preserves two inspected historical control views. Combined is rejected: approach additions read as pale blockout slabs with black support contacts; bend shows mainly a roof wedge; new road atlas flattens the opening and warm gallery. Later regression views retain broad static relationships, subject to recorded pose differences.

Added the four-component [native comparison](opening-native-comparison.html). The first substantial correction will make compact occupied fronts and their supports visible from both required views, narrow/darken the exposed walk roofs, reuse metric architectural finishes, and restore original road detail frequency beneath broad variation. Existing source footprints will be shaped coherently; no global lift, extra light count, camera/route changes or reflected-structure claim. This is a new testable hypothesis, not acceptance.

## 2026-09-09 — Opening02 road frequency and fixture correction

Restored the original road albedo/normal textures as a separate URP detail sample at their original UV frequency/phase, while retaining scoped broad albedo/roughness/normal atlases. Macro normals fade to flat at boundaries; fine normals retain .25 strength and .35–.50 ×.90 smoothness limits. A dedicated Resources material with an authored detail-map seed retains `_DETAIL_MULX2`; the preview builder now fails if import/preparation loses that template/map/keyword. Existing light counts and current study intensity totals are unchanged; tighter oblique footprints and reduced gallery backing/face contrast address the native failures.

Independent review of the frozen surface math/ownership found no blocker; combined construction review/native build remain pending. Source analytical checks and whitespace check pass. This is the first substantial correction's surface step, not a native acceptance claim.

## 2026-09-09 — Opening02 connected frontage correction and guarded build failure

Replaced hidden ground rooms/pale broad slabs with compact occupied fronts on the original service datums,7 m walks,grounded frame contacts and reused metric architectural finishes. Fixed overlapping mapped room/walk floors during independent review. Frozen audit accounts for101boxes/1212triangles including all lamp housings, no route/reservation screening conflicts; exactlyfour local lights. Fresh42/42 EditMode tests pass. Independent003d finds no remaining source blocker but leaves native gates open.

The first build then stops with exit1 beforeBuildPlayer: the added full-variant guard finds `_NORMALMAP` missing after prepared-control property copying. No opening02 app is produced. This failure is preserved separately; copying temporary control defaults did not reliably retain the complete template. Next correction uses real existing authored normal/gloss/detail map references, explicit five-keyword restoration and a post-save assertion. Runtime still replaces those seeds before creating the regional renderer.

## 2026-09-09 — Opening02 persistent shader-template seeds

Corrected the demonstrated preparation failure with persistent existing normal/gloss/detail map references on the separate template, white retention emission, explicit restoration of all five required keywords and assertions after SaveAssets. Runtime replaces every seed before the region renderer exists. Independent source review clears retry; no source defect is silently relabeled as a visual pass. The preserved attempt01 remains distinct from the new build.

## 2026-09-09 — Opening02 native control-template failure preserved

Build attempt02 succeeds and exits0, GUID820fb25545d3470894c50fee5095e2c0, with correct regional detail-template guard. Generated baseline RoadSurface nevertheless loses its normal and gloss-map keywords. Native diagnostics confirm the actual road material has only emission/environment-opt-out keywords while scalar smoothness is.9: assigned normal/wear textures are ineffective. Off and combined each finish9 intact native diagnostic PNGs with all1440 poses and unchanged source/app hashes. These are rejected as a valid whole-frame comparison; only construction placement may inform a provisional review.

Preserved opening02 app/evidence and generated source state. Next technical correction persistently seeds both baseline and regional build templates and asserts their distinct full keyword sets. Original control app remains unchanged. This is a retention correction within the first visual revision, not another art-constant round or accepted quality gain.

## 2026-09-09 — Preserve baseline road shader features in preview builds

Added persistent authored normal/gloss seed references and explicit baseline keyword restoration alongside the regional template, with both feature combinations asserted after save. Runtime replaces seed values before drawing. This corrects the observed opening02 control failure and will be verified in separately named opening03; art source remains the same first correction.

## 2026-09-09 — Opening03 valid native component evidence

Separate appGUID5b0ab4accd4c40279591b27098ff49ed builds and exits0; both template guards pass and native actual baseline road now retains all four required features. Off/construction/surface/combined each complete9 original1080p PNGs with1440poses; source242/app189 identity and image integrity pass, exact poses differ. Runtime accepts both construction groups and all four lights after99existing-city OBB checks. No native rendering/initialization exceptions found in these logs; shutdown diagnostics remain recorded. Original app unchanged. Candidate preserved before separate whole-frame verdict; no motion/performance claim.

## 2026-09-09 — Opening03 surface rejection and final bounded correction

Independent003f inspects all18 off/combined originals: construction now adds useful occupied fronts at.15/.22, depth at.30 remains, later views broadly preserve hierarchy. Combined surface is rejected because broad cloudy patches replace the control road's directional response, especially in the warm gallery. Fine-normal frequency restoration did not recover that response.

Final permitted substantive correction: remove the regional road integration and retain the complete original road mesh/material/all three maps, while keeping useful construction and the bounded gallery/fixture study for one last combined comparison. This removes a demonstrated regression and does not claim the broader reflected-source road goal. If the retained surface/lighting state still regresses, select the verified construction-only state rather than perform a third art sweep.

## 2026-09-09 — Final correction restores original road rendering

Removed WorldBuilder's regional road integration. Independent source review confirms the full pre-opening road mesh/material/generated maps/collider path is restored; construction and scoped gallery/fixture study are unchanged. The helper remains isolated as historical experimental source. Whitespace/source comparison passes; separate opening04 native compilation and combined-preservation review follow.


## 2026-09-09 — Global critique after opening04 owner rejection

The owner found the latest visible changes too small and requested a realistic whole-game critique. The parent inspected current native views and current source, with independent visual and gameplay/production critics. The [synthesis](visual-target-reviews/004-global-aaa-assessment.md) judges the game a functioning prototype substantially below the requested visual target and with whole-race experience still unproven. The opening promotion path is paused. Scene-wide construction, material/light coherence and sequence design take priority over another isolated module or renderer tweak; an integrated section is proposed, not implemented.

The review distinguishes current source from older race telemetry and avoids re-reporting corrected decorative markings, old AI pacing defects or old frame-time samples as current failures. Original game/source artifacts and pending opening04 captures remain intact. No new game implementation, build, tests, manual play, audio audition or performance measurement was performed for this documentation milestone. Independent reports are preserved separately as 004 and 004a.


## 2026-09-09 — Plan a substantial production rebuild and retain the lesson

The owner asked for a solution plan and explicitly rejected further small, low-impact work. The [production rebuild plan](superpowers/plans/2026-09-09-nocturne-production-rebuild.md) acknowledges that earlier plans already promised broad outcomes: execution and acceptance narrowed them. It therefore requires whole-lap rough composition, a replace/rework/retain/demote inventory, finished native asset exemplars, a saved editable Unity production scene with proven lighting persistence, integrated sound and racing, mandatory full-course rollout, and complete experience validation.

The parent checked current Bootstrap/WorldBuilder/Prepare behavior and Unity's official lighting documentation before proposing the scene workflow. An independent visual critic reviews whether the plan materially differs from the rejected process. Project-level lessons are retained in AGENTS.md. No game implementation, build, bake, new tests, human play or performance sample is part of this planning milestone; the plan does not promise an unsupported AAA completion date.


## 2026-09-09 — Concrete Sol coding / Astra modeling packet and owner-first review gate

The owner requested separate coding and modeling roles, with GPT-5.6 Sol coding and Astra authoring Blender/models, and an explicit stop for owner review before the parent reviews. The [execution packet](superpowers/plans/2026-09-09-sol-astra/README.md) supplies separate assignments, a versioned coordinate/mesh/material/layout/light contract, immutable handoffs, runtime integration traps, behavioral tests, required delivery artifacts and a state machine that blocks automatic review. Blender Python stays with Astra; Unity/import/build/settings and gameplay code stay with Sol.

The parent reconciled this order with the previous production plan and retained it in AGENTS.md. This milestone writes plans only: no implementation worker was launched, no rebuild code/art changed, and no new runtime quality or performance claim is made. Future handoff is implementation-complete/awaiting-user-review, never an inferred AAA acceptance.


## 2026-09-14: polished-race planning and conversation handoffs

Owner approved the reviewed direction and requested plans plus separate handoffs, all for GPT-5.6 Sol. Created `docs/plans/2026-09-14-playable-race/README.md` and four self-contained execution handoffs: Integration, Environment, Racing, and Replay/presentation. Updated the implementation-plan entry and AGENTS.md precedence so older model assignments and dispatch states do not override this packet.

The packet preserves the original night-world composition, makes gallery native acceptance the first visual milestone, retains full-course rollout, and defines competition, personal-best ghost/sector feedback, audiovisual verification and human play gates. Shared files, isolated workspace setup, hardware scheduling, report exchange and integration ownership are explicit. The user will start the new conversations; none were created for this planning task.

Checks: re-read current root workflow, gallery integration contract, Unity version and gameplay interfaces; checked handoff file presence, model assignments and packet references. No gameplay, assets, scenes or runtime settings changed. No tests/builds were needed for this documentation-only change.

Repository limitation: active branch `codex/nocturne-production-handoff` has no HEAD commit and extensive pre-existing staged/untracked content. The existing index was not changed and no commit/push was attempted, avoiding accidental inclusion or rewriting of prior work. Handoff 01 owns recoverable snapshotting and scoped milestone history in a prepared integration repository. Execution under this packet remains not started.

## 2026-09-14: preserve baseline and prepare isolated race workspaces

Captured the original checkout without changing its branch, index, worktree or remote. The original had no local `HEAD`, so worktrees were not used. A copy-on-write snapshot retains `.git`, editable sources, Unity metadata, historical evidence and native apps. A clean authored-source repository was established at commit `1762224deed3c9e994483f5307999b68ae386e38`, then cloned into independent Environment, Racing and Replay/presentation lane branches.

Published `coordination.md`, `contracts.md` and the initial Integration report. Coordination records provenance, excluded caches, absolute ownership paths, report exchange and the atomic heavy-process lease. The contract keeps race timing and ordered progress authoritative in Gameplay, makes mixed automation ineligible, invalidates recovery laps for ghost/sector comparison, and keys persistent data to course, driving rules and schema.

Checks: all four repositories resolve to the same baseline commit and were clean after creation. The private pre-commit path audit found no sensitive-looking filenames; browser page snapshots were excluded and the only Unity password field is blank. Live GitHub metadata reports `emrickk/vector-rush` is public, so no push was attempted under the private-only authorization. The preserved baseline app identity and executable hash were recorded. Current Unity compile/tests and native behavior remain open; static inspection found shared tests referring to an alternative race/record API and does not treat historical 114-pass output as current evidence.

## 2026-09-14: current baseline compile and Gallery02 identity verification

Ran a fresh Unity 6000.6.0f1 EditMode attempt from the isolated Integration workspace. Unity exited before producing test results because runtime compilation reported 37 distinct errors. The failures separate cleanly by ownership: `AAABaselineIntegration` expects missing Environment lighting constants, `ProductionSettingsUI` expects an alternative Replay-owned preferences API, and shared `ProductionEvidence` expects an alternative Gameplay finish-record shape. The Unity log is retained outside source under `artifacts/integration/baseline-01`; this is a compile failure and no test count is claimed.

Independently hashed the current 1,201-frame Gallery02 course export using the C# float-bit contract. The canonical identity is `8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e`, while the exact JSON source hash is `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`. Manifest, layout, lighting and STAGED still use the JSON hash as course identity. All 40 current sealed entries and the STAGED checksum verify. No package bytes were changed; Environment owns correction and resealing before Integration imports.

Added the unfilled five-person playtest observation sheet. Native baseline launch was deferred when Racing acquired the shared heavy-process lease. No process or lease belonging to that worker was changed.

After Racing released the lease, launched the preserved baseline `Vector Rush.app` directly at 1280 by 800 in its quick evidence mode. The app exited 0 and wrote title and race-start PNGs plus its player log to `artifacts/integration/native-baseline-01`. Direct inspection confirms a rendered Nocturne menu and live start view with the player, an adjacent rival, elevated night-city course and HUD. The log contains no runtime exception/error match and retains Unity shutdown thread-finalization warnings. This proves only that the preserved native app launches and renders those two states; its source revision, full-race behavior, sound, handling and performance remain unverified.

## 2026-09-14: combine lane source and restore the production-editor assembly

Integrated Environment commit `39bd39d`, Racing commits `4cfa3e8` and `4cfbc4a`, and Replay commits `b2f95ba`, `611ae1e`, `696eda0`, `07d5775`, `6867886`, `b1a2ce9`, and `f988902` into the isolated Integration branch. The shared bootstrap now constructs `ReplayPolishController` immediately after `RaceDirector.Initialize`, so replay observes Gameplay's authoritative clock, eligibility, and ordered crossings without becoming a second race authority. Integration also revised `RaceDirector.DrivingRulesId` to `vector-rush-rules-v2` and aligned `ProductionEvidence` with the retained finish-record shape.

The next combined compile exposed a baseline source-integrity defect: `VectorRush.EditorTests.asmdef` referenced `VectorRush.ProductionEditor`, but the corresponding importer, manifest, scene builder, assembly definition, and metadata were absent from the captured worktree. The original index still named those eight files, although its local object database could not read their blobs. The existing public `codex/nocturne-production-handoff` commit `873150d` contains all eight files, and each downloaded Git blob hash exactly matches the preserved index entry. Those exact sources are restored here. This is source recovery for an already-referenced assembly, not a new importer design.

Checks: every recovered file hash matches the preserved original index, the Replay commit range passes `git diff --check`, and the Integration worktree is limited to the recovery files plus this documentation before commit. No Unity test verdict, Gallery02 import, candidate build, native race, audiovisual review, or performance result is claimed yet. The next bounded step is the focused Racing/Replay/Environment suite followed by the full EditMode suite under the shared heavy-process lease.

## 2026-09-14: combined compatibility stabilization

Reconciled the combined source with the retained legacy interaction contract without adding a second race clock. `RaceFinishLedger` now exposes the legacy integer API as an adapter over `RaceFinishLifecycle` when owned by `RaceDirector`; direct legacy ledger behavior remains available to its existing tests. Restored the expected private `finishLedger` and `records` integration surfaces, preserved sticky automated-run exclusion through the player vehicle update path, rejected partially invalid persisted record rows, and kept record-save failures non-fatal with ordered save and cleanup diagnostics. The finish state now reconciles externally observed legacy crossings and permits pausing a finished race while unresolved rivals continue.

Focused Unity 6000.6.0f1 EditMode verification passes 70/70 with zero failures or skips. The NUnit result is `artifacts/integration/combined-tests-01/focused-05.xml`; the corresponding Editor log is `artifacts/integration/combined-tests-01/focused-editor-05.log` and records exit code 0. This establishes the combined contract tests only. The full EditMode suite, Gallery02 import, candidate build, native race/replay behavior, visual/audio review, performance, controller comfort and human handling remain open.

## 2026-09-14: full EditMode compatibility repair

The first complete-suite run executed 164 tests: 143 passed and 21 failed. Twenty failures exposed the retained production settings fixture against Replay's newer HUD-owned menu, and one exposed a missing public context-export helper in the recovered production editor source. Added a narrow modal-input bridge so the legacy settings panel owns its activation frame in either component update order, retained an integer menu-focus compatibility field without replacing the newer focus controller, and allowed Input System keyboard activation when both Unity input backends are enabled. Restored `ExportContextToDirectory` with exact closed-course bytes, source identity, context checksum and status output.

The targeted repair suite passes 40/40 and the full Unity 6000.6.0f1 EditMode suite passes 164/164 with zero failures or skips. Results are `artifacts/integration/combined-tests-01/full-repairs-02.xml` and `artifacts/integration/combined-tests-01/full-02.xml`; the full Editor log is `artifacts/integration/combined-tests-01/full-editor-02.log` and records exit code 0. Gallery02 import, candidate build, native behavior, visual/audio review, performance, controller comfort and human handling remain open.

## 2026-09-14: Gallery02 FBX axis failure diagnosed

The first Gallery02 import passed package, checksum, course, layout, material, and texture validation, then stopped before publication on the first FBX bounds check. `NA_GalleryPortal_G02_LOD0.fbx` measured `Z -1.61..0.905` against sealed expected bounds `Z -0.905..1.61`, a `0.705 m` discrepancy. The failed report and partial staging remain preserved under `artifacts/integration/gallery-import-01` and the Unity project respectively while the cause was tested.

A bounded Unity 6000.6.0f1 probe reimported that retained FBX with axis baking on and off, then restored the original setting. `bakeAxisConversion=false` reproduced the declared `(x,z,-y)` bounds to floating-point tolerance with identity root transform and unit scale; `true` reversed the forward sign. Updated the importer to disable Unity's extra axis bake while retaining unit scale, the exact package bounds, and the existing 1 cm rejection. Probe evidence is `artifacts/integration/gallery-axis-probe-01`. Successful full import, publication, native appearance, and motion remain open.

## 2026-09-14: Gallery02 technical import complete

Moved the failed partial destination intact into its failed-attempt evidence, then retried from a fresh Unity asset path and fresh evidence directory. Unity 6000.6.0f1 imported all 12 selected station FBXs with axis baking disabled. The maximum bounds error across the imported meshes is `0.00000190734863 m`, well inside the unchanged `0.01 m` rejection threshold. The importer created six prefabs, 10 materials, 11 textures, 19 placements, and 20 local lights, then published only `Assets/Resources/AAA/GalleryExemplar.asset`. The report status is `TECHNICAL_STAGING_COMPLETE`; visual and native status remain blocked and not run.

The complete post-import EditMode suite passes 164/164 with zero failures or skips. Import report, Editor log, test result, and test log are under `artifacts/integration/gallery-import-03`. This establishes package integrity, persistent imported assets, resource publication, and code regression coverage. It does not establish native appearance, LOD behavior in motion, audio, handling, controller comfort, or performance.

The first candidate's matched capture then proved the Gallery flag had no effect: `off` and `gallery` reported identical scene counts, and neither player log contained the runtime hook's load marker. Source inspection found `AAABaselineIntegration.TryAttach` was implemented but never called. Added the missing call immediately after the legacy world build. With no flag or `off`, the hook still returns before loading or mutating a resource. A replacement candidate and matched native evidence are required; the first binary is retained as rejected integration evidence.

## Opening-city baseline — 2026-09-14

Recorded e243303 / r4 f8e1fcbc54e74d04bd9ffa8a05be1237 and inspected its native opening frames. Read supplied original/concept/critic. Defined one 18-second opening passage and isolated branch art/opening-city. Pending upstream gallery/replay corrections; no visual acceptance.

## Opening-city large shapes and early native review

Original Blender source + four FBX exports, visual-only district construction and default-on hook. Early-02 app GUID 535eac3073334c9db87bc0b965d94fcb imported/built/launched; exact natural pose matches across 432 frames. Art review requires revised landmark placement, connected braces, subdued lower transit, stronger reveal foreground. Those corrections are included in this source checkpoint and await native verification. Optional paused-still diagnostic removed after craft-disappearance defect; valid natural captures preserved. Integrated Gallery/replay source through c2de061 is retained.

## 2026-09-15: owner-requested Stage 1 parallel execution

Three subagent roles covered scene ownership/native integration, independent visual review, and source/evidence validation. Inspection found the requested art corrections already committed at `88d6399`; no duplicate art mutation was made. The coordinator preserved the interrupted Integration lease intact after verifying its task was interrupted, its owner PID absent, and no heavy process running, then used the existing atomic lease wrapper for each new build/capture.

The first fresh build exposed dropped named screenshots during full-frame capture. Its failed evidence remains at `artifacts/opening-city/stage1-checkpoint-01`. Commit `69bafb2` corrects this without changing artwork. Commit `8d733a8` provides a strict external evidence validator; `366e5aa` records the independent visual review.

Final app `Vector Rush Meridian stage1-candidate-01.app`, GUID `c366cc453e5145029c61e7f0b627b198`, builds from runtime source `69bafb2` and existing art `88d6399`. The same binary produced 432 natural frames in each mode, 870 verified PNGs, zero paired recorded pose/FOV difference, three image pairs and two silent 18-second videos. All three final after anchors are byte-identical to the inspected checkpoint frames. Prior early-02 was stills-only and does not supply a complete saved continuous PNG sequence; this stage's validation closes that evidence gap.

The visual verdict is owner-reviewable composition/construction progress, not finished-game quality. See [Stage 1 report](stage1-opening-review.md). Materials, lighting finish, continuously watched motion, audio, performance and human handling are not accepted by this result. Original app and untracked art metadata remain preserved. Stage 2 has not started; stop for the owner.

## Stage 2 propulsion and interface planning

The owner requested the HUD and all player interfaces alongside the proposed stronger propulsion effects. Inspected current RaceHUD screen branches, preference/navigation behavior and existing IonPropulsion inputs. Prepared [the staged plan](stage2-propulsion-and-interface-plan.md): shared visual direction, native propulsion/HUD milestone, complete player screen rollout, and integrated verification. Explicit ownership prevents HUD/menu edits colliding in the same source file. This is documentation only; no runtime, assets or builds changed.

## Stage 2A interface references

The owner requested a stronger HUD direction and authorized reference imagery. With the built-in generator unavailable, authored editable HTML/CSS reference views directly over the actual game. A subagent added the narrowly gated diagnostic `-openingHideHud` at `32d1cf2`, built a fresh diagnostic app and captured clean approach/bend/reveal images. Same-build HUD-on captures are byte-identical to Stage 1; all 432 recorded camera/vehicle poses match. Normal launch and hide-flag-alone checks leave the HUD unchanged.

Delivered six reference PNGs and an interactive current-versus-proposed board in `references/interface-stage2a`, including licensed local fonts and provenance. Independent critique led to larger secondary labels, stronger minimap separation, native metre-based rival gaps and simpler headings. Browser checks cover the local-file preview, navigation, sliders/toggles and responsive board fitting. These are UI reference designs, not implemented native UI, new exhaust effects or AAA acceptance. The propulsion portion of 2A and implementation milestone 2B remain open.

## Stage 2 implementation and integration checkpoint

The owner authorized completing 2A–2D without intermediate stops. Parallel owners implemented native UI/preferences (`9f7bc09`), state-driven propulsion (`ba8c3bc`) and native evidence/profile isolation (`66bb6b1`, `66cf493`, metadata correction `d4981b9`). Integration adds ReturnToTitle lifecycle reset, cached personal-best reads, truthful swallowed-save failure reporting, boost-release state exposure and dedicated diagnostic profile wiring for preferences/records/ghost.

The initial build passed but its capture hook was unavailable because of malformed new script metadata. Initial tests exposed this and keyboard/modal input regressions. Corrective changes preserve the existing interaction tests. The complete corrected EditMode suite passes 203/203 with no skips at `artifacts/stage2/initial-01/tests-modal-fixed.xml`. New lifecycle tests verify leaving paused results stops the continuation and permits a fresh race; record-write failure remains visible without inventing a new best. Native visual, input and performance review remains pending at this checkpoint. Prior app/captures and the failed attempt are preserved.


## Scenario HUD and menu references — 2026-09-15

The owner rejects the current interface finish, requests polish without additional features, and explicitly selects the Scenario API. Generated four screenshot-based concepts: normal racing, boosted racing, title menu and pause menu. Race/title outputs provide style references for boost/pause. All four original 2048 × 1152 PNGs, exact input images, submitted prompts, request bodies and slim job/hash provenance are preserved in [the reference pack](../references/interface-polish-scenario-v1/README.md). Raw private API responses are outside the repository.

Visually inspected all four pairs; verified image decoding and hashes; checked gallery screen navigation, current/concept mode and keyboard comparison slider in the browser with no console errors. The critique preserves known differences: resynthesized world details, omitted transient race feedback, restart shortcut approximation and instrument silhouette variation. These are concepts, not native changes or owner acceptance. No features, source assets, native build or game behavior changed. History is local; no public push.


## Item 1 ribbon exhaust prototype: owner rejected

Preserve the first exhaust implementation as rejected evidence. Candidate 04 (`c27b8dc16d804a13ac9d27f00432bdf8`) uses camera-facing soft ribbons and bounded world-space history. All 221 EditMode tests pass, and native acceleration/boost/release complete, but the owner finds the continuous glowing surface looks like a texture rather than flame or particles. This is a visual failure; tests do not close item 1. The first 1080p comparison also includes 82 paused baseline frames and fails strict frame matching. A repeat capture was stopped following the owner's rejection. Artifacts remain under `../artifacts/exhaust-item1/`. Next: replace continuous surfaces with independently emitted 3D particle plumes and inspect before accepting. No road, camera, HUD or city work is authorized by this step.


## Blue exhaust candidate — 2026-09-15

Implemented the resumed item 1 with animated blue particles, bounded world-space wake, near-camera/depth fades and subframe emission placement. Preserved paused blue treatment for same-binary comparison. Three Editor visual iterations preceded native capture. Native build and 225 Editor tests pass; matched 432-frame A/B records have identical vehicle/camera motion. Separate M4 Max 1080p sustained-boost samples show no observed regression (8.389 ms baseline, 8.362 ms candidate mean). See [review and build limitation](blue-exhaust-review.md). No later polish items or gameplay changes. Candidate is ready for owner review, not artistically accepted.


## Road light-response handoff prepared, 2026-09-15

Prepared the owner-requested [plan and handoff](plans/2026-09-15-road-light-response/HANDOFF.md) for a new GPT-5.6 Sol task. Verified baseline commit, working-tree boundaries, road material keywords and prior failed probe/SSR evidence. The new assignment is a single opening passage with visible native acceptance criteria, preserving the blue exhaust. No runtime changes or new task dispatch.

## Opening-road light-response candidate, 2026-09-15

Re-aimed and strengthened the existing overhead fixtures across the start-line wrap and opening bend, with alternating cyan/amber color and a broad outer penumbra. No lights, geometry, material textures, camera, HUD, exhaust or gameplay features were added. Ordinary launch enables the candidate; `-vrRoadLightBaseline` provides a road-only control in the same binary.

The retained third visual iteration completes an 18-second native 1080p A/B with 432 frames per variant and zero recorded vehicle/camera/input/FOV deltas. Layered blue exhaust is enabled in both. Unity 6000.6.0f1 EditMode tests pass 233/233 after generated duplicate package-cache entries were removed. Separate M4 Max real-time samples show baseline mean/p95 8.336/9.112 ms and candidate 8.361/9.068 ms. See [the review note](road-light-response-review.md). This is ready for owner review, not artistic acceptance or authorization to roll out beyond the passage.


## 完整赛车体验规划，2026-09-15

用户否定继续局部修补，补充了城市多样性/密度/活跃感、速度模糊、饱满尾焰、远景雾、霓虹、平顺赛道侧倾、护栏细节、船身状态变化、碰撞粒子与字体/UI 动效。编写[整体计划与交接](plans/2026-09-15-integrated-racing-experience/PLAN.md)，覆盖统一美术目标、资产取舍、约 30 秒完整原生示范段、整体用户评审及后续全赛道/全部既有界面扩展。核对到道路提交 `7358f77`，保留其实现；旧道路计划标记为历史。源码确认已有倾斜、碰撞粒子、Bloom、部分 UI/声音反馈，避免误当缺失系统。仅修改规划文档，没有实施、运行测试、创建任务或派发代理。

## 四份模型分工交接，2026-09-15

用户要求几个可在新任务执行的 handoff，并指定全部资产和 Blender 由 Astra 制作。新增[启动说明与四份交接](plans/2026-09-15-integrated-racing-experience/START_HERE.md)：01 / Sol 场景统筹与集成、02 / Astra 全部美术资产、03 / Sol 速度/飞船/碰撞特效代码、04 / Sol UI/声音实现。明确独立工作区、共同基线、唯一文件归属、共享契约与分批原生集成；02 首批同时供应城市、VFX 和字体/UI 样件，避免技术任务等待完整资产包。

更新 AGENTS、总计划和原单任务入口，覆盖旧的全部 Sol 分工。保留约 30 秒完整示范段、用户整体评审与后续扩展顺序。本步只提交文档；校验链接、编码、职责一致性与差异空白，没有运行游戏测试、创建任务或改动运行时。已有 Unity 资产元数据、Packages、设置与独立评审文档的未提交修改均保留。

## English handoff packet — 2026-09-15

At the user's request, added [English versions](plans/2026-09-15-integrated-racing-experience/START_HERE.en.md) of all four handoffs, the startup guide, execution index, overall plan, and acceptance checklist. Preserved the Chinese originals, model assignments, file ownership, shared coordination paths, staged integration, and user review gate. Checked English packet links, reference paths, encoding, and scope consistency. Documentation only; no game implementation, runtime tests, task creation, or unrelated working-tree changes.
# 2026-09-15 — Integrated racing experience workspace preparation

- Preserved the original `opening-city` checkout on `art/opening-city` at `dfc2b1d`; no original files were committed, discarded, or rewritten.
- Copied the current source state, including pre-existing FBX metadata, package, ProjectSettings, Art metadata, and Stage 2 review differences, into `experience-workspaces/integration` while excluding Unity caches, logs, user settings, and native builds.
- Updated the workspace runner so all four experience workspaces use one lease under the shared project root. Native compile, route export, and contributor workspace checks follow this baseline commit.
- Committed baseline `d586986141af487af578f7b20544e579c4ecd635` and cloned clean `experience/art-assets`, `experience/motion-vfx`, and `experience/ui-audio` branches from it.
- Unity `6000.6.0f1` imported and compiled the integration project successfully in batch mode. `ProductionSceneSetup.ExportContext` exported 1,201 frames for the 1,844.5176 m by 22 m course, hash `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`.
- Published `WORKSPACES_READY`, contract version 1, source/reference paths, contributor ownership, event/preference/render integration rules, and the next integration order in the shared coordination directory.

# 2026-09-15: integrated racing experience A checkpoint

Integrated the motion/VFX and UI/audio deliveries plus Astra asset batches A through D without changing the ordinary launch scene. The integration-owned scene path is `Assets/Scenes/NocturneExperience.unity`; its current validated art revision is `vrx-a01`. It now retains persistent URP renderer, pipeline, volume, motion-blur, depth, motion-vector, lighting, and dark non-emissive road assets. `VehicleContactAudioBridge` maps the authoritative VFX contact stream into the UI/audio lane and clears contact state on restart, title return, disable, and teardown.

Batch B cannot replace the A city. Unity's baked-axis policy mirrors `vrx_city_exchange_b` on Z, while the alternate policy then mirrors `vrx_city_services_b` on X. Neither policy satisfies all sealed bounds. The strict check remains unchanged and no prefab mirror was introduced. Failure evidence is preserved in `artifacts/integrated-racing-experience/b01-scene-prepare-01-unity.log` and `b01-scene-prepare-02-unity.log`; the required next input is corrected Astra FBX export orientation. Batch C ship maps are present but its boost-state shader properties are not consumed by the current URP Lit material, so appearance binding remains with lane 03. The UI/audio implementation passes source checks but remains visually unaccepted.

The first native A build exposed a white emissive road. R2 disables that emission and persists a dark road material. Fresh 1920 by 1080 title/start captures confirm the white-road defect is removed, but direct inspection rejects artistic promotion because the road is nearly black and large portions of the lap contain sparse prototype city coverage. The full route sheet is `artifacts/integrated-racing-experience/a01-native-preview-02/route-contact-sheet.jpg`.

The first audiovisual preview exposed a separate evidence defect: a route-anchor request could collide with the 10 Hz sequence request and drop one PNG. The harness now reuses the exact sequence frame for that anchor. The replacement app `Vector Rush Experience A01 integration-03.app`, GUID `c332adf9551842c698d1afaa4c56d5cb`, completes a 42.816-second real-time lap with 402/402 frames, all 12 anchors, matching 42.816-second stereo audio, peak 0.1433, and RMS 0.0318. The encoded preview is `artifacts/integrated-racing-experience/a01-native-preview-02/full-lap.mp4`.

The complete EditMode suite passes 246/246. A separate real-time performance race completes in 129.17 seconds with all six racers, zero recoveries, and 8.49/9.29/16.68 ms mean/P95/P99 delivered frame intervals; no interval exceeds 33.3 ms. The three-race lifecycle run remains failed: races 1 and 2 complete with all six finishers, zero recoveries, frozen result state, pause freeze, and clean restart, but race 3 does not reach `Finished` within the 220-second deadline. This is recorded as a technical blocker, not normalized away. No human handling, physical controller, artistic, or ordinary-launch acceptance is claimed.

## 2026-09-15 — Stage 1 source import repair (Astra)

Preserved integration baseline `e214ad4` and original A01 apps. On branch `stage1/astra-reference-city`, corrected Batch B handedness in Blender source and re-exported all 30 FBX LOD models to the new immutable `SourceAssets/Stage1/city-s1` package. Declared Unity bounds, material slot names, track hash and original package remain unchanged. The original strict Unity importer passes all 30 bounds/material checks; evidence: `artifacts/stage1-reference-match/prepare-01/import.json`. This is an import repair, not artistic acceptance. Native checkpoint01 was rejected after revealing separate shared-material lifetime and emissive-flag defects in the new scene authoring code; retained locally for diagnosis. Visual authoring and native iteration continue under the authorized Stage1 scope.

## 2026-09-15 — Stage 1 native city candidate (Astra)

Authored a separate persistent `Stage1City` scene with 140 building footprints, three skyline depth layers, lower city roofs and corrected imported plant, stepped/twin massing, framed NOVA service facade, local bank-following rail geometry and actual-turn chevrons. Added dark road material, bounded cyan/magenta response, atmospheric depth, shipped surface-map binding, masked boost accents and fuller blue particle appearance. Track, handling, original chase-camera code, hero geometry and ordinary HUD remain preserved. All art and programming were performed under the owner's current all-Astra instruction.

Native checkpoints were visually inspected and iterated. Checkpoint01 was rejected for missing road material references and disabled emission; persistent-asset reuse and GI flags fixed both. Subsequent passes reduced the coarse road pattern, added distant skyline/lower city, varied massing and improved directional/foreground structure. Candidate build `7cba6941ae2a469b83a9c807ba5ac957` now shows a meaningful native change, while facade/material richness and full dynamic reflection/motion parity remain below the generated targets.

All 248 EditMode tests pass, including actual ship-map/emission binding and material persistence checks. Three selected before/after stills have exact camera position/rotation/FOV and racer-position parity versus a fresh A01 capture. The complete 432-frame data has identical position/FOV/path/speed/racer positions; non-anchor camera rotation differs by at most 0.3357 degrees. Both 1080p clips fully decode: 8.54-second HUD-visible excerpt plus complete 18-second simulation-time sequence. Separate real-time opening measurements: baseline 8.341 ms mean / 9.118 ms P95, candidate 8.353 ms mean / 9.137 ms P95 on Apple M4 Max at 1080p, under the existing 120 fps cap. Full-race check and delivery packaging follow as a separate validation record. Native artistic acceptance remains the owner's decision; Stage 2 has not started.

## 2026-09-15 — Stage 1 review package and complete race verification

The candidate completes one real-time native race with all six racers finishing, zero recoveries and a valid frozen result/post-finish pause. Player time is 129.166 seconds. Delivered frame intervals during the measured race are 8.382 ms mean, 9.264 ms P95 and 9.354 ms P99, with no intervals above 33 ms. This is automatic ordinary physics, not human handling or hardware-controller evidence. The prior three-race issue was not retested.

The review package lives outside git at the shared project `artifacts/stage1-reference-match/delivery/`: the two selected Scenario targets, original native stills, exact-anchor before/after comparisons, HUD-visible 8.54-second clip, full 18-second simulation-time sequence, checked primary records, checksums and an interactive HTML/README. Actual native images and decoded sequence samples were inspected. Report distinguishes the stylized foreground/material gap, authored road-response approximation, HUD contrast gap and bounded opening scope. Builds, captures, temporary images and licensing logs remain outside source commits. No external push or later-stage implementation occurred.


### Opening-city architectural rebuild — 2026-09-16

Owner approved replacing the prototype building treatment while preserving the improved track. Rebuilt three façade systems plus service floors, authored grouped architectural surface/normal/roughness maps, added continuous glass spines, foundations/street connections and a varied exit skyline. Native06 exposed floating lower blocks and road darkening from global probes; native07 moved city reflection to building materials and grounded the district. Native08 added quieter glazing/left skyline; native09 removed reflection feedback and faded far-field contribution. Rejected checkpoints remain local.

Candidate Architecture09, GUID c05f767bfbc74d59ac0284a8c417c602: 248/248 Editor tests, 432 verified motion frames, exact three-anchor camera parity, mean8.3449ms/P95 9.1742ms opening frame delivery under120fps cap, and one complete six-finisher race with zero recoveries. Normal title launch was visually inspected. Source game logic and banked route remain unchanged. The review explicitly identifies stylized/repetitive surfaces, static reflection limits, human-play gap and blocked automatic browser preview. Source milestone is local; builds, captures and licensing logs stay outside Git. [Evidence and review](city-architecture-review.md).


## 2026-09-16 — Existing city lighting and signage

Owner rejected adding pedestrians/trains and authorized working with existing elements. Reworked occupancy atlas, existing advertisement surfaces/placement, facade proportions and local lighting/reflections. Native City Light03 milestone, 248/248 tests, all 432 frames verified, exact three anchor poses and separate before/after capped frame checks. [Full record](city-light-review.md). Visual progress is recorded without claiming reference parity or artistic acceptance.

## 2026-09-16 — Neon lighting candidate and roadside glow correction

Continued the interrupted neon lighting pass; retained localized spill/HDR grading and reflection persistence repair, optimized reflection sampling, and restored textured roadside wash. Neon09 native build `9f6b3726179b48f7b51eec19aa9d9b2e`; 249/249 tests, exact three-anchor camera/racer parity, 432 verified motion frames, mean 8.353 ms / P95 9.146 ms at 1080p on M4 Max under the 120 fps cap. [Delivery and explicit limits](neon-lighting-review.md). Owner art review remains open; no camera/blur or circuit expansion.

## 2026-09-16 — Motion03 camera and speed presentation

Implemented the newly authorized motion stage with scene-scoped chase tuning, speed blur and hold-to-look-back. Rejected the first overhead transition and replaced it with a side arc. Delivered build b3ff6a21b92d4d95b1a61a446278d66e, 251 passing tests, 432 native frames per same-build variant with unchanged racer physics, verified reduced-motion/transition blur suppression and separate performance results. Mean 8.419 ms, P95 9.165 ms, P99 15.873 ms on M4 Max/1080p; tail regression is explicit. [Review and limitations](motion-camera-review.md). Earlier native apps preserved.

## 2026-09-16 — Complete-circuit lighting rollout

Extended the accepted first 310-span test-field infrastructure system through all 926 circuit spans while preserving its start, spacing, construction values, materials, lights and ordering. The continuation begins at normalized progress 0.3228 and reaches 0.98808. Reused the three accepted city reflection captures, preserved the earlier road material and made no track, handling, camera, ship, HUD or city-architecture change.

Build `Vector Rush Circuit Lighting 01.app`, GUID `655f9a6ebeed4bbeb6b7678b957c6dca`, completes a 401-frame full-lap preview across 12 route anchors and a separate complete six-finisher performance race with zero recoveries. The three protected-field anchors have exact racer and camera pose parity with Motion03. Current EditMode result is 252/252. The isolated race measured 8.349 ms mean, 9.151 ms P95, 9.310 ms P99 and 17.514 ms maximum at 1920 by 1080, with zero frames above 33.3 ms. [Native review and limits](circuit-lighting-review.md).

## 2026-09-16 — Lighting-field/original-map hybrid

Created a separate persistent hybrid scene and native Hybrid Route 02 app, retaining both existing apps. Imported the original canyon/gallery, thermal, station/civic and distant-city districts while keeping the lighting field and one shared road course. Trimmed overlapping source geometry only in the hybrid copy; a progress-gated brief fade hides district activation beyond 0.3228 and before the lap wrap. Protected Stage1City source remains SHA-256 `a05eafe1ffa7864ee580f98a149110dfc749efbb6c4acf4d849e6276297373ca`.

Verification: 253 editor tests; three consecutive native six-finisher races, pause/results/restart checks and zero recoveries; isolated 1920 × 1080 full-race performance at mean 8.35 ms / P99 9.30 ms. Two frame hitches up to 183.89 ms remain. Build GUID `37d40c498a7c49d1b51678d91eda0f4f`. The prior repeat-race timeout did not recur after adding opt-in telemetry; its cause remains unresolved and no gameplay fix is claimed. Matched opening stills and the source identity establish the preserved field; a previous same-scene binary supplies the timestamped native handoff clip. Human feel and artistic acceptance remain open. See [hybrid review](hybrid-route-review.md).

Source milestone is local: origin is another local repository. Native apps, captures, logs, existing .gitignore changes and unrelated ProjectSettings changes are excluded.

## 2026-09-16 — Environment structural Stage 1

Following owner approval to implement the screenshot-led environment plan in stages, authored a separate `EnvironmentStructureStage1` scene. Disabled only its copied thin-post material batch; added a continuous deep deck, 48 vertical pier/crosshead/bearing assemblies and bounded service podiums with an exposed bridge opening. Baseline scene and shared assets remain intact. No racing surface, course, collision, handling, camera, ship or HUD changes. Three static mesh batches, three shared materials and zero additional lights/colliders.

Verification: 255/255 editor tests; unchanged course hash and serialized racing colliders; 2,400 route samples across five lateral positions for geometric road-clearance checks; native full-lap capture with 12 anchors and 400 timestamped frames. Corrected coarse deck clearance at a bank transition and flat-face shading after inspecting the initial native build. Final build GUID `11b17a9bc497459a9844f05b03e7b6c7`. Baseline scene SHA-256 remains `89477673bd11a6b0165144861003a68f29816473257c1faf9438eb13ac6a53ed`.

This is a structural blockout milestone, not finished art or human play acceptance. Open galleries, blackouts, repetitive signs/roofs, blank masses and the apparently floating tower remain explicit later-stage work. [Native before/after and checks](../../../../../Vector%20Rush%20Review%202/environment-stage1/REVIEW.md). Builds, raw captures and logs remain outside source history; the superseded candidate and its raw recording were moved to a recoverable Trash folder.

Separate final-build uncaptured 1080p race: six finishers, zero recoveries, stable finish/pause state; player second in 129.166 s. Frame delivery mean 8.357 ms, P95 9.063 ms, P99 9.293 ms, max 17.949 ms, zero frames over 33.3 ms. This single sample is not a controlled A/B improvement claim, and one race does not validate repeated restarts.
## 2026-09-17 — Selected HUD A implemented

Reconstructed the owner's selected generated HUD with live data and cached code-authored art in the existing runtime UI. Preserved environment, race rules, menus and settings; retained opt-in legacy overlay. Native comparison drove a corrective panel/typography iteration. Enabled the existing HUD evidence runner for production scenes only when explicitly launched with its diagnostic flag.

Final GUID `11d4bbd805824b568b40da78952fc63c`; 263 editor tests pass, 21 real-state screenshots across three aspect ratios, completed three-lap race in 129.166 s and zero player recoveries. A visually identical preceding build supplies the 42.90 s full-lap recording. No performance or pixel-identical-match claim. [Review, provenance and limitations](hud-a-review.md). Builds/captures remain outside source history; unrelated dirty files and executable-mode changes are preserved.
## 2026-09-17 — HUD completed with Saira and boost response

Integrated real Saira Black/Medium Italic static instances with OFL/provenance; removed synthetic bold/italic/stretch from arcade labels. Added smoothed, speed-dependent vibration and directional cyan afterglow only to the speed/energy panel. Reduced Interface Motion and non-racing phases suppress immediately. Extended opt-in native HUD evidence with an actual boosted 60-simulation-fps sequence and temporary in-memory accessibility switch, restored afterward.

Build `a42a55aa30f74b808167201dc3136303`: 267 tests pass; 21 state/aspect views; 120 motion frames with 30 zero-motion accessibility samples, 24 still boosting; three laps in 129.166 s and zero player recoveries. Source environment and course unchanged, earlier apps preserved. No human handling/performance or exact-reference claim. [Completion review](hud-a-review.md).

## 2026-09-17 — GitHub relay 01, clean-checkout verification

Received `game/current` at `a7b8d70`, verified all 2,757 payload hashes, completed a fresh Unity import and 271/271 current tests, and built native GUID `98d13df67244414c923b8e1b64f3c643`. Complete automated lap: 43.2 s / 405 frames with game audio; recorded camera-clearance check passed. Separate three-lap race: six finishers, zero recoveries, 130.015 s player time; mean 8.334 ms, P99 9.312 ms, maximum 16.676 ms. No game code/assets changed. Updated active handoff to distinguish resumed work from the old publication pause and identify the next visual step. Repeated-race/manual/artistic review remains pending. [Results and evidence](current-game/relay-2026-09-17/README.md).


## 2026-09-17: underground terrace finish candidate

Completed the owner-approved local environment pass from `e3d3ff6`: additive planted terraces, podium fronts, wall recesses, material relief and curved tunnel services. The candidate retains the completed HUD/baseline, five driving meshes, ship and course. Current suite: 273 passed; native build and 405-frame lap/camera check passed; three consecutive races each had six finishers and zero recoveries with pause/result/restart invariants intact. Separate uncaptured run: mean 8.334 ms, P99 9.318 ms, zero frames above 33.3 ms. This is similar local frame delivery to the prior build, not a GPU cost claim.

See [milestone and matched native comparisons](current-game/underground-finish-2026-09-17/README.md). Continuous clip and playable app are in the local output folder. Foliage/facade/concrete quality still trails the reference; owner artistic acceptance and manual/controller testing remain open. Whole-circuit rollout was not performed. Source/document whitespace checks pass; Unity-generated empty YAML fields retain the Editor's trailing spaces.

## September 17, 2026 — rain shelter and atmosphere correction

Owner approved dry covered road, no tunnel rain/spray, rain sound fading with depth and stronger outdoor haze. Added a collider-derived shelter profile and spatial road material palette in `RainAtmosphereStage5.unity`; the current-game wrapper selects it. Original vertex positions/collision meshes, course, ship, HUD and handling remain preserved. Retained prior scenes and the merged Apex icon.

282 tests passed. A 43.029-second native lap provides 1,009 timestamped frames, captured audio and shelter telemetry; inspected before/after views visibly remove wet tunnel reflections. The isolated 1920×1080 three-lap race passed with six finishers, zero recoveries, frozen results, pause and restart checks. Frame delivery: 8.33479 ms mean, 8.33334 ms P95/P99, 17.20417 ms maximum, zero frames above 33.3 ms. See [the atmosphere milestone](current-game/atmosphere-2026-09-17/README.md) for exact evidence and local review/build paths. This remains an owner-review candidate, with wider art and manual/controller acceptance open.
