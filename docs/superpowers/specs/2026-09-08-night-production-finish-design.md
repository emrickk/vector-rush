# Night racing production finish: design and acceptance

**Direction confirmed by the owner:** retain the current night scene and bring the same racing concept toward the supplied video's integrated level of finish. This supersedes the earlier recommendation to try coastal restoration first. The owner subsequently authorized Stage 1. Its partial implementation is now paused at the owner's wrap-up request; see [the handoff](../../HANDOFF.md).

## Global constraints

- Unity 6000.6.0f1, URP 17.6.0, Apple Silicon macOS; retain the existing Forward+ pipeline.
- Keep the current night direction, six racers, three laps, useful HUD and original asset identity.
- Freeze route, player physics, AI, camera and craft geometry through the environment/material stages; consider bounded motion changes only in Task 5 of the implementation plan.
- Preserve the three road-fixture renderer groups with shadow casting disabled.
- Keep editable source, Unity metadata, prior deliveries, rejected candidates and published history.
- No phase/shield system, eight-racer expansion, full HUD replacement or engine migration.
- Review five natural route crossings at progress .15, .22, .30, .37 and .43, plus thermal and final-station controls.
- Native visual evidence uses ordinary physics and the racing camera; injected poses and recording-only pace changes cannot establish acceptance.
- Observed motion, manual play with audio and technical checks are separate gates; unobserved coverage remains pending.
- Target 1920×1080 on Apple M2 Max; verify 1280×800 and 1920×810 presentation.
- Proposed working performance budget: P95 ≤ 12 ms and P99 ≤ 16.7 ms; review regression over 25% against a fresh comparable baseline.
- No Editor build, baking or video encoding overlaps a performance sample.
- Allow at most two substantial correction rounds before revisiting the diagnosis or technique; a critical below-target judgment blocks circuit expansion.
- Commit and push completed source/evidence milestones; native release publication remains a separately authorized action.

## Intended experience

A fast, readable race through a substantial illuminated city. Light reveals the road and construction. Near structures frame the bends, a middle district gives the route context, and atmospheric distance gives the skyline scale. The craft feels engineered, opponents are visibly involved, and effects strengthen acceleration and contact without obscuring the race.

[Reference mechanisms and identity](../../../references/night-production-video.md) · [Current comparative diagnosis](../../environment-reviews/018-coastal-versus-current-synthesis.md)

Production finish here means consistent presentation of the current playable circuit. It is an acceptance target to earn in the native game, not a label earned by polygon count, shaders or passing tests.

## What we preserve

Keep the current project, its six-racer race, useful HUD, original assets, authored landmarks, steering/guard corrections, throttle-responsive propulsion and completed diagnostics. Keep both the historical coastal and current night deliveries intact. Preserve Unity metadata, editable source and published history. Do not rebuild the game, migrate engine, replace the whole craft or restore the coast as an automatic first step.

The supplied video's phase ability, shields, eight-racer count and exact HUD are not scope. No new gameplay system is needed to establish this visual standard. Bounded changes to ordinary starting order, rivalry, camera and presentation may be evaluated in the later motion/racing stage when observations show a specific need.

## One representative benchmark

Use the opening city bends from the mast approach through the first gallery portal. The current complete native sequence's frames 121–480 give a 360-frame / 15-second passage, approximately progress .119–.439 and race time 5.17–20.13 s. Most of the passage is open city, so success cannot be confined to the already stronger enclosed gallery. Use five primary stills near progress .15, .22, .30, .37, .43; identify the first natural crossing in each newly captured sequence. Frame numbers are baseline identifiers, not a license to inject camera/racer poses.

The current passage includes pronounced slowdown near progress .33–.39. Preserve that in the baseline and assess it during motion work; do not choose only flattering high-speed frames. Add short thermal-service and final-station control views to expose harmful global lighting changes outside the benchmark. After race or camera changes, compare by route progress and label pose/speed differences honestly.

## Visual decisions

| Area | Intended result | Guard against |
| --- | --- | --- |
| Light and atmosphere | Cool teal/blue distance; readable neutral road/craft surfaces; a few warm service lights and restrained magenta district accents. A few large lit surfaces dominate the picture. | Global brightening, uniform fog, or many equally bright lamps masking dark construction. |
| Road | Satin-to-damp response carrying broad, coherent colored highlights and reflections, with restrained roughness variation and visible joints. | Mirror-like track everywhere, false reflection geometry, revived diagonal bands, tiled patterns or light flicker. |
| City | Continuous visible track supports/service levels, near/middle/far layers, a composed mast district and varied large silhouettes. | More isolated buildings, a black void beneath them, sharp repeated window grids competing with the course. |
| Craft and rivals | Clear canopy/hull/hardware separation, controlled specular shape and readable team identities at race distance. | Broad flat white casing, a new complete ship rebuild before lighting is assessed, or opponents represented only by HUD distance text. |
| Motion and effects | Strong passing scenery, restrained camera response, throttle/boost distinction and visible pursuit/side-by-side action. | Huge speed numbers, exaggerated blur/roll, giant exhaust or scene cuts substituting for actual racing. |
| Sound and UI | Engine, airflow, boost, rivals and impacts form a coherent mix; essential HUD remains legible in busy moments. | A new HUD redesign, decorative center-screen announcements or audio quality inferred from silent footage. |

The exact light intensities, reflection technique, exposure and surface values are resolved through bounded native comparisons. They are not fixed from the supplied video's pixels or copied from its speed display.

## Existing technical facts that shape the work

