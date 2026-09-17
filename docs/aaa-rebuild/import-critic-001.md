# Staged Blender-to-Unity importer technical critique

Date: 2026-09-09
Scope: source-only independent review of the staged exemplar importer, runtime AAA contracts, and `SourceAssets/aaa-nocturne/exemplar-01`.
Verdict: **No source-proven blocker prevents one diagnostic import in an unchanged isolated project. Do not treat a successful import as a durable runtime contract or as visual adoption.** Two implementation defects can permit a stale or unavailable resource to escape the intended validation boundary.

## Findings

### 1. High: the validated course identity is discarded before runtime

The importer computes the current `TrackPath` hash and requires the manifest, layout, and staging marker to match it (`UnityProject/Assets/Editor/AAA/AAAExemplarImport.cs:70-76`, `145-152`). The generated `AAAExemplar` does not retain that hash (`AAAExemplarImport.cs:360-379`), because the runtime payload has no course-identity field (`UnityProject/Assets/Scripts/World/AAA/AAAExemplar.cs:10-14`). Runtime attachment consequently validates only the payload and then evaluates placements against whatever `TrackPath` is current (`UnityProject/Assets/Scripts/World/AAA/AAABaselineIntegration.cs:31-36`, `128-133`).

This does not block the parent's immediate import and build if both commands use the same unchanged code. It is a runtime safety blocker for retaining or reusing the staged resources: any later track change can move every exemplar without rejection, even though course-hash equality was the importer's primary frame guarantee. The additive current payload reduces replacement risk, but it does not prevent new geometry from intersecting the route after a track change.

Lowest-risk corrective path:

1. Move the exact course serialization and SHA-256 routine used by `ProductionSceneSetup.CourseHash` into a runtime-safe shared helper.
2. Add `courseHash` to `AAAExemplar` and populate it from the already computed `currentHash` in both recipes.
3. In `AAABaselineIntegration.TryAttach`, compare the payload hash with the supplied runtime `TrackPath` before adding the component or instantiating anything. On mismatch, log and retain the baseline.
4. Add an editor test that imports or constructs a valid payload, mutates the course identity, and proves runtime attachment refuses it.

### 2. Medium: post-publication validation accepts a missing required resource

The importer moves both recipe assets to their public resource paths, invokes `AAAExemplarValidation.ValidateStaging()`, and then records `TECHNICAL_STAGING_COMPLETE` (`AAAExemplarImport.cs:110-123`). That validator treats any missing `Resources.Load<AAAExemplar>` result as an optional absence and continues (`UnityProject/Assets/Editor/AAA/AAAExemplarValidation.cs:13-18`). This behavior is suitable for the validator's standalone menu command, but it is not a valid postcondition for an importer that has just declared both resources published.

The move operation normally updates the Asset Database, so this is not proof that the current package will fail. It is a false-success path: a resource-discovery or publication defect can be logged and then converted into importer success.

Lowest-risk corrective path:

1. After both moves, save/refresh as required by the supported Unity API sequence.
2. Explicitly load both exact public paths in the importer and require non-null `AAAExemplar` results before calling the optional standalone validator.
3. Prefer a `ValidateStaging(bool requireBoth)` or a dedicated required-resource entry point so the standalone optional behavior cannot be reused accidentally.
4. Add a test where one public resource is absent and assert that importer post-publication validation fails and rolls back anything published by that run.

### 3. Low, latent: renderer replacement does not provide the collision protection its comment claims

Current recipes are additive: the importer writes a zero-size replacement volume and empty renderer/light name arrays for every placement (`AAAExemplarImport.cs:377-379`). With the current package, `Named` therefore returns false and no baseline renderer or light is disabled (`AAABaselineIntegration.cs:140-154`, `183`). There is no current-package replacement blocker.

For a future nonempty replacement payload, the runtime comment says it protects the "entire collision-bearing visual," but the check only looks for a `Collider` on the renderer's own GameObject (`AAABaselineIntegration.cs:140-146`). A collider on a parent or sibling can remain active after the named child renderer is hidden, producing invisible collision.

Lowest-risk corrective path before enabling replacement in any schema: keep schema 1 replacement arrays required empty. Introduce replacement only with an explicit authored renderer-to-collider contract and validation of the full hierarchy. Do not infer safety from `renderer.GetComponent<Collider>()` alone.

## Package and contract checks that did not produce a source defect

- All 29 entries in `checksums.json` matched the bytes currently on disk. The live SHA-256 of `checksums.json` is `86d07dc969f3130ac81066d97a287ffe762228f9202b224ed4f605071e5a44e5`, matching `STAGED.json:5`. `STAGED.json` and `checksums.json` are the only intentionally unlisted regular files; self-checksumming either document is not required by the implemented identity sequence.
- The package contains 5 assets, 10 FBXs, 8 materials, and 13 placements. Manifest LOD0 bounds agree with `asset-stats.json`; each LOD stats entry declares the source-to-Unity mapping `(x,z,-y)`, and the importer independently measures imported vertices in prefab-root space before publication (`AAAExemplarImport.cs:181-198`, `289-347`). This is a strong unit/axis-extrema gate, but only the actual Unity import can execute it.
- Placement quaternions are converted to route-local Euler corrections and immediately round-tripped against the authored absolute transform (`AAAExemplarImport.cs:365-379`). Source frames are compared with exact `TrackPath.Evaluate` values before destination mutation (`AAAExemplarImport.cs:201-220`). No source inconsistency was found in the current layout.
- Textured base color is imported as sRGB and uses a white material tint; metallic/smoothness masks are linear and smoothness reads alpha with scalar smoothness set to 1; normal maps use `NormalMap` import with authored strength (`AAAExemplarImport.cs:239-280`). The package recipe writes metallic into mask R and smoothness into mask A (`SourceAssets/aaa-nocturne/exemplar-01/source/recipe_snapshot.py:31-45`). No source-proven material channel blocker was found.
- The manifest contains no native light payload, and the importer explicitly rejects a nonempty one (`AAAExemplarImport.cs:157-158`). Emissive diffuser materials are created, but no Unity `Light` is created. This is safe for technical staging and insufficient to establish native night lighting.
- Both public resources reference shared persistent prefab/material assets. Source inspection does not prove duplicated mesh payload in a player build. Because the entire imported destination is under `Resources`, build-size and residency cost remain measurements for the parent, not claims made by this review.

## Limits and required parent evidence

No Unity Editor, Blender, native build, image inspection, or play session was run. The handoff's Roslyn result was not independently reproduced. This review cannot verify actual FBX importer behavior, handedness/facing, generated UV2 quality, URP shader variants, LOD transitions, baseline overlap, camera clearance, emissive response, performance, motion, audio, or handling.

`layoutComplete` is false in the manifest and layout (`manifest.json:5`, `layout.json:4`). Replacement lists are empty and the baseline remains visible. The package is a bounded additive technical candidate. Appearance and AAA acceptance remain **BLOCKED_VISUAL_REVIEW** regardless of source/import success.
