# Improvement approach after the negligible SSR comparison

The owner judged SSR02's difference almost negligible and requested parallel independent criticism before choosing an improvement approach. Three critics reviewed visual priorities, rendering, and environment construction. Their common finding is that the current SSR experiment does not justify adoption and that a complete scene needs more than a reflection switch. This document is a recommendation; no new rendering or asset implementation is included.

## Recommended next milestone

**Finish one coherent opening passage, with SSR parked.** The desired visible result is a road passing through a constructed, selectively lit district: supports meet readable bases, nearby walls and rail bodies have depth, and the road carries broad varied surface response. Keep the circuit, camera, craft silhouette and racing behavior. The unit of improvement should be the ordinary gameplay image through a short sequence, not a renderer setting or a close-up material crop.

The previous proposal to shift spotlight origins, aim and cone size remains a valid local experiment, but it is too narrow to be the headline milestone. Its likely outcome is redistributed soft pools; it adds neither connected construction nor a new reflected source. Do not spend another full milestone presenting that as a major finish upgrade.

## What the critics contribute

| Review | Main finding | Implication |
| --- | --- | --- |
| [Visual critique](002a-improvement-visual-critique.md) | The large road area lacks structured bright/dark response; gallery surfaces lack visible material/depth separation; exterior bases remain disconnected. | Judge a complete image. Warm gallery is the cheapest controlled place to test road/wall/ceiling treatment. |
| [Rendering critique](002b-improvement-rendering-critique.md) | CPU SSR scopes do not prove useful GPU contribution; a small fixture correction cannot explain the full visual gap. | Park SSR adoption. A single bounded buffer inspection can diagnose it, but should not hold art work hostage. |
| [Environment critique](002c-improvement-environment-critique.md) | Supports, service levels and landmark connections need readable physical relationships and local illumination. | Promote opening construction and light into the next demonstration instead of waiting behind more road-only tuning. |

The critics differ on order. The visual critic prefers gallery first because it isolates surface finish with existing geometry. The environment critic favors the opening because construction changes offer a larger compositional gain. The integrator chooses **the opening as the delivered milestone**, using the gallery as a small material calibration task. This keeps the work aimed at the first racing impression without asking an uncontrolled outdoor image to answer every shader question.

## Three bounded parts of the opening milestone

### 1. Make the track belong to its surroundings

Audit the five existing opening camera views against current placements before creating assets. Existing authored terraces, workshops, podiums and streets must be checked for visibility, orientation and lighting. For example, the method named `BuildApproachServiceGroup` currently samples route progress `.735`, so its name does not establish coverage of the `.15–.43` opening benchmark.

Reuse useful assets and add only the missing connections: a support/podium interface, one service frontage or recessed maintenance entrance, and a short supported platform/catwalk where the camera can actually read it. Reveal selected structural sides and occupied recesses with local practical light. Preserve dark gaps and the recognizable signal mast rather than filling the skyline with more towers or window dots.

**Visible result:** in the normal approach and bend views, a viewer can follow a support into a base/service level and on to adjacent construction. Near, middle and distant layers are distinct. Adding more geometry that stays black or off camera fails this part.

### 2. Establish light and material contrast together

Use the existing warm gallery for a compact surface study: painted wall faces, darker structural metal, folded returns/recesses, and the running deck under known warm practical sources. Check actual material inputs first. Author broad spatial variation at a scale visible from the chase camera; avoid another blanket smoothness increase or a layer of tiny noise. Light must reveal existing depth and material changes, including the rail body, rather than merely tint large planes.

Transfer only a visibly useful treatment to the opening. Fixture aim/coverage may be one ingredient. Direct specular highlights and reflected scene structure remain different results; direct lighting must not be reported as solved reflections. If the available lighting/material approach cannot produce coherent elongated road response, keep that limitation explicit and make a separate technique decision instead of baking camera-dependent bright streaks into the road.

**Visible result:** obvious broad bright/dark road variation follows the bend and light rhythm, while wall/rail faces and structural sides separate at ordinary viewing size. A uniformly brighter road, relocated circular pools, or differences requiring zoom fail. The gallery test is not a prerequisite for starting the independent construction work.

### 3. Integrate the scene, then refine the craft

Combine accepted construction and surface/light treatments in the opening passage. Adjust their relative brightness so road and nearby architecture carry the space and cyan remains clear navigation. Preserve the current craft design; assess pearl highlight roll, canopy/graphite separation and hover integration under the improved environment before spending time on new craft maps or small asset details.

**Visible result:** the passage reads as a more complete place immediately, with readable craft and driving line. The user should be able to identify the stronger image without annotations explaining where a small change occurred.

## Evidence and stopping rules

- Keep the original playable app and all rejected experiments. Use a separate candidate and SSR off as the explicit art baseline.
- Preserve separate construction, surface/light and combined snapshots so causes remain reviewable; evaluate the coherent final image rather than insisting each constant change be its own milestone.
- Review all five opening crossings plus thermal, warm-gallery and station controls. Preserve actual pose differences. No need for another full-lap capture during every source edit: use representative native views to reject weak drafts before the complete evidence run.
- Require obvious gain in both ordinary approach and bend, not only a favorable corner. If a first proposal remains negligible, reconsider its scene-level hypothesis; do not enter another smoothness/intensity sweep.
- Watch the opening continuously for new light transitions, shimmer and occlusion problems. Then run separate performance measurements on the selected candidate. The existing provisional budget and unresolved control P99 remain unchanged.
- Keep one optional SSR GPU-input/hit/final-contribution investigation bounded to a representative view. If it cannot be observed with the available tooling, record that and leave SSR parked. It is neither a production gate for direct-light art work nor a license for unlimited renderer debugging.

## Later work

Once the opening shows a clear gain, adapt its construction and lighting rules to the thermal platform and station, finish the gallery consistently, then refine craft materials and integrated racing/motion/audio. Circuit rollout still waits for native visual, motion and technical acceptance. The generated targets guide relationships and finish; their fine grain, apparent transparent-exhaust reflections and small geometry drift are not exact implementation requirements.
