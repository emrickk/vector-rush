# Independent ship review 002 — V3 form pass 01 rejected

Reviewer: Astra. Date: 2026-09-07. Scope: **Gate 1 primary form and visible Gate 2 construction**, using clay and simple neutral materials. **Gate 1: reject. Gate 2: unresolved/reject for integration. Overall hero/AAA status remains rejected.** Textures, livery, final material finish and game lighting are outside this early form verdict; their absence is not the reason for rejection.

## Revision and direct evidence

Reviewed the preserved staged revision [Kestrel07-v3.blend](../../SourceAssets/hero-v3/pass-01/Kestrel07-v3.blend), represented by these actual rendered views:

- [01 clay rear quarter](../../evidence/ship-v3/pass-01/01-clay-rear-quarter.png) and [02 neutral rear quarter](../../evidence/ship-v3/pass-01/02-neutral-rear-quarter.png).
- [03 clay front quarter](../../evidence/ship-v3/pass-01/03-clay-front-quarter.png) and [04 neutral front quarter](../../evidence/ship-v3/pass-01/04-neutral-front-quarter.png).
- [05 clay chase](../../evidence/ship-v3/pass-01/05-clay-chase.png) and [06 neutral chase](../../evidence/ship-v3/pass-01/06-neutral-chase.png).
- [07 clay top](../../evidence/ship-v3/pass-01/07-clay-top.png) and [08 clay side](../../evidence/ship-v3/pass-01/08-clay-side.png).
- [09 rear perspective](../../evidence/ship-v3/pass-01/09-C-perspective.png), inspected after the orthographic set. It confirms the same beam-like nacelles, square intake wells and wedge canopy. It is a useful close inspection view, but its 4:3 framing and much larger ship coverage do not match reference C's 16:9, roughly 40%-width composition.

Read the [capture contract](../../evidence/ship-v3/pass-01/capture-contract.json), [asset manifest](../../SourceAssets/hero-v3/pass-01/asset-stats.json) and [topology audit](../../evidence/ship-v3/pass-01/topology-audit.json). The front/rear/chase/top views use the established orthographic inspection cameras; they are not gameplay captures. Neutral white studio lighting and no bloom keep the form judgment separate from cinematic presentation. Reference C remains the primary target.

Identity at review: source SHA256 `cf8849e318aba28c1d2b751eb114a4f1a425b9bdd3f79038f16f1a32402862b0`; staged FBX SHA256 `b9912422b62a3ceb29decae5d76d4fa1a030a3a472a8ec209cec85200298665b`; recipe SHA256 from the manifest `722d027fa7f2a4836ada84d052cc11ac5eb9e5b63b1a1459f34a64f9e8f1b654`. Manifest bounds: 5.14 × 6.885 × 1.515 m; 21,176 triangles. These are revision identifiers and technical context, not quality scores.

## Improvements to retain

The large black slab fins are gone. The craft has a lower, more unified envelope. The canopy/frame is narrower and less bulbous than V2, and the dark center now extends into the rear spine. The white nacelle shell surrounds each engine mouth rather than ending in front of a separate-looking round can. Rear chase shows dark physical chamber depth with a subordinate core/ring; it is easier to read than V2's predominantly turquoise tubes. The old puffy intake grab rails are absent. Structural crossmembers now look more attached than the old loose pipe ends.

Those are useful directional changes. They do not establish convincing primary surfacing. Pass 01 has overcorrected from swollen toy forms into a coarse chamfered-box blockout.

## Gate 1 failures

### 1. The nacelles read as long beams, not authored propulsion housings

The side view is decisive: most of the upper and lower silhouette runs almost straight, with a large uninterrupted side band. The top view reads as two parallel planks with only a short forebody taper. The rear and front quarters show broad flat roofs, abrupt shoulder bevels and blunt octagonal ends. The main panels subdivide that extrusion without changing its underlying form. This is a primary shape failure, not a lack of screws or textures.

Reference C combines controlled flat areas with gradual changes in volume and contour. The next pass needs a shaped aft engine shoulder, a deliberate waist around the cockpit, a lower/slimmer forebody, and a lower return whose depth/chamfer changes along the nacelle. The plan silhouette and side profile must both improve. Merely adding more segments to the same straight section will not fix the result.

