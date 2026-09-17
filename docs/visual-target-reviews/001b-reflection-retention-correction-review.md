# SSR OFF variant retention correction review

Scope: independently review `RoadReflectionPreviewSetup.cs` correction against `bd0e817`, plus the generated renderer/pipeline assets available during attempt 02. No Unity execution, tests, native capture, or runtime/settings edits were performed by this reviewer. The earlier [source critique](001a-reflection-source-review.md) remains preserved.

**Result:** the correction addresses the identified P1 OFF-variant stripping defect at the source and generated-prefilter levels. No new blocking source defect was identified. This does not establish native off/on equivalence, shader output, appearance or performance.

## Correction checks

- `RetainRuntimeOffVariants` runs after active SSR registration and before asset saving and player build. It creates a separate renderer clone, removes SSR from that clone only, and preserves all non-SSR references/settings.
- The method verifies that the original `VectorRenderer` is the selected default before changes; appending the clone does not move existing entries or alter the default index. Thus off and on still use the original SSR-capable renderer, with the runtime Volume selecting execution mode.
- Repeated invocation refreshes an existing clone with `CopySerialized`, removes SSR again and appends only if absent. Its asset GUID remains stable. The original source renderer/features are not mutated by the clone's list removal.
- Pinned `ShaderBuildPreprocessor.cs:748–776` collects both renderer requirements; one has SSR and one does not. Their union retains SSR support while `everyRendererHasSSR=false` selects `PrefilteringMode.Select` rather than `SelectOnly`. Pinned `ShaderScriptableStripper.cs:1361–1392` likewise requires rejection by every renderer feature set before removing a variant.
- Global unused-variant stripping is unchanged. Matching renderer settings and sharing the existing SSAO reference avoids requesting unrelated renderer feature combinations. The clone remains a build-retention entry, not a camera-selected renderer.

## Generated serialization observed during attempt 02

- `VectorPipeline.asset` now has the original renderer GUID `99fa91cd9943a4d51b7dd5c15e707a1d` at index 0, plus clone GUID `1826198ef517c466290c8bc5e9fd72bf`. `m_DefaultRendererIndex` remains `0`.
- `m_PrefilteringModeScreenSpaceReflection` changes `2 -> 1`, confirming the installed build preprocessor now requests both SSR keyword states. `m_PrefilterWriteSmoothness` remains `0`, retaining smoothness-output variants. Other pipeline fields have no correction diff.
- The new `VectorSSRVariantRetentionRenderer.asset` has one feature: the original renderer's SSAO subasset reference `775147244208755010`. It contains no SSR feature. Layer masks, stencil, depth settings and Forward+ `m_RenderingMode: 2` match the source renderer. Its `.meta` exists.
- `UniversalRenderPipelineGlobalSettings.asset` has no correction diff. The original active renderer is retained unchanged by this correction.

## Limits and identity

The actual shader/player build is still the integrator's responsibility. Final native logs must distinguish the selected renderer from the extra unused list entry; counting renderer features across the list alone is not camera-selection evidence. Require native SSR-off reproduction of the preserved control and SSR-on pass/output checks before accepting the comparison. All prior image/motion/data/performance gates remain open.

Reviewed setup SHA-256: `ee6ffb11be424cd39c6cf2b41aad95ba9f760f87869dcaf13e9a112a7df0512c`.
