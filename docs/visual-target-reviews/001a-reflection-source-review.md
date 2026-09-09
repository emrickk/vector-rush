# SSR preview independent source review

Scope: Task 1 in the Nocturne V2 execution plan; source baseline `ac6992c`. This review reads the new preview setup/runtime, bootstrap diff, project settings diff, existing preparation routine, and installed URP SSR feature/pass/resources/stripper/Lit code. It does not execute Unity, tests, native capture, or performance sampling. Native evidence is pending.

## Findings

No blocking source defect identified in this snapshot. This is not an SSR availability, appearance, motion, or performance pass.

1. **Evidence interpretation:** `SSR - Depth Pyramid Generation` is conditionally executed only for non-linear marching in the installed `ScreenSpaceReflectionPass.RecordRenderGraph`. The selected configuration is Linear; a zero value for this diagnostic marker is expected. It must not be interpreted as SSR failing to execute. Upscaling and the final-blit path are the relevant selected-path markers, with actual GPU/output inspection still separate.
2. **Performance comparability:** the preview explicitly uses `BuildOptions.Development`, whereas ordinary `BuildMac` uses `BuildOptions.None`. The same preview binary supplies a valid internal off/on/off comparison, but absolute timing against the historical non-development control also includes build-configuration differences. Preserve that qualifier alongside the provisional P95/P99 thresholds; this source review does not resolve them.
3. **History remains involved:** `temporalFiltering=false` disables that filtering stage, but `afterOpaque=false` still requests motion vectors and consumes prior-frame color/depth for reprojection. Native turn/occlusion/history-artifact checks remain necessary. A temporal-filtering flag alone cannot establish absence of history artifacts.

These are interpretation constraints, not requested source corrections.

## Source checks

- `EnableDefine` contains no SSR type references, preserves existing Standalone symbols and adds only the specified symbol. The separate `BuildPreview` invocation fails explicitly if the symbol was not compiled. The settings diff observed here adds only `Standalone: URP_SCREEN_SPACE_REFLECTION`.
- `BuildPreview` invokes `Prepare` once, finds or creates a feature subasset, rejects duplicate registered SSR features, sets `afterOpaque=false`, activates it, marks assets dirty and saves before `BuildPlayer`. Existing SSAO and renderer feature entries are retained. It targets the separate preview app and throws on failed build; the ordinary control build method is not called.
- The installed resource stripper retains persistent resources when an active SSR feature is present in a renderer used by a build pipeline asset. Existing preparation assigns `VectorPipeline` to Graphics/Quality settings. Setup checks both actual shader resource references; this supports the intended retention path but does not prove native retention or shader support.
- `Configure` runs before the global Volume receives its profile. Missing, invalid, or differently cased option values select Disabled; only the exact `-vrRoadSSR on` token pair enables OpaquesOnly. All stated starting settings match Task 1. Guarded no-symbol builds log an effective Disabled state.
- The runtime change to `VectorBootstrap` is the single profile-configuration call. No world/road-light/craft/physics/camera/race source diff was present. New scripts have accompanying `.meta` files. Preparation is pre-existing and can reserialize assets; post-build serialized diffs still require integrator inspection.
- Road cubemap opt-out is compatible with SSR in the pinned Lit code: `_SCREENSPACEREFLECTIONS_OFF` gates SSR sampling separately from `_ENVIRONMENTREFLECTIONS_OFF`, and the SSR result is blended afterward. Existing road preparation does not introduce the SSR opt-out. Actual native material/keyword checks remain pending.
- Diagnostics are opt-in through `-vrSSRDiagnostics`. They distinguish requested mode, native introspection, shader support, CPU marker samples and unverified GPU output. Runtime road map reporting explicitly says pixels are not inspected. Exceptions produce an unverified-state warning. These records must not substitute for depth/normal/smoothness image inspection or native output observation.

## Unresolved native gates

Compilation and separate build success; final renderer/global-settings retention and unintended serialized changes; native feature/resources/effective volume; depth/normal/smoothness data; actual pass/output; preview-off equivalence to retained control; all nine natural-crossing views and pose deltas; continuous opening motion and artifact inspection; useful whole-frame gain; and, if gain warrants it, isolated off/on/off real-time performance. No retain/reject technique verdict is issued here.

## Reviewed file identities

SHA-256 at review time (integrator work was concurrent):

| File | SHA-256 |
| --- | --- |
| `UnityProject/Assets/Editor/RoadReflectionPreviewSetup.cs` | `80b112d9c4bbceb931eb25cbaf57622b2539fa05c7c05a60880cb595591f762d` |
| `UnityProject/Assets/Scripts/Presentation/RoadReflectionPreview.cs` | `dcdcc078e38e92abd0c057a8b130527475cba098dbee5f4706779843cc55127a` |
| `UnityProject/Assets/Scripts/VectorBootstrap.cs` | `aa406006ccd7c4c3cc477b8a1cadfe21f33246ab1a5c11402a89d53dd8b42eee` |

Installed URP source read from `UnityProject/Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184/`; no package edits were made.
