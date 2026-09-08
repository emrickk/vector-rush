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

## Candidate B — retained road reflection state

B retains A's lighting, fog/sky, geometry, normals, generated road maps, smoothness, global probe configuration and gameplay. The running deck selects `RoadSurfaceReflections`, a serialized Resources material created in Editor setup from the unchanged control, with `_EnvironmentReflections=1` and `_ENVIRONMENTREFLECTIONS_OFF` removed. Copying the control properties preserves normal/metallic/emission keywords and direct specular; the native build must show the intended response. No local probe, blending, box projection, increased gloss or additional rendering technique is introduced in this initial B comparison.

## B native result and next diagnosis

Build `de942ca116044e6eb2141c36b547d8a3` completes1,440 frames with all PNG CRC/decompression checks and197-source/189-app hash checks passing. Selected indices are149/211/283/398/473/607/642/1004; exact-pose tolerance fails and remains explicit. Parent inspection of149/283 finds a darker road with no useful broad reflected-source shape. B is retained as a controlled comparison, not accepted. Independent B review follows.

The weak skyline separation warrants a separate fog-color readback before further brightness tuning. Local URP source consumes `unity_FogColor` directly; its upload/conversion is in native engine code. A's assigned fog RGB could be gamma-converted, but this is a hypothesis until actual native readback. The next diagnostic logs active color space, assigned fog, its linear conversion and resolved shader fog globals after rendering. A zero built-in global readback is inconclusive; it cannot establish black fog. A supported color-space correction, if verified, must preserve density, lights, sky and reflection state for the subsequent comparison.

## Candidate C — verified fog color-space correction

The native diagnostic build `1f13c130ce6f4d9895edf31998701007` resolves a nonzero `unity_FogColor` of `.00317427842, .00645073876, .007985829`, matching the assigned RGB's `.linear` conversion, in Linear rendering. This confirms that the fog was uploaded substantially darker than the intended linear sky palette. See [actual readback](fog-readback/fog-state.txt); the paused title fog-on/off images are diagnostic views, not racing acceptance.

C changes only the fog assignment to `new Color(.041f,.074f,.086f).gamma` in runtime and serialized setup, so the renderer should receive the intended `.041/.074/.086` linear color. Fog density, sky, moon, ambient, road material/reflection state and maps, geometry, physics, camera and effects remain B's. Repeat the native readback to verify the result, then capture ordinary racing for visual acceptance. C is a fog correction round, not an accepted road solution. After C, the permitted local-probe technique can be tested separately against this fixed atmosphere if the current global probe remains inadequate.

The C native readback confirms shader RGB `.0409999937/.0739999861/.08599998`, with unchanged Exp2 parameters, on GUID `1287761a66a64ff9bb7a952eb5519b0a`. The parent-inspected opening149 and descent283 show substantially clearer far/middle building separation; thermal642 retains pipe highlights and warm couplings. The road remains weak. Full-image integrity validation and independent review follow before accepting the atmosphere.

## Candidate D — local opening reflection environments

C's 1,440 PNGs and197-source/189-app identities validate; all eight exact-pose checks still fail. The corrected atmosphere is the fixed control for D. D adds two256px HDR probes at route`.19` and`.315`, five metres above the banked centreline, spanning sampled corridor intervals`.11–.27` and`.25–.39`. Influence bounds include the17m half-corridor plus22m horizontal/32m vertical padding per side,20m blending and importance10. Both use intensity.8, box projection and one initialization capture; the existing global probe remains the fallback. Native logs record completion of each render ID. Vehicle geometry on layer8 is excluded, though this mask does not independently remove its light contribution.

The retained Forward+ pipeline enables reflection blending, box projection and atlas sampling. Local URP source confirms this path weights probes by fragment world position, so the unchanged single road renderer can receive local reflections. The influence boxes approximate broad local surroundings; they cannot precisely reconstruct every curved barrier, lamp and distant tower. This is a documented technique change after the unsuccessful whole-circuit cubemap, not another global brightness increase.

Keep C's fog, lighting, sky, road texture/mask and effective smoothness`.315–.45`, geometry, physics, camera and effects fixed. This roughness filters roughly mip4.3–5.0, so broad surroundings are a realistic target and crisp lamp-strip reflections are not promised. If the spatial environment is useful, evaluate the plan's single roughness adjustment separately. Otherwise reassess the road technique before further tuning. Native five-view/control comparison and isolated performance remain required.

### D result and owner wrap-up

D GUID `5edbc3cfa5be45358031fe8050f71d28` completes 1,440 validated PNGs with 197 source and 189 app hashes unchanged. All three one-shot captures report `VECTOR_PROBE_READY`; none times out. Natural selections are 149/211/283/398/473/607/642/1005; every exact-pose check fails. Parent inspected all five primary originals: road midtones and source-related reflection shape remain insufficient, so D is not selected. No independent D visual verdict, motion observation or isolated performance result is claimed.

The owner requested “wrap up and push” before the planned roughness experiment. Stop new visual experiments. Preserve D source/evidence in a separate corrective history entry, then restore A's clearer reflection-off road and C's accepted fog correction as the resumable checkpoint. Remove the two local probes and their pipeline toggles. Leave the unused serialized reflection material and all evidence for future work. Stage 1 is partially delivered, with the production road-response criterion still open. No SSR experiment or later stage starts.
