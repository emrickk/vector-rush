# Independent critique 01 — source and authored asset

Review date: 2026-09-07. Reviewed independently, before runtime evidence was available. Source files are actively being integrated; line numbers below refer to the version read for this review. This is not a gameplay pass, performance pass, or AAA quality claim.

## Evidence and limits

- Read `docs/design.md`, all runtime C# scripts, editor setup, race-rule tests, ocean shader and asset generation source.
- Visually inspected `evidence/asset-renders/hero-front-three-quarter.png` at 1600 × 1200.
- Inspected `references/solstice-chase-concept.png` only as an art-direction reference. It is generated concept art and supplies no evidence of working rendering or gameplay.
- Rear, top, and runtime captures were not yet available when this initial review was written. Their absence is not a visual defect; it limits what can be concluded.

## Observed asset defects

1. **Identification disappears in the bright hull.** The thin yellow nose and stabilizer stripes are pale against the ivory panels in the rendered lighting. The tiny gray KESTREL labels are also low contrast. At chase-camera distance these small identifiers are unlikely to carry the craft's identity. This last distance judgment is an inference to verify in runtime.
2. **Large surfaces have little material information.** The front and mid-pontoon armor read as broad, uniformly smooth slabs. There are panel separations and vents, but the render lacks convincing painted-surface roughness variation, inset panel details, or wear concentrated around working parts. The model reads as a clean manufactured concept model, not a finished high-budget hero asset.
3. **Canopy is opaque and strongly faceted.** The dark canopy's planar highlight changes are readily visible in the front render; it reads more like a solid polygon shell than glazed cockpit construction. Faceting can be a deliberate style, but this diverges from the physically detailed direction in the design brief.

The twin-pontoon outline and separated center fuselage are clearly legible. This is a useful foundation, not evidence that the full visual target has been met.

## Three highest-impact visual/integration fixes

1. Correct the barrier geometry and bring its collision height into the craft's flight envelope. The source currently undermines both readable containment and physical contact; see P1 findings below. Verify with a screenshot looking along the inner wall and a recorded glancing collision on each side.
2. Preserve authored engine material contrast in Unity, then judge the actual rear chase view. Add an explicit Ceramic mapping, retain separate white markings, and verify FBX axes, scale, collider fit and trail origins. These are prerequisites to judging the hero craft in gameplay.
3. Strengthen a few medium-scale livery marks and material contrasts that survive the chase camera. Use larger, darker identification, restrained roughness variation and clear separation between hull paint, exposed metal and canopy. Re-render the asset and recapture the identical runtime camera before adding more small parts.

## Concrete source defects

### P1 — Level craft clears the safety barrier

`Gameplay/HoverVehicle.cs:12` sets hover height to 2.5 m. The hover force equilibrium in lines 122–135 agrees with that height. `VectorBootstrap.cs:36` creates a collider 1.25 m tall, centered 0.12 m above the craft origin, so its bottom is **2.5 + 0.12 − 0.625 = 1.995 m** above the track. `World/WorldBuilder.cs:80` puts the barrier top at **1.2 m**. The level collision box therefore clears it by **0.795 m**. A nominal sideways departure cannot meet the wall until the craft has already dropped. Correct the relative geometry, then verify both sides with real physics. This is a numerical source defect; the actual resulting flight behavior has not been observed.

### P1 — Safety barrier shell winding is reversed

`World/WorldBuilder.cs:80–83`: on the right barrier, the inner-face triangle `(a,b,c)` goes lower-inner → upper-inner → next-lower-inner. Its cross product is `Up × Forward = +Right`, pointing into the barrier instead of toward the racing surface. The mirrored left barrier has the equivalent error. The top points down and the outer wall points inward as well. Reverse the wall triangles. Standard backface culling makes the near inner faces disappear; one-sided mesh contact is also at risk. Inspect both inner and outer sides after the change.

### P1 — Gate validation accepts projected off-course crossings

