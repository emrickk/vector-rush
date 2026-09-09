# Next development step: road response in the opening passage

Proposed on September 8, 2026, following the owner's request to continue and explain the next step. The preceding game checkpoint is `3097bc8`, native GUID `40bed5f53c2449418e7fb56bf59739f6`. The owner subsequently approved and executed this bounded experiment. **Completed: candidate 01 rejected; exact control restored.** See the [decision and evidence](../evidence/night-production/road-response/decision.md), [independent native verdict](road-reviews/004-resumed-smoothness-native.md) and [reflection feasibility report](road-reviews/005-reflection-feasibility.md). The instructions below preserve the original plan; they are not pending permission requests.

The immediate objective is a road surface that shows broad, plausible light response while retaining panel detail, readable edges and stable highlights through bends. Finish this bounded part of Stage 1 before adding more city or ship detail.

## Establish the current control

Capture the actual combined checkpoint through the opening mast-to-gallery passage before editing it. Its existing verification covers the title and race start, not the whole benchmark. Use the five natural progress crossings `.15`, `.22`, `.30`, `.37`, `.43`, plus the two thermal controls and station reveal already defined in the [production plan](superpowers/plans/2026-09-08-night-production-finish.md). Preserve a continuous ordinary-driving clip that includes the opening slowdown. Review its motion separately from still images and decoder integrity. Report camera drift rather than calling nearby crossings pixel-matched.

Keep the accepted fog and A lighting fixed, including the sky, exposure, all light positions/intensities and the three fixture caster-off groups. Keep road geometry, normals, color texture, track collision, craft, camera and racing logic fixed. The clearer reflection-off road is the control; preserve B/C/D as rejected or unaccepted comparisons.

## Make one material comparison

Inspect how the existing direct highlights move and which visible sources produce them. Then make one documented road-smoothness variation against that control. Change the packed mask's effective response deliberately: the current alpha `.35–.50` times the runtime multiplier `.90` yields `.315–.45`. Editing only the template's saved smoothness will be overridden at runtime. Record both the new alpha range and multiplier, and verify their effective native values.

Keep reflection state fixed for this comparison. Do not combine a new smoothness range with another probe, light, normal-map or exposure change. Compare ordinary dark road and boosted movement, including highlight shape during banking, abrupt boundaries, panel visibility and any return of strong diagonal artifacts. A brighter feature without a plausible source is insufficient.

## Decide using the whole passage

Retain the candidate only if the original full-frame stills and continuous passage show a useful gain, the craft remains grounded and readable, and the thermal/station controls retain contrast. Have the independent critic assess those original captures and preserve a rejection if it fails. Do not describe a material improvement alone as completion of the entire production benchmark.

If the direct-light treatment still cannot produce the required road response, stop parameter tweaking. Use the preserved failed probe evidence to write the bounded feasibility comparison for a different reflection technique, including native shader availability, visual limitations and cost. Source guarded by `URP_SCREEN_SPACE_REFLECTION` does not prove a working native feature. No costly technique is assumed in advance.

## Verify and preserve the result

After selecting a candidate, run the relevant existing checks and a separate real-time native race/performance sample with no build, bake or encoder overlapping it. Compare to the actual control, rather than assigning the older baseline's performance to the new app. Preserve source/build identities, native evidence, the decision and remaining limits; commit and push this bounded milestone under the project workflow. Broader Stage 1, watched/manual, alternate-aspect and circuit acceptance remain governed by the production plan.

Deliverable: one before/after opening-passage comparison, one retained or rejected material experiment, an independent verdict and an explicit next decision. The original planning document made no game or asset changes. Execution subsequently preserved the rejected candidate at `a5debfb`, then restored the original runtime and app. Continuous watched-motion acceptance remains open; the candidate failed the prerequisite still-image gain gate.
