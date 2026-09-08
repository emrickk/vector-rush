# Kestrel hero ship: production plan and visual gates

Status: **baseline rejected; hero quality is not achieved.** Independent Astra planning/review, 2026-09-07. The ship is the active art task. Environment work cannot compensate for defects in this asset.

## Target and scope

Use [reference C](../references/nocturne/C-craft-materials.png) as the primary ship target: broad pearl nacelle shells, a long dark central cockpit, legible structural gaps, layered manufactured construction, and dark engine chambers containing compact luminous cores. Use [reference B](../references/nocturne/B-amber-corridor.png) for the ship's response to warm overhead practicals and cool navigation light. The two concepts contain different engine/armor arrangements; do not merge their incompatible geometry. Preserve the original Kestrel identity while making C's coherent twin-nacelle construction the controlling direction.

Both references are generated concepts. Neither demonstrates implemented game quality, nor supplies an exact engineering blueprint. Dimensions and material ranges below are authoring starting points, not measurements of the concepts or objective proof of AAA quality.

The current V2 is approximately 5.265 m wide × 7.205 m long × 1.997 m high including its fins, according to its asset manifest. Keep the gameplay integration envelope stable unless a shape change requires a documented update. Judge visible proportions, not polygon count: the current 73,364 triangles do not prevent its toy-like appearance.

The [baseline review](ship-reviews/001-baseline-rejected.md) records the inspected evidence and failures. Retain that record after revisions. Each new review gets a separate numbered file identifying the exact exported revision and images reviewed.

## Ranked production work

### 1. Rebuild the primary hull, cockpit, and silhouette

This is the first bounded implementation item. Produce a new editable revision and a clay/neutral comparison before secondary detailing.

- **Nacelles:** replace the three-dart/surfboard reading with two substantial propulsion housings and one subordinate central fuselage. Carry believable shell volume into the rear engine housing; the current nozzle cylinders look appended to narrow rounded bodies. Use a broad controlled crown, an intentional shoulder break, and a distinct lower return. Avoid uniformly inflated, pillow-shaped cap transitions. Keep a tapered nose, but shorten the long needle-like tips enough that the large nacelle mass carries the silhouette.
- **Upper decks:** remove the oversized white grab-rail appearance around the intake. Recess the intake into the shell with narrow lips and readable depth. Establish a few large panel groups whose boundaries follow the shape; a collection of small raised pieces is not a substitute for good surfacing.
- **Cockpit:** replace the teal bubble and wide rounded silver surround with a lower, longer, near-black canopy and a crisp narrow frame. A controlled planar crown and faceted shoulder transitions should connect the cockpit to the fuselage. The pale rear dorsal cap currently interrupts the long dark center; redesign that transition so the central rear taper reads as an integrated spine.
- **Fins:** remove the current tall slab fins in the first massing candidate. If directional stabilizers are retained later, integrate low swept surfaces into the nacelle shoulders, with thickness and a resolved root. Their black rectangular silhouettes must not dominate the rear view.
- **Negative space:** retain the two open longitudinal channels, but shape the nacelle inner wall and central fuselage to form deliberate spaces. The current large empty gaps plus loose rods look like parts connected after the fact.

Starting proportion experiment: lower the canopy crown by roughly 15–25% from V2 while retaining a plausible cockpit volume; replace tall fins with shoulder-height integrated surfaces; widen the rear fairing locally instead of uniformly widening the whole craft. Treat these as candidate changes, then choose by rendered comparison. Do not lock arbitrary ratios when the images still look wrong.

**Gate 1:** in untextured medium-gray material, top, front three-quarter, rear three-quarter and ordinary chase views show one coherent fast machine. No puffy armor caps, surfboard bundle, bubble-canopy reading, dangling fins, needle-dominant silhouette, or bolt-on engine cans remains. Broad highlights travel cleanly across the intended crown and break at designed shoulders. If any of those defects remains, revise this item before adding detail.

### 2. Resolve the assembly and visible surface defects

