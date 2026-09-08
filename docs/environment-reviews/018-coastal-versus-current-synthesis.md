# Coastal versus current: comparative visual review

User clarification: “first try” means the earlier bright coastal version. Review date: 2026-09-08. This is a review only; no runtime or asset change is made.

**Verdict: the coastal version has the stronger overall visual foundation.** The current build improves specific assets, construction details, UI and technical behavior, but its open-track images convey less depth, material variety and distinctive place. Neither version establishes an AAA-quality finish. More completed correction steps do not establish a better overall image.

[Unaltered side-by-side native comparison](coastal-current-comparison.html) · [Independent visual critique 017](017-first-versus-current-independent.md)

## What changed perceptually

| Priority | Coastal native view | Current native view | Why this matters |
| --- | --- | --- | --- |
| 1. Broad lighting and material separation | Sunlit white structures, shaded dark track, blue water and brown rock are distinct large areas. Shadows and highlights reveal thickness. | Road, barrier bodies, building faces and ground frequently occupy similar dark tones. Emissive strips/windows carry disproportionate contrast. | Construction and material detail become hard to perceive even when modeled. Extra texture on a dark surface has little effect on the full image. |
| 2. Visible world and foundations | Water, cliffs, tower platforms and exposed track piers give the elevated course a visible context. The world is sparse, but its major relationships read. | The city contains more geometry, yet many bases and service levels disappear into black. Isolated lit outlines can make the track and buildings seem suspended. | More objects do not automatically create convincing scale or depth. |
| 3. Architectural identity and composition | The ocean horizon, irregular cliffs, white towers and sweeping track form a recognizable coastal racing setting. | Much of the view consists of dark rectangular masses, window grids and repeated streetlamps. The new mast and thermal plant are recognizable exceptions. | Repetition is conspicuous, and large distinctive forms compete with fine repeated marks. The scene feels more generated and less deliberately composed. |
| 4. Ship presentation | The earlier craft displays strongly separated cowls, intakes, fins, open gaps and engine hardware from the chase view. | The current craft's broad pale pods can read flatter and bulkier under the available highlights; smaller construction details recede. | The player's main object can feel less mechanically rich despite improved surface cleanup and authored maps. This combines shape and lighting; it does not prove that geometry alone regressed. |
| 5. Whole-scene consistency | The simpler setting shares a consistent bright architectural language across the sampled sections. | The warm gallery and newer service structures are more developed than the surrounding open city. | Quality varies between individual improved locations and the full course. |

The revised HUD is clearer and less dominant. Gallery construction, thermal entrance/pipe supports, road artifact corrections and race behavior are real gains. A wholesale rollback would discard useful work.

## Supported technical contributors

The source comparison uses the coastal delivery commit `3351c38` and the current delivered commit `a948825`:

- `ChaseCamera.cs` is byte-identical between those commits. A newly introduced camera-code regression is not supported. Actual framing can still differ with speed, banking, pose and the changed ship geometry.
- The broad directional key changes from warm intensity 1.65 / shadow strength .75 to cool intensity .62 / shadow strength .4. Ambient RGB changes from (.36,.43,.49) to (.095,.115,.155). Fog changes from a light blue .0007-density field to a dark blue .0015-density field. These source facts support the observed darker image structure; they do not isolate the contribution of each parameter.
- ACES tonemapping and high-quality SMAA were already present in the coastal build. The difference is not explained by simply adding a missing post-processing checkbox.
- The actual player preset changes from 78/108 m/s cruise/boost to 53/72, while the automated three-lap result changes from 112.64 to 128.32 seconds. This may contribute to a calmer experience. It is a secondary hypothesis: no continuous playback comparison was performed, and the similarly located crest/descent stills have much closer speeds. It does not explain away their visual hierarchy differences.

## Where our review process fell short

We repeatedly checked whether a specific defect was repaired, whether a new asset read better, and whether tests/performance remained sound. Those are necessary checks. We did not consistently compare the complete presentation against the stronger coastal baseline. Local acceptance accumulated without a sustained overall art-direction gate. That allowed detailed individual improvements to coexist with a weaker whole image. Parent takes responsibility for that gap; the earlier reports' bounded passes should not be presented as an overall AAA-quality verdict.

## Recommended next experiment

Restore the coastal visual direction as the leading candidate, while keeping the useful current systems and assets. First make one short native comparison passage with current ship and race settings fixed: daylight or late-afternoon coastal lighting, a readable ocean/ground plane, bright structural masses and one composed landmark. Compare it with the current night passage before further asset production. Give the full frame, silhouette and visible material separation priority over microdetail. Reassess ship form under that light before rebuilding or reverting it.

This is a proposal, not an implemented change. If the owner chooses to retain night, the same whole-image problems must be solved through stronger surface illumination, depth layers and a less repetitive skyline; raising exposure globally is not sufficient evidence of a fix.

## Evidence limits

Parent directly inspected coastal `run-09-coastal/03-race`, `05-crest` and `06-city-descent`; current final-performance `05-crest` and `06-city-descent`; and current candidate frames 175 and 641. Previously inspected current gallery, thermal and station views inform the listed gains. The independent critic has a separate named image set in 017. The paired crest and descent images come from the same harness's route-region triggers but are not exact camera/pose matches. Both sets are actual 1920×1080 native rendering; generated references are excluded. No human driving, continuous motion, audio or external commercial-game benchmark is claimed.
