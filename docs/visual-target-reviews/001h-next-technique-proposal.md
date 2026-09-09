# Proposal after SSR02: bounded direct-light shaping

**Recommend one opening-road direct-light candidate using the existing fixtures and frozen satin material.** This supplies source-related diffuse/specular light pools, not reflected buildings, rails or lamps. It is a scoped road-and-light finish experiment; it cannot close a requirement for reflected scene structure.

No rendering code, materials or settings were changed for this proposal. The integrator's nine-view SSR02 review is still being finalized; its current assessment is no useful whole-frame gain. This report does not substitute for that visual verdict. If the final review confirms that assessment, preserve SSR02 as rejected for the production appearance objective and use the proposal below as the next independently reviewable candidate.

## Evidence for this choice

- [Reflection feasibility 005](../road-reviews/005-reflection-feasibility.md) records that prior cubemap/probe changes did not provide the required broad response, and that increasing road mask smoothness from `.35–.50` to `.45–.60` with the `.90` multiplier did not provide enough whole-frame improvement. More smoothness does not create missing reflected information.
- SSR02's native log now reports positive CPU SSR/inline marker samples with ON and zero samples with OFF (final inline counts 1,690 versus zero). This narrows the earlier execution uncertainty but does not prove correct GPU reflection pixels. The current no-useful-gain assessment comes from the integrator's images, not those CPU counters.
- Existing `NightTrackLighting.Build` places outdoor spots alongside 58 regularly spaced fixture stations, skips gallery interiors, aims each toward the centerline six metres ahead, and assigns every outdoor spot the same range `41`, outer angle `98`, and inner angle `52`. Intensity and colour switch between warm/cool groups. This is a concrete place to test coherent road illumination without introducing new renderer dependencies.
- The running deck is 22 metres wide, with existing restrained normals and spatial smoothness. `TrackPath` changes elevation and bank along a curved spline. The road can already respond to direct lights; source position, aim and beam footprint can be evaluated independently of its material.

## Precise next candidate: opening pool correspondence 01

Build a separately named local candidate with SSR explicitly off for both its control and change runs. Preserve the accepted control app, SSR02 app/source and all failed attempts.

1. Limit changes to outdoor fixture stations whose normalized progress lies in `[.12, .38]`. This covers the opening approach/crest/descent/bend without editing either gallery's lights. Keep all station counts, warm/cool assignments, fixture geometry, fixture shadow-casting exclusions and other world inputs unchanged.
2. Place each affected spotlight origin at the corresponding visible linear diffuser's center (`p + Up*12.6 - Right*side*4.5`) rather than its existing offset position. Aim at the local road center `f.Position`, eliminating the common six-metre forward bias for this candidate. This tests whether broad illumination reads as originating under the visible fixture rather than displaced ahead of it.
3. Derive the outer cone from a receiver footprint rather than another arbitrary angle sweep: at each station, sample the actual curved/banked deck centerline five metres before/after the station, take points eight metres left/right at those samples, and calculate twice the maximum angle from the chosen light axis to those four points. Keep the inner cone at `52/98` of that computed outer angle to preserve the existing relative falloff softness. Reject an unrepresentable cone or footprint outside the unchanged range instead of increasing range or silently clamping it. The 16-by-10-metre target is a test footprint inside the 22-metre road, leaving unlit margins; it is not a physically exact rectangular area light.
4. Hold intensity (`460/540`), range (`41`), colour, emission, road maps, normal strength, `.35–.50` mask and `.90` multiplier fixed. This deliberately isolates origin/aim/beam shape. If the spot's native intensity semantics change total delivered light as the cone changes, record the result; do not add an exposure or intensity compensation in the same candidate.
5. Compare all five opening crossings plus both thermal controls, warm gallery and station with measured pose deltas. The affected opening views must show a worthwhile whole-frame improvement in light-source correspondence and road readability; unchanged sections are regression controls. Preserve dark intervals, lane margins, seams, craft grounding and the skyline hierarchy.
6. Watch the full opening continuously for moving highlight fans, abrupt pool edges, shimmer and distracting brightness changes on bank transitions. If appearance improves, measure an isolated same-binary control/change comparison; constant light count does not prove equal fragment cost because footprint coverage changes.

Accept only if the ordinary approach and bend both improve with no critical loss elsewhere. If the shape-only candidate does not improve them, reject it; do not immediately increase smoothness, probe strength, lamp count or exposure. Preserve its diagnosis before considering a second substantial correction. This proposal does not authorize circuit-wide rollout of an unreviewed lighting treatment.

## Why not a custom planar road reflection next

A planar reflected camera is exact for one plane. This deck changes both height and bank, so one global mirror plane would disagree with much of the receiver and cause position-dependent sliding or incorrect alignment. Segmented planes would require receiver partitioning, clipping, seam/transition management and additional scene rendering, with explicit performance and motion risks. That is a new rendering system, not a small road-material correction.

A single genuinely flat, localized receiver could justify a separate planar-reflection feasibility experiment later if actual reflected structure remains mandatory. It would need a measured flat-region contract and its own render/cost review; it would not establish feasibility for the complete running deck. Do not silently replace the current road with planar geometry or disable the existing probe to make a sample work.

## Remaining SSR diagnosis versus production choice

GPU output/depth/normal/smoothness inspection can still distinguish ineffective ray hits, invalid inputs, blending loss or other SSR faults. That would explain SSR02, and remains necessary before claiming the effect itself was correctly producing reflections. It is optional further diagnosis for a rejected appearance candidate, not evidence already obtained, and not a reason to start a parameter sweep.

The production decision can remain narrower: the current native ON images do not demonstrate the required gain, so do not roll out SSR02. Direct-light shaping is independently justified as a road-lighting improvement while the reflected-structure objective remains explicitly unresolved. No engine/package upgrade, new dependency, probe-intensity iteration or arbitrary smoothness change is proposed.

Integrator follow-up: [independent review 001g](001g-ssr02-visual-review.md) is now complete and rejects demonstrated useful visual gain after all 18 originals. The conditional visual premise above is therefore confirmed as a rejection result, without diagnosing the GPU cause.
