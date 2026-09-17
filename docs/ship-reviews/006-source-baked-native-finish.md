# Independent ship review 006 — V3 finish integrated; advance with follow-ups

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: advance from material/import readiness into propulsion and whole-scene work, with named local finish follow-ups. Keep the major shape frozen.** This is a practical advancement decision under the user's clarified scope. It is not a final hero-asset, propulsion-motion or AAA pass.

## Evidence and identity

Directly inspected the source-finish [rear view](../../evidence/ship-v3/pass-03-finish/01-neutral-rear-quarter.png), the completed baked-runtime [rear](../../evidence/ship-v3/pass-03-baked-preview/01-baked-rear.png) and [chase](../../evidence/ship-v3/pass-03-baked-preview/02-baked-chase.png) previews, and all eight images in [native V3 evidence](../../evidence/ship-native-v3-01/inspection-scope.json):

| Native frame | Inspected scope |
| --- | --- |
| [00 — ordinary gameplay chase](../../evidence/ship-native-v3-01/00-native-gameplay-chase.png) | Normal player camera and HUD, automated input through normal physics; 224 km/h at race time 1.440 s. |
| [01 — rear three-quarter](../../evidence/ship-native-v3-01/01-studio-rear-three-quarter.png) | Full craft, rear markings, shell/structure/nozzle relationships. |
| [02 — front three-quarter](../../evidence/ship-native-v3-01/02-studio-front-three-quarter.png) | Forward form, intake placement, canopy and both nacelles. |
| [03 — starboard side](../../evidence/ship-native-v3-01/03-studio-starboard-side.png) | Low profile, connected large volumes and material separation. |
| [04 — orthographic top](../../evidence/ship-native-v3-01/04-studio-top-ORTHOGRAPHIC.png) | Planform, longitudinal gaps and livery placement. |
| [05 — engine emission OFF](../../evidence/ship-native-v3-01/05-studio-engine-emission-OFF.png) | Chamber, collar and core surfaces with emission disabled. |
| [06 — engine emission ON](../../evidence/ship-native-v3-01/06-studio-engine-emission-ON.png) | The same close pose with emission restored; no propulsion effect. |
| [07 — canopy close](../../evidence/ship-native-v3-01/07-studio-canopy-close.png) | Glazing response, sill, intake borders and nearby paint. |

The source/baked previews are 1600 × 1200 orthographic Cycles renders, 32 samples, no denoising and no bloom. Their fixed rear camera is (-8, 10, 6), target (0, 0, 0.2), scale 9.6; baked chase is (0, 10, 4.4), target (0, -0.8, 0.05). They are authoring evidence, not gameplay cameras. The [baked capture contract](../../evidence/ship-v3/pass-03-baked-preview/capture-contract.json) identifies the exact inspected runtime-mesh file.

Native images are actual 1920 × 1080 Unity 6000.6.0f1 captures on Apple M2 Max, captured 2026-09-08 04:12:25–04:12:33 UTC. Studio views retain the accepted fixed neutral rig, ACES at exposure 0, no bloom/vignette/fog and no propulsion effects. Only frame 05 temporarily disables material emission. Frame 00 uses the race scene. The [native identity record](../../evidence/ship-native-v3-01/asset-identity.json) reports build success and process exit 0; neither establishes visual quality.

| Asset stage | Recorded SHA-256 |
| --- | --- |
| `pass-03-finish/Kestrel07-v3.blend` | `e07cde687c143f73fba4a62cfbfae070286cc0edba0c426d259fd0efac83fcfc` |
| `pass-03-maps-01/Kestrel07-baked-materials.blend` | `31e9fa53449f1ca0b0441e3efc6d4470bae9c9ca7030490f6ace06d9864593b4` |
| Imported `HeroShip.fbx` | `0415a4a829347f6556f0f8ba1274ebbbd949b5bf6cf6ae128218c4bbd5eb436e` |
| Imported `HeroShipEngineAnchors.json` | `ade72d97ffe3c6ce32b360544272cc993199cf3026840503577ff7f356a0718d` |

The [map manifest](../../SourceAssets/hero-v3/pass-03-maps-01/material-manifest.json) matches the source-finish hash. It records 1024-pixel maps on existing runtime UVs for Ivory, Graphite, Metal and Ceramic: sRGB base color; linear metallic/smoothness with smoothness in alpha; occlusion in green; tangent normals in OpenGL +Y. The native scope's smoothness value of 1 on mapped materials is the map multiplier, not evidence that every surface has constant mirror smoothness.

