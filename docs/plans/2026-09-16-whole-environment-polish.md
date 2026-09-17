# Whole-environment polish plan

Status: owner authorized staged implementation. Current milestone: Stage 1 structural candidate, correcting the coaster-like viaduct before tunnel/signage/finish work. Preserve the baseline and verify native views before advancing. Earlier planning-only statements below describe the original document delivery, not the subsequent implementation authorization.
Date: 2026-09-16 Pacific.

## Intended result

A complete lap that feels like one designed place: a convincing city opening, a gradually changing city edge, partly underground tunnels, quieter open surroundings, and a coherent return to the start. Density should rise and fall deliberately. The first approximately ten seconds are the quality benchmark, not the only finished section.

The owner identifies five priorities:
1. The left side becomes visually weak after the opening.
2. Covered “hood” sections should feel partly underground, not like exposed galleries.
3. Sparse or empty surroundings are acceptable when the approach, departure, and composition are polished.
4. Opening billboards repeat in shape, artwork and mounting height.
5. Exposed road spans on forests of thin posts read as a roller coaster, not believable urban infrastructure.

Environment is the next focus. Handling, AI balance, vehicle redesign, camera redesign and HUD redesign remain separate work.

## Baseline and interpretation

Use the current integration WIP and Stage1HybridRoute scene, with the fresh reviewed build GUID `578f19f86d7d4144b1de6e052ce196c2` as the native comparison baseline. The fresh run passed 253 tests and a complete six-finisher race. Its 1080p frame times on M2 Max were 18.19 ms mean, 25.19 ms P95 and 25.68 ms P99; do not substitute earlier, faster reports.

The current world imports canyon/cool-gallery, thermal-works, station/civic and distant-city districts. Its runtime district switch blacks out the view near normalized progress 0.3228 and 0.975. These are implementation boundaries, not acceptable final environmental transitions.

The owner's approximate “ten seconds” locates a perceived quality drop; it is not a trigger time. Map all planned sections to course distance/progress and camera views during the first phase.

Preserve the existing course spline, banking, driving width, collision surface, vehicle, camera and HUD. Achieve the impression of partial underground travel by raising retaining terrain, structural slabs and urban foundations around the existing road. Do not require a new descent in the road geometry. If the fixed course cannot support a convincing section, record that specific limitation before proposing a route change.

Historical opening-protection rules remain applicable to the archived baseline. This proposed candidate deliberately revisits opening signage and section joins in response to the owner's latest direction. Preserve the successful opening's overall density, night identity and driving readability; pixel-identical opening screenshots are not a goal for the new candidate.

## Screenshot-led work list: the current game, not an imagined replacement

These are native chase-camera screenshots from the current reviewed build, not generated concepts. Times below are the visible race HUD times in this automated capture; they are navigation aids, not implementation triggers. Progress values identify capture anchors, not surveyed section boundaries. Automated steering is useful for matching views but is not a human handling assessment. The linked frames establish composition; continuous footage and later manual driving must establish transition quality and readability in motion.

