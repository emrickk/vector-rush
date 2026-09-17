# Architecture worker — bounded exemplar 01

Status: staged technical candidate complete; **BLOCKED_VISUAL_REVIEW**. This worker cannot inspect images: `view_image` returned `view_image is not allowed because you do not support image inputs`. Native original/target appearance has not been judged. No renders, Unity/native processes, shared documentation changes, or git operations were performed in this lane.

Bounded step defined before implementation: author an editable Blender civic/service facade family and a deep portal/rib/panel gallery family, export real FBX LOD0/LOD1 payloads with material/UV contracts, verify geometry and FBX round trips, and calculate proposed world placements from the exact direct-02 course frames. All work stays in `SourceAssets/aaa-nocturne/` and this handoff. Parent owns integration and shared history.

Written direction read: original Nocturne A/B reference descriptions and prompts; global independent critique 004; the dispatch and coordination documents; original gallery cross-section in `NightTrackLighting.cs`; proven transit-kit FBX export recipe. These are textual requirements and historical findings, not this worker's visual observations. `critic-001.md` is now available and reports the same image-inspection blocker; it supplies no observed concrete mesh defect to correct. Its foreground inventory and native composition gates remain open.

The full circuit rebuild remains unfinished. This is a bounded staging package, not a replacement scene, adoption verdict, finished-game or AAA pass. Completed exports and measured validation evidence are documented below.

## Delivered package and exact paths

Local authoring/export is complete. Status: **TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW**.

- Package: `SourceAssets/aaa-nocturne/exemplar-01/`
- Editable Blender: `SourceAssets/aaa-nocturne/exemplar-01/source/Nocturne_Architecture_Exemplar.blend`
- Executable recipe: `SourceAssets/aaa-nocturne/recipes/build_exemplar.py`
- Exact recipe snapshot: `SourceAssets/aaa-nocturne/exemplar-01/source/recipe_snapshot.py`
- Integrity recipe: `SourceAssets/aaa-nocturne/recipes/finalize_package.py`
- Mesh files: `SourceAssets/aaa-nocturne/exemplar-01/meshes/<assetId>_LOD0.fbx` and `<assetId>_LOD1.fbx`
- `manifest.json`: every asset, measured bounds, slots, material constants/maps, axes and UV conventions.
- `layout.json`: 13 proposed world placements, normalized `[x,y,z,w]` quaternions, unit scales, exact frame index and complete source frame per instance.
- `fixture-sockets.json`: physical diffuser centroids in Unity local coordinates for parent light integration. These are fixture centers, not validated light-source transforms or prescribed native intensities.
- `asset-stats.json`, `checks/fbx-roundtrip.json`, `checks/placement-clearance.json`, `checks/texture-integrity.json`: machine-readable measured evidence.
- `checks/blender-build.log`: actual nonrendering Blender run and FBX reimports.
- `checksums.json` hashes every payload file except itself and `STAGED.json`; `STAGED.json` hashes the checksum manifest. Deliberately no production `READY.json` or adoption signal.

This is schema `aaa-nocturne-exemplar-1`, not a drop-in full legacy Sol/Astra production package. Parent maps the explicit records into its optional original-baseline exemplar resources. All referenced files exist. No external assets or licenses are required.

| Asset ID | LOD0 triangles | LOD1 triangles | Unity dimensions X / Y / Z, metres | Construction intent |
| --- | ---: | ---: | --- | --- |
| `NA_CivicFacade_A` | 32,288 | 4,596 | 10.764 / 26.122 / 26.800 | Four structural bays, two recessed glazed levels, spandrel ventilation, folded roof/coping, subordinate stair lantern, drain grates, haunches, podium with underslung supports. |
| `NA_ServiceFacade_A` | 13,220 | 2,588 | 10.764 / 17.525 / 14.000 | Two service portals, deep returns, segmented shutters, integrated canopy lights, extract hoods and braced podium. |
| `NA_GalleryPortal_A` | 9,792 | 1,600 | 30.860 / 16.075 / 3.950 | Original chamfered clear opening, welded U-section, second recessed threshold, deep shoulder/crown returns, bearing feet, splice plates, shielded entry and brow fixtures. |
| `NA_GalleryRib_A` | 6,964 | 932 | 30.860 / 16.075 / 1.820 | Continuous structural U-section with bearing collars, splice plates, service chases and crown contacts. |
| `NA_GalleryPanel_A` | 6,016 | 1,320 | 4.589 / 14.439 / 5.400 | Right-side wall plus angled shoulder; dark tray, deep edge folds, broad ceramic faces, recessed maintenance louvres and shielded lower fixture. |

