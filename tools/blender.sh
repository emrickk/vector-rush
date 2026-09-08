#!/bin/bash
set -euo pipefail
task_root="$(cd "$(dirname "$0")/.." && pwd)"
mode="${1:-help}"
if [ "$#" -gt 0 ]; then shift; fi
case "$mode" in
  help|--help|-h)
    cat <<'HELP'
Usage: tools/blender.sh {hero-v2|hero-v2-inspect|environment-v2|legacy-v1} [generator arguments]
Every operation uses a new evidence/asset-regeneration staging directory.
No mode copies generated assets into Unity or overwrites reviewed source files.
  hero-v2             Rebuild the current sculpted craft (optional --preview).
  hero-v2-inspect     Render a copy of the frozen V2 blend; do not export geometry.
  environment-v2     Rebuild and audit the coastal kit; requires rock3_material.py.
  legacy-v1          Rebuild the superseded craft in an isolated project-shaped directory.
BLENDER_BIN can override the Blender executable.
VECTOR_RUSH_ENVIRONMENT_SOURCE can explicitly select a complete environment source folder.
See docs/asset-regeneration.md for dependencies and reviewed runtime integration.
HELP
    exit 0 ;;
  hero-v2|hero-v2-inspect|environment-v2|legacy-v1) ;;
  *) printf 'Unknown asset operation: %s\nRun tools/blender.sh --help\n' "$mode" >&2; exit 2 ;;
esac
blender_bin="${BLENDER_BIN:-/Applications/Blender.app/Contents/MacOS/Blender}"
if [ ! -x "$blender_bin" ]; then printf 'Blender executable missing: %s\n' "$blender_bin" >&2; exit 1; fi
environment_source="${VECTOR_RUSH_ENVIRONMENT_SOURCE:-$task_root/SourceAssets/environment-v2}"
if [ "$mode" = environment-v2 ] && [ ! -f "$environment_source/rock3_material.py" ]; then
  printf 'Missing environment dependency: %s/rock3_material.py\nSee docs/asset-regeneration.md; no generation started.\n' "$environment_source" >&2
  exit 1
fi
mkdir -p "$task_root/evidence/asset-regeneration"
stage="$(mktemp -d "$task_root/evidence/asset-regeneration/$mode.XXXXXX")"
case "$mode" in
  hero-v2)
    cp "$task_root/SourceAssets/hero-v2/build_hero_v2.py" "$stage/"
    script="$stage/build_hero_v2.py" ;;
  hero-v2-inspect)
    cp "$task_root/SourceAssets/hero-v2/render_inspection.py" "$task_root/SourceAssets/hero-v2/Kestrel07-v2.blend" "$stage/"
    mkdir -p "$stage/renders"
    script="$stage/render_inspection.py" ;;
  environment-v2)
    cp "$environment_source/build_environment.py" "$environment_source/rock3_material.py" "$environment_source/audit_exports.py" "$stage/"
    cp -R "$environment_source/textures" "$stage/textures"
    mkdir -p "$stage/exports" "$stage/renders"
    script="$stage/build_environment.py" ;;
  legacy-v1)
    mkdir -p "$stage/SourceAssets"
    cp "$task_root/SourceAssets/build_assets.py" "$stage/SourceAssets/"
    script="$stage/SourceAssets/build_assets.py" ;;
esac
printf 'Staged asset operation: %s\nOutput directory: %s\n' "$mode" "$stage"
"$blender_bin" --factory-startup --background --python "$script" -- "$@"
if [ "$mode" = environment-v2 ]; then
  "$blender_bin" --factory-startup --background --python "$stage/audit_exports.py"
fi
printf 'Complete. Candidate files remain staged in: %s\n' "$stage"
