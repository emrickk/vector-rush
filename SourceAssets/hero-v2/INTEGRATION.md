# Kestrel 07 — sculpted V2 (staged visual revision)

This package changes the actual game mesh. The source project and Unity files have not been edited.

- `Kestrel07-v2.blend`: editable named objects and studio inspection setup.
- `HeroShip.fbx`: runtime export, nine meshes batched by the existing nine material names.
- `build_hero_v2.py`: reproducible Blender authoring and CPU Cycles inspection renders.
- `render_inspection.py`: render-only inspection of the frozen blend; leaves the FBX untouched.
- `frozen-export.json`: runtime handoff SHA and geometry count.
- `engine-anchors.json`: local source and Unity positions of each nozzle exit/throat, plus effect directions.
- `asset-stats.json`: geometry and dimensions.

## Design changes

The hull has fuller curved shoulder volumes, a raised teardrop cockpit with rollover frame, and ceramic shells separated from the carbon structural chassis. Seven-vane intake channels are recessed between forward and aft armor. The outer floating cheek panels expose a dark seam and frame the suspension links and propulsion feed lines. The aft stabilizers have taller swept silhouettes. Race numbers appear both forward and aft, with physical yellow-green sashes that remain visible after material remapping.

Each main engine has an open tapered cavity, a titanium rolled lip, intermediate luminous annulus and an emissive throat 0.685 m behind the exit. It does not use a fan cap. Internal longitudinal staves indicate depth without covering the visible core.

## Integration

Replace the existing `HeroShip.fbx` while preserving its Unity `.meta`. Existing material names remain Ivory, Graphite, Glass, Signal, Engine, Metal, Ceramic, Ink, WhiteMark. Geometry uses no required textures. Preserve the Engine emissive shader variant and Glass reflections when remapping; otherwise the runtime will flatten a substantial part of the intended appearance.

Source coordinates remain nose -Y, up +Z. FBX uses the existing export axis policy. Unity mapping is (source X, source Z, -source Y). Main effect exits now sit at Unity (±1.68, -0.035, -3.405), facing local -Z. Full anchor details are supplied in JSON; no extra empty transforms are included in the nine-mesh export. Existing VFX at ±1.9 will be visibly misaligned.

The export contains 73,364 triangles. Use it as the player hero asset; derive an LOD or simplified opponent mesh if needed for a full field. This package does not claim native frame rate or in-game visual validation. Inspect the model under actual gameplay sun, reflection and emission settings before approving it.

## Visual acceptance

Inspect the supplied front, rear, top and chase renders, then verify the silhouette, intact panel layering and luminous nozzle depth from the real gameplay camera. Studio lighting demonstrates geometry and material separation but is not evidence of native game appearance. The source procedural construction retains some simplified mechanical joins; it is a meaningful form revision, not a final AAA character-quality production asset.
