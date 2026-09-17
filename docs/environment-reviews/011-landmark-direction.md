# Environment review 011: landmark placement and trackside hierarchy

Independent direction review, 2026-09-08. **Give the first sector a narrow split mast on the right and the middle sector a wider industrial silhouette on the left, with clear space around each. Remove the existing competing slabs before adding detail.** The art author has adopted the left-side thermal plant after a course-wide clearance audit. These are placement facts and proposals against the current build, not acceptance of the new assets.

## Evidence and camera references

Directly inspected twelve original 1920 × 1080 frames from the [current final full-lap capture](../../evidence/close-race-final/full-lap/environment-evidence.json): first-sector frames **122, 148, 175, 202, 229 and 256**; middle-sector frames **534, 570, 606, 641, 668 and 691**. The completed sequence contains 1,440 frames covering 60 simulation seconds, recorded on Unity 6000.6.0f1, Apple M2 Max, build GUID `7da2883ade4f4223b7a611db8cf60eb6`, from 10:02:16 to 10:03:57 UTC. It uses automated steering and normal hover physics at a 24 fps simulation capture rate.

Only the twelve identified pictures were visually reviewed for this direction. All twelve show a grounded player without boost. This is sampled camera evidence; it does not establish continuous motion quality, performance, audio, or human driving feel.

| Current native reference | Track progress | Camera position, world XYZ, rounded to metres | Composition purpose |
| --- | --- | --- | --- |
| [148](../../evidence/close-race-final/full-lap/frames/frame-0148.png) | 0.1497 | (235, 45, −154) | Introduce the mast beyond the outside barrier; clear the large right-hand slab. |
| [175](../../evidence/close-race-final/full-lap/frames/frame-0175.png) | 0.1799 | (257, 50, −104) | Read the complete fork and offset skyroom before the close pass. |
| [202](../../evidence/close-race-final/full-lap/frames/frame-0202.png) | 0.2101 | (261, 54, −50) | Keep a recognizable portion and a grounded base as the camera climbs. |
| [570](../../evidence/close-race-final/full-lap/frames/frame-0570.png) | 0.5402 | (−67, 40, 283) | Introduce the thermal plant beyond the middle-sector gate. |
| [606](../../evidence/close-race-final/full-lap/frames/frame-0606.png) | 0.5805 | (−129, 47, 245) | Read the staggered drums, shared podium and broad overall shape. |
| [641](../../evidence/close-race-final/full-lap/frames/frame-0641.png) | 0.6194 | (−178, 54, 194) | Retain identity during the close pass without merging into the nearby viaduct. |

The full report records precise transforms and rotations. These references have a field of view near 69.6 degrees; the camera rises substantially through each approach. Use the recorded camera views when judging scale, rather than a free editor camera.

## First sector: split communications mast, driver’s right

The [early approach](../../evidence/close-race-final/full-lap/frames/frame-0122.png) leads into an open left bend. Frames 148 and 175 contain a very large dark blue slab on the right, with broad pale horizontal strips and little readable depth. Its size consumes the intended landmark space. The distant field of window towers supplies additional competing verticals.

Place the mast outside the bend around progress **0.22–0.24**, introduced from **0.15–0.18**. First remove or move back that near slab, then clear the closest tower silhouettes directly behind the fork. The split needs a visible piece of sky through it; a bright line on an otherwise merged mass will not give it a distinct shape. Keep the offset skyroom beside the track rather than suspended across the driving line or visually attached to the course gate.

Frame 175 is the principal full-silhouette reference. Some cropping during the closer frame 202 is natural, but the visible portion must retain the split, lean or offset room, with a support or service podium establishing how the structure reaches the ground. Check the [crest at 229](../../evidence/close-race-final/full-lap/frames/frame-0229.png) and [departure at 256](../../evidence/close-race-final/full-lap/frames/frame-0256.png) for a clear road and gate silhouette. Avoid concentrating the distinguishing feature behind the upper-right timer.

## Middle sector: staggered thermal plant, broad and grounded

The [approach at 534](../../evidence/close-race-final/full-lap/frames/frame-0534.png) already has an oversized window tower on the right. Frames 570 and 606 provide the useful introduction and recognition window before the tighter bend. Prune that nearby window mass and competing right-hand towers so the cylinders have their own outline.

**The adopted placement is inside-left at progress 0.625, with a lateral offset of −51 m**, alternating the visual weight from the first mast. The art author reports that 12,000 samples across the whole course establish a minimum centerline clearance of 25.306 m at progress 0.65042. The crossing viaduct visible at 606, 641 and [668](../../evidence/close-race-final/full-lap/frames/frame-0668.png) remains a visual-overlap check for the native candidate; this sampled-frame review does not independently certify that view.

Make the plant visibly wider and lower than the mast: staggered drum tops, gaps between cylinders and a shared horizontal service podium. Keep the opposite right side quieter. Connect the podium to recognizable service construction below or beside the elevated road; do not let its dark base merge indistinguishably with the viaduct. At the [tight bend at 691](../../evidence/close-race-final/full-lap/frames/frame-0691.png), preserve the barrier, next turn and existing track structure as the primary foreground shapes.

## Scale and integration risks

The art author’s placement audit reports a 123 m mast at progress 0.230, 56 m to the right. The adopted plant is at progress 0.625, 51 m to the left, world position **(−155.881, 0, 148.334)**, with reserved footprint half-extents **23 × 43 m**. Its maximum height is **91 m total, including the 32 m podium**, not 91 m above the podium. The author also reports projected roof positions of 115 px at progress 0.55 and 65 px at 0.58. These are reported source and projection results, not independent native visual acceptance. Verify the intended lower, wider appearance and viaduct relationship from frames 570 and 606 without enlarging the plant to force visibility.

Preserve the final station passage, current camera, HUD, race pace and road finish while assessing this silhouette pass. Give each landmark a foreground connection, a readable middle-distance form and a quiet background. Additional window grids, signs and bright trim would work against that hierarchy.

One native candidate review should revisit the approach, full-form and close-pass references for both landmarks, plus the retained final station. Acceptance should depend on recognizable different silhouettes, removal of the obstructing slabs, readable bases and clear driving space. Report any remaining sampled-view limitations explicitly. Continuous playback is not a prerequisite for this bounded still-art review; it also cannot be claimed as reviewed from the pictures. Road-surface work can follow the accepted composition pass as a separate change.

Only this direction document was authored. No runtime source or assets were edited.
