#!/bin/bash
# Portable entry point. Never regenerates scenes/assets as a side effect of building.
set -euo pipefail
task_root="$(cd "$(dirname "$0")/.." && pwd)"
task_editor="${VECTOR_UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity}"
task_scene="Assets/Scenes/FullLapAdsStage8.unity"
if [[ ! -x "$task_editor" ]]; then
  echo 'Install Unity 6000.6.0f1 and set VECTOR_UNITY_EDITOR to its executable.' >&2
  exit 2
fi
case "${1:-help}" in
  open)
    echo "Open $task_scene in the Editor; the historical default scene is not authoritative."
    exec "$task_editor" -projectPath "$task_root/UnityProject"
    ;;
  build|baseline-build|p4-build|stage7-build|koi-build)
    if [[ "${1}" == baseline-build ]]; then task_scene="Assets/Scenes/EnvironmentStructureStage1.unity"; fi
    if [[ "${1}" == p4-build ]]; then task_scene="Assets/Scenes/AnimatedBillboardsStage6.unity"; fi
    if [[ "${1}" == koi-build ]]; then task_scene="Assets/Scenes/KoiLanternStage9.unity"; fi
    if [[ "${1}" == stage7-build ]]; then task_scene="Assets/Scenes/NightCityStage7.unity"; fi
    if [[ $# != 3 || "$2" != /*.app || "$3" != /* ]]; then
      echo 'Usage: bash tools/current-game.sh build /absolute/fresh/Game.app /absolute/fresh/evidence' >&2; exit 2
    fi
    if [[ -e "$2" || -e "$3" ]]; then echo 'Build and evidence outputs must not already exist.' >&2; exit 2; fi
    mkdir -p "$3"
    # Log must be outside evidence: the build helper requires initially empty evidence.
    exec "$task_editor" -batchmode -quit -projectPath "$task_root/UnityProject" \
      -executeMethod VectorRush.Editor.ProductionSceneSetup.BuildExperienceCandidate \
      -experienceScene "$task_scene" -experienceBuildOutput "$2" -productionEvidence "$3" -logFile "${3}.log"
    ;;
  test)
    task_evidence="${2:-$task_root/artifacts/current-game/tests}"
    if [[ "$task_evidence" != /* || -e "$task_evidence" ]]; then echo 'Use a fresh absolute test directory.' >&2; exit 2; fi
    mkdir -p "$task_evidence"
    exec "$task_editor" -batchmode -projectPath "$task_root/UnityProject" -runTests -testPlatform EditMode \
      -testResults "$task_evidence/results.xml" -logFile "$task_evidence/editor.log"
    ;;
  *) echo 'Usage: bash tools/current-game.sh open | build <fresh absolute .app> <fresh absolute evidence> | koi-build <app> <evidence> | stage7-build <app> <evidence> | p4-build <app> <evidence> | baseline-build <app> <evidence> | test [fresh absolute directory]' ;;
esac
