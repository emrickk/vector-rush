# Modeling and art assignment — Astra

**Model:** `gpt-6-astra`. You own modeling, Blender Python, UV/textures and world/lighting design data. You do not own Unity integration or gameplay code. Read README, contract-v1 and handoff-and-stop before starting. Do not spawn reviewers or subagents.

## Owned output and reference inputs

Write only `SourceAssets/nocturne-production/` and `docs/production-handoffs/ART_HANDOFF.md`. Read current native views under `evidence/nocturne-v2/opening-04/draft/combined/selected/`, generated art targets under `references/nocturne-v2/` (explicitly not native), current source geometry/assets and the global critique. Preserve existing Blender files/exports; rework copies in your new source tree.

Use the installed Blender binary or available Blender tools. Your Python is part of your modeling work. Do not make Sol implement geometry recipes from prose. Deliver actual editable meshes, exports, material textures and concrete placement/light data. No source-only handoff with missing exports.

## A1 — complete circuit design and replacement decisions

Create `source/NocturneProduction.blend`, `asset-ledger.csv`, and `design.md`. The ledger records every major current family as retained, reworked, replaced or demoted, with a visible reason. Do not default to retaining the existing near-city assets. The road/edge foreground, galleries, near facades, industrial connections and station require substantial finish improvement across large visible areas.

Design all four zones from the current native racing views:

- `viaduct`, progress0.00–0.30: supported track inside inhabited service levels, readable lower construction and a framed signal mast, with a quiet distant city.
- `canyon`,0.30–0.52: close facade/overhead framing and a deep cool gallery approach; readable corner/exit and room for traffic.
- `thermal`,0.52–0.72: large connected plant, pipe/platform/access hierarchy and selective process light, with one open side and a crest reveal.
- `station`,0.72–1.00: warm enclosure with distinct portal/interior/exit, then a civic frontage and an open start/finish approach.

Names alone do not establish distinction. For each zone specify what encloses the track, the focal structure, near/middle/far relationships, the quiet area, light/value hierarchy and transition to its neighbor. Inspect the whole-loop layout in Blender using Sol's actual geometry context as a reference. Do not approximate its course from screenshots or move its root.

While waiting for context, author reusable families and inspect existing source. Exact placement waits for the exported course hash/frames. Keep camera/route/physics unchanged for this run; arrange art around them. The existing night/blue-slate/amber/cyan identity stays recognizable without preserving every crude mass or light pole.

## A2 — author finished construction families

The following is the required family inventory, not permission to meet it with simple boxes. Dimensions are target envelopes in Unity metres; adjust to fit the actual context when necessary and declare the exact exported bounds. Do not distort finished assets through non-unit placement scale.

| Asset ID | Starting envelope / construction | Visible requirements |
| --- | --- | --- |
| NR_TrackProfile | Cross-section matches current22m deck and existing barrier alignment; supplied as profile data | Substantial outer barrier/base, fascia and underside with distinct material boundaries; continuous sweep through bends |
| NR_Pier | Approximately6×20×8, width×height×depth | Real foot/shaft/bearing hierarchy; may use separate height variants rather than scaled stretching |
| NR_GalleryEntry | Approximately30×18×6 | Deep portal, structural thickness, supported brow and integrated fixtures; clear racing opening |
| NR_GalleryBay | Approximately30×18×8 | Separate ribs, wall/ceiling returns, recessed panels and service channel; no surface floating in front of a blank shell |
| NR_GalleryExit | Approximately30×18×5 | Lighter exit framing and visible city destination; distinct from entrance |
| NR_ServiceFront | Approximately16×10×24 | Occupied recesses, credible base/roof, canopy support and readable nearby facade depth |
| NR_CanyonFacade | Approximately18×32×24 | Multi-storey facade structure, setback/opening depth and roof termination; designed at chase-camera distance |
| NR_StationFront | Approximately24×24×60 | Civic concourse/platform relationships, asymmetrical circulation and an intelligible service level |
| NR_SignalMast | Fit the retained mast location and course sightline | Substantial frame, occupied room, braces/bearings and connected base; retain recognizable paired signal identity |
| NR_ThermalPlant | Fit actual reserved plant location without blocking any route branch | Three major cylinder identities connected through supported pipes, platforms/access and plausible structure; no cylinders on a naked slab |
| NR_MidBlockA/B | Distinct medium-distance building silhouettes | Architectural base/middle/roof; quiet grouped occupancy, not different random-window seeds on the same box |
| NR_FarClusterA/B | Quiet background grouped silhouettes | Controlled gaps/heights/value; sufficient economy that the far city does not compete with nearby detail |
| NR_LightFixture | Housing variants for gallery/road/service use | Visible source, recess/diffuser and attached support; matches lighting.json source positions |

