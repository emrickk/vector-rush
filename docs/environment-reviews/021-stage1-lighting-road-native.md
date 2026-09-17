# Environment review 021: Stage 1 lighting and road candidates

Independent native image review. Baseline diagnosis and lighting-stage criteria remain in [review 020](020-production-baseline.md). Candidate sections preserve their separate evidence and verdicts. No continuous playback, manual driving, audio, alternate-aspect or performance acceptance is claimed here.

## Candidate A: atmosphere and direct light

Reviewed 2026-09-08. **A is a modest improvement and a useful fixed-lighting control for the planned B experiment. It does not close Stage 1.** Broader surface readability improves without an obvious harmful change to the warm controls, but the road response and city depth remain below the intended finish.

### Evidence

Directly inspected eight A originals against eight baseline originals, all **1920 × 1080**. Baseline indices **149/211/283/398/472/606/641/1004** correspond to A **149/211/283/398/473/606/642/1004**. All eight A selected copies were independently verified byte-identical to their source frames by SHA-256; baseline copies were checked in review 020.

The [validation report](../../evidence/night-production/lighting-road/A/capture-validation.json) records native completion, 1,440 validated PNGs and GUID `9a03e9b1767f4b89abd9c3bf938f7898`. The [native report](../../evidence/night-production/lighting-road/A/full-lap/environment-evidence.json) identifies Unity 6000.6.0f1 / Apple M2 Max and completion at 21:53:51 UTC. These are ordinary physics and racing-camera images from automated simulation capture.

**Every exact-pose tolerance check failed.** Camera differences are 0.105–0.221 m for the six unshifted index pairs. Baseline 472 versus A 473 differs by 1.818 m and 0.030 s; baseline 641 versus A 642 differs by 1.877 m and 0.040 s. Maximum camera-angle difference is 0.364° and maximum FOV difference 0.01857°. These are qualitative comparisons of nearby natural crossings, not pixel-matched lighting measurements.

### What changed visibly

| A view | Independent observation |
| --- | --- |
| [149](../../evidence/night-production/lighting-road/A/selected/frame-0149.png) | The deck, inner barrier and mast faces read more neutrally, with a wider visible road-light pool. The teal background separates silhouettes more clearly. Lower city and support surroundings remain mostly black. |
| [211](../../evidence/night-production/lighting-road/A/selected/frame-0211.png) | Broad road shading and the close mast face are easier to read. The hull loses some of its blue cast. The deck still reads as smooth panels, and the brighter background does not establish distinct city depth layers. |
| [283](../../evidence/night-production/lighting-road/A/selected/frame-0283.png) | The thermal podium face, nearby rooftops and road gain useful midtones. Warm road/craft illumination remains present alongside cooler areas. Dark building masses and repeated windows still dominate the skyline. |
| [398](../../evidence/night-production/lighting-road/A/selected/frame-0398.png) | Near strut faces and road curvature become more legible; engine interiors also separate slightly better. Existing gallery illumination remains coherent. The close boundary position and structural obstruction are not resolved by this lighting change. |
| [473](../../evidence/night-production/lighting-road/A/selected/frame-0473.png) | The road beyond the portal has broader readable shading and the hull stays distinct. The scene still returns to dark city faces against a brighter sky. Account for the larger camera displacement when assessing the portal edge and highlight positions. |

At [606](../../evidence/night-production/lighting-road/A/selected/frame-0606.png), the service deck becomes more readable while drums retain curved shading. At [642](../../evidence/night-production/lighting-road/A/selected/frame-0642.png), pipe highlights, warm couplings and support-face contrast remain useful. The hull is brighter beneath the nearby fixture; its exact contribution cannot be separated from the shifted view. No obvious loss of the thermal material hierarchy appears.

At [1004](../../evidence/night-production/lighting-road/A/selected/frame-1004.png), the workshop and canopy retain warm recess depth while their outer surfaces and the road gain visibility. Road boundaries, player and HUD remain clear across the inspected set. No new conspicuous diagonal road band or destructive broad glare was observed in these stills; temporal behavior is unverified.

### Remaining criterion and decision

The missing lighting-stage result is **broad, plausible surface response that gives the outdoor road convincing material depth**, together with stronger separation between existing middle and distant city masses. A mainly supplies brighter, more neutral lamp pools and a teal background. It has not demonstrated the supplied reference's coordinated reflected light and atmosphere.

Retain A for the controlled B comparison. Do not raise overall brightness or add luminous strips merely to make the difference larger. Assess whether B adds coherent road response while preserving joints, roughness, turn visibility and all three controls. City connections, rival distribution, craft geometry and the slowdown remain deferred work; their limitations must not become excuses to declare the lighting target met.

At the complete production target, composition, depth and road response remain **below**; craft read remains **approaching**. Visible racing is **below in these five opening stills**, and motion clarity remains **pending**. These ratings acknowledge A's improvement without converting it into integrated acceptance.

