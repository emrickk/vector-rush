# Nocturne direct implementation status

Status: **OWNER-REJECTED VISUAL CANDIDATE — superseded by the AAA rebuild lanes in `docs/aaa-rebuild/dispatch.md`.**

The launcher now opens the preserved original game as the visual baseline. Candidate 03 contains the corrected race lifecycle and passed three native races, but its environment shares the owner-rejected direct-production art. The architecture/rendering workers are staging original-baseline improvements; none is visually adopted yet.

The owner authorized direct work from the previous checkpoint. The new authoring path uses persistent Unity meshes and materials, generated from checked-in C# recipes. It preserves the original craft, route and baseline driving constants; it does not instantiate the old city generator in the production scene.

Current app: `Builds/Vector Rush-production-02.app`, GUID `f75eb8151eea444ba1592222ea97431d`, course hash `32eb94bda0b257b30c3b111b2a67beb1626d1dca77577905ae021eee7bc36f07`, art revision `direct-architecture-01`. Launch using `tools/production/play-production.command`.

Implemented source includes saved track/barrier geometry, four named environment zones, independent finish timing and continued rival simulation, manual-only personal records, persisted volume/sensitivity/shake/bindings, menu focus, original procedural music and nearby rival audio. The latest Editor suite passes 63 tests on Unity 6000.5.3f1; that includes the Director integration test added after building candidate 02. Only the test fixture changed after that build.

The separate first native candidate completed a full traversal with nine natural-crossing screenshots and 1,440 simulation-time pose records. Those images pass file integrity; they have not received a visual verdict because the current session cannot view image content. The comparison page is `docs/production-native-comparison.html`.

Required work still open:

- Complete candidate-02 native races, full-lap audio preview and isolated performance evidence.
- Native settings interaction coverage; physical gamepad coverage is absent.
- Review actual camera visibility and foreground construction against the targets. Parametric construction and successful asset serialization do not establish the requested finish standard.
- Complete/rework materials, baked lighting/reflections, LODs and clearance validation across decorative families. The current generation does not meet all earlier art-family requirements.
- Produce editable Blender source if that workflow is resumed; none was fabricated or claimed for this candidate.
- Push source/history once repository permissions permit. Commit `82702d2` is local; origin still returns HTTP 403 for `anpingwang-unity`.

Human play, continuous audiovisual inspection, independent visual/code review and owner acceptance are **NOT_REVIEWED**. Do not promote this document to a final completion handoff while required implementation remains open.