| Current view | What is actually visible | Targeted change and what to preserve |
| --- | --- | --- |
| [02 — 7.090 s, opening](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-02.png), anchor .15 | Close towers and the banked bend already make a strong city corridor. Prominent ads repeat tall rectangular silhouettes and bright symbol-led graphics at similar facade levels. A rival occupies the right foreground. | Keep tower enclosure and rival visibility. Recompose the existing prominent signs into a small hierarchy: one hero, lower landscape/fascia signage, a narrow secondary sign, and unsigned facades. Do not replace this successful corridor with a new skyline. |
| [03 — 11.980 s](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-03.png), anchor .28 | The left foreground is a large dark, banded building face; the middle and right still have substantial skyline density. | Break up this specific near wall with a setback/base and one readable service or structural feature. This is not evidence that the entire city disappears after ten seconds. Keep the existing distant towers. |
| [04 — 13.580 s](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-04.png), anchor .32 | Banking reveals a field of similar low rooftops on the left and a distant elevated track carried by many thin posts. Buildings on the right remain dense. | Compose the exposed lower-left district into a few connected podium masses and a credible viaduct/retaining structure. Use this view to establish the transition toward enclosure; do not simply add more tall buildings. Keep the upcoming bend and opponent unobscured. |
| [05 — 18.080 s, first covered bend](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-05.png), anchor .38 | Gray ribs and cross-members dominate; exterior tower windows remain visible through repeated side openings. The ship is close to the inside left rail at 99 km/h. | Make the passage a solid cut-and-cover space around the existing road: fill most side bays, add a substantial roof shell, retain a few deliberate light wells. Keep all new thickness outside the driving and chase-camera envelope, especially the inside bend. This screenshot does not establish why the ship is close to the rail. |
| [06 — 22.880 s](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-06.png), anchor .49 | Repeated flat rooftop blocks and another broad blank near-left wall are exposed. Three industrial cylinders ahead already provide a distinct landmark. | Keep the cylinders as the destination cue. Connect the low blocks into a service district with shared bases, fewer repeated roof silhouettes and a restrained wall treatment. Carry tunnel-exit construction into this district; no additional hero tower needed. |
| [08 — 27.340 s, quieter crest](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-08.png), anchor .61 | A large nearly featureless left mass, separated towers and considerable open sky. The road crest is the main event. | Preserve the open sky and quieter rhythm. Articulate the left mass with a base, setback and restrained warm recess; use only enough midground to explain its placement. The earlier generated skyline image is not permission to fill the gaps. |
| [09 — 31.250 s](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-09.png), anchor .70 | Dense lit facades return on the left while plain dark towers and open sky sit to the right. | Preserve this asymmetry and the release toward open space. Improve the right-side forms' grounding and transition into the next portal, not uniform density on both sides. |
| [10 — 35.250 s, second entrance](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-10.png), anchor .76 | The gallery stands openly ahead with a thin roof edge. Adjacent roofing and long support posts are exposed; a dark tower on the right appears disconnected from the ground in this view. The HUD reports sustained contact. | Highest-priority underground-read blockout: build a retaining/urban foundation mass around the approach, bury the roof edge into that mass, and give the entrance thickness and a visible inner lining. Resolve the apparently ungrounded tower silhouette. Verify collision separately; the HUD alone cannot attribute contact to scenery. |
| [11 — 39.149 s, second interior](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-11.png), anchor .845 | Repeated exposed gray roof framing, dark side panels and strong continuous rail emissions read as a covered gallery. | Extend the entrance's enclosure through the interior; retain selected ribs as secondary structure. Add restrained overhead practicals to reveal the ceiling and road, with the exit visible as a distinct release. Surface polish alone will not make this underground. |
| [12 — 43.008 s, return](../../../../../../Vector%20Rush%20Review%202/visual-goals/native-lap/zone-12.png), anchor .955 | A strong dense corridor returns, but numerous blue/red/orange ring advertisements repeat down both sides. The closest blue sign is very bright during boost. | Include this return view in the opening signage pass: break repeated silhouettes/content and balance the nearest sign's luminance. Keep the corridor's depth and boost readability. Validate the following .975 district boundary in footage; this still precedes it and cannot prove a seamless lap join. |

### What the source confirms

In `UnityProject/Assets/Editor/Production/Stage1CitySetup.cs`, `BuildCity` places advertisements on every other eligible near-layer building. Panels use only two portrait sizes (14 × 22 or 10 × 18), cycle four materials with `ads[(n/2)%4]`, and put their centres approximately 10–19.8 units above local track height. Both sides use the same index-based cycle. This supports the visible shape/content/height repetition: adding artwork alone will not fix the placement rhythm. Replace the coupled cycle with authored placements at the opening and return views, while retaining shared materials and geometry batching.

`HybridRouteTransition.cs` also explicitly fades to black around whole-district activation. A [retained black frame](../../../../../../Vector%20Rush%20Review%202/current-review/lap/transition-blackout.png) from the same reviewed build confirms this is visible, not just a source concern. Its 12.683 s capture-elapsed timestamp is from a separate 720p run and must not be equated with the 1080p HUD times above. Remove the visibility discontinuity through spatial layout/activation work, then remove the fade; neither screenshots nor code alone prove the resulting transition works in motion.

