# Handoff 04: replay motivation, UI, sound and feedback

Use **GPT-5.6 Sol**. The user authorized executing this lane of Vector Rush's polished-race plan.

Shared packet: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush/docs/plans/2026-09-14-playable-race`.
Original project: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush`.

Read the packet's `README.md`, `coordination.md`, `contracts.md`, workspace `AGENTS.md`, current `RaceRecords.cs`, `RaceHUD.cs`, `RaceAudio.cs`, `PlayerPreferences.cs` and `ProductionSettingsUI.cs`. Work only in your assigned isolated workspace. If setup is missing, inspect read-only and report the dependency.

## Outcome

Give players a concrete reason to retry the same course and clear sensory feedback while racing. Build on the existing personal-record, settings and audio systems rather than replacing them.

First inventory actual current behavior. The previous review found later source already contains personal records, procedural music and nearby-rival audio. “Add music” or “add records” is not the assignment.

## First bounded implementation

Add an optional personal-best lap ghost and three fixed course-sector comparisons.

- Use the authoritative gameplay clock/progress and `contracts.md`. Start with existing observation APIs; request missing hooks through Integration rather than editing `RaceDirector`.
- Record only eligible manual laps. Automated/test driving cannot update records or ghosts; any mixed automated/manual lap stays ineligible. Follow the agreed recovery/invalid-lap rule.
- Store a bounded position/orientation/time recording. Key it to course identity, driving-rules revision and recording schema. Keep file size/sample rate bounded and document interpolation.
- Ghost has no collider, rigidbody gameplay effects, AI target identity, ranking or sound source. Use a restrained translucent treatment with sufficient separation from real rivals. Hide it when it obscures the player at launch; expose an off toggle.
- Playback follows lap time, freezes on pause, resets on restart and finishes cleanly. An old ghost never leaks into a different course/ruleset.
- Sector boundaries default to normalized progress one-third, two-thirds and finish. Handle wrap, ordered forward crossing and recovery without fabricated fast splits.
- First-ever play has a clear empty state. Explain incompatible or unreadable recordings without breaking the race. Safe persistence failures must not show a successful save.
- Show useful result deltas and a direct retry action using the existing visual language. Do not add accounts, online leaderboards, progression currencies or a new menu design.

Racing may still be tuning. Develop recording/storage/ghost components and tests against the agreed interface first. Final native comparisons must use the integrated driving-rules identity.

## Sensory feedback pass

Audit existing engine, boost, impact, rival and finish sounds and VFX. Improve distinguishability at normal play volume: boost should communicate commitment, nearby traffic should help locate competitors, impacts should communicate severity, and the finish should resolve the race.

Preserve volume settings and comfort options. Avoid louder-everything mixes, opaque trails, excessive shake or HUD clutter. Camera behavior belongs to Racing; request changes through Integration. Review moving visuals and audible output before making quality claims. Waveform existence or successful compilation is not listening evidence.

## Ownership

Own `UnityProject/Assets/Scripts/Gameplay/RaceRecords.cs`, new `Assets/Scripts/Replay/`, and `Assets/Scripts/Presentation/` except existing opening/road/ship diagnostic render tools. Own new lane-specific tests named `ReplayPolish...` or `PresentationPolish...`.

Do not edit gameplay controllers, `ChaseCamera`, world art, scenes, bootstrap, Packages, ProjectSettings or shared tests. Provide a minimal shared-file patch request in your report for Integration.

## Acceptance

Verify eligible save/load, no-ghost first run, incompatible/corrupt recording fallback, bounded storage, pause/restart synchronization, toggling off, ordered sector crossings, recovery policy and excluded automated/mixed runs. Verify UI keyboard/pointer behavior natively and controller behavior only if hardware is actually available.

Compare an eligible manual lap with its replay on the same build/rules. Automated replay fixtures can validate synchronization but cannot be presented as human best-lap evidence. If manual input is unavailable, deliver the fixture evidence plus the precise owner check still needed.

## Delivery

Write `reports/04-replay-presentation.md` in the shared packet: workspace/baseline, scoped commits, data/API compatibility contract, player-visible improvements, screenshots and audiovisual evidence, tests, unresolved defects and Integration's exact wiring steps.

Honor the heavy-process lease. Do not spawn workers, buy audio/assets, publish, merge main or claim playtest results. Deliver the replay feature as a hypothesis to validate in the five-person playtest, not proof of retention.
