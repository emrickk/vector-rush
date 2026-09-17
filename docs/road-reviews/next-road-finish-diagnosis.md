# Next road finish: isolate lamp-caster shadows after the landmarks

2026-09-08. **The conspicuous L-shaped band in the latest exterior reveal aligns with the moon shadow of the nearby streetlamp. Test that caster before changing road triangles or roughness again.** This finding explains a specific current shape, not every polygon in the earlier damp-road experiments. No runtime source, assets or native build were changed for this investigation, and no Unity or GPU job was launched.

## The strongest current evidence

The [latest native reveal](../../evidence/close-race-final/full-lap/05-reveal.png) shows a dark transverse strip below the craft that turns diagonally toward the right barrier. Its [native report](../../evidence/close-race-final/full-lap/environment-evidence.json) identifies build `7da2883ade4f4223b7a611db8cf60eb6`, frame 1004, race time 41.958 s and progress 0.9462143.

[NightTrackLighting](../../UnityProject/Assets/Scripts/World/NightTrackLighting.cs) places road fixture 55 at progress `55 / 58 = 0.9482759`, on the right, 14 m from the centerline. Its pole reaches approximately 12.8 m above the road and its arm extends inward. These cubes are combined into `Track lighting steelwork`; the ordinary `MeshRenderer` retains its default shadow-casting behavior. The cool and amber diffuser meshes are separate combined renderers. A local spotlight's `shadows = None` only disables shadows from that light: it does not stop the physical fixture casting a shadow from the moon.

[WorldBuilder.BuildLighting](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs) creates `Midnight soft key` at Euler rotation (43, −28, 0), with soft shadows, intensity 0.62 and shadow strength 0.4. A CPU reconstruction used that light direction, the source fixture dimensions, the unchanged track spline/frame and the native reveal's actual camera pose/FOV. Projecting the fixture along the moon rays onto the local road gives:

| Fixture feature | Predicted shadow position in the 1920 × 1080 reveal |
| --- | --- |
| Pole base | Approximately (1707, 685), at the right edge |
| Pole top / arm junction | Approximately (1361, 807), at the bend in the dark L |
| Inner end of the arm | Approximately (902, 799), at the strip's left end |

Those three locations visually correspond to the three joints of the native dark shape. The projection approximates the local curved surface and uses double-precision reconstruction of Unity source calculations; it is not a captured shadow-buffer inspection or a pixel-error measurement. Nevertheless, it provides a concrete caster-and-light explanation that a triangle-shaped appearance alone did not supply. The current L-shaped shadow is the recommended test target.

## What the previous experiments actually established

I directly reopened seven original PNGs for this review: road-surface-01 baseline/calibrated-Lit/no-additional-lights; road-surface-02 no-additional-lights/no-sun-shadows; the current reveal above; and [landmark baseline frame 175](../../evidence/landmarks-baseline/progress-0.18-frame-0175.png). I also read the prior reports and source. I did not re-inspect every old control or watch full-speed playback.

- In [road diagnostic 002](../environment-reviews/002-road-diagnostic.md), the large foreground polygon survives an untextured calibrated Lit material, constant smoothness, neutral normal scale and removal of non-directional lights. The varying road textures and local-light overlap are therefore not necessary to produce that particular old boundary. The directional moon still illuminates and casts shadows in the local-light-off control.
- [Road diagnostic 003](../environment-reviews/003-road-normal-shadow-controls.md) does not conclusively exclude shadows. Its new gallery baseline already reproduces the old polygon weakly, with a different pose and gallery construction. Disabling sun shadows there did not establish a robust correction; nor did replacing the mesh normals. Global-up normals redirect the highlight but are incorrect for the banked road.
- [The dampness history](../road-dampness-diagnosis.md) correctly distinguishes a periodic-mask seam correction from the remaining diagonal issue. Narrowing smoothness subsequently reduced contrast; it did not establish a root-cause fix.

The current exterior L should not be conflated with the earlier large cool foreground polygon. Its source-aligned lamp projection is a new, stronger diagnosis of the current band. A positive native caster control would establish this contribution without retroactively proving the cause of every previous artifact.

