# Resumed road-response experiment

## Scope and current control

The owner resumed work after the proposed next-road plan. This experiment preserves accepted A lighting plus corrected C atmosphere, the clearer reflection-off road, all geometry/normals, camera, craft, race logic and the three fixture caster-off groups. The current control is native GUID `40bed5f53c2449418e7fb56bf59739f6` with unchanged 197 source/resource/settings and 189 app hashes. Documentation commit `566cbde` precedes the capture without changing the app.

The fresh control completed all 1,440 native frames at 1920×1080. PNG CRC/decompression and identities pass. Natural primary indices are149/211/283/398/473, with thermal607/642 and station1005. Comparisons against the older production baseline fail exact-pose tolerance; no pixel-matched claim is made. The parent inspected all eight original selections: panels and edges remain readable; open-road light is soft and fairly even; near/middle/far city layers survive; thermal drums, pipe supports and station entrances retain their separation. Broad reflected-source shapes remain absent.

The control clip uses original121–480 (360frames,15seconds) at24simulationfps, including the ordinary opening slowdown. FFmpeg full decode passes. The clip is actual continuous native rendering with automated steering and no recorded audio. Its existence/decoding and sampled visual inspection do not establish continuous watched-motion acceptance. No new real-time performance or manual-play verdict belongs to this control capture.

## One candidate, before implementation

Technical review recommends remapping packed mask alpha from `.35–.50` to `.45–.60` while retaining the material multiplier `.90`: nominal effective smoothness `.315–.450` becomes `.405–.540`. This shifts the same pattern by `.09` effective smoothness without changing its range width or phase. Only the two `Mathf.Lerp` endpoints in `WorldBuilder.Build` change the rendered material; no other light, texture, normal, reflection or geometry contribution changes.

Higher smoothness narrows and strengthens the existing direct-specular lobe. It cannot create broad area sources or reflected fixture silhouettes. The experiment asks whether the existing light pools gain useful depth without strong isolated patches, diagonal facets, lost panel detail or unstable moving highlights. If not, reject it and reassess technique rather than repeating roughness variants.

Add read-only material telemetry to the opt-in `EnvironmentEvidence` harness: report the actual native material values/keywords and the uploaded mask's min/max alpha through a temporary GPU readback before starting the race. Restore the render target and release the temporary resources. This is evidence instrumentation, not another visual adjustment. Record both the nominal design range and measured quantized texture range.

An exact copy of the control app is retained locally as `Builds/Vector Rush-road-control.app` before rebuilding. Candidate source/build/evidence and the independent verdict will remain separate. The earlier A/B/C/D attempts remain under `lighting-road/`; no new probe or costly reflection technique is implemented by this experiment.

## Capture interruption and validator correction

The first candidate capture begins at1920×1080, then changes to3024×1832 at frame433. That attempt cannot supply the full controlled comparison, even if it later completes its circuit. Preserve the mixed-resolution sequence as rejected capture evidence and rerun the unchanged candidate. No material or light adjustment is made because of this interruption.

The existing PNG validator checked each image against its frame metadata but did not enforce the requested dimensions across the run. Add explicit expected-width/height checks (default1920×1080), plus an optional reference-selection path so the new material is compared against the freshly captured combined checkpoint rather than the older lighting baseline. This is a validation correction, not a change to the native game's rendering.

## Clean candidate result and decision

Unchanged candidate GUID `1441c9f070434c73a2c65dd244581929` completes the fresh1,440-frame run entirely at1920×1080. PNG integrity,197 source files,189 app files and the separately recorded scene hash pass. The uploaded512×512 base-level mask spans alpha124/255–144/255; multiplier.89999998 yields effective smoothness.437647–.508235. This is narrower than the nominal.405–.54 because the generated noise does not reach the interpolation endpoints. URPLit, mapped-alpha/normal keywords, reflection sampling off, base color, bump.25 and metallic0 are confirmed in the native report.

The new validator compares the candidate against the fresh control: primary149/211/283/398/473 and controls606/641/1004. Every exact-pose tolerance fails; this is a nearby natural-crossing comparison. The continuous silent15-second candidate clip includes the slowdown and passes full decoding. Parent inspects all eight originals. Independent [review004](../../../../docs/road-reviews/004-resumed-smoothness-native.md) separately inspects the same candidate set against its recorded control observations.

Both verdicts reject candidate01 for insufficient useful full-frame gain. Panel/edge readability, thermal/station separation and vehicle grounding survive; no catastrophic still regression is found. The opening road remains plain with restrained pools, the tight-turn fan-like boundaries persist, and broad reflected-source structure is absent. The still-image improvement gate fails, so unobserved continuous-motion behavior cannot justify retaining it. No new candidate real-time-performance or human-play verdict is claimed.

Preserve this candidate's source, readback and rejected/resumed capture evidence as a separate milestone. Restore the exact control's three runtime/scene files and its retained native app. Keep the validator's new resolution guard and explicit reference option. Stop roughness iteration; write a bounded reflection-technique feasibility comparison before any other rendering implementation.