- Give shell edges consistent, plausible thickness. Use small controlled edge radii on manufactured armor; avoid one large rounded radius on every object. For this approximately 7 m craft, begin with centimeter-scale edge treatments and compare at close and chase distances. A panel should read as a formed shell, not a cushion or wafer.
- Replace the isolated diagonal pipes, open-ended rods and single thin rear crossbar with a small number of designed structural modules: nacelle mounts, recessed braces, and propulsion feeds that visibly terminate in sockets, clamps or housings. Mechanical density should explain how parts connect. Keep the spaces open enough to read from chase.
- Build a restrained panel hierarchy: large nacelle shell sections first, smaller removable covers second, sparse fasteners at believable service points last. Use uneven but deliberate panel lengths. Avoid repetitive slots or bolts applied everywhere.
- Resolve hard intersections, lumpy highlights, awkward fin roots, projected label boundaries and cap-to-body transitions in the actual exported geometry. Check both sides. Shading modifiers or a high mesh count do not establish that this work is complete.

**Gate 2:** close rear/front views show solid shell thickness, clean edge flow, intentional seams and physically connected structures. No floating-looking pipe ends, surface clipping, broken marking edges, uncontrolled shading dents, or obvious intersecting attachments remains in visible areas. An attractive angle does not excuse defects on the other side.

### 3. Build material separation and restrained authored finish

- **Pearl armor:** make coated armor distinguishable from exposed metal. Start with a dielectric coating, a modest clear finish, broad soft highlights and restrained roughness variation. Use subtle differences between panel groups and localized service wear. The desired finish is maintained racing machinery; blanket dirt and random noise will weaken it.
- **Dark structure:** use dark graphite with readable midtone edges and rougher recessed surfaces. It must reveal mounting and layered depth under a neutral light, instead of becoming either featureless black or the same silvery plastic as the trim.
- **Canopy:** near-black tinted glazing, narrow continuous highlights, separate framing and a credible sill. Avoid the current saturated teal jewel appearance and broad polished silver gasket.
- **Engine metal:** reserve stronger metallic response for the nozzle lip, heat shields, collars and selected hardware. Differentiate dull heat-treated interior surfaces from machined edge accents. Avoid chrome-like treatment across the whole assembly.
- **Accent/graphics:** use small inset citron panels and a restrained race-number layout. Remove the current raised sticker-like strip appearance. Correct rear number legibility and the ragged/clipped borders visible in V2. Tiny service markings should be subordinate and consistently aligned.
- Export the chosen finish faithfully. Textures or procedural authoring are means, not the acceptance test; the runtime result must retain the same material hierarchy. Document color-space and channel choices where maps are used so remapping does not silently flatten the asset.

**Gate 3:** under the same neutral lighting, armor, glazing, graphite structure, metal and emissive surfaces are immediately distinguishable. Moving a broad light across the hull exposes a controlled finish and clean geometry. Surface variation appears tied to panel manufacture/use, with no uniform plastic sheen, noisy weathering, smeared maps or glaring seams. Marks remain clean in both close views and normal race framing.

### 4. Redesign engine presentation and integrate propulsion

The current engine does contain physical depth; its look fails because the bright broad cyan annuli and large shiny lip dominate that depth. Do not treat increasing bloom as the correction.

- Integrate each main engine housing into the nacelle shell. Build a dark outer recess, a restrained metal lip, visible inner collar/heat shield, and a compact near-white core. Keep luminous annuli thin and dim enough that the chamber's dark structure survives.
- Give the interior a small number of coherent rings and axial details, with dark gaps between stages. Avoid the current turquoise open-tube/toy-fan impression and a uniformly luminous chamber.
- Keep the small center engine subordinate in both physical size and light output. It should belong to the central spine.
- Refresh the engine-anchor contract from the actual exported exits whenever geometry changes. Verify alignment under yaw, banking and acceleration. Do not rely on V2 anchor positions after remodeling.
- Maintain short soft translucent exhaust with a bright compact origin, fading into the air. Keep the hull edge and nozzle construction visible through the effect. The reference's approximate 1–2 m fade is an art target; judge scale in the actual camera. Idle, cruise and boost should be related states with stable attachment.

