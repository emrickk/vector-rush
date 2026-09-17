# Shared art / Unity contract — version 1

This file is authoritative for both implementers. It defines data exchange, not new game code already present. All path examples below are relative to the project root unless described as package-relative.

## Coordinate and course identity

- Unity: metres, +Y up, +Z forward, +X right; rotation is a normalized quaternion `[x,y,z,w]`. Scene track root remains position zero, rotation identity, scale one.
- Blender source: metres, +Z up, -Y forward. Use the existing proven FBX axis/export convention in `SourceAssets/environment-v3/build_transit_kit.py`; verify exported dimensions and asymmetric orientation in Unity rather than assuming the flags are correct. Every exported mesh root has applied scale and its documented pivot.
- Sol exports `evidence/nocturne-production/context/geometry-context.json` from the actual `TrackPath.Evaluate` and `ChaseCamera` settings. Include `contractVersion:1`, `sourceCommit`, SHA-256 course hash, width, length, camera distance/height/FOV formula, and 1,201 frames at normalized `i/1200` for `i=0..1200`. Each frame has `progress`, `distanceMeters`, `position`, `forward`, `right`, `up`, all vectors float arrays of length 3. Final frame repeats the start pose but distance equals course length.
- Sol also writes `course-data.json` containing only width, length and the frame array, in invariant-culture compact JSON. `courseHash` is the SHA-256 of those exact UTF-8 file bytes; source commit and camera settings are excluded. Hash the actual course data, not only a file name. Astra copies that `courseHash` into its manifest/layout. Import fails on a mismatch. Astra does not independently redraw or approximate the route spline.
- Astra writes final placements in Unity world coordinates, computed from this context. For track-relative authoring, `position = frame.position + frame.right*lateral + frame.up*vertical + frame.forward*along`. Resolve that expression before writing layout. Upright buildings can use yaw-only orientation; track components use the actual banked frame. Declare which was used in each instance's `frameMode`.

## Collision, clearance and track construction

Sol retains current driving geometry/rules for this run: the running surface spans local x=-11..11 m, existing shoulders/walls retain their actual collision positions. Copy the collision-generation logic faithfully into the editor production path; do not preserve it by also building the legacy city at runtime.

Astra supplies an art cross-section specification for deck underside, barrier outer body, fascia, drains and maintenance edge in `track-profile.json`. Sol sweeps that profile along the actual path into persistent meshes. Do not line up straight 22 m-wide FBX tiles around curves or replace collision with visual LOD meshes. Fields are `contractVersion`, `courseHash`, and `strips[]`; each strip has `id`, `materialId`, `pointsXY` (ordered cross-section vertices), `surfaceRole`, `uvMetersPerTile`, `twoSided:false`. Surface roles are `outerBarrier`, `underside`, `fascia`, `drain`, `ledge`; the existing running surface uses the separately provided road material.

Unrelated decorative assets must not intrude into any segment's protected road/craft volume: lateral -12.2..12.2 m and vertical -0.05..8 m relative to the sampled route. This volume is a conservative starting constraint, not proof of camera clearance. Intentional deck/barrier pieces are separately tagged `trackAssembly` and checked against original collision/visible-boundary alignment. Check candidate geometry against every nearby course branch, not only its nominal placement anchor. Test swept vehicle and observed camera paths around turns/recovery. A bounding-sphere pass alone is insufficient for long rotated roofs or piers.

## Immutable art package

Astra owns `SourceAssets/nocturne-production/`. Publish revisions as `art-package-01`, `art-package-02`, etc. Build a temporary directory, validate it, then publish it atomically and write `READY.json` last. Sol never imports a package without READY. Do not alter a package after publication.

Each package contains:

```text
manifest.json
layout.json
lighting.json
track-profile.json
meshes/<assetId>_LOD0.fbx
meshes/<assetId>_LOD1.fbx
textures/<materialId>_BaseColor.png
textures/<materialId>_Normal.png
textures/<materialId>_MetallicSmoothness.png
textures/<materialId>_Occlusion.png
textures/<materialId>_Emission.png   (only when used)
asset-stats.json
checksums.json
READY.json
```

Blender source files, source textures, recipes and studio previews live alongside packages under `SourceAssets/nocturne-production/source/`, `recipes/`, and `previews/`; preserve their exact paths/hashes in the manifest. A production package is complete even when its first exemplars are not placed around the whole course yet; `layoutComplete:false` makes that stage explicit. The final package must have `layoutComplete:true` and all four zone IDs present.

## Manifest fields and one valid shape

```json
{
  "contractVersion": 1,
  "revision": "art-package-01",
  "courseHash": "0000000000000000000000000000000000000000000000000000000000000000",
  "layoutComplete": false,
  "sourceBlend": "source/NocturneProduction.blend",
  "assets": [
    {
      "id": "NR_GalleryEntry",
      "lod0": "meshes/NR_GalleryEntry_LOD0.fbx",
      "lod1": "meshes/NR_GalleryEntry_LOD1.fbx",
      "pivot": "ground-center",
      "boundsMin": [-15.0,0.0,-3.0],
      "boundsMax": [15.0,18.0,3.0],
      "materialSlots": ["NR_Structure","NR_Ceramic","NR_Metal","NR_Diffuser"],
      "trackAssembly": false,
      "collider": "none",
      "lightmapUVs": "generate"
    }
  ],
  "materials": [
    {
      "id": "NR_Ceramic",
      "baseColor": "textures/NR_Ceramic_BaseColor.png",
      "normal": "textures/NR_Ceramic_Normal.png",
      "metallicSmoothness": "textures/NR_Ceramic_MetallicSmoothness.png",
      "occlusion": "textures/NR_Ceramic_Occlusion.png",
      "emission": "",
      "metersPerTile": [4.0,4.0],
      "uvMode": "metric",
      "normalScale": 1.0,
      "emissionColorLinear": [0.0,0.0,0.0],
      "emissionIntensity": 0.0
    }
  ]
}
```

