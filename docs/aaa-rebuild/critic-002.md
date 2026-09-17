# Independent visual critique 002: Nocturne opening and warm gallery

Date: 2026-09-09
Verdict: **REJECT the direct-production visuals.** They are substantial regressions from the original native art and are not a viable base for polish. The original captures remain the native visual control, but they also require major construction, material, and lighting work to approach the supplied concept targets.

## Evidence and limits

I directly inspected all six supplied files at original resolution:

1. Original native opening: `evidence/nocturne-v2/opening-04/draft/off/selected/opening02.png`
2. Rejected direct-production opening: `evidence/nocturne-production/direct-01/native/opening02.png`
3. Generated opening concept target: `references/nocturne-v2/A-opening-city.png`
4. Original native gallery: `evidence/nocturne-v2/opening-04/draft/off/selected/warm-gallery.png`
5. Rejected direct-production gallery: `evidence/nocturne-production/direct-01/native/warm-gallery.png`
6. Generated gallery concept target: `references/nocturne-v2/B-amber-gallery.png`

The generated images are concept targets. They are evidence of the intended composition and finish direction, not proof of feasible native geometry, game readability, or Wipeout quality. No actual Wipeout Omega Collection reference was supplied, so this report makes no direct Wipeout comparison. These are stills only. They do not establish motion, audio, handling, performance, or whole-lap quality.

## Opening: observed regressions and required direction

The direct-production opening loses the original scene's defining image: a fast elevated ribbon crossing a layered nocturnal city. A huge featureless slab blocks most of the left side, while a repetitive dark-window block crowds the right. The original's lit skyline, stacked transit routes, thin lamp rhythm, distant haze, and gate silhouette are either removed or visually marginalized. The result reads as a gray blockout corridor rather than a city set piece.

The direct-production lighting also collapses the frame into a narrow blue-gray range. The road is uniformly bright and smooth, the buildings have weak material separation, and almost no practical illumination connects architecture to track. Course readability survives because the road and edge lines remain obvious, but the environmental speed cues, depth layers, and sense of altitude are much weaker.

The original is clearly stronger, but it is not a finished target. Its lower city falls into large black voids, the road has little surface response, the gate and lamps are thin, and the giant pale structure cropped at right is crude and compositionally distracting. The generated target shows the relevant improvement direction: dense but separated city layers, a memorable midground tower, physically substantial viaduct structure, localized cool and amber lights, and a dark road carrying controlled specular reflections. It must not be copied as literal geometry.

## Warm gallery: observed regressions and required direction

The original gallery has a strong section identity. The continuous chamfered enclosure, repeating wall cassettes, ceiling rhythm, warm fixture bands, cool cyan guard line, and blue exterior exit create a clear warm-to-cool sequence. The craft remains readable against the road and the corridor frames the bend.

The direct-production capture removes that identity almost completely. An open skeletal truss and scattered facade pieces replace the enclosed gallery. The outside city leaks through both sides and the ceiling, the structure has no convincing wall build-up, and thin brown strips appear disconnected from any working light fixture. Flat cool illumination makes the road, truss, and buildings occupy nearly the same value family. A large pale patch on the road reads as an arbitrary lighting or material region rather than a light pool. This is not a weaker version of the gallery. It is a different, generic section.

The original gallery still needs major rework. Its broad wall panels are largely featureless, the amber wash is too uniform, fixture housings are crude or lost in black, and the dry, flat road does not convincingly receive the lighting. The concept target preserves the original enclosure and route framing while adding deeper ribs and panel returns, believable fixture housings, localized pools and falloff, material contrast, small-scale construction detail, and controlled road reflections. Those are the useful art-direction signals.

## Retain, rework, replace