Dimensions include support feet and roof details; facade pivots are at route-level podium centers, **not at the lowest support foot**. Civic lower bound Y is -8.7 m. The facade primary roof is approximately 13.6 m above its pivot. Do not mistake its 26.1 m total bounds for a 26 m wall above the track. Exact bounds per asset and LOD are in `asset-stats.json`.

## Geometry, pivots and placement contract

Blender uses metres, +X right, +Z up, -Y forward. FBX uses the proven transit-kit options: `axis_forward='-Z'`, `axis_up='Y'`, `bake_space_transform=True`, `apply_unit_scale=True`, `apply_scale_options='FBX_SCALE_UNITS'`. Expected Unity local conversion is `(x,z,-y)`. All export roots have applied transforms and unit scale; one mesh object per FBX. No cameras, lights, rigs or studio floors are exported. Runtime LOD meshes are hidden in the source file, separate from named editable construction parts. Enable individual editable collections to inspect each family; all assets retain their own local pivot rather than a shared presentation offset.

The facade road-facing face is local **-X**, along the building's long local Z axis. The right-side placement uses this directly. The left facade rotates 180 degrees around local up so its recessed face looks toward the road. The gallery portal/rib pivot is road center at local Y=0. The panel pivot is also road center, **not panel center**: its wall is around local X=14.5 m. The left panel uses a positive-scale 180-degree up-axis rotation; do not mirror it with negative X scale. Entry second threshold lies on the approaching side, local Z=-2.3 m.

Source course: `evidence/nocturne-production/direct-02/context/course-data.json`; 1,201 frames, width 22 m, length 1,844.517578125 m. Exact bytes SHA-256:

`32eb94bda0b257b30c3b111b2a67beb1626d1dca77577905ae021eee7bc36f07`

| Instances | Exact frame indices | Offset from supplied frame | Orientation |
| --- | --- | --- | --- |
| Opening civic right | 95 | lateral +23 m, vertical -1.5 m | Upright, horizontal heading |
| Opening service left | 120 | lateral -22 m, vertical -2 m | Upright, horizontal heading +180 degrees |
| Warm entry | 1032 | zero | Banked frame |
| Warm ribs | 1040, 1049 | zero | Banked frame |
| Warm right panels | 1034, 1038, 1043, 1047 | zero | Banked frame |
| Warm left panels | 1034, 1038, 1043, 1047 | zero | Banked frame +180 degrees around local up |

The opening is a two-building exemplar, not a skyline replacement. The gallery covers only the entrance and first approximately 26 m; the rest of the existing warm gallery and full course are unfinished rollout. Frame 1040 is the nearest exported sample to the original .867 rib; 1049 is the nearest exported sample to .874. No unexported interpolated route was fabricated. These small sampling differences must be resolved in the parent's bounded replacement grouping.

**Integrator offset detail:** layout positions always use the supplied banked vectors in `frame.position + right*lateral + up*vertical`. Upright applies to orientation only. The current rendering handoff says its upright offsets use the yaw frame. If translating into `AAAExemplar` track-relative offsets, project `layout.position - frame.position` into the hook's actual yaw basis; do not paste the lateral/vertical fields unchanged. The resolved world transforms in `layout.json` are authoritative for this staged proposal. Unity axis/orientation remains unverified; compare the asymmetric stair lantern and road-facing reveals before accepting import settings.

## Material and texture contract

Exact slot IDs are `NA_Slate`, `NA_Ceramic`, `NA_Graphite`, `NA_Aluminium`, `NA_Glass`, `NA_Interior`, `NA_WarmDiffuser`, `NA_CoolDiffuser`. Each mesh uses only its listed subset; bind by exact imported material name from `manifest.json`. All surfaces use opaque materials, including controlled dark glazing; no transparent building shells or large emissive window walls are required. Warm light is limited to selected transoms/landing strips and actual fixture diffusers.

Four opaque construction materials each have 2048² BaseColor and MetallicSmoothness maps. One shared 2048² `NA_Micro_Normal.png` provides subtle periodic tooling. Maps are generated from deterministic mathematical fields and seeded variation. They contain no image-generated architecture, external texture photos, baked shadows or fake geometric fasteners. Macro construction resides in geometry. BaseColor is sRGB; packed masks and normal are linear; **mask R = metallic and A = smoothness**. Use Unity NormalMap import for the +Y tangent normal. UV0 is metric, 4 m per tile, intentional overlaps. Unity texture tiling is `(1,1)`, Repeat with mipmaps. Set base-color tint white on mapped materials, otherwise use the manifest's linear constants without double multiplication. Use mask channel A for smoothness. The shared normal's proposed scale is .3, subject to actual native orientation inspection. No AO map or supplied lightmap UV1 is claimed; generate lightmap UVs in Unity if baking.