The example demonstrates field names/types, not a publishable asset. Actual bounds, hash, material records and paths must all be populated from exported data; the example's all-zero hash and other placeholder values are forbidden in a published package. Every referenced material slot has exactly one material record. IDs are unique, case-sensitive ASCII. Paths stay inside the package or declared source root; reject traversal and absent dependencies. `checksums.json` maps every payload file other than READY/checksums to its SHA-256; READY records the hash of checksums plus revision/course hash.

Sol implements explicit serializable DTOs matching this schema; unknown versions, duplicate IDs, non-finite transforms, missing dependencies and checksum mismatch fail before scene mutation. Import into staging first; preserve the last usable scene if validation fails.

## Materials and meshes

- BaseColor and Emission use sRGB import; Normal uses Unity NormalMap type; MetallicSmoothness and Occlusion are linear. Mask channels: metallic in R, smoothness in A, occlusion in the separate map's G. Do not put roughness in the smoothness channel.
- Normal maps are tangent-space +Y convention, with flat `(0.5,0.5,1)`. Check their actual direction on the orientation specimen. No displacement is assumed by the shader.
- Default surface textures are 2048²; use 4096² only for a documented hero surface where native viewing warrants it. Use mipmaps; repeat when `uvMode` is `metric` and clamp when it is `atlas`. Keep road anisotropic sampling consistent with native testing. These are starting allocations, not proof of sufficient visual quality.
- UV0 uses the declared metric tile size; do not multiply the same metre conversion again in Unity material tiling. Sol imports with unit material texture scale unless a declared unique atlas requires otherwise.
- Set `lightmapUVs` to `authored` for supplied UV1 suitable for baking, or `generate` for Unity generation; Sol verifies non-overlap/padding at intended lightmap allocation before the bake. Separate this from allowed overlapping UV0 tiling.
- Include normals/tangents and applied geometry modifiers. No degenerate triangles, negative/non-uniform root scale, floating duplicate coplanar overlays, exported cameras/lights or hidden studio floors. Openings and thin sheet geometry can have legitimate boundary edges; report them instead of claiming all meshes must be closed solids.
- LOD1 preserves main silhouette/openings and reduces small details. Sol adds LODGroup settings and checks transitions in native motion. Artists do not encode runtime LOD thresholds in Blender.
- Initial per-instance LOD0 guidance: small reusable module <=15k triangles, near facade/station unit <=80k, unique major landmark <=150k. These are early budgeting signals. If a necessary asset exceeds them, report geometry/view reason and measure in the representative scene; do not silently flatten it into a box to hit a count.

## Layout fields

`layout.json` has `contractVersion`, `revision`, `courseHash`, `zones`, and `instances` arrays. Zones use IDs `viaduct`, `canyon`, `thermal`, `station` with the master-plan ranges. Each instance has `id`, `assetId`, `zoneId`, `position:[x,y,z]`, `rotation:[x,y,z,w]`, `scale:[1,1,1]`, `frameMode:"upright"|"banked"`, `role:"near"|"middle"|"far"`, and `gi:"lightmap"|"probe"|"none"`. No unseeded runtime scatter. Whole-lap spatial design belongs to Astra; Sol reports placement conflicts with coordinates and evidence instead of silently moving buildings out of view.

## Lighting fields

`lighting.json` has identity fields, `environment`, `lights`, and `reflectionVolumes`. Environment fields: `fogColorLinear:[r,g,b]`, `fogDensity`, `exposureEV`, `ambientTintLinear:[r,g,b]`, `skyMaterialId` (must exist in material payload or be `production-night-sky` implemented by Sol). Sol isolates production renderer/volume assets and implements the documented Exp2 fog conversion correctly.

Each light: `id`, `fixtureInstanceId`, `type:"spot"|"point"|"directional"`, `position`, `rotation`, `colorLinear`, `intensity`, `range`, `spotOuterDegrees`, `spotInnerDegrees`, `mode:"baked"|"mixed"|"realtime"`, `castsShadows`. Intensity means the current URP Light.intensity value, not an unsupported physical-lumen promise. Point/directional unused angles are zero. Fixtures and light coordinates must agree. For baked emitters the source surface must use the intended emission and GI setup.

Each reflection volume: `id`, `position`, `size`, `resolution` (128 or 256 initially), `intensity`, `boxProjection`. Sol creates baked local probes excluding vehicles; these are not dynamic ship reflections. Sol places moving-object light probes through the road corridor and records bake backend/settings. Astra's values are art intent; Sol may fix invalid implementation values, but communicates visible lighting changes back to Astra. Keep all revisioned adjustments documented.

## Outputs and responsibility

Sol alone copies verified packages to `UnityProject/Assets/Art/NocturneProduction/Imported/<revision>/`, creates Unity materials/prefabs under `Assets/World/NocturneProduction/`, and writes production scenes/settings. Sol generates/preserves Unity `.meta` files. Astra never writes these destinations.

Astra's source package is the authority for geometry/textures/layout/light intent. Unity native output is the authority for how those inputs actually render. Neither worker can assert final visual acceptance; their task is a technically functioning, substantially rebuilt candidate with honest self-checks and raw evidence for the owner's first review.
