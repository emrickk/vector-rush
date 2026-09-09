# Vector Rush handoff — 2026-09-08

## Latest requested direction review

The owner found SSR's visible difference negligible and requested parallel critics and an improvement approach. [Three independent reports and synthesis](visual-target-reviews/002-improvement-approach.md) recommend parking SSR and making one opening passage visibly more complete through connected existing/new construction, local lighting and material separation. The narrow lamp-aim proposal is supporting work, not the next headline result. A small gallery study can calibrate surface response. No new runtime or asset implementation is included in this review.

## Latest execution — SSR prototype rejected for insufficient visible gain

The owner authorized the Nocturne V2 implementation plan. SSR01 failed on reflection cameras; SSR02 fixes that scope and supplies actual instrumented CPU pass samples, but independent inspection of all nine off/on pairs finds no convincing useful reflection improvement. [Original native comparison](ssr02-native-comparison.html) · [verdict and limits](visual-target-reviews/001-reflection-feasibility.md) · [next technique proposal](visual-target-reviews/001h-next-technique-proposal.md).

SSR02 source/evidence is committed at `dd7a417`; its separately named app has GUID `f1a459e36c064afc930b841eede3d488`. Both 1,440-frame captures exit 0 and pass integrity/source/app checks. GPU buffer contents, continuous motion, manual/audio and realtime performance are unverified for this preview. The source checkout retains experimental SSR infrastructure and Instrumented target settings with runtime SSR defaulting off; it is not the exact historical control source. Do not treat a normal rebuild as reproducing the retained control binary.

The original `Builds/Vector Rush.app`, GUID `40bed5f53c2449418e7fb56bf59739f6`, remains unchanged across all 189 hashed files. Earlier control tests/performance below belong only to that control. The SSR experiment has a rejection result; the production rendering choice and Tasks 2–7 remain unresolved. The sections below are historical records, not new acceptance claims.

## Latest reference refresh — appearance targets and gap review

The owner requested consistent reference images and evaluation after reviewing this handoff. Four new generated [Nocturne V2 targets](../references/nocturne-v2/README.md) now share the current craft and one appearance reference across the opening bend, amber gallery, thermal passage, and a neutral craft study. The [interactive comparison](visual-targets-v2.html) places original native evidence beside each target. Exact prompts, input camera/file provenance, image integrity checks, consistency limitations, and prioritized native acceptance criteria are preserved with the set.

The main gaps are road/light interaction, connected lower/middle city construction, and architectural/material finish. The craft's primary identity should remain. Generated road sheen is an upper limit; small generated geometry/projection changes are not blueprint requirements. The studio comparison uses the historical contact-repair control with older maps and is labeled accordingly. This is a parent-assistant art-direction review, not an independent production pass. Runtime and the local app are unchanged. The proposed SSR test below remains a separate implementation step and would address only part of the visual gap.

## Current result — resumed road pass completed

The owner resumed after the historical pause below. One higher-smoothness candidate was rejected for insufficient useful whole-frame gain by the parent and independent [review004](road-reviews/004-resumed-smoothness-native.md). Candidate source is preserved at `a5debfb`; the exact control runtime, scene and app are restored. The app remains GUID `40bed5f53c2449418e7fb56bf59739f6`. [Decision](../evidence/night-production/road-response/decision.md) · [restoration identity](../evidence/night-production/road-response/restoration-validation.json).

Fresh validation now supersedes the older checkpoint's missing technical checks: **42/42 tests pass**, the actual automated race finishes three laps in **128.32 s** with all six racers at zero recoveries in the final telemetry, both restart launches pass and all three countdown-pause checks pass. The native process exits zero. Six 1920×1080 PNGs pass CRC/decompression; the parent also inspects the actual results image. The 197 source/resource/settings files, 189 app files and scene hash still match the exact restored control. [Final verification](../evidence/night-production/road-response/final/verification.json).

The separate real-time 60-second sample on Apple M2 Max at 1920×1080/VSync 1 records 6,967 intervals: **mean 8.61 ms, P95 9.25 ms, P99 16.74 ms**, allocated memory 243.3 MiB. No build, bake or encoder overlaps it. The higher P99 than the older baseline (9.31 ms) is preserved, not explained away or assigned to the rejected candidate. These are observed frame intervals, not isolated GPU timings or a new production-performance acceptance.