Keep a broad crown and intentional hard shoulder breaks. Add restrained compound shaping between those breaks so highlights roll over a manufactured shell. Do not globally inflate the body or apply a larger bevel to every edge.

### 2. The canopy remains a coarse solid wedge

The canopy is lower, but the neutral front/rear/chase views show a few large straight facets and abrupt rectangular reflection patches. Its shape still reads as a wedge placed on the fuselage. The side view confirms that the height is already sufficiently restrained; lowering it again is not the correction.

Give the canopy a shallow convex roof and a controlled roof-to-side transition that meets the narrow sill cleanly. Preserve the long dark center and tapered rear spine. The test is continuous, intentional highlight behavior and a credible glazing volume without returning to the teal bubble of V2. This can be established in gray clay and simple dark glazing.

### 3. The intake is visibly a small rectangular well in a large flat deck

In quarter views the mouth appears almost square with straight vertical walls. The top view reveals three bars deep inside a rectangular pocket, but the surrounding crown does not participate in the intake form. The feature looks cut from a primitive block.

Replace it with a longer swept opening and a ramped/recessed section integrated into the changing crown. Give the lip a narrow, controlled edge and a readable rear transition. Solve the opening's large shape first; do not rebuild the V2 raised white rails or fill it with many decorative vanes.

## Gate 2 construction concerns

- The major transverse panel boundaries read as deep cross-cuts, especially in top and side views. Their local offsets produce wedges/gaps instead of a consistent manufactured joint. Resolve the panel ends and provide a clean supported seam/return. This does not require a complete tertiary panel network.
- The rear mouth now has a useful integrated outer shell, but its octagonal sleeve and thin trumpet remain visibly separate primitive forms. Shape the shell-to-collar transition and its return so the engine sits in a designed housing. Preserve the improved dark cavity/core hierarchy.
- The new crossmembers and rear mounts are directionally better. Their rectangular blocks and broad empty straight channels still read as construction placeholders. Establish a few coherent mount profiles that meet the shaped inner walls; keep the channel open. Fine clamps, cabling and fasteners may wait until the primary surfaces are approved.
- The worker reported eight nonmanifold edges per nacelle core shell and tangent-export warnings. The saved audit lists single-face boundary edges at the rear rim, so technical integration remains held. The worker attributes this to a thin return collapsing during bevel/boolean operations and plans a thicker return plus explicit runtime triangulation. That causal diagnosis is the worker's; this review did not rerun the mesh audit. Require a clean follow-up audit and a visually intact exported mouth before clearing integration. A topology repair by itself will not pass the visual gate.

## Bounded pass 02 correction

Retain this low twin-nacelle layout and the improved dark engine presentation. Make one coupled geometry revision with four targets:

1. **Compound nacelle form:** authored aft shoulder, cockpit-area waist/inner-wall relief, low shaped forebody and changing underside/return. Preserve engine scale and the overall integration envelope where feasible.
2. **Integrated intake:** long swept mouth, ramped depth, narrow lip, clean crown transition.
3. **Canopy surfacing:** restrained convex crown and side transition, with the existing low height and narrow sill; clean connection into the central spine.
4. **Supported construction:** resolved major seams, shell/collar return and mount profiles, plus the known topology/tangent-export repairs.

Submit the same clay rear/front/chase/top/side views and the neutral rear/front/chase set. Preserve the existing rear perspective for before/after inspection, and add a 16:9 reference-C comparison with the whole ship at roughly 40% frame width; record the focal length as well as the camera pose. Preserve pass 01 source, export, audit and images. A new mesh audit should identify the revised source/export explicitly.

Pass 02 can clear Gate 1 only if it reads as a convincingly shaped machine in the side and top views as well as the flattering rear quarter. No texture, weathering, race-number or bloom work is needed to satisfy this correction. Gate 2 and later material/native/motion gates remain separate; none receives a pass from these stills.

## Evidence limits

No V3 native integration or motion was inspected. The parent's first native V2 neutral-rig attempt had a reported pose/lighting calibration failure and is excluded from material judgment. The present decision rests on the actual V3 studio images above. The rejection is specific to visible shape and construction, not a prediction that this design cannot reach the target.
