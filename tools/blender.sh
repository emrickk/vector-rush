#!/bin/bash
set -euo pipefail
task_root="$(cd "$(dirname "$0")/.." && pwd)"
exec /Applications/Blender.app/Contents/MacOS/Blender --background --python "$task_root/SourceAssets/build_assets.py" -- "$@"
