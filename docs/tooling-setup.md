# Local Blender and Unity tools

Verified on 2026-09-09. These are workstation tools; binaries and personal configuration are not stored in this repository.

| Tool | Installed version | Command / location |
| --- | --- | --- |
| Blender | 5.2.1 LTS | `blender` (`~/.local/bin/blender`) |
| Blender MCP server | 1.9.1 | `blender-mcp` (`uv tool` installation) |
| Blender MCP addon | 1.6, protocol 5 | Blender 5.2 user addons, enabled in saved preferences |
| Unity CLI | 1.0.0-beta.8 | `unity` (`~/.unity/bin/unity`) |
| Project Unity Editor | 6000.6.0f1 | `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app` |

## Use

```sh
blender --version
blender --background --python your-script.py
unity --help
unity editors verify 6000.6.0f1
```

Blender's launcher invokes the executable inside the application bundle directly. A symlink to the binary failed to locate bundled Python/resources on this installation; use the launcher instead.

Codex's global MCP configuration registers `blender` with the absolute `~/.local/bin/blender-mcp` executable path, `BLENDER_HOST=127.0.0.1`, and `BLENDER_PORT=9876`. Telemetry is disabled in both the server environment and Blender addon preferences. Refresh/restart the MCP connection in Codex (or restart Codex) to expose its tools to a new task.

Keep Blender open for MCP commands. The installed addon starts its local server automatically with the GUI. If stopped, open the 3D viewport sidebar (`N`), select **MCP for Blender**, and click **Start MCP Server**. Background Blender remains suitable for Python authoring scripts, but the addon intentionally does not serve MCP in background mode.

Existing project wrappers remain `tools/blender.sh` for staged historical asset recipes and `tools/unity.sh` for project preparation/build/tests.

## Verification and limits

- Blender background Python successfully imported `bpy`, enabled `blender_mcp`, saved addon preferences and exited normally through the final launcher.
- A standalone MCP client initialized the registered server, listed 28 tools, completed the addon protocol handshake and called `get_scene_info` successfully. The open default scene returned Cube, Light and Camera (3 objects).
- Unity CLI returned its version/help and verified the pinned Editor binary, Mac IL2CPP module, localization and editor integration paths.
- The first addon install could not discover an uninitialized Blender user folder; explicitly selecting the Blender 5.2 addons directory resolved it. The first scene query raced GUI startup; the subsequent query succeeded after the port opened.
- Tools are verified independently of Codex task tool refresh. This task does not claim Unity compilation, native gameplay, visual acceptance or current-build performance.

Sources: [Blender MCP installation](https://github.com/ahujasid/blender-mcp), [official Codex MCP configuration](https://developers.openai.com/codex/mcp).
