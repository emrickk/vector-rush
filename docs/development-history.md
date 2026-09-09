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
