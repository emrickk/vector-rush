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
