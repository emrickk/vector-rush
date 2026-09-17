# Gameplay corrections handoff

Scope: source-confirmed findings 1–5 in `gameplay-critic-001.md`. Source implementation and regression tests only; parent owns commits, integration, test execution and subsequent independent critique. No native, visual, audio or human-play acceptance is asserted.

Bounded corrective checkpoints, defined before implementation:

1. Settings input ownership and rebinding lifecycle (findings 1, 3): shared once-per-frame modal dispatch, close-frame ownership, mouse-only IMGUI activation, controller cancel, and reset on every close/open path. Validate both component Update orders.
2. Finish deadline ordering (finding 2): sample and adjudicate crossings before finalizing timeout. Accept an exact-deadline crossing; reject a later crossing. Preserve the existing standalone ledger advance behavior.
3. Race record provenance and storage (findings 4, 5): latch automated control for the race, normalize duplicate keys to componentwise minima, and contain temporary-file cleanup failures. Validate mixed control, restart, corrupt input and injected storage failures.

The worker's explicit file ownership excludes the central implementation plan and development history; parent must update those during integration.

## Implemented source changes

### Checkpoint 1: own settings input through close and reopen

`ProductionSettingsUI.BlocksRaceInput` dispatches modal input once per render frame before either HUD input or director pause input. It retains an owned-frame stamp after closing. Thus an underlying consumer that runs first dispatches settings first; one that runs later sees the retained ownership. F1/Y toggles return immediately, so an opening press combined with Enter/A cannot activate a settings row. Escape/B cancels a pending binding first and closes the panel on a subsequent press. Every close/open path resets binding capture, including the mouse Back callback and disable.

Settings and HUD consume IMGUI key events before creating interactive buttons. Keyboard/controller activation has one polled path; settings IMGUI buttons require a left-button MouseUp and permit only one mouse/polled action in a frame. `PlayerPreferences.MenuPressed` provides the same keyboard backend to settings, HUD and director, preferring InputSystem in this project's `activeInputHandler: 2` (Both) configuration. Existing gameplay `Held`/`Pressed` selection is preserved.

Parent commit boundary: `ProductionSettingsUI.cs`, `PlayerPreferences.cs`, `RaceHUD.cs`, only the `RaceDirector.Update` menu-input changes, and the input/IMGUI test methods plus shared fixture and test meta. Suggested message: `Fix settings input ownership and rebinding cancellation`.

### Checkpoint 2: adjudicate crossings before timeout

`BeginStep` advances the clock without creating DNF entries. Director samples and sorts all candidate crossings, records accepted finishes, then calls `FinalizeTimeouts` and disables collisions for every completed craft in the same step. The deadline is player finish + 60 seconds; an exact tie is accepted and a later crossing is rejected. Only accepted crossings enter `finishOrder`. Existing `Advance` remains a convenience for no-crossing steps, preserving existing timeout tests and external call sites.

Parent commit boundary: `RaceFinishLedger.cs`, `RaceDirector.FixedUpdate` deadline/adjudication changes, and deadline tests. Suggested message: `Adjudicate finish crossings before deadline finalization`.

### Checkpoint 3: preserve record provenance and best values under failure

`AutomatedRecordExcluded` starts from the player's automation flag at each race start, latches during active race/countdown/pause observations, and latches at the existing `CanSimulate` call immediately before vehicle Update/FixedUpdate selects automated control. Returning to manual control cannot clear it. Restart starts a new eligibility decision. The record write uses this latch rather than the mutable instantaneous flag. A simulation-permission query can conservatively exclude a race even if no AI physics step follows; this favors provenance integrity. The flag is not latched merely by automation on a finished craft.

RaceRecords merges valid duplicate `(course, rules)` entries to independent lap/race minima on load. Invalid entries remain discarded. The unique temporary-file replacement strategy is retained. A failed write rolls back the in-memory improvement; temporary cleanup has its own protected handler. Cleanup failure after a successful replacement logs a warning and preserves the successful result. Internal write/delete delegates provide deterministic failure injection without changing production System.IO behavior.

Parent commit boundary: `RaceRecords.cs`, remaining automation-related `RaceDirector` hunks and record/provenance tests. Suggested message: `Latch automated race exclusion and harden personal best storage`.

