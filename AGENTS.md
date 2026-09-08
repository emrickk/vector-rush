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

