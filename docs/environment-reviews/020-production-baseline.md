# Environment review 020: opening-city production baseline

Independent baseline review, 2026-09-08. **Prioritize broad outdoor lighting, distance separation and a more convincing road response at 149, 211 and 283.** The route is readable, but small luminous strips carry much of the picture while large surfaces remain subdued. The gallery and thermal materials already demonstrate useful local shading; they cannot substitute for improvement across the opening outdoor views.

## Evidence and scope

Directly inspected eight original **1920 × 1080** images: opening frames **149, 211, 283, 398 and 472**, plus controls **606, 641 and 1004**. The [selection manifest](../../evidence/night-production/baseline/selection.json) identifies baseline GUID `5c49b92643bd4d41820ba81857fdf8c7`, source delivery `a948825`. All eight selected copies were verified byte-identical to their original sequence frames by SHA-256.

These are selections from the already completed [native capture](../../evidence/lighting-depth-candidate-01/full-lap/environment-evidence.json), which finished at 20:08:17 UTC on Unity 6000.6.0f1 / Apple M2 Max. It used ordinary hover physics and automated steering at 24 simulation fps. The five opening images are natural crossings of .15, .22, .30, .37 and .43; the existing station anchors are separate controls.

The four supplied-video samples directly inspected for [review 019](019-production-finish-plan-critique.md) provide mechanisms: broad reflected color, atmospheric skyline layers and readable nearby vehicles. They are not exposure settings or proof of a particular rendering technique. No continuous playback, manual driving, audio, alternate aspects or performance was reviewed here. No runtime, assets or Unity state was changed.

## Per-view diagnosis

| View | What the native image shows | Lighting-stage priority |
| --- | --- | --- |
| [149](../../evidence/night-production/baseline/selected/frame-0149.png), .1511, 6.34 s | The cyan boundary explains the turn. Mast faces read, but track supports and middle city largely merge into darkness. The road has subdued pools with little broad colored response. Pale pods separate from it; canopy and inner hardware merge. | Establish a clearer relationship between foreground road, existing support faces and distant city. Preserve the mast's dark internal gap and restrained occupied room. |
| [211](../../evidence/night-production/baseline/selected/frame-0211.png), .2204, 8.92 s | A large road area leads toward the gate, with weak cool shading between obvious joints. Left support faces descend into black surroundings; the close right mast face is readable but fairly flat. White hull surfaces dominate over internal material distinctions. | Give the road useful broad shading and separate near structure from background. More sky brightness alone would leave the construction unresolved. |
| [283](../../evidence/night-production/baseline/selected/frame-0283.png), .3004, 11.92 s | This is the strongest open-city test. Small window rows and lamps dominate dark building masses; little atmospheric separation explains the distant skyline. Warm light tints the ship and foreground road, but the deck still reads as large smooth panels. | Show substantial illuminated surfaces and coherent cool/warm road response while retaining the bend, dark joints and neutral areas of the craft. |
| [398](../../evidence/night-production/baseline/selected/frame-0398.png), .3706, 16.71 s | Gallery wall lights already reveal volume ahead. The near strut and surrounding construction remain very dark; the broad curved deck has little material variation. The craft sits close to the left boundary at a lower recorded speed. | Improve existing surface separation without obscuring the curve with glare. Do not treat lighting as a correction for this driving position or the obstructing geometry. |
| [472](../../evidence/night-production/baseline/selected/frame-0472.png), .4301, 19.80 s | Portal framing and local lighting give this view more depth. Outside, the road returns to weak blue pools and the skyline is mainly dark faces with small windows. The hull is clear but its broad casing remains evenly pale. | Carry the improved outdoor hierarchy beyond the lit portal. The strong local wall lights must not become the sole evidence of progress. |

## Controls to preserve

[606](../../evidence/night-production/baseline/selected/frame-0606.png) already separates the drums through broad curved highlights and reveals some pipe/support connections. [641](../../evidence/night-production/baseline/selected/frame-0641.png) makes pipe curvature, warm couplings and a support face particularly clear. Preserve that material distinction and avoid washing the lit support cap or drum faces into uniform brightness.

[1004](../../evidence/night-production/baseline/selected/frame-1004.png) preserves warm workshop depth and illuminated canopy supports beside a clear road. A global cool/ambient change must retain those warm recesses and their contrast with outer surfaces. This boost view also checks that the player and HUD remain distinct.

## Six experience dimensions

| Dimension | Baseline judgment | Evidence limit |
| --- | --- | --- |
| Composition | **Below** | Clear route, but broad masses and ordinary city views have weak hierarchy. |
| Depth | **Below** | Near structure, middle city and distance often share similar dark values. |
| Surface response | **Below** | Some local shading works; the opening road lacks the reference's broad light response. |
| Craft read | **Approaching** | Recognizable silhouette and engines; casing, canopy and hardware separation remains uneven. |
| Visible racing | **Below in these views** | No nearby rival is visible in the five opening stills. Sustained racing was not observed. |
| Motion clarity | **Pending** | No continuous native passage was watched for this review. |

## Lighting-stage acceptance

1. Demonstrate visible improvement in **all three open views 149/211/283**, with coherent near/middle/far separation and meaningful surface shapes. Improvement confined to gallery lighting does not pass this stage.
2. Compare the planned direct-light and retained-reflection candidates with their differences recorded. Broad highlights/reflections must relate plausibly to scene sources, preserve road roughness and joints, and avoid mirror-like uniformity or conspicuous diagonal bands. Temporal stability remains pending until actually observed in motion.
3. Preserve clear road edges, usable turn visibility, hull curvature and cockpit/hardware contrast across all five anchors. More bloom, brighter rails or uniformly colored hull surfaces are not sufficient evidence.
4. Keep all three controls readable and retain their existing warm/cool material distinction. Record camera and speed differences honestly when comparing new natural crossings.

This stage accepts lighting/road progress only. Continuous city connections, new geometry, craft-form changes, rival distribution and the cause of the slowdown belong to later stages. The complete six-dimension target, crowded-case effects diagnosis, watched motion and manual/audio gates remain open.
