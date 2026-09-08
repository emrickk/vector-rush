# Independent runtime critique 03

Directly inspected all four native 1920 × 1080 captures in `evidence/run-03`, race verification, final telemetry and performance sample. Compared against run-02 evidence and the previously inspected concept. Reviewed rendering source and the installed URP source to investigate the road defect. No game or Editor was launched by this reviewer.

**Verdict: prototype presentation and automated race completion remain acceptable within the tested scope. AAA visual quality still fails.** Two bounded polish changes visibly work; the road correction is not yet successful.

## Observed changes

The grandstand now has visible columns, roof bays and crossmembers. This gives the formerly floating blank slab a readable structural rhythm in title/start/results views. The continuous terraces remain simplistic, but the strongest roof placeholder cue is reduced.

The right-side rival now has a broad gold/olive tint against the player's pale hull. That is more useful for identification than tiny accent changes. The title, numeric HUD and results typography remain readable. All craft still visibly carry 07. Engine cores and trails still look subdued and graphic rather than convincingly luminous.

The running surface still shows broad alternating dark/light bands and dense jagged streaks around the distant bend and crest. The close ground in `03-race.png` is particularly dark, and the improvement in canopy and paint cannot hide this unresolved defect. No claim that the shadow correction succeeded is supported by these images. Temporal shimmer remains unverified from stills.

## Targeted road diagnosis

The prior change assigned `Light.shadowBias=.8f` and `shadowNormalBias=.65f`, and disabled expansion-joint shadow casting. Source inspection found that the per-light bias assignments were **not actually selected by the URP rendering path**:

- `WorldBuilder.BuildLighting` created a plain Light without `UniversalAdditionalLightData`.
- The installed `UniversalRenderPipeline.cs` at lines 2146–2149 selects the Light's bias only when additional light data exists and `usePipelineSettings` is false. Otherwise it uses the pipeline asset.
- `VectorPipeline.asset` had depth bias 1 and normal bias 1 at review time. `UniversalAdditionalLightData` defaults `usePipelineSettings` to true.

This is a concrete reason not to interpret run-03 as a test of the intended .8/.65 bias values. It does not by itself prove that bias is the cause of every streak.

**Controlled test performed after run 03:** directly inspected `evidence/final-caster-check/01-title.png` and `02-start.png`. The source at that point disabled shadow casting only on the running-surface ribbon, preserving its shadow receiving and the underbody/vehicle/structure casters. The distant road bands and zigzags remained visibly intact in both captures. This is a negative diagnostic: top-surface self-shadow casting is not supported as the sole cause, and the change does not qualify as a verified fix. Restoring the original casting behavior is appropriate rather than retaining an ineffective workaround. This short check is not a second full-race test.

To test per-light bias deliberately in future, add the URP additional data and disable pipeline bias selection, or change the effective pipeline values. The failed caster test means that further blind bias changes are not a high-confidence solution.

The alternate geometry hypothesis remains plausible: `TrackPath.Evaluate` uses a forward vector that is piecewise constant per source sample and calculates banking from a moving interpolated center; the wide ribbon then recalculates shared normals. That can introduce orientation/normal variation, but this review has not demonstrated its visible contribution. The noise texture's limited value range alone is insufficient to explain the very dark band contrast. Diagnose before adding surface detail.

## Race and frame evidence

The automated hover-physics race again finished three laps with **zero player recoveries**, at 112.58 seconds in the text report and 01:52.576 in the result screenshot. Best lap is visibly 00:37.181. Three restart checks report that the countdown freezes while paused and completed laps reset to zero. One rival recovered once; four rivals did not. Rivals were stopped by the player's finish at approximately 2.59–2.76 laps, so balanced competition and every rival finishing are not demonstrated.

On Apple M2 Max at 1920 × 1080 with VSync enabled, the 6,760-interval sample reports mean 8.88 ms, median 8.33 ms, p95 16.62 ms, p99 16.72 ms and 182.2 MiB allocated Unity memory. The changed cadence from run-02 makes this inappropriate evidence of a rendering speedup by itself. It supports good sampled frame delivery on this machine, with the same limits: no isolated GPU timing, no minimum-spec certification and no proof about unsampled conditions.

## Remaining priorities

1. Resolve the road rendering defect through a further isolated rendering/geometry diagnosis; the caster-only test above failed, so retain explicit failure status until a native capture confirms an actual correction.
2. Improve the engine focal point and the hull/environment material response. Reflections, controlled highlights and convincing propulsion still fall well short of the art target.
3. Validate manual keyboard/controller handling and pack interaction. The automated completion is real evidence of the race loop, but does not establish human control comfort, fair difficulty or camera behavior during overtakes.

The correct current label is an original racing prototype with readable UI, improved structure/identity and a repeatable automated full-race pass. It is not an AAA visual delivery, and it is not yet a manually validated finished game.