## Candidate B: retained road reflection sampling

Reviewed 2026-09-08. **Reject B as the road improvement. A remains the better visual control, and Stage 1 still requires correction.** B reduces broad road readability without demonstrating the intended reflected light response. Preserving nearby landmarks does not compensate for that failure.

### Evidence and comparison limits

Directly inspected all eight B originals against all eight A originals at **1920 × 1080**. B indices are **149/211/283/398/473/607/642/1004**; A uses 606 instead of 607. All sixteen selected copies were independently verified byte-identical to their respective full-lap source frames by SHA-256.

The [B validation report](../../evidence/night-production/lighting-road/B/capture-validation.json) records native completion, 1,440 validated PNGs, 197 unchanged source files, 189 unchanged app files and GUID `de942ca116044e6eb2141c36b547d8a3`. This review independently checked the selected copies, not every PNG or build file. The [native report](../../evidence/night-production/lighting-road/B/full-lap/environment-evidence.json) records Unity 6000.6.0f1 / Apple M2 Max and completion at 22:00:47 UTC. The supplied experiment changes retained road cubemap sampling with A lighting, maps and probe configuration held fixed; the images establish the visual outcome, not its precise shader cause.

All eight recorded B comparisons against the original baseline fail exact-pose tolerance. Separately calculated A-to-B camera differences are **0.106–0.221 m** for the seven same-index pairs and **1.870 m / 0.030 s** for A 606 versus B 607. Maximum camera-angle difference is 0.270° and maximum absolute FOV difference is 0.00459°. Boost and grounded states match throughout the selected pairs. These remain qualitative natural-crossing comparisons; the larger thermal-view displacement particularly limits highlight-position judgments.

### What B actually delivers

| B view | Independent comparison with A |
| --- | --- |
| [149](../../evidence/night-production/lighting-road/B/selected/frame-0149.png) | The bend loses the broad gray road midtones visible in A. Warm and cool pools remain, but do not acquire useful reflected source shapes. Bright lamps and the cyan edge gain relative dominance over the deck. |
| [211](../../evidence/night-production/lighting-road/B/selected/frame-0211.png) | The uphill road becomes nearly black between isolated pools. Panel joints fade with the surface. The nearby mast and skyline retain their previous appearance. |
| [283](../../evidence/night-production/lighting-road/B/selected/frame-0283.png) | The descent loses broad surface shading while the thermal podium and warm craft illumination remain readable. A few center marks stand out against the darker road; they do not establish richer material response. |
| [398](../../evidence/night-production/lighting-road/B/selected/frame-0398.png) | Gallery wall lighting and strut faces remain useful, but the road through the bend is darker. The stronger black gaps between pools provide no convincing reflection benefit. |
| [473](../../evidence/night-production/lighting-road/B/selected/frame-0473.png) | The open deck beyond the portal loses much of A's readable panel shading. The portal, rail and windows remain bright; broad road response is still absent. |

At [607](../../evidence/night-production/lighting-road/B/selected/frame-0607.png) and [642](../../evidence/night-production/lighting-road/B/selected/frame-0642.png), drum curvature, pipe highlights, warm couplings and support-face contrast remain legible. Their adjacent road darkens, particularly toward the crest. At [1004](../../evidence/night-production/lighting-road/B/selected/frame-1004.png), the warm workshop and canopy recesses survive; the near right-hand lamp pool remains, while the road farther ahead loses broad midtones. Player, rail and HUD remain distinguishable in all inspected views.

No inspected B frame demonstrates a useful new broad reflection that offsets this loss. No conspicuous new diagonal band or destructive glare was observed in these stills. Shimmer, reflection movement and continuous driving visibility remain untested.

### Required Stage 1 correction

1. **Recover the readable road surface and demonstrate the reflection benefit.** Use A as the visual control. Resolve the road material/probe response so broad illumination remains readable between lamps, joints and roughness remain visible, and reflected light has a plausible relationship to scene sources. A darker deck with unchanged soft pools fails this criterion.
2. **Establish depth in existing city masses.** Near, middle and far building faces still lack sufficient separation. Verify the rendered atmosphere and surface lighting, then demonstrate layered distance in the three open primary views while preserving the thermal and workshop controls. A brighter sky alone does not meet that criterion.

Preserve B as a failed experiment. Neither A nor B closes Stage 1, and the complete-production ratings above remain unchanged. Source-level diagnostics or later candidates require their own native evidence before receiving a verdict.

## Candidate C: corrected fog color assignment

Reviewed 2026-09-08. **Accept C's fog correction as a clear improvement in atmospheric depth. Retain it for the next controlled road experiment; Stage 1 remains open.** Distant buildings now recede through visible teal layers while close structures preserve contrast. C still carries B's weak road response.

