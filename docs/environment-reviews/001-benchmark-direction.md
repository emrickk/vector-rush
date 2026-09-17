# Environment review 001: benchmark direction

Reviewer: Astra. Date: 2026-09-08. **Nominate the five views below and lock frames 0000 through 0263 as the continuous baseline.** This is an 11-second passage covering the open approach, gallery compression and skyline release. It supports the user's visual-environment priority in [the plan](../visual-environment-plan.md). This review authorizes a composition direction, not acceptance of new art.

## Baseline selection

All five views were directly inspected at native 1920 × 1080, alongside neighboring samples. Values below come from [the original capture metadata](../../evidence/hud-motion-01/replay-metadata.json), not the smoothed HUD speed display. All five samples are grounded.

| View | Source frame | Race time, seconds | Track progress | Raw speed, km/h | Boost / energy |
| --- | --- | ---: | ---: | ---: | --- |
| Open approach and final-sector setup | [0000](../../evidence/hud-motion-01/frames/frame-0000.png) | 27.020523 | 0.71626353 | 117.73 | Off / 100% |
| Gallery entry | [0170](../../evidence/hud-motion-01/frames/frame-0170.png) | 34.100285 | 0.84775656 | 202.13 | Off / 100% |
| Gallery middle | [0190](../../evidence/hud-motion-01/frames/frame-0190.png) | 34.940144 | 0.87269884 | 191.01 | Off / 100% |
| Exit threshold | [0210](../../evidence/hud-motion-01/frames/frame-0210.png) | 35.770004 | 0.89735913 | 210.40 | Off / 100% |
| Open skyline reveal | [0240](../../evidence/hud-motion-01/frames/frame-0240.png) | 37.019794 | 0.94585216 | 327.89 | On / 86.20% |

Frames 0000–0263 inclusive contain 264 images, which encode to 11.00 seconds at 24 fps. Their recorded race times span 27.020523–37.979633; the last sample is at progress 0.99356407. The extra frames after the skyline still retain the release into open space before the lap transition. The first view intentionally includes the difficult open setup rather than starting immediately beside the attractive warm portal.

This is one existing continuous run with automated steering through ordinary physics. It includes ordinary travel through the gallery and an observed boost afterward. It is **not** a matched ordinary-versus-boost traversal of the entire passage, a real-time performance sample, or a completed continuous-motion review.

## Composition instructions for the five views

**0000: establish the circuit's weight above the district.** The broad bend is the foreground subject. Give its outside edge a readable deck thickness, barrier base and support relationship to the lower service district. Compose one substantial stepped terrace mass to the left, low transit/service construction below the track, then quieter distant towers. Preserve skyline gaps instead of filling every opening. The near edge must lead the eye through the bend; a newly detailed building must not become a false driving destination. Keep the existing camera bank and road clearance.

**0170: make entry into a constructed place unmistakable.** The portal is the focal point, with its opening already showing the continuing left curve. Use a stronger service-building mass on one side and a lighter, more open structural treatment on the other. Give the header depth, supported ribs and visible connections to the deck. Warm light should reveal those surfaces. A complete glowing rectangular outline with black structure behind it would repeat the current weakness. Reduce the contrast of the adjacent right-side tower windows so the portal wins through form and light, not more brightness everywhere.

**0190: prove enclosure without losing the next bend.** This view exposes the black ceiling, repeated wall spans and diagonal sheen, so it must remain in the benchmark. Make the ceiling readable as primary ribs, recessed cassettes and a quiet continuous backing plane. Show restrained light on their underside and joints. Keep the large left wall relatively quiet; let selected right-side recesses and service bays establish depth. Preserve the opening toward the next curve. Road joints and navigation markings should support that route, with limited contrast beneath the craft. Keep this same view for the new material check; the existing [gallery frame 0200](../../evidence/hud-motion-01/selected-0200.png) can remain a supplemental road diagnostic, not a replacement for the nominated middle view.

**0210: frame the release into the city.** Retain a close exit edge as a foreground silhouette, but reveal more sky and a cooler district beyond. Place the signature transit structure off the road, toward the right third, with a visible supporting podium and an asymmetric profile. Its extension may frame the skyline without becoming another low roof across the racing line. The left edge and road perspective should continue to lead forward. The exit must feel different from one more repeated interior bay.

**0240: deliver the landmark and layered skyline at speed.** The transit structure introduced at the threshold must resolve into the same recognizable object here. Use low stepped masses to balance it on the left, a readable middle district around its base, and a quieter far skyline with deliberate height groups. Leave a clear sky gap around its main silhouette. Keep the central roadway open. Near barriers and supported fixtures can supply parallax; equal-brightness towers and evenly spaced luminous bars must not flatten the view into another patterned corridor.

These are instructions for one connected three-dimensional passage. Five individually attractive compositions that require inconsistent landmark placement fail the direction.

## Lock and guardrails before art production

- **Preserve the actual reference pixels.** Copy the five nominated PNGs unchanged into the parent's retained benchmark set, with source frame indexes, file hashes and metadata. Retain the complete source recording separately. The selection must continue to expose the current ceiling, repetition and sheen defects.
- **Close the pose-recording gap.** The existing metadata does not contain vehicle/camera world position, rotation, velocity or camera FOV. The PNGs are visual pose references; exact numeric poses cannot be reconstructed from track progress alone. Before changing art, record an unchanged baseline with those transforms, camera settings, input/boost state and frame timing. Preserve the source/build, lighting, exposure and render-setting identity. Compare against that recorded baseline; a merely similar progress value is not an exact camera match.
- **Separate construction from brightness.** Each architectural family needs a recognizable silhouette and support logic. Depth must survive signs being hidden. Do not cover every plane with windows, trim or vents, and do not replace the black ceiling with a uniformly glowing tunnel.
- **Keep the road diagnosis isolated.** Preserve the [failed periodic-smoothness hypothesis](../road-dampness-diagnosis.md). Restore each diagnostic before testing the next variable. A quieter highlight is mitigation unless the visible cause is demonstrated; a deliberately stable satin redesign is a valid alternative when labeled honestly.
- **Protect the moving view.** Keep centerline, banking, craft, HUD and control behavior as stable references. Check visible versus physical boundaries and camera clearance. Judge ordinary and boosted traversals, including nearby craft, before calling the passage readable at speed. The current boost frame alone does not cover a boosted gallery entry.
- **Require motion and cost evidence before expansion.** Independently watch the continuous native passage at normal playback speed; look for window shimmer, fixture strobing, road instability, light popping and loss of the exit. Compare the exact candidate's frame cost with a fresh unchanged baseline. Preserve supported aspect ratios. Do not spread the kit around the circuit until the benchmark passes.

No gameplay, AI, HUD or environment source was changed for this nomination. The next review should judge the parent's locked composition studies and then actual new native images, keeping targets distinct from implementation evidence.
