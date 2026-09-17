# SSR01 OFF versus retained road-response control

Verdict: **broad static scene/material preservation passes across the nine inspected natural crossings.** This is a visual comparison of actual original PNGs, not pixel equality, an exact-pose comparison, a performance result, or watched-motion acceptance. SSR01 ON is outside this review; its separately reported probe RenderGraph errors already disqualify it.

## Evidence inspected

Viewed all nine original 1920 × 1080 OFF PNGs under `evidence/nocturne-v2/ssr-01/off/selected`, alongside the eight original PNGs in `evidence/night-production/road-response/control/selected` and the warm control `evidence/night-production/road-response/control/full-lap/03-middle.png`. Read `evidence/nocturne-v2/ssr-01/control-selection.json` and `off/capture-validation.json`. The OFF build GUID is `47944ad3754d45adb51c354c3251c518`; the retained control GUID is `40bed5f53c2449418e7fb56bf59739f6`.

The OFF validation metadata reports native completion, 1,440 validated PNGs and unchanged source/app files. This review uses that metadata for provenance; it does not independently reproduce the integrity checks. Metadata retains the original natural-crossing poses and marks every comparison as failing exact-pose tolerance. In particular, OFF frame 1004 compares with retained control frame 1005, not a nonexistent control selected frame 1004. The control-selection file also contains older deltas against an earlier baseline; the table below uses the OFF validation deltas against the retained control selections.

## Observations

| OFF / control view | Appearance observed |
|---|---|
| 149 / 149 | Banked open-city climb preserves dark blue road plates, subdued cool light pools, cyan rails, warm isolated lamps, distant window hierarchy and white craft. Small camera-dependent edge shifts are visible. |
| 211 / 211 | Crest approach preserves the same road brightness, tower mass, horizon haze and craft material response. HUD time differs by 0.01 seconds. |
| 283 / 283 | Descending city view retains the pink/warm cast on the craft, road tint, cool skyline and industrial cylinders. No broad material or exposure shift is apparent. |
| 398 / 398 | Tight bank at the cool gallery approach retains the road shading, dark structural ribs and cool interior light pools. Speed display differs (114 versus 113); this is not a frozen identical state. |
| 473 / 473 | Gallery exit retains cool wall and craft highlights, dark road seams and city-window contrast. Near foreground edges shift slightly. |
| 607 / 607 | Thermal cylinders retain their smooth bright metal highlights, dark pipes, blue inset and restrained road lighting. Exhaust shape differs slightly in these sampled instants. |
| 642 / 642 | Thermal exit retains pipe specular highlights, bright craft top surfaces, subdued road and warm/cool windows. No broad glossy or flat-material replacement is visible. |
| 1004 / 1005 | Boosted finish approach retains material palette, broad road highlight, warm station windows and distant city depth. This has the largest pose discrepancy: the nearby right lamp and left station shift substantially. Read only as broad appearance evidence; no local pixel-change attribution is valid. |
| 943 / warm 03-middle | Warm gallery preserves amber wall pools, strip brightness, pale amber craft, cyan rails, road grain and plate seams. No broad warm/cool balance change is apparent. |

A **Development Build** watermark appears at bottom right in every OFF sample and is absent from the retained control. It is a visible overlay difference, not evidence of a scene-material regression. Neither set is visually reworked here; no crops, retouching, alignment, rescaling or difference composites were written.

## Retained pose deltas

Absolute camera/player distances and camera/FOV angles; signed route progress and race-time differences are OFF minus control, as reported by capture validation.

| OFF → control frame | Camera m | Camera ° | FOV ° | Player m | Progress Δ | Race s Δ |
|---|---:|---:|---:|---:|---:|---:|
| 149 → 149 | 0.161056 | 0.089799 | 0.000122 | 0.000000 | +0.00000000 | +0.000000 |
| 211 → 211 | 0.160905 | 0.093055 | 0.000046 | 0.479337 | +0.00025964 | +0.010000 |
| 283 → 283 | 0.163487 | 0.076465 | 0.000069 | 0.489817 | +0.00026456 | +0.010000 |
| 398 → 398 | 0.106394 | 0.047634 | 0.001846 | 0.309443 | +0.00016809 | +0.010000 |
| 473 → 473 | 0.158213 | 0.030633 | 0.001495 | 0.000000 | +0.00000000 | +0.000000 |
| 607 → 607 | 0.162579 | 0.019862 | 0.000038 | 0.488678 | +0.00026488 | +0.010000 |
| 642 → 642 | 0.163128 | 0.028950 | 0.000008 | 0.000000 | +0.00000000 | +0.000000 |
| 1004 → 1005 | 2.549403 | 0.276544 | 0.111565 | 2.679151 | -0.00145942 | -0.039993 |
| 943 → 943 | 0.160860 | 0.064594 | 0.000282 | 0.485006 | +0.00029528 | +0.009998 |

The eight close comparisons have camera displacement about 0.106–0.163 m; the finish comparison is 2.549 m, with 2.679 m player displacement. The visual evidence supports using SSR01 OFF as a broad baseline-preservation check. It does not establish exact-pose equivalence, temporal stability, reflection quality, frame-time acceptability, or SSR02 acceptance. Fresh SSR02 OFF/ON evidence still needs its own review.
