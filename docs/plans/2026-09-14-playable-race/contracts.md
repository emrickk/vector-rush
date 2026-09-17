# Racing and replay contract

Status: initial contract at baseline `1762224deed3c9e994483f5307999b68ae386e38`.

This contract keeps race rules in Gameplay and lets Replay observe them. It adds only the values needed to prevent competing clocks, fabricated progress and incompatible saved data.

## Authoritative state

- `RaceDirector` and `RaceProgress` own race phase, ordered checkpoints, lap completion, rank and finish adjudication.
- `RaceFinishLifecycle.SimulationTime` is the monotonic race clock. It advances only through fixed simulation steps while racing or while unresolved rivals finish. `RaceDirector.RaceTime` and `FinishTime` are presentation values and freeze at the player's finish.
- A lap crossing uses the existing fixed-step interpolation: step start plus `RaceProgress.LastCrossingFraction * Time.fixedDeltaTime`. Sector, lap, record and ghost timestamps must use that same clock and interpolation rule.
- `RaceProgress.Distance` is authoritative ordered progress for ranking. `HoverVehicle.TrackProgress` and `RaceProgress.LastProgress` are wrapped geometric samples for playback and visuals, not proof of an ordered lap.
- `RaceFinishLifecycle` is the active finish authority. `RaceFinishLedger` is legacy compatibility code and must not become a second clock or lifecycle.

## Compatibility identity

Every record, sector comparison and ghost is keyed by all three values:

1. course identity passed to `RaceDirector.Initialize`; production uses the canonical current course hash, and the legacy source uses its explicit fallback identity;
2. `RaceDirector.DrivingRulesId`;
3. the replay recording schema version.

Racing reports every physics, assistance, progress or recovery change that can alter a lap. Integration performs one driving-rules revision bump before Replay's final connection. Replay rejects a missing or mismatched identity and never silently compares incompatible data.

## Eligibility and recovery

- Manual-run eligibility is sticky from `StartRace` to player finish. If player autopilot, evidence/test driving or another automated player-control path is active at any point, the entire run is ineligible for records and ghosts. Switching automation off later does not restore eligibility.
- Ordered forward gate completion is required for every lap.
- A recovery invalidates the current lap for best-lap, ghost and sector-comparison purposes because it teleports the craft. The next valid start/finish crossing begins a fresh lap. A fully manual race may still set a best-race time after recovery.
- Reverse motion, invalid gate samples, discontinuities and respawn re-entry never create sector or finish crossings.
- Corrupt, incomplete or unwritable persistence never interrupts the race and never shows a successful save.

## Minimal Gameplay surface for Replay

Racing may name the members idiomatically, but the delivered semantics must be no larger than this surface:

- Read-only course identity and driving-rules identity.
- Read-only monotonic simulation time, current lap-local time, current ordered progress and wrapped track progress.
- Read-only sticky manual-run eligibility and current-lap eligibility.
- Existing `RaceRestarted` notification.
- One authoritative ordered-crossing notification containing lap index, boundary index, normalized boundary (`1/3`, `2/3` or finish), crossing simulation time, lap-local crossing time and lap eligibility.
- Existing phase can be polled. Ghost playback keys directly to the authoritative clock, so a separate pause timer or large phase event system is unnecessary.

The three comparison sectors are checkpoints 4, 8 and 12 of the existing 12-checkpoint ordered lap. The finish boundary is also the lap rollover. Events fire once, in order, only after Gameplay accepts the crossing.

Replay samples the existing player transform and wrapped progress at a bounded rate. It must not write vehicle transforms, progress, timing, phase, rank or eligibility.

## Replay behavior

- A ghost recording contains bounded time, position, rotation and progress samples plus the compatibility identity. Replay owns file schema, interpolation and storage limits.
- The ghost is visual only: no collider, gameplay rigidbody, AI identity, ranking entry, audio source or record participation.
- Playback time is lap-local authoritative time. It freezes automatically when the simulation clock stops, resets on `RaceRestarted`, and hides after playback ends or when no compatible valid manual lap exists.
- Sector deltas compare the current eligible lap with the compatible saved eligible lap. On an invalid current lap, presentation shows the invalid state rather than a synthetic faster split.
- Results and retry UI observe Gameplay's finish record and existing restart command. They do not submit an independent time.

## Current baseline gaps

The baseline already exposes phase, simulation time, finish records, `RaceRestarted`, ordered progress, wrapped progress, player transform and record compatibility inputs. It does not yet expose course identity, sticky manual eligibility, lap eligibility, lap-local time or ordered sector crossings. Racing owns those minimal hooks. Replay builds storage and visual components against this contract first, then requests only the shared bootstrap wiring it cannot own.