**Gate 4:** rear three-quarter and chase views read dark chamber → restrained collar → compact core at a glance. Cores remain visible without turning the nozzle mouth into a solid cyan disk or a bright blue outline. Native idle/cruise/boost evidence shows soft attached exhaust and restrained local spill, with no opaque cones, detached points, overlong streaks or clipped shells.

### 5. Prove the finished asset in Unity and in motion

Use the parent's repeatable native material-inspection rig to isolate mesh/export/material defects from the city lighting. Match the ship revision and material mapping used in gameplay. The neutral Blender render is a geometry/authoring gate, not a native-quality pass.

After neutral native review, capture the ordinary player chase camera at the start, a cool-lit bend, a darker interval, and an amber-lit corridor/inspection equivalent. Include a visible nearby opponent using the same model to expose side geometry. Keep the camera settings and exposure comparable across revisions; record any deliberate change.

Capture a short real native motion passage containing idle-to-acceleration, ordinary cornering/bank, cruise, boost and a warm/cool lighting transition. Label automated steering accurately if used. Full resolution stills establish form/material appearance; moving evidence establishes shimmer, reflection stability, exhaust attachment and lighting transitions.

**Gate 5:** the same premium material and shape reading survives native rendering and ordinary gameplay size. The cockpit is dark but formed; pearl panels retain highlight shape; structural gaps stay readable; engines do not consume the silhouette. No visible alias shimmer, material popping, clipped livery, unstable exhaust, abrupt lighting discontinuity or camera concealment of important defects remains. The ship remains the readable focal object while the route is visible.

## Required evidence for each review

| Evidence | Fixed conditions | What it must establish |
| --- | --- | --- |
| Clay top + side | Orthographic, medium gray, no emission/graphics | Proportion, height, silhouette, negative space; the side view is missing from the current baseline set. |
| Neutral front/rear three-quarter | Full ship, 1600 px or greater on the long edge; broad white key, weaker fill, fixed exposure | Clean crown/shoulder highlights, shell thickness, joints and both sides of the craft. |
| Reference-C perspective | Approximately 7 m behind and 2.5 m above, rear quarter; choose and record a stable focal length; whole ship at about 40% frame width | Comparable form/material hierarchy. This is an inspection camera, not a replacement for normal gameplay. |
| Engine and canopy close views | Same white light, emission-off and emission-on engine pair | Real recess depth, collar hierarchy, clean surfacing and graphics; darkness or bloom cannot hide defects. |
| Neutral Unity render | Same export/materials as game; fixed rig/exposure, close rear quarter and chase | Import fidelity and real-time material separation. |
| Normal native gameplay | Normal player camera, original-size 1920×1080 or higher; cool, dark and warm views | Whether the authored improvement survives the real presentation. Retain HUD for at least one normal capture. |
| Native motion | Continuous footage with recorded revision and steering method | Temporal quality, plume stability, banking/camera behavior and light transitions. |

Preserve the V2 studio camera set for exact comparisons: source-coordinate rear quarter (-8, 10, 6) targeting (0, 0, 0.2); front quarter (9, -11, 7) targeting (0, -0.1, 0.1); chase (0, 10, 4.4) targeting (0, -0.8, 0.05); top (0, -0.001, 15) targeting (0, -0.2, 0). Existing cameras are orthographic with scales 9.6, and 10 for top. Add a perspective set rather than mislabeling this orthographic evidence as gameplay matching. Record lighting changes as well as camera changes.

## Review policy

The worker submits images and the exact asset/export revision. The independent critic inspects the images directly, identifies improvements and remaining failures, and returns a bounded next correction. Preserve each rejected iteration. Do not bundle many hidden changes and infer which one worked.

Each gate may be accepted for its own scope while the overall ship remains rejected. Missing native or motion evidence is pending, never a pass. One remaining major visible defect is enough to reject the corresponding gate. Do not claim that completing a checklist, raising polygon count, compiling cleanly, or comparing the game to generated art proves AAA quality. That judgment requires the final images and moving game to sustain the reference's visual standard; if they do not, record the failure candidly and continue correction.