| Area | Decision | Evidence-based direction |
| --- | --- | --- |
| Original opening composition | **Retain** | Preserve the exposed elevated curve, layered skyline, secondary viaducts, lamps, gate, cyan route edge, craft silhouette, and HUD hierarchy. These provide place identity and depth that the direct-production capture loses. |
| Original opening foreground and finish | **Rework** | Replace the crude cropped right structure, strengthen the gate and lamp construction, recover lower-city readability, and add restrained road and structure material response without lifting the whole frame into gray. |
| Direct-production opening architecture and grade | **Replace** | The left slab, repetitive right block, sparse skyline, flat road, and global blue-gray wash are rejected. Do not iterate them through another exposure or roughness sweep. |
| Original gallery spatial shell | **Retain** | Preserve the enclosed chamfered profile, repeated cadence, framed exit, curve, cyan edge, craft, camera, and HUD. This is the section's visual identity. |
| Original gallery construction, materials, and lights | **Rework** | Give ribs, panels, fixtures, trim, and road distinct construction and material roles. Use localized warm illumination with shadowed intervals and road response instead of a uniform orange wash. |
| Direct-production gallery | **Replace** | Reject the open scaffold, exposed exterior clutter, disconnected brown strips, flat cool flood, and pale road patch. None should survive into the exemplar. |
| Craft, thrusters, and HUD | **Retain for this slice** | They remain legible in both native pairs and provide a stable comparison control. The concept targets omit HUD, so they provide no evidence for a HUD redesign. |

## Staged package context

The staged metadata declares five asset families, 13 proposed placements, an opening limited to two buildings, and a gallery limited to the entrance and first approximately 26 metres. It also explicitly records `layoutComplete: false` and `visualStatus: BLOCKED_VISUAL_REVIEW`. Its source checks establish payload integrity and static clearance only. They provide no evidence of native appearance.

The declared portal, rib, panel, facade, material-slot, and fixture-socket structure addresses the right categories of missing work. That is not an adoption finding. In particular, two opening facades cannot by themselves recover the lost skyline, infrastructure layering, and foreground composition visible in the original and concept target. The gallery package is the more falsifiable first test because it covers a bounded section with geometry, material, and lighting responsibilities concentrated in one view.

## Highest-impact integrated recommendation

Build the **warm gallery as the first native vertical slice on the preserved original baseline**. Keep the original track, camera, craft, HUD, cyan guard treatment, city exit, and continuous backing shell. Use bounded replacement geometry only for the portal, ribs, panels, and their fixtures. Integrate construction, mapped materials, local lighting, shadow contact, and road response in the same candidate. Do not apply the direct-production world or a global exposure reset. If the staged assets cannot meet the gates below, reject them rather than weakening the gates.

Do not begin opening replication or whole-circuit rollout until this integrated gallery slice passes. The opening needs a broader city composition plan than the staged two-building proposal provides.

## Falsifiable native gates for the gallery candidate

1. **Comparable evidence:** Capture `off` control and `gallery` candidate from the same native binary at 1920 x 1080 with identical camera, field of view, exposure, quality settings, and matched route position. Record build identity and the active exemplar flag. A mismatched capture is an automatic re-test.
2. **Enclosure and assembly:** In the matched still, a continuous portal must connect both walls and the crown, followed by a readable repeated rib and panel cadence. Reject any exposed generic scaffold inside the replacement span, doubled surfaces, z-fighting, floating joins, shell gaps, or abrupt uncovered ends.
3. **Material separation:** The road, broad wall panels, dark structural ribs, metal trim, and diffuser faces must remain visibly distinct under one exposure. Reject a candidate if these collapse into the same blue-gray or amber value family, or if texture detail replaces readable macro construction.
4. **Lighting integration:** Warm fixtures must sit in visible housings and create at least three separated pools or reflection bands on the road and adjacent panels, with darker intervals between them. Cyan guard lighting must remain cyan and visually separate. Reject uniform global orange wash, flat cool flood, clipped emitters, or light that does not affect nearby surfaces.
5. **Route and craft readability:** The bend, both road boundaries, framed exit, craft silhouette, twin thrusters, and all existing HUD text must be at least as legible as in the original native gallery at the matched view. Any new foreground piece that obscures them fails.
6. **Section transition:** Provide one uninterrupted native drive-through from before the new portal to beyond the approximately 26 metre staged span. Reject visible LOD pops, lighting steps, clipping, flicker, bank misalignment, or an obvious quality seam where staged and retained gallery construction meet.
7. **Target-direction check:** The native still must visibly deliver all four primary concept signals: enclosed construction depth, localized warm-versus-cool lighting, dark controlled road reflections, and a cleanly framed city exit. Missing any one triggers another corrective pass.

Passing these gates would approve only the bounded gallery section for continued production. It would not establish AAA acceptance, Wipeout parity, whole-circuit quality, motion feel, audio quality, human playability, or performance.
