# Independent gameplay correction critique · 2026-09-09

**Verdict: the current source closes the original production paths for findings 1–5, but gameplay acceptance remains withheld.** One public deadline API still permits the rejected ordering, settings can still report persistence success after a failed save, and the new input tests do not exercise the native IMGUI event sequence. The parent is running fresh Unity tests; no passing result is assumed here.

This is a source review of the requested files. P2 marks a bounded correctness or regression risk. A coverage gap is not presented as a reproduced runtime defect.

## Actionable findings

### 1. P2 · The public ledger API still makes exact-deadline adjudication order-dependent

`RaceDirector.FixedUpdate` now uses the correct sequence: `BeginStep`, sorted `Cross` calls, then `FinalizeTimeouts` (`RaceDirector.cs:129–164`). The reported timeout-step production bug is closed in that caller.

`RaceFinishLedger.Advance`, however, still calls `FinalizeTimeouts` immediately (`RaceFinishLedger.cs:25–30`). The class therefore exposes two incompatible step contracts. With a player finish at 10 seconds, `Advance(60)` marks the rival DNF before `Cross(1, 70)` can apply, even though `Cross` documents an exact-deadline tie as a finish (`:39–46`). Existing `ProductionRaceTests` also establish `Advance(...); Cross(...)` as a normal call pattern, just away from the deadline.

This is not a current `RaceDirector` failure, since the sole production caller uses the split API. It is a source-proven regression trap in the public ledger contract.

**Correction:** expose one unambiguous step operation. Prefer making clock advance, crossing adjudication, and timeout finalization a single ordered API, or remove/restrict `Advance` so callers cannot finalize before submitting crossings. Add an exact-tie regression through the supported public call pattern rather than only through `BeginStep`.

### 2. P2 · Binding and reset UI still claim persistence success after save failure

`PlayerPreferences.Save` returns `void`, catches `IOException` and `UnauthorizedAccessException`, and only logs (`PlayerPreferences.cs:46–51`). `ProductionSettingsUI` then reports `Binding saved` after every accepted bind and `Defaults restored` after every reset (`ProductionSettingsUI.cs:59–65`, `:88–90`). On a denied or failed write, the in-memory setting changes while the old disk value survives, so relaunch can contradict the success message.

This is current source behavior, not a stale test failure. The corrections handoff explicitly leaves preference save-failure messaging outside its scope, so finding 3's controller cancellation is fixed while the rebinding failure path remains incomplete.

**Correction:** make preference save return success, keep success copy conditional on it, and show an actionable failure state. Test bind and reset against a disposable unwritable/fault-injected store, then reload to verify that displayed status matches persisted state.

## Regression and evidence gaps

### Modal ownership and event double activation

The polled source path is coherent. `BlocksRaceInput` dispatches settings first and retains `ownedFrame` after close (`ProductionSettingsUI.cs:13–23`, `:27–31`); HUD and director query it before their inputs (`RaceHUD.cs:60`, `RaceDirector.cs:107–115`). Escape/B now cancels rebinding before a later close (`ProductionSettingsUI.cs:47–58`). No source path was found that lets the same polled Enter/A/Escape/B event reach the underlying menu.

The tests do not establish native event safety. `BackActivationAndReopenResetPendingBinding` invokes `Activate` directly instead of executing `GUI.Button`, hit testing, focus retention, or callback order (`AAAInteractionTests.cs:410–421`). `MenuGUIKeyEventClassifierConsumesActivationKeys` supplies standalone `Event` objects rather than a real `OnGUI` pass (`:245–257`). The close-order matrix covers underlying start/resume/restart, but not camera shake or quit (`:334–360`). Phase assertions would not detect an accidental shake toggle, and quit is deliberately not invoked.

**Required regression checks:** in the native player, mouse-focus each settings control, then close with Enter, Space, A, Escape, B, F1, Y, and mouse Back. Repeat with the underlying focus on Restart, Camera Shake, and Quit. Record one action count per frame and assert the underlying preference, phase, and quit request stay unchanged until a fresh press. Test both mouse-open and mouse-close ordering. Automated EditMode quantity is not evidence of physical-device or IMGUI correctness.

### Deadline semantics

The new direct cases cover before, exact, and after the deadline, and the director test exercises a sampled before/after crossing (`AAAInteractionTests.cs:60–119`). The exact native tie remains untested because the integration case intentionally uses direct ledger time for equality. Native floating-point projection and fixed-step timing still require logged old clock, new clock, crossing fraction, computed crossing time, deadline, and adjudication.

### Automation provenance

The latch is reset from the automation flag at race start, observed during the active race, set by the player's `CanSimulate` query before automated control selection, and used for the record write (`RaceDirector.cs:33–40`, `:71–86`, `:125–183`). Returning the flag to false cannot clear it. No false-negative path was found for `AutopilotForTesting` in the reviewed source.

The tests use synthetic flag changes and reflected component calls (`AAAInteractionTests.cs:121–160`). They do not prove that every native automation or evidence-driving route sets `AutopilotForTesting`, nor do they inspect a native profile after relaunch. Confirm the actual automation harness leaves a race-scoped provenance trace and that a mixed automated/manual native race cannot create or improve a record. A conservative false positive remains intentional because a permitted automated simulation query latches exclusion even if no following AI physics step occurs.

### Record corruption and storage failure

Valid duplicate keys are reduced to independent lap/race minima, caught write failures restore the prior in-memory entry, and cleanup failures no longer escape (`RaceRecords.cs:21–34`, `:40–62`). I found no source-proven regression in those fixes.

The tests cover malformed/unsupported JSON, duplicate normalization after an improvement, an injected failure before replacement, and an injected cleanup failure (`AAAInteractionTests.cs:163–243`). They do not prove OS permission-denial behavior, failure during `File.Replace`, process interruption, directory loss, or crash durability. There is still no backup recovery for a corrupt record file. Those are explicit resilience limits, not passing evidence.

## Acceptance limits

- No fresh Unity result existed when this review was written. Test declarations and case counts are not results.
- No native player, physical controller, IMGUI event loop, filesystem permission fault, process interruption, audio, or continuous motion was inspected.
- No human race was played. This review cannot establish handling feel, discoverability, fairness, recovery quality, or Wipeout Omega Collection-level playability.
- Findings 6–7 from the earlier critique remain outside this correction review. Source correctness for findings 1–5 would still not satisfy gameplay acceptance.

**Next gate:** fix or constrain the ledger's order-sensitive public API and make preference persistence status truthful, then run the fresh Unity suite. Follow with the focused native modal, deadline, mixed-automation, and storage-failure checks above before any gameplay acceptance claim.
