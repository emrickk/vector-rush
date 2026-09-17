# Vector Rush / Meridian Afterhours

Art proposal and production contract. Baseline d586986; shared contract 1.
Native acceptance is pending. Author: GPT-6 Astra, task 02.

## The image

An elevated blue racing line cuts through an occupied, vertically layered city.
Large, asymmetric silhouettes establish place; deep podiums and repeated short
guidance modules establish speed. Keep a readable dark road and a recognisable
solid craft through the strongest boost moment.

Use charcoal blue structure, mineral cladding and recessed glazing. Cyan belongs
to route guidance and ship energy, cobalt to the dispersing exhaust, magenta to
occasional commercial signs, and amber to occupied interiors and contact sparks.
Small white cores are the brightest shapes. Large surfaces never become white
emission panels. Limit signage to one focal message per group of buildings.

## Audit and decisions

| Existing asset | Decision | Reason and concrete use |
| --- | --- | --- |
| HeroShip / Kestrel07 v3 and engine anchors | Retain form; rework finish/state | Distinct twin nacelles read from the chase camera. Preserve exact geometry, UVs and nozzle alignment. Use darker satin mineral paint, retained race number, cyan energy seams and an opaque boost treatment. |
| MeridianExchange | Demote | Existing source has unequal towers and sky hall, but its repeated floor returns dominate the older native views. Use only outside the new foreground composition, subject to 01 assembly. |
| RiversideOffices | Replace in exemplar foreground | Broad commercial family needs a projecting occupied head and deep service core, rather than more repeated window strips. |
| TransitTerraces | Rework through new kit | Retain the terraced idea; deliver asymmetric receding terraces with separate mechanical and circulation masses. |
| CanalWorks | Demote | Low industrial mass is useful at distance. New near industrial kit has a sawtooth roof and exposed structural rhythm. |
| Solstice / Nocturne terrace and split towers | Demote | Repeated background silhouettes in inspected previous native contact sheet. Do not treat recolouring as a new family. |
| Transit station / workshop / buttress sources | Retain as reference and reserve | Credible supports and folded canopies. New compact modules use the same construction discipline with fewer material partitions. |
| Gallery02 modules | Reserve outside open corridor | Enclosure would obscure the new skyline; do not populate this route with a gallery by default. |
| BlueFlameAtlas | Replace for new plume | Existing narrow, beaded appearance in native stills. New periodic fields preserve temporal continuity and support a broad body plus separate core and wake. |
| Barlow / Barlow Condensed | Retain for small text / secondary labels | Existing OFL source available. Small text remains upright and quiet. |
| Rajdhani SemiBold | Adopt for display | Existing OFL font; angular numerals connect the HUD, sign and vehicle language. Verify actual 1080p readability in native UI. |

The inventory is based on source recipes, exported payloads and the previous
native still sequence. It is not a claim that every older model has passed a new
moving-scene audit.

## Three route compositions

Use the shared 1,201 sampled frames and course hash, metres, Unity +Y up/+Z
forward. The course is 1,844.518 m long, with a 22 m running surface.

1. **Entry, progress .025.** Low occupied podiums create a foreground threshold.
   An off-axis broad commercial head and slender hotel establish unequal sides.
   Guidance repeats close to the rail; preserve the vanishing point.
2. **Turn, progress .150.** Keep the inside sightline low. Layer stepped service
   buildings beneath taller commercial masses on the outside. Short banked
   arrow modules provide parallax without straight rails cutting across a curve.
3. **Exit, progress .280.** Open the immediate inside edge, retain a continuous
   middle layer, and reveal an open-crown landmark beyond the bend. Avoid a wall
   of identical roofs. Recovery remains visibly in the city.

Continue this language only through the agreed exemplar (.98482 through wrap
to .70). Do not populate the remainder of the circuit in this assignment.

## First production steps

A. Four building families, podium, near guidance; shared materials, blue plume
and contact sample atlases, font files, style board and composition sketches.

B. Alternate masses, landmark, transit canopy, connector bridge, rooftop plant,
advertisement and route placement using actual course frames. Export LODs and
simple non-gameplay collision-proxy descriptions.

C. Ship coating/state textures, nozzle-local volume mesh, coherent core/wake/
spark assets and event timing contract. No articulation is justified by current
evidence; preserve the hull and its collision geometry.

D. HUD/menu graphics, display and small-text size tokens, full state storyboards,
and timbre/mix specifications for the existing sound synthesis. No new source
audio is necessary for this first exemplar.

## Material and import intent

Base colour is sRGB; normals use tangent-space +Y; packed metallic/smoothness is
linear with R metallic, A smoothness = 1 - roughness. Occlusion uses G. Emission
maps are sRGB masks multiplied by explicit linear HDR colours/intensity.
Building glass is opaque dark glazing, not a transparent outer cube.

Facade UVs are metric, one unit per metre with four metres per material tile.
Atlas/VFX UVs are 0..1. Building pivots are ground-centre, applied transforms,
Blender +Z up/-Y forward with baked FBX -Z-forward/Y-up export. Buildings use
no gameplay mesh collider. Simple box proxies are separate data for 01.

Native LOD thresholds need calibration by 01. Start at 0.20/0.035 relative screen
height for towers, 0.35/0.06 for close modules, with short crossfade. The old
importer's fixed 0.55/0.12 values cull distant buildings too early for this design.

## Evidence and limits

The 10.112-second supplied video is visual reference only, sampled across its
full duration for shape/state observations. No reference artwork becomes a
runtime asset. Previous road-light native stills establish the starting
appearance; they establish neither current performance nor acceptance.

Reject this proposal if native driving still shows repeated silhouettes,
detached construction, a blown-out road, a thin water-like plume, unreadable
display numerals, or a disappearing boost hull. Blender previews are composition
diagnostics. Only 01's integrated moving scene can establish usability.