Preserve the original independent critic report. These are proposed source checkpoints for parent commits, not commits made by the worker. If the parent stages shared-file hunks, include the shared fixture in the first checkpoint and add the later test methods with their corresponding production changes.

## Checks and expected validation

Completed worker checks: reread all changed production paths and relevant callers, checked balanced lexical delimiters and preprocessor directives in the seven C# files, verified the new test meta GUID is unique under Assets, and ran `git diff --check` over the owned files. Those checks passed. They are not a C# compile or NUnit result. No commit/push, Unity/native/Blender process, or subprocess agent was used.

The new file declares **47 NUnit cases**: 20 in `AAAInteractionTests`, 27 in `AAASettingsInputTests` when InputSystem is enabled. Parent should run both fixtures plus existing `ProductionRaceTests` and `HoverVehicleInputTests`, followed by the normal integrated Editor suite. Expected assertions include:

- Enter/A closes settings with unchanged underlying menu/countdown/restart state in both component Update orders; a fresh later press activates the menu. F1/Y plus accept opens without triggering the underlying menu. Escape cannot cancel/close and also resume the race, including director-first ordering.
- B cancels every binding row while keeping settings open, permits subsequent navigation, and closes only after binding capture has ended. Back and Y close/reopen leave no stale capture.
- Crossings at 69.999 and 70.000 finish when player time is 10.000; 70.001 is DNF. The real director sampling path credits a finish before the deadline and freezes late/DNF craft in the timeout step. Frozen player time and single DNF entries remain stable.
- Entirely manual races save; automation present at start, a director observation, vehicle Update, or vehicle FixedUpdate excludes a race even after the flag is cleared before finishing. Restart resets eligibility.
- Duplicate/corrupt records do not regress minima across improvement and reload. Simultaneous write and cleanup failure preserves prior disk and memory values, logs both failures, and still reaches results. Cleanup failure after a successful replace keeps the committed improvement.

## Harness limits and remaining native checks

Input tests follow the repository's InputSystem EditMode reflection pattern. They reset the three settings frame stamps between simulated render frames because invoking Update does not advance `Time.frameCount`; they never reset between the ordered consumers under test. This proves the intended source dispatch against synthetic input only after execution. It does not verify Unity's native scheduling, physical controllers, or the legacy-only keyboard backend.

The first parent run used a stale isolated test copy whose legacy-backend guards skipped 12 InputSystem keyboard cases. The owned source no longer suppresses those cases: menu keyboard input deliberately prefers InputSystem when the project defines both input backends, so the same synthetic keyboard events now exercise production dispatch. This does not turn the run into legacy-only backend coverage; that configuration still requires a separate project/native check.

The first parent run's three IMGUI failures came from assigning `Event.current` outside Unity's native `OnGUI` event context; the getter remained unavailable, so the production helper correctly reported no event. The corrected test passes explicit `Event` instances through the same classification-and-consumption implementation, while production still supplies the real `Event.current`. IMGUI coverage therefore checks key-event consumption and the actual Back activation callback, but it does not execute GUI.Button layout, mouse hit testing, focus retention, GUI event ordering, or native keyboard-repeat behavior. Parent must mouse-focus settings controls, then use Enter/Space/A, and close/reopen with the mouse, F1/Y, Escape/B. Test underlying Restart, camera-shake and Quit focus, with no accidental action. Quit is intentionally not invoked by the Editor test fixture. Confirm a later fresh press still works.

Director deadline integration tests seed the preceding validated lap state and body position, then invoke the real director sample path. Exact ties use direct ledger times; track projection is not assumed to produce an exact floating-point tie. Native before/exact/after deadline timing remains required.

Record tests write only to disposable test directories. They inject write/delete failures and use actual replacement/reload for successful writes; they do not prove OS permission-denial behavior or crash/interruption durability. A failed cleanup can leave a temporary file, accompanied by its warning. No corruption recovery backup was added. Preferences save-failure messaging, critic findings 6–7, human handling, art and audio quality remain outside this correction scope.

After parent test execution, request independent source critique as directed by the owner. Gameplay acceptance remains open pending those results and the native/human evidence described in the original report.
