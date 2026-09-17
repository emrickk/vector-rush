# Independent night visual critique 02

**Static review; motion assessment pending.** Directly inspected native `evidence/night-02/01-title.png`, `02-start.png`, `05-crest.png` and `06-city-descent.png`, matched against the same night-01 views. Read selected current facade shader, lighting and saved-scene settings. Current source can be ahead of the captured build; source intent is not treated as proof of a rendered improvement.

**Verdict: substantial improvement, but the requested high-quality visual target still fails. No AAA pass is earned.** The second pass now reads as a night race inside a city instead of a sparse track in a void. It remains visibly procedural, with flattened depth, repetitive buildings and lighting that needs stronger composition. Functional race acceptance is separate.

## Changes that visibly worked

- The start, crest and descent now have surrounding urban masses at several heights. Near service blocks and the more populated skyline establish much better occupancy and scale.
- The road is substantially more readable. Its running surface, crest and banking survive beyond the immediate engine glow. The ship's pale armor separates from its dark open frame, and the green rival is distinguishable. Broad local sheen is visible without the earlier severe reflective cells.
- The obvious hard exhaust cone shells are gone in these static frames. Compact engines and softer trailing light are a clear improvement. Fine plume stability still needs motion review.
- Darker interface panels compete less with the night scene. The title and numeric HUD remain readable.

## Three priorities for the next targeted pass

### 1. Establish atmospheric depth and a clear foreground/background hierarchy

The distant skyline has almost the same crisp window edges, saturation and dark/bright contrast as the nearer buildings. At the crest and descent, dozens of lit facades collapse into a busy wall of window patterns. Building overlap exists geometrically but does not consistently read as distance. This is the largest remaining image problem.

First verify effective fog in the native build using matched views with a deliberately obvious temporary diagnostic density, then restore a restrained artistic value. The current facade shader contains `ComputeFogFactor` and `MixFog`, while the saved bootstrap scene reports fog disabled. Fog-variant stripping is a plausible explanation to test; the screenshots and source alone do not prove it. Do not describe a settings change as a fix until the actual distant facades visibly respond.

After fog is verified, lower distant window contrast/brightness and let the nearest few masses carry the sharper highlights. Preserve a readable deep blue sky/ground transition. **Acceptance:** start/crest/descent each have a crisp near layer, a distinct middle layer and quieter atmospheric background, without bleaching the foreground or hiding the route. A uniformly gray veil is not the objective.

### 2. Reduce repeated building profiles and make nearby facades read as architecture

The added density solves emptiness but exposes repetition. Many towers share the same stepped rectangular profile and near-black surfaces covered with regular warm/blue window grids. The near blocks look like texture-covered boxes; distant black roof caps repeat conspicuously. A fog pass will soften distance, but it will not resolve the nearest architecture.

Concentrate on the most visible right-side start buildings and the buildings below the descent: give these a few materially distinct profiles, readable setbacks/crown treatments, a subdued illuminated facade plane, and structural breaks around the windows. Retain coherent occupied floor groups and larger dark areas. Preserve some skyline gaps instead of distributing equal brightness and density across the whole horizon. **Acceptance:** nearby buildings have recognizable mass and construction, and the silhouette no longer repeats the same stepped block at every height. These selected improvements must be visible from the race camera; hidden detail earns no credit.

### 3. Shape the improved road illumination into a deliberate lighting rhythm

The road is no longer too dark, which should be preserved. However, large stretches now become a fairly even gray-blue textured carpet. The strong pool at the descent is readable, but the next sequence of light and dark intervals is still weak, and the elongated blue engine highlights compete with the damp surface grain. The broad deck could communicate speed and curvature more effectively.

Tune the existing lamps' effective pool overlap and intensity along the photographed bend, keeping soft damp highlights tied to those sources. Allow controlled darker intervals and occasional warmer sectors rather than a uniform lift. Keep the ship's connected pearl body planes through the transition; do not return to the night-01 crushed silhouette. Reduce grain/normal emphasis if it competes with the light pool shape. **Acceptance:** motion through consecutive lamps reveals a clear but comfortable illumination rhythm, with stable sheen and no distracting exposure pulses or return of the earlier reflective-cell defect.

## Evidence still required

The four stills do not establish parallax quality, close-pack camera clearance, window shimmer, temporal light selection, plume stability or lighting-transition comfort. A short native moving passage through the start pack, a light pool and the dense descent should be reviewed before a final motion verdict. The final assessment must retain the improvement/failure distinction: this pass is much better than night-01, and the visual target remains unmet.
