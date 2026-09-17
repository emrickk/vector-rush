# Independent gameplay/code critique · 2026-09-09

**Verdict: gameplay acceptance withheld.** The existing evidence supports repeatable automated race completion. It does not establish polished player interaction, robust deadline adjudication, record integrity under exceptional conditions, or Wipeout Omega Collection handling quality. Fix input ownership and deadline ordering first, then test actual human racing.

This is a source review, not a claim that every proposed reproduction has been executed. P1 means a correctness issue to resolve before gameplay acceptance; P2 means a bounded robustness or presentation defect; an acceptance gate requires measurement before a quality verdict. No P0 failure was established.

## Evidence and limits

- Read `AGENTS.md`, `docs/aaa-rebuild/dispatch.md`, all eight requested classes, relevant tests, `RaceProgress`, bootstrap wiring, and native validation implementation. Applied rigor-pass to distinguish source facts, supplied evidence, and untested hypotheses. Implementation, delegation, git/history changes, and native execution were deliberately excluded by this assignment.
- `evidence/nocturne-production/direct-03/tests.xml` reports **63 passed, 0 failed, 0 skipped**, on 2026-09-09 at 22:44 UTC. Its five `ProductionRaceTests` and five input test cases cover useful subsets, not the full interaction flows below.
- `races/validation.json` reports **three completed automated native races**, six finishers per race, no DNF, zero recoveries for every craft, and passing frozen-result, pause, and restart flags. The player finishes first in all three at 128.320358 seconds. This repeats one starting configuration and automated driving policy; it does not cover a trailing human player, recovery into traffic, settings, or deadline-edge finishes.
- Build identity and validation share GUID `723d5bf92401486a9fbfdcd56424081b`, Unity `6000.5.3f1`. SHA-256 checks of the eight reviewed classes, `ProductionRaceTests`, `HoverVehicleInputTests`, `ProductionEvidence`, and `VectorBootstrap` matched `source-identity.json`. That connects this review to the supplied source manifest; I did not independently inspect the compiled binary.
- The native pause flag checks frozen finish time, finish count, and time scale for half a second after player finish. It does not assert all body transforms, drive inputs, or paused settings interactions (`ProductionEvidence.cs:46–55`). Its restart flag checks empty finish records and zero player completed laps (`:39`). Performance fields are zero because race mode never enables timing measurement (`:42`, `:59`); these are not performance results.
- No Editor/native run, image inspection, audio listening, human play, or new executable test was performed. No visual verdict is offered. Native log shutdown messages include “may have been prematurely finalized”; the supplied report completes, so these messages alone do not establish a gameplay failure.

Source references below are relative to `UnityProject/Assets/`. Line numbers refer to the inspected source snapshot.

## Ranked correctness findings

### 1. P1 · Settings does not own its activation press through the end of the frame

**Source-confirmed ordering hazard; manifestation in the native build remains to be reproduced.** `Scripts/Presentation/ProductionSettingsUI.cs:38–45` accepts Enter/A and `:58` immediately sets `IsOpen=false` for “Back to race menu.” `Scripts/Presentation/RaceHUD.cs:60–87` independently polls the same frame's Enter/A press, guarded only by the current value of `IsOpen`. If settings updates before HUD, closing settings also activates the underlying focused item: start, resume, restart, camera shake, or quit. There is no consumed-input token or frame barrier in these paths. A boolean modal guard is insufficient at the transition itself.

Reproduction proposal: open settings from a paused race, select Back, press Enter once; repeat with controller A, and with the underlying pause focus on Restart and Quit. An integration test should invoke the two Updates in both orders using one input event and require exactly one action. Also test keyboard activation after mouse-focusing a settings `GUI.Button`: settings has both manual Update activation and IMGUI buttons (`:69–77`), so duplicate activation through those routes is an additional unverified risk, not a demonstrated second bug.

**Next native/human test:** operate these transitions with keyboard and a physical controller while recording phase, settings-open state, and action counts per frame. Closing settings must leave the underlying menu unchanged until a fresh press. Correct by establishing one input owner/consumption mechanism, including IMGUI activation.

### 2. P1 · A legitimate crossing in the timeout step becomes DNF

**Source-confirmed deadline-ordering bug.** `Scripts/Gameplay/RaceDirector.cs:125–137` advances the ledger before sampling crossings. `RaceFinishLedger.cs:30–32` immediately completes all remaining racers as DNF when the new clock reaches player finish + 60 seconds. The director then skips those racers at `:133`, so interpolation never gets a chance to credit an earlier crossing within that step.