- Unity 6000.6.0f1, URP 17.6.0, Apple Silicon macOS; existing Forward+ renderer. Do not add a rendering dependency or migrate the pipeline without a demonstrated need.
- `UnityProject/Assets/Resources/RoadSurface.mat` currently enables `_ENVIRONMENTREFLECTIONS_OFF`, with `_EnvironmentReflections=0` and direct specular highlights enabled. WorldBuilder's packed road smoothness is approximately .315–.45 after its .9 multiplier. A glossy texture alone cannot produce the reference's environmental reflections.
- One 256 px scene reflection probe covers a large volume and captures once. Start with existing Lit/direct highlights and a bounded, retained probe-reflection variant. Verify parallax and shape honestly before choosing a more expensive method. Do not promise planar or screen-space reflections before a native feasibility/performance check.
- Preserve the proven road-fixture caster correction. Its three renderer groups remain non-casting; enabling their shadows again is not part of polishing the road. Other earlier road-band causes were not all established.
- NightWindows already mixes fog, but does not evaluate additional lights or reflection-probe specular. Improve its emission/depth hierarchy deliberately; adding local lights alone will not change those panes as it changes Lit structures.
- The current Forward+ configuration does not establish reliable craft exclusion through ordinary Light.cullingMask. Judge spill in native frames and use a supported isolation method only when needed.
- Retain serialized shader/material variants for any new reflection or texture state. A runtime keyword or `shader.isSupported` alone is not proof that a native build contains the required variant.
- The current mast has a documented tangent limitation. Keep its present color and metallic/smoothness finish path unless a separate geometry audit justifies normal maps.

## Acceptance: three separate gates

### A. Whole-image visual gate

At each of the five ordinary benchmark views, the next road direction is clear; craft, road, nearby construction and skyline have readable separation. At least one visible support/service-level relationship grounds the course where the camera exposes it. Building faces and large masses remain meaningful without reading individual windows or signs. The road reflects or highlights a plausible source without concealing the bend or replacing dark joints with glowing bands. Craft shape and any visible opponent remain identifiable through bright effects.

An independent reviewer compares current baseline, candidate and supplied reference mechanisms. Each of six dimensions—composition, depth, surface response, craft read, visible racing and motion clarity—receives **below / approaching / meets the target**, with named frame/time evidence. These labels are judgments, not a numeric score averaged into a pass. All six dimensions are required: “approaching” records progress but does not close the benchmark, and “below” or pending observation blocks expansion. Task 6 closes only when all six meet the stated target and the other two gates pass. The previously stronger coastal image remains a sanity check for readability, while night remains the chosen direction.

### B. Motion and play gate

A reviewer actually watches the complete benchmark route passage at normal speed and a continuous full lap. The baseline lasts 15 seconds; later pace changes retain the same route interval at its true recorded duration, without retiming it. Record who watched, exact build/video, playback speed and any observed defect times. Sampled frames and a successful video decoder do not satisfy this gate. If no available tool/reviewer can watch, leave the gate pending and provide the playable clip for the owner; never manufacture acceptance.

The race must show at least two genuine pursuit or alongside moments beyond the launch during the first lap. The benchmark should contain a visible rival through a meaningful approach/bend, not merely a distance label. Actual ordinary starting order or controller tuning may be changed in its own measured stage to achieve this; no teleporting, injected positions, artificial recording-only speed, or edits that pretend separate moments are continuous. Verify that the severe baseline slowdown is understood and acceptable or corrected through real driving behavior.

A separate manual drive covers acceleration, coasting, braking, boost, both galleries, contact, recovery, pause, restart and finish with audio enabled. Check that decorative effects do not hide the turn, craft or rival. A documented equivalent diagnostic with bloom, motion blur, speed streaks, extra trails, sparks and exaggerated additive exhaust suppressed must retain useful scene structure; it is a control, not the delivery setting. Navigation markings and functional HUD stay on.

### C. Technical and delivery gate

Use the actual native build at 1920×1080 on Apple M2 Max, plus 1280×800 and 1920×810 presentation checks. Keep simulations, pose freezes, controlled diagnostics, human driving and real-time performance labeled separately. Final existing tests must pass; add behavior tests only for changed logic with meaningful failure cases. Keep the all-six-racer zero-recovery and restart/countdown-pause checks.

Current observed baseline is mean 8.35 ms / P95 9.21 ms / P99 9.33 ms over 7,184 intervals. Initial working budget: P95 ≤ 12 ms and P99 ≤ 16.7 ms at the same display setup, with no persistent hitching. This is a proposed production budget, not a measured result or promise. Refresh baseline under the same conditions; review increases over 25% even if within budget. If the target cannot be achieved, reduce the costly contribution or make the quality/frame-rate tradeoff explicit before expanding it. Preserve poor first runs; do not cherry-pick repeats or attribute hitches to unrelated processes without evidence.

No Editor build, baking or video encoding may overlap the performance run. Native source/build/archive identity, video decoding and PNG integrity must pass. Publish source and completed milestones to the authorized repository; native release publication remains a separately authorized action.

## Iteration discipline

One integrator owns the whole image. Independent specialists can own separate files, but their local approvals do not close the benchmark. Allow at most two substantial correction rounds per benchmark candidate before revisiting the diagnosis or technique. Failure pauses expansion, not useful work on the identified cause. Once a changed technical check passes, repeat it only after a relevant change, failure or new concern.

After the benchmark meets visual, motion/play and technical gates, spread its material/light/architecture rules across the circuit in three district passes. Each district retains a distinct rhythm and quieter transitions. End with an uninterrupted full-lap review, including ordinary stretches and crowded moments, before calling the presentation consistent.
