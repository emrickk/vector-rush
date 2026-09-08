# Vector Rush — visual and modeling pass

The revised ship and two tower designs are visibly integrated in native run-06. The ship has readable open structural channels, layered armor and recessed luminous engines. Architecture now has modeled glazing, terraces, entrances and two distinct silhouettes. The severe road reflection artifact is absent in the inspected start, crest and descent images. This is a concrete improvement over the starting prototype; the scene still falls short of the requested AAA visual target.

## Delivered models

| Asset | Geometry | Review and integration status |
| --- | ---: | --- |
| Kestrel 07 hero ship | 73,364 triangles; nine material batches | Revised after rejecting the soft, toy-like first pass; integrated and inspected in native chase views. |
| Solstice TerraceTower A | 39,452 triangles; five material slots | Open garden floors, recessed facade, occupied podium and roof equipment; integrated. |
| Solstice SplitTower B | 22,916 triangles; five material slots | Unequal blades, deep separation and skybridges; integrated. |
| Solstice CoastalCliff C | 37,424 triangles; two material slots | Revised three times; scanned rock surface selected. Frozen and handed over for native evaluation. |

Editable Blender scenes, authoring scripts, material contracts and export evidence are in `hero-v2/` and `environment-v2/`. The engine anchor file keeps exhaust effects aligned with the remodeled nozzle exits. Source generators must retain these revisions when assets are regenerated.

## What the review changed

The first hero preview was held back because broad rounded trim, shallow intake bars and filled structural gaps hid construction. The accepted revision uses tighter armor edges, thinner recessed vanes, open channels, a defined canopy boundary and tapered housings around genuinely open engine cavities. Recessed luminous rings and throats remain visible from the actual chase camera.

The first cliff resembled stacked clay. The second resembled cut concrete. Subsequent passes introduced irregular headlands, erosion clefts, angular face relief, a wave-cut shelf and separate scrub geometry. The final material uses Rock 3 by Rob Tuytel / Poly Haven, licensed CC0: <https://polyhaven.com/a/rock_3>. Original source maps and provenance are preserved; the URP metallic/smoothness map is a named derivative. Texture detail improves the surface, but it does not resolve the large smooth vertical planes and simple upper silhouette. This cliff is accepted for background and middle-distance evaluation, not close hero shots.

## Native evidence already inspected

- `../Vector Rush/evidence/road-diagnostic-05/diag-01-baseline.png` and `02-start.png`: continuous road response and visible propulsion after rendering-resource, emission and trail corrections. These changes were combined, so the review does not assign sole causality to one fix.
- `../Vector Rush/evidence/run-06-models/02-start.png`, `05-crest.png`, `06-city-descent.png`: remodeled ship and towers survive actual gameplay lighting; the road no longer has the earlier crumpled reflective cells.

## Remaining visual priorities

1. **Build a believable waterfront.** Run-06 towers sit on isolated thin platforms in open water. Group them around connected shore, podiums, seawalls and access routes. Preserve the sweeping route views.
2. **Improve depth and surface separation in native lighting.** Tower glazing reads almost black, broad surfaces remain flat, and the water has a repetitive soft ripple pattern. The runtime owner is evaluating lighter glass and restrained ambient occlusion.
3. **Keep the cliff away from close hero framing.** Its broad upright faces and simple top still expose procedural construction. Distinct future cliff modules need more natural fractures and shoreline variation.
4. **Finish small hero details.** Aft number clipping, a few inset-edge defects and simplified mechanical connections remain visible in close inspection. They do not invalidate the chase-view improvement.

## Export traceability

- Hero FBX SHA256: `61f634392f52c054a152641f16770b10b3c6eab7d37f5323cb627d2c084ee035`.
- Tower A FBX SHA256: `33517d32cc55e46f8a377fc1da02ba47a5b8184fd523b8f6f2bbe8f242e41bff`.
- Tower B FBX SHA256: `b0883080681e9d844029c0433019f30c36646b66f173f6b80039bdb9d3e4765d`.
- Cliff C FBX SHA256: `a3ea2fdbcc89c8218a54a55fd07ad5128714f0de9ae3a982ca21f9e76a58cb8c`.

Tower B's recorded 52 zero-area cap triangles have no visible area; this known export cleanup remains documented. No frame-rate claim is made from offline studio renders.

## Final coastal integration review

Completed in native run09 and the corrected coast-inspection-10 view. The independent final review in `critique-coastal-final.md` accepts prototype visual integration: the deck is coherent, towers share substantial quays, the scanned cliff maps are present and the near-duplicate cliff was removed. AAA quality remains a failure. Broad cliff planes/plateaus, sparse quays, dark glazing and repetitive water are the principal remaining visual limits.
