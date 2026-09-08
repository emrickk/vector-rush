# Environment review 017: coastal versus current, independent comparison

Independent review, 2026-09-08. **The user's impression is supported by these captures: the earlier bright coastal version has a stronger overall visual identity and clearer scene construction.** The current night version contains better individual features, but its broad composition, material readability and sense of place are weaker. More detailed pieces have not produced a more convincing whole. Coastal still shows simple water, coarse cliff shapes and repetitive towers; its advantage is composition and legibility.

## What was actually compared

Directly inspected ten original 1920 × 1080 native game images:

| Version | Originals inspected |
| --- | --- |
| Bright coastal, `run-09-coastal` | [02-start](../../evidence/run-09-coastal/02-start.png), [03-race](../../evidence/run-09-coastal/03-race.png), [05-crest](../../evidence/run-09-coastal/05-crest.png), [06-city-descent](../../evidence/run-09-coastal/06-city-descent.png) |
| Current final lighting, candidate sequence | [148, mast](../../evidence/lighting-depth-candidate-01/selected/frame-0148.png), [360, gallery approach](../../evidence/lighting-depth-candidate-01/selected/frame-0360.png), [455, gallery interior](../../evidence/lighting-depth-candidate-01/selected/frame-0455.png), [606, thermal plant](../../evidence/lighting-depth-candidate-01/selected/frame-0606.png) |
| Current final lighting, separate native race | [05-crest](../../evidence/lighting-depth-final/performance/05-crest.png), [06-city-descent](../../evidence/lighting-depth-final/performance/06-city-descent.png) |

The [final build manifest](../../evidence/lighting-depth-final/build-manifest.json) identifies the same build GUID as the candidate sequence: `5c49b92643bd4d41820ba81857fdf8c7`. The four selected candidate images were verified byte-identical to their original sequence frames. The dedicated coastal cliff inspection, title screens and generated concepts were excluded. This compares runtime against runtime.

The crest and descent pairs show comparable route situations, not identical camera poses. Their displayed speeds are 187 versus 177 km/h at the crest and 109 versus 111 km/h at the descent. The other samples cover different moments; the current gallery approach includes a low-speed wall-contact situation and should not represent ordinary race pace. These stills cannot establish motion quality, actual world-scale changes, responsiveness, perceived speed or audio quality. No code, assets, build or Unity state was changed for this review.

## Six reasons the current version can feel less finished

### 1. Broad light and material separation have been lost

The coastal crest separates pale structural concrete, dark road and glazing, brown rock, blue water and sky into readable areas. Light and shadow explain the ship's cowlings and the buildings' corners. In the current crest, many building faces, foundations and supports occupy nearly the same dark range. Small luminous elements carry much of the visible information.

This reduces the apparent depth and material quality of geometry that may still exist. **Priority: restore readable large surfaces, directional shading and separation between near, middle and distant scenery.** Simply increasing every light or adding bloom would not establish that hierarchy.

### 2. The world is less visibly grounded

In the coastal start and crest, the ocean plane, building podiums and bridge footings show where structures meet their surroundings. The track visibly crosses a place. In the current crest and mast view, supports and buildings descend into a largely black field punctuated by thin amber lines. Their relationships become harder to understand, so the scenery can resemble separate objects surrounding a road.

**Priority: make the city's ground, lower building masses and track connections legible as one environment.** The relevant loss is visible spatial continuity; adding more isolated buildings would not address it.

### 3. The strongest identity cues were replaced by more generic forms

The white arch, maritime horizon, cliffs and pale infrastructure give the coastal version a recognizable setting even before reading the HUD. The current skyline is dominated by dark towers and repeated window rectangles. The mast and thermal plant introduce variety, but they do not yet provide an equally strong identity across the whole view.

Night is a valid direction, but these pictures show a loss of distinctive scenery, not merely a time-of-day preference. **Priority: decide which large silhouettes, setting and material palette define the game, then judge the entire race composition against that direction.**

### 4. Repeated small bright elements compete with the main forms

Current lamps, rail strips, window grids and ground lines recur at similar visual strength. At the crest and gallery approach, they describe many disconnected edges while the larger masses remain subdued. The route is navigable, but the picture feels busier and more visibly assembled from repeated units. The coastal version also repeats assets, yet broad white structures and quiet sky/water keep their hierarchy clearer.

**Priority: give a few architectural forms dominance and leave quieter areas around them.** Judge light placement and repetition as part of the whole composition before increasing detail density.

### 5. The player craft reads as broader and less mechanically articulated

The coastal craft visibly separates intake openings, layered engine cowls, metal nozzle shells, fins and open structural gaps. The current craft presents larger continuous pale pod surfaces and a dark central cockpit. Its markings and engine interiors are useful improvements, but the broad surfaces can read as soft casing rather than a tightly engineered racing machine.

Lighting, shape and viewpoint all contribute; these images do not isolate a geometry-only regression or prove a physical size change. **Priority: compare the two designs under shared lighting and camera conditions before choosing between stronger surface shading and silhouette changes.** The current craft's focal prominence makes this consequential.

### 6. Local polish is surrounded by much simpler scenery

The gallery at 455 has coherent framing and light response. The thermal plant at 606 has readable curved metal and pipe connections. Those are real gains. Outside such moments, large smooth road panels, dark flat facades and repetitive window fields occupy much of the picture. The contrast in finish makes the improved pieces feel localized rather than lifting the entire environment.

**Priority: raise the consistency of ordinary race views before adding another showcase object.** The current HUD's compact timing, combined speed/boost cluster and actual circuit map should be retained as gains; restoring the coastal version's strengths does not require reverting every later improvement.

## Review implication

[Review 016](016-lighting-material-native-critique.md) accepted a bounded lighting improvement against the preceding night build. It did not demonstrate that the night direction surpassed the earlier coastal version. This wider comparison exposes that missing criterion. Future visual acceptance should include the broad race composition, representative ordinary views and comparison with the strongest earlier runtime reference, alongside checks of individual changes.

The first priorities are overall value structure and spatial grounding, followed by setting identity and the hero craft's material/shape read. More texture detail and additional isolated assets are lower priorities. This review proposes an order for discussion; no redesign was implemented.
