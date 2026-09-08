# Stage 1: lighting and road decisions

## Candidate A — broad exterior light and atmospheric separation

The independent baseline identifies the open mast/crest views as the weak cases. This candidate retains the entire road material/mask/normal setup, camera, geometry, player/AI state and postprocessing. It changes one documented lighting environment:

| Contribution | Baseline | A |
| --- | --- | --- |
| Custom ambient SH RGB | .095, .115, .155 | .14, .16, .18 |
| Moon RGB / intensity | .64, .74, 1 / .62 | .78, .85, .92 / .78 |
| Exp2 fog RGB / density | .028, .045, .075 / .0015 | .041, .074, .086 / .0018 |
| Sky horizon RGB | .039, .053, .084 | .047, .081, .096 |
| Sky zenith RGB | .0015, .0028, .009 | .0015, .0035, .008 |
| Exterior cool road-pool RGB / intensity | .74, .86, 1 / 390 | .68, .86, .91 / 460 |

Warmer exterior pools, all gallery and landmark lights, fixture count/positions/range/angles, shadow strength, the three fixture caster-off groups, road roughness, bloom/exposure and hull materials remain fixed. The sky/fog change gives distant faces a quieter teal atmosphere; ambient and key light aim to reveal existing neutral surfaces; the existing broad exterior pools strengthen source-related road light.

A is a light/atmosphere candidate, not a one-variable causality experiment among its own lighting contributions. Candidate B will use A's exact lighting and road maps while changing only the retained road cubemap-sampling state initially. A's existing reflection-off keyword skips cubemap sampling but can retain the URP glossy-environment fallback; it must not be described as eliminating all indirect specular.

`BuildMac` calls `Prepare` internally. A single build invocation updates the scene's serialized Exp2 fog and retains required variants; a separate duplicate prepare invocation is unnecessary. Native source/build identity, five natural opening crossings and three global controls will determine acceptance. Neither this implementation nor a clean build establishes visual or motion acceptance.

## Technical bounds

The initial B comparison preserves the one-shot 256 px whole-circuit probe. A local-box fallback, if justified by observed projection error, requires the pipeline's serialized box-projection option as well as `ReflectionProbe.boxProjection`; a small volume alone does not guarantee a correct transition on the single large road renderer. Probe mip completion is asynchronous: AllFacesAtOnce is documented locally as nine frames, and its render ID can be checked with `IsFinishedRendering`. No per-frame probe rendering is planned.

## Status

Baseline complete and published. Candidate A implementation is ready for its native build and first comparison. Later construction, craft and racing stages are deferred.

## A native result

Build `9a03e9b1767f4b89abd9c3bf938f7898` completes 1,440 native frames. All PNG chunk CRC and stream-decompression checks pass; all 195 tracked source and 189 app hashes remain unchanged after capture. The five new natural crossings are149/211/283/398/473, with controls606/642/1004. Exact-pose comparisons all fail: usual camera offsets are0.1–0.2m, with1.8–1.9m at the shifted portal/thermal frames. Per-frame values are retained rather than called matched. Parent sees broader exterior surface light and preserved warm controls, with a still-shallow city and modest road response. Independent A review is underway; proceed with the planned single reflection-state comparison, preserving A as its control.
