# SSR02 camera-scoped source review

Scope: independently review changes since `b7ef66d`: new `RoadReflectionRendererFeature`, setup replacement/managed instrumentation, optional marker additions and available generated serialization. No Unity execution, native run or source/settings edits were performed by this reviewer. `git diff --check` passed.

**Source result:** no blocking defect identified in this scoped correction. The implementation follows the separate proposal in [001d](001d-reflection-camera-failure-diagnosis.md). The rejected unfiltered prototype remains rejected; SSR02 still requires native verification.

## Checks

- The guarded project-owned subclass calls `base.Create`, preserving installed pass construction, then detects the opt-in diagnostics flag. Its ordinary managed `HashSet<CameraType>` initializer performs no Unity native allocation.
- `AddRenderPasses` reads the public `renderingData.cameraData.cameraType`, rejects every non-Game camera before base invocation, and delegates Game cameras unchanged. This avoids SSR pass enqueueing on the identified reflection-camera route without changing probe construction, refresh settings, materials or lighting. It deliberately excludes SceneView/Preview/Reflection/VR camera types; it does not establish arbitrary offscreen Game-camera compatibility.
- Camera-route logs are opt-in and once per encountered type. They explicitly distinguish delegation from execution. An off-mode Game-camera delegation is therefore not incorrectly presented as a pass being run.
- Setup finds the existing base-compatible SSR entry and rejects duplicates. It creates a subclass subasset, replaces that one list slot, then destroys the old subasset. Existing SSAO and list order remain intact. Repeated builds reuse the subclass. The retained SSR-free renderer is refreshed afterward; its `is ScreenSpaceReflectionRendererFeature` removal also removes subclasses.
- The installed feature/resource strippers use base-compatible `as`/`is` checks. Subclass registration remains recognized. Existing private-resource diagnostics explicitly reflect on the base type, so they continue to find inherited private resource fields.
- Output changes to the separately named `Vector Rush-SSR-preview-02.app`; it does not overwrite SSR01. Runtime profile settings and command-line default-off semantics remain unchanged.
- `PlayerSettings.SetManagedCodeVariant(NamedBuildTarget.Standalone, ManagedCodeVariant.Instrumented)` is supported by installed `UnityEditor.CoreModule.xml` member 42762. Its enum documentation at 34535 states that Instrumented uses optimized C# and defines `UNITY_INCLUDE_INSTRUMENTATION` and `ENABLE_PROFILER`. This matches the intended RenderGraph instrumentation path without injected defines. No script-debugger or automatic profiler connection is enabled.
- Added `SSR` and `Inl_` marker variants are supported by the pinned `ProfilingScope.cs` documentation: the primary and inline marker names are both registered under `ProfilerCategory.Render`. Inline samples cover calling-thread work; they do not establish GPU output correctness or isolated GPU cost. Existing diagnostic-only activation and honest output labels remain intact.

## Generated state observed during the build

`VectorRenderer.asset` replaces only the SSR subasset/reference with the new script GUID `e0ec9fda102d4edc8372bd30f5180951`; `m_Active=1` and `afterOpaque=0` remain. SSAO is unchanged. `VectorPipeline.asset` retains `m_DefaultRendererIndex=0` and SSR prefilter `1=Select`, with no diff from the preceding retained-variants state. `ProjectSettings.asset` gains `managedCodeVariant.Standalone: 2` through the supported API. The new script `.meta` is present.

## Evidence limits

Managed instrumentation is a saved target setting and part of the new build identity. Label SSR02 timing with this configuration; compare off/on on the same binary, and do not silently equate it with earlier build configurations. Instrumented is distinct from Checked: the installed enum documentation assigns additional validation defines to Checked. Absence of a validation exception under Instrumented alone cannot retroactively prove a defect corrected under Checked, although the earlier allocation source correction is independently reviewed.

Required native gates remain: Reflection skip and Game delegation telemetry; no observed RenderGraph exception/console contamination; functioning unchanged probe contribution; main Game-camera SSR pass/output and effective volume; fresh same-binary off/control equivalence and on appearance; original data, motion and conditional performance gates. Positive CPU marker samples narrow execution uncertainty but cannot close GPU/data/image gates.

## Reviewed SHA-256 identities

| File | SHA-256 |
| --- | --- |
| `RoadReflectionRendererFeature.cs` | `f342d1ef67e8073cff6ef0d78e88ef36ee089fd58c3b019fb07bec4ff2cf42b9` |
| `RoadReflectionPreviewSetup.cs` | `3fda98663cefbcbbc0d180b6b4aba7d57204d8e121760345904326d585fc6ab2` |
| `RoadReflectionPreview.cs` | `bfab378c302885225f376a6041b8ed735113cc41090bffa9a3d2d003597c8809` |
