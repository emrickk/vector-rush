# Road diagnostic 002: diagonal boundary survives material and local-light controls

Independent review, 2026-09-08. **The visible road defect remains unresolved.** All 12 matched native images were directly inspected. The large foreground-right diagonal polygon survives the calibrated material, zero normal scale, constant smoothness mask, and removal of every non-directional light. These are useful exclusions, not proof that the mesh is defective. The next single test should isolate the remaining directional-light shadow path.

## Evidence and matching

Reviewed the [native report](../../evidence/road-surface-01/road-surface-evidence.json), [capture harness](../../UnityProject/Assets/Scripts/RoadSurfaceEvidence.cs), [road construction](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs), and [earlier dampness diagnosis](../road-dampness-diagnosis.md).

The report records 12/12 complete 1920 × 1080 captures on Unity 6000.6.0f1, Metal, Apple M2 Max, between 07:59:11 and 07:59:56 UTC. Every view has the same recorded camera position, camera rotation, race time 35.3500748, and track progress 0.8847515. FOV is 70.06458°. Normal physics and the existing testing autopilot reach the gallery; the harness then freezes the rendered racer and chase-camera poses. It checks those poses before each image and restores the baseline material and light state between conditions.

This is one stationary diagnostic view. It does not test motion, other road segments, performance, or human driving. The restored image recovers the baseline appearance and recorded settings; the two PNG files are not byte-identical, so no pixel-perfect restoration claim is made. No missing-shader pink output is visible. Recorded shader support and keywords still do not certify every compiled variant.

## What the twelve images show

The boundary under review is the large cool polygon beginning near the right road edge above the speed dial and extending diagonally toward the lower center. The narrower longitudinal wedges nearer the craft also remain apparent. Ordinary transverse expansion joints are separate authored objects and should not be counted as this defect.

| Native view | Observation | Supported conclusion |
| --- | --- | --- |
| [00 baseline](../../evidence/road-surface-01/00-baseline.png) | Strong diagonal edge cuts through the broad cool road sheen. | Establishes the visible defect at this pose. |
| [01 constant smoothness, map disabled](../../evidence/road-surface-01/01-constant-smoothness-map-disabled.png) | Fine dampness variation disappears; the polygon remains, with a cleaner broad fill. | Removing the varying mask does not remove this edge. |
| [02 normal map disabled](../../evidence/road-surface-01/02-normal-disabled.png) | Road darkens substantially and the highlight tightens; a weaker polygon edge remains. | This altered shader combination does not provide a clean visual match to the zero-scale control. Do not interpret the brightness difference as proof of bad normal encoding. |
| [03 both maps disabled](../../evidence/road-surface-01/03-both-disabled.png) | Smooth broad lighting still has the same foreground polygon. | The simultaneous map-removal condition also fails to eliminate the boundary. |
| [04 calibrated simple Lit](../../evidence/road-surface-01/04-calibrated-simple-Lit.png) | No road textures; constant base color, metallic 0 and smoothness 0.56. The large polygon is especially easy to see. | Authored road texture content is not a sufficient explanation for the persistent shape. |
| [05 normal scale zero, original keywords](../../evidence/road-surface-01/05-normal-scale-zero-original-keywords.png) | Broad textured sheen and the diagonal boundary remain. | Setting normal perturbation strength to zero does not eliminate it in the baseline keyword combination. |
| [06 constant mask, original keywords](../../evidence/road-surface-01/06-constant-mask-original-keywords.png) | Damp streak variation drops out; grain and the polygon remain. | A spatially varying smoothness mask is not necessary for this boundary. This is the stronger mask control because it preserves the original keywords. |
| [07 no additional lights](../../evidence/road-surface-01/07-no-additional-lights.png) | Warm illumination and craft pools disappear; the cool diagonal road shape remains prominent. | Overlap or selection of the non-directional lights is not necessary for this edge. |
| [08 selected light 1](../../evidence/road-surface-01/08-selected-light-1.png) | The selected cool pool brightens the craft; the persistent road polygon remains. | This light does not uniquely account for the boundary. |
| [08 selected light 2](../../evidence/road-surface-01/08-selected-light-2.png) | The selected ceiling pool gives the craft warm illumination; the same road boundary remains. | Neither selected pool is established as the source. |
| [09 selected lights combined](../../evidence/road-surface-01/09-selected-lights-combined.png) | Mixed craft illumination returns without removing the road polygon. | Combining these two lights does not explain away the zero-additional-light result. |
| [10 baseline restored](../../evidence/road-surface-01/10-baseline-restored.png) | Baseline sheen, wall lighting and the same edge return. | Supports the comparison sequence and restoration of recorded settings. |

The selected lights were ranked by approximate attenuation at three road points. Their ranking is not a measurement of their rendered specular contribution. The zero-additional-light condition is the decisive local-light exclusion.

## What is still open

The earlier periodic-mask change addressed a real source texture discontinuity, but the surviving diagonal cannot be called fixed by that correction or by reduced smoothness contrast. The new constant-mask control strengthens that distinction.

Geometry and geometric normals were never varied. The actual runtime road has one submesh, 12,493 vertices with matching normal/tangent/UV counts, and 23,040 triangles. Source construction shares interior vertices across a 960-row, 12-column ribbon and recalculates normals and tangents. This argues against a deliberately flat-shaded road; it does not establish that every normal or interpolation result is suitable, nor does the polygon's appearance prove alignment with a particular mesh triangle. No runtime normal visualization or projected triangle-edge comparison was captured.

One enabled directional light, `Midnight soft key`, remains in every condition. It has soft shadows; the recorded pipeline enables a 4096 main shadow map and four cascades, and the camera renders shadows. Ambient illumination, screen-space AO and postprocessing also remain. Therefore “no additional lights” is neither “no lighting” nor “no shadows.” These paths have not been separated from mesh shading. A shadow-related boundary is a testable hypothesis, not an identified cause.

## Next single diagnostic

Repeat **07 → shadow-off → 07 restored** within one newly frozen native pose. Keep the original road material and its keywords, the road mesh, the camera and FOV, all racer poses, AO, postprocessing and ambient settings unchanged. Keep all non-directional lights disabled. Change only `Midnight soft key.shadows` from `Soft` to `None`, preserving that light's enabled state, direction, intensity and color. Record the changed setting and verify restoration in the report.

- If the diagonal disappears while useful directional illumination remains, the shadow path contributes to this boundary at that pose. That still does not identify the caster, bias, cascade behavior or mesh interaction responsible.
- If the same edge survives, disabling that shadow path is not sufficient. Then inspect the actual rendered geometric normals and their correspondence to triangle edges before changing tessellation or normals.
- If the control changes more than the intended setting or fails to preserve readable road lighting, treat the result as inconclusive and correct the diagnostic.

This is a diagnostic toggle, not a proposed shipping look. Do not suppress all light, disable specular response, retune the dampness mask, or rebuild the road in the same comparison. Preserve this failed material-isolation history whichever test succeeds next.

The separately locked [environment baseline manifest](../../evidence/environment-baseline-01/baseline-manifest.json) now records the five 0000/0170/0190/0210/0240 anchors and source hashes; its [264-frame report](../../evidence/environment-baseline-01/environment-evidence.json) includes camera transforms, FOV and racer poses. That resolves the earlier baseline-recording gap for subsequent passage comparisons. It does not change this road diagnosis or constitute acceptance of an environment art pass.
