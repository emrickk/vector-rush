# Rendering critique: stop treating renderer activity as visual progress

**Recommendation: park SSR02 from the production appearance path; allow at most one short GPU-buffer diagnosis to classify its failure. Do not make proposal 001h the next headline visual milestone.** Its fixture-origin correction is small, and its larger cone/aim change still redistributes the same soft illumination. The next deliverable should be one visibly finished opening sector, combining deliberate light rhythm, broad material regions and connected nearby construction. Reflected structure remains an explicit unresolved requirement rather than a prerequisite that indefinitely blocks unrelated visible improvements.

This is a read-only technical and visual critique. No runtime, material, renderer, package or build was changed. I read the project SSR preview, fixture and track code; the pinned URP 17.6 source in `com.unity.render-pipelines.universal@8457e85b8184`; the current execution plan and reviews 001g/001h. I directly viewed original OFF/ON pairs 0149 and 0943 at whole-frame size. Those four images support the negligible-gain assessment in the opening and warm gallery; the nine-view coverage belongs to review 001g. I did not view continuous motion or inspect live GPU buffers.

## What is established, and what is still unknown

The opening pair still reads as a dark, regularly panelled deck with isolated broad cool/amber pools and a very bright cyan boundary. The gallery pair still reads as warm walls around a dark satin road. Neither pair presents an obvious new reflected source. The implementation work removed documented failures and preserved the scene, but it has not supplied the visual transformation the user asked for.

CPU markers are correctly labelled in the existing diagnostics. Their presence establishes instrumented work on the CPU; it does not establish populated camera history, valid ray hits, reflection alpha, useful reflected colour, or final material contribution. Conversely, these images cannot prove that the SSR buffer is empty. There may be valid but dark, spatially limited or visually redundant reflection data.

Source-supported limits matter:

- This configuration traces 30 m, at half resolution, with 32 linear steps. It is a near-structure test, not a test of reflected distant city towers. Higher settings would be a separate hypothesis, not an explanation already demonstrated.
- `ScreenSpaceReflectionPass.cs` uses previous-frame colour/depth and motion reprojection when `afterOpaque=false`. That remains true with temporal filtering disabled. An ON marker cannot show those inputs contain useful data.
- `ComputeScreenSpaceReflection.hlsl` rejects offscreen/reprojected samples and fades camera-facing rays, maximum-distance hits and screen-edge hits. It returns RGB even on misses, with alpha zero. A colourful RGB debug image alone could therefore be misleading: inspect validity/alpha too.
- `GlobalIllumination.hlsl` blends existing environment irradiance with SSR using that alpha. The material response then matters. Valid rays are necessary but do not guarantee a visibly useful final result.
- The road changes elevation and bank. A global planar mirror is not a drop-in substitute. No evidence here justifies a package upgrade, a probe-strength sweep, or raising smoothness endpoints again.

The potential limitations above are inspection targets, **not a claimed root cause** of SSR02's negligible appearance.

## Why 001h is too weak as the primary improvement approach

The existing visible diffuser centre is at local `(side*9.5, 12.6, 0)`; its spotlight is at `(side*10, 12.4, 0)`, relative to the track frame. Their separation is approximately **0.54 m**, compared with about **16 m** to the road centre. Fixing that offset is sensible housekeeping, but there is no demonstrated gross source-position error in the image, and it is unlikely to transform the shot.

Removing the six-metre forward aim bias turns the light axis by about **20.7°**. On a locally flat deck, 001h's four target corners produce an approximately **73°** outer cone, compared with the existing 98°. Actual bank/curve samples will differ. This can visibly move and narrow illumination; it is not necessarily negligible. But movement is not improvement. Forward aim is not inherently incorrect for a road fixture, and the current review has not established that the forward displacement is the dominant visual defect.

The proposal's 16-by-10-metre corner construction is an enclosing cone, not a rectangular area source or exact bounded road footprint. Keeping intensity and emission frozen while changing coverage may simply make the existing broad pools smaller or the scene darker. It cannot make the 4.8-metre visible diffuser behave like an extended reflected light source. All important visual ingredients remain: repetitive stations, the same warm/cool sequence, the same road map, identical material differentiation and disconnected near-city massing.

