# Independent final coastal integration critique

Reviewed the actual native images in `evidence/run-09-coastal`: title, start, crest, city descent and the dedicated coast inspection. Also reviewed `AuthoredCity.cs`, `CoastalSetDressing.cs`, the asset dimensions/export notes and `docs/visual-modeling-review-03.md`. This review concerns coastal placement and final visual integration, not a new gameplay audit.

**Verdict: acceptable visual integration for the racing prototype in the inspected normal chase views. No new release-blocking visual defect is demonstrated in those views. AAA visual quality still fails.** The improvements are substantial relative to the earlier primitive scene, but the remaining terrain, water and waterfront treatment is visibly below the concept target.

## What the native evidence establishes

- **The road is coherent again.** The severe crumpled/grid-like reflection artifact is absent in the inspected start, crest and descent. The dark road still has ordinary broad shadows and joints. Source confirms a dedicated Lit `RoadSurface` with environment reflections and specular highlights disabled; the ship and architecture retain their separate materials. This validates the visible outcome in these captures. It does not establish a unique underlying engine/shader cause or prove that SSAO alone caused the earlier defect.
- **The ship survives the actual chase view.** Layered pontoons, open structure, recessed luminous engine rings and tapered exhaust are readable. It has a much clearer material and propulsion hierarchy than the earlier fan-like engines and flat trails. This review does not replace the detailed asset review of small geometry defects.
- **The towers now share substantial foundations.** The start and crest show a continuous quay with a deep seawall, common paved area, perimeter circulation and low service structures. These are a concrete improvement over isolated thin building platforms. Tower silhouettes, setbacks and glazing remain readable against the sea and sky.
- **The scanned cliffs are integrated.** The crest and descent show textured rock faces, clefts, shelves and a less uniform outline. No missing material, obvious floating cliff or direct road penetration is visible in the inspected race frames. That is a view-limited observation, not an exhaustive intersection certification.

## Placement findings and correction

The tower dimensions fit comfortably within the generated quay footprints. Quay and jetty placement tests cover their horizontal extents with additional course clearance; no clear source-level building/course intersection was found.

The pre-final placement arithmetic produced two accepted harbor cliff centers only about 1.48 metres apart, near world X/Z `(11, -47)`, despite their roughly 88 × 61 metre unscaled footprints. This was a real near-duplicate placement, not merely a speculative texture complaint. Final source adds a 40-metre minimum accepted cliff-center spacing, which rejects that near-duplicate. Some edge overlap between geology modules can be intentional.

Remaining placement checks use fixed center-distance/padding thresholds rather than every transformed cliff vertex. The harbor quay padding is 40 metres while rotated/scaled cliff extents can exceed that. Current images do not reveal a blocking collision, but the placement code alone is insufficient to certify every shoreline contact. The quays also still read as reclaimed islands beside rock masses in several views; the evidence does not establish a fully connected natural waterfront.

## Remaining visual limitations

1. **Cliff shape remains the largest environment weakness.** The descent gives a close view of broad upright planes, a softly undulating plateau and repeated fine texture. Texture detail cannot replace large-scale erosion, varied silhouettes and believable broken shore transitions. The cliff was accepted for background/middle distance; this framing exposes it more closely than ideal.
2. **The waterfront remains sparsely resolved.** Shared quays establish scale and support, but large pale empty decks and simple block service structures still look like a prototype. The water's repeated soft ripples and plain horizon reduce depth and coastal realism.
3. **Materials need more separation.** Tower glazing remains very dark, and broad architectural faces lack the nuanced reflected light and surface variation of the reference. These are production-quality gaps, not newly demonstrated functional failures.

## Dedicated inspection limitation

`07-coast-inspection.png` is explicitly a separate native inspection camera, not a normal race frame. That camera is obstructed by nearby architecture: a broad pale surface covers most of the lower image and a dark overhead surface covers much of the upper image, leaving only a narrow view of the cliff. It is not useful evidence for a full coastal close inspection and must not be advertised as such. The normal chase images listed above remain valid. An unobstructed elevated three-quarter view would close this evidence gap.

The final claim supported here is a materially improved, visually integrated racing prototype with the earlier severe road artifact absent from the inspected views. A production or AAA visual pass is not earned.

## Follow-up: unobstructed coastal inspection

Directly inspected `evidence/coast-inspection-10/07-coast-inspection.png`, the replacement elevated native three-quarter camera. The cliff is now fully visible, with its top, front/side faces, erosion clefts, small scrub groups and shoreline fragments readable. This closes the obstructed-camera evidence gap described above; the original run-09 image remains preserved as a failed inspection attempt. This is a dedicated inspection view, not a racing-camera frame, and it does not replace the normal chase evidence.

The new view confirms the rock texture is applied and the imported module renders coherently. It also confirms the previously recorded limitations: large smooth upright planes, a soft plateau outline, sparse vegetation and weak visual contact with the water. The ocean retains conspicuously repeated wave bands. No missing material or new severe cliff geometry failure is demonstrated in this view. The bounded prototype integration verdict stands, and the AAA verdict remains **FAIL**. No further modeling scope is added by this follow-up.
