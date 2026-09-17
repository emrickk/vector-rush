# Handoff 03: racing feel and visible competition

Use **GPT-5.6 Sol**. The user authorized executing the racing lane for Vector Rush: one polished, competitive race on the existing Nocturne Circuit.

Shared packet: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush/docs/plans/2026-09-14-playable-race`.
Original project: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush`.

Read the packet's `README.md`, `coordination.md`, `contracts.md`, workspace `AGENTS.md`, `docs/aaa-rebuild/gameplay-critic-002.md`, and `docs/pace-reviews/002-native-pace-comparison.md`. Work only in your assigned isolated workspace. If setup is missing, do read-only diagnosis and report the dependency.

## Problem to solve

The earlier pacing evidence kept rivals close but behind the automated player and demonstrated no overtake. That is evidence of a testing/experience gap, not proof that all human races behave the same. Your job is to make normal racing support readable opponents ahead, useful passing opportunities, pressure from behind and recoverable mistakes.

Begin by tracing current `HoverVehicle`, `RaceDirector`, `RaceProgress`, `RaceFinishLifecycle`, `RaceFinishLedger`, `ChaseCamera` and actual initialization/tuning. Do not confuse class defaults with runtime speed settings. Verify historical defects against current active code before fixing them.

## Bounded execution

1. Record baseline race/input/camera behavior and current tuning. Identify the smallest changes justified by source and native evidence.
2. Keep hover physics and existing boost/airbrake vocabulary. Tune responsiveness, turn anticipation, camera readability and recovery only where evidence supports a defect. Do not redesign the whole physics model.
3. Improve AI's ability to occupy reachable positions ahead and contest a pass through normal physics. Prefer sensible grid, pace, lane choice and braking over hidden teleportation or forced overtakes. If any pace assistance is introduced, document bounds and fairness.
4. Create controlled cases for clean passing, side-by-side traffic, wall contact/escape, player mistake and rejoining. Label setup automation. Then run varied full races so one canned scenario does not become a broad quality claim.
5. Validate ordered finish timing, continued rival simulation, pause/resume, restart, recovery input and record eligibility. Distinguish the active lifecycle from the legacy ledger convenience API.

Do not change the course layout, visual art, HUD or audio. Request needed telemetry/feedback hooks through Integration.

## Ownership and Replay contract

Own `UnityProject/Assets/Scripts/Gameplay/` except `RaceRecords.cs`, plus `PaceEvidence.cs` and lane-specific gameplay tests. Existing shared tests are Integration-owned; add distinct `RacingPolish...` tests or submit a scoped request.

You own camera behavior; Replay/presentation owns effects/audio and must not edit it. Keep an unchanged camera configuration available for Environment's matched art comparisons.

Provide minimal authoritative values/hooks for lap time, ordered progress, pause/restart, recovery/invalid-lap status and manual-run eligibility, following `contracts.md`. Do not build an elaborate event system. Identify any change to driving rules so records/ghost compatibility can be revised. Integration handles shared bootstrap and settings wiring.

## Evidence and acceptance

- Three complete native automated races with varied defined starting conditions, correct standings and functional restart, with recoveries/contact behavior explicitly reported.
- Recorded examples of an actual player pass and a rival pass/response opportunity; normal-play captures distinguished from engineered diagnostics.
- Proximity data separated from ahead/behind position, camera visibility and actual rank changes. A nearby rival behind the camera is not a demonstrated racing moment.
- Steering, airbraking, boost, recovery and camera checks described with exact build/configuration.
- No subjective “feels great” claim from automation. Prepare a short human driving checklist; owner/participant play remains required.

Do not optimize to arbitrary historical distance thresholds or manufacture passes to satisfy the report. If repeated testing shows competition requires a larger design change, explain the evidence and propose one bounded experiment.

## Delivery

Write `reports/03-racing.md` in the shared packet: workspace/baseline, scoped commits and tuning changes, observed behavior, native evidence, tests, unresolved issues, minimal Replay API contract and shared-file integration requests.

Honor the heavy-process lease. Do not spawn workers, edit another lane, publish or merge main. Hand off an integrated, testable candidate; leave unsupported human/hardware criteria explicit.
