# Warm gallery Unity integration handoff

Status: source integration is ready for parent validation. Native import, Unity tests, build, captures, appearance, motion, and performance remain unverified.

Unity 6000.6 importer contract: keep `ModelImporter.bakeAxisConversion` disabled for these FBX files. The Blender export already uses `axis_forward: "-Z"`, `axis_up: "Y"`, and `bake_space_transform: true`; enabling Unity's additional axis bake reverses the declared forward sign. A retained-FBX probe measured Gallery Portal LOD0 as `Z -1.61..0.905` with the extra bake and `Z -0.905..1.61` without it. The latter matches the sealed `(x,z,-y)` package bounds while preserving identity root transform and unit scale. Keep the 1 cm bounds rejection unchanged.

Current package reconciliation: the sealed `exemplar-01-gallery02` package still declares the legacy JSON-byte hash `598718f0...` in four `courseHash` fields. Keep that value only as `courseSource.sha256`; set `courseHash` in manifest, layout, lighting, and STAGED to the canonical float-bit identity `8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e`, then reseal checksums and STAGED. Retained `viaduct` records and assets may remain in the sealed package for provenance, but this gallery-only import ignores them and does not republish opening art. Existing `sourcePlacement` and `sourcePart` lighting keys are accepted aliases for `housingInstanceId` and `socket`.

## Bounded step

Import one checksummed, full-span warm-gallery payload authored from the current 1,201-frame course export. Publish it only to the optional `AAA/GalleryExemplar` resource. Preserve `AAA/OpeningExemplar` unchanged when it exists; its absence is valid. Keep the continuous legacy warm-gallery shell, road, barriers and cyan navigation dashes, camera, craft, HUD, physics, and global atmosphere unchanged. With `-vrAAAExemplar off` or no flag, the source baseline must remain byte-for-byte behaviorally unchanged.

The candidate may replace legacy warm detail geometry and local warm lights only after the entire package validates. It must never layer duplicate legacy ribs, panels, diffusers, or warm pools under the new assembly.

## Gallery package contract

The importer continues to accept `schema: "aaa-nocturne-exemplar-1"`. The gallery revision must match `exemplar-01-gallery02[-A-Za-z0-9_]*`. The package is gallery-only and must declare:

```json
{
  "manifest.json": {
    "schema": "aaa-nocturne-exemplar-1",
    "revision": "exemplar-01-gallery02",
    "courseHash": "<canonical float-bit course identity>",
    "layoutComplete": true,
    "courseSource": {
      "path": "course-data.json",
      "sha256": "<sha256 of exact source bytes>"
    },
    "galleryLighting": "gallery-lighting.json"
  },
  "layout.json": {
    "schema": "aaa-nocturne-exemplar-1",
    "courseHash": "<same identity>",
    "layoutComplete": true,
    "gallerySpan": { "startProgress": 0.86, "endProgress": 0.902 },
    "replacement": {
      "mode": "warm-details-and-local-lights",
      "rendererNames": [
        "Warm gallery primary portal structure",
        "Warm gallery recessed ceramic cassettes",
        "Warm gallery folded service returns",
        "Warm gallery light and maintenance housings",
        "Warm gallery integrated diffusers",
        "Warm gallery maintenance markings"
      ],
      "lightNames": [
        "Warm gallery concealed surface wash",
        "Warm gallery road pool"
      ]
    },
    "instances": ["<station placements only, using the existing placement schema>"]
  }
}
```

`course-data.json`, `gallery-lighting.json`, the source blend, every FBX, and every texture must be listed in `checksums.json`. `STAGED.json` must carry the same revision and course hash, `status: "TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW"`, and `layoutComplete: true`.

The canonical course identity is SHA-256 over this little-endian byte stream: ASCII `VRCRS1`, Int32 sample count `1200`, then float32 width and length, followed for frame indices `0..1200` by float32 progress, distance, position XYZ, forward XYZ, right XYZ, and up XYZ. The separate `courseSource.sha256` is the SHA-256 of the exact `course-data.json` bytes. The importer parses all 1,201 source frames and requires every parsed float bit to match the current `TrackPath`; matching a declared hash alone is insufficient. A retained legacy opening may have only its verified course-identity metadata migrated to the canonical hash. Its geometry, materials, placements, and public resource GUID remain unchanged.

The replacement declaration is exact and closed. The continuous `Warm gallery continuous shell` renderer is intentionally absent and prohibited from the replacement list. Track/road, barriers, navigation dashes, craft, camera, HUD, directional lights, and global atmosphere are also prohibited replacement targets.

Every adopted gallery placement must use `zoneId: "station"`, a source frame in the inclusive `.860000–.902000` span, positive uniform scale, and banked or upright frame mode. Retained `viaduct` records may coexist but are validated then ignored by this gallery-only publication. The station family must contain at least seven anchors, reach within one exported frame of each boundary, and leave no anchor gap greater than `.01` progress. It must cover the connected entrance, interior cadence, and exit. The importer rejects partial layouts, out-of-span station instances, absent end coverage, and unrecognized replacement names before mutating project assets.

## `gallery-lighting.json` schema

