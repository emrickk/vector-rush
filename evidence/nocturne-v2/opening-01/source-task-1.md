# Task 1 report

## DONE

- Added managed-only `OpeningFinishPreview` parsing for `off`, `construction`, `surface` and `combined`; missing/invalid values resolve to off. Both public gates and one startup configuration log are wired without camera/physics changes.
- Added the separate Solstice macOS preview build entry point. It calls `Prepare()` once, verifies the selected default renderer and SSAO state, disables the installed SSR feature in place, uses `ManagedCodeVariant.Instrumented` with `BuildOptions.None`, and reports the same-binary/default-off configuration.
- Default output is `../Builds/Vector Rush-opening-01.app`; optional `-vrOpeningPreviewOutput` is supported. The original app path and any existing output file/directory are rejected before setup/build work.
- Added opt-in `-environmentOpeningStills`: 1,440 pose records, nine sorted natural anchors (`opening01`–`opening05`, `thermal01`–`thermal02`, `warm-gallery`, `station`) and exactly nine flat PNGs whose `frame.file` fields name the files written. Ordinary evidence arrays and modes remain unchanged; success/reference loops use `anchors.Length`.

## Concerns

- Native screenshot completion and all nine progress crossings still require the integrator's player run.

## Source checks

- `git diff --check` passed.
- Source assertions passed for one `Prepare()` call, `BuildOptions.None` only, instrumented managed code, SSR deactivation, output safeguards, both public preview gates, exact anchor/label arrays, dynamic anchor loops and captured-file authority.
- Installed URP 17.6 source confirms the referenced SSAO/SSR feature types and `ScriptableRendererFeature.isActive`/`SetActive`; the managed-code API matches the repository's existing Unity 6 usage.

## Unity compile pending

- Unity was not run, as assigned. Editor/runtime compilation, generated setting changes, separate app build and native 9-anchor/end-status validation remain with the integrator.