Native/canonical bounds agree within the recorded floating-point precision, at 5.226 m wide × 1.520 m high × 6.885 m long. The native scope counts seven meshes and 37,686 triangles, while the staged audit counted 38,036. The 350-triangle difference is not resolved by this visual review. Record the import-cleanup explanation before describing triangle counts as identical; no large missing component or gross orientation/scale failure is visible in this set.

## What now supports advancement

The pale armor, near-black glazing, graphite body and metallic nozzle collars remain distinct through source, baked and native views. The coherent low twin-nacelle form survives integration. The main surfaces are still simple, but they no longer depend on a uniform teal/plastic response to communicate the craft.

The aft 07 number and opposite insignia are flush, legible and present in the ordinary native chase view. Restrained citron marks establish an identity without becoming raised plates. V2's nearly missing native numeral fills have not recurred. Fine paint-edge quality still needs attention at close range, but the primary number placement is ready.

The matched native engine pair establishes physical chamber depth, a restrained inner cyan annulus and a distinct compact near-white core. Frame 05 retains a pale blue core base color when emission is disabled; that alone is not a failed emission test. Frame 06 restores the luminous core without turning the neutral chamber into V2's broad turquoise ring. The baked chase view independently shows the same core/annulus distinction. The oblique rear view hides much of the core behind the chamber; it does not establish that the core is missing.

These improvements clear the current material/import advancement threshold. They do not require another broad nacelle/canopy redesign before the surrounding game can improve.

## Remaining failures, ordered by player-view benefit

1. **Propulsion volume is weak in ordinary chase.** In frame 00 the white main cores are readable, but blue-lit nozzle bowls and the blue road spill attract more attention than any short attached plasma volume. A nearby rival shows a narrow straight streak, which also does not provide the intended soft volumetric tail. Keep the physical core/annulus hierarchy; tune the runtime effect to have a compact bright origin and a visible short translucent body with a soft fade. Reduce interior/spill dominance as needed. Do not solve this by enlarging the physical white disk or adding more broad blue bloom. This still does not establish absence of effects or temporal behavior; it establishes poor volume readability in the captured player view.
2. **The lower canopy edge has a conspicuous angular surface/shading artifact.** Frame 07 shows a broad silver/gray patch with a jagged, stepped boundary along the black canopy near the sill. A related angular patch is already visible in source and baked rear views. It is therefore not only a race-scene lighting issue. The crown reflection is smoother, making the lower interruption more noticeable. Surface overlap, normals, material boundaries or reflections could contribute; the cause has not been isolated. Diagnose and clean this locally with the major silhouette unchanged. Escalate its priority if a close opponent or banking view exposes shimmer in normal play.
3. **Several manufactured borders remain ragged at close range.** The intake mouth in frame 07 and outer armor/nozzle boundary in frames 05/06 show pale/dark serrations rather than clean continuous edges. These may share a surface or shading cause with the canopy problem, but that is unproven. Check the actual adjacent surfaces and exported normals before adding detail. Fix visible border defects; do not restart primary form work.
4. **Fine finish remains simplified.** Broad armor panels, simple mounts and collars lack the restrained construction richness of reference C. Some citron boundaries on the dark spine are soft/blocky in the close native view. The current 1024-pixel coating atlas and multi-facet placement may contribute, but the image alone does not prove a cause. Prioritize only details that survive normal player or nearby-opponent size. Main livery and palette should remain stable while this backlog is addressed.

## Next bounded work and outstanding gates

The parent-owned next ship step should improve the native plume read, then capture matched idle, cruise and boost states plus a brief continuous passage with acceleration and banking. Judge attachment, fade, transition stability and nozzle readability in actual motion. A single still cannot pass those checks.

Local canopy/intake/rim diagnosis can proceed independently as a limited asset-finish correction. It must preserve the current large shape, coating contract and engine-anchor positions unless a demonstrated defect requires a documented change. Reuse the matched close cameras so that the correction can be reviewed directly.

Proceed with whole-scene lighting, road response and foreground environment work alongside those ship fixes. Frame 00 still shows crisp repeated window grids on dark simple building masses; the ship's improvement has not established a finished city. Use the separately scoped [night next-pass plan](../night-next-pass-plan.md) to select the environment work. Warm/cool/dark scene coverage, propulsion in motion, camera behavior and combined-game quality remain pending. Overall AAA quality remains unmet.