`Gameplay/RaceDirector.cs:107` supplies only `ClosestProgress(position)` to the tracker. `Gameplay/RaceProgress.cs:41–67` receives no lateral distance, deck height, track validity or crossing plane information. A vehicle moving across a checkpoint's progress value while outside the drivable ribbon is indistinguishable from a valid pass. `Gameplay/HoverVehicle.cs:158–160` deliberately allows 2.7 seconds airborne before recovery and a distance of up to 48.4 m from the centerline, so recovery alone does not guarantee valid gates. Add a gate corridor or valid-track condition before advancing progress, with explicit tolerance for normal hover and jumps. Verify an off-track crossing is rejected while a legitimate crossing at the track edge is accepted. The missing validation is certain; a useful player exploit on this exact course has not been demonstrated.

### P2 — Unity remapping changes dark turbine casings to ivory

`SourceAssets/build_assets.py` defines Ceramic as dark gray `(0.055, 0.08, 0.088)` and assigns it to both turbine casings. `VectorBootstrap.cs:43–49` has no Ceramic case, so it falls through to `world.Ivory`. The authored dark propulsion contrast cannot survive this mapping. WhiteMark also falls through to hull ivory. Add deliberate mappings instead of a catch-all for known materials. Runtime screenshots are needed to evaluate the final appearance.

### P2 — Recovery advances the player before the first start crossing

`Gameplay/RaceProgress.cs:13` always respawns an unstarted racer at 0.995. `Gameplay/HoverVehicle.cs:61` starts the player 14 m behind the line, and lines 187–194 accept manual recovery. Re-evaluating the identical spline math gives track length **1844.517 m**, so 0.995 is only **9.223 m** behind the line: pressing recover immediately after launch moves the player forward **4.777 m**. It costs energy, but contradicts the explicit no-respawn-shortcut rule. Preserve the racer's initial progress until it crosses the start; test recovery immediately after release.

### P2 — Finish order is racer-list order inside one physics step

`Gameplay/RaceDirector.cs:104–122` iterates the player first and appends finishers immediately. If two racers cross during the same 10 ms physics interval, the player wins that ordering even if the other racer crossed earlier within the step. Interpolate a crossing fraction from previous and current progress and sort same-step finishers by that fraction. This is a bounded fairness issue, not a claim that every close finish is incorrect.

## Additional integration risks to inspect

- The initial asset statistics report 5.4375 m width and 7.487 m length, versus a 4.5 × 6.7 m collision footprint (`VectorBootstrap.cs:36`). Art outside a forgiving collider is often intentional, but the ~0.47 m side difference can visibly intersect a rival or barrier. Check actual imported renderer bounds; source export is still changing.
- `WorldBuilder.cs:106–109` draws luminous “Boost chevron” strips without any corresponding pickup/trigger behavior in the vehicle code. They are currently decorative speed-pad signals. Either implement a response or change their visual language so they do not promise acceleration.
- `VectorBootstrap.cs:64–66` fixes wake origins and gives TrailRenderer an opaque Lit engine material. Verify origin placement, opacity and reset behavior. Alpha values in trail colors do not by themselves configure transparent blending.
- The landscape source uses stretched spheres for islands and plain boxes for most buildings (`WorldBuilder.cs:132–144`). This establishes a rough scene but cannot substantiate the rocky coastal environmental fidelity of the direction reference. Runtime appearance remains unreviewed.
- `RaceEvidence.cs` captures a title and start, then samples frame intervals for 60 seconds. It does not drive the player or prove a lap, completed race, restart, collision, or recovery. Its own report correctly disclaims complete-race proof. Add actual play evidence separately.

## Next review gate

Review the actual macOS build at 1920 × 1080: title, clean chase on a straight, tight bend, left/right wall contact, recovery, pause, complete three-lap results, and repeated restart. Preserve exact captures and logs. Judge runtime visuals separately from Blender asset improvements, and report unresolved defects instead of marking the slice passed from still images alone.
