# SSR02 native OFF / ON visual review

**Reject SSR02 as demonstrated production visual feasibility.** The nine whole-frame comparisons do not show a convincing reflection improvement. They preserve the general scene well, but the road still reads mainly as dark plates with broad lighting pools. There is no clearly legible new reflected lamp, rail, architecture or craft feature that would justify accepting the reflection approach from these samples.

This verdict concerns demonstrated visual benefit, not whether the pass executes. The parent reports clean logs, positive instrumented SSR markers ON and zero OFF. Those are useful execution evidence, but neither visible-gain evidence nor GPU timing. This reviewer inspected original images and capture metadata; it did not inspect source, settings, execution logs, motion, audio or performance.

## Evidence and comparison limits

Viewed all 18 original 1920 × 1080 selected PNGs in `evidence/nocturne-v2/ssr-02/{off,on}/selected`. Both capture-validation files identify build `f1a459e36c064afc930b841eede3d488`. Every frame has the Development Build watermark. No image edits, alignment or difference composites were made.

These are separate natural runs. The validation flags against the retained legacy control all fail exact-pose tolerance; those flags are not an OFF-versus-ON pose test. Direct camera/player distances below were calculated from the two current selections' recorded positions. They document that even same-index pairs are not frozen identical poses. Thermal selections intentionally differ: OFF607 pairs with ON606, and OFF642 with ON641. Do not substitute ON607/642 merely to obtain matching filenames.

| OFF / ON frame | Camera distance m | Player visual distance m | Whole-frame visual observation |
|---|---:|---:|---|
| 149 / 149 | 0.161016 | 0.161365 | Open bank retains cool road pools, cyan boundary and craft response. No clear new coherent road reflection. |
| 211 / 211 | 0.161006 | 0.159687 | Crest retains same dark road character. Nearby light/geometry shifts are visible; no persuasive reflection gain. |
| 283 / 283 | 0.163441 | 0.163167 | Warm-tinted craft and city descent remain similar. Road has no obvious new reflected structure or light pattern. |
| 398 / 398 | 0.105663 | 0.103079 | Cool gallery approach preserves structural darkness and wall light pools. No obvious reflection contribution or catastrophic artifact. |
| 473 / 473 | 0.158182 | 0.159551 | Gallery exit remains similar in lighting/material appearance; near rail and seam changes cannot be isolated from pose. |
| 607 / 606 | 1.870284 | 1.873208 | Thermal cylinders and pipe highlights remain strong; road gains no unmistakable reflective feature. Foreground placement differs substantially. |
| 642 / 641 | 1.877439 | 1.876358 | Bright craft, pipes and distant road appear materially consistent. Stronger/different dark shapes beneath craft are present, but changed pose and light relation prevent attributing them to SSR. |
| 943 / 943 | 0.160900 | 0.161570 | Warm gallery road and walls retain amber balance and visible grain. Minor local shading/edge changes do not read as a substantial reflection gain. |
| 1004 / 1004 | 0.221169 | 0.222723 | Finish straight retains broad cool road sheen and warm station windows. No clearly new lamp or window reflection is apparent. |

No large black output region, gross inverted reflection, obvious detached duplicate of the craft, or major exposure/color regression is apparent in these nine stills. This narrow observation cannot establish temporal stability, absence of flicker/ghosting, or behavior elsewhere on the lap. Minor local differences are not declared zero; their origin and value remain inconclusive because the samples have pose, time and exhaust differences.

## Bounded next diagnosis

Before another aesthetic tuning iteration, use a separate diagnostic capture of one representative road view (warm gallery 943 is a useful candidate) to show the actual road mask, valid reflection hits and final SSR contribution individually. That can distinguish a contribution that is absent, heavily rejected, or merely too weak to matter. Keep such diagnostic views separate from production acceptance imagery. If those buffers prove a useful coherent contribution, produce a fresh normal OFF/ON comparison and assess it at full frame before wider rollout. Do not infer the cause from these normal-color stills alone, or claim that a running marker proves the intended reflected scene reached the final image.

Current status: broad static scene preservation is acceptable; visual improvement sufficient for the production feasibility gate is **not demonstrated**. Motion, audio and performance remain unreviewed here.