The fresh control and candidate each have a complete 1,440-frame 1080p circuit, eight inspected original selections and a fully decoded 15-second silent simulation-time clip. Exact-pose tolerances fail and remain explicit. Continuous watched-motion, manual input/audio, alternate-aspect and broader production acceptance remain open. This bounded experiment is complete; **Stage 1 remains incomplete**.

Next proposed work is one isolated native preview-SSR prototype on the installed URP 17.6 package, starting with compilation, actual feature/resource availability and pass verification. [Feasibility005](road-reviews/005-reflection-feasibility.md) documents the guarded source, official opt-in route, limitations and gates. No SSR implementation, package upgrade or later city/craft/racing stage was started. Do not repeat arbitrary smoothness or probe-strength variants.

The sections below preserve the earlier checkpoint history; their past-tense validation gaps and pause refer to that earlier milestone.

## Historical checkpoint and subsequent resumption

**This records the earlier owner-requested pause at `3097bc8`. The owner subsequently resumed work and authorized the [next road pass](next-road-pass.md); that current instruction supersedes the hold below.** See the [complete work record](development-work-record.html) for the project history.

Historical hold: The owner asked to work on Stage 1, then said “wrap up and push”; the coordinating task also requested a durable handoff and cessation of further game work. Finish this checkpoint, push, and stop. No new experiment, later stage, merge or release is authorized by the wrap-up alone.

Repository: `/Users/anping/Documents/Stuff/AI Space/Vector Rush`  
Branch: `build/first-playable`  
Remote: `https://github.com/emrickk/vector-rush.git`

The saved game checkpoint is commit `3097bc8`. Later documentation commits do not change that native app identity. Published history is preserved without squashing. `AGENTS.md` requires current plans, separate corrective/review commits, and pushing completed milestones.

## Resumable source and local app

The source now combines **candidate A’s broader direct lighting and clearer reflection-off road** with **candidate C’s independently accepted atmosphere correction**. The final local app is `Builds/Vector Rush.app`, GUID **`40bed5f53c2449418e7fb56bf59739f6`**.

- Ambient SH RGB `.14/.16/.18`; moon RGB `.78/.85/.92`, intensity `.78`, shadow strength `.4`.
- Exp2 fog density `.0018`; intended linear RGB `.041/.074/.086` is assigned through `.gamma` in runtime and Editor scene setup. Native readback confirms the intended linear shader values. Sky horizon `.047/.081/.096`, zenith `.0015/.0035/.008`.
- Cool exterior pool RGB `.68/.86/.91`, intensity `460`; warm pools and gallery/landmark lights remain fixed.
- Running deck uses `RoadSurface`, with environment cubemap sampling disabled and direct specular retained. Original maps, normals, geometry and nominal effective-smoothness endpoints `.315–.45` remain unchanged; these endpoints are not a measured GPU texture range. The unused serialized `RoadSurfaceReflections` variant remains for reproducible comparisons.
- D’s two local probes and pipeline blending/box-projection toggles are removed. The original single 256 px one-shot global probe remains; pipeline blending and box projection are off.
- Route, race/AI, camera, craft geometry, craft texture payloads and postprocessing are unchanged by this stage. The three proven road-fixture caster-off groups remain off.

`evidence/night-production/lighting-road/checkpoint/build-identity.json` identifies 197 source/resource/settings files and 189 app files. The final checkpoint has a successful native build, two short native fog/title-to-race runs exiting zero, five validated PNGs and unchanged source/app hashes; see its [validation report](../evidence/night-production/lighting-road/checkpoint/checkpoint-validation.json). It has **not** received a new full-circuit comparison, 42-test run, alternate-aspect review, real-time three-lap/performance sample, watched-motion or manual-with-audio acceptance.

## Preserved evidence and verdicts

| Record | Result and identity |
| --- | --- |
| Baseline | `evidence/night-production/baseline/`: five natural opening views, three controls, a fully decoded 15-second silent simulation-time clip, source identity and fresh real-time sample. Native GUID `5c49b92643bd4d41820ba81857fdf8c7`. |
| A | GUID `9a03e9b1767f4b89abd9c3bf938f7898`; modest broad-light/readability gain. Best road control of the inspected candidates. |
| B | GUID `de942ca116044e6eb2141c36b547d8a3`; same A lights, reflection-enabled road. Independently rejected: darker deck without useful broad reflection shape. |
| Fog diagnosis | GUID `1f13c130ce6f4d9895edf31998701007`; confirms intended fog `.041/.074/.086` was uploaded as `.003174/.006451/.007986` in Linear color space. |
| C | GUID `1287761a66a64ff9bb7a952eb5519b0a`; fixes only the fog assignment, retaining B’s road. Independent review accepts its clear city-depth gain and preserved thermal/workshop contrast. Road remains insufficient. |
| D | GUID `5edbc3cfa5be45358031fe8050f71d28`; two local 256 px box-projected probes, retained Forward+ blending/atlas, C atmosphere and unchanged roughness. All three probes finish; parent’s five primary views still do not establish the required road improvement. Unaccepted, with no independent D visual or motion/performance verdict. |

