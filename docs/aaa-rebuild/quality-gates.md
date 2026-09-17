# Whole-game quality gates

The owner targets Wipeout Omega Collection level finish. That is the benchmark for work and review, not a completion claim. No single numerical score or automated test certifies AAA quality. All gates below remain open unless separately evidenced on the current build.

| Area | Evidence required for adoption | Rejection trigger |
|---|---|---|
| Whole image | Original and candidate native views, full-lap sequence, explicit visual critic findings against references | Lost original composition, weak foreground, unreadable course, generic massing, inconsistent zone quality |
| Architecture | Native near/middle/far views and exact route clearance, source meshes/UVs/LODs | Floating joins, visibly crude forms, repetition, facades outside useful camera views, corridor intersections |
| Materials and lighting | Actual mapped materials under race lighting, relevant native buffers/variants, exposure/fixture coherence | Flattened texture response, missing maps, painted light effects, excess emission, unstable surface response |
| Craft and VFX | Native close and race-distance views, crowd/boost/contact sequence | Identity drift, detached effects, visual noise hiding traffic or road, clipping |
| Handling and competition | Human races from different grid positions, wall/airbrake/recovery tests, measurable competition | Unpredictable control, recovery into traffic, unexplained assistance, stalled or trivial opposition |
| Race rules and persistence | Regression tests plus native menu/countdown/pause/finish/restart/settings/records flows | Duplicate activation, incorrect results, lost records, inconsistent input ownership |
| Audio | Continuous native listening and recorded actual game mix | Inaudible cues, harsh loops, mix masking, music-only preview that conceals gameplay sound |
| UI/accessibility | Keyboard, pointer and physical controller traversal across all actions; aspect tests | Trapped focus, stale controls, same press activates two screens, settings not retained |
| Performance | Exact current build, warmed isolated frame delivery/memory sample, crowded sectors | Unreported spikes, capture-time sample passed off as performance, regressions hidden by averages |

## Iteration rule

An implementation worker produces a bounded revision and its self-check evidence. A different critic reviews the relevant actual output, labels failures with source/view references, and withholds acceptance until corrected. Fix the highest-impact demonstrated failure, preserve a separate corrective commit, regenerate current evidence, and re-review. A repeated negligible improvement requires a new production hypothesis. Do not replicate an inadequate exemplar.

Whole-course rollout and complete experience remain required after the exemplar. Missing image perception, missing human play or unobserved sound cannot be converted into a pass by additional source work. Visual work stays staged while image capability is unavailable. Original art is the control; the owner-rejected direct-production scene is retained only as failed history and a correctness-test fixture.
