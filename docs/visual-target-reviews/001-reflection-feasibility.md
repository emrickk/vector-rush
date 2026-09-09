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
