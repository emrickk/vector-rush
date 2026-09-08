# Environment review 012: native landmark candidate

Independent visual critique, 2026-09-08. **Keep both landmarks and the quieter skyline. Make one local composition correction before closing this pass: remove the large blank slab immediately beside the first-sector mast.** The left-side thermal plant is visually viable in the inspected approaches and close pass. No landmark rebuild, relocation or additional asset is recommended.

## Evidence and scope

Directly inspected fifteen original 1920 × 1080 images from [landmarks candidate 01](../../evidence/landmarks-candidate-01/full-lap/environment-evidence.json): ten landmark frames and all five final-station anchors. Compared them with fifteen original baseline images: eight from the [preserved landmark selection](../../evidence/landmarks-baseline/selection-manifest.json), frames [166](../../evidence/close-race-final/full-lap/frames/frame-0166.png) and [193](../../evidence/close-race-final/full-lap/frames/frame-0193.png) from that selection's original source, and the five anchors from [the previous final full-lap capture](../../evidence/close-race-final/full-lap/environment-evidence.json).

The candidate completed all 1,440 frames from 17:22:42 to 17:24:38 UTC. Its metadata identifies Unity 6000.6.0f1, Apple M2 Max and build GUID `827452d9244a421b827bd5a4c90a85fa`, with 1,247 scene renderers and 101 active lights. [The native log](../../evidence/landmarks-candidate-01/native.log) records both landmark resources loading at the adopted right-mast and left-plant world positions.

This sequence covers 60 simulation seconds using automated steering, normal hover physics and a 24 fps simulation capture rate. Only the fifteen identified candidate pictures were visually inspected. All ten landmark frames show a grounded player without boost; the final-station reveal shows boost. No continuous playback, audio, human driving, other aspect ratios or performance measurement was reviewed here.

The comparisons are close camera references, not identical-pose controls. At the ten landmark frame indices, camera distance from the baseline is approximately 0.196–0.245 m, maximum absolute progress difference is 0.000286, and maximum field-of-view difference is 0.00761 degrees. The report marks all five station anchor comparisons outside its matching tolerance. Its reveal is frame 1005 rather than baseline frame 1004, with a 2.439 m camera difference. These differences permit a composition review; they do not support pixel-level material or shadow conclusions.

## Mast: the new form works; its competing slab remains

Candidate frames [148](../../evidence/landmarks-candidate-01/selected/frame-0148.png), [166](../../evidence/landmarks-candidate-01/selected/frame-0166.png) and [175](../../evidence/landmarks-candidate-01/selected/frame-0175.png), at progress approximately 0.15–0.18, establish a recognizable pair of uprights, a gap of sky and an offset enclosed room. The lower, less busy background allows the structure to read independently. The road curve, course gate and player remain easy to locate. This is a visible improvement over the baseline's repeated window towers.

At [193](../../evidence/landmarks-candidate-01/selected/frame-0193.png), the larger close view retains the room, split and supports; by [211](../../evidence/landmarks-candidate-01/selected/frame-0211.png), it naturally crops off the right edge. That later crop does not erase the earlier recognition window. The supports descend behind the barrier, although the actual ground connection is not exposed clearly enough in these views to certify a finished foundation treatment.

**One required correction:** the huge stepped dark-blue slab with two pale horizontal strips still occupies the right side of frames 148, 166 and 175. It consumes more screen area than the new mast and preserves the most conspicuous blank mass identified in [direction review 011](011-landmark-direction.md). Remove this slab rather than increasing landmark size, brightness or detail.

The integration owner has identified it as a `BenchmarkSkyline` placement at local `(−226, 0, 467)`, height 113 m, transformed to world `(333.658, 0, −45.411)`. The owner reports projected centers of `(1129, 434)` in frame 148 and `(1731, 486)` in frame 175, consistent with the visible target. It is a different object from the previously removed first-district plinth. This object identification is attributed to the owner's source/projection audit; the visible composition problem is independently confirmed by the images.

## Thermal plant: accept the adopted left placement

Frames [534](../../evidence/landmarks-candidate-01/selected/frame-0534.png) and [570](../../evidence/landmarks-candidate-01/selected/frame-0570.png), around progress 0.50 and 0.54, introduce the cylindrical group and a broad service podium on the left. Their rounded tops and horizontal pipework give the middle sector a different identity from the first mast. The reduced right-hand skyline supplies useful visual balance.

At [606](../../evidence/landmarks-candidate-01/selected/frame-0606.png), around progress 0.58, the three staggered drums, pipe supports and shared platform are legible. The tallest top is already cropping near the upper edge after the earlier introduction. The podium provides a clearer structural base than another freestanding window tower, even though its lowest facade remains very dark.

At [641](../../evidence/landmarks-candidate-01/selected/frame-0641.png), the close cylinder and pipes stay outside the visible barrier. They obscure some background structure, but the current driving surface and next viaduct remain readable. At [678](../../evidence/landmarks-candidate-01/selected/frame-0678.png), the plant has passed out of view and the tightening bend is clear. The observed overlaps do not justify moving the plant. This is visual placement acceptance, separate from the author's course-wide clearance audit.

The thermal structure passes behind part of the top-left HUD during the approach and close pass; position, lap and rival text remain readable in these sampled views. Neither landmark introduces a visible road obstruction or a HUD legibility blocker in the inspected images.

## Retained passage and close boundary

The five station references remain coherent: [approach](../../evidence/landmarks-candidate-01/full-lap/01-approach.png), [entry](../../evidence/landmarks-candidate-01/full-lap/02-entry.png), [middle](../../evidence/landmarks-candidate-01/full-lap/03-middle.png), [exit](../../evidence/landmarks-candidate-01/full-lap/04-exit.png) and [reveal](../../evidence/landmarks-candidate-01/full-lap/05-reveal.png). Warm gallery construction, the near workshop and the station canopy retain their roles against the quieter skyline. No second composition correction is warranted from these views.

Preserve candidate 01 and this review. After removing the identified slab, check the first-sector approach references and retain a station reference for context, then close the landmark milestone and proceed to the separate road-finish pass. Simplified landmark surfaces, weakly exposed foundations, repeated distant architecture and existing broad road bands still limit the overall finish. They are not a request for another asset cycle here, and this milestone should not be described as AAA visual completion or continuous-motion acceptance.

Only this review document was authored. No runtime source or assets were edited.
