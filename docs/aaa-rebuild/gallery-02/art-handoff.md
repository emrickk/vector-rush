# Warm gallery 02 art handoff

Status: **TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW**. Blender authoring/export is complete. Unity import, native appearance, motion, performance and owner acceptance are not claimed.

## Delivered package

- Package: `SourceAssets/aaa-nocturne/exemplar-01-gallery02/`
- Source: `source/Nocturne_Gallery_Exemplar_02.blend`
- Recipe: `SourceAssets/aaa-nocturne/recipes/build_exemplar_gallery02.py`
- Finalizer: `SourceAssets/aaa-nocturne/recipes/finalize_package_gallery02.py`
- Studio-only inspection: `checks/studio-gallery.png`
- Schema/revision: `aaa-nocturne-exemplar-1` / `exemplar-01-gallery02`
- Course identity and exact current source SHA-256: `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`

I directly inspected the supplied native original and generated concept. The original's useful continuous chamfered shell, route framing and warm/cyan identity are retained as the baseline. The new kit answers the visible flat panels, crude fixture housings and uniform wash with deeper load frames, recessed wall cassettes, connected shoulder/crown trays, shielded inward-facing diffusers and a local-pool lighting contract. It does not copy the rejected open scaffold.

## Assembly and replacement contract

The two opening placements are unchanged. Their four facade FBXs are byte-identical to `exemplar-01`; they remain only for schema/resource compatibility.

The station recipe has 19 placements from frames `1032, 1040, 1049, 1057, 1066, 1074, 1082`, covering progress `.860000014–.901666641` and the declared `.86–.902` gallery span:

- Entry and exit: `NA_GalleryPortal_G02`
- Five interior keyed frames: `NA_GalleryRib_G02`
- Six connected two-sided wall spans: short/long `NA_GalleryWallBay_*_G02`
- Six connected crown/fixture spans: short/long `NA_GalleryCeilingBay_*_G02`

Short and long bays match the actual alternating 12.30 m / 13.84 m frame spacing. Panel/crown pivots sit at each span start and extend along source `-Y` / Unity local `+Z`; no negative scale or stretched runtime mesh is required. Portal/rib pivots sit at route centre. The retained `Warm gallery continuous shell` is deliberately absent from replacement scope.

Replace exactly these renderer groups:

1. `Warm gallery primary portal structure`
2. `Warm gallery recessed ceramic cassettes`
3. `Warm gallery folded service returns`
4. `Warm gallery light and maintenance housings`
5. `Warm gallery integrated diffusers`
6. `Warm gallery maintenance markings`

Replace exactly these local light families:

1. `Warm gallery concealed surface wash`
2. `Warm gallery road pool`

Do not replace the continuous shell, road, barriers, cyan navigation treatment, craft, camera, HUD, physics, global atmosphere or global lights.

## Assets and measured payload

| Asset | LOD0 tris | LOD1 tris | Unity dimensions X/Y/Z m | Function |
| --- | ---: | ---: | --- | --- |
| `NA_GalleryPortal_G02` | 9,228 | 1,116 | 30.30 / 15.475 / 2.515 | Deep entry/exit ring, bearing plinths, service spines, crown fascia and shielded crown fixture. |
| `NA_GalleryRib_G02` | 2,836 | 676 | 30.30 / 15.495 / 1.56 | Continuous keyed structural ring, wear liner, feet, shoulder cleats and crown joints. |
| `NA_GalleryWallBay_S_G02` | 13,912 | 2,376 | 30.21 / 14.224 / 12.86 | 12.30 m paired wall/shoulder assembly with broad recessed cassettes and fixtures. |
| `NA_GalleryWallBay_L_G02` | 13,912 | 2,376 | 30.21 / 14.224 / 14.40 | 13.84 m paired wall/shoulder assembly. |
| `NA_GalleryCeilingBay_S_G02` | 3,008 | 704 | 20.50 / 1.36 / 12.60 | 12.30 m connected crown tray, panels, ties and recessed transverse luminaire. |
| `NA_GalleryCeilingBay_L_G02` | 3,008 | 704 | 20.50 / 1.36 / 14.14 | 13.84 m connected crown/fixture assembly. |

Every runtime mesh is a single FBX mesh with authored UV0, applied transforms, bevel-derived normals, tangents and hand-reduced LOD1. Materials use restrained 2048² BaseColor/metallic-smoothness maps plus a shared micro-normal; `NA_BronzePanel` is the new warm panel surface. Macro seams, louvres, housings, returns, feet and joints are geometry rather than baked texture detail.

## Fixture and lighting contract

`manifest.json` keeps `lights: []`. `gallery-lighting.json` is the separate integrator-owned optional contract and contains 20 geometry-derived world-space spot suggestions:

- 8 downward crown sources (entry, exit and six bay fixtures), each tied to a recessed diffuser tray; suggested 500 intensity, 25 m range, 76°/36° cone, soft shadows.
- 12 inward/downward wall sources (left and right in each wall bay), each tied to a diffuser behind a structural bracket and glare visor; suggested 225 intensity, 16 m range, 62°/26° cone, no shadows.
- Linear RGB is `[1.0, 0.67, 0.36]`. Every record has explicit position, aim direction, source placement and source part. `fixture-sockets.json` preserves the local and resolved world geometry evidence.

These values are integration starting points, not validated native illumination. The integrator should retain separated pools/dark intervals and cyan edge separation without applying a global orange wash.

## Checks and limits

- Blender 5.2.1 LTS generated and exported the source, 16 FBXs and studio render. The sandboxed binary crashed in an optional Metal whitelist probe; a temporary ad-hoc-signed clone disabled only that probe. The installed Blender app was not modified. Cycles studio rendering used CPU.
- All 16 FBXs reimported in Blender with matching triangle counts/material slots, UVs, finite normals and maximum bounds error `0.0000011325 m`.
- All authored meshes report zero nonmanifold edges, degenerates, duplicate triangles, zero-area UV triangles and nonpositive closed components.
- All 21 placements report zero protected-corridor BVH intersections and zero contained vertices across all 1,200 cells. Warm structural/wall minimum sampled lateral margin is `0.3797 m`; ceiling assets lie above the protected height.
- Eleven PBR PNGs pass CRC/decompression and 2048² channel checks. `checksums.json` covers the current course bytes, blend, recipe snapshot, FBXs, maps, JSON contracts, logs and studio still. `STAGED.json` is sealed with `layoutComplete: true` and the required technical-ready status.
- The studio still confirms a connected enclosure, broader cassette rhythm, substantial housings and no open scaffold. It is a straightened authoring view without craft/HUD, route curvature or native renderer behavior, so it cannot satisfy the native critic gates.

Heavy Blender slot **RELEASED**. Parent/integrator may now import the immutable staged package and obtain matched native control/candidate evidence. No Unity, Git or shared plan/history file was changed in this lane.