### Order dictated by these views

1. Block out the second entrance/interior (10–11) and the first covered bend (05), including their approaches and exits. These are the clearest mismatch with the intended underground setting.
2. Repair the exposed left-side composition (03–04 and 06), then the quiet crest (08). Reuse existing skyline and industrial landmarks; fix bases, silhouettes and connections first.
3. Make the district boundaries continuous and review the whole lap, including 09–12. Do not hide incomplete joins behind darkness.
4. Recompose the advertisements visible in 02 and 12, then finish surfaces and lighting. The numbers of kit variants below are provisional production bounds, not evidence that every format is necessary.

For each item, deliver a matched native before/after at the linked anchor plus approach/departure footage. Keep the same camera, resolution and comparable speed/boost state; do not obtain an apparent improvement by changing FOV, removing rivals or choosing a flattering editor angle. Generated references guide materials and structure only and never count as completion evidence.

## Route composition

This is a visual sequence to fit to the existing circuit, not a proposal to reorder its geometry.

| Section | Intended experience | Required environment work |
| --- | --- | --- |
| City opening | Dense, lively and legible | Keep the established city composition; vary billboard formats, content and placement. |
| City edge, especially left side | A designed change in density | Ground near structures; give the left foreground a sequence of podium, retaining wall/service frontage and a clear tunnel approach. Maintain middle-distance depth and deliberate gaps. |
| Tunnel approach | Gradual enclosure | Raise side walls, reduce sky visibility and introduce a structural portal, roof thickness and service details before the covered segment. |
| Partly underground passage | Sheltered, embedded, readable at speed | Solid wall/roof volumes, recessed fixtures, drainage/service bands and occasional believable openings. Surrounding terrain or urban mass explains what is above the roof. |
| Tunnel exit / open section | Release and breathing space | Reveal the open area through the exit; taper retaining structures and carry road materials and guidance through it. Use restrained terrain, foundations and a few distant forms. |
| Return to city / lap seam | Familiar space returns naturally | Bring back city silhouettes and signage progressively, with no blackout or sudden whole-district appearance. |

Audit both covered areas if multiple galleries exist; do not finish one and leave the other as a skeletal canopy. Retain district differences through proportions, wall treatment and light temperature within one construction language.

### Left-side composition rule

Do not fill every empty pixel. Establish a readable near/middle/far relationship from the chase camera: a grounded foreground edge, one useful middle-distance form and a subdued skyline. Place a small number of recognizable structures where the road reveals them. Replace blank slabs, exposed undersides and accidentally floating-looking masses. Judge this across the approach, apex and departure, not only from an elevated editor view.

### Road-to-city structure: remove the roller-coaster reading

The owner's annotated crop points directly at the distant exposed span also visible in native views 04 and 06. The visible combination is the problem: a thin illuminated road ribbon, numerous very tall slender posts, exposed air beneath most of the route, and little apparent connection to the surrounding low buildings. This is a visual diagnosis, not a structural engineering assessment. Changing concrete textures or adding more buildings in the distance will not resolve that silhouette.

Use three related treatments, selected by the actual chase-camera sightlines:

- **Exposed crossing → urban viaduct.** Add a continuous, visibly deep box-girder underside and substantial opaque parapets beneath the existing guidance strips. Replace the dense picket-like post rhythm with fewer broad piers and readable crossheads/bearing points. Piers remain vertical rather than following road banking; deck geometry follows the existing road. Choose spans by visual scale and clearance, not an arbitrary every-Nth-post deletion.
- **Road beside low blocks → shared podium.** Consolidate selected existing rooftop masses into a service/parking/industrial podium that visibly carries part of the road. Show a few recessed loading bays, vents or service openings to establish human/building scale. This changes the left-side silhouette without buying a new skyline or filling every gap. Do not merely push buildings through the deck: model an intentional top slab and road interface.
- **Approach to a hood → retaining structure.** Let the podium or retaining mass rise beside the route, then carry a roof over it. This connects the viaduct treatment to the partly underground passage, explaining where the enclosure comes from while keeping the driving spline unchanged.

