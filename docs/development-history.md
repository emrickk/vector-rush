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

The companion visual task requested stronger modeling and visual focus. New isolated hero/environment packages are being authored there while this task retains Unity integration. Numerical analysis found bank discontinuities and wide twisting quads; continuous horizontal-curvature banking and lateral tessellation were implemented. Run04 completed3laps in112.64 seconds with zero player recoveries, both results/paused restart launches entered Racing with movement and reset state, and countdown-pause checks passed. Road shading still failed visually, so matched material/shadow/normal diagnostics follow.

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
