# Meridian opening candidate

Baseline: integrated `e243303`, r4 native GUID `f8e1fcbc54e74d04bd9ffa8a05be1237`. The original and r4 apps remain available. Work lives in isolated branch `art/opening-city`. Gallery lighting and Replay boundary corrections are pending upstream dependencies; adopt their completed commits without changing their ownership.

## Passage fixed before editing

One continuous 18 simulation-second first-lap passage, starting at the first forward crossing of progress `.005` after race start and ending 18 seconds later. At baseline pace this includes approach `.15`, bend `.22`, and reveal `.30`, before the first gallery. Stills use those three progress anchors; comparisons verify the recorded native camera/racer poses without injecting transforms. Motion uses the ordinary ChaseCamera and physics with explicitly labeled test autopilot, not human driving. Normal launch remains manual.

## Native assessment and design

Inspected current r4 native opening frames from `artifacts/integration/gallery-control-02` (GUID verified in the capture report), the supplied original image, concept and critic-002. Three visible weaknesses: disconnected and thin viaduct construction above black ground; a crude pale mast dominating the bend; isolated generic boxes with no deliberate middle-distance group.

Build connected bearing/cap/pier assemblies below the unchanged road, a terraced transit hall on the inside of the bend, and one asymmetrical Meridian Exchange tower group outside it. Retain the elevated ribbon, clear sky gaps, distant haze, night palette and cyan navigation hierarchy. Replace competing weak assets in reserved new footprints. Use recessed glazing, substantial floor/corner returns, limited occupied rooms and local cool/amber illumination. No track, collider, craft, camera, HUD, handling or race edits.

## Bounded steps

1. Record checkpoint, native assessment and passage; commit.
2. Build large architecture and infrastructure with reproducible sources. Produce an early native view; revise the composition if the change is weak.
3. Finish materials/local lighting, verify clearance and moving views, incorporate completed upstream corrections.
4. Deliver named playable app with opening enabled normally, three matched native A/B views and continuous 18-second A/B clips. Record checks and limitations. Visual acceptance belongs to the owner.

Source history stays local while the authorized-private-remote requirement is unresolved. Generated frames/builds stay outside source control.

## Early native check

`early-02`, GUID `535eac3073334c9db87bc0b965d94fcb`, built and launched successfully. All four FBX bounds passed metre/axis checks. Before/after natural camera, FOV, rigidbody and visual poses match exactly at all 432 frames. Three natural anchors were reached at 7.08, 9.66 and 12.66 race seconds; the fixed passage runs from 1.62 to 19.58 seconds (432 frames at 24 Hz). Reject optional `*-matched.png` captures: a paused diagnostic lost the craft. That path was removed; natural PNGs are valid.

Art verdict: visibly stronger altitude cues and architecture, but not ready. Move the landmark farther ahead so it stays visible at the bend; connect the V struts to actual piers; soften the competing lower-route brightness; strengthen the lower reveal cluster while retaining the Thermal Works in the distance. These corrections are now in source awaiting the next native check. No visual acceptance claimed.