For the exact span marked by the owner, first prototype a podium-supported section interrupted by one clear viaduct opening. Keep a visible portion of the underside and its supports so the road's construction is understandable; do not conceal the entire problem behind a foreground wall. Carry the support language into the approach visible at 35.250 s, where exposed posts currently reinforce the same coaster impression.

Keep the cyan/magenta navigation cues, but seat them in architectural parapets instead of letting emissive lines define the entire distant road silhouette. Any reduction in background bloom/brightness must preserve readable guidance on the road being driven. Fog can soften far structures after their silhouettes work; it is not the structural fix.

Implementation stays bounded: a small reusable deck/fascia kit, pier/crosshead kit, podium walls and portal/retaining pieces. Inspect how existing supports are generated before replacement, preserve racing collisions, and check all intersecting track levels, rivals, look-back and camera clearance. Start with untextured blockouts at native views 04, 06 and 10. Acceptance: the road has visible depth, supports meet both deck and credible bases, no forest of thin posts dominates those views, and city-to-enclosure connections remain legible through approach and departure. If the unchanged banking/crest still reads as a coaster after this pass, flag that specific view for owner review rather than silently changing handling or track geometry.

This is part of Phase 2's structural pass, not optional late decoration. The first before/after should show the owner's marked span with identical camera framing; do not count a fogged-out or occluded comparison as a solution.

### Tunnel enclosure and finish

The tunnel should read as partly buried even with emissive effects disabled. Require a substantial portal, continuous enclosing surfaces, visible structural depth and a credible mass above/alongside the route. Skylight cuts or service openings can retain partial exposure, but must not restore the open-sided pavilion appearance. Use warm practicals and cooler exits without crushing the road into black. Keep ship/rival silhouettes and turn guidance readable.

The previously generated warm-gallery image is useful for surface finish and light hierarchy only. It is not the structural target for this tunnel plan. Create a revised screenshot-anchored tunnel reference showing actual enclosure before detailing.

### Billboard system

Use deliberate art direction rather than randomized placement alone. Proposed initial kit:
- Four mounting formats: facade-integrated landscape panel, narrow vertical banner, small projecting sign, and occasional long podium fascia.
- At least six distinct original graphic layouts, mixing typography-led, symbol-led and product/service-like compositions; avoid recoloring the same circular symbol.
- Three useful mounting bands: podium, middle facade, and occasional high landmark. Align each sign to credible building structure.
- A few dominant signs and many quieter or unadvertised surfaces. Include visual rest between hero signs.

Reuse atlases and shared frame materials. At the three opening anchors, no identical hero artwork should repeat in the same view; the closest prominent signs should differ in shape, height and graphic composition. Remove signs when spacing and hierarchy benefit more than additional variety. Test small-text readability at driving speed; major shapes and color blocks matter more than fine copy. Keep advertisements distinct from navigational arrows.

### Continuous transitions

Prefer one continuously present environment with ordinary culling/LODs where budgets allow. If district activation remains necessary, split it into spatial chunks, warm required resources, activate ahead of visibility and retire only after they leave the camera view. Check reverse/look-back views, recovery, restart and lap wrap—not only forward autopilot.

Hide unavoidable visibility boundaries behind actual portals, bends or terrain, not a full-screen fade. Blend materials, lighting and density across overlap areas. Establish transition lengths in metres using native approach footage; no fixed second-based swap. Removing the blackout alone is insufficient if it reveals abrupt asset popping.

## Implementation sequence