Editable source parts and runtime exports both have UV0. Runtime exports carry weighted surface normals and tangents. The Blender material graph reproduces albedo, roughness and micro-normal intent; Unity material import remains parent work. Diffuser emission uses low source values (.65 warm / .8 cool) as geometry-preview intent, not a claim about native lux, intensity, exposure, GI or completed illumination.

## Integration boundary and source-derived collision issues

The original scene remains the baseline. These gallery meshes occupy the original structural ring/cassette envelope and must be used as **bounded replacement geometry**. Adding them on top of `Warm gallery primary portal structure`, `recessed ceramic cassettes`, `folded service returns`, or coincident fixture geometry would introduce duplicate surfaces and false visual evidence. Existing grouped meshes cover the whole gallery, so the rendering hook's containment guard cannot safely hide only this sample. Parent must extract/rebuild the bounded original groups, preserve the surrounding gallery and keep collider/camera/route unchanged. Do not enlarge a replacement volume until it hides the whole gallery. Retain the swept continuous shell where it provides the backing; check shoulder contact and shell/panel separation in the actual combined scene.

Facade placement is a proposal based on exact route clearance, not a survey of existing city objects. Existing-city collision, skyline composition and semantic foreground retain/rework/replace decisions are unassessed because no images can be inspected. The original paired signal mast, thermal/station silhouettes, lamps, city layering, craft and road are not redesigned by this package.

Fixture sockets locate source-authored geometry only. Parent must position local lights outside the appropriate emitting surface and judge broad surface illumination in the integrated original scene. This worker does not prescribe a global lighting/exposure reset. Native LOD transition thresholds also remain parent work: LOD1 removes micro fittings and reduces bevel segmentation while retaining openings and bounding silhouettes; transition popping has not been observed.

## Measured self-check evidence

Actual authoring/export ran in Blender **5.2.1 LTS**, build hash `9e2066aef7ef`, through CLI background mode with two threads and no renders. Sandboxed startup crashed in Metal device detection before Python; the authorized nonrendering run succeeded outside the sandbox. No native game or Unity process ran.

- Ten FBXs exported and reimported into a separate Blender scene. Triangle totals, exact material triangle counts and UV layer presence match; all imported corner normals are finite. Maximum transformed bounds error: **0.0000011921 m**. Blender round trip is not a Unity import pass.
- Every runtime LOD has zero boundary/nonmanifold edges, degenerate triangles, exact duplicate triangles, zero-area UV triangles, or nonpositive-volume closed components. UV values are finite. These checks establish closed component winding and triangle integrity, not absence of all assembly intersections: intended bearings/joins overlap volumetrically and require visual contact inspection.
- LOD0 and LOD1 facade/portal/rib bounds match. Panel LOD1 differs from LOD0 by less than 0.007 m in X/Y due to bevel segmentation; long-axis span stays 5.4 m.
- All 13 proposed placements have zero triangle intersections against **1,200 closed protected road corridor cells**, across every course branch. A separate all-frame vertex-containment check also reports zero vertices in lateral ±12.2 m / vertical -0.05..8 m. Measured lowest sampled lateral margin within that height is approximately **1.324 m** at the entry. This is a static sampled envelope check, not continuous collision, swept craft or chase-camera validation.
- Nine PNGs pass CRC/decompression and 2048² channel checks. Package references and file SHA-256 values pass integrity verification.
- A technical correction shortened the gallery cassettes from 5.9 to 5.4 m before final export to leave longitudinal reveals beside the sampled rib positions. Editable source UVs and explicit fixture centroids were also completed before sealing this staging package.

The four supplied reference files exist with expected native 1920×1080 / target 1672×941 dimensions. Their identities were checked from PNG headers and hashes, **not visually inspected**. The critic-001 image blocker remains unresolved and contains no observed defect to claim corrected. No stills, motion/audio, human-play, performance or AAA pass is asserted.

## Remaining limitations and stop

Native material response, Unity axis import, complete foreground appearance, existing-city overlap, gallery replacement integration, lighting, shadow contacts, visual LOD transitions and camera clearance are pending. No render was attempted while the parent held the heavy-work slot. Image capability is a blocker to artistic adoption. Whole-circuit architectural rollout, other zones and the full racing experience remain unfinished.

This worker changed only its owned art source/package tree and this handoff. No Unity files, shared plans/history, git state, credentials or native processes were touched. Parent owns shared milestone history and any commit/push. Required local work is complete; no reviewer, additional agent, or continuation has been launched.