Coordinates are Unity world metres from the pinned course export. RGB values are linear and each component is in `[0, 1]`. Spot fixtures require exactly one of `aimDirection` or `target`; providing both is invalid. Direction vectors must be finite and nonzero. Angles are degrees.

```json
{
  "schema": "aaa-nocturne-gallery-lighting-1",
  "courseHash": "<same identity>",
  "fixtures": [
    {
      "id": "warm-pool-01",
      "type": "spot",
      "position": [0.0, 0.0, 0.0],
      "target": [0.0, 0.0, 1.0],
      "rgb": [1.0, 0.72, 0.46],
      "intensity": 350.0,
      "range": 30.0,
      "outerAngle": 74.0,
      "innerAngle": 36.0,
      "housingInstanceId": "ceiling-bay-01",
      "socket": "recessed transverse diffuser",
      "shadows": "none"
    }
  ]
}
```

Allowed light types are `point` and `spot`. Spot fixtures require exactly one aim field. Point fixtures omit aim and angles. Every Gallery 02 fixture requires a housing placement and a non-empty socket label; `sourcePlacement` and `sourcePart` remain accepted aliases for `housingInstanceId` and `socket`. The housing ID must resolve to an imported station placement. Limits: 1–32 fixtures, including at least three spot fixtures; intensity `(0, 2000]`; range `(0, 60]`; spot outer angle `[10, 120]`; spot inner angle `[0, outerAngle]`; optional shadows `none`, `hard`, or `soft`, defaulting to `none`. All IDs must be safe and unique. All positions and targets must be finite and remain inside a conservative world-space envelope derived from track frames `.86–.902`: lateral distance at most 18 m, vertical offset `[-2, 20]` m, and no more than 3.5 m longitudinally from the nearest sampled frame. The importer creates named child lights in the gallery payload; it does not add global, directional, area, mixed, or baked lights.

Fixture sources must visibly correspond to authored housings or diffuser geometry. Use separated local pools with darker gaps, restrained material emission, and warm light that affects nearby panels and road. Preserve the cyan edge color. Any road response must come from these scoped fixtures and the existing road material; this pass adds no screen-space reflection feature and does not repaint the whole road.

Gallery material `emissionIntensity` is capped at `2`; current authored diffuser values `.42` warm and `.55` cool are within the contract. This cap limits self-emission but does not prove exposure, bloom, or native brightness.

## Atomic runtime behavior

`-vrAAAExemplar gallery` is the only candidate switch. Import publication is transactional: validate package bytes, source frames, full span, replacement declaration, materials, meshes, and lights first; create imported assets second; replace only `AAA/GalleryExemplar` last. A failure retains the prior opening and gallery resources.

At runtime, instantiate and validate the complete inactive candidate first. Then disable exactly the six named warm detail renderers and all matching local warm lights, and activate the candidate. If any required renderer or light family is absent, ambiguous, collider-bearing, or outside the warm section, abort and restore the baseline. Repeated same-name warm lights are allowed only with the expected counts: 12 concealed washes and 6 road pools. The continuous shell stays enabled throughout.

## Parent validation packet

Source checks can establish schema, checksums, finite values, exact current source-frame correspondence, replacement completeness, and flag-off isolation. They cannot establish visual quality. Parent should run editor tests, import to fresh evidence, build one binary, and capture matched `off` and `gallery` stills plus an uninterrupted approach/interior/exit drive-through. Native review must verify construction depth, at least three separated warm pools with dark gaps, material separation, preserved cyan edge, no doubled surfaces or gaps, clean exit framing, stable LODs, and no road-wide reflection artifact.

Recommended parent order:

1. Have the artist update all four `courseHash` fields to `8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e`, then seal `checksums.json` and `STAGED.json`.
2. Run `VectorRush.Tests.AAAGalleryIntegrationTests` and the existing AAA editor tests.
3. Run `VectorRush.Editor.AAAExemplarImport.ImportStaging` with the finalized gallery package and a fresh evidence directory. The importer must leave Unity axis baking disabled and pass the sealed 1 cm bounds check.
4. Run `VectorRush.Editor.AAAReviewBuild.BuildSolstice` with fresh build and evidence paths.
5. Launch the same binary separately with `-vrAAAExemplar off` and `-vrAAAExemplar gallery`; capture the matched stills and continuous drive-through required by the critic.

Source-only checks completed in this lane: all runtime scripts compile together against the Unity 6000.6.0f1 managed reference set, the three owned AAA editor scripts compile against that result, and `AAAGalleryIntegrationTests.cs` compiles against Unity's editor-test references. The test compile reports only pre-existing warnings in `ProductionBoundaryTests.cs`. The canonical hash independently recomputed from the artist's current `course-data.json` is `8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e`. The current station layout has 19 placements from `.8600000143` to `.9016666412` with maximum anchor gap `.0075`; its 20 spot fixtures all resolve to named station placements and carry non-empty socket labels. These are source/data checks, not Unity import, executed tests, native, or visual results.

Parent-run Unity EditMode results remain required. Import publication validates before resource mutation and restores prior serialized resources on caught failures; process interruption during the final Unity asset save is not proven crash-atomic.
