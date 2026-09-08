# Native fixture-caster A/B/A review

2026-09-08. **Adopt the limited fixture shadow-casting adjustment, subject to the final moving exterior check.** All three original native images show the intended causal result: the conspicuous L-shaped road band disappears when the road-fixture caster family is disabled and returns when its original casting modes are restored. This identifies the current pole-and-arm shadow contribution. It does not establish the cause of every polygon seen in earlier road experiments.

## Actual image result

I directly inspected the complete, original 1920 × 1080 [baseline](../../evidence/road-fixture-caster-01/00-baseline.png), [fixture-casters-off control](../../evidence/road-fixture-caster-01/01-fixture-casters-off.png), and [restored baseline](../../evidence/road-fixture-caster-01/02-baseline-restored.png).

In the baseline, a thick dark strip runs beneath the craft, bends near the right half of the road, and angles toward the right barrier. The control removes that recognizable L. Restoring the fixture casters restores the L in the same place. The thin transverse road seam crossing this area remains in all three images; it must not be mistaken for a failed shadow removal.

The visible streetlamp, glowing diffuser, road light pools, cyan barriers, vehicle silhouette, propulsion glow and surrounding buildings remain visible in the control. The foreground road reads more continuously without the false-looking obstruction shape. No obvious loss of vehicle grounding or district depth is apparent in this one view. This is a deliberate shadow-composition improvement; the evidence does not support remeshing the road or further lowering its roughness to fix this particular L.

## Capture identity and control checks

The [native report](../../evidence/road-fixture-caster-01/road-surface-evidence.json) and status report completion with no error. The build GUID is **`a1325c823e8b4541b20c143f42f12470`**, matching the corrected-landmark binary identified by the parent. Capture ran from 17:32:34 to 17:33:21 UTC. Target progress was 0.94585; the normal-physics testing driver reached **0.9462143**, at race time **41.958965 s**, before the rendered camera and racer poses were frozen. Reported pre-freeze speed was 240.69 km/h; the visible HUD displays 239 km/h.

The three exact, active renderer identities were resolved and read back:

| Renderer | Baseline | Control | Restored |
| --- | --- | --- | --- |
| Track lighting steelwork | On | Off | On |
| Cool linear road lamps | On | Off | On |
| Amber linear road lamps | On | Off | On |

All three remain enabled and active. Their recorded transforms, mesh identities and material identities match across views; only the casting mode differs. The recorded original and finally restored caster states equal both baseline states. The harness also reports `casterRestorationVerified = true`.

I independently compared the serialized view data. Camera position/rotation, race time/progress, road material properties and texture descriptions, road mesh/collider identities, enabled additional-light list, resolution, normal mode and sun-shadow control flag are identical. All **101 recorded light states** match. The moon remains enabled with soft shadows, intensity 0.62 and shadow strength 0.4. The road renderer and collider share the same recorded mesh identity; the setup retains 12,493 vertices, normals, tangents and UVs, and 23,040 triangles.

The source review confirms that caster mode changes only those renderer casting modes, creates no material control, and restores in both condition and outer cleanup paths. The metadata verifies identities and selected state, not a bytewise hash of every live mesh attribute. The one-pose controls isolate the family as a whole; they do not establish the individual contribution of each of its three members.

## Restoration and image validation

All three PNGs fully decode at 1920 × 1080 RGB. Their hashes, state comparisons and decoded-pixel measurements are preserved in [analysis.json](../../evidence/road-fixture-caster-01/analysis.json).

The baseline and restored PNGs are **not pixel-identical**. Their full-frame mean absolute channel difference is 0.0100 on the 0–255 scale. In a manually selected rectangle around the road band (x 780–1609, y 690–809), baseline-to-control mean difference is 0.4664, versus 0.0121 for baseline-to-restored. Respectively 9,609 and 103 pixels in that region differ by more than two levels in any RGB channel. The region includes some exhaust and is not a semantic road mask; these measurements support the visual restoration finding and do not replace it or prove the source of small temporal differences.

## Recommended scope and remaining check

Set shadow casting off only for the three named road-fixture renderers. Keep their visible meshes, emission and local lighting, along with the road material, collider, vehicle physics, moon shadows and other scene casters. The positive control makes the previously proposed triangle-flip follow-up unnecessary for this specific band.

Before declaring the road finish complete, inspect a short native moving exterior passage at ordinary speed and under boost on the shipping change. Confirm that useful light pools remain, the removed fixture pattern stays absent as the camera moves, and vehicle/district depth remains acceptable. This review covers three frozen native views at one progress value; it does not certify motion stability, all road boundaries, full-lap visual quality or performance. No runtime or asset changes were made by this review, and no Unity or native process was launched by the reviewer. The reviewer authored the diagnostic harness; this is a separate evidence analysis, not an independent implementation audit.