Longitudinal gallery shells must fit route curvature without overlaps or polygon gaps. If a rigid bay cannot follow the bend, provide panels/ribs as separate assets plus placement data or curve-specific variants; do not force Sol to invent geometry to repair them. The art-profile sweep belongs to Sol's geometry integration but the shape/material design belongs to you.

Finish an exemplar of every required family before filling the entire course. Make large, middle and small construction scales read together. Chamfer/bevel visible edges, use consistent material junctions, resolve roof/base contact and give glazing/interiors actual depth where the view warrants it. Tiny bolts/noise cannot serve as the headline improvement. A library of more detailed assets that stays off camera does not satisfy the task.

Provide LOD0/LOD1 for mesh families, correct pivots, UVs, normals/tangents, applied transforms and material slot IDs. Use the contract's triangle guidance as a measured starting budget, not a reason to strip away the planned construction.

## A3 — materials, lighting intent and native self-check

Author actual texture families with these IDs: `NR_Road`, `NR_Structure`, `NR_Concrete`, `NR_Ceramic`, `NR_Metal`, `NR_Glass`, `NR_Interior`, `NR_Diffuser`. Every texture follows the contract's color space, mask channel and metre scale. The shared library must distinguish material response under neutral and scene light. Road finish needs broad readable variation plus restrained grain; avoid painted view-dependent light streaks, excessive sparkle or the rejected cloudy atlas.

Do not build a shiny-road-only solution. Road, barrier body, nearby structure, wall/ceiling and craft surroundings must all contribute to the complete image. Preserve dark areas deliberately while making surfaces and contact relationships readable. Do not use overall darkness to conceal unfinished geometry.

Write lighting.json for all zones: environment intent, actual fixture-linked lights, source position/orientation/color/coverage, suggested bake mode and local reflection volumes. Values are proposals evaluated in native integration; fixture and light placement must agree. Sol owns Unity-specific implementation and baking.

Publish an immutable exemplar package and notify Sol with its revision/READY hash. Provide studio previews labeled as Blender renders and four-zone layout previews. When Sol supplies native views, inspect them as your own implementation self-check: intended facade visible, material scale correct, believable shape under actual light, no collision/camera obstruction, no obvious repetition or missing dominant surfaces. This is not an independent review or a final AAA verdict.

Fix art-side problems by publishing a new immutable package. Ask Sol to fix importer/shader/bake defects with exact IDs and images. Do not conceal faults by changing the camera or applying image edits to evidence. Parent does not approve artistic choices during this run; resolve within this brief, or report a specific unresolved design/capability blocker.

## A4 — complete the entire layout and hand off

Extend the self-checked kit across every zone and connecting stretch. The first gallery/station/viaduct section is a construction-method proof, not your stopping point. Explicitly remove/demote superseded weak near assets in the final layout; do not layer new structures over the old city indiscriminately. Sol's production scene must not instantiate the legacy city as an unexamined filler.

Final package requires complete layout, all referenced meshes/materials, all four zones and meaningful transitions. Include source files/recipes, a texture/material sheet, full-loop Blender layout, export/reimport evidence, declared open mesh boundaries, LOD/material/triangle counts, exact bounds and checksums. Use actual final dimensions and hashes; no placeholder manifest values.

In `ART_HANDOFF.md`, state final package/revision/course hash, editable source paths, all families delivered, replacement ledger decisions, native self-check views inspected, exact known limitations and whether any family remains incomplete. If incomplete, status is BLOCKED or INCOMPLETE, not ART_IMPLEMENTATION_COMPLETE.

Once `ART_IMPLEMENTATION_COMPLETE` is sent, stop heavy work and remain available only for concrete integration defects until Sol's final handoff. Do not launch a critic or begin another polish pass. The owner sees the integrated candidate first; parent independent review comes later only when requested.