**Expected gain: local polish at best, with a real risk of another technically precise but aesthetically inconsequential comparison.** 001h can be folded into light authoring when its aim helps the selected composition. It should not consume a full build/review milestone by itself or be described as solving reflections.

## Bounded SSR decision experiment

Allocate one diagnostic session, capped at roughly one hour of investigation and no more than one diagnostic build. Use a warm-gallery frame after history has populated. In a frozen diagnostic view, preserve the native camera/material/light state and inspect: road depth, normals/smoothness, sampled previous colour/depth, raw hit/alpha, and the final SSR texture. Treat this frozen inspection as diagnosis; the eventual appearance gate still uses normal racing motion.

Use the following stop rules:

| Observation | Decision |
| --- | --- |
| Required buffer access cannot be obtained within the session | Preserve “GPU cause unverified” and stop. Do not replace the missing evidence with more CPU counters. |
| One concrete input/binding fault is identified | Permit one correction of that demonstrated fault, then a fresh normal full-frame comparison. |
| Inputs are valid but useful road hit coverage is sparse/dark/rejected | Park this SSR configuration. Do not begin an open-ended step/thickness/strength sweep. |
| Coherent reflected structure exists in the buffer but is lost at composition | Inspect that one composition/material path. Correct only a demonstrated error; do not redesign the material to force the experiment to pass. |
| Corrected normal output still needs a crop, slider hunting or explanation to see a benefit | Reject for production and stop. |

This experiment's expected deliverable is a **decision**, not a prettier image. It earns its small budget by preventing another blind renderer iteration. If the user's priority is immediate visible progress, skip it and retain the honest unresolved diagnosis. Neither choice should delay the main scene work.

## Substantive alternative: finish one opening-sector composition

Use the existing opening approach and bend as a single art-direction trial with SSR off. Keep route, camera, craft geometry, physics, corrected fog and fixture shadow exclusions fixed. The proposed change package should target three visible outcomes together:

1. **Readable light rhythm.** Author a small group of existing fixtures so an illuminated approach, a darker transition and a second illuminated bend are perceptually distinct at chase distance. Adjust beam shape, aim and calibrated intensity together around those zones. Preserve visible source correspondence and colour hierarchy. A uniform six-metre aim correction is a tool inside this task, not its visual goal.
2. **Broad surface structure.** If the light pass demonstrates where useful response exists, redistribute the existing satin mask into road-scale broken regions with restrained dark margins. Keep the rejected global smoothness endpoints fixed; change spatial organisation based on a named visible defect. Seam/wear placement must read at normal gameplay size, rather than relying on microscopic noise. Save light-only and combined snapshots so attribution is still possible without turning each scalar into a separate milestone.
3. **Near-city attachment.** Add the already planned limited support/podium/service layer where the opening currently drops into dark disconnected space. These broad forms create actual depth and attachment that a reflection switch cannot supply. Light their relevant faces using a restrained, source-motivated treatment so the change survives ordinary chase framing.

The expected visible difference is a road with an intentional sequence of light/dark zones and broad broken satin response, supported by tangible near/middle construction. This is a stronger route toward the target image than asking a reflection pass to add detail to otherwise unchanged composition. It does not promise reflected lamps or buildings; that requirement stays separate.

Before implementation, choose two ordinary full-frame views and annotate exactly which road region, surface boundary and support connection should change. After one candidate, ask independent critics to identify those improvements in unlabelled whole-frame images, then watch the passage continuously. If they cannot readily identify improved road depth/light rhythm and scene attachment, reject the package before rollout. Permit only the plan's bounded substantial corrections, not sequential tiny retunes. Run wider regression and isolated performance checks only after a visible gain survives motion.

This approach requires a recorded revision to the current sequential milestone plan: tasks 2 and the limited opening portion of 3 become one reviewable sector deliverable, with component evidence retained. It is a recommendation for the integrator and owner to adopt, not a claim that implementation or acceptance has occurred.
