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