Concrete trace: player finishes at 10.000; ledger clock is 69.995; next delta is .010; a rival crosses at 69.999. `Advance` makes the clock 70.005 and records DNF, even though the crossing preceded the 70.000 deadline. Existing timeout coverage (`Tests/Editor/ProductionRaceTests.cs:17–22`) has no same-step crossing. Process crossings against the deadline before finalizing timeouts, with an explicit policy for an exact tie.

**Next native test:** arrange one unfinished rival just before the finish as the deadline expires; log the old/new clock, crossing fraction, accepted crossing time, and DNF. Test before, exactly at, and after the deadline. This is controlled native correctness evidence, not human feel evidence.

### 3. P2 · Controller cancel cannot exit rebinding

**Source-confirmed missing input path.** In `ProductionSettingsUI.cs:24–35`, rebinding returns before the controller B cancel handler at `:41`. Rebinding accepts keyboard keys or Escape; a controller user who presses A on a binding row cannot cancel with B. Y can close settings through `:20–22`, so this is not a total lockout. Closing via Back also leaves `rebinding` uncleared (`:58`), whereas F1/Y toggle clears it (`:22`); a mouse-closed binding session can therefore reopen in its old binding state.

**Next native/human test:** using only a physical controller, enter each binding row, press B, then navigate away. Expect B to cancel rebinding before closing the panel. Separately close a pending binding with the mouse Back button and reopen with the settings button; expect no stale binding capture.

### 4. P2 · Record exclusion describes the last instant, not the whole race

**Source-confirmed provenance gap; the three supplied races are correctly excluded by their persistent true flag.** `RaceDirector.cs:168` passes only the current `Player.AutopilotForTesting` value to `RaceRecords.Record`. That flag is public and mutable (`HoverVehicle.cs:18`); automation is not latched from race start or from any automated tick. Run most of a race under autopilot, set the flag false before finishing, and the resulting record is eligible. `ProductionRaceTests.cs:29` proves that a true argument is rejected; it does not test the integrated mixed-control race.

**Next native test:** in an isolated test profile, automate the first two laps, return control before the finish, finish manually, relaunch, and inspect the record. The race must remain excluded. Add race-scoped eligibility that is permanently invalidated by automated control; do not infer that the shipped UI currently exposes an autopilot cheat switch.

### 5. P2 · Record corruption handling can make best times regress; cleanup can escape the save handler

**Source-confirmed exceptional-state defects.** `RaceRecords.cs:17–19` removes invalid entries but retains duplicate `(course,rules)` keys. `Get`/`Record` use the first match (`:24`, `:30`), then remove that entry and append its improvement (`:33`). A structurally valid file with duplicate times `(40,125)` and `(50,160)`, followed by `Record(...,39,124,false)`, leaves the slower duplicate first. `Get` now returns `(50,160)`, despite a successful improvement. Such a file is not produced by the ordinary single-instance writer, so this is corrupted-input robustness, not a demonstrated normal-play loss.

The ordinary write strategy is sound in intent: unique temporary file, same-directory replace/move, and rollback on caught write failure (`:34–44`). Do not label it a direct truncate-in-place implementation. However, `finally { File.Delete(temp); }` at `:46` is outside the catch protection. If saving and cleanup both fail, cleanup throws rather than returning false, including when called from race finish. Current tests cover successful replacement and malformed JSON (`ProductionRaceTests.cs:24–35`), not duplicate keys, denied writes, cleanup failures, or interruption during replacement. There is no corruption backup/recovery path; malformed input defaults to empty records.

**Next native test:** use a disposable profile containing duplicate-key JSON, improve the best, restart, and check that each key resolves to componentwise minima. In the same fault-injection validation session, deny writes/cleanup and require old disk contents to survive, gameplay to reach results, and failure to be reported without an escaping exception. Actual interrupted-write testing is needed before claiming crash durability.

## Presentation and handling acceptance failures

### 6. P2 · Race results omit the field's results, and rebindings leave instructions stale

**Source-confirmed product omissions.** `RaceHUD.cs:410–428` displays only player position, race time, best lap, and personal-best comparison. It never reads `FinishRecords`, so the distinct rival times and DNF decisions being validated are invisible to the player. The “COMPLETE” results presentation begins while rivals can still race. Add the ordered field with explicit running/finished/DNF states and a stable player result.

`RaceHUD.cs:445–448` always prints WASD, Space, Q/E, and R, even after `PreferenceData.Bind` changes them. Also, boost exhaustion is latched until release (`HoverVehicle.cs:165–168`), while HUD status uses only charge and boosting (`RaceHUD.cs:287`). A player holding boost can recharge to a seemingly available state without learning that release is required. RaceAudio contains countdown/start/finish/boost/impact cues (`RaceAudio.cs:111–140`), but no lap, wrong-way, or recovery cue. Recovery reasons currently go to a debug log (`HoverVehicle.cs:358–370`). These are feedback gaps; source cannot determine their perceived severity or audio quality.