A/B/C/D each retain a complete 1,440-frame native report, validated PNG integrity, eight selected originals and source/app identities under `evidence/night-production/lighting-road/`. Raw sequences stay local under ignored `frames/`; compact selections and metadata are committed. **All exact-pose comparisons fail their tolerance**; these are nearby natural route crossings, not pixel-matched measurements. Do not relabel automated simulation-time captures as real-time performance or human play.

Key review: [021 — A/B/C native verdicts](environment-reviews/021-stage1-lighting-road-native.md). See also [020 — baseline](environment-reviews/020-production-baseline.md) and [lighting decisions](../evidence/night-production/lighting-road/decision.md).

Fresh baseline performance: 7,189 intervals at 1920×1080 on Apple M2 Max, VSync 1; mean 8.35 ms, P95 9.18 ms, P99 9.31 ms, allocated memory 241.9 MiB. Its separate race finishes three laps in 128.32 s, with both restart launches and three countdown-pause checks passing. These numbers belong to the baseline, **not this final checkpoint**.

## Completion boundary and next work, only after resumption

Task 1 of the [production plan](superpowers/plans/2026-09-08-night-production-finish.md) is complete. Task 2 / Stage 1 is **partially delivered**: atmosphere has an accepted gain; the production road-response target remains open. Tasks 3–8 have not started. Full-production acceptance remains pending across the required six visual/experience dimensions plus watched/manual and technical gates.

The planned single roughness adjustment was **not started** before the owner stopped work. If resumed, reassess the road technique using preserved A/C/D evidence before another bounded experiment. A small probe cannot precisely project every curved barrier, near lamp and distant tower, and current roughness strongly blurs thin lamps. Preserve the readable road and accepted atmosphere as the new control. Do not merely raise every light or add emissive strips.

A stopped read-only SSR contingency found installed source guarded by `URP_SCREEN_SPACE_REFLECTION`, with no enabling define in the inspected project settings/runtime assembly. Source presence does not establish native availability. No SSR implementation, new dependency, planar reflection or roughness experiment was made. Any future expensive technique requires the documented feasibility comparison and isolated performance check; no build/bake/encoding may overlap that sample.

## Milestones and delivery distinction

- `2b8e751`: verified production baseline.
- `af71d07`: candidate A.
- `e57df91`: candidate B.
- `b5fcb37`: verified fog correction C.
- `9d2badd`: independent A/B/C review.
- `69ba4af`: D source/evidence preserved separately.
- Final containing commit: restore A road + C atmosphere, validate local checkpoint, update plans/README/handoff, and pause.

The previous fully checked local delivery remains `Builds/VectorRush-macOS-lighting-depth-2026-09-08.zip`, with its original videos and `evidence/lighting-depth-final/` manifest. It is different from the newer checkpoint app. **No new ZIP, GitHub release or merge is part of this wrap-up.** No new preview claims the old delivery’s footage represents the checkpoint.

## Processes and operational notes

No game work should remain running after the final push. The integrator and critic own no continuing Unity, encoder, render or agent job. An unrelated Blender GUI was left untouched. Earlier automatic approval review rejected a performance launch because a stale Unity job might overlap; read-only provenance established it was our licensing-failed batch job, it was stopped, and the subsequent baseline run was approved. That issue was resolved; it is not a pending approval or reason to kill unrelated apps.

Native runs require the existing approved unrestricted game launch; sandboxed launches can abort before useful output. The project wrapper is `./tools/unity.sh`; `BuildMac` already calls `Prepare`, so do not duplicate preparation by habit. Editor licensing/build logs remain ignored. Preserve existing archives, original assets, Unity `.meta` files and all failed experiments.

## Nocturne V2 implementation plan

The reference-aligned [implementation plan](superpowers/plans/2026-09-08-nocturne-v2-execution.md) is prepared: isolated reflection feasibility, road/light finish, connected city, architectural finish, craft materials, integrated play validation, then circuit rollout/delivery. It preserves the restored control and starts with a separately named preview app. Runtime implementation has not started.
