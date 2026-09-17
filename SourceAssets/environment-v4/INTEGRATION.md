# Nocturne landmarks — first native candidate

The published candidate is **pass-02-landmarks**. The parent owns native visual validation and integration into NightDistrict. Source/studio approval is not a native lighting or composition verdict.

## Authored families

| Resource | Editable parts | Runtime renderers | Material slots | Triangles | Unity dimensions, metres |
|---|---:|---:|---:|---:|---|
| Nocturne_SplitSignal_Mast | 121 | 1 | 7 | 13,580 | 52.066 × 123 × 64 |
| Nocturne_ThermalExchange_Works | 183 | 1 | 8 | 27,736 | 44.7 × 90.95 × 84 |

The signal mast has unequal leaning ceramic pylons, an open structural middle, a diagonal transfer member, an offset two-storey skyroom, a grounded service hall, a rear equipment hall and a low stair terrace. The industrial landmark has three unequal cylindrical heat-rejection vessels with open recessed fan wells, broad cladding bands, a three-storey service podium, a lower rain canopy and an open pipe bridge on substantial piers. Source geometry, including every service form, is imported through FBX; the runtime component creates only landmark instances and four fixed spot lights.

## Placement and reservations

| Landmark | Progress | Side from route | World origin X,Y,Z | Yaw | Reserved half-footprint X,Z |
|---|---:|---:|---|---:|---|
| Signal mast | .230 | right 56 m | 304.1306, 0, 24.6507 | -27.7537° | 27, 33 m |
| Thermal works | .625 | left 51 m | -155.8806, 0, 148.3345 | -143.3585° | 23, 43 m |

Every prefab uses uniform scale 1 and a ground-level origin. Blender source X/Y/Z maps to the existing Unity importer convention -X/Z/Y; source parts intentionally have finished approach/end elevations. The signal service facade is source +X and the thermal service facade is source -X. Parent should verify this convention in native renders before any facing correction.

NightLandmarks exposes `Build(WorldBuilder, TrackPath)` and `public bool Overlaps(Vector3 center, Quaternion rotation, Vector2 halfSize)`. Build reserves both full footprints before loading resources. Overlaps uses four planar separating axes plus 3 m of exclusion padding. It is intended to run before generic districts, middle buildings or skyline placement. The parent has added that integration to NightDistrict; this package does not edit NightDistrict or existing city assets.

`placement_audit.py` ports TrackPath's 1,200-sample arc-length lookup and samples the whole route at 12,000 points. Minimum horizontal distance from road centerline to full cluster rectangle is **29.000 m** for the mast and **25.306 m** for the thermal works, including the later adjacent segment at progress .65042. Subtracting the 11 m road half-width leaves **18.000 m** and **14.306 m** beyond the road edge. Runtime uses 1,200 points with a conservative threshold of 18 m plus one complete sample spacing (19.537 m). Ground origins and full height envelopes remain outside the road horizontally, so there is no reliance on an overhead-height exception.

Prior actual camera metadata is used only for projection estimates. From the lap-one .17 camera the signal's base-at-33 m and roof span about 466 pixels; from the .55 camera the thermal works span about 369 pixels, on the left. At close passes the tallest parts naturally leave the upper frame. Camera metadata does not prove lighting, occlusion or visual acceptance.

## Materials and lights

Seven/eight material groups are remapped to shared WorldBuilder-owned materials: broad ceramic, cast concrete, recessed structure, satin titanium, muted oxide pipe insulation, smoked glazing, limited occupied rooms and concealed service lamps. No repeated facade window shader is applied to the drums. No normal maps are used by these materials. Only selected skyroom and service windows emit light.

There are four broad fixed spot washes: two on the signal structure and skyroom, two on the drums and gallery. They cast no shadows and have no animation. The authored small lamp housings belong to the FBX; runtime adds no visible light geometry. Final brightness and surface response require native review.

## Files and checks

- `pass-02-landmarks/Nocturne_Landmark_Kit.blend`: editable source collections and a hidden combined export collection. Mast is visible by default; unhide the thermal collection and hide the mast to work on the second asset.
- `build_landmarks.py`: current pass-02 recipe, with an immutable-output guard. Select a new staging pass to regenerate.
- `pass-02-landmarks/*.fbx`: exact candidate copied to `UnityProject/Assets/Resources/Art/Environment/Landmarks/`, with importer metas. All live hashes match `asset-stats.json`.
- `pass-02-landmarks/01-mast-studio.png`, `02-thermal-studio.png`: neutral full-bounds source previews.
- `audit_exports.py` and `pass-02-landmarks/export-audit.json`: Blender FBX reimport checks, separate from Unity testing.
- `placement-audit.json`: exact site, full-road clearance and prior-camera projection results.

Both runtime meshes have zero nonmanifold edges and zero degenerate triangles before export and after FBX reimport. UV0 exists, is finite and uses overlapping metric projection (4 m per tile). The thermal export has finite unit tangents on every loop. The mast has **one zero tangent at loop 964 after Blender FBX reimport** on a small upper structural bevel face. Other tangents are finite/unit. This is documented rather than described as a complete tangent pass; none of the runtime landmark materials uses tangent-space normal mapping. Any future normal-map addition must fix or regenerate this tangent first.

## Iteration record

Pass 01 was rejected for radial drum folds/fan blades rotating around the asset origin, leaving disconnected parts outside the planned silhouette. Pass 02 corrects those part pivots, restores the complete reserved footprint and widens studio framing to show full bounds. Pass 03 attempted integer-tile UV separation on identical geometry to eliminate the single mast tangent defect; it did not change that defect and is **not published**. Its diagnostic output is retained but no runtime file references pass 03. The published version remains pass 02.

No Unity editor, native player, or visual acceptance test was run by this asset task. Parent owns compile, native screenshots, independent review and any subsequent bounded correction.
