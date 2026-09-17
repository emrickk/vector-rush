# Unity import corrective pass

Date: 2026-09-09
Status: source changes ready for parent compile, test, import, build and native review. Visual status remains **BLOCKED_VISUAL_REVIEW**.

## Evidence and correction scope

The retained import report failed before publication because the package course hash
`32eb94bda0b257b30c3b111b2a67beb1626d1dca77577905ae021eee7bc36f07` did not match the current
`TrackPath` hash `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`. This pass preserves that
failure. The parent must regenerate the art package from the current course export. No mismatch is
ignored or rewritten.

The corrective changes are limited to the authorized AAA importer, runtime contracts, tests and this
record:

- `AAACourseIdentity` reproduces the exact 1,201-frame field order, `JsonUtility` serialization and
  SHA-256 convention currently implemented by `ProductionSceneSetup.CourseHash`. The importer now
  writes this hash into each `AAAExemplar`. Runtime attachment computes the active track identity and
  rejects a mismatch before adding `AAABaselineIntegration` or instantiating staging geometry.
- Import publication now saves and synchronously refreshes the Asset Database, explicitly loads both
  exact public resource names, requires both payloads, then runs required staging validation. Any
  exception remains inside the existing publication rollback boundary.
- Named renderer replacement now refuses a renderer when a `Collider` exists on its GameObject or any
  ancestor. Current schema 1 recipes remain additive with empty replacement lists and zero-size
  replacement volumes.
- `AAAReviewBuild.BuildSolstice` builds only the existing `Assets/Scenes/Solstice.unity`. It does not
  call `Prepare`, create a production scene, open a scene or save a scene. It requires fresh absolute
  `-aaaBuildOutput` and `-aaaEvidence` paths. Before building, it requires both published resources and
  validates each against the build course identity. It records build result/GUID, scene
  path/GUID/SHA-256, resource GUIDs/revisions, shared course identity, Unity version, timing, size,
  output and the explicit `NOT_RUN` preparation status in `build-identity.json`.

## Added editor contracts

`AAAImportContractTests.cs` covers four source-level contracts for the parent to execute in Unity:

1. Shared runtime course identity equals the exact existing `ProductionSceneSetup.CourseHash` output.
2. A payload authored for one course is rejected after the active course identity changes.
3. Required publication fails when either Opening or Gallery resource is missing.
4. A named child renderer cannot be disabled when an ancestor carries collision.

## Parent commands

After regenerating and finalizing the art package from the current course export, rerun the importer
with fresh destinations and evidence as described in `import-handoff.md`. The corrected importer still
requires `-aaaArtPackage` and a new absolute `-aaaEvidence` directory.

Build the unchanged Solstice source scene only after the corrected import and parent-owned tests pass:

```sh
"/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/absolute/path/to/isolated/UnityProject" \
  -executeMethod VectorRush.Editor.AAAReviewBuild.BuildSolstice \
  -aaaBuildOutput "/absolute/fresh/path/Vector Rush AAA Review.app" \
  -aaaEvidence "/absolute/fresh/path/aaa-review-build-evidence" \
  -logFile "/absolute/fresh/path/aaa-review-build.log"
```

The build entry does not select an exemplar. Launch the resulting binary separately with no AAA flag
for baseline, or with `-vrAAAExemplar opening` / `-vrAAAExemplar gallery` for technical comparison.

## Source-only limits

A standalone Roslyn compile against the installed Unity 6000.6.0f1 reference set passed for the
corrected importer, runtime contracts, review builder and new tests. The reported warnings are the
existing obsolete API uses in `ProductionSceneSetup.cs`, outside this worker's ownership. This check
does not load the project or execute Unity tests.

This worker did not run Unity, Blender, a native build, GPU capture, gameplay, tests or imports. Static
source checks cannot prove Unity resource discovery after `MoveAsset`, runtime loading, build success,
FBX orientation, collision clearance, appearance, motion, audio, handling or performance. No art was
accepted. Parent-owned Unity execution and native evidence remain required.
