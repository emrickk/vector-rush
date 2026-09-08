# Kestrel 07 — original Vector Rush craft

`Kestrel07.blend` is the editable authored model and studio. `build_assets.py` reproduces the model, FBX and three studio images using Blender 5.2 LTS. All geometry and markings are original; no external textures or purchased assets are used.

Run from the project root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --factory-startup --background --python SourceAssets/build_assets.py
```

The headless application needs normal graphics-device access on macOS. Restricted graphics detection can crash Blender before Python starts. This is independent of the asset script.

## Geometry and coordinates

Source uses metres, forward **−Y**, up **+Z**. FBX exports with `axis_forward='-Z'`, `axis_up='Y'`, `bake_space_transform=True`. The craft nose should face **Unity +Z**, with Unity +Y up, at scale one. Confirm the nose orientation during the first Unity import; the long sharp ivory tips identify the nose and circular turbines identify the rear. Do not add a compensating scale of 100.

Overall source bounds are approximately 5.44 × 7.49 × 1.70 metres (X, Y, Z). Origin is near the center of the structural keel, not the underside. Runtime collision should use a simplified hull, not the detailed render mesh. Asset stats are written to `asset-stats.json` on each build.

The source retains meaningful individual objects. The FBX combines objects sharing a material to keep renderer count low. Do not use its detailed topology for track/vehicle collision.

The final model contains 120 editable source objects and exports as 9 material meshes, with 29,922 vertices and 51,772 triangles. A read-only binary FBX check is available as `python3 SourceAssets/verify_fbx.py`; its report is saved to `evidence/asset-renders/fbx-validation.json`.

Studio review views are `evidence/asset-renders/hero-front-three-quarter.png`, `hero-rear-three-quarter.png`, and `hero-top.png`, each 1600 × 1200. These are actual Cycles renders of the exported source geometry. The original pilot pass was revised to strengthen identification, replace the faceted canopy with smooth glazing, add physical service recesses, correct mirrored face normals, sweep the fins, and batch the runtime meshes by material.

## Unity URP material mapping

Replace imported materials by **name** with URP/Lit materials; the Blender renderer's node system is not a portable Unity shader. All slots use an opaque surface. Smoothness below is approximately one minus Blender roughness.

| Name | Base color RGB (linear) | Metallic | Smoothness | Note |
|---|---|---:|---:|---|
| Ivory | 0.80, 0.84, 0.81 | 0.44 | 0.72 | Layered pearl ceramic armor |
| Graphite | 0.024, 0.037, 0.045 | 0.68 | 0.69 | Structural carbon and dark frames |
| Glass | 0.009, 0.042, 0.053 | 0.79 | 0.86 | Opaque smoked canopy; reflections matter |
| Signal | 0.34, 0.47, 0.001 | 0.00 | 0.42 | Saturated acid yellow identifiers |
| Engine | 0.015, 0.88, 0.81 | 0.30 | 0.81 | Enable emission, same RGB × 5; enable bloom |
| Metal | 0.19, 0.25, 0.27 | 0.85 | 0.72 | Turbine rims, blades and mechanisms |
| Ceramic | 0.055, 0.08, 0.088 | 0.35 | 0.53 | Heat-resistant turbine shells |
| Ink | 0.02, 0.04, 0.045 | 0.20 | 0.50 | Wordmark and class designation |
| WhiteMark | 0.83, 0.91, 0.90 | 0.20 | 0.55 | High contrast race numbers on graphite panels |

For rivals, tint Ivory and Signal per team while retaining the dark mechanical and emissive parts. Studio images are asset review evidence, not gameplay screenshots.
