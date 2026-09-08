# Road dampness seam diagnosis

The native amber-mid image in `evidence/night-v3-motion-02/selected/amber-mid.png` shows hard rectangular and diagonal boundaries within the right-side road sheen. The strongest source-backed candidate for the rectangular boundaries is the repeated smoothness mask. The exact contribution to every diagonal facet remains unproven.

`WorldBuilder` previously generated nonperiodic Perlin patch/streak noise for smoothness alpha, then repeated that texture. Road UVs are `(across, row * 0.25)` and the shared material UV scale is `(3, 1)`, so the field wraps three times across the road and every four longitudinal mesh rows. The installed URP `LitForwardPass.hlsl` transforms the shared UV using `_BaseMap`; `LitInput.hlsl` uses that UV for the metallic/smoothness sample. Nonmatching tile edges can therefore create abrupt roughness boundaries under direct specular lighting. The runtime smoothness multiplier is 0.94.

The road already disables environment reflections. The older `ao-diagnostic-08` reflection-disabled image retains residual cells; that earlier comparison did not establish a unique cause. Current road geometry shares interior vertices, recalculates normals and tangents, and has 12 lateral columns. Inspection found no obvious interior hard-normal or UV split. UV derivatives and specular aliasing remain possible contributors to diagonal artifacts.

The bounded correction blends four translated Perlin samples with smooth endpoint weights for the smoothness field only. Opposite tile edges have matching noise values and slopes. Patch/streak frequencies, their 65/35 mix, the existing smoothness remap and multiplier, texture dimensions/filtering, albedo, normal map, geometry, and lighting remain unchanged. This retains useful damp direct highlights.

Validation: source review and whitespace checks only. Native comparison is pending alongside the next integration. `night-v3-motion-03` records the preceding road implementation and is not evidence for this correction. Compare the same amber segment before judging improvement. If diagonal artifacts persist, isolate constant smoothness and disabled normal mapping in separate matched diagnostics before changing geometry or suppressing specular lighting.


## First native comparison

The periodic mask compiled and ran in `night-v4-race-01` and `night-v4-motion-01`. The selected amber interior is matched by track progress to motion03 (0.885627 versus0.885914). Repeated diagonal highlights remain plainly visible on the right side. This falsifies any claim that periodic smoothness alone resolves every sheen boundary; it fixes the source texture seam but does not close the visible road issue. The source diagnosis retains normal/interpolation/specular-aliasing uncertainty.


## Final bounded material finish

The 0.42–0.78 range in `night-v4-motion-02` modestly reduces bright diagonal-band contrast while retaining damp-road and warm/cool pool readability. Parent and Astra keep this version for the playable milestone. The actual boundaries remain, so this is a contrast improvement only; no further road or scene rebuild is part of this iteration. The matched interior is at 35.380 seconds in both comparison captures.
