# Independent critique 02 — scoped source fixes and revised asset

Review date: 2026-09-07 local time (2026-09-08 UTC). Re-review of critique 01's findings and regressions in those changes. No runtime images or gameplay recording were available to this reviewer. No runtime, collision, performance or AAA pass is issued.

## Evidence

Inspected the current front, rear and top PNGs under `evidence/asset-renders` with an image viewer. Read the changed portions of `WorldBuilder`, `HoverVehicle`, `VectorBootstrap`, `RaceDirector`, `RaceProgress`, the related test source, `fbx-validation.json`, and `evidence/editmode-results.xml`. The XML records **17 tests, 17 passed, zero failures**, platform **EditMode**. Parent reports successful compilation with Unity 6000.6.0f1; the test XML is independent evidence that the tested assembly ran, but is not a standalone build or gameplay result.

## Original source findings: disposition

| Finding | Verdict | Evidence and remaining verification |
|---|---|---|
| Craft clears the barrier | **Addressed in source** | `HoverVehicle.cs:12` uses 1.55 m hover; `VectorBootstrap.cs:36` uses a 1.2 m-high collider centered at +0.12 m. Bottom is now **1.07 m**. `WorldBuilder.cs:81` raises the wall to 2 m, giving **0.93 m** vertical overlap at level equilibrium. Actual glancing collisions on both sides still need runtime verification. |
| Reversed wall shell winding | **Addressed in source** | `WorldBuilder.cs:84` reverses both mirrored triangle sets. Right inner face now points −Right, left inner face +Right, and top faces upward. Confirm lighting, visible inner faces and mesh contact in runtime. |
| Projected off-track gate acceptance | **Addressed in source and scoped tests** | `RaceDirector.cs:113–116,145–151` validates lateral distance, hover envelope and forward velocity. `RaceProgress.cs:53–60` requires consecutive valid samples, preventing re-entry from inventing a crossing. The XML confirms passing corridor, off-track/re-entry and invalid finish tests. This does not prove all possible course shortcuts are impossible. |
| Dark Ceramic becomes ivory; WhiteMark lost | **Addressed in source** | `VectorBootstrap.cs:46,48` explicitly maps Ceramic to graphite and WhiteMark to a separate bright material. Dark-vs-light identity is preserved, though Ceramic's exact authored roughness/metalness is still intentionally replaced. Judge final lighting on the imported craft. |
| Prestart recovery gains distance | **Addressed in source and scoped test** | `RaceProgress.cs:14–15,37–38` retains each racer's exact starting progress. `StartingGridRecoveryStillRequiresForwardStartCrossing` passes and checks preservation. |
| Same-step finish order favors racer list | **Addressed in source and scoped test** | `RaceProgress.cs:73,80` calculates crossing fractions. `RaceDirector.cs:134–140` sorts this step's finishers and interpolates finish time. The fraction-distinguishing test passes. The test exercises fractions, not a full multi-vehicle director simulation; a close race finish still needs runtime confirmation. |

No new blocking defect was found in this scoped source re-review. That statement is limited to these changes and is not a full source audit.

## Asset findings: disposition

- **Identification: partly addressed.** The new large white **07** marks on dark plaques are immediately legible in both front and rear renders. They solve the original lack of a medium-scale identifying element. The pale yellow stripes and small gray wordmarks remain weak against the bright hull.
- **Broad uniform armor: partly addressed.** Added service panels, fasteners and labels break up the front panels, but material response across most armor remains highly uniform. The rear view also shows large simple slabs around the engine mounts.
- **Faceted opaque canopy: partly addressed.** The original obvious planar faceting is gone; the new surface has continuous highlights. It remains opaque and reads like a glossy inset shell. Smooth geometry alone does not supply cockpit construction, depth or glass transmission.
- **Collider fit: improved in source, pending import/runtime contact.** The collider is now 5.2 × 7.2 m. The FBX validation artifact reports craft bounds of 5.4375 × 1.6941 × 7.487 m, substantially reducing the original width mismatch. Bounds are asymmetrical along the length, so total dimensions alone are not sufficient to declare exact contact fit.

## Remaining top three observed asset defects

1. **Painted surfaces still lack convincing material variation.** The clean, nearly identical finish across the nose, mid armor and tail armor gives the craft a concept-model appearance. Apply restrained changes in roughness and surface scale, especially around panel breaks and maintenance areas. Reassess in the actual game lighting before investing in microscopic detail.
2. **Yellow accent contrast remains too weak in these renders.** Both nose stripes and outer fin identifiers look pale against the ivory under the studio lighting. The newly clear number plaques are a successful hierarchy cue; use them as the contrast benchmark and strengthen a small number of yellow/graphite relationships. Preserve the restrained palette.
3. **Canopy is now smooth but still visually opaque and featureless.** In the rear and top views it reads as a glossy dark oval with broad softbox reflections. Add visible construction such as a convincing seal/frame, a subtle interior or controlled glass depth. Verify that any additional detail is visible from the actual chase angle.

The rear render newly demonstrates distinct twin propulsion units with visible radial vanes and turquoise rings. That authored geometry is visible and readable in the asset render; its appearance and motion in the game remain unverified.

## Evidence capture defect

The currently available `hero-top.png` cuts off both forward pontoon tips at the upper border and exhaust bottoms at the lower border. It cannot serve as a full silhouette or complete-bounds review image. Pull the orthographic camera back or change its framing and recapture. The front and rear views show the whole craft and remain usable. This is a capture defect, not a claim that the mesh is truncated.

## Runtime review still required

Inspect actual 1920 × 1080 build screenshots and recorded play for imported orientation/materials, rear silhouette, wake position, wall contact, readable track direction, a completed three-lap race, pause/recovery and repeated restart. The source fixes and 17 passing EditMode tests are real progress; they do not replace this next evidence gate.
