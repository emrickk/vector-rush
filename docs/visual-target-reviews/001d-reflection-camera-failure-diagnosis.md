# Native SSR reflection-camera failure diagnosis

Scope: read-only diagnosis of the SSR-on native exception and feasibility of a project-owned camera-filtering renderer feature. This report proposes a bounded follow-up experiment; it does not change the failed prototype into a passing result. No Unity execution or code/settings edits were performed by the reviewer.

## Observed failure and diagnosis

`evidence/nocturne-v2/ssr-01/on/native-capture.log` contains six `NullReferenceException` stacks entering `ScreenSpaceReflectionPass.CreateRenderTextureHandles` at line 635 through `BuiltinRuntimeReflectionSystem.TickRealtimeProbes`. At that source line, SSR calls `resourceData.cameraColor.GetDescriptor(renderGraph)`, which reaches the failing RenderGraph resource-descriptor lookup. The log identifies a failing reflection-system rendering route; it does not directly print each camera's type or cubemap face.

Existing `WorldBuilder` creates a realtime, script-refreshed, 256-pixel coastal reflection probe with `AllFacesAtOnce` and calls `RenderProbe()` after a frame. Six exceptions are consistent with the six cubemap faces of that render. That face attribution remains an inference from source plus the stack, not explicit per-face telemetry. The concrete fault is SSR assuming a usable `cameraColor` descriptor on this route. No main racing-camera failure is established by these six stacks alone.

The installed feature checks offscreen-depth, orthographic projection and disabled Volume mode before resource preparation/enqueueing. It does not reject reflection cameras. Its before-opaque path then reaches the descriptor assumption during RenderGraph recording.

## Source-supported proposal

Create a separately identified follow-up candidate with a project-owned subclass of `ScreenSpaceReflectionRendererFeature`. Override `AddRenderPasses(ScriptableRenderer, ref RenderingData)` and return before calling the base method whenever `renderingData.cameraData.cameraType != CameraType.Game`; otherwise delegate unchanged to the base implementation. The public base class is non-sealed, the override is not sealed, and the legacy camera-data API exposes the camera type publicly in this pinned package.

This prevents SSR resource preparation, input requests and pass enqueueing on the observed reflection route. The probe itself still renders with its existing settings, geometry, materials and lights. Racing Game cameras keep the installed SSR implementation and planned volume parameters. SceneView, Preview, Reflection and VR camera types are outside this proposed candidate's SSR scope; label that restriction explicitly. CameraType.Game can also describe offscreen game cameras, so this is a bounded exclusion of the observed route rather than a universal camera-compatibility fix.

The installed resource stripper recognizes features by `is ScreenSpaceReflectionRendererFeature`, while shader feature collection uses `as ScreenSpaceReflectionRendererFeature`. Both recognize a subclass. The existing SSR-free retention renderer must remain SSR-free, and the selected renderer must contain exactly the subclass feature rather than both base and subclass instances. Current clone removal using `is` also removes subclasses correctly.

The pinned `DrawObjectsPass` sets the SSR shader keyword from the current valid SSR texture handle; `UniversalResourceData.Reset` clears that handle each frame context. This supports skipping the feature without intentionally leaving a prior game-camera SSR texture active on the probe. Native reflected output should still be checked; source inspection alone cannot prove its rendered result.

## Gates before accepting the follow-up

1. Preserve the current failed app/source, six errors and contaminated capture as a rejected comparison. Give the filtered candidate fresh identity and evidence paths. Do not use error-console-contaminated images as an appearance control.
2. Verify subclass compilation, one selected feature, retained resources and computed SSR prefilter `Select(1)` with the original default renderer unchanged. Avoid package changes or global stripping expansion.
3. Record bounded camera-route diagnostics showing Game cameras delegated and Reflection cameras skipped; the first six-stack failure did not itself log camera types. Confirm the existing probe is still requested/rendered and its material contribution remains valid.
4. Reproduce the native SSR-on startup on the new binary with no RenderGraph/descriptor exceptions and no console overlay. Confirm main Game-camera SSR pass/output still executes; merely eliminating exceptions by never running SSR would fail the proposal.
5. Obtain a fresh off/on pair on that exact corrected binary, verify off reproduces the retained control, and complete the original native data, nine-view, motion and conditional performance gates. The existing completed off run supports the preceding candidate only; it cannot substitute silently for the new binary's control.

**Feasibility verdict:** a project-owned Game-camera filter is a source-supported, narrowly scoped proposal to avoid the observed unsupported reflection-camera path while preserving probe inputs. Its native efficacy and visual value remain unverified. The current unfiltered prototype remains failed.
