# IonPropulsion native initialization correction review

Scope: independently review the `IonPropulsion.cs` allocation-only diff against `95d574c`, plus the retained attempt-02 native failure. No Unity execution, tests, or runtime edits were performed by this reviewer.

**Source result:** the correction addresses the observed constructor allocation error, with no new blocking source defect identified. Successful native initialization remains pending on the rebuilt app.

## Evidence and change

The retained `evidence/nocturne-v2/ssr-01/attempt-02-off/native-capture.log` reports `UnityException: CreateImpl is not allowed to be called from a MonoBehaviour constructor (or instance field initializer), call it in Awake or Start instead`, pointing to `MaterialPropertyBlock..ctor` and `IonPropulsion..ctor`. The later `SetIntensity` null-reference failure is consistent with that allocation having failed. This occurred with SSR requested off; it is not evidence of an SSR shader failure. `attempt-02-failure.json` preserves the interrupted capture status and archived failed app identity.

The diff replaces the native-backed field initializer with an uninitialized field and allocates the same `MaterialPropertyBlock` in `Awake`. Existing arrays remain ordinary managed allocations. No material values, light values, geometry, throttle response, update logic, camera, physics or race logic changes are introduced.

## Lifecycle review

- The normal bootstrap creates active racer roots, calls `AddComponent<IonPropulsion>()`, then `Initialize(vehicle)`. Unity's active-component lifecycle invokes `Awake` before subsequent frame callbacks; the first use of `properties` occurs in `LateUpdate` through `SetIntensity`.
- `Initialize` itself does not use the property block, so it introduces no new requirement to allocate before that method's body. An inactive object likewise cannot reach a normal `LateUpdate` before its activation/Awake lifecycle.
- Existing `LateUpdate` guards for missing vehicle/material and pause behavior are unchanged. Re-enabling the component retains the allocated block. No new disposal path or repeated per-frame allocation is added.
- No other production code directly invokes `SetIntensity` or `LateUpdate`. The source review does not claim that manually invoking lifecycle methods out of order would initialize a component correctly.

## Required covering check

Use the preserved native failure as the reproduction: the corrected Development player must launch, initialize all six craft, and advance through propulsion updates without the constructor exception or subsequent `SetIntensity` null-reference flood. That native rerun is the relevant covering check; no redundant constant/lifecycle-mirroring test is requested. Inspect the completed log and capture before calling this runtime issue resolved. All SSR comparison and visual/motion/performance gates remain separate.

Reviewed `IonPropulsion.cs` SHA-256: `e02050427b8828917f4e89bf17d31d78a9226a7a22d0676f1788d1df077852ae`.
