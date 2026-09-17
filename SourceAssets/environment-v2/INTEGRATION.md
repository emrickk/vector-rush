# Solstice coastal environment kit — staged assets

These are original modeled assets for Vector Rush, created from the Solstice reference and the actual run-03 visual failures. This folder is an independent staging area. It does not change the Unity project or certify the game's rendered visual quality.

## The intended visible improvement

- **TerraceTower A:** rounded, tapering ivory structure; deep recessed curtain glazing; three genuinely open garden floors; occupied podium, entrance, balcony balustrades and screened roof equipment. Its long vertical silhouette is interrupted by inhabited terraces rather than the old unbroken slab.
- **SplitTower B:** two unequal-height inhabited blades, a full-height open slot and three occupied connecting bridges. Ceramic end walls and glazed long sides produce a different value pattern and skyline silhouette from A.
- **CoastalCliff C:** asymmetric headlands, broken vertical erosion channels, shifting fine bedding planes, an irregular upland saddle and a broad lower wave-cut shelf. Fallen shoreline slabs continue the same geology. This replaces the low-resolution radial wedges in the current environment.

## Import and materials

Each FBX contains one mesh renderer. Geometry uses metres and the asset origin is centered at its base; the cliff's submerged skirt extends below zero. The exported scene basis is Y-up using Blender's normal Unity FBX conversion. Model front is the entrance facade; consult `asset_stats.json` for exact dimensions and export settings. Do not rotate the complete asset 90 degrees again after the importer has applied its conversion.

The strict shared material names are `Ivory`, `Graphite`, `Glass`, `Metal`, `Rock` and `Vegetation`. Remap names once on the imported shared materials:

| Slot | Runtime mapping | Appearance |
| --- | --- | --- |
| Ivory | `world.Ivory` | Warm ceramic, broad highlights; do not make emissive |
| Graphite | `world.Graphite` | Dark structural recesses, modest reflections |
| Glass | `world.Glass` | Opaque smoked teal glazing; no transparency sorting |
| Metal | `world.Metal` | Restrained gray metal on handrails, mullions and equipment |
| Rock | dedicated shared URP Lit material retaining supplied textures | Warm gray stone; use the texture contract below instead of a flat-color replacement |
| Vegetation | shared URP Lit material | Muted olive scrub, roughness about 0.9 |

The cliff's selected surface uses **Rock 3 by Rob Tuytel / Poly Haven (CC0)**. Preserve `textures/Rock3_PROVENANCE.md` and `Rock3_download-record.json`; these are third-party scanned textures, not agent-authored imagery. The original maps were copied without alterations. **Do not replace Rock with the old flat Basalt material.**

- `Rock3_CC0_Albedo.jpg`: sRGB, Repeat; assign `_BaseMap`, `_BaseColor=white`.
- `Rock3_CC0_NormalGL.png`: import as Normal Map, Repeat; `_BumpMap`, normal strength **0.75**.
- `Rock3_CC0_Roughness.jpg`: non-color reference map. URP Lit does not directly consume roughness.
- `Rock3_DERIVED_MetallicSmoothness.png`: non-color packed derivative, RGB=0 and alpha=1−source roughness. Assign `_MetallicGlossMap`, enable `_METALLICSPECGLOSSMAP`, use `_Smoothness=1` and metallic-alpha smoothness source. This mask is an explicit derivative; source maps remain unchanged.

All selected maps are 2048×2048. Use **texture scale (4,4)** on all three runtime inputs. The cliff has a single metric UV0 channel, with 10 metres per UV unit, so this yields **2.5-metre tiles**. The source scan's documented width is 1.5 metres; the 2.5-metre trial deliberately enlarges features modestly for this prototype's chase distance. UV0 overlaps intentionally for tiling; generate a separate UV1 if baking lightmaps. Preserve sharp imported normals.

The older `Solstice_Limestone_*` procedural maps remain in this staging folder only to reproduce the rejected A/B material passes. They are superseded by the Rock 3 selection and should not be copied into the current runtime material.

All final shader inputs are ordinary portable image maps and shared scalar PBR settings. The mesh carries metric UV0 for textures; baked lightmaps require generated UV1. Preserve imported custom/split normals; Unity's indiscriminate normal recalculation can round off building walls. Generate secondary lightmap UVs only if these static objects are baked. Architecture may use simple compound box colliders if needed; distant coast does not need mesh collision.

## Placement and visual validation

Replace the existing primitive tower/crown/facade-band group; leaving those cubes inside the new structures can occlude the new glazing and open floors. Use a restrained mix of A and B at uniform scales around 0.75–1.35. Avoid independently stretching height because the floor heights, entrance, balconies and facade rhythm provide human scale. A few deliberate clusters with waterfront gaps will read better than thirty equally spaced towers.

Replace the corresponding old procedural island mesh where using Cliff C. Do not superimpose it on the old concentric geology. Its shelf should intersect the ocean close to local Y=0; only the lower 3 metres belong below water. Small rotations and uniform scales vary placement. Keep the track clearance logic and evaluate clearance against the transformed footprint, not just the object's origin.

Inspect both models in a low chase-camera view at roughly 100, 200 and 350 metres, at 1080p with the game's actual lighting and fog. Check that the tower's glass/recess rhythm survives, B's open slot remains visible, cliff walls no longer read as giant triangle fans, and rock color remains lighter and more varied than the old dark synthetic cones. Review sun-facing and shadow-facing views; a Blender asset render is not runtime acceptance.

## Source and repeatability

`build_environment.py` (with `rock3_material.py` and the supplied CC0 source maps) regenerates the editable `.blend`, separate FBX files, statistics and CPU-rendered inspections. The final `.blend` includes clearly named preview lights/ground/camera; those are excluded from the asset exports. The asset meshes remain separate and contain meaningful material groups. The staged file names should remain stable for review and integration.

## Export audit and known limit

`export_audit.json` records a Blender FBX round trip and the raw FBX global axes: +Y up, +Z front, UnitScaleFactor 100 (metres), local origins zero. All vertices are finite and the tower dimensions survive import. The accepted frozen Tower B FBX contains 52 zero-area cap tessellation triangles; these do not contribute visible area. This is recorded rather than silently changing the already handed-off tower file. Cliff C and Tower A contain no zero-area triangles. The kit remains a candidate for native visual evaluation, not an AAA quality certification.
