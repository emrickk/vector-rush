# Integration contract / Meridian Afterhours

Status: assets ready for integration once the named package has READY.json.
Native usability is pending. All design values below belong to Astra.

## City and track / 01

Use ProductionArtManifest v1 and the package course hash. Validate checksums
before import. The immutable source package includes FBX LOD0/LOD1, .blend,
textures, material records, layout and conservative proxy boxes.

The direct ExperienceArt payload mirrors the latest delivered revision at stable
paths and GUIDs. Choose either that payload or the package importer as the runtime
source; do not load duplicate copies. Generated materials/prefabs belong outside
ExperienceArt. Do not copy the whole source package into a visible Unity folder.

Use metre units, ground-centre pivots and unit scales. Match material slots in
manifest order. UV0 is in metres; set texture scale to (0.25, 0.25). For source
Blender coordinates (X,Y,Z), Unity coordinates are (X,Z,-Y). Preserve source
normals; calculate Mikk tangents if import does not retain them. Generate UV1
only for a selected baked lighting path. Building colliders are none.

The layout contains exact Unity world transforms, stable instance IDs, sampled
progress, lateral/vertical offsets, frameMode and near/middle/far roles. Re-run
clearance against production road branches, camera paths and retained older
city pieces. The package's placement check does not inspect the production scene.
Place guidance at its short banked segments. Do not stretch it into straight
long rails across turns. Ground/proxy foundations must meet a visible support
surface in the assembled city; request adapters if current terrain leaves gaps.

Per asset LOD0 is deliberately modest. Try towers at .20/.035 screen-relative
height, close modules at .35/.06; preserve far skyline silhouettes. Target
crossfade <= .15. Do not use the old importer's .12 last-LOD cull as an art
approval. Instancing/static batching and material sharing are 01's implementation.

`lighting.json` exists to satisfy the schema, and records artistic intent only.
It is not permission to overwrite the production Volume, sky or light setup.
Transparent glazing, realtime light-per-window and per-building shadow lights
are unnecessary. Keep all glazing opaque and combine facade regions by material.

## Propulsion and contact / 03

Atlases are uncompressed RGBA8, linear, straight alpha, bilinear, clamp, no
mipmaps. RGB is linear emitted colour, A coverage; intensity is applied once.
Use additive or premultiplied additive output with A applied exactly once.
Do not treat the RGB image as a packed mask or colour it orange.

All flame grids are 4x4 / 16 frames / 24 fps. Frame 0 is top-left in the PNG,
sequence left-to-right then downward. Convert row to bottom-left UV coordinates:
rowUV = rows - 1 - floor(frame / columns). There is a 4 px transparent gutter.
U advances from nozzle to tail; V spans width. Flame phase is a periodic
advected field. Interpolate adjacent frames, including last to first. Phase
offset between engines is permitted, randomized unrelated frames are not.

Use three coordinated layers at each real engine exit:

| Layer | Cruise size relative to nozzle radius r | Boost size | Role |
| --- | --- | --- | --- |
| Core | length 2.5r, full width 1.2r | length 3.0r, width 1.4r | Attached, bright, short white-blue mass |
| Main body | length 9r, full width 2.1r | length 12r, width 2.6r | Full blue turbulent jet; carries most silhouette |
| Wake | length 14r, full width 3.2r | length 19r, width 4r | Low-opacity breakup, follows world velocity |

Lengths are starting art proportions, not acceptance measurements. Use existing
engine anchors and actual visual-root scale. Never put a billboard at the ship
centre. Fade body opacity over the last 20% of length. Layer meshes/particles
in depth; avoid one flat card or crossed-sheet silhouette. Release cuts fresh
core emission promptly while already emitted wake dissipates in world space.

Impact atlas is 4x2 / 8 frames, a one-shot 24 fps expansion and fade. Spark atlas
is 4x2 / 8 variants: choose a fixed tile per particle, stretch along velocity.
Do not animate unrelated spark variants. Keep long sparks thin and contact-local.

| Event from real state | Visual onset / peak / decay | UI / audio alignment |
| --- | --- | --- |
| Boost enter | 0-80 ms seam/core rise; 80-160 ms body expansion; settle by 240 ms | Label and engine rise on the same state transition |
| Boost sustain | Stable hull and body, slow turbulent modulation | No repeated onset sound or label flashing |
| Boost release | Core 90 ms; body 160 ms; wake 280 ms | Label fades 180 ms; wind/engine settle 220 ms |
| Light graze | 35 ms local flash; 80-160 ms fine sparks | Soft high scrape, no large centred warning |
| Sustained scrape | Bounded thin streaks while contact stays; emission stops on exit | Continuous filtered friction, no per-frame one-shots |
| Heavy impact | 25 ms flash; sparks peak <=90 ms; fragments fade <=320 ms | Single low transient and 300 ms edge warning |
| Rank change | Display change immediately; 110 ms emphasis; 450 ms hold; 170 ms exit | One quiet tick; coalesce multiple changes within 180 ms |

These durations specify presentation envelopes, never collision classification
or physics. Shared contacts come from 03/01. Pause freezes gameplay-time VFX;
restart/recovery/disable clears all transient visuals and loops.

## Typography and UI / 04

Display: Rajdhani SemiBold, weight 600; labels/body: Barlow Medium, weight 500.
Both retained from repository with OFL 1.1 text and font name-table provenance.
ASCII HUD/menu glyphs were verified in their cmap. Neither supplies a CJK
fallback. Preserve the existing fallback for any other supported script.

Reference 1920x1080: safe area 48 px horizontal/36 px vertical; speed 84 px,
rank 70 px, lap/time 30 px, tiny labels 20 px, main menu rows 28 px, title 64 px.
Keep labels upright. Display tracking +1 px; uppercase short event labels +2 px.
Avoid artificially skewing every glyph. Widths must accommodate three speed
digits, two rank digits and actual current time formatting.

HUD anchors: rank top-left, race/time top-right, map bottom-left, speed and
boost bottom-right. Keep the centre track sightline free. `vrx_ui_panel_a` is
optional backing only where contrast requires it. `vrx_ui_selection_a` carries
keyboard/controller menu focus, not a new button or menu hierarchy.

Boost track and fill are matching 16-slash strips. Fill is clipped by authoritative
Boost01, with no fake extra segments. Collision icon is contact feedback only,
not shield or damage capacity. Rank up/down arrow accompanies the actual number
change. Wordmark and advertisement graphics contain no gameplay values.

ReducedInterfaceMotion: immediate value changes; <=100 ms opacity transition,
zero positional travel, zero scale pulse. Low-motion state remains clear.
HudScale applies uniformly within clamped safe areas; reflow narrower aspect
ratios rather than clipping map, speed or menu text.

## Audio timbre / 04

Reuse existing engine, wind, boost, collision and menu synthesis. No new music
system or source audio is required. This is a timbre brief for runtime synthesis
and mixing code, owned by 04.

Engine: stable low-mid motor body with a narrow higher whine following speed;
avoid a broadband constant hiss masking contact. Boost: a short bright onset,
then a fuller low-mid motor and restrained airy tail. Wind increases smoothly
with speed; lower it briefly under rank and contact events. Light graze is a
fine metallic scrape; heavy impact adds one short low body transient. Sustained
scrape roughness follows tangential contact without repeated explosive hits.
Rank/menu ticks are dry, short and quieter than collisions.

Envelope onsets share the event table. No source-level audio or listening pass
is claimed by 02. 04 validates output, clipping/headroom, pause/restart behaviour,
and the final listening experience with the integrated capture.
