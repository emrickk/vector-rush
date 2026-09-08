# Pass03 triangle-count reconciliation

The staged FBX has 38,036 triangles; the native inspection has 37,686. The 350-triangle difference is fully accounted for at the same FBX hash. No app or render was launched, and no geometry changed.

| Runtime mesh | FBX | Native | Difference | Exact degenerate | After float32 cm |
|---|---:|---:|---:|---:|---:|
| Ceramic | 8468 | 8138 | 330 | 324 | 330 |
| Engine | 1920 | 1900 | 20 | 8 | 20 |
| EngineCore | 468 | 468 | 0 | 0 | 0 |
| Glass | 1046 | 1046 | 0 | 0 | 0 |
| Graphite | 9034 | 9034 | 0 | 0 | 0 |
| Ivory | 10564 | 10564 | 0 | 0 | 0 |
| Metal | 6536 | 6536 | 0 | 0 | 0 |

Direct binary parsing found 332 triangles whose vertices already occupy duplicate positions. Rounding the FBX centimeter coordinates to float32 collapses 18 more. The resulting 330 Ceramic and 20 Engine degenerates match the native reductions exactly; all other material meshes retain their triangle totals.

All seven model names appear in the native MeshFilter inventory, and native totals come directly from triangle submesh index counts. Current importer settings include vertex welding, mesh optimization, no compression and no generated LODs.

The evidence strongly supports import-time removal of degenerate triangles. The exact importer step was not traced; the float32 conversion is an emulation that reproduces every per-mesh count. See triangle-reconciliation.json for identity, method and limitations.
