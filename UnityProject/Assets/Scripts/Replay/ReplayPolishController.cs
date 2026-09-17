using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    /// <summary>
    /// Presentation-only replay endpoint. Gameplay feeds it authoritative lap time,
    /// eligibility and accepted ordered crossings; it never changes race state.
    /// </summary>
    public sealed class ReplayPolishController : MonoBehaviour
    {
        public static ReplayPolishController Instance { get; private set; }
        public bool GhostAvailable => loadedGhost != null;
        public bool GhostVisible => ghostRoot && ghostRoot.activeSelf;
        public bool CurrentLapEligible { get; private set; }
        public bool ManualRunEligible { get; private set; }
        public bool HasLatestSectorDelta { get; private set; }
        public float LatestSectorDelta { get; private set; }
        public int LatestSectorNumber { get; private set; }
        public string StatusMessage { get; private set; } = "Complete an eligible lap to create your first ghost.";
        public IReadOnlyList<float> ResultSectorDeltas => resultSectorDeltas;

        readonly GhostLapBuilder builder = new GhostLapBuilder();
        readonly float[] resultSectorDeltas = new float[3];
        readonly bool[] resultSectorHasReference = new bool[3];
        PersonalBestGhostStore store;
        GhostLapRecording loadedGhost;
        SectorComparisonTracker sectors;
        RaceDirector connectedDirector;
        Transform playerTransform;
        GameObject ghostRoot;
        Material ghostMaterial;
        string courseIdentity;
        string drivingRulesIdentity;
        float previousLapTime = -1f;
        float latestLapTime;
        float latestProgress;
        bool lapStarted;
        bool hasObservedPose;
        float observedSimulationTime;
        Vector3 observedPosition;
        Quaternion observedRotation;
        float observedProgress;

        public bool ResultSectorHasReference(int index) => index >= 0 && index < resultSectorHasReference.Length && resultSectorHasReference[index];

        public void Initialize(RaceDirector director, string persistenceDirectory = null)
        {
            if (!director || !director.Player) throw new ArgumentException("An initialized race director with a player is required", nameof(director));
            connectedDirector = director;
            Transform visual = director.Player.VisualRoot ? director.Player.VisualRoot : director.Player.transform;
            Initialize(director.CourseIdentity, RaceDirector.DrivingRulesId, visual, persistenceDirectory);
            director.RaceRestarted += HandleRaceRestarted;
            director.OrderedCrossed += HandleOrderedCrossing;
            RaceStarted(director.ManualRunEligible);
        }

        public void Initialize(string course, string rules, Transform playerVisual, string persistenceDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(course)) throw new ArgumentException("Course identity is required", nameof(course));
            if (string.IsNullOrWhiteSpace(rules)) throw new ArgumentException("Driving-rules identity is required", nameof(rules));
            courseIdentity = course;
            drivingRulesIdentity = rules;
            playerTransform = playerVisual;
            store = new PersonalBestGhostStore(persistenceDirectory);
            LoadCompatibleGhost();
            CreateVisual(playerVisual);
            SetGhostVisible(false);
            Instance = this;
        }

        public void RaceStarted(bool manualRunEligible)
        {
            ManualRunEligible = manualRunEligible;
            CurrentLapEligible = manualRunEligible;
            previousLapTime = -1f;
            latestLapTime = latestProgress = 0f;
            lapStarted = false;
            hasObservedPose = false;
            builder.Reset();
            sectors = new SectorComparisonTracker(loadedGhost == null ? null : loadedGhost.sectorTimes);
            ClearLapPresentation();
            SetGhostVisible(false);
        }

        public void ObserveRace(float lapLocalTime, float wrappedProgress, bool manualRunEligible, bool lapEligible, RacePhase phase)
            => ObserveRace(float.NaN, lapLocalTime, wrappedProgress, manualRunEligible, lapEligible, phase);

        public void ObserveRace(float simulationTime, float lapLocalTime, float wrappedProgress,
            bool manualRunEligible, bool lapEligible, RacePhase phase)
        {
            if (!manualRunEligible) ManualRunEligible = false;
            latestLapTime = lapLocalTime;
            latestProgress = wrappedProgress;
            bool hasCurrentPose = playerTransform;
            Vector3 currentPosition = hasCurrentPose ? playerTransform.position : Vector3.zero;
            Quaternion currentRotation = hasCurrentPose ? playerTransform.rotation : Quaternion.identity;
            if (phase == RacePhase.Paused)
            {
                UpdateGhost(lapLocalTime);
                CacheObservedPose(simulationTime, currentPosition, currentRotation, wrappedProgress, hasCurrentPose);
                return;
            }
            if (phase != RacePhase.Racing)
            {
                SetGhostVisible(false);
                CacheObservedPose(simulationTime, currentPosition, currentRotation, wrappedProgress, hasCurrentPose);
                return;
            }
            if (!Finite(lapLocalTime) || lapLocalTime < 0f) return;

            bool beganNewLap = !lapStarted || (previousLapTime >= 0f && lapLocalTime + .01f < previousLapTime);
            if (beganNewLap)
            {
                lapStarted = true;
                CurrentLapEligible = ManualRunEligible && lapEligible;
                builder.Reset();
                sectors = new SectorComparisonTracker(loadedGhost == null ? null : loadedGhost.sectorTimes);
                sectors.BeginLap(CurrentLapEligible);
                ClearLapPresentation();
                if (CurrentLapEligible && hasCurrentPose)
                {
                    GhostPose start = BoundaryPose(simulationTime - lapLocalTime, simulationTime,
                        currentPosition, currentRotation, wrappedProgress);
                    builder.TrySample(0f, start.Position, start.Rotation, start.Progress, true);
                }
            }
            if (!lapEligible || !ManualRunEligible)
            {
                CurrentLapEligible = false;
                sectors?.InvalidateLap();
                ClearLapPresentation();
            }

            if (CurrentLapEligible && hasCurrentPose)
                builder.TrySample(lapLocalTime, currentPosition, currentRotation, wrappedProgress);
            UpdateGhost(lapLocalTime);
            previousLapTime = lapLocalTime;
            CacheObservedPose(simulationTime, currentPosition, currentRotation, wrappedProgress, hasCurrentPose);
        }

        public void OrderedCrossing(int boundaryIndex, float lapLocalTime, bool lapEligible)
        {
            GhostPose boundary = new GhostPose(playerTransform ? playerTransform.position : Vector3.zero,
                playerTransform ? playerTransform.rotation : Quaternion.identity, latestProgress);
            OrderedCrossing(boundaryIndex, lapLocalTime, lapEligible, boundary.Position, boundary.Rotation, boundary.Progress);
        }

        public void OrderedCrossing(int boundaryIndex, float lapLocalTime, bool lapEligible,
            Vector3 boundaryPosition, Quaternion boundaryRotation, float boundaryProgress)
        {
            if (sectors == null) sectors = new SectorComparisonTracker(loadedGhost == null ? null : loadedGhost.sectorTimes);
            if (!sectors.TryRecord(boundaryIndex, lapLocalTime, ManualRunEligible && CurrentLapEligible && lapEligible, out float delta))
            {
                if (!lapEligible)
                {
                    CurrentLapEligible = false;
                    ClearLapPresentation();
                    StatusMessage = "Current lap invalid after recovery.";
                }
                return;
            }

            LatestSectorNumber = boundaryIndex + 1;
            HasLatestSectorDelta = loadedGhost != null;
            LatestSectorDelta = delta;
            resultSectorDeltas[boundaryIndex] = delta;
            resultSectorHasReference[boundaryIndex] = loadedGhost != null;

            if (boundaryIndex != 2) return;
            if (CurrentLapEligible && ManualRunEligible && playerTransform)
            {
                GhostLapRecording recording = builder.Finish(courseIdentity, drivingRulesIdentity, lapLocalTime,
                    sectors.CurrentSectorTimes, boundaryPosition, boundaryRotation, boundaryProgress);
                GhostSaveResult result = store.TrySaveIfFaster(recording);
                StatusMessage = result.Message;
                if (result.Saved) LoadCompatibleGhost();
            }
            else StatusMessage = "Lap excluded from personal-best ghost comparisons.";
        }

        public void OrderedCrossing(OrderedRaceCrossing crossing, float observationSimulationTime,
            Vector3 observationPosition, Quaternion observationRotation, float observationProgress)
        {
            GhostPose boundary = BoundaryPose(crossing.SimulationTime, observationSimulationTime,
                observationPosition, observationRotation, observationProgress);
            OrderedCrossing(crossing.BoundaryIndex, crossing.LapTime, crossing.LapEligible,
                boundary.Position, boundary.Rotation, boundary.Progress);
        }

        public void InvalidateCurrentLap()
        {
            CurrentLapEligible = false;
            sectors?.InvalidateLap();
            ClearLapPresentation();
            StatusMessage = "Current lap invalid after recovery.";
        }

        public void RaceRestarted() => RaceStarted(true);

        void HandleRaceRestarted() => RaceStarted(connectedDirector && connectedDirector.ManualRunEligible);

        void HandleOrderedCrossing(OrderedRaceCrossing crossing)
        {
            Transform player = connectedDirector && connectedDirector.Player
                ? (connectedDirector.Player.VisualRoot ? connectedDirector.Player.VisualRoot : connectedDirector.Player.transform)
                : playerTransform;
            Vector3 currentPosition = player ? player.position : Vector3.zero;
            Quaternion currentRotation = player ? player.rotation : Quaternion.identity;
            float currentProgress = connectedDirector ? connectedDirector.CurrentWrappedProgress : latestProgress;
            float currentSimulationTime = connectedDirector ? connectedDirector.SimulationTime : crossing.SimulationTime;
            OrderedCrossing(crossing, currentSimulationTime, currentPosition, currentRotation, currentProgress);
        }

        void LateUpdate()
        {
            if (!connectedDirector || !connectedDirector.Player || connectedDirector.Player.ProgressTracker == null) return;
            if (!connectedDirector.Player.ProgressTracker.HasStarted)
            {
                CacheObservedPose(connectedDirector.SimulationTime, playerTransform.position, playerTransform.rotation,
                    connectedDirector.CurrentWrappedProgress, true);
                SetGhostVisible(false);
                return;
            }
            ObserveRace(connectedDirector.SimulationTime, connectedDirector.CurrentLapTime, connectedDirector.CurrentWrappedProgress,
                connectedDirector.ManualRunEligible, connectedDirector.CurrentLapEligible, connectedDirector.Phase);
        }

        void ClearLapPresentation()
        {
            Array.Clear(resultSectorDeltas, 0, resultSectorDeltas.Length);
            Array.Clear(resultSectorHasReference, 0, resultSectorHasReference.Length);
            HasLatestSectorDelta = false;
            LatestSectorDelta = 0f;
            LatestSectorNumber = 0;
        }

        GhostPose BoundaryPose(float boundarySimulationTime, float currentSimulationTime,
            Vector3 currentPosition, Quaternion currentRotation, float currentProgress)
        {
            if (!hasObservedPose || !Finite(boundarySimulationTime) || !Finite(currentSimulationTime) ||
                currentSimulationTime <= observedSimulationTime + .000001f ||
                boundarySimulationTime < observedSimulationTime - .001f || boundarySimulationTime > currentSimulationTime + .001f)
                return new GhostPose(currentPosition, currentRotation, Mathf.Repeat(currentProgress, 1f));

            float blend = Mathf.InverseLerp(observedSimulationTime, currentSimulationTime, boundarySimulationTime);
            float progressDelta = Mathf.Repeat(currentProgress - observedProgress + .5f, 1f) - .5f;
            return new GhostPose(Vector3.Lerp(observedPosition, currentPosition, blend),
                Quaternion.Slerp(observedRotation, currentRotation, blend),
                Mathf.Repeat(observedProgress + progressDelta * blend, 1f));
        }

        void CacheObservedPose(float simulationTime, Vector3 position, Quaternion rotation, float progress, bool hasPose)
        {
            if (!hasPose || !Finite(simulationTime)) return;
            hasObservedPose = true;
            observedSimulationTime = simulationTime;
            observedPosition = position;
            observedRotation = rotation;
            observedProgress = Mathf.Repeat(progress, 1f);
        }

        void LoadCompatibleGhost()
        {
            GhostLoadResult result = store.Load(courseIdentity, drivingRulesIdentity);
            loadedGhost = result.Status == GhostLoadStatus.Loaded ? result.Recording : null;
            StatusMessage = result.Status == GhostLoadStatus.Loaded ? "Personal-best ghost ready." : result.Message;
        }

        void UpdateGhost(float lapLocalTime)
        {
            bool enabled = PlayerPreferences.Current.GhostEnabled;
            if (!enabled || loadedGhost == null || !ghostRoot || !GhostPlayback.TryEvaluate(loadedGhost, lapLocalTime, out GhostPose pose))
            {
                SetGhostVisible(false);
                return;
            }
            ghostRoot.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            bool obscuresLaunch = lapLocalTime < 1.25f && playerTransform && (playerTransform.position - pose.Position).sqrMagnitude < 64f;
            SetGhostVisible(!obscuresLaunch);
        }

        void CreateVisual(Transform source)
        {
            if (!source) return;
            ghostRoot = new GameObject("Personal best ghost / visual only");
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (!shader) shader = Shader.Find("Unlit/Color");
            if (!shader) return;
            ghostMaterial = new Material(shader) { name = "Personal best ghost (runtime)", renderQueue = 3000 };
            Color color = new Color(.22f, .95f, 1f, .28f);
            if (ghostMaterial.HasProperty("_BaseColor")) ghostMaterial.SetColor("_BaseColor", color);
            if (ghostMaterial.HasProperty("_Color")) ghostMaterial.SetColor("_Color", color);
            if (ghostMaterial.HasProperty("_Surface")) ghostMaterial.SetFloat("_Surface", 1f);
            if (ghostMaterial.HasProperty("_ZWrite")) ghostMaterial.SetFloat("_ZWrite", 0f);
            ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ghostMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            foreach (MeshFilter sourceFilter in source.GetComponentsInChildren<MeshFilter>(true))
            {
                MeshRenderer sourceRenderer = sourceFilter.GetComponent<MeshRenderer>();
                if (!sourceRenderer || !sourceFilter.sharedMesh) continue;
                var child = new GameObject(sourceFilter.name);
                child.transform.SetParent(ghostRoot.transform, false);
                child.transform.localPosition = source.InverseTransformPoint(sourceFilter.transform.position);
                child.transform.localRotation = Quaternion.Inverse(source.rotation) * sourceFilter.transform.rotation;
                child.transform.localScale = sourceFilter.transform.lossyScale;
                child.AddComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
                MeshRenderer renderer = child.AddComponent<MeshRenderer>();
                var materials = new Material[Mathf.Max(1, sourceRenderer.sharedMaterials.Length)];
                for (int i = 0; i < materials.Length; i++) materials[i] = ghostMaterial;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }
        }

        void SetGhostVisible(bool visible)
        {
            if (ghostRoot && ghostRoot.activeSelf != visible) ghostRoot.SetActive(visible);
        }

        void OnDestroy()
        {
            if (connectedDirector)
            {
                connectedDirector.RaceRestarted -= HandleRaceRestarted;
                connectedDirector.OrderedCrossed -= HandleOrderedCrossing;
            }
            if (Instance == this) Instance = null;
            if (ghostRoot) Destroy(ghostRoot);
            if (ghostMaterial) Destroy(ghostMaterial);
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
