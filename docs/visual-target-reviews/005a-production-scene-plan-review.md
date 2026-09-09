# Production scene plan: technical feasibility review

**Verdict: the six-milestone plan is technically plausible in the current project. Keep its substantial playable deliverables. Tighten the migration boundary in Milestone 1; no new architecture milestone or engine migration is needed.** A saved scene is not itself a visual improvement, and the plan correctly makes its value contingent on a lit native result and full-course completion.

This is a bounded source review of the proposed `2026-09-09-nocturne-production-rebuild.md`. No build, bake, implementation or runtime validation was performed.

## Five concrete traps and minimal corrections

### 1. TrackPath is not currently an authored spline

`TrackPath` serializes width, but its fourteen knots are a code-initialized readonly array. `Evaluate` returns world-space coordinates without applying its Transform; its sampled length table is cached once. Moving the scene's TrackPath object will therefore not move the simulated route. Saving a TrackPath component does not create editable route data. The existing generated road/barriers likewise use these coordinates.

**Correction to M1:** retain the existing route at an identity transform and generate/place geometry against that same route. Make this explicit. If later driving findings justify route editing, change to an explicit serialized route definition with cache rebuilding and regenerate the affected road/colliders together. Do not let artists move the spline root independently. This preserves the current plan's option to improve corners later without prematurely building a general track editor.

Source: [TrackPath](../../UnityProject/Assets/Scripts/World/TrackPath.cs:18), [road generation](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:102).

### 2. The current world factories cannot simply run in edit mode and then be saved

`WorldBuilder.Box` removes primitive colliders with runtime `Destroy`. `WorldBuilder` and several child builders own temporary meshes/materials/textures and destroy them on teardown. `NightTrackLighting` also destroys its temporary primitive after fetching its mesh. A production conversion that merely invokes the current Build methods and saves the scene can retain unwanted colliders, omit asset dependencies or leave generator ownership attached to retained objects. Saving only meshes/materials also misses the generated road textures.

**Correction to M1:** define one editor construction/export path that uses editor-appropriate temporary-object disposal, persists meshes **and texture dependencies**, assigns persistent materials, and leaves a plain authored hierarchy without legacy generator cleanup ownership. Re-running construction must replace only its declared generated output and preserve separately authored scene work. Runtime craft materials can remain transient with their own owner. Do not make every existing city builder editor-safe as a separate project.

Source: [factory and disposal](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:148), [road textures](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:35), [cleanup](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:225), [lighting helper](../../UnityProject/Assets/Scripts/World/NightTrackLighting.cs:19).

### 3. Asset persistence does not make the geometry ready for a light bake

The current procedural mesh helper writes primary UVs and normals/tangents, but no secondary lightmap UVs or static/GI authoring. Its existing reflection probe is realtime and explicitly rendered after startup. These runtime assumptions do not turn into baked lighting merely by serializing the scene.

**Correction to M1:** the representative lit area must include deliberate GI participation, suitable lightmap UVs/texel allocation, explicitly configured light modes, persisted lighting/reflection data and verified moving-craft probe response. Scale the bake only after that representative native build succeeds. This is a clarification of the existing bake proof, not another gate. Avoid marking the whole inherited city static and attempting a full bake first.

Source: [mesh helper](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:148), [current lighting/probe](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:212).

### 4. The production branch must separate shared resources as well as scene construction

Bootstrap constructs the track, city, camera, postprocessing profile and craft materials in one Awake. Craft surface creation specifically accepts `WorldBuilder`; the metal fallback also depends on a field initialized by `WorldBuilder.Build`. Keeping a dormant WorldBuilder reference is therefore not sufficient. Meanwhile `Prepare` changes the shared renderer, pipeline, Resources templates and shader settings before recreating Solstice. Avoiding its scene reset alone does not protect legacy reproduction.

**Correction to M1:** extract the narrow craft-material factory and its runtime disposal owner, update `ShipSurfaceMaps.Create` accordingly, and initialize its metal fallback without building a city. Explicitly assign ownership of the gameplay camera and its URP settings; skip legacy global grade/lighting/preview configuration in the production path. The production build entry selects the saved scene and production rendering assets directly. Preserve the legacy settings/material assets, including referenced renderer features, rather than silently retuning them through shared references. Historical apps remain immutable as already required.

Source: [Bootstrap](../../UnityProject/Assets/Scripts/VectorBootstrap.cs:21), [craft helper](../../UnityProject/Assets/Scripts/Presentation/ShipSurfaceMaps.cs:12), [Prepare](../../UnityProject/Assets/Editor/VectorRushSetup.cs:16).

### 5. Independent rival completion requires a race-lifecycle decision

The existing RaceDirector stops sampling and freezes every vehicle when the player finishes. M5 correctly calls for independent completion evidence, but slower rivals cannot produce finish times under this lifecycle. Persisting results before deciding this contract would entrench the current limitation.

**Correction to M5:** define how rival simulation/completion continues after the player's result is fixed, or explicitly limit the result data to finishes actually observed. Separate the frozen player time from any continuing race clock. Then implement records and comparisons against that contract. No campaign, scoring economy or additional content is needed.

Source: [finish behavior](../../UnityProject/Assets/Scripts/Gameplay/RaceDirector.cs:99).

## What should remain unchanged in the plan

Keep the rough whole lap before exemplar production, native exemplar acceptance before repetition, mandatory full-course rollout, integrated sound, and the final separate human-play/performance verdicts. The plan already acknowledges that direct controller/human coverage can remain unavailable; do not relabel automated driving as a substitute. The bounded migration corrections above make the work executable without turning the response into another sequence of tiny fixes.

## Follow-up: all five planning corrections addressed

Verified the revised plan's M1 at lines 53–60 and M5 at line 112. It now explicitly covers the identity/world-space route and later serialized-data/cache changes; editor export of persistent meshes/materials/texture dependencies without legacy cleanup ownership; actual bake UV/GI/light/probe preparation; the narrow craft factory, one gameplay camera and isolated production rendering references without `Prepare`; and a bounded post-player-finish lifecycle with a frozen player result, up to 60 seconds for actual rival crossings, DNF at the limit and immediate restart/quit. These resolve the five source-level ambiguities raised above. The review is closed at the planning level; implementation feasibility in the native editor and achieved game quality remain the plan's execution gates.