**Next native/human test:** have a new player change throttle/boost/recover bindings, exhaust and hold boost, deliberately recover once, and finish behind at least one rival. Ask them to identify controls, why boost will not engage, what recovery cost, and the field's finishing order using game feedback alone. Observe continuous native motion/audio; no still-image or automated-steering substitute.

### 7. Acceptance gate · Rival/player physics differ, and safe traffic recovery is unproven

**Measured fairness and subjective feel remain open; there is no basis for declaring the game unfair or enjoyable from this source alone.** Bootstrap sets player cruise/boost to 53/72 m/s (`Scripts/VectorBootstrap.cs:87–90`), while rivals retain 78/108 defaults (`HoverVehicle.cs:13–14`). Rivals additionally receive up to 6 m/s² of inward acceleration through the corridor guard; the player is explicitly excluded (`:273–274`, `:295–300`, `:313–326`). Therefore a claim that all entrants use identical physical assistance would be false. Whether those compensations yield good competition requires play. The automated player winning all three races does not answer it.

`HoverVehicle.cs:367–380` respawns a human onto the center of the last validated gate with zero velocity, without checking nearby traffic or adding collision grace. This is a confirmed absence of clearance handling; an actual overlap collision is a testable consequence, not observed evidence here. Outside the guard, ordinary blocker/lane checks use a fixed 5.5 m lateral separation (`:217`, `:238`), while the guard accounts for yaw-dependent collider width (`:258`, `:338–345`). The bootstrap box is 5.2 by 7.2 m (`VectorBootstrap.cs:66`), so side-by-side clearance under yaw deserves targeted testing. Collision-free behavior cannot be inferred from zero recoveries.

**Next native/human test:** a player races from the back through traffic, deliberately rubs both walls, passes with airbrakes, and recovers with another craft crossing the respawn gate. Record contact impulses, recovery chains, progress/position, and the player's account of control and predictability. Compare matched player/rival wall disturbances to understand the assistance difference. Acceptance requires human handling evidence at the intended speed and complete race length.

## Scrutinized issues that are not established blockers

- **Finished craft collision:** current finish processing makes craft kinematic and disables collisions (`RaceDirector.cs:158–160`); timed-out craft receive the same treatment in `:133`; restart re-enables collisions (`:71`). AI explicitly excludes finished craft from blocker and lane scans (`HoverVehicle.cs:213`, `:233`, `:257`, `:263`). Do not repeat an obsolete “finished craft physically block traffic” allegation. The lifecycle unit test manually disables collisions (`ProductionRaceTests.cs:58`) rather than asserting a real finish caused it, so native coverage should still include a human finishing last and passing already-finished craft. No renderers are disabled by this finish path, and rival audio/minimap do not filter finish state (`RaceAudio.cs:101–106`, `RaceHUD.cs:345–350`); inspect/listen to that treatment before artistic adoption, without claiming an unseen visual defect.
- **Paused drive behavior:** `HoverVehicle.Update` clears drive/recover input while `CanSimulate` is false (`:84–94`), and pause sets time scale to zero (`RaceDirector.cs:86–98`). The input test genuinely covers a North recovery press during pause, release, and resume (`HoverVehicleInputTests.cs:69–85`). No retained-recovery bug was established for that sequence. Held RT/A will be read again after resume, which is a policy choice, not inherently a bug. Test A-to-resume while holding RT and keyboard R in the pause menu; R intentionally restarts there (`RaceHUD.cs:89`). Native evidence currently bypasses these inputs by calling `TogglePause` directly.
- **Timeout scope:** the 60-second deadline begins only after the player finishes (`RaceFinishLedger.cs:31`). A stranded/idle player never becomes DNF from a rival finishing. That can be intentional single-player behavior; it needs an explicit design decision, not a fabricated correctness charge.
- **Settings storage:** preferences clamp finite values, reject duplicate/reserved bindings, and replace a temporary file (`PlayerPreferences.cs:16–35`, `:46–57`). Save failures only log, and UI can still say “Binding saved” (`ProductionSettingsUI.cs:30`). Validate denied-write/relaunch behavior before claiming persistent settings are reliable under failure. This is separate from the demonstrated normal round-trip test.

## Recommended next bounded correction

Give settings exclusive ownership of each input event through its close transition, including cancel during rebinding, and add integrated tests for both component Update orders. Correct ledger deadline adjudication in a separate change with before/at/after-deadline crossing tests. Preserve separate corrective commits and this independent report through the parent workflow. Then run the listed native cases and one complete human race; existing automated passes remain useful lifecycle evidence, with gameplay/art/audio acceptance still open.
