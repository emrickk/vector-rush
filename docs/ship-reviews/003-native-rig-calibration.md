# Independent review 003 — native inspection rig suitable

Reviewer: Astra. Date: 2026-09-07 local. **Verdict: the repaired rig is suitable for native modeling, import and material diagnosis. This accepts the inspection setup only. V2 remains visually rejected; no V3 or AAA acceptance is implied.**

Directly inspected all eight native 1920×1080 images in [ship-native-v2-neutral-02](../../evidence/ship-native-v2-neutral-02/inspection-scope.json), including the [rear quarter](../../evidence/ship-native-v2-neutral-02/01-studio-rear-three-quarter.png), [orthographic top](../../evidence/ship-native-v2-neutral-02/04-studio-top-ORTHOGRAPHIC.png), [engine emission OFF](../../evidence/ship-native-v2-neutral-02/05-studio-engine-emission-OFF.png), [engine emission ON](../../evidence/ship-native-v2-neutral-02/06-studio-engine-emission-ON.png), and [canopy close-up](../../evidence/ship-native-v2-neutral-02/07-studio-canopy-close.png). Read the scope metadata and completion status. This review inspects the recorded output; it did not rerun the native capture process.

## Calibration evidence

- Rear, front, side and top show the intended whole ship at the expected orientation. The floor and shadows are beneath it, and the lighting/reflection environment visibly reaches the model. The earlier misplaced world-pose failure is absent in this evidence.
- The metadata's runtime and canonical bounds have identical sizes: 5.265108 × 1.997324 × 7.205001 m in Unity axes. Their centers agree to floating-point precision; the Z difference is approximately 0.00000024 m. Their nonzero center offsets describe the model's local bounds, not a displaced inspection origin.
- The inspected main aperture is recorded at approximately (1.68, -0.035, -3.405), consistent with the V2 anchor contract. The metadata records eight calibration checks and a completed reflection probe.
- Emission OFF and ON have identical camera position, rotation, lens and framing. The images visibly differ in the intended rings/core while their surrounding geometry remains aligned. The OFF image reveals physical collars and recesses without luminous fill.
- The canopy close-up contains broad reflected cards and smaller highlights that reveal its surface response. Armor, dark structure, metal and glazing are distinguishable. The rig can expose defects rather than relying on bloom to conceal them.

## What this setup does and does not establish

The seven studio images use the runtime mesh/material mapping with fixed neutral lights, ACES and exposure zero. Fog, bloom, vignette and propulsion effects are disabled; only the clearly labeled emission-OFF view temporarily removes material emission. The [gameplay frame](../../evidence/ship-native-v2-neutral-02/00-native-gameplay-chase.png) is separately labeled and shows normal chase/HUD context with automated input through normal physics.

This is a useful diagnostic environment, not a physically matched recreation of the Blender studio or reference C. The gray floor, strong directional shadow and reflection cards are acceptable inspection aids. Do not infer numerical material equivalence across renderers from different lighting or treat a favorable rig render as a gameplay pass. These are still images; temporal quality remains untested here.

The rig plainly retains V2's inflated armor transitions, bubble canopy, slab fins and simplified attachments. It also reveals a concrete native finish defect: the race-number fields show almost absent numeral fills and thin traces, especially in top and canopy views. That appearance is observed; the cause has not been isolated. It must not be labeled a lighting-calibration failure to excuse the asset.

For V3 comparison, keep this setup stable, retain the per-view camera metadata, and record any bounds-driven reframing. Attach the exact export/source identity to the capture record so `Art/HeroShip` cannot ambiguously refer to different revisions. Revalidate pose and nozzle targeting after importing the new geometry. The first failed folder, `ship-native-v2-neutral`, remains excluded and should retain its calibration-failure label.