| Phase | Deliverable | Completion check |
| --- | --- | --- |
| 1. Whole-lap layout | Expand the screenshot anchors above into actual start/end progress ranges and scene-object inventory; block out 05 and 10–11 and left-side repairs at 03–04/06/08; revised tunnel reference | Each screenshot finding has a keep/rework/replace decision and concrete scene target. Both district switches and intentional open areas are accounted for. |
| 2. Continuous structural pass | Separate candidate scene with grounded city edge, enclosed tunnel volumes, exit treatment and return connection | A native complete lap has no blackouts, visible district popping, accidental voids or road/camera clearance violations. No detailed polish before this works. |
| 3. Opening signage | Small reusable sign kit and individually composed opening placements | Matched opening views demonstrate variation in shape, content and height without new clutter or loss of route readability. |
| 4. Environment finish | Shared surface kit, completed tunnel treatment, selective lighting and coherent open-area depth across the full lap | The opening, left city edge, tunnel interiors, exits and open stretches share a consistent finish. Native motion demonstrates the result. |
| 5. Integrated verification | Final playable candidate, compact full-lap footage, representative before/after pairs, test and performance results | All acceptance conditions below pass or are explicitly recorded as unfinished. Owner visual acceptance remains separate from technical checks. |

Phase 1 must cover the whole environment. A polished tunnel alone does not complete this request. Phase 2 is the first meaningful native milestone; later passes should be tested as part of that continuous route.

## Feasible asset and rendering approach

Reuse current buildings and structural ribs where their silhouettes work. Author a bounded kit of wall/roof panels, portal segments, retaining walls, podium/base modules, vents and sign mounts. Use trim sheets and shared concrete/metal/asphalt materials with controlled roughness. Group window occupancy in coherent regions rather than repeating bright strips.

Prefer baked/static environment lighting, emissive artwork and a bounded set of local lights for moving vehicles. Validate light probes and reflection continuity where districts toggle. Use simple depth fog and low-detail distant meshes. No real-time traffic, crowds, weather, ray tracing or large library of unique skyscrapers is required.

Keep the active project's cache. For each milestone retain one current candidate, one baseline, selected native stills, a compressed clip and concise test/performance summaries; avoid retaining every raw-frame sequence and intermediate build.

## Acceptance and validation

- **Continuity:** inspect a continuous complete lap, both directions of the lap seam, tunnel entry/exit, look-back and recovery at transition boundaries. No blackout, abrupt exposure flash, visible spawn or unexplained disappearance.
- **Composition:** matched chase-camera views cover opening, the weak left side after it, every tunnel entrance/interior/exit, open stretch and return. Quiet sections remain sparse but grounded; they are not forced into dense city scenes.
- **Tunnel reading:** ordinary gameplay views clearly show embedded enclosed space rather than a roof standing over an exposed elevated road.
- **Billboards:** shape/content/height variation and a clear dominant/secondary hierarchy are visible in native opening views and motion.
- **Gameplay preservation:** course geometry/hash and racing surface remain unchanged; verify obstacle/camera clearance with rivals and look-back. Run relevant structural tests and the existing suite, then three consecutive complete races including pause/restart. Record the prior intermittent timeout honestly if it recurs.
- **Performance:** first establish repeatable baseline/candidate measurements on the same Mac, resolution, foreground state, cap and warmed conditions without editor/capture/encoding overlap. Environment work must not worsen frame-time tails beyond measured baseline variation. Working quality target is stable 60 fps at 1080p, ideally P95 at or below 16.7 ms; this is not achieved by the current sample and is not promised by the art pass. If required optimization exceeds environment scope, report it separately rather than declaring a performance pass.
- **Presentation evidence:** do not infer motion or gameplay acceptance from stills, compilation or automated steering. Include continuous native footage and a human driving review before final sign-off.

## First implementation packet when work begins

Create an isolated candidate from the current WIP, preserve the baseline scene and all existing source metadata, and record the starting source/build identities. Reuse the native baseline shots linked above; capture additional approach/exit views only where they resolve an unanswered layout question. Map the visible structures to exact scene objects before editing: screenshot labels are location identifiers, not verified object names. Deliver the route map and whole-lap structural blockout before expanding asset production. This document itself makes no implementation changes.

## Supporting references

- [Fresh game review](../../../../../../Vector%20Rush%20Review%202/current-review/REVIEW.md)
- [Current/goal comparisons](../../../../../../Vector%20Rush%20Review%202/visual-goals/comparison.html)
- [Previous hybrid implementation and limitations](../hybrid-route-review.md)

The owner's tunnel and sparse-area clarification in this plan takes precedence over the earlier generated gallery/skyline images. Those images are suggestions, not approved final targets.
