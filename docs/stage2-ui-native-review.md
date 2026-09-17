# Stage 2 native interface review

Review status: the title, HUD and menu polish is ready for owner review. Results/retry and boosted-HUD capture review are pending.

Candidate 07 is [`Vector Rush Stage 2 candidate-07.app`](../../builds/Vector%20Rush%20Stage%202%20candidate-07.app), build GUID `71c571078c934d3599898ec0eaf13382`. Its [build provenance](../../artifacts/stage2/candidate-07/provenance.json) records source revision `a3fe28960bebfb614335679d979f17a52742d726` and a dirty workspace. The UI source is frozen at `dd6e620`.

## Scope

The latest pass follows the owner's request to polish the HUD and menus. Commit `dd6e620` adds the forward-slanted title wordmark, transparent cut-corner primary buttons with a continuous focus outline, spaced menu and instrument labels, and angled boost segments. It changes presentation in `RaceHUD.cs`; race values, action hit areas, navigation, bindings and gameplay remain unchanged.

The comparison target is the six approved views in [`references/interface-stage2a`](../references/interface-stage2a/README.md), particularly the typography, warm-white/lime/cyan palette, open instruments and shared button treatment. Reference timing and race numbers are illustrative. The native interface continues to show actual race, record and ghost state.

## Native appearance

The [44-image screen inventory](../../artifacts/stage2/candidate-07/ui-review/screen-review-inventory.json) covers 11 views at each of 1280×720, 1600×900, 1920×1080 and 2560×1080. Resolution sheets were inspected for layout; the 720p title, racing HUD, pause, audio, controls, bindings and display/comfort screens were also inspected at full size for text fit. All four capture validators identify the Candidate 07 build and pass image/report integrity checks.

The wordmark now follows the reference's forward angle. Primary actions have a genuine transparent cut corner and a continuous outline that follows it. Menu-label spacing and angled boost segments carry the same treatment into the HUD. Start Race and Resume remain visually dominant; the racing line stays open. No clipping, overlapping labels, broken focus outlines or unsafe margins were found in these captures. The [matched native comparison](../../artifacts/stage2/candidate-07/ui-review/native-polish-comparison.jpg) shows the visible effect of this pass.

At 720p, the long Display & Comfort, Quit Application and Edit Bindings labels fit, and the complete binding list and bottom actions remain visible. Secondary hints are deliberately smaller than actions and instruments; these static checks do not establish glance readability while driving. The wider view retains edge anchoring and more space between settings labels and controls.

## Input evidence

The integration owner's [Candidate 06 native input record](../../artifacts/stage2/candidate-06/manual-input-checks.json) identifies build `4d4d75fc93e94226a85dc4a44158a902`. It reports direct native pointer and keyboard checks for navigation, rapid slider drags, binding conflict/cancellation, Confirm remapping, pause/settings isolation, restart, return to title and quit. It also reports persisted settings across relaunch, reset defaults, and an isolated preference-save failure followed by recovery.

These checks were performed by the integration owner and are prior-build evidence. Fresh Candidate 07 input smoke checks, if performed, will be recorded separately. The visual polish does not replace input verification.

## Method and limits

Appearance is assessed from native captures compared with the approved reference views. Programmatic menu captures establish visible states and layout; direct native inputs are a separate source. The [current source EditMode run](../../artifacts/stage2/candidate-07/tests-corrected.xml) passed 217/217 tests, with no skips. An earlier run failed one diagnostic focus-policy test; its correction and rerun are preserved in the candidate evidence. Race-loop evidence is pending.

No physical gamepad test has been performed. Synthetic InputSystem coverage does not establish hardware compatibility or comfort. Automated race captures and silent clips do not assess human driving feel or sound. Still images do not establish real-time performance, transition quality or owner artistic acceptance.
