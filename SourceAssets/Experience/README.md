# Meridian Afterhours / Astra asset delivery

The art package is a candidate for the agreed racing exemplar. Native
integration, moving-scene usability and user artistic acceptance remain separate.
The authoritative staged status and commit IDs live in the shared `02-art.md`.

## Delivered structure

- `packages/vrx-a01`: first environment/effect/font/UI package and recovery point.
- `packages/vrx-b01`: cumulative environment kit and exemplar layout, replacing A
  for city integration. Preserve the A commit; do not place both layouts.
- `packages/vrx-c01`: retained-UV ship coating, boost-region maps, three-dimensional
  exhaust shell and .blend source. Supplemental manifest, not an environment package.
- `packages/vrx-d01`: HUD/menu tokens and event storyboards, with existing-audio
  timbre instructions. Supplemental manifest, not an environment package.

Only folders with READY.json are published. Checksums cover every package file.
The latest runtime bytes live at `UnityProject/Assets/Resources/ExperienceArt`
with stable metadata. Source scripts, reference observations and previews stay
outside Unity. Generated materials/prefabs and gameplay code belong to Sol.

Read `design/ART-DIRECTION.md` for asset decisions and
`design/INTEGRATION.md` for binding, formats, materials, timing and typography.
The most useful previews are `evidence/style-board.png`,
`vrx-b01-{entry,turn,exit}-composition.png`, `vrx-c01-ship-{cruise,boost}.png`,
and `vrx-d01-{hud,menu,motion-storyboard}.png`.

## Reproduction

Use the recorded milestone's scripts with Blender 5.2.1 LTS, Python 3, Pillow,
numpy and fontTools. Use the shared `tools/opening-city-run.py` lease for Blender
and Unity jobs. Do not copy Unity caches or run heavy jobs without that lease.

In a clean copy of that milestone, remove only generated outputs of the chosen
revision, then run from the repository root:

1. `python3 SourceAssets/Experience/scripts/package_art.py prepare b01`
2. Blender background with `scripts/build_city.py -- b01` under the lease.
3. Blender background with `scripts/preview_route.py -- b01` under the lease.
4. `package_art.py publish b01`, then `check_package.py` on the package directory.
5. `ship_finish.py`, then Blender `ship_blender.py`, then `publish_extra.py c01`.
6. `ui_storyboards.py`, then `publish_extra.py d01`.

Package recipe snapshots identify source content. Run them at their original
`SourceAssets/Experience/scripts` location, or use the milestone's tracked
scripts. Scripts deliberately refuse to mutate a published READY revision.
Geometry, textures and placement seeds are reproducible; binary Blender/FBX
container metadata need not have identical timestamp bytes across reruns.

## Budgets and provenance

Design budgets, pending native measurements: keep the unique city kit below
25,000 LOD0 triangles; retain skyline LODs rather than culling them early.
Aim below 180,000 placed city triangles when every exemplar instance uses LOD0.
Limit the fully resident uncompressed source texture set, including mips, to
160 MiB. Actual memory, overdraw, draw calls, CPU/GPU time and frame pacing must
be measured by 01 in the integrated player. Triangle counts are not a performance pass.

Models, surface textures, VFX, graphics and composition are original scripted
project assets. Ship coatings derive from existing project Kestrel07 maps and
preserve their UVs; source hashes are recorded. The original ship FBX is retained.
Rajdhani SemiBold and Barlow Medium are retained fonts with embedded provenance,
OFL 1.1 license files and checked ASCII HUD glyphs. CJK coverage is not supplied.

The supplied video and collision screenshot are observation references only.
The reference contact sheet stays in evidence and is never a runtime texture.
No assets were purchased and no new external audio sources were introduced.

## Validation boundary

Source checks cover package integrity, texture decoding, IDs, LOD reduction,
mesh degeneracy, closed building components, material slots, bounds and font
glyphs. Placement tests use the actual course frames and conservative boxes.
Roof plant intentionally attaches to the authored service roof.

Unity 6000.6.0f1 completed imports for all 31 FBXs, 86 PNG textures and two fonts,
plus the accompanying data/folders, in the isolated art workspace. All payload
bytes and metadata remained unchanged. No C# or asset-import failures were logged.
Two separate DX11/FXC compiler failures occurred in the bundled
Hidden/ProbeVolume/VoxelizeScene shader; Unity restarted the compiler and exited
successfully. This does not establish a clean rendering pass. The issue is
recorded for 01 in `evidence/unity-import-summary.json`.

Blender camera previews use the settled baseline chase-camera formula. They
omit dynamic smoothing, actual URP rendering, gameplay motion and audio.
The ordinary game scene was not edited by 02. 01 must validate actual moving
occlusion, retained-city overlap, support contacts, LOD transitions, atlas
orientation/opacity, boost readability and typography at native screen sizes.
