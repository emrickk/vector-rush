# Reflection feasibility — SSR02 rejected for insufficient demonstrated gain

SSR02 is **not accepted for production**. The camera-scoped correction removes the earlier observed reflection-camera failures and records native CPU pass work, but all nine whole-frame OFF/ON comparisons fail to demonstrate a convincing useful reflection improvement. This is not a claim that SSR is universally unsuitable or that its GPU output has been proven correct. [Independent visual review 001g](001g-ssr02-visual-review.md) · [original native comparison](../ssr02-native-comparison.html).

Source/evidence milestone `dd7a417` preserves the corrected separate app `Builds/Vector Rush-SSR-preview-02.app`, GUID `f1a459e36c064afc930b841eede3d488`. Both native runs exit 0 and each yields 1,440 intact 1920 × 1080 original PNGs with unchanged source/app hashes. SSR, upscaling and final-blit CPU counters record 1,690 positive frames ON and zero OFF. Reflection cameras are skipped; Game cameras delegate to the installed feature. The prior rendering/initialization exceptions are absent. Native shutdown warnings remain in the logs. [Validation](../../evidence/nocturne-v2/ssr-02/native-validation.json).

The scene remains broadly readable in all reviewed stills. The reviewer finds no clearly legible new reflected lamp, rail, architecture or craft feature. Local differences remain inconclusive. These are naturally steered runs, not identical frozen poses: current pair camera separation is about 0.106–0.221 m except the two thermal pairs at about 1.87 m. Legacy exact-pose tolerance flags also remain failed. Do not interpret small pixel differences as a measured SSR contribution.

## Gate result and retained state

- Feature/resource retention, supported Instrumented compilation and native CPU pass work are evidenced.
- Actual GPU depth/normal/smoothness contents, hit validity and final reflection buffer are **unverified**. CPU counters cannot close that gate.
- Useful whole-frame appearance is **not demonstrated**. No gross static failure is apparent in the nine views, which does not establish temporal stability.
- Continuous motion, manual/audio and separate realtime performance are **not passed** for SSR02. With no useful appearance gain, conditional performance sampling was not triggered.
- Original playable app GUID `40bed5f53c2449418e7fb56bf59739f6` remains unchanged across all 189 hashed files. Earlier control behavior/performance results do not transfer to the experimental preview. Source retains the default-off SSR experiment and Instrumented target setting; it is not an exact control-source restoration.

The prototype has a rejection result. A useful reflected-structure technique remains unresolved; Tasks 2–7 are still pending. Do not roll out the current SSR configuration or infer completion of the Nocturne V2 appearance target.

## One bounded alternative before new rendering work

[Proposal 001h](001h-next-technique-proposal.md) defines one opening-only direct-light origin, aim and footprint candidate with SSR off, existing fixtures and unchanged road material. It is grounded in the current displaced spotlight origins and common forward aim. It would supply direct diffuse/specular pools, **not reflected scene structure**. The banked, elevated road makes a single planar mirror an unsuitable drop-in alternative; a segmented solution would be a separate rendering-system project.

If explaining SSR itself becomes the priority, one isolated warm-gallery GPU mask/hit/final-contribution inspection could resolve whether information is absent or lost. No further parameter sweep is justified by the current stills. The proposal is recorded before any new rendering changes; it has not been implemented or accepted.

---

# SSR01 feasibility verdict — rejected

The installed preview feature compiled and retained resources, but SSR01 is **rejected as configured**. With SSR enabled, the native player logs six RenderGraph failures originating in the realtime reflection system. The SSR pass tries to obtain a descriptor for an invalid camera color texture. The error console contaminates the ON images; probe rendering also cannot be assumed intact. The six events are consistent with cubemap faces, not independently labeled camera telemetry.

The corrected Development player GUID `47944ad3754d45adb51c354c3251c518` finishes both 1,440-frame captures with exit0. Every1920×1080 PNG passes integrity checks; nine natural crossings and failed exact-pose tolerances remain preserved. SSR OFF is free of the propulsion constructor exceptions after the allocation-only Awake correction. Completion and image integrity do not override the failed ON availability gate. No useful-gain or performance verdict is possible from this contaminated comparison. Original control app GUID `40bed5f53c2449418e7fb56bf59739f6` remains unchanged.

## Bounded alternative proposal before further rendering work

Use a project-owned subclass of the same installed feature to skip non-Game cameras before calling base.AddRenderPasses. This confines the preview technique to the actual racing camera while retaining the existing reflection probe, its source objects and capture settings. The pinned feature is public/nonsealed, and its resource/variant strippers recognize derived instances. Replace only the preview feature subasset; preserve SSAO, renderer default and matching SSR-free retention renderer. Build separately as `Vector Rush-SSR-preview-02.app`, with new `evidence/nocturne-v2/ssr-02/` off/on evidence. No package-cache edit, upgrade, new dependency, light/material change or probe-strength iteration is proposed. [Independent diagnosis](001d-reflection-camera-failure-diagnosis.md).

The native marker recorder also needs a correctly instrumented build. The installed Core RenderGraphProfilingScope is guarded by UNITY_INCLUDE_INSTRUMENTATION; that symbol was absent from the observed Development compiler response. The installed Editor API documentation exposes PlayerSettings.SetManagedCodeVariant(NamedBuildTarget, ManagedCodeVariant.Instrumented), and [Unity's explanation](https://discussions.unity.com/t/unity-64-and-development-build-deprecation-and-managed-code-variants/1721546) confirms this setting controls RenderGraph instrumentation independently of Development Build. Use that supported setting in SSR02; do not inject speculative defines. Keep all profiling opt-in and performance measurements explicitly configuration-specific.

A20-second Metal System Trace was attempted on SSR01. Its export remained unfinished after roughly8minutes, and the integrator stopped that profiling process. The partial local trace is not usable GPU-pass evidence. Zero original recorder samples are also inconclusive because the instrumentation scope was compiled out.

## New gates

1. Confirm original renderer/default, both keyword variants, Instrumented compiler symbol and unchanged probe/scene inputs.
2. Confirm logged Reflection-camera skips and Game-camera delegation, no prior RenderGraph/initialization errors, and actual SSR pass samples or output evidence.
3. Fresh OFF must reproduce the intact comparison; ON must supply useful whole-frame response at all required passages. Preserve pose deltas and original PNGs.
4. Only after availability and useful appearance, collect separate off/on/off real-time performance and continuous-motion evidence. No prior runtime settings are retuned to manufacture a gain.

This is a diagnosis-supported camera-scope correction, not proof of production SSR or completion of the later visual milestones.
