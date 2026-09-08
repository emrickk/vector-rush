# Development history

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