## Current material and geometry facts

The road uses standard **URP Lit**, not a custom road shader. `RoadSurface.mat` enables `_ENVIRONMENTREFLECTIONS_OFF` and sets `_EnvironmentReflections = 0`; direct specular highlights and received shadows remain enabled. A live scene reflection probe therefore does not establish that reflection-probe imagery causes this road band. The installed Lit code computes view direction from interpolated world position, normalizes the normal per pixel and evaluates direct lighting with shadow attenuation.

The runtime builder overrides the template's saved smoothness: multiplier **0.90**, periodic mask alpha **0.35–0.50**, yielding effective smoothness approximately **0.315–0.45**. Editing only the material asset's saved `_Smoothness = 0.16` would not alter that runtime multiplier. The aggregate color, fine relief and satin mask are generated at runtime; `_BumpScale` is 0.25. This is already a deliberately restrained finish, so another roughness reduction risks dulling useful light pools while leaving the shadow shape intact.

The running ribbon has **960 longitudinal cells × 12 lateral cells**, 12,493 shared vertices and 23,040 triangles. Every quad uses the same diagonal, with recalculated normals and tangents. Triangulation/interpolation remains a testable possibility for other shapes, but native triangle-edge correspondence has not been established. An exploratory CPU projection of the old material-test view did not establish a precise vertex-to-polygon match. Do not label the road flat shaded or remesh it solely because a band is angular.

## One bounded experiment, after landmark acceptance

Use a dedicated opt-in **fixture shadow A → B → A** capture on the newly integrated landmark scene. Do not run the old 12/17-condition matrix.

1. Reach a conspicuous current exterior band through the ordinary testing driver, at approximately **progress 0.94585–0.95**. Expose an explicit target-progress flag; retain 0.88475 as the legacy diagnostic default. Freeze the actual rendered camera and racer poses only once the new target is reached. First verify that the band is visible in A; a weak target is inconclusive.
2. Capture A with the native settings. Record the exact three fixture-renderer identities and actual `shadowCastingMode`, the moon identity/direction/shadow mode, camera/FOV, runtime road material values and mesh/collider identity.
3. For B, change only the shadow-casting mode of **`Track lighting steelwork`, `Cool linear road lamps`, and `Amber linear road lamps`** to `Off`. These three renderers comprise one fixture-caster family. Their visible geometry, materials, emission and local lights remain enabled; the moon and every other caster remain unchanged. Including the diffuser meshes prevents their narrower shadow strips from obscuring interpretation of the housing control.
4. Restore each original shadow-casting mode and capture A again. Read back the effective modes in all three states, verify camera/pose and road attributes remain fixed, and use a fresh output directory. Missing or ambiguous renderer matches must fail the diagnostic rather than silently test an incomplete family.

The road mesh, normal/tangent/UV arrays, material maps, smoothness, collision mesh, race physics, exposure, AO, global shadows and new landmark geometry stay fixed. No collider or gameplay change is needed. This is a targeted caster experiment, not global shadow suppression.

## Decision and finish criteria

If the native L disappears or loses its pole-and-arm shape in B and returns in restored A, the fixture shadows contribute to that band. The supported shipping adjustment is then limited to those fixture casters, subject to a short ordinary/boosted exterior review: preserve useful road highlights, visible fixtures, landmark depth and vehicle contact grounding. It remains a deliberate shadow-composition adjustment, not a repair to nonexistent broken road triangles.

If a residual band remains, distinguish it from the removed L by position and shape. Do not attribute every residual boundary to the same cause. If the target itself stays unchanged, this caster family is not sufficient; preserve the negative result. Only then consider the previously discussed render-only diagonal-flip A/B/A, with collider and vertex attributes unchanged, as a separate authorized follow-up. Do not add that test to this initial experiment.

Successful stationary restoration establishes causality for the selected shape. Stable moving highlights, road texture scale and overall finish still require native motion review after the landmarks. This read-only report does not claim those visual passes or the implementation of the proposed control.
