# Vector Rush — first playable

An original anti-gravity racer pursuing the speed, clarity, and finish of Wipeout Omega Collection. The first deliverable is a complete three-lap race on Solstice Circuit, an elevated coastal course, with one authored craft and five rivals. AAA is a quality target, never a claim based on concept art.

## Visual direction
Sunlit ivory infrastructure, deep ocean blue, graphite track, restrained acid-yellow racing identifiers, turquoise engine glow. Strong silhouette, physically meaningful material variation, industrial details and open ocean vistas. Hero craft: slender twin pontoons framing a recessed cockpit, swept stabilizers and exposed rear propulsion. Chase camera readable at speed. No copied logos or game assets.

## Technology and interfaces
Unity 6.3 LTS (exact patch follows user install), URP 17.3, C#, Apple Silicon macOS. Blender 5.2 LTS sources and reproducible FBX exports. One Unity unit = one metre; +Y up, +Z craft forward. Namespace VectorRush. Core uses legacy Input with activeInputHandler=Both initially; gamepad via InputSystem if available. Shared settings, scenes, editor code and integration owned by parent.

TrackPath is authoritative for track geometry. TrackFrame has Position, Forward, Right, Up. TrackPath.Evaluate(float normalized) returns TrackFrame; ClosestProgress(Vector3) returns [0,1). Width=22m. Runtime mesh includes collision surface and walls. Vehicle simulation owns speed, steering and suspension. Race progress must use ordered sectors and forward crossings, not raw wraparound.

Gameplay publishes RaceDirector.Instance, Phase (RacePhase.Menu/Countdown/Racing/Paused/Finished), Player (HoverVehicle), Racers (List<HoverVehicle>), CountdownRemaining, RaceTime, Lap, Position, BestLap, LastLap, TotalLaps=3, StartRace(), RestartRace(), TogglePause(). HoverVehicle exposes SpeedKph, Boost01, IsBoosting, IsPlayer, RaceProgress, DisplayName. Integration initializes track and vehicles before race director.

## Gameplay
Fixed-step hover suspension, acceleration, braking, airbrakes, slip, boost, collisions and recovery. Six racers follow an actual closed circuit. Start, countdown, three laps, results, pause, restart and quit. Keyboard and gamepad, camera shake toggle, controls shown in menus. Order/checkpoint validation prevents reverse laps or respawn shortcuts.

## Verification
Meaningful race-rule tests, actual Editor compile, complete race and repeated restart, runtime screenshots and video. Profile standalone at 1920x1080 targeting 60fps after warmup; report frame time percentiles and actual limits. Independent critic inspects runtime evidence and authored-asset renders separately, identifies concrete defects, and reviews fixes. Up to three substantial visual cycles; unresolved failures remain visible. If Editor/license is unavailable, complete independent work and report runtime validation blocked explicitly.
