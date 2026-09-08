#!/bin/bash
set -euo pipefail
task_root="$(cd "$(dirname "$0")/.." && pwd)"
task_editor="${VECTOR_UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity}"
if [[ ! -x "$task_editor" ]]; then
  echo 'Unity 6000.6.0f1 is not available. Set VECTOR_UNITY_EDITOR to your Editor executable.' >&2
  exit 2
fi
mkdir -p "$task_root/evidence"
case "${1:-open}" in
  open) exec "$task_editor" -projectPath "$task_root/UnityProject" ;;
  prepare) exec "$task_editor" -batchmode -quit -projectPath "$task_root/UnityProject" -executeMethod VectorRush.Editor.VectorRushSetup.Prepare -logFile "$task_root/evidence/unity-setup.log" ;;
  build) exec "$task_editor" -batchmode -quit -projectPath "$task_root/UnityProject" -executeMethod VectorRush.Editor.VectorRushSetup.BuildMac -logFile "$task_root/evidence/unity-build.log" ;;
  test) exec "$task_editor" -batchmode -projectPath "$task_root/UnityProject" -runTests -testPlatform EditMode -testResults "$task_root/evidence/editmode-results.xml" -logFile "$task_root/evidence/unity-tests.log" ;;
  *) echo 'Usage: tools/unity.sh open|prepare|build|test' >&2; exit 2 ;;
esac
