#!/bin/bash
set -euo pipefail
project_root="$(cd "$(dirname "$0")/../.." && pwd)"
app="$project_root/Builds/Vector Rush.app"
if [[ ! -d "$app" ]]; then
  printf 'Original visual baseline not found: %s\n' "$app" >&2
  exit 1
fi
open "$app"
