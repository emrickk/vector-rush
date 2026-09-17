# Gallery 02 Unity FBX axis diagnosis

Status: **RESOLVED AND VERIFIED BY FULL TECHNICAL IMPORT**

Unity 6000.6.0f1 rejected the first Gallery 02 import before publication. Package identity, course identity, checksums, layout, materials, and texture validation had passed. The first mesh, `NA_GalleryPortal_G02_LOD0.fbx`, then measured `Z -1.61..0.905` against sealed expected bounds `Z -0.905..1.61`, a maximum axis discrepancy of `0.705 m`. The failed report remains at `artifacts/integration/gallery-import-01/import-report.json`; its partial destination was retained during diagnosis.

The package declares source-to-Unity `(x,z,-y)` and exports FBX with `axis_forward: "-Z"`, `axis_up: "Y"`, and `bake_space_transform: true`. A bounded Unity probe reimported the retained FBX with both `ModelImporter.bakeAxisConversion` values and restored its original setting afterward:

| Unity setting | Measured Z bounds | Root transform | Result |
| --- | --- | --- | --- |
| `true` | `-1.61..0.905` | identity, unit scale | Fails the declared forward sign |
| `false` | `-0.905..1.61000013` | identity, unit scale | Matches the sealed package bounds |

Probe output is `artifacts/integration/gallery-axis-probe-01/axis-probe.json`; the Editor log records a clean batch exit. The importer now disables Unity's additional axis bake. It does not change scale, loosen the 1 cm tolerance, reinterpret the package bounds, or bypass any checksum or course-identity guard.

The retained failed staging destination was moved intact under the failed-attempt evidence before retrying. A fresh Unity process then completed the import. All 12 FBXs passed the unchanged bounds gate; the maximum measured error was `0.00000190734863 m`. The import created 19 placements and 20 lights, then published only `Assets/Resources/AAA/GalleryExemplar.asset`. The full post-import EditMode suite passes 164/164. Evidence is under `artifacts/integration/gallery-import-03`. Native visual and motion review remain separate acceptance gates.
