# Vector Rush development workflow

The user requested GitHub history for every completed plan step.

- Keep `docs/implementation-plan.md` current. Define the next bounded step before implementing it.
- Commit each completed step with a descriptive message. Record its checks, evidence, and remaining issues in `docs/development-history.md`.
- Preserve separate corrective commits and independent critique reports. Do not squash or rewrite published history without explicit user authorization.
- Push completed milestones to the authorized GitHub repository. Never commit credentials, Unity caches, temporary captures, or local licensing data.
- Preserve Unity `.meta` files alongside their assets. Commit Blender source, export scripts, and exported game assets.
- Keep native builds out of source control. Use versioned releases for distributing builds when requested.
- A clean compile is not a playability or visual-quality pass. Validate relevant behavior in the native game and label automated steering and simulation-time recordings accurately.
- Use parallel agents with explicit file ownership when independent game subsystems benefit from it, as requested by the user. An independent critic may report failures; never manufacture a passing verdict.


## Quality lessons from the rejected opening pass — 2026-09-09

The owner rejected changes that were technically verified but barely visible and explicitly asked us to learn from that failure.

- Judge visual milestones against the intended finished-game reference and complete native view/sequence, not merely against the previous build. A local improvement does not establish a global quality pass.
- Keep the full requested outcome authoritative. Do not silently shrink a scene/circuit rebuild into connector additions, shader repair or surface tweaks and call it complete. Record required rollout as unfinished until it is actually delivered.
- Inspect and explicitly retain, rework, replace or demote weak foreground assets. Prior work and successful imports do not entitle an asset to remain in the final foreground.
- Integrate construction, materials and lighting before adopting a visual treatment. One scene owner is responsible for the whole image; separate subsystem passes do not add up to an art verdict.
- Reject an inadequate native exemplar before replicating it. Repeated negligible results require a different production hypothesis, not another intensity/roughness sweep. Technical repairs may be committed, but must be labeled as repairs rather than artistic progress.
- Match the evidence to the claim: stills for appearance, continuously observed native motion/audio for presentation, human play for feel, isolated current-build measurements for performance. Never manufacture missing experience evidence.
