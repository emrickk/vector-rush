# Playable race coordination

Status: **WORKSPACES_READY**

Prepared: 2026-09-14 18:18 PDT. Integration owns this file.

## Baseline and preservation

- Original checkout: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush`
- Recoverable copy-on-write snapshot: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/snapshot-original`
- Shared source baseline commit: `1762224deed3c9e994483f5307999b68ae386e38`
- Shared source baseline tree: `a82088ba86cd24bd7f22ad1897f566d3ace2db2c`
- Original branch at capture: `codex/nocturne-production-handoff`, with no local `HEAD` commit. Status contained 2,057 added, 35 added/deleted, 6 added/modified and 45 untracked paths.
- Original Git-state fingerprints: index file `005e696084fa4fbbbfdd9ee3b08ada6d77fedd36f9b1f412635dacb645c9c0a8`; index entries `b29bf77f27852306a0d2c4357ad843b32d2631c5c28c2cc0449ea4faf0461b48`; working status `dec8e84a6054045657e1546b80406a2273a67c6244ae906e75ee34725431af83`.
- The original index, worktree, branch and remotes were not changed. The snapshot retains `.git`, editable sources, Unity metadata, historical evidence and all native builds.

The clean baseline commit contains authored source, editable assets, package locks, project settings and Unity `.meta` files from the captured worktree. Working copies exclude `UnityProject/Library`, `Temp`, `Logs`, `Obj`, `UserSettings`, native `Builds`, historical `evidence`, browser-capture logs and the 4.1 GB Instruments trace. Those exclusions remain recoverable in the snapshot or original and are not source inputs.

A private filename-only secret audit is under the workspace root's mode-700 `private` directory. It found no sensitive-looking filenames. Seven content-marker candidates were reviewed: six are browser page captures excluded from Git, and the remaining Unity setting is a blank `metroCertificatePassword`. No push is authorized from this baseline.

## Workspaces and ownership

| Lane | Workspace | Branch | Shared report |
| --- | --- | --- | --- |
| Integration | `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/integration` | `integration/playable-race` | `reports/01-integration.md` |
| Environment | `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/environment` | `lane/environment` | `reports/02-environment.md` |
| Racing | `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/racing` | `lane/racing` | `reports/03-racing.md` |
| Replay and presentation | `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/replay-presentation` | `lane/replay-presentation` | `reports/04-replay-presentation.md` |

All four branches start at the same baseline commit. Lane repositories use the Integration repository as their local `origin`. Workers commit only owned paths and write their durable handoff to the shared packet in the original checkout. Integration fetches or cherry-picks lane commits only after reading the report and checking path ownership.

The source remote resolves to `https://github.com/emrickk/vector-rush`, but live verification reports it is **public**, while the execution packet permits pushing only to an authorized private repository. Do not push. Integration will preserve local history until the repository is private or the owner gives new direction.

## Start status

- Unity project version is `6000.6.0f1`; manifest pins Input System `1.20.0`, URP `17.6.0`, Test Framework `1.8.0` and Pipeline `0.6.0-exp.1`.
- Unity CLI `1.0.0-beta.8` is installed. No Pipeline-connected Unity Editor, Blender GUI or Vector Rush app was running at capture. Existing Blender MCP helper processes were left untouched.
- Preserved baseline app: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/snapshot-original/Builds/Vector Rush.app`
- Baseline app identity: bundle `com.Vector-Studio.Vector-Rush`, version `1.0`, arm64, ad hoc signature, executable SHA-256 `648cb33824e961ecdeeba54e15a82632bae1f32c405838280169cc2352ba9612`. Strict code-sign verification fails on both original and snapshot because of resource-fork/Finder detritus. No source revision can be assigned because the original branch has no `HEAD`.
- Static inspection found current shared tests that reference an older alternative race/record API. This is a present-source risk, not a test verdict. Integration owns the baseline compile/test result; Racing and Replay should follow `contracts.md` and report any owned compatibility fix.

## Heavy-process lease

Lease parent: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/locks`

Before a Unity import/build/test, native capture or Blender render:

1. Atomically create `heavy-process.lease` as a directory under the lease parent. Creation succeeds for only one owner.
2. Inside it, write `owner.txt` with lane, workspace, process ID, start time and intended command. Recheck that the directory still contains your owner record before starting the heavy process.
3. If the directory already exists, continue source-only work or wait. Never delete another owner's lease, assume it is stale or terminate their process.
4. The owner releases the lease only after its own heavy process exits and after verifying `owner.txt` still names that owner. If the owner cannot release it, report the lease to Integration.

Integration owns baseline and candidate capture slots. Environment may use the lease for Blender rendering and requested Unity import checks. Racing and Replay use it for their own isolated test/build/capture runs.

## Report exchange

Reports live in `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush/docs/plans/2026-09-14-playable-race/reports/`. Each report states status, workspace and baseline, commits and files, observed change, checks and evidence, remaining defects, integration dependencies and exact next action. Do not infer work from an old report or claim monitoring after a task ends.