### Evidence and diagnostic scope

Directly inspected eight C originals against all eight corresponding B originals at **1920 × 1080**. C uses **149/211/283/398/473/606/642/1004**; B uses 607 instead of 606. All eight C selected copies were independently verified byte-identical to their full-lap source frames by SHA-256. The preceding sections contain the independently inspected A and B comparisons.

The [C validation report](../../evidence/night-production/lighting-road/C/capture-validation.json) records native completion, 1,440 validated PNGs, 197 unchanged source files and 189 unchanged app files. The [native report](../../evidence/night-production/lighting-road/C/full-lap/environment-evidence.json) identifies GUID `1287761a66a64ff9bb7a952eb5519b0a`, Unity 6000.6.0f1 / Apple M2 Max and completion at 22:11:52 UTC. Full-sequence validation is reported evidence; this reviewer independently checked the selected copies.

C's supplied rendering change is the corrected fog color assignment, with density, lighting, sky, maps and reflection state held fixed. The [preceding diagnostic readback](../../evidence/night-production/lighting-road/fog-readback/fog-state.txt), from separate diagnostic build `1f13c130ce6f4d9895edf31998701007`, records shader fog RGB approximately `(0.003174, 0.006451, 0.007986)`. The [C readback](../../evidence/night-production/lighting-road/C/fog-readback/fog-state.txt) records `(0.041, 0.074, 0.086)`. Both record exponential-squared density `0.0018` and identical fog parameters. These supplied measurements support the diagnosis; the native images independently demonstrate the visible result.

All eight C comparisons against the original baseline fail exact-pose tolerance. Separately calculated B-to-C camera differences are **0.106–0.221 m** for the seven same-index pairs and **1.870 m / −0.030 s** for B 607 versus C 606. Maximum camera-angle difference is less than 0.373° and maximum absolute FOV difference is 0.00459°. Selected boost and grounded states match. This is a qualitative comparison of natural crossings, without watched motion or pixel-matched photometry.

### Visible depth gain and preserved controls

| C view | Independent observation against B |
| --- | --- |
| [149](../../evidence/night-production/lighting-road/C/selected/frame-0149.png) | Buildings behind the mast and beyond the left-hand viaduct now sit at visibly different depths. The near mast, supports and road edge retain stronger contrast than the softened towers. |
| [211](../../evidence/night-production/lighting-road/C/selected/frame-0211.png) | The left skyline separates into close windowed blocks and progressively softer background masses. The foreground mast remains solid, and the uphill route stays legible. |
| [283](../../evidence/night-production/lighting-road/C/selected/frame-0283.png) | The thermal podium and near rooftops stand forward of several atmospheric tower layers. Distant faces and windows lose contrast coherently. This is a substantial improvement over the similarly dark skyline masses in A and B. |
| [398](../../evidence/night-production/lighting-road/C/selected/frame-0398.png) | Background towers and ground lighting recede behind the gallery and nearby structural frames. Gallery wall pools and the dark foreground strut retain their existing contrast. |
| [473](../../evidence/night-production/lighting-road/C/selected/frame-0473.png) | Near right-hand blocks separate from the taller, softer towers behind them. The portal remains distinct and its illumination preserves local depth. |

At [606](../../evidence/night-production/lighting-road/C/selected/frame-0606.png), drum curvature, the service deck and coupling highlights survive while distant towers soften. At [642](../../evidence/night-production/lighting-road/C/selected/frame-0642.png), near pipe highlights and warm rings remain clear, and the viaduct separates more convincingly from buildings behind it. The [1004 workshop view](../../evidence/night-production/lighting-road/C/selected/frame-1004.png) gains a visibly receding skyline while preserving warm canopy recesses, workshop surfaces and the near road-light pool.

No obvious uniform wash of close objects or loss of player, rail or HUD contrast appears in this set. C nevertheless retains the nearly black deck between B's isolated light pools. It adds no useful broad reflected-source shape and does not recover A's broader panel shading.

### Decision and remaining Stage 1 criterion

The requested atmospheric separation of existing city masses is now demonstrated in the three open primary stills. Keep C's fog state fixed while correcting the road. The remaining visual blocker is **broad, plausible road response with readable surface joints and roughness**, supported by scene-related reflected light. That correction must preserve C's distance layers and all three thermal/workshop controls.

At the complete production target, depth advances to **approaching**. Composition and road response remain **below**; craft read remains **approaching**; visible racing remains **below in the five opening stills**; motion clarity remains **pending**. Black foreground gaps, repeated city forms and other deferred work still limit the whole scene. No reflection stability, uninterrupted playback, manual driving, audio or performance acceptance is claimed. Later probe experiments have not been reviewed here.

Only this review document was authored for the current review. No runtime source, assets, Unity process or commit was changed by the reviewer.
