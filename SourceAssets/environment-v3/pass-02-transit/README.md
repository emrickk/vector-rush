# Nocturne transit district — integrated candidate 01

2026-09-08. Editable original source: `Nocturne_Transit_Kit.blend`. Production recipe is preserved alongside the source. Source assets use metres and Blender Z up / -Y forward. FBXs target Unity Y up / +Z forward with applied scale and no generated colliders. Runtime meshes in `EXPORT_RUNTIME` retain each asset's local origin. Editable workshop and buttress source groups are offset only for the kit studio layout.

| Asset | Unity dimensions X / Y / Z, m | Triangles |
|---|---|---:|
| Transit Station A | 23 / 22.35 / 64 | 13,424 |
| Service Workshop A | 8 / 6.985 / 18 | 2,972 |
| Platform Buttress A | 5.1 / 11.135 / 8 | 648 |

The station has a two-leaf folded canopy, narrow clerestory, open platform, deep room reveals, real columns and knees, and an asymmetric stair core. The workshop is intentionally lower and quieter. Geometry and UV0 are explicit; UV0 uses overlapping metric tiling at four metres per tile, not a unique baked atlas. Near-window occupancy is authored in a few room groups using a separate material, not a facade-wide dot shader.

The first source pass remains preserved. Its buttress knee's roll placed part of the beam above the cap. Pass 02 corrects the beam basis throughout the kit, preserving the design while keeping the full support below its ledge. It also separates the workshop in the studio view.

All three final FBXs pass Blender reimport: triangle counts, bounds within 0.003 mm, manifold edges, nonzero triangle area, UV0 and valid tangents. Detailed source/export hashes are in `asset-stats.json`; the roundtrip evidence is in `evidence/environment-v3/pass-02-transit/export-roundtrip-audit.json`.

`placement-contract.json` records precise proposed transforms and an all-course horizontal clearance audit. `integration-placement-audit.json` reproduces NightDistrict's reserve order and predicts all 11 major placements will be accepted. The conservative minimum across the full integrated footprints is 21.245 m from the centerline. The exact asymmetric station mesh rectangle has 21.445 m. A 12,000-point course audit subtracts the maximum sample spacing; vertical separation does not excuse lateral intrusion.

NightDistrict now loads these exports with Unity metadata, creates grounded station supports/service level, reuses the existing terrace and split towers at uniform scale, and reserves local skyline gaps. It removes the old final-sector giant plinth and inner works, and moves or excludes generic blocks that conflict with the new composition. No new props colliders or local lights are added.

Status: integrated for the parent's next native build. One honest Cycles studio preview exists; actual camera readability, instantiated bounds/orientation, passage motion and performance remain pending native validation. This is not a visual acceptance claim.
